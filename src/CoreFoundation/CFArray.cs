// 
// CFArray.cs: P/Invokes for CFArray
//
// Authors:
//    Mono Team
//    Rolf Bjarne Kvinge (rolf@xamarin.com)
//
//     
// Copyright 2010 Novell, Inc
// Copyright 2012-2014 Xamarin Inc. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

using CFIndex = System.nint;
using CFArrayRef = System.IntPtr;
using CFAllocatorRef = System.IntPtr;

#nullable enable

namespace CoreFoundation {
	
	public partial class CFArray : NativeObject {

		internal CFArray (IntPtr handle)
			: base (handle, false)
		{
		}

		[Preserve (Conditional = true)]
		internal CFArray (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
		
		[DllImport (Constants.CoreFoundationLibrary, EntryPoint="CFArrayGetTypeID")]
		internal extern static /* CFTypeID */ nint GetTypeID ();

		// pointer to a const struct (REALLY APPLE?)
		static IntPtr kCFTypeArrayCallbacks_ptr_value;
		static IntPtr kCFTypeArrayCallbacks_ptr {
			get {
				// FIXME: right now we can't use [Fields] for GetIndirect
				if (kCFTypeArrayCallbacks_ptr_value == IntPtr.Zero)
					kCFTypeArrayCallbacks_ptr_value = Dlfcn.GetIndirect (Libraries.CoreFoundation.Handle, "kCFTypeArrayCallBacks");
				return kCFTypeArrayCallbacks_ptr_value;
			}
		}

		internal static CFArray FromIntPtrs (params IntPtr[] values)
		{
			return new CFArray (Create (values), true);
		}

		internal static CFArray FromNativeObjects (params INativeObject[] values)
		{
			return new CFArray (Create (values), true);
		}

		public nint Count {
			get { return CFArrayGetCount (GetCheckedHandle ()); }
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static /* CFArrayRef */ IntPtr CFArrayCreate (/* CFAllocatorRef */ IntPtr allocator, /* void** */ IntPtr values, nint numValues, /* CFArrayCallBacks* */ IntPtr callBacks);

		[DllImport (Constants.CoreFoundationLibrary)]
		internal extern static /* void* */ IntPtr CFArrayGetValueAtIndex (/* CFArrayRef */ IntPtr theArray, /* CFIndex */ nint idx);

		public IntPtr GetValue (nint index)
		{
			return CFArrayGetValueAtIndex (GetCheckedHandle (), index);
		}

		internal static unsafe IntPtr Create (params IntPtr[] values)
		{
			if (values is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (values));
			fixed (IntPtr* pv = values) {
				return CFArrayCreate (IntPtr.Zero, 
						(IntPtr) pv,
						values.Length,
						kCFTypeArrayCallbacks_ptr);
			}
		}

		internal static IntPtr Create (params INativeObject[] values)
		{
			if (values is null)
				ObjCRuntime.ThrowHelper.ThrowArgumentNullException (nameof (values));
			IntPtr[] _values = new IntPtr [values.Length];
			for (int i = 0; i < _values.Length; ++i)
				_values [i] = values [i].Handle;
			return Create (_values);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static /* CFIndex */ nint CFArrayGetCount (/* CFArrayRef */ IntPtr theArray);

		internal static nint GetCount (IntPtr array)
		{
			return CFArrayGetCount (array);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFArrayRef CFArrayCreateCopy (CFAllocatorRef allocator, CFArrayRef theArray);

		internal CFArray Clone () => new CFArray (CFArrayCreateCopy (IntPtr.Zero, GetCheckedHandle ()), true);

		// identical signature to NSArray API
		static public string?[]? StringArrayFromHandle (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;

			var c = CFArrayGetCount (handle);
			string?[] ret = new string [c];

			for (nint i = 0; i < c; i++)
				ret [i] = CFString.FromHandle (CFArrayGetValueAtIndex (handle, i));
			return ret;
		}
	}
}
