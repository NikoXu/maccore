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
			var nextObject = IOIteratorNext (Handle);
			Current = (nextObject == IntPtr.Zero) ? null : IOObject.MarshalNativeObject<T> (nextObject, true);
			if (!IOIteratorIsValid (Handle))
				throw new InvalidOperationException ();
			return Current != null;
		}

		public void Reset ()
		{
			IOIteratorReset (Handle);
		}

		#endregion

		#region IEnumerator implementation

		object System.Collections.IEnumerator.Current {
			get {
				return Current;
			}
		}

		#endregion

		/// <summary>
		/// Returns the next object in an iteration.
		/// </summary>
		/// <returns>If the iterator handle is valid, the next element in the iteration is returned, otherwise zero is returned.
		///  The element should be released by the caller when it is finished.</returns>
		/// <param name="iterator">An IOKit iterator handle.</param>
		/// <remarks>This function returns the next object in an iteration, or zero if no more remain or the iterator is invalid.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static io_object_t IOIteratorNext (io_iterator_t iterator);

		/// <summary>
		/// Resets an iteration back to the beginning.
		/// </summary>
		/// <param name="iterator">An IOKit iterator handle.</param>
		/// <remarks>If an iterator is invalid, or if the caller wants to start over, IOIteratorReset will set the iteration back to the beginning.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOIteratorReset (io_iterator_t iterator);

		/// <summary>
		/// Checks an iterator is still valid.
		/// </summary>
		/// <returns>True if the iterator handle is valid, otherwise false is returned./returns>
		/// <param name="iterator">An IOKit iterator handle.</param>
		/// <remarks>Some iterators will be made invalid if changes are made to the structure they are iterating over.
		/// This function checks the iterator is still valid and should be called when IOIteratorNext returns zero.
		/// An invalid iterator can be reset and the iteration restarted.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static boolean_t IOIteratorIsValid (io_iterator_t iterator);
	}
}

