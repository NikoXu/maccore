//
// IOUSBNub.cs
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

using UInt8 = System.Byte;

namespace MonoMac.IOKit.USB
{
	public class IOUSBNub : IOService
	{
		internal IOUSBNub (IntPtr handle, bool owns) : base (handle, owns)
		{
		}
	}

	public interface IIOUSBDescriptor
	{
		byte Length { get; }
		DescriptorType DescriptorType { get; }
	}

	/// <summary>
	/// Standard header used for all USB descriptors.
	/// Used to read the length of a descriptor so that we can allocate storage for the whole descriptor later on.
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	struct IOUSBDescriptorHeader : IIOUSBDescriptor
	{
		UInt8 length;
		public byte Length { get { return length; } }

		public DescriptorType descriptorType;
		public DescriptorType DescriptorType { get { return descriptorType; } }
	}

	/// <summary>
	/// Standard USB Configuration Descriptor.
	/// It is variable length, so this only specifies the known fields.
	/// We use the TotalLength field to read the whole descriptor.
	/// See the USB Specification at http://www.usb.org.
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBConfigurationDescriptor : IIOUSBDescriptor
	{
		UInt8 length;

		/// <summary>
		/// The length of this descriptor (bLength).
		/// </summary>
		public byte Length { get { return length; } }

		DescriptorType descriptorType;

		/// <summary>
		/// The type of the descriptor (bDescriptorType).
		/// </summary>
		/// <remarks>
		/// Will always be <see cref="DescriptorType.Configuration"/>
		/// </remarks>
		public DescriptorType DescriptorType { get { return descriptorType; } }

		UInt16 totalLength;

		/// <summary>
		/// The total length (wTotalLength).
		/// </summary>
		public ushort TotalLength {
			get { return (ushort)IOUSB.USBToHostOrder ((short)totalLength); }
		}

		/// <summary>
		/// The count of suppoted interfaces  (bNumInterfaces).
		/// </summary>
		public UInt8 InterfaceCount;

		/// <summary>
		/// The configuration value (bConfigurationValue).
		/// </summary>
		/// <remarks>
		/// Set the IOUSBDevice.CurrentConfiguration property to this value to
		/// select this configuration.
		/// </remarks>
		public UInt8 ConfigurationValue;

		/// <summary>
		/// The configuration index (iConfiguration).
		/// </summary>
		public UInt8 ConfigurationIndex;

		/// <summary>
		/// The attributes (bmAttributes).
		/// </summary>
		/// <remarks>
		/// <see cref="PowerAttributes.BusPowered"/> will always be set.
		/// </remarks>
		public PowerAttributes PowerAttributes;

		UInt8 maxPower;

		/// <summary>
		/// The max power in mA (bMaxPower).
		/// </summary>
		public int MaxPower {
			get {
				// TODO: check for Gen X mode
//				if (GenX)
//					return maxPower * 8;
				return maxPower * 2;
			}
		}
	}
}

