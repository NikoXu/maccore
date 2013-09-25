//
// Enums.cs
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

namespace MonoMac.IOKit.HID
{
	/// <summary>
	/// Describes different types of HID elements.
	/// </summary>
	/// <remarks>
	/// Used by the IOHIDFamily to identify the type of
	/// element processed.  Represented by the key kIOHIDElementTypeKey in the 
	/// dictionary describing the element.
	/// </remarks>
	public enum IOHIDElementType
	{
		/// <summary>
		/// Misc input data field or varying size.
		/// </summary>
		Input_Misc        = 1,

		/// <summary>
		/// One bit input data field.
		/// </summary>
		Input_Button      = 2,

		/// <summary>
		/// Input data field used to represent an axis.
		/// </summary>
		Input_Axis        = 3,

		/// <summary>
		/// Input data field used to represent a scan code or usage selector.
		/// </summary>
		Input_ScanCodes   = 4,

		/// <summary>
		/// Used to represent an output data field in a report.
		/// </summary>
		Output            = 129,

		/// <summary>
		/// Describes input and output elements not intended for 
		/// consumption by the end user.
		/// </summary>
		Feature           = 257,

		/// <summary>
		/// Element used to identify a relationship between two or more elements.
		/// </summary>
		Collection        = 513
	}

	/// <summary>
	/// Describes different types of HID collections.
	/// </summary>
	/// <remarks>
	/// Collections identify a relationship between two or more
	/// elements.
	/// </remarks>
	public enum IOHIDElementCollectionType
	{
		/// <summary>
		/// Used for a set of data items that represent data points 
		/// collected at one geometric point.
		/// </summary>
		Physical	= 0x00,

		/// <summary>
		/// Identifies item groups serving different purposes in a single device.
		/// </summary>
		Application,

		/// <summary>
		/// Used when a set of data items form a composite data structure.
		/// </summary>
		Logical,

		/// <summary>
		/// Wraps all the fields in a report.
		/// </summary>
		Report,

		/// <summary>
		/// Contains an array of selector usages.
		/// </summary>
		NamedArray,

		/// <summary>
		/// Modifies the meaning of the usage it contains.
		/// </summary>
		UsageSwitch,

		/// <summary>
		/// Modifies the meaning of the usage attached to the encompassing collection.
		/// </summary>
		UsageModifier
	}

	/// <summary>
	/// Describes different type of HID reports.
	/// </summary>
	/// <remarks>
	/// Used by the IOHIDFamily to identify the type of
	/// report being processed.
	/// </remarks>
	public enum IOHIDReportType
	{
		/// <summary>
		/// Input report.
		/// </summary>
		Input = 0,

		/// <summary>
		/// Output report.
		/// </summary>
		Output,

		/// <summary>
		/// Feature report.
		/// </summary>
		Feature,
	}

	/// <summary>
	/// Options for opening a device via IOHIDLib.
	/// </summary>
	public enum IOHIDOptionsType : uint
	{
		/// <summary>
		/// Default option.
		/// </summary>
		None	 = 0x00,

		/// <summary>
		/// Used to open exclusive
		/// communication with the device.  This will prevent the system
		/// and other clients from receiving events from the device.
		/// </summary>
		SeizeDevice = 0x01
	}

	/// <summary>
	/// Options for creating a queue via IOHIDLib.
	/// </summary>
	public enum IOHIDQueueOptionsType : uint
	{
		/// <summary>
		/// Default option.
		/// </summary>
		None	 = 0x00,

		/// <summary>
		/// Force the IOHIDQueue
		/// to enqueue all events, relative or absolute, regardless of change.
		/// </summary>
		EnqueueAll = 0x01
	}

	/// <summary>
	/// IOHID element flags.
	/// </summary>
	[Flags]
	public enum IOHIDElementFlags : uint
	{
		Constant        = 0x0001,
		Variable        = 0x0002,
		Relative        = 0x0004,
		Wrap            = 0x0008,
		NonLinear       = 0x0010,
		NoPreferred     = 0x0020,
		NullState       = 0x0040,
		Volative        = 0x0080,
		BufferedByte    = 0x0100
	}

