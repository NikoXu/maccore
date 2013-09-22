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

namespace MonoMac.IOKit.USB
{
	/// <summary>
	/// Constants used by IOUSB classes
	/// </summary>
	public static class IOUSB
	{
		const string iousbFamilyBundlePath = "/System/Library/Extensions/IOUSBFamily.kext";

		// Constants relating to USB Power.
		public const int _100mAAvailable  = 50;
		public const int _500mAAvailable  = 250;
		public const int _100mA           = 50;
		public const int USB2MaxPowerPerPort = _500mAAvailable * 2;
		public const int _150mAAvailable  = 75;
		public const int _900mAAvailable  = 450;
		public const int _150mA           = 75;
		public const int USB3MaxPowerPerPort = _900mAAvailable * 2;

		// USB Descriptor and IORegistry constants
		// Various constants used to describe the fields in the various USB Device Descriptors and IORegistry names used for some of those fields 

		/// <summary>
		/// The field in the USB Device Descriptor corresponding to the device class
		/// </summary>
		public static string DeviceClass            = "bDeviceClass";

		/// <summary>
		/// The field in the USB Device Descriptor corresponding to the device sub class
		/// </summary>
		public static string DeviceSubClass         = "bDeviceSubClass";

		/// <summary>
		/// The field in the USB Device Descriptor corresponding to the device protocol
		/// </summary>
		public static string DeviceProtocol         = "bDeviceProtocol";

		/// <summary>
		/// The field in the USB Device Descriptor corresponding to the maximum packet size for endpoint 0
		/// </summary>
		public static string DeviceMaxPacketSize    = "bMaxPacketSize0";

		/// <summary>
		/// The field in the USB Device Descriptor corresponding to the device USB Vendor ID
		/// </summary>
		public static string VendorID = "idVendor";

		/// <summary>
		/// The field in the USB Device Descriptor corresponding to the device USB Product ID
		/// </summary>
		public static string ProductID               = "idProduct";

		/// <summary>
		/// The field in the USB Device Descriptor corresponding to the device release version
		/// </summary>
		public static string DeviceReleaseNumber     = "bcdDevice";

		/// <summary>
		/// The field in the USB Device Descriptor corresponding to the index for the manufacturer's string
		/// </summary>
		public static string ManufacturerStringIndex = "iManufacturer";

		/// <summary>
		/// The field in the USB Device Descriptor corresponding to the index for the product name's string
		/// </summary>
		public static string ProductStringIndex      = "iProduct";

		/// <summary>
		/// The field in the USB Device Descriptor corresponding to the index for the serial number's string
		/// </summary>
		public static string SerialNumberStringIndex = "iSerialNumber";

		/// <summary>
		/// The field in the USB Configuration Descriptor corresponding to the number of configurations
		/// </summary>
		public static string DeviceNumConfigs        = "bNumConfigurations";

		/// <summary>
		/// The field in the USB Configuration Descriptor corresponding to the number of configurations
		/// </summary>
		public static string InterfaceNumber         = "bInterfaceNumber";

		/// <summary>
		/// The field in the USB Configuration Descriptor corresponding to the number of configurations
		/// </summary>
		public static string AlternateSetting        = "bAlternateSetting";

		/// <summary>
		/// The field in the USB Configuration Descriptor corresponding to the number of configurations
		/// </summary>
		public static string NumEndpoints            = "bNumEndpoints";

		/// <summary>
		/// The field in the USB Interface Descriptor corresponding to the interface class
		/// </summary>
		public static string InterfaceClass          = "bInterfaceClass";

		/// <summary>
		/// The field in the USB Interface Descriptor corresponding to the interface sub class
		/// </summary>
		public static string InterfaceSubClass       = "bInterfaceSubClass";

		/// <summary>
		/// The field in the USB Interface Descriptor corresponding to the interface protocol
		/// </summary>
		public static string InterfaceProtocol       = "bInterfaceProtocol";

		/// <summary>
		/// The field in the USB Interface Descriptor corresponding to the index for the interface name's string
		/// </summary>
		public static string InterfaceStringIndex    = "iInterface";

		/// <summary>
		/// The field in the USB Interface Descriptor corresponding to the configuration
		/// </summary>
		public static string ConfigurationValue      = "bConfigurationValue";

		/// <summary>
		/// IORegistry key for the device's USB Product string
		/// </summary>
		public static string ProductString           = "USB Product Name";

		/// <summary>
		/// IORegistry key for the device's USB manufacturer string
		/// </summary>
		public static string VendorString            = "USB Vendor Name";

		/// <summary>
		/// IORegistry key for the device's USB serial number string
		/// </summary>
		public static string SerialNumberString      = "USB Serial Number";

		/// <summary>
		/// IORegistry key for the 1284 Device ID of a printer
		/// </summary>
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

		/// <summary>
		/// Converts a short value from host byte order to USB (little-endian) byte order.
		/// </summary>
		/// <returns>The value in USB order.</returns>
		/// <param name="value">The value in host order.</param>
		public static short HostToUSBOrder (this short value)
		{
			if (BitConverter.IsLittleEndian)
				return value;
			var array = BitConverter.GetBytes (value);
			Array.Reverse (array);
			return BitConverter.ToInt16 (array, 0);
		}

		/// <summary>
		/// Converts a short value from USB (little-endian) byte order to host byte order.
		/// </summary>
		/// <returns>The value in host order.</returns>
		/// <param name="value">The value in USB order.</param>
		public static short USBToHostOrder (this short value)
		{
			if (BitConverter.IsLittleEndian)
				return value;
			var array = BitConverter.GetBytes (value);
			Array.Reverse (array);
			return BitConverter.ToInt16 (array, 0);
		}

		/// <summary>
		/// Converts an int value from host byte order to USB (little-endian) byte order.
		/// </summary>
		/// <returns>The value in USB order.</returns>
		/// <param name="value">The value in host order.</param>
		public static int HostToUSBOrder (this int value)
		{
			if (BitConverter.IsLittleEndian)
				return value;
			var array = BitConverter.GetBytes (value);
			Array.Reverse (array);
			return BitConverter.ToInt32 (array, 0);
		}

		/// <summary>
		/// Converts an int value from USB (little-endian) byte order to host byte order.
		/// </summary>
		/// <returns>The value in host order.</returns>
		/// <param name="value">The value in USB order.</param>
		public static int USBToHostOrder (this int value)
		{
			if (BitConverter.IsLittleEndian)
				return value;
			var array = BitConverter.GetBytes (value);
			Array.Reverse (array);
			return BitConverter.ToInt32 (array, 0);
		}

		
		/// <summary>
		/// Converts an int value from host byte order to USB (little-endian) byte order.
		/// </summary>
		/// <returns>The value in USB order.</returns>
		/// <param name="value">The value in host order.</param>
		public static long HostToUSBOrder (this long value)
		{
			if (BitConverter.IsLittleEndian)
				return value;
			var array = BitConverter.GetBytes (value);
			Array.Reverse (array);
			return BitConverter.ToInt64 (array, 0);
		}

		/// <summary>
		/// Converts an int value from USB (little-endian) byte order to host byte order.
		/// </summary>
		/// <returns>The value in host order.</returns>
		/// <param name="value">The value in USB order.</param>
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

