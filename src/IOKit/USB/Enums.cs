//
// Enums.cs
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

namespace MonoMac.IOKit.USB
{
	public enum EndpointTransferType : byte
	{
		Control     = 0,
		Isoc        = 1,
		Bulk        = 2,
		Interrupt   = 3,
		Any         = 0xFF
	}

	public enum EndpointDirection : byte
	{
		Out         = 0,
		In          = 1,
		None        = 2,
		Any         = 3
	}

	public enum DeviceRequestType : byte
	{
		Standard    = 0,
		Class       = 1,
		Vendor      = 2
	};

	public enum DeviceRequestRecipient : byte
	{
		Device      = 0,
		Interface   = 1,
		Endpoint    = 2,
		Other       = 3
	}

	public enum RequestType : byte
	{
		GetStatus     = 0,
		ClearFeature  = 1,
		GetState      = 2,
		SetFeature    = 3,
		Reserved2     = 4,
		SetAddress    = 5,
		GetDescriptor = 6,
		SetDescriptor = 7,
		GetConfig     = 8,
		SetConfig     = 9,
		GetInterface  = 10,
		SetInterface  = 11,
		SyncFrame     = 12,
		SetSel        = 48,
		SetIsochDelay = 49
	}
#if COREBUILD
	public enum DescriptorType
	{
		Temp = 1;
	}

	public enum DeviceCapabilityType
	{
		Temp = 1;
	}
#else
	public enum DescriptorType : byte
	{
		[DescriptorClass (typeof(IOUSBDescriptorHeader))]
		Any                         = 0,
		[DescriptorClass (typeof(IOUSBDeviceDescriptor))]
		Device                      = 1,
		[DescriptorClass (typeof(IOUSBConfigurationDescriptor))]
		Configuration               = 2,
		String                      = 3,
		[DescriptorClass (typeof(IOUSBInterfaceDescriptor))]
		Interface                   = 4,
		[DescriptorClass (typeof(IOUSBEndpointDescriptor))]
		Endpoint                    = 5,
		DeviceQualifier             = 6,
		OtherSpeedConfiguration     = 7,
		InterfacePower              = 8,
		OnTheGo	                    = 9,
		Debug                       = 10,
		InterfaceAssociation        = 11,
		[DescriptorClass (typeof(IOUSBBOSDescriptor))]
		BOS                          = 15,
		[DescriptorClass (typeof(IOUSBDeviceCapabilityDescriptorHeader))]
		DeviceCapability            = 16,
		[DescriptorClass (typeof(IOUSBSuperSpeedEndpointCompanionDescriptor))]
		SuperSpeedEndpointCompanion = 48,
		ThreeHUB                    = 0x2A,
		HID                         = 0x21,
		Report                      = 0x22,
		Physical                    = 0x23,
		HUB                         = 0x29,
	}

	public enum DeviceCapabilityType : byte
	{
		WirelessUSB     = 1,
		[DescriptorClass (typeof(IOUSBDeviceCapabilityUSB2Extension))]
		USB20Extension  = 2,
		[DescriptorClass (typeof(IOUSBDeviceCapabilitySuperSpeedUSB))]
		SuperSpeedUSB   = 3,
		[DescriptorClass (typeof(IOUSBDeviceCapabilityContainerID))]
		ContainerID     = 4
	}

	public static class DescriptorTypeExtensions
	{
		public static Type GetClassType (this DescriptorType value)
		{
			var fieldInfo = typeof(DescriptorType).GetField (value.ToString ());
			var attribute = (DescriptorClassAttribute)Attribute.GetCustomAttribute (fieldInfo, typeof(DescriptorClassAttribute));
			return attribute.Type;
		}

		public static Type GetClassType (this DeviceCapabilityType value)
		{
			var fieldInfo = typeof(DeviceCapabilityType).GetField (value.ToString ());
			var attribute = (DescriptorClassAttribute)Attribute.GetCustomAttribute (fieldInfo, typeof(DescriptorClassAttribute));
			return attribute.Type;
		}
	}

