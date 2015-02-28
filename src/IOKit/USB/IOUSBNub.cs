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
using MonoMac.CoreFoundation;

namespace MonoMac.IOKit
{
	public class IOUSBNub : IOService
	{
		internal IOUSBNub (IntPtr handle, bool owns) : base (handle, owns)
		{
		}
	}

	public interface IIOUSBDescriptor
	{
		int Length { get; }
		DescriptorType DescriptorType { get; }
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOUSBDescriptorHeader : IIOUSBDescriptor
	{
		UInt8 bLength;
		UInt8 bDescriptorType;

		public int Length { get { return (int)bLength; } }

		public DescriptorType DescriptorType {
			get { return (DescriptorType)bDescriptorType; }
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOUSBDeviceDescriptor : IOUSBDescriptorHeader
	{
		UInt16  bcdUSB;
		UInt8  bDeviceClass;
		UInt8  bDeviceSubClass;
		UInt8  bDeviceProtocol;
		UInt8  bMaxPacketSize0;
		UInt16 idVendor;
		UInt16 idProduct;
		UInt16 bcdDevice;
		UInt8  iManufacturer;
		UInt8  iProduct;
		UInt8  iSerialNumber;
		UInt8  bNumConfigurations;
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOUSBBOSDescriptor : IOUSBDescriptorHeader
	{
		UInt16 			wTotalLength;
		UInt8 			bNumDeviceCaps;

		public int TotalLength {
			get { return (int)IOUSB.USBToHostOrder ((short)wTotalLength); }
		}

		public int DeviceCapabilityCount { get { return (int)bNumDeviceCaps; } }
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOUSBDeviceCapabilityDescriptorHeader : IOUSBDescriptorHeader
	{
		UInt8 			bDevCapabilityType;

		public DeviceCapabilityType DeviceCapabilityType {
			get { return (DeviceCapabilityType)bDevCapabilityType; }
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOUSBDeviceCapabilityUSB2Extension : IOUSBDeviceCapabilityDescriptorHeader
	{
		UInt32 			bmAttributes;

		public bool SupportsLPM { get { return (bmAttributes & 0x02) == 0x02; } }
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOUSBDeviceCapabilitySuperSpeedUSB : IOUSBDeviceCapabilityDescriptorHeader
	{
		UInt8 			bmAttributes;
		UInt16			wSpeedsSupported;
		UInt8			bFunctionalitySupport;
		UInt8			bU1DevExitLat;
		UInt16			wU2DevExitLat;

		public bool SupportsLPM { get { return (bmAttributes & 0x02) == 0x02; } }

		public SupportedDeviceSpeeds SupportedSpeeds {
			get { return (SupportedDeviceSpeeds)(wSpeedsSupported & 0x0F); }
		}

		public DeviceSpeed DeviceSpeed {
			get { return (DeviceSpeed)(bFunctionalitySupport & 0x0F); }
		}

		public byte U1DevExitLat { get { return bU1DevExitLat; } }

		public ushort U2DevExitLat { get { return wU2DevExitLat; } }
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOUSBDeviceCapabilityContainerID : IOUSBDescriptorHeader
	{
		UInt8			bReservedID;
		[MarshalAs (UnmanagedType.LPArray, SizeConst = 16)]
		UInt8[]			containerID;

		public CFUUID ID { get { return new CFUUID (containerID); } }
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOUSBConfigurationDescriptor : IOUSBDescriptorHeader
	{
		UInt16 wTotalLength;
		UInt8  bNumInterfaces;
		UInt8  bConfigurationValue;
		UInt8  iConfiguration;
		UInt8  bmAttributes;
		UInt8  maxPower;

		public int TotalLength {
			get { return (int)IOUSB.USBToHostOrder ((short)wTotalLength); }
		}

		public int InterfaceCount { get { return bNumInterfaces; } }

		public byte ConfigurationValue { get { return bConfigurationValue; } }

		public byte ConfigurationIndex { get { return iConfiguration; } }

		public PowerAttributes PowerAttributes {
			get { return (PowerAttributes)bmAttributes; }
		}

		public int MaxPower {
			get {
				// TODO: check for Gen X mode
//				if (GenX)
//					return maxPower * 8;
				return maxPower * 2;
			}
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOUSBInterfaceDescriptor : IOUSBDescriptorHeader
	{
		UInt8 bInterfaceNumber;
		UInt8 bAlternateSetting;
		UInt8 bNumEndpoints;
		UInt8 bInterfaceClass;
		UInt8 bInterfaceSubClass;
		UInt8 bInterfaceProtocol;
		UInt8 iInterface;

		public byte InterfaceNumber { get { return bInterfaceNumber; } }

		public byte AlternateSetting { get { return bAlternateSetting; } }

		public int EndpointCount { get { return (int)bNumEndpoints; } }

		public InterfaceClass InterfaceClass {
			get { return (InterfaceClass)bInterfaceClass; }
		}

		public InterfaceSubClass InterfaceSubClass {
			get { return (InterfaceSubClass)bInterfaceSubClass; }
		}

		public InterfaceProtocol InterfaceProtocol {
			get { return (InterfaceProtocol)bInterfaceProtocol; }
		}

		public byte InterfaceIndex { get { return iInterface; } }
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOUSBEndpointDescriptor : IOUSBDescriptorHeader
	{
		UInt8  bEndpointAddress;
		UInt8  bmAttributes;
		UInt16 wMaxPacketSize;
		UInt8  bInterval;

		public byte Address {
			get { return (byte)(bEndpointAddress & 0x0F); }
		}

		public EndpointDirection Direction {
			get { return (EndpointDirection)(bEndpointAddress >> 7); }
		}

		public EndpointTransferType TransferType {
			get { return (EndpointTransferType)(bmAttributes & 0x03); }
		}

		public InterruptUsageType InterruptUsageType {
			get { return (InterruptUsageType)(bmAttributes >> 4 & 0x03); }
		}

		public IsocSyncType IsocSyncType {
			get { return (IsocSyncType)(bmAttributes >> 2 & 0x03); }
		}

		public IsocUsageType IsocUsageType {
			get { return (IsocUsageType)(bmAttributes >> 4 & 0x03); }
		}

		public int MaxPacketSize { get { return (int)wMaxPacketSize; } }

		public int Interval { get { return bInterval; } }
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOUSBSuperSpeedEndpointCompanionDescriptor : IOUSBDescriptorHeader
	{
		UInt8  bMaxBurst;
		UInt8  bmAttributes;
		UInt16 wBytesPerInterval;

		public int MaxBurst { get { return (int)bMaxBurst; } }

		public int MaxStreams { get { return (int)(bmAttributes & 0x1F); } }

		public int Mult { get { return (int)(bmAttributes & 0x2); } }

		public bool SspIsoCompanion { get { return (bmAttributes & 0x80) != 0; } }

		public int BytesPerInterval { get { return (int)wBytesPerInterval; } }
	}
}

