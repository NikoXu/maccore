//
// IORegistryIterator.cs
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
using System.Runtime.InteropServices;
using MonoMac;

using io_iterator_t = System.IntPtr;
using kern_return_t = MonoMac.IOKit.IOReturn;

namespace MonoMac.IOKit
{
	public class IORegistryIterator<T> : IOIterator<T> where T : IOObject
	{
		internal IORegistryIterator (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		/// <summary>Recurse into the current entry in the registry iteration.</summary>
		/// <remarks>This method makes the current entry, ie. the last entry returned by IOIteratorNext,
		///  the root in a new level of recursion.</remarks>
		/// <returns>A kern_return_t error code.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryIteratorEnterEntry(io_iterator_t iterator);

		/// <summary>Exits a level of recursion, restoring the current entry.</summary>
		/// <remarks>This method undoes an IORegistryIteratorEnterEntry, restoring the current entry.
 		/// If there are no more levels of recursion to exit false is returned, otherwise true is returned.</remarks>
		/// <returns>kIOReturnSuccess if a level of recursion was undone, kIOReturnNoDevice
		/// if no recursive levels are left in the iteration.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryIteratorExitEntry(io_iterator_t iterator);

	}
}

