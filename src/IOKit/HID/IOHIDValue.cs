//
// IOHIDDevice.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.Kernel.Mach;
using MonoMac.ObjCRuntime;

using CFAllocatorRef = System.IntPtr;
using CFArrayRef = System.IntPtr;
using CFDictionaryRef = System.IntPtr;
using CFIndex = System.IntPtr;
using CFRunLoopRef = System.IntPtr;
using CFStringRef = System.IntPtr;
using CFTimeInterval = System.Double;
using CFTypeID = System.UInt32;
using CFTypeRef = System.IntPtr;
using IOHIDDeviceRef = System.IntPtr;
using IOHIDElementRef = System.IntPtr;
using IOHIDValueRef = System.IntPtr;
using IOOptionBits = System.UInt32;
using double_t = System.Double;
using io_service_t = System.IntPtr;
using uint32_t = System.UInt32;
using uint64_t = System.UInt64;
using uint8_t = System.Byte;

namespace MonoMac.IOKit.HID
{
	[Since (5,0)]
	public class IOHIDValue : CFType
	{
		internal IOHIDValue (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeID IOHIDValueGetTypeID ();

		public static uint TypeID {
			get { return IOHIDValueGetTypeID (); }
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDValueRef IOHIDValueCreateWithIntegerValue (CFAllocatorRef allocator,
		                                                              IOHIDElementRef element,
		                                                              uint64_t timeStamp,
		                                                              CFIndex value);

		public static IOHIDValue FromInteger (IOHIDElement element, int value)
		{
			var timeStamp = Time.Absolute;
			var valueRef = IOHIDValueCreateWithIntegerValue (IntPtr.Zero,
			                                                 element.Handle,
			                                                 timeStamp,
			                                                 (CFIndex)value);
			return new IOHIDValue (valueRef, true);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDValueRef IOHIDValueCreateWithBytes (CFAllocatorRef allocator,
		                                                       IOHIDElementRef element,
		                                                       uint64_t timeStamp,
		                                                       [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)]
		                                                       [In]uint8_t[] bytes,
		                                                       CFIndex length);

		public static IOHIDValue FromBytes (IOHIDElement element, byte[] bytes)
		{
			var timeStamp = Time.Absolute;
			var valueRef = IOHIDValueCreateWithBytes (IntPtr.Zero,
			                                          element.Handle,
			                                          timeStamp, bytes,
			                                          (CFIndex)bytes.Length);
			return new IOHIDValue (valueRef, true);
		}

//		[DllImport (Constants.IOKitLibrary)]
//		extern static IOHIDValueRef IOHIDValueCreateWithBytesNoCopy (CFAllocatorRef allocator,
//		                                                             IOHIDElementRef element,
//		                                                             uint64_t timeStamp,
//		                                                             [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)]
//		                                                             [In]uint8_t[] bytes,
//		                                                             CFIndex length);

		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementRef IOHIDValueGetElement (IOHIDValueRef value);

		public IOHIDElement Element {
			get {
				ThrowIfDisposed ();
				var elementRef = IOHIDValueGetElement (Handle);
				if (elementRef == IntPtr.Zero)
					return null;
				return new IOHIDElement (elementRef, false);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static uint64_t IOHIDValueGetTimeStamp (IOHIDValueRef value);

		public ulong TimeStamp {
			get {
				ThrowIfDisposed ();
				return IOHIDValueGetTimeStamp (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDValueGetLength (IOHIDValueRef value);

		public int Length {
			get {
				ThrowIfDisposed ();
				return (int)IOHIDValueGetLength (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static /*const uint8_t*/ IntPtr IOHIDValueGetBytePtr (IOHIDValueRef value);

		public byte[] Bytes {
			get {
				ThrowIfDisposed ();
				var bytePtr = IOHIDValueGetBytePtr (Handle);
				var bytes = new byte[Length];
				for (int offset = 0; offset < Length; offset++)
					bytes[offset] = Marshal.ReadByte (bytePtr, offset);
				return bytes;
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDValueGetIntegerValue (IOHIDValueRef value);

		public int IntegerValue {
			get {
				ThrowIfDisposed ();
				return (int)IOHIDValueGetIntegerValue (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static double_t IOHIDValueGetScaledValue (IOHIDValueRef value,
		                                                 IOHIDValueScaleType type);

		public double GetScaledValue (IOHIDValueScaleType type)
		{
			ThrowIfDisposed ();
			return IOHIDValueGetScaledValue (Handle, type);
		}
	}
}

