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
	/// <summary>
	/// Used in IOUSBFindEndpointRequest's type field
	/// </summary>
	public enum EndpointType : byte
	{
		Control     = 0,
		Isoc        = 1,
		Bulk        = 2,
		Interrupt   = 3,
		Any         = 0xFF
	}

	/// <summary>
	/// Used in IOUSBFindEndpointRequest's direction field
	/// </summary>
	public enum EndpointDirection : byte
	{
		Out         = 0,
		In          = 1,
		None        = 2,
		Any         = 3
	}

	/// <summary>
	/// This type is encoded in the bmRequestType field of a Device Request.
	/// It specifies the type of request: standard, class or vendor specific.
	/// </summary>
	public enum DeviceRequestType : byte
	{
		Standard    = 0,
		Class       = 1,
		Vendor      = 2
	};

	/// <summary>
	/// This recipient is encoded in the bmRequestType field of a Device Request.
	/// It specifies the type of recipient for a request:  the device, the interface, or an endpoint.
	/// </summary>
	public enum DeviceRequestRecipient : byte
	{
		Device      = 0,
		Interface   = 1,
		Endpoint    = 2,
		Other       = 3
	}

	/// <summary>
	/// Specifies values for the bRequest field of a Device Request.
	/// </summary>
	public enum RequestType : byte
	{
		RqGetStatus     = 0,
		RqClearFeature  = 1,
		RqGetState      = 2,
		RqSetFeature    = 3,
		RqReserved2     = 4,
		RqSetAddress    = 5,
		RqGetDescriptor = 6,
		RqSetDescriptor = 7,
		RqGetConfig     = 8,
		RqSetConfig     = 9,
		RqGetInterface  = 10,
		RqSetInterface  = 11,
		RqSyncFrame     = 12,
		SetSel			= 48,
		SetIsochDelay	= 49
	}

	/// <summary>
	/// Specifies values for diffent descriptor types.
	/// </summary>
	public enum DescriptorType : byte
	{
		Any                         = 0,    // Wildcard for searches
		Device                      = 1,
		Configuration               = 2,
		String                      = 3,
		Interface                   = 4,
		Endpoint                    = 5,
		DeviceQualifier             = 6,
		OtherSpeedConfiguration     = 7,
		InterfacePower              = 8,
		OnTheGo	                    = 9,
		Debug                       = 10,
		InterfaceAssociation        = 11,
		OS                          = 15,
		DeviceCapability            = 16,
		SuperSpeedEndpointCompanion = 48,
		ThreeHUB                    = 0x2A,
		HID                         = 0x21,
		Report                      = 0x22,
		Physical                    = 0x23,
		HUB                         = 0x29,
	}

	/// <summary>
	/// Used with decoding the Device Capability descriptor
	/// </summary>
	public enum DeviceCapabilityTypes : byte
	{
		WirelessUSB     = 1,
		USB20Extension  = 2,
		SuperSpeedUSB   = 3,
		ContainerID     = 4
	}

	/// <summary>
	/// Used with SET/CLEAR_FEATURE requests.
	/// </summary>
	public enum FeatureSelectors : byte
	{
		EndpointStall       = 0,		// Endpoint
		DeviceRemoteWakeup  = 1,		// Device
		TestMode            = 2,		// Device
		FunctionSuspend     = 0,		// Interface
		U1Enable            = 48,		// Device
		U2Enable            = 49,		// Device
		LTMEnable           = 50		// Device
	}

	[Flags]
	public enum PowerAttributes : byte
	{
		BusPowered   = 0x80,
		SelfPowered  = 0x40,
		RemoteWakeup = 0x20,
	}

	/// <summary>
	/// Constants relating to USB releases as found in the bcdUSB field of the Device Descriptor.
	/// </summary>
	public enum Release : ushort
	{
		_10       = 0x0100,
		_11       = 0x0110,
		_20       = 0x0200,
		_30       = 0x0300
	}

	/// <summary>
	/// Constants for HID requests.
	/// </summary>
	public enum HIDRquest : byte
	{
		GetReport     = 1,
		GetIdle       = 2,
		GetProtocol   = 3,
		SetReport     = 9,
		SetIdle       = 10,
		SetProtocol   = 11
	}

	/// <summary>
	/// Constants for the three kinds of HID reports.
	/// </summary>
	public enum HIDReport : byte
	{
		Input       = 1,
		Output      = 2,
		Feature     = 3
	}

	/// <summary>
	/// Used in the SET_PROTOCOL device request
	/// </summary>
	public enum HIDProtocol : byte
	{
		Boot   = 0,
		Report = 1
	}

	/// <summary>
	/// Device Class Codes
	/// </summary>
	/// <remarks>
	/// Constants for USB Device classes (bDeviceClass).
	/// </remarks>
	public enum DeviceClass : byte
	{
		Composite             = 0,
//		Comm                  = 2,	// Deprecated
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

	
	/// <summary>
	/// Interface Class
	/// </summary>
	/// <remarks>
	/// Constants for Interface classes (bInterfaceClass).
	/// </remarks>
	public enum InterfaceClass : ushort
	{
//		Audio                          = 1,	// Deprecated
		AudioInterface                 = 1,
		CommunicationControlInterface  = 2,
		CommunicationDataInterface     = 10,
		HID                            = 3,
		HIDInterface                   = 3,
		PhysicalInterfac               = 5,
		ImageInterface                 = 6,
//		Printing                       = 7,	// Deprecated
		PrintingInterface              = 7,
//		MassStorage                    = 8,	// Deprecated
		MassStorageInterface           = 8,
		ChipSmartCardInterface         = 11,
		ContentSecurityInterface       = 13,
		VideoInterface                 = 14,
		PersonalHealthcareInterface    = 15,
		DiagnosticDeviceInterface      = 220,
		WirelessControllerInterface    = 224,
		ApplicationSpecificInterface   = 254,
		VendorSpecificInterface        = 255
	}

	
	/// <summary>
	/// Interface SubClass
	/// </summary>
	/// <remarks>
	/// Constants for USB Interface SubClasses (bInterfaceSubClass).
	/// </remarks>
	public enum InterfaceSubClass : ushort
	{
		Composite               = 0,

		Hub                     = 0,

		// For the AudioInterfaceClass
		//
		AudioControl            = 0x01,
		AudioStreaming          = 0x02,
		MIDIStreaming           = 0x03,

		// For the ApplicationSpecificInterfaceClass
		//
		DFU                     = 0x01,
		IrDABridge              = 0x02,
		TestMeasurement         = 0x03,

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
		ATMNetworking           = 0x07,

		// For the DiagnosticDeviceInterfaceClass
		//
		ReprogrammableDiagnostic	= 0x01,

		// For the WirelessControllerInterfaceClass
		//
		RFController		= 0x01,

		// For the MiscellaneousClass
		//
		CommonClass		= 0x02,

		// For the VideoInterfaceClass
		//
		VideoControl             = 0x01,
		VideoStreaming           = 0x02,
		VideoInterfaceCollection = 0x03
	}

	/// <summary>
	/// Interface protocol.
	/// </summary>
	/// <remarks>
	/// Reported in the bInterfaceProtocol field of the Interface Descriptor.
	/// </remarks>
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

	/// <summary>
	/// USB device speed.
	/// </summary>
	public enum DeviceSpeed : byte
	{
		/// <summary>
		/// The device is a low speed device. (kUSBDeviceSpeedLow)
		/// </summary>
		Low		= 0,

		/// <summary>
		/// The device is a full speed device. (kUSBDeviceSpeedFull)
		/// </summary>
		Full	= 1,

		/// <summary>
		/// The device is a high speed device. (kUSBDeviceSpeedHigh)
		/// </summary>
		High	= 2,

		/// <summary>
		/// The device is a SuperSpeed device (kUSBDeviceSpeedSuper)
		/// </summary>
		Super	= 3
	}

	/// <summary>
	/// Device information flags. Used by <see cref="IOUSBDevice.DeviceInformation"/>
	/// </summary>
	[Flags]
	public enum DeviceInformation : uint
	{
		/// <summary>
		/// The USB device is directly attached to its hub and cannot be removed.
		/// </summary>
		IsCaptive               = (1 << 0),

		/// <summary>
		/// The USB device is directly attached to the root hub
		/// </summary>
		IsAttachedToRootHubMask	= (1 << 1),

		/// <summary>
		/// The USB device is internal to the enclosure (all the hubs it attaches to are captive)
		/// </summary>
		IsInternal              = (1 << 2),

		/// <summary>
		/// The USB device is connected to its hub
		/// </summary>
		IsConnected             = (1 << 3),

		/// <summary>
		/// The hub port to which the USB device is attached is enabled
		/// </summary>
		IsEnabled               = (1 << 4),

		/// <summary>
		/// The hub port to which the USB device is attached is suspended
		/// </summary>
		IsSuspended             = (1 << 5),

		/// <summary>
		/// The hub port to which the USB device is attached is being reset
		/// </summary>
		IsInReset               = (1 << 6),

		/// <summary>
		/// The USB device generated an overcurrent
		/// </summary>
		Overcurrent             = (1 << 7),

		/// <summary>
		/// The hub port to which the USB device is attached is in test mode
		/// </summary>
		PortIsInTestMode        = (1 << 8),

		/// <summary>
		/// The device is the root hub simulation
		/// </summary>
		IsRootHub               = (1 << 9),

		/// <summary>
		/// If this is a root hub simulation and it's built into the enclosure, this bit is set.
		/// If it's on an expansion card, it will be cleared
		/// </summary>
		RootHubIsBuiltIn        = (1 << 10),

		/// <summary>
		/// This device is "attached" to the controller through a remote connection
		/// </summary>
		IsRemote                = (1 << 11),

		/// <summary>
		/// The hub port to which the USB device is connected has a USB connector on the enclosure
		/// </summary>
		IsAttachedToEnclosure   = (1 << 12),

		/// <summary>
		/// The USB device is downstream of a controller that is attached through Thunderbolt
		/// </summary>
		IsOnThunderbolt         = (1 << 13)
	}

	/// <summary>
	/// Used to specify what kind of power will be reserved using the IOUSBDevice RequestExtraPower and ReturnExtraPower APIs. 
	/// </summary>
	public enum PowerRequestType : byte
	{
		/// <summary>
		/// The power is to be used during sleep.
		/// </summary>
		kUSBPowerDuringSleep            = 0,

		/// <summary>
		/// The power is to be used while the system is awake (i.e not sleeping)
		/// </summary>
		kUSBPowerDuringWake             = 1,

		/// <summary>
		/// When used with ReturnExtraPower(), it will send a message to all devices to return any extra wake power if possible.
		/// </summary>
		kUSBPowerRequestWakeRelease     = 2,

		/// <summary>
		/// When used with ReturnExtraPower(), it will send a message to all devices to return any sleep power if possible.
		/// </summary>
		kUSBPowerRequestSleepRelease    = 3,

		/// <summary>
		/// When used with ReturnExtraPower(), it will send a message to all devices indicating that they can ask for more wake power, as some device has released it.
		/// </summary>
		kUSBPowerRequestWakeReallocate  = 4,

		/// <summary>
		/// When used with ReturnExtraPower(), it will send a message to all devices indicating that they can ask for more sleep power, as some device has released it.
		/// </summary>
		kUSBPowerRequestSleepReallocate = 5,

		/// <summary>
		/// The power is to be used while the system is awake (i.e not sleeping), but can be taken away (via the kUSBPowerRequestWakeRelease message).
		/// The system can then allocate that extra power to another device.
		/// </summary>
		kUSBPowerDuringWakeRevocable    = 6,

		/// <summary>
		/// This is used by the USB stack to allocate the 400mA extra for USB3, above the 500ma allocated by USB2
		/// </summary>
		kUSBPowerDuringWakeUSB3         = 7
	}

	public enum Interval : byte
	{
		_1 = 1,
		_2 = 2,
		_4 = 4,
		_8 = 8,
		_16 = 16,
		_32 = 32,
		_64 = 64,
		_128 = 128
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

