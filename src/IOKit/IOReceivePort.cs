//
// IOReceivePort.cs
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
using MonoMac.CoreFoundation;
using MonoMac.Kernel.Mach;
using MonoMac.ObjCRuntime;

using kern_return_t = MonoMac.IOKit.IOReturn;
using mach_port_t = System.IntPtr;
using uint32_t = System.UInt32;

namespace MonoMac.IOKit
{
	public class IOReceivePort : Port
	{
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOCreateReceivePort (uint32_t msgType, out mach_port_t recvPort);

		public IOReceivePort (OSMessageID msgType) : base (IntPtr.Zero)
		{
			IntPtr receivePort;
			var result = IOCreateReceivePort ((uint)msgType, out receivePort);
			IOObject.ThrowIfError (result);
			Handle = receivePort;
		}
	}
}
