//
// IOIterator.cs
//
// Author(s):
//       David Lechner <david@lechnology.com>
//
// Copyright (c) 2013 David Lechner
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using MonoMac;
using MonoMac.CoreFoundation;

using boolean_t = System.Boolean;
using io_iterator_t = System.IntPtr;
using io_object_t = System.IntPtr;

namespace MonoMac.IOKit
{
	public class IOIterator<T> : IOObject, IEnumerator<T> where T : IOObject
	{
		public IOIterator (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		#region IEumerator implementation

		public T Current { get; private set; }

		public bool MoveNext ()
		{
			var nextObject = IOIteratorStatic.IOIteratorNext (Handle);
			Current = (nextObject == IntPtr.Zero) ? null : IOObject.MarshalNativeObject<T> (nextObject, true);
			if (!IOIteratorStatic.IOIteratorIsValid (Handle))
				throw new InvalidOperationException ();
			return Current != null;
		}

		public void Reset ()
		{
			IOIteratorStatic.IOIteratorReset (Handle);
		}

		#endregion

		#region IEnumerator implementation

		object System.Collections.IEnumerator.Current {
			get {
				return Current;
			}
		}

		#endregion
	}

	static class IOIteratorStatic {
		// have to put DllImport in separate class becuase compiler does not
		// allow them in generic types.

		[DllImport (Constants.IOKitLibrary)]
		public extern static io_object_t IOIteratorNext (io_iterator_t iterator);

		[DllImport (Constants.IOKitLibrary)]
		public extern static boolean_t IOIteratorIsValid (io_iterator_t iterator);

		[DllImport (Constants.IOKitLibrary)]
		public extern static void IOIteratorReset (io_iterator_t iterator);
	}
}

