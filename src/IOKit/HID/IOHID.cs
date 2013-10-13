//
// IOHIDBase.cs
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
using System.Runtime.InteropServices;
using MonoMac.CoreFoundation;

using CFDictionaryRef = System.IntPtr;
using CFIndex = System.IntPtr;
using IOHIDDeviceRef = System.IntPtr;
using IOHIDValueRef = System.IntPtr;
using uint32_t = System.UInt32;
using uint8_t = System.Byte;

namespace MonoMac.IOKit.HID
{
	public static class IOHID
	{
		public const string DeviceKey                   = "IOHIDDevice";
		public const string ElementKey                  = "Elements";
	}

	delegate void IOHIDCallback (
                                    IntPtr                  context, 
                                    IOReturn                result, 
                                    IntPtr                  sender);

	delegate void IOHIDReportCallback (
                                    IntPtr                  context, 
                                    IOReturn                result, 
                                    IntPtr                  sender, 
                                    IOHIDReportType         type, 
                                    uint32_t                reportID,
	                                [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 6)]
                                    uint8_t[]               report, 
                                    CFIndex                 reportLength);

	delegate void IOHIDValueCallback ( 
                                    IntPtr                  context,
                                    IOReturn                result, 
                                    IntPtr                  sender,
                                    IOHIDValueRef           value);

	delegate void IOHIDValueMultipleCallback ( 
                                    IntPtr                  context,
                                    IOReturn                result, 
                                    IntPtr                  sender,
                                    CFDictionaryRef         multiple);

	delegate void IOHIDDeviceCallback ( 
                                    IntPtr                  context,
                                    IOReturn                result, 
                                    IntPtr                  sender,
                                    IOHIDDeviceRef          device);
}

