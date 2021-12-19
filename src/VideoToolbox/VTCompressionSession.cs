// 
// VTCompressionSession.cs: VideoTools Compression Session class 
//
// Authors: 
//    Miguel de Icaza (miguel@xamarin.com)
//    Alex Soto (alex.soto@xamarin.com
// 
// Copyright 2014 Xamarin Inc.
//
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using CoreFoundation;
using ObjCRuntime;
using Foundation;
using CoreMedia;
using CoreVideo;

#if !NET
using NativeHandle = System.IntPtr;
#endif

#nullable enable

namespace VideoToolbox {

#if NET
	[SupportedOSPlatform ("tvos10.2")]
#else
	[iOS (8,0), TV (10,2)]
#endif
	public class VTCompressionSession : VTSession {
		GCHandle callbackHandle;

#if !NET
		/* invoked by marshallers */
		protected internal VTCompressionSession (NativeHandle handle) : base (handle)
		{
		}
#endif

		[Preserve (Conditional=true)]
		internal VTCompressionSession (NativeHandle handle, bool owns) : base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero)
				VTCompressionSessionInvalidate (Handle);

			if (callbackHandle.IsAllocated)
				callbackHandle.Free();

			base.Dispose (disposing);
		}

		// sourceFrame: It seems it's only used as a parameter to be passed into EncodeFrame so no need to strong type it
		public delegate void VTCompressionOutputCallback (/* void* */ IntPtr sourceFrame, /* OSStatus */ VTStatus status, VTEncodeInfoFlags flags, CMSampleBuffer? buffer);
#if !NET
		delegate void CompressionOutputCallback (/* void* CM_NULLABLE */ IntPtr outputCallbackClosure, /* void* CM_NULLABLE */ IntPtr sourceFrame, /* OSStatus */ VTStatus status, VTEncodeInfoFlags infoFlags, /* CMSampleBufferRef CM_NULLABLE */ IntPtr cmSampleBufferPtr);

		//
		// Here for legacy code, which would only work under duress (user had to manually ref the CMSampleBuffer on the callback)
		//
		static CompressionOutputCallback? _static_CompressionOutputCallback;
		static CompressionOutputCallback static_CompressionOutputCallback {
			get {
				if (_static_CompressionOutputCallback is null)
					_static_CompressionOutputCallback = new CompressionOutputCallback (CompressionCallback);
				return _static_CompressionOutputCallback;
			}
		}
#endif

		static void CompressionCallback (IntPtr outputCallbackClosure, IntPtr sourceFrame, VTStatus status, VTEncodeInfoFlags infoFlags, IntPtr cmSampleBufferPtr, bool owns)
		{
			var gch = GCHandle.FromIntPtr (outputCallbackClosure);
			var func = (VTCompressionOutputCallback) gch.Target!;
			if (cmSampleBufferPtr == IntPtr.Zero) {
				func (sourceFrame, status, infoFlags, null);
			} else {
				using (var sampleBuffer = new CMSampleBuffer (cmSampleBufferPtr, owns: owns))
					func (sourceFrame, status, infoFlags, sampleBuffer);
			}
		}

#if NET
		[UnmanagedCallersOnly]
#else
#if !MONOMAC
		[MonoPInvokeCallback (typeof (CompressionOutputCallback))]
#endif
#endif
		static void CompressionCallback (IntPtr outputCallbackClosure, IntPtr sourceFrame, VTStatus status, VTEncodeInfoFlags infoFlags, IntPtr cmSampleBufferPtr)
		{
			CompressionCallback (outputCallbackClosure, sourceFrame, status, infoFlags, cmSampleBufferPtr, true);
		}

		[Obsolete ("This overload requires that the provided compressionOutputCallback manually CFRetain the passed CMSampleBuffer, use Create(int,int,CMVideoCodecType,VTCompressionOutputCallback,VTVideoEncoderSpecification,CVPixelBufferAttributes) variant instead which does not have that requirement.")]
		public static VTCompressionSession? Create (int width, int height, CMVideoCodecType codecType,
			VTCompressionOutputCallback compressionOutputCallback,
			VTVideoEncoderSpecification? encoderSpecification = null, // hardware acceleration is default behavior on iOS. no opt-in required.
			NSDictionary? sourceImageBufferAttributes = null)
		{
#if NET
			unsafe {
				return Create (width, height, codecType, compressionOutputCallback, encoderSpecification, sourceImageBufferAttributes, &NewCompressionCallback);
			}
#else
			return Create (width, height, codecType, compressionOutputCallback, encoderSpecification, sourceImageBufferAttributes, static_newCompressionOutputCallback);
#endif
		}

