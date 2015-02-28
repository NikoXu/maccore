//
// IOUSB.cs
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
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace MonoMac.IOKit
{

	public static class IOUSB
	{
		const string iousbFamilyBundlePath = "/System/Library/Extensions/IOUSBFamily.kext";

		// Constants relating to USB Power.
		public const int _100mAAvailable     = 50;
		public const int _500mAAvailable     = 250;
		public const int _100mA              = 50;
		public const int USB2MaxPowerPerPort = _500mAAvailable * 2;
		public const int _150mAAvailable     = 75;
		public const int _900mAAvailable     = 450;
		public const int _150mA              = 75;
		public const int USB3MaxPowerPerPort = _900mAAvailable * 2;

		// USB Descriptor and IORegistry constants
		// Various constants used to describe the fields in the various USB Device Descriptors and IORegistry names used for some of those fields
		public static string DeviceClass             = "bDeviceClass";
		public static string DeviceSubClass          = "bDeviceSubClass";
		public static string DeviceProtocol          = "bDeviceProtocol";
		public static string DeviceMaxPacketSize     = "bMaxPacketSize0";
		public static string VendorID                = "idVendor";
		public static string ProductID               = "idProduct";
		public static string DeviceReleaseNumber     = "bcdDevice";
		public static string ManufacturerStringIndex = "iManufacturer";
		public static string ProductStringIndex      = "iProduct";
		public static string SerialNumberStringIndex = "iSerialNumber";
		public static string DeviceNumConfigs        = "bNumConfigurations";
		public static string InterfaceNumber         = "bInterfaceNumber";
		public static string AlternateSetting        = "bAlternateSetting";
		public static string NumEndpoints            = "bNumEndpoints";
		public static string InterfaceClass          = "bInterfaceClass";
		public static string InterfaceSubClass       = "bInterfaceSubClass";
		public static string InterfaceProtocol       = "bInterfaceProtocol";
		public static string InterfaceStringIndex    = "iInterface";
		public static string ConfigurationValue      = "bConfigurationValue";
		public static string ProductString           = "USB Product Name";
		public static string VendorString            = "USB Vendor Name";
		public static string SerialNumberString      = "USB Serial Number";
		public static string _1284DeviceID           = "1284 Device ID";

		static Lazy<NSBundle> bundle;

		static IOUSB ()
		{
			bundle = new Lazy<NSBundle> (() => NSBundle.FromPath (iousbFamilyBundlePath));
		}

		public static NSBundle Bundle { get { return bundle.Value; } }
		public static Version BundleVersion {
			get {
				var versionString = Bundle.InfoDictionary ["CFBundleShortVersionString"] as NSString;
				return new Version (versionString.ToString ());
			}
		}

		public static short HostToUSBOrder (this short value)
		{
			if (BitConverter.IsLittleEndian)
				return value;
			var array = BitConverter.GetBytes (value);
			Array.Reverse (array);
			return BitConverter.ToInt16 (array, 0);
		}

		public static short USBToHostOrder (this short value)
		{
			if (BitConverter.IsLittleEndian)
				return value;
			var array = BitConverter.GetBytes (value);
			Array.Reverse (array);
			return BitConverter.ToInt16 (array, 0);
		}

		public static int HostToUSBOrder (this int value)
		{
			if (BitConverter.IsLittleEndian)
				return value;
			var array = BitConverter.GetBytes (value);
			Array.Reverse (array);
			return BitConverter.ToInt32 (array, 0);
		}

		public static int USBToHostOrder (this int value)
		{
			if (BitConverter.IsLittleEndian)
				return value;
			var array = BitConverter.GetBytes (value);
			Array.Reverse (array);
			return BitConverter.ToInt32 (array, 0);
		}

		public static long HostToUSBOrder (this long value)
		{
			if (BitConverter.IsLittleEndian)
				return value;
			var array = BitConverter.GetBytes (value);
			Array.Reverse (array);
			return BitConverter.ToInt64 (array, 0);
		}

		public static long USBToHostOrder (this long value)
		{
			if (BitConverter.IsLittleEndian)
				return value;
			var array = BitConverter.GetBytes (value);
			Array.Reverse (array);
			return BitConverter.ToInt64 (array, 0);
		}
	}
}

