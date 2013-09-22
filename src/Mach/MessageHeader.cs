//
// MessageHeader.cs
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

using mach_msg_bits_t = System.UInt32;
using mach_msg_id_t = System.IntPtr;
using mach_msg_size_t = System.IntPtr;
using mach_port_t = System.IntPtr;

namespace MonoMac.Mach
{
	[StructLayout (LayoutKind.Sequential)]
	public struct MessageHeader
	{
		public mach_msg_bits_t	msgh_bits;
		public mach_msg_size_t	msgh_size;
		public mach_port_t		msgh_remote_port;
		public mach_port_t		msgh_local_port;
		public mach_msg_size_t 	msgh_reserved;
		public mach_msg_id_t	msgh_id;
	}
}