#if !NET
		// End region of legacy code

		static CompressionOutputCallback? _static_newCompressionOutputCallback;
		static CompressionOutputCallback static_newCompressionOutputCallback {
			get {
				if (_static_newCompressionOutputCallback is null)
					_static_newCompressionOutputCallback = new CompressionOutputCallback (NewCompressionCallback);
				return _static_newCompressionOutputCallback;
			}
		}
#endif

#if NET
		[UnmanagedCallersOnly]
#else
#if !MONOMAC
		[MonoPInvokeCallback (typeof (CompressionOutputCallback))]
#endif
#endif
		static void NewCompressionCallback (IntPtr outputCallbackClosure, IntPtr sourceFrame, VTStatus status, VTEncodeInfoFlags infoFlags, IntPtr cmSampleBufferPtr)
		{
			CompressionCallback (outputCallbackClosure, sourceFrame, status, infoFlags, cmSampleBufferPtr, false);
		}

		[DllImport (Constants.VideoToolboxLibrary)]
		unsafe extern static VTStatus VTCompressionSessionCreate (
			/* CFAllocatorRef */ IntPtr allocator, /* can be null */
			/* int32_t */ int width,
			/* int32_t */ int height,
			/* CMVideoCodecType */ CMVideoCodecType codecType,
			/* CFDictionaryRef */ IntPtr dictionaryEncoderSpecification, /* can be null */
			/* CFDictionaryRef */ IntPtr dictionarySourceImageBufferAttributes, /* can be null */
			/* CFDictionaryRef */ IntPtr compressedDataAllocator, /* can be null */
#if NET
			/* VTCompressionOutputCallback */ delegate* unmanaged</* void* CM_NULLABLE */ IntPtr, /* void* CM_NULLABLE */ IntPtr, /* OSStatus */ VTStatus, VTEncodeInfoFlags, /* CMSampleBufferRef CM_NULLABLE */ IntPtr, void> outputCallback,
#else
			/* VTCompressionOutputCallback */ CompressionOutputCallback? outputCallback,
#endif
			/* void* */ IntPtr outputCallbackClosure,
			/* VTCompressionSessionRef* */ out IntPtr compressionSessionOut);

#if false // Disabling for now until we have some tests on this
		[Mac (10,11), iOS (9,0)]
		public static VTCompressionSession? Create (int width, int height, CMVideoCodecType codecType,
			VTVideoEncoderSpecification? encoderSpecification = null,
			NSDictionary? sourceImageBufferAttributes = null)
		{
			return Create (width, height, codecType, null,
				encoderSpecification, sourceImageBufferAttributes);
		}
#endif
		public static VTCompressionSession? Create (int width, int height, CMVideoCodecType codecType,
			VTCompressionOutputCallback compressionOutputCallback,
			VTVideoEncoderSpecification? encoderSpecification, // hardware acceleration is default behavior on iOS. no opt-in required.
			CVPixelBufferAttributes? sourceImageBufferAttributes)
		{
#if NET
			unsafe {
				return Create (width, height, codecType, compressionOutputCallback, encoderSpecification, sourceImageBufferAttributes == null ? null : sourceImageBufferAttributes.Dictionary, &CompressionCallback);
			}
#else
			return Create (width, height, codecType, compressionOutputCallback, encoderSpecification, sourceImageBufferAttributes == null ? null : sourceImageBufferAttributes.Dictionary, static_CompressionOutputCallback);
#endif
		}

		unsafe static VTCompressionSession? Create (int width, int height, CMVideoCodecType codecType,
			VTCompressionOutputCallback compressionOutputCallback,
			VTVideoEncoderSpecification? encoderSpecification, // hardware acceleration is default behavior on iOS. no opt-in required.
		        NSDictionary? sourceImageBufferAttributes, // Undocumented options, probably always null
#if NET
		        delegate* unmanaged</* void* CM_NULLABLE */ IntPtr, /* void* CM_NULLABLE */ IntPtr, /* OSStatus */ VTStatus, VTEncodeInfoFlags, /* CMSampleBufferRef CM_NULLABLE */ IntPtr, void> staticCback)
#else
		        CompressionOutputCallback staticCback)
