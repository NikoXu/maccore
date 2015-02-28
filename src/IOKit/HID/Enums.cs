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

namespace MonoMac.IOKit
{
	public enum IOHIDElementType
	{
		InputMisc        = 1,
		InputButton      = 2,
		InputAxis        = 3,
		InputScanCodes   = 4,
		Output           = 129,
		Feature          = 257,
		Collection       = 513
	}

	public enum IOHIDElementCollectionType
	{
		Physical = 0x00,
		Application,
		Logical,
		Report,
		NamedArray,
		UsageSwitch,
		UsageModifier
	}

	public enum IOHIDReportType
	{
		Input = 0,
		Output,
		Feature,
	}
	[Flags]
	public enum IOHIDOptionsType : uint
	{
		None	 = 0x00,
		SeizeDevice = 0x01
	}

	[Flags]
	public enum IOHIDQueueOptionsType : uint
	{
		None	 = 0x00,
		EnqueueAll = 0x01
	}

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

	public enum IOHIDStandardType : uint
	{
		ANSI = 0,
		ISO  = 1,
		JIS  = 2
	}

	public enum IOHIDValueScaleType : uint
	{
		Calibrated,
		Physical
	}

	public enum IOHIDTransactionDirectionType : uint
	{
		Input,
		Output
	}

	[Flags]
	public enum IOHIDManagerOptions : uint
	{
		None                    = 0x0,
		UsePersistentProperties = 0x1,
		DoNotLoadProperties     = 0x2,
		DoNotSaveProperties     = 0x4,
	}

	public enum UsagePage : ushort
	{
		Undefined	= 0x00,
		GenericDesktop	= 0x01,
		Simulation	= 0x02,
		VR	= 0x03,
		Sport	= 0x04,
		Game	= 0x05,
		/* Reserved 0x06 */
		KeyboardOrKeypad	= 0x07,
		LEDs	= 0x08,
		Button	= 0x09,
		Ordinal	= 0x0A,
		Telephony	= 0x0B,
		Consumer	= 0x0C,
		Digitizer	= 0x0D,
		/* Reserved 0x0E */
		PhysicalInterfaceDevice	= 0x0F,
		Unicode	= 0x10,
		/* Reserved 0x11 - 0x13 */
		AlphanumericDisplay	= 0x14,
		/* Reserved 0x15 - 0x7F */
		/* Monitor 0x80 - 0x83 */
		/* Power 0x84 - 0x87 */
		PowerDevice = 0x84,
		BatterySystem = 0x85,
		/* Reserved 0x88 - 0x8B */
		BarCodeScanner	= 0x8C,
		WeighingDevice	= 0x8D,
		Scale	= 0x8D,
		MagneticStripeReader = 0x8E,
		/* ReservedPointofSalepages 0x8F */
		CameraControl	= 0x90,
		Arcade	= 0x91,
		/* Reserved 0x92 - 0xFEFF */
		/* VendorDefined 0xFF00 - 0xFFFF */
		VendorDefinedStart	= 0xFF00
	}
}

