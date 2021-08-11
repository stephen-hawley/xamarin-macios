#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2009 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace OpenTK.Compute.CL10
{
    /// <summary>
    /// Provides access to the OpenCL 1.0 flat API.
    /// </summary>
    public partial class CL
    {
        #region Private Members

        const string Library = "opencl.dll";

        #endregion

        #region Constructors

        static CL()
        {
            Type imports = typeof(CL).GetNestedType("Core", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo[] delegates = typeof(CL).GetNestedType("Delegates", BindingFlags.Static | BindingFlags.NonPublic).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
            if (delegates == null || imports == null)
                throw new Exception("[Internal Error] Failed to locate CL.Delegates. Please file a bug report at http://www.opentk.com/issues");

            for (int i = 0; i < delegates.Length; i++)
            {
                MethodInfo method = imports.GetMethod(delegates[i].Name.Substring(2), BindingFlags.Static | BindingFlags.NonPublic);
                Delegate @new = Delegate.CreateDelegate(delegates[i].FieldType, method);
                delegates[i].SetValue(null, @new);
            }
        }

        #endregion
    }
}