#endif
		{
			var callbackHandle = default (GCHandle);
			if (compressionOutputCallback is not null)
				callbackHandle = GCHandle.Alloc (compressionOutputCallback);

			var result = VTCompressionSessionCreate (IntPtr.Zero, width, height, codecType,
				encoderSpecification.GetHandle (),
				sourceImageBufferAttributes.GetHandle (),
				IntPtr.Zero,
				callbackHandle.IsAllocated ? (staticCback) : null,
				GCHandle.ToIntPtr (callbackHandle),
	            out var ret);

			if (result == VTStatus.Ok && ret != IntPtr.Zero)
				return new VTCompressionSession (ret, true) {
					callbackHandle = callbackHandle
				};

			if (callbackHandle.IsAllocated)
				callbackHandle.Free ();

			return null;
		}

		[DllImport (Constants.VideoToolboxLibrary)]
		extern static void VTCompressionSessionInvalidate (IntPtr handle);

		[DllImport (Constants.VideoToolboxLibrary)]
		extern static IntPtr /* cvpixelbufferpoolref */ VTCompressionSessionGetPixelBufferPool (IntPtr handle);

		public CVPixelBufferPool? GetPixelBufferPool ()
		{
			var ret = VTCompressionSessionGetPixelBufferPool (GetCheckedHandle ());

			if (ret != IntPtr.Zero) 
				return new CVPixelBufferPool (ret, false);

			return null;
		}

#if !NET
		[Mac (10,9)]
#endif
		[DllImport (Constants.VideoToolboxLibrary)]
		extern static VTStatus VTCompressionSessionPrepareToEncodeFrames (IntPtr handle);

#if !NET
		[Mac (10,9)]
#endif
		public VTStatus PrepareToEncodeFrames ()
		{
			return VTCompressionSessionPrepareToEncodeFrames (GetCheckedHandle ());
		}
		
		[DllImport (Constants.VideoToolboxLibrary)]
		extern static VTStatus VTCompressionSessionEncodeFrame (
			/* VTCompressionSessionRef */ IntPtr session, 
			/* CVImageBufferRef */ IntPtr imageBuffer, 
			/* CMTime */ CMTime presentation, 
			/* CMTime */ CMTime duration, // can ve CMTime.Invalid
			/* CFDictionaryRef */ IntPtr dict, // can be null, undocumented options
			/* void* */ IntPtr sourceFrame, 
			/* VTEncodeInfoFlags */ out VTEncodeInfoFlags flags);

		public VTStatus EncodeFrame (CVImageBuffer imageBuffer, CMTime presentationTimestampe, CMTime duration, 
			NSDictionary frameProperties, IntPtr sourceFrame, out VTEncodeInfoFlags infoFlags)
		{
			if (imageBuffer is null)
				throw new ArgumentNullException (nameof (imageBuffer));
			
			return VTCompressionSessionEncodeFrame (GetCheckedHandle (), imageBuffer.Handle, presentationTimestampe, duration,
				frameProperties.GetHandle (),
				sourceFrame, out infoFlags);
		}		

#if false // Disabling for now until we have some tests on this
		[Mac (10,11), iOS (9,0)]
		[DllImport (Constants.VideoToolboxLibrary)]
		extern static VTStatus VTCompressionSessionEncodeFrameWithOutputHandler (
			/* VTCompressionSessionRef */ IntPtr session,
			/* CVImageBufferRef */ IntPtr imageBuffer,
			/* CMTime */ CMTime presentation,
			/* CMTime */ CMTime duration, // can ve CMTime.Invalid
			/* CFDictionaryRef */ IntPtr dict, // can be null, undocumented options
			/* VTEncodeInfoFlags */ out VTEncodeInfoFlags flags,
			/* VTCompressionOutputHandler */ ref BlockLiteral outputHandler);

		public delegate void VTCompressionOutputHandler (VTStatus status, VTEncodeInfoFlags infoFlags, CMSampleBuffer sampleBuffer);

		unsafe delegate void VTCompressionOutputHandlerProxy (BlockLiteral *block,
			VTStatus status, VTEncodeInfoFlags infoFlags, IntPtr sampleBuffer);

		static unsafe readonly VTCompressionOutputHandlerProxy compressionOutputHandlerTrampoline = VTCompressionOutputHandlerTrampoline;

		[MonoPInvokeCallback (typeof (VTCompressionOutputHandlerProxy))]
		static unsafe void VTCompressionOutputHandlerTrampoline (BlockLiteral *block,
			VTStatus status, VTEncodeInfoFlags infoFlags, IntPtr sampleBuffer)
		{
			var del = (VTCompressionOutputHandler)(block->Target);
			if (del is not null)
				del (status, infoFlags, new CMSampleBuffer (sampleBuffer));
		}

		[Mac (10,11), iOS (9,0)]
		public VTStatus EncodeFrame (CVImageBuffer imageBuffer, CMTime presentationTimestamp, CMTime duration,
			NSDictionary frameProperties, IntPtr sourceFrame, out VTEncodeInfoFlags infoFlags,
			VTCompressionOutputHandler outputHandler)
		{
			if (imageBuffer is null)
				throw new ArgumentNullException ((imageBuffer));
			if (outputHandler is null)
				throw new ArgumentNullException (nameof (outputHandler));

			var block = new BlockLiteral ();
			block.SetupBlockUnsafe (compressionOutputHandlerTrampoline, outputHandler);

			try {
				return VTCompressionSessionEncodeFrameWithOutputHandler (GetCheckedHandle (),
					imageBuffer.Handle, presentationTimestamp, duration,
					frameProperties.GetHandle (),
					out infoFlags, ref block);
			} finally {
				block.CleanupBlock ();
			}
		}