	/// <summary>
	/// Type to define what industrial standard the device is referencing.
	/// </summary>
	public enum IOHIDStandardType : uint
	{
		/// <summary>
		/// ANSI.
		/// </summary>
		ANSI                = 0,

		/// <summary>
		/// ISO.
		/// </summary>
		ISO                 = 1,

		/// <summary>
		/// JIS.
		/// </summary>
		JIS                 = 2
	}

	/// <summary>
	/// Describes different types of scaling that can be performed on element values.
	/// </summary>
	public enum IOHIDValueScaleType : uint
	{
		/// <summary>
		/// Type for value that is scaled with respect to the calibration properties.
		/// </summary>
		Calibrated,

		/// <summary>
		/// Type for value that is scaled with respect to the physical min and physical max of the element.
		/// </summary>
		Physical
	}

	/// <summary>
	/// Direction for an IOHIDDeviceTransactionInterface.
	/// </summary>
	public enum IOHIDTransactionDirectionType : uint
	{
		/// <summary>
		/// Transaction direction used for requesting element values from a device.
		/// </summary>
		Input,

		/// <summary>
		/// Transaction direction used for dispatching element values to a device.
		/// </summary>
		Output
	}

	/// <summary>
	/// Various options that can be supplied to IOHIDManager functions.
	/// </summary>
	[Flags]
	public enum IOHIDManagerOptions : uint
	{
		/// <summary>
		/// For those times when supplying 0 just isn't explicit enough.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// This constant can be supplied to @link IOHIDManagerCreate @/link
		/// to create and/or use a persistent properties store.
		/// </summary>
		UsePersistentProperties = 0x1,

		/// <summary>
		/// This constant can be supplied to @link IOHIDManagerCreate when you wish to overwrite 
		/// the persistent properties store without loading it first.
		/// </summary>
		DoNotLoadProperties = 0x2,

		/// <summary>
		/// This constant can be supplied to @link IOHIDManagerCreate @/link when you want to 
		/// use the persistent property store but do not want to add to it.
		/// </summary>
		DoNotSaveProperties = 0x4,
	}

	/// <summary>
	/// Usage Pages
	/// </summary>
	public enum UsagePage : ushort
	{
		Undefined	= 0x00,
		GenericDesktop	= 0x01,
		Simulation	= 0x02,
		VR	= 0x03,
		Sport	= 0x04,
		Game	= 0x05,
		/* Reserved 0x06 */
		KeyboardOrKeypad	= 0x07,	/* USB Device Class Definition for Human Interface Devices (HID). Note: the usage type for all key codes is Selector (Sel). */
		LEDs	= 0x08,
		Button	= 0x09,
		Ordinal	= 0x0A,
		Telephony	= 0x0B,
		Consumer	= 0x0C,
		Digitizer	= 0x0D,
		/* Reserved 0x0E */
		PID	= 0x0F,	/* USB Physical Interface Device definitions for force feedback and related devices. */
		Unicode	= 0x10,
		/* Reserved 0x11 - 0x13 */
		AlphanumericDisplay	= 0x14,
		/* Reserved 0x15 - 0x7F */
		/* Monitor 0x80 - 0x83	 USB Device Class Definition for Monitor Devices */
		/* Power 0x84 - 0x87	 USB Device Class Definition for Power Devices */
		PowerDevice = 0x84, 				/* Power Device Page */
		BatterySystem = 0x85, 				/* Battery System Page */
		/* Reserved 0x88 - 0x8B */
		BarCodeScanner	= 0x8C,	/* (Point of Sale) USB Device Class Definition for Bar Code Scanner Devices */
		WeighingDevice	= 0x8D,	/* (Point of Sale) USB Device Class Definition for Weighing Devices */
		Scale	= 0x8D,	/* (Point of Sale) USB Device Class Definition for Scale Devices */
		MagneticStripeReader = 0x8E,
		/* ReservedPointofSalepages 0x8F */
		CameraControl	= 0x90,	/* USB Device Class Definition for Image Class Devices */
		Arcade	= 0x91,	/* OAAF Definitions for arcade and coinop related Devices */
		/* Reserved 0x92 - 0xFEFF */
		/* VendorDefined 0xFF00 - 0xFFFF */
		VendorDefinedStart	= 0xFF00
	};
}

