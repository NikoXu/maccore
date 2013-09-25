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
		/* This is used to find HID Devices in the IORegistry */
		public const string DeviceKey                   = "IOHIDDevice";

		/*!
		    @define kIOHIDElementKey
		    @abstract Keys that represents an element property.
		    @discussion Property for a HID Device or element dictionary.
		        Elements can be heirarchical, so they can contain other elements.
		*/
		public const string ElementKey                  = "Elements";
	}

	/*! @	delegate IOHIDCallback
    @discussion Type and arguments of callout C function that is used when a completion routine is called.
    @param context void * pointer to your data, often a pointer to an object.
    @param result Completion result of desired operation.
    @param refcon void * pointer to more data.
    @param sender Interface instance sending the completion routine.
*/
	delegate void IOHIDCallback (
                                    IntPtr                  context, 
                                    IOReturn                result, 
                                    IntPtr                  sender);

/*! @	delegate IOHIDReportCallback
    @discussion Type and arguments of callout C function that is used when a HID report completion routine is called.
    @param context void * pointer to your data, often a pointer to an object.
    @param result Completion result of desired operation.
    @param refcon void * pointer to more data.
    @param sender Interface instance sending the completion routine.
    @param type The type of the report that was completed.
    @param reportID The ID of the report that was completed.
    @param report Pointer to the buffer containing the contents of the report.
    @param reportLength Size of the buffer received upon completion.
*/
	delegate void IOHIDReportCallback (
                                    IntPtr                  context, 
                                    IOReturn                result, 
                                    IntPtr                  sender, 
                                    IOHIDReportType         type, 
                                    uint32_t                reportID,
	                                [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 6)]
                                    uint8_t[]               report, 
                                    CFIndex                 reportLength);

/*! @	delegate IOHIDValueCallback
    @discussion Type and arguments of callout C function that is used when an element value completion routine is called.
    @param context void * pointer to more data.
    @param result Completion result of desired operation.
    @param sender Interface instance sending the completion routine.
    @param value IOHIDValueRef containing the returned element value.
*/
	delegate void IOHIDValueCallback ( 
                                    IntPtr                  context,
                                    IOReturn                result, 
                                    IntPtr                  sender,
                                    IOHIDValueRef           value);

/*! @	delegate IOHIDValueMultipleCallback
    @discussion Type and arguments of callout C function that is used when an element value completion routine is called.
    @param context void * pointer to more data.
    @param result Completion result of desired operation.
    @param sender Interface instance sending the completion routine.
    @param multiple CFDictionaryRef containing the returned element key value pairs.
*/
	delegate void IOHIDValueMultipleCallback ( 
                                    IntPtr                  context,
                                    IOReturn                result, 
                                    IntPtr                  sender,
                                    CFDictionaryRef         multiple);

/*! @	delegate IOHIDDeviceCallback
    @discussion Type and arguments of callout C function that is used when a device routine is called.
    @param context void * pointer to more data.
    @param result Completion result of desired operation.
    @param device IOHIDDeviceRef containing the sending device.
*/
	delegate void IOHIDDeviceCallback ( 
                                    IntPtr                  context,
                                    IOReturn                result, 
                                    IntPtr                  sender,
                                    IOHIDDeviceRef          device);
}