#endif
		[DllImport (Constants.VideoToolboxLibrary)]
		extern static VTStatus VTCompressionSessionCompleteFrames (IntPtr session, CMTime completeUntilPresentationTimeStamp);

		public VTStatus CompleteFrames (CMTime completeUntilPresentationTimeStamp)
		{
			return VTCompressionSessionCompleteFrames (GetCheckedHandle (), completeUntilPresentationTimeStamp);
		}

#if !NET
		[Mac (10,10)]
#endif
		[DllImport (Constants.VideoToolboxLibrary)]
		extern static VTStatus VTCompressionSessionBeginPass (IntPtr session, VTCompressionSessionOptionFlags flags, IntPtr reserved);

#if !NET
		[Mac (10,10)]
#endif
		public VTStatus BeginPass (VTCompressionSessionOptionFlags flags)
		{
			return VTCompressionSessionBeginPass (GetCheckedHandle (), flags, IntPtr.Zero);
		}

#if !NET
		[Mac (10,10)]
#endif
		[DllImport (Constants.VideoToolboxLibrary)]
		extern static VTStatus VTCompressionSessionEndPass (IntPtr session, out byte furtherPassesRequestedOut, IntPtr reserved);

#if !NET
		[Mac (10,10)]
#endif
		[DllImport (Constants.VideoToolboxLibrary)]
		extern static VTStatus VTCompressionSessionEndPass (IntPtr session, IntPtr ptrByte, IntPtr reserved);

#if !NET
		[Mac (10,10)]
#endif
		public VTStatus EndPass (out bool furtherPassesRequested)
		{
			var result = VTCompressionSessionEndPass (GetCheckedHandle (), out var b, IntPtr.Zero);
			furtherPassesRequested = b != 0;
			return result;
		}

		// Like EndPass, but this will be the final pass, so the encoder will skip the evaluation.
		public VTStatus EndPassAsFinal ()
		{
			return VTCompressionSessionEndPass (GetCheckedHandle (), IntPtr.Zero, IntPtr.Zero);
		}

#if !NET
		[Mac (10,10)]
#endif
		[DllImport (Constants.VideoToolboxLibrary)]
		extern static VTStatus VTCompressionSessionGetTimeRangesForNextPass (
			/* VTCompressionSessionRef */ IntPtr session, 
			/* CMItemCount* */ out int itemCount, 
			/* const CMTimeRange** */ out IntPtr target);

#if !NET
		[Mac (10,10)]
#endif
		public VTStatus GetTimeRangesForNextPass (out CMTimeRange []? timeRanges)
		{
			var v = VTCompressionSessionGetTimeRangesForNextPass (GetCheckedHandle (), out var count, out var target);
			if (v != VTStatus.Ok) {
				timeRanges = null;
				return v;
			}
			timeRanges = new CMTimeRange [count];
			unsafe {
				CMTimeRange *ptr = (CMTimeRange *)target;
				for (int i = 0; i < count; i++)
					timeRanges [i] = ptr [i];
			}
			return VTStatus.Ok;
		}

		public VTStatus SetCompressionProperties (VTCompressionProperties options)
		{
			if (options is null)
				throw new ArgumentNullException (nameof (options));

			return VTSessionSetProperties (GetCheckedHandle (), options.Dictionary.Handle);
		}
	}	
}
