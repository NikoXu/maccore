//
// Error.cs
//
// Author:
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

namespace MonoMac.Kernel.Mach
{
	public static class Error
	{
		public static ErrorSystem GetSystem (int value)
		{
			return (ErrorSystem)(((value) >> 26) & 0x3f);
		}

		public static int GetSubSystem (int value)
		{
			return (((value) >> 14) & 0xfff);
		}

		public static int GetCode (int value)
		{
			return ((value) & 0x3fff);
		}
	}

	public enum ErrorSystem
	{
		/*	major error systems	*/
		Kernel = 0x0,
		UserSpaceLibrary = 0x1,
		UserSpaceServers = 0x2,
		OldIPC = 0x3,
		MachIPC = 0x4,
		DistributedIPC = 0x7,
		IOKit = 0x38,
		UserDefined = 0x3e,
		MachIPCCompatibility = 0x3f,
	}
}