	[AttributeUsage (AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class DescriptorClassAttribute : Attribute
	{
		public Type Type { get; private set; }

		public DescriptorClassAttribute (Type type)
		{
			this.Type = type;
		}
	}
#endif

	public enum FeatureSelectors : byte
	{
		EndpointStall       = 0,
		DeviceRemoteWakeup  = 1,
		TestMode            = 2,
		FunctionSuspend     = 0,
		U1Enable            = 48,
		U2Enable            = 49,
		LTMEnable           = 50
	}

	[Flags]
	public enum PowerAttributes : byte
	{
		BusPowered   = 0x80,
		SelfPowered  = 0x40,
		RemoteWakeup = 0x20,
	}

	public enum Release : ushort
	{
		R10       = 0x0100,
		R11       = 0x0110,
		R20       = 0x0200,
		R30       = 0x0300
	}

	public enum HIDRequest : byte
	{
		GetReport     = 1,
		GetIdle       = 2,
		GetProtocol   = 3,
		SetReport     = 9,
		SetIdle       = 10,
		SetProtocol   = 11
	}

	public enum HIDReport : byte
	{
		Input       = 1,
		Output      = 2,
		Feature     = 3
	}

	public enum HIDProtocol : byte
	{
		Boot   = 0,
		Report = 1
	}

	public enum DeviceClass : byte
	{
		Composite             = 0,
		Communication         = 2,
		Hub                   = 9,
		Data                  = 10,
		PersonalHealthcare    = 15,
		Diagnostic            = 220,
		WirelessController    = 224,
		Miscellaneous         = 239,
		ApplicationSpecific   = 254,
		VendorSpecific        = 255
	}


	public enum InterfaceClass : ushort
	{
		Audio                 = 1,
		CommunicationControl  = 2,
		CommunicationData     = 10,
		HID                   = 3,
		Physical              = 5,
		Image                 = 6,
		Printing              = 7,
		MassStorage           = 8,
		ChipSmartCard         = 11,
		ContentSecurity       = 13,
		Video                 = 14,
		PersonalHealthcare    = 15,
		DiagnosticDevice      = 220,
		WirelessController    = 224,
		ApplicationSpecific   = 254,
		VendorSpecific        = 255
	}

	public enum InterfaceSubClass : ushort
	{
		Composite               = 0,

		Hub                     = 0,

		// For the AudioInterfaceClass
		//
		AudioControl            = 0x01,
		AudioStreaming          = 0x02,
		AudioMIDIStreaming      = 0x03,

		// For the ApplicationSpecificInterfaceClass
		//
		AppDFU                  = 0x01,
		AppIrDABridge           = 0x02,
		AppTestMeasurement      = 0x03,

		// For the MassStorageInterfaceClass
		//
		MassStorageRBC          = 0x01,
		MassStorageATAPI        = 0x02,
		MassStorageQIC157       = 0x03,
		MassStorageUFI          = 0x04,
		MassStorageSFF8070i     = 0x05,
		MassStorageSCSI         = 0x06,

		// For the HIDInterfaceClass
		//
		HIDBootInterface        = 0x01,

		// For the CommunicationDataInterfaceClass
		//
		CommDirectLine          = 0x01,
		CommAbstract            = 0x02,
		CommTelephone           = 0x03,
		CommMultiChannel        = 0x04,
		CommCAPI                = 0x05,
		CommEthernetNetworking  = 0x06,
		CommATMNetworking       = 0x07,

		// For the DiagnosticDeviceInterfaceClass
		//
		ReprogrammableDiagnostic = 0x01,

		// For the WirelessControllerInterfaceClass
		//
		RFController = 0x01,

		// For the MiscellaneousClass
		//
		CommonClass = 0x02,

		// For the VideoInterfaceClass
		//
		VideoControl             = 0x01,
		VideoStreaming           = 0x02,
		VideoInterfaceCollection = 0x03
	}

	public enum InterfaceProtocol : ushort
	{
		// For kUSBHubClass
		HubSuperSpeed			= 3,

		// For kUSBHIDInterfaceClass
		HIDNoInterface			= 0,
		HIDKeyboardInterface	= 1,
		HIDMouseInterface		= 2,
		USBVendorSpecific		= 0xff,

		// For kUSBDiagnosticDeviceInterfaceClass
		USB2ComplianceDevice	= 0x01,

		// For kUSBWirelessControllerInterfaceClass
		USBBluetoothProgrammingInterface	= 0x01,

		// For kUSBMiscellaneousClass
		USBInterfaceAssociationDescriptor	= 0x01,

		// For Mass Storage
		MSCControlBulkInterrupt		= 0x00,
		MSCControlBulk				= 0x01,
		MSCBulkOnly					= 0x50,
		MSCUSBAttachedSCSI			= 0x62
	}

	public enum DeviceSpeed : byte
	{
		Low   = 0,
		Full  = 1,
		High  = 2,
		Super = 3
	}

	[Flags]
	public enum SupportedDeviceSpeeds
	{
		Low   = 1 << 0,
		Full  = 1 << 1,
		High  = 1 << 2,
		Gen1  = 1 << 3
	}

	[Flags]
	public enum DeviceInformation : uint
	{
		IsCaptive               = (1 << 0),
		IsAttachedToRootHubMask	= (1 << 1),
		IsInternal              = (1 << 2),
		IsConnected             = (1 << 3),
		IsEnabled               = (1 << 4),
		IsSuspended             = (1 << 5),
		IsInReset               = (1 << 6),
		Overcurrent             = (1 << 7),
		PortIsInTestMode        = (1 << 8),
		IsRootHub               = (1 << 9),
		RootHubIsBuiltIn        = (1 << 10),
		IsRemote                = (1 << 11),
		IsAttachedToEnclosure   = (1 << 12),
		IsOnThunderbolt         = (1 << 13)
	}

	public enum PowerRequestType : byte
	{
		DuringSleep            = 0,
		DuringWake             = 1,
		RequestWakeRelease     = 2,
		RequestSleepRelease    = 3,
		RequestWakeReallocate  = 4,
		RequestSleepReallocate = 5,
		DuringWakeRevocable    = 6,
		DuringWakeUSB3         = 7
	}

	public enum Interval : byte
	{
		I1   = 1,
		I2   = 2,
		I4   = 4,
		I8   = 8,
		I16  = 16,
		I32  = 32,
		I64  = 64,
		I128 = 128
	}

	public enum EndpointPropertiesVersion : byte
	{
		V1 = 1,
		V2 = 2,
		V3 = 3
	}

	public enum InterruptUsageType : byte
	{
		Periodic     = 0,
		Notification = 1,
	}

	public enum IsocSyncType : byte
	{
		NoSynchronization = 0,
		Asynchronous      = 1,
		Adaptive          = 2,
		Synchronous       = 3,
	}

	public enum IsocUsageType : byte
	{
		Data                 = 0,
		Feedback             = 1,
		ImplicitFeedbackData = 2
	}
}

