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

		public static uint TypeID {
			get { return IOHIDValueGetTypeID (); }
		}

		public static IOHIDValue FromInteger (IOHIDElement element, int value)
		{
			var timeStamp = Time.Absolute;
			var valueRef = IOHIDValueCreateWithIntegerValue (IntPtr.Zero, element.Handle, timeStamp, (CFIndex)value);
			return new IOHIDValue (valueRef, true);
		}

		public static IOHIDValue FromBytes (IOHIDElement element, byte[] bytes)
		{
			var timeStamp = Time.Absolute;
			var valueRef = IOHIDValueCreateWithBytes (IntPtr.Zero, element.Handle, timeStamp, bytes, (CFIndex)bytes.Length);
			return new IOHIDValue (valueRef, true);
		}

		public IOHIDElement Element {
			get {
				ThrowIfDisposed ();
				var elementRef = IOHIDValueGetElement (Handle);
				if (elementRef == IntPtr.Zero)
					return null;
				return GetCFObject<IOHIDElement> (elementRef);
			}
		}

		public ulong TimeStamp {
			get {
				ThrowIfDisposed ();
				return IOHIDValueGetTimeStamp (Handle);
			}
		}

		public int Length {
			get {
				ThrowIfDisposed ();
				return (int)IOHIDValueGetLength (Handle);
			}
		}

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

		public int IntegerValue {
			get {
				ThrowIfDisposed ();
				return (int)IOHIDValueGetIntegerValue (Handle);
			}
		}

		public double GetScaledValue (IOHIDValueScaleType type)
		{
			ThrowIfDisposed ();
			return IOHIDValueGetScaledValue (Handle, type);
		}

		/*!
			@function   IOHIDValueGetTypeID
			@abstract   Returns the type identifier of all IOHIDValue instances.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeID IOHIDValueGetTypeID ();
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		/*!
			@function   IOHIDValueCreateWithIntegerValue
			@abstract   Creates a new element value using an integer value.
		    @discussion IOHIDValueGetTimeStamp should represent OS AbsoluteTime, not CFAbsoluteTime.
		                To obtain the OS AbsoluteTime, please reference the APIs declared in <mach/mach_time.h>
		    @param      allocator The CFAllocator which should be used to allocate memory for the value.  This 
		                parameter may be NULL in which case the current default CFAllocator is used. If this 
		                reference is not a valid CFAllocator, the behavior is undefined.
		    @param      element IOHIDElementRef associated with this value.
		    @param      timeStamp OS absolute time timestamp for this value.
		    @param      value Integer value to be copied to this object.
		    @result     Returns a reference to a new IOHIDValueRef.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDValueRef IOHIDValueCreateWithIntegerValue (CFAllocatorRef allocator, IOHIDElementRef element, uint64_t timeStamp, CFIndex value);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		/*!
			@function   IOHIDValueCreateWithBytes
			@abstract   Creates a new element value using byte data.
		    @discussion IOHIDValueGetTimeStamp should represent OS AbsoluteTime, not CFAbsoluteTime.
		                To obtain the OS AbsoluteTime, please reference the APIs declared in <mach/mach_time.h>
		    @param      allocator The CFAllocator which should be used to allocate memory for the value.  This 
		                parameter may be NULL in which case the current default CFAllocator is used. If this 
		                reference is not a valid CFAllocator, the behavior is undefined.
		    @param      element IOHIDElementRef associated with this value.
		    @param      timeStamp OS absolute time timestamp for this value.
		    @param      bytes Pointer to a buffer of uint8_t to be copied to this object.
		    @param      length Number of bytes in the passed buffer.
		    @result     Returns a reference to a new IOHIDValueRef.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDValueRef IOHIDValueCreateWithBytes (CFAllocatorRef allocator, IOHIDElementRef element, uint64_t timeStamp, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)][In]uint8_t[] bytes, CFIndex length);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		/*!
			@function   IOHIDValueCreateWithBytesNoCopy
			@abstract   Creates a new element value using byte data without performing a copy.
		    @discussion The timestamp value passed should represent OS AbsoluteTime, not CFAbsoluteTime.
		                To obtain the OS AbsoluteTime, please reference the APIs declared in <mach/mach_time.h>
		    @param      allocator The CFAllocator which should be used to allocate memory for the value.  This 
		                parameter may be NULL in which case the current default CFAllocator is used. If this 
		                reference is not a valid CFAllocator, the behavior is undefined.
		    @param      element IOHIDElementRef associated with this value.
		    @param      timeStamp OS absolute time timestamp for this value.
		    @param      bytes Pointer to a buffer of uint8_t to be referenced by this object.
		    @param      length Number of bytes in the passed buffer.
		    @result     Returns a reference to a new IOHIDValueRef.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDValueRef IOHIDValueCreateWithBytesNoCopy (CFAllocatorRef allocator, IOHIDElementRef element, uint64_t timeStamp, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)][In]uint8_t[] bytes, CFIndex length);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		/*!
			@function   IOHIDValueGetElement
			@abstract   Returns the element value associated with this IOHIDValueRef.
		    @param      value The value to be queried. If this parameter is not a valid IOHIDValueRef, the behavior is undefined.
		    @result     Returns a IOHIDElementRef referenced by this value.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementRef IOHIDValueGetElement (IOHIDValueRef value);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		/*!
			@function   IOHIDValueGetTimeStamp
			@abstract   Returns the timestamp value contained in this IOHIDValueRef.
		    @discussion The timestamp value returned represents OS AbsoluteTime, not CFAbsoluteTime.
		    @param      value The value to be queried. If this parameter is not a valid IOHIDValueRef, the behavior is undefined.
		    @result     Returns a uint64_t representing the timestamp of this value.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static uint64_t IOHIDValueGetTimeStamp (IOHIDValueRef value);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		/*!
			@function   IOHIDValueGetLength
			@abstract   Returns the size, in bytes, of the value contained in this IOHIDValueRef.
		    @param      value The value to be queried. If this parameter is not a valid IOHIDValueRef, the behavior is undefined.
		    @result     Returns length of the value.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDValueGetLength (IOHIDValueRef value);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		/*!
			@function   IOHIDValueGetBytePtr
			@abstract   Returns a byte pointer to the value contained in this IOHIDValueRef.
		    @param      value The value to be queried. If this parameter is not a valid IOHIDValueRef, the behavior is undefined.
		    @result     Returns a pointer to the value.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static /*const uint8_t*/ IntPtr IOHIDValueGetBytePtr (IOHIDValueRef value);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		/*!
			@function   IOHIDValueGetIntegerValue
			@abstract   Returns an integer representaion of the value contained in this IOHIDValueRef.
		    @discussion The value is based on the logical element value contained in the report returned by the device.
		    @param      value The value to be queried. If this parameter is not a valid IOHIDValueRef, the behavior is undefined.
		    @result     Returns an integer representation of the value.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDValueGetIntegerValue (IOHIDValueRef value);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		/*!
			@function   IOHIDValueGetScaledValue
			@abstract   Returns an scaled representaion of the value contained in this IOHIDValueRef based on the scale type.
		    @discussion The scaled value is based on the range described by the scale type's min and max, such that:
		        <br>
		        scaledValue = ((value - min) * (scaledMax - scaledMin) / (max - min)) + scaledMin
		        <br>
		        <b>Note:</b>
		        <br>
		        There are currently two types of scaling that can be applied:  
		        <ul>
		        <li><b>kIOHIDValueScaleTypePhysical</b>: Scales element value using the physical bounds of the device such that <b>scaledMin = physicalMin</b> and <b>scaledMax = physicalMax</b>.
		        <li><b>kIOHIDValueScaleTypeCalibrated</b>: Scales element value such that <b>scaledMin = -1</b> and <b>scaledMax = 1</b>.  This value will also take into account the calibration properties associated with this element.
		        </ul>
		    @param      value The value to be queried. If this parameter is not a valid IOHIDValueRef, the behavior is undefined.
		    @param      type The type of scaling to be performed.
		    @result     Returns an scaled floating point representation of the value.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static double_t IOHIDValueGetScaledValue (IOHIDValueRef value, IOHIDValueScaleType type);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER
	}
}

