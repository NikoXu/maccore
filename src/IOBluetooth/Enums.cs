//
// Enums.cs
//
// Author:
//     David Lechner <david@lechnology.com>
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

namespace MonoMac.IOBluetooth
{
	public enum BluetoothCompanyIdentifers
	{
		EricssonTechnologyLicensing                       = 0,
		NokiaMobilePhones                                 = 1,
		Intel                                             = 2,
		IBM                                               = 3,
		Toshiba                                           = 4,
		_3Com                                             = 5,
		Microsoft                                         = 6,
		Lucent                                            = 7,
		Motorola                                          = 8,
		InfineonTechnologiesAG                            = 9,
		CambridgeSiliconRadio                             = 10,
		SiliconWave                                       = 11,
		DigianswerAS                                      = 12,
		TexasInstruments                                  = 13,
		ParthusTechnologies                               = 14,
		Broadcom                                          = 15,
		MitelSemiconductor                                = 16,
		Widcomm                                           = 17,
		Zeevo                                             = 18,
		Atmel                                             = 19,
		MistubishiElectric                                = 20,
		RTXTelecom                                        = 21,
		KCTechnology                                      = 22,
		Newlogic                                          = 23,
		Transilica                                        = 24,
		RohdeandSchwarz                                   = 25,
		TTPCom                                            = 26,
		SigniaTechnologies                                = 27,
		ConexantSystems                                   = 28,
		Qualcomm                                          = 29,
		Inventel                                          = 30,
		AVMBerlin                                         = 31,
		Bandspeed                                         = 32,
		Mansella                                          = 33,
		NEC                                               = 34,
		WavePlusTechnology                                = 35,
		Alcatel                                           = 36,
		PhilipsSemiconductor                              = 37,
		CTechnologies                                     = 38,
		OpenInterface                                     = 39,
		RFCMicroDevices                                   = 40,
		Hitachi                                           = 41,
		SymbolTechnologies                                = 42,
		Tenovis                                           = 43,
		MacronixInternational                             = 44,
		GCTSemiconductor                                  = 45,
		NorwoodSystems                                    = 46,
		MewTelTechnology                                  = 47,
		STMicroelectronics                                = 48,
		Synopsys                                          = 49,
		RedMCommunications                                = 50,
		Commil                                            = 51,
		CATC                                              = 52,
		Eclipse                                           = 53,
		RenesasTechnology                                 = 54,
		Mobilian                                          = 55,
		Terax                                             = 56,
		IntegratedSystemSolution                          = 57,
		MatsushitaElectricIndustrial                      = 58,
		Gennum                                            = 59,
		ResearchInMotion                                  = 60,
		IPextreme                                         = 61,
		SystemsAndChips                                   = 62,
		BluetoothSIG                                      = 63,
		SeikoEpson                                        = 64,
		IntegratedSiliconSolution                         = 65,
		CONWISETechnology                                 = 66,
		ParrotSA                                          = 67,
		SocketCommunications                              = 68,
		AtherosCommunications                             = 69,
		MediaTek                                          = 70,
		Bluegiga                                          = 71,
		MarvellTechnologyGroup                            = 72,
		_3DSP                                             = 73,
		AccelSemiconductor                                = 74,
		ContinentialAutomotiveSystems                     = 75,
		Apple                                             = 76,
		StaccatoCommunications                            = 77,
		AvagoTechnologies                                 = 78,
		APT                                               = 79,
		SiRFTechnology                                    = 80,
		TZeroTechnologies                                 = 81,
		JandM                                             = 82,
		Free2Move                                         = 83,
		_3DiJoy                                           = 84,
		Plantronics                                       = 85,
		SonyEricssonMobileCommunications                  = 86,
		HarmonInternational                               = 87,
		Visio                                             = 88,
		NordicSemiconductor                               = 89,
		EMMicroElectronicMarin                            = 90,

		InteropIdentifier                                 = 65535
	}

	[Flags]
	public enum BluetoothServiceClassMajor
	{
		LimitedDiscoverableMode      = 0x001,	// Bit 13 - Limited Discoverable Mode
		Reserved1                    = 0x002, 	// Bit 14 - Reserved for future use.
		Reserved2                    = 0x004, 	// Bit 15 - Reserved for future use.
		Positioning                  = 0x008, 	// Bit 16 - Positioning (Location ID)
		Networking                   = 0x010, 	// Bit 17 - LAN, Ad hoc, etc...
		Rendering                    = 0x020, 	// Bit 18 - Printing, Speaker, etc...
		Capturing                    = 0x040,	// Bit 19 - Scanner, Microphone, etc...
		ObjectTransfer               = 0x080,	// Bit 20 - v-Inbox, v-Folder, etc...
		Audio                        = 0x100,	// Bit 21 - Speaker, Microphone, Headset, etc...
		Telephony                    = 0x200,	// Bit 22 - Cordless telephony, Modem, Headset, etc...
		Information                  = 0x400,	// Bit 23 - Web server, WAP server, etc...

		Any                          = 0x2A2A2A2A, //'****'	// Pseudo-class - means anything acceptable.
		None                         = 0x6E6F6E65, //'none'	// Pseudo-class - means no matching.
	}

	public enum BluetoothDeviceClassMajor
	{
		Miscellaneous                 = 0x00, // [00000] Miscellaneous
		Computer                      = 0x01, // [00001] Desktop, Notebook, PDA, Organizers, etc...
		Phone                         = 0x02, // [00010] Cellular, Cordless, Payphone, Modem, etc...
		LANAccessPoint                = 0x03, // [00011] LAN Access Point
		Audio                         = 0x04, // [00100] Headset, Speaker, Stereo, etc...
		Peripheral                    = 0x05, // [00101] Mouse, Joystick, Keyboards, etc...
		Imaging                       = 0x06, // [00110] Printing, scanner, camera, display, etc...
		Wearable                      = 0x07, // [00111] Wearable
		Toy                           = 0x08, // [01000] Toy
		Health                        = 0x09, // [01001] Health devices
		Unclassified                  = 0x1F, // [11111] Specific device code not assigned

		// Range 0x06 to 0x1E Reserved for future use.

		Any                           = 0x2A2A2A2A, //'****'	// Pseudo-class - means anything acceptable.
		None                          = 0x6E6F6E65, //'none'	// Pseudo-class - means no matching.
	}

	public enum BluetoothDeviceClassMinor
	{
		///
		/// Computer Major Class
		///

		ComputerUnclassified          = 0x00, 	// [000000] Specific device code not assigned
		ComputerDesktopWorkstation    = 0x01, 	// [000001] Desktop workstation
		ComputerServer                = 0x02, 	// [000010] Server-class computer
		ComputerLaptop                = 0x03, 	// [000011] Laptop
		ComputerHandheld              = 0x04, 	// [000100] Handheld PC/PDA (clam shell)
		ComputerPalmSized             = 0x05, 	// [000101] Palm-sized PC/PDA
		ComputerWearable              = 0x06,        // [000110] Wearable computer (watch sized)

		// Range 0x06 to 0x7F Reserved for future use.


		///
		/// Phone Major Class
		///

		PhoneUnclassified              = 0x00, 	// [000000] Specific device code not assigned
		PhoneCellular                  = 0x01, 	// [000001] Cellular
		PhoneCordless                  = 0x02, 	// [000010] Cordless
		PhoneSmartPhone                = 0x03, 	// [000011] Smart phone
		PhoneWiredModemOrVoiceGateway  = 0x04, 	// [000100] Wired modem or voice gateway
		PhoneCommonISDNAccess          = 0x05,        // [000101] Common ISDN Access

		// Range 0x05 to 0x7F Reserved for future use.


		///
		/// LAN Access Point Major Class
		///

		// See the Bluetooth specification for LAN Access Point minor classes, which are broken into bits 5-7 for utilization and bits 2-4 for class.


		///
		/// Audio Minor Class
		///

		AudioUnclassified                 = 0x00, 	// [000000] Specific device code not assigned
		AudioHeadset                      = 0x01, 	// [000001] Device conforms to the Headset profile
		AudioHandsFree                    = 0x02,        // [000010] Hands-free
		AudioReserved1                    = 0x03,        // [000011] Reserved
		AudioMicrophone                   = 0x04,        // [000100] Microphone
		AudioLoudspeaker                  = 0x05,        // [000101] Loudspeaker
		AudioHeadphones                   = 0x06,        // [000110] Headphones
		AudioPortable                     = 0x07,        // [000111] Portable Audio
		AudioCar                          = 0x08,        // [001000] Car Audio
		AudioSetTopBox                    = 0x09,        // [001001] Set-top box
		AudioHiFi                         = 0x0a,        // [001010] HiFi Audio Device
		AudioVCR                          = 0x0b,        // [001011] VCR
		AudioVideoCamera                  = 0x0c,        // [001100] Video Camera
		AudioCamcorder                    = 0x0d,        // [001101] Camcorder
		AudioVideoMonitor                 = 0x0e,        // [001110] Video Monitor
		AudioVideoDisplayAndLoudspeaker   = 0x0f,        // [001111] Video Display and Loudspeaker
		AudioVideoConferencing            = 0x10,        // [010000] Video Conferencing
		AudioReserved2                    = 0x11,        // [010001] Reserved
		AudioGamingToy                    = 0x12,        // [010010] Gaming/Toy


		// Range 0x13 to 0x7F Reserved for future use.


		///
		/// Peripheral Minor Class
		///

		// Peripheral1 subclass is bits 7 & 6

		Peripheral1Keyboard            = 0x10,        // [01XXXX] Keyboard
		Peripheral1Pointing            = 0x20,        // [10XXXX] Pointing device
		Peripheral1Combo               = 0x30,        // [11XXXX] Combo keyboard/pointing device

		// Peripheral2 subclass is bits 5-2

		Peripheral2Unclassified        = 0x00,        // [XX0000] Uncategorized device
		Peripheral2Joystick            = 0x01,        // [XX0001] Joystick
		Peripheral2Gamepad             = 0x02,        // [XX0010] Gamepad
		Peripheral2RemoteControl       = 0x03,        // [XX0011] Remote control
		Peripheral2SensingDevice       = 0x04,        // [XX0100] Sensing device
		Peripheral2DigitizerTablet     = 0x05,        // [XX0101] Digitizer Tablet	
		Peripheral2CardReader          = 0x06,        // [XX0110] Card Reader
		Peripheral2DigitalPen          = 0x07,         // [XX0111] Digital Pen
		Peripheral2HandheldScanner     = 0x08,         // [XX1000] Handheld scanner for bar-codes, RFID, etc.
		Peripheral2GesturalInputDevice = 0x09,        // [XX1001] Handheld gestural input device (e.g., "wand" form factor)

		Peripheral2AnyPointing         = 0x706F696E, //'poin'	// Anything under MinorPeripheral1Pointing

		// Range 0x05 to 0x0f reserved for future use


		///
		/// Imaging Minor Class
		///

		// Imaging1 subclass is bits 7 - 4

		Imaging1Display               = 0x04,        // [XXX1XX] Display
		Imaging1Camera                = 0x08,        // [XX1XXX] Camera
		Imaging1Scanner               = 0x10,        // [X1XXXX] Scanner
		Imaging1Printer               = 0x20,        // [1XXXXX] Printer

		// Imaging2 subclass is bits 3 - 2

		Imaging2Unclassified          = 0x00,        // [XXXX00] Uncategorized, default

		// Range 0x01 - 0x03 reserved for future use

		///
		/// Wearable Minor Class
		///

		WearableWristWatch            = 0x01,        // [000001] Watch
		WearablePager                 = 0x02,        // [000010] Pager
		WearableJacket                = 0x03,        // [000011] Jacket
		WearableHelmet                = 0x04,        // [000100] Helmet
		WearableGlasses               = 0x05,        // [000101] Glasses

		///
		/// Toy Minor Class
		///

		ToyRobot                      = 0x01,        // [000001] Robot
		ToyVehicle                    = 0x02,        // [000010] Vehicle
		ToyDollActionFigure           = 0x03,        // [000011] Doll / Action Figure
		ToyController                 = 0x04,        // [000100] Controller
		ToyGame                       = 0x05,        // [000101] Game

		///
		/// Health Minor Class
		///

		HealthUndefined               = 0x00,        // [000000] Undefined
		HealthBloodPressureMonitor    = 0x01,        // [000001] Blood Pressure Monitor
		HealthThermometer             = 0x02,        // [000010] Thermometer
		HealthScale                   = 0x03,        // [000011] Scale
		HealthGlucoseMeter            = 0x04,        // [000100] Glucose Meter
		HealthPulseOximeter           = 0x05,        // [000101] Pulse Oximeter
		HealthHeartRateMonitor        = 0x06,        // [000111] Heart Rate Monitor
		HealthDataDisplay             = 0x07,        // [001000] Display

		///
		///	Misc
		///

		Any                           = 0x2A2A2A2A, //'****'	// Pseudo-class - means anything acceptable.
		None                          = 0x6E6F6E65, //'none'	// Pseudo-class - means no matching.
	}

	public static class DeviceClassMinorExtension
	{
		public static BluetoothDeviceClassMinor GetPeripheral1 (this BluetoothDeviceClassMinor minorClass)
		{
			return (BluetoothDeviceClassMinor)((uint)minorClass & 0x30);
		}

		public static BluetoothDeviceClassMinor GetPeripheral2 (this BluetoothDeviceClassMinor minorClass)
		{
			return (BluetoothDeviceClassMinor)((uint)minorClass & 0x0F);
		}
	}

	public enum BluetoothL2CAPPSM
	{
		SDP                     = 0x0001, 
		RFCOMM                  = 0x0003, 
		TCS_BIN                 = 0x0005,	// Telephony Control Specifictation / TCS Binary
		TCS_BIN_Cordless        = 0x0007,	// Telephony Control Specifictation / TCS Binary
		BNEP                    = 0x000F,	// Bluetooth Network Encapsulation Protocol
		HIDControl              = 0x0011,	// HID profile - control interface
		HIDInterrupt            = 0x0013,	// HID profile - interrupt interface
		AVCTP                   = 0x0017,	// Audio/Video Control Transport Protocol
		AVDTP                   = 0x0019,	// Audio/Video Distribution Transport Protocol
		UID_C_Plane             = 0x001D,	// Unrestricted Digital Information Profile (UDI)
		ATT                     = 0x001F,	// Attribute Protocol

		// Range < 0x1000 reserved.
		ReservedStart           = 0x0001,
		ReservedEnd             = 0x1000,

		// Range 0x1001-0xFFFF dynamically assigned.
		DynamicStart            = 0x1001,
		D2D                     = 0x100F,
		DynamicEnd              = 0xFFFF,

		None                    = 0x0000
	}

	public enum ServiceDiscoveryProtocol
	{
		// General

		Base                                 = 0x0000, 	// 00000000-0000-1000-8000-00805f9b34fb

		// Protocols

		SDP                                  = 0x0001, 	// 00000001-0000-1000-8000-00805f9b34fb
		UDP                                  = 0x0002, 	// 00000002-0000-1000-8000-00805f9b34fb 
		RFCOMM                               = 0x0003, 	// 00000003-0000-1000-8000-00805f9b34fb 
		TCP                                  = 0x0004, 	// 00000004-0000-1000-8000-00805f9b34fb 
		TCSBIN                               = 0x0005, 	// 00000005-0000-1000-8000-00805f9b34fb 
		TCSAT                                = 0x0006, 	// 00000006-0000-1000-8000-00805f9b34fb 
		OBEX                                 = 0x0008, 	// 00000008-0000-1000-8000-00805f9b34fb 
		IP                                   = 0x0009, 	// 00000009-0000-1000-8000-00805f9b34fb 
		FTP                                  = 0x000A, 	// 0000000A-0000-1000-8000-00805f9b34fb 
		HTTP                                 = 0x000C, 	// 0000000C-0000-1000-8000-00805f9b34fb 
		WSP	                                 = 0x000E, 	// 0000000E-0000-1000-8000-00805f9b34fb 
		BNEP                                 = 0x000F,
		UPNP                                 = 0x0010,
		HIDP                                 = 0x0011,
		HardcopyControlChannel               = 0x0012,
		HardcopyDataChannel                  = 0x0014,
		HardcopyNotification                 = 0x0016,
		AVCTP                                = 0x0017,
		AVDTP                                = 0x0019,
		CMPT                                 = 0x001B,
		UDI_C_Plane                          = 0x001D,
		L2CAP                                = 0x0100, 	// 00000100-0000-1000-8000-00805f9b34fb 
	}

#if !COREBUILD
	public static class ServiceDiscoveryProtocolExension
	{
		public static IOBluetoothSDPUUID ToUUID (this ServiceDiscoveryProtocol value)
		{
			return new IOBluetoothSDPUUID ((ushort)value);
		}
	}
#endif

	public enum SDPServiceClasses
	{
		ServiceDiscoveryServer                   = 0x1000,	// 00001000-0000-1000-8000-00805f9b34fb
		BrowseGroupDescriptor                    = 0x1001,	// 00001001-0000-1000-8000-00805f9b34fb
		PublicBrowseGroup                        = 0x1002,	// 00001002-0000-1000-8000-00805f9b34fb
		SerialPort                               = 0x1101,	// 00001101-0000-1000-8000-00805f9b34fb
		LANAccessUsingPPP                        = 0x1102,	// 00001102-0000-1000-8000-00805f9b34fb
		DialupNetworking                         = 0x1103,	// 00001103-0000-1000-8000-00805f9b34fb
		IrMCSync                                 = 0x1104,	// 00001104-0000-1000-8000-00805f9b34fb
		OBEXObjectPush                           = 0x1105,	// 00001105-0000-1000-8000-00805f9b34fb
		OBEXFileTransfer                         = 0x1106,	// 00001106-0000-1000-8000-00805f9b34fb
		IrMCSyncCommand                          = 0x1107,	// 00001107-0000-1000-8000-00805f9b34fb
		Headset                                  = 0x1108,	// 00001108-0000-1000-8000-00805f9b34fb
		CordlessTelephony                        = 0x1109,	// 00001109-0000-1000-8000-00805f9b34fb
		AudioSource                              = 0x110A,
		AudioSink                                = 0x110B,
		AVRemoteControlTarget                    = 0x110C,
		AdvancedAudioDistribution                = 0x110D,
		AVRemoteControl                          = 0x110E,
		VideoConferencing                        = 0x110F,
		Intercom                                 = 0x1110,	// 00001110-0000-1000-8000-00805f9b34fb
		Fax                                      = 0x1111,	// 00001111-0000-1000-8000-00805f9b34fb
		HeadsetAudioGateway                      = 0x1112,	// 00001112-0000-1000-8000-00805f9b34fb
		WAP                                      = 0x1113,
		WAPClient                                = 0x1114,
		PANU                                     = 0x1115,
		NAP                                      = 0x1116,
		GN                                       = 0x1117,
		DirectPrinting                           = 0x1118,
		ReferencePrinting                        = 0x1119,
		Imaging                                  = 0x111A,
		ImagingResponder                         = 0x111B,
		ImagingAutomaticArchive                  = 0x111C,
		ImagingReferencedObjects                 = 0x111D,
		HandsFree                                = 0x111E,
		HandsFreeAudioGateway                    = 0x111F,
		DirectPrintingReferenceObjectsService    = 0x1120,
		ReflectedUI                              = 0x1121,
		BasicPrinting                            = 0x1122,
		PrintingStatus                           = 0x1123,
		HumanInterfaceDeviceService              = 0x1124,
		HardcopyCableReplacement                 = 0x1125,
		HCR_Print                                = 0x1126,
		HCR_Scan                                 = 0x1127,
		CommonISDNAccess                         = 0x1128,
		VideoConferencingGW                      = 0x1129,
		UDI_MT                                   = 0x112A,
		UDI_TA                                   = 0x112B,
		AudioVideo                               = 0x112C,
		SIM_Access                               = 0x112D,
		PhonebookAccess_PCE                      = 0x112E,
		PhonebookAccess_PSE                      = 0x112F,
		PhonebookAccess                          = 0x1130,
		Headset_HS                               = 0x1131,
		MessageAccessServer                      = 0x1132,
		MessageNotificationServer                = 0x1133,
		MessageAccessProfile                     = 0x1134,
		PnPInformation                           = 0x1200,	// 00001200-0000-1000-8000-00805f9b34fb
		GenericNetworking                        = 0x1201,	// 00001201-0000-1000-8000-00805f9b34fb
		GenericFileTransfer                      = 0x1202,	// 00001202-0000-1000-8000-00805f9b34fb
		GenericAudio                             = 0x1203,	// 00001203-0000-1000-8000-00805f9b34fb
		GenericTelephony                         = 0x1204	// 00001204-0000-1000-8000-00805f9b34fb
	}

	public enum SDPAttributeIdentifierCodes
	{
		ServiceRecordHandle                 = 0x0000,
		ServiceClassIDList                  = 0x0001,
		ServiceRecordState                  = 0x0002,
		ServiceID                           = 0x0003,
		ProtocolDescriptorList              = 0x0004,
		BrowseGroupList                     = 0x0005,
		LanguageBaseAttributeIDList         = 0x0006,
		ServiceInfoTimeToLive               = 0x0007,
		ServiceAvailability                 = 0x0008,
		BluetoothProfileDescriptorList      = 0x0009,
		DocumentationURL                    = 0x000A,
		ClientExecutableURL                 = 0x000B,
		IconURL                             = 0x000C,
		AdditionalProtocolsDescriptorList   = 0x000D,

		// Service Discovery Server
		VersionNumberList                   = 0x0200,
		ServiceDatabaseState                = 0x0201,

		// Browse Group Descriptor
		GroupID                             = 0x0200,

		// PAN
		IPSubnet                            = 0x0200,

		// HID                                                                            Required        Type        Notes
		HIDReleaseNumber                    = 0x0200,	// O            uint16
		HIDParserVersion                    = 0x0201,	// M            uint16
		HIDDeviceSubclass                   = 0x0202,	// M            uint8             Should match the low order 8 bits of CoD unless a combo device
		HIDCountryCode                      = 0x0203,	// M            uint8
		HIDVirtualCable                     = 0x0204,	// M            bool
		HIDReconnectInitiate                = 0x0205,	// M            bool
		HIDDescriptorList                   = 0x0206,	// M            sequence        Layout is defined in Section 6.2 of the HID Specification
		HIDLangIDBaseList                   = 0x0207,	// M            sequence
		HIDSDPDisable                       = 0x0208,	// O            bool
		HIDBatteryPower                     = 0x0209,	// O            bool
		HIDRemoteWake                       = 0x020A,	// O            bool
		HIDProfileVersion                   = 0x020B,	// M            uint16
		HIDSupervisionTimeout               = 0x020C,	// O            uint16        Default to 5 seconds or longer if not defined
		HIDNormallyConnectable              = 0x020D,	// O            bool
		HIDBootDevice                       = 0x020E,	// M            bool        Only defined for mice and keyboards as of HID 1.0
		HIDSSRHostMaxLatency                = 0x020F,	// O            uint16
		HIDSSRHostMinTimeout                = 0x0210,	// O            uint16

		ServiceVersion                      = 0x0300,
		ExternalNetwork                     = 0x0301,	// Cordless telephony
		Network                             = 0x0301,	// Handsfree Profile (HFP)
		SupportedDataStoresList             = 0x0301,	// Sync Profile
		FaxClass1Support                    = 0x0302,	// Fax Profile
		RemoteAudioVolumeControl            = 0x0302,	// GAP???
		FaxClass2_0Support                  = 0x0303,
		SupporterFormatsList                = 0x0303,
		FaxClass2Support                    = 0x0304,
		AudioFeedbackSupport                = 0x0305,
		NetworkAddress                      = 0x0306,	// WAP
		WAPGateway                          = 0x0307,	// WAP
		HomepageURL                         = 0x0308,	// WAP
		WAPStackType                        = 0x0309,	// WAP
		SecurityDescription                 = 0x030A,	// PAN
		NetAccessType                       = 0x030B,	// PAN
		MaxNetAccessRate                    = 0x030C,	// PAN
		SupportedCapabilities               = 0x0310,	// Imaging
		SupportedFeatures                   = 0x0311,	// Imaging & HFP
		SupportedFunctions                  = 0x0312,	// Imaging
		TotalImagingDataCapacity            = 0x0313,	// Imaging

		ServiceName                         = 0x0000, /* +language base offset*/
		ServiceDescription                  = 0x0001, /* +language base offset*/
		ProviderName                        = 0x0002    /* +language base offset*/
	}

	public enum SDPAttributeDeviceIdentificationRecord
	{
		// DeviceID v1.3                                                                Required        Type        Section        Notes
		ServiceDescription            = 0x0001,	// O            string        -
		DocumentationURL              = 0x000A,	// O            url            -
		ClientExecutableURL           = 0x000B,	// O            url            -
		SpecificationID               = 0x0200,	// M            uint16        5.1
		VendorID                      = 0x0201,	// M            uint16        5.2
		ProductID                     = 0x0202,	// M            uint16        5.3
		Version                       = 0x0203,	// M            uint16        5.4
		PrimaryRecord                 = 0x0204,	// M            bool        5.5
		VendorIDSource                = 0x0205,	// M            uint16        5.6
		ReservedRangeStart            = 0x0206,
		ReservedRangeEnd              = 0x02FF
	}

	public enum ProtocolParameters
	{
		L2CAPPSM                               = 1,
		RFCOMMChannel                          = 1,
		TCPPort                                = 1,
		UDPPort                                = 1,
		BNEPVersion                            = 1,
		BNEPSupportedNetworkPacketTypeList     = 2
	}

	//Inquiries with 'Extended Inquiry Response' (v2.1 specification)

	public enum BluetoothHCIExtendedInquiryResponseDataTypes
	{
		Flags                                        = 0x01,
		_16BitServiceClassUUIDsWithMoreAvailable     = 0x02,
		_16BitServiceClassUUIDsCompleteList          = 0x03,
		_32BitServiceClassUUIDsWithMoreAvailable     = 0x04,
		_32BitServiceClassUUIDsCompleteList          = 0x05,
		_128BitServiceClassUUIDsWithMoreAvailable    = 0x06,
		_128BitServiceClassUUIDsCompleteList         = 0x07,
		ShortenedLocalName                           = 0x08,
		CompleteLocalName                            = 0x09,
		TransmitPowerLevel                           = 0x0A,
		SSPOOBClassOfDevice                          = 0x0D,
		SSPOOBSimplePairingHashC                     = 0x0E,
		SSPOOBSimplePairingRandomizerR               = 0x0F,
		DeviceID                                     = 0x10,
		SecurityManagerTKValue                       = 0x10,
		SecurityManagerOOBFlags                      = 0x11,
		SlaveConnectionIntervalRange                 = 0x12,
		ServiceSolicitation16BitUUIDs                = 0x14,
		ServiceSolicitation128BitUUIDs               = 0x15,
		ServiceData                                  = 0x16,
		ManufacturerSpecificData                     = 0xFF
	}


	// HCI Versions

	public enum BluetoothHCIVersions
	{
		V1_0b                                               = 0x00,
		V1_1                                                = 0x01,
		V1_2                                                = 0x02,
		V2_0EDR                                             = 0x03,
		V2_1EDR                                             = 0x04,
		V3_0HS                                              = 0x05,
		V4_0                                                = 0x06
	}


	// LMP Versions

	public enum BluetoothLMPVersions
	{
		V1_0b                                               = 0x00,
		V1_1                                                = 0x01,
		V1_2                                                = 0x02,
		V2_0EDR                                             = 0x03,
		V2_1EDR                                             = 0x04,
		V3_0HS                                              = 0x05,
		V4_0                                                = 0x06
	}

	public enum BluetoothPageScanRepetitionMode : byte
	{
		R0  = 0x00,
		R1  = 0x01,
		R2  = 0x02

		// All other values are reserved for future use.
	}

	public enum BluetoothPageScanPeriodMode : byte
	{
		P0  = 0x00,
		P1  = 0x01,
		P2  = 0x02

		// All other values are reserved for future use.
	}

	public enum BluetoothPageScanMode : byte
	{
		Mandatory  = 0x00,
		Optional1  = 0x01,
		Optional2  = 0x02,
		Optional3  = 0x03

		// All other values are reserved for future use.
	}

	public enum BluetoothLinkType : byte
	{
		SCOConnection  = 0,
		ACLConnection  = 1,
		ESCOConnection = 2,
		LinkTypeNone   = 0xff
	}

	public enum BluetoothHCIEncryptionMode : byte
	{
		Disabled                               = 0x00,		// Default.
		OnlyForPointToPointPackets             = 0x01,
		ForBothPointToPointAndBroadcastPackets = 0x02,
	}

	/// <summary>
	/// Bits to determine what Bluetooth devices to search for
	/// </summary>
	[Flags]
	public enum IOBluetoothDeviceSearchTypes : uint
	{
		Classic = 1,
	}

	public enum IOBluetoothUserNotificationChannelDirection
	{
		Any						= 0,
		Incoming				= 1,
		Outgoing				= 2,
	}

	/// <summary>
	/// Channel Identifiers (Bluetooth L2CAP spec section 2.1).
	/// </summary>
	public enum BluetoothL2CAPChannelID : ushort
	{
		Null               = 0x0000, 	// Illegal, should not be used
		Signalling         = 0x0001, 	// L2CAP signalling channel
		ConnectionLessData = 0x0002, 	// L2CAP connection less data
		AMPManagerProtocol = 0x0003,	// AMP Manager Protocol
		AttributeProtocol  = 0x0004,
		LESignalling       = 0x0005,
		SecurityManager    = 0x0006,
		// Range 0x0003 to 0x003F reserved for future use.
		ReservedStart      = 0x0007,
		ReservedEnd        = 0x003F,

		// Range 0x0040 to 0xFFFF are dynamically allocated.
		DynamicStart       = 0x0040,
		DynamicEnd         = 0xffff,
		End                = 0xffff
	}

	public enum IOBluetoothSMSMode : uint
	{
		Phase2SMSSupport              = 1 << 0,
		Phase2pSMSSupport             = 1 << 1,
		ManufactureSpecificSMSSupport = 1 << 2
	}

	public enum BluetoothHCIPowerState
	{
		On           = 0x01,
		Off          = 0x00,
		Unintialized = 0xFF,
	}

	public enum BluetoothConnectionHandle : ushort
	{
		None = 0xffff
	}

	/// <remarks>
	/// Pass these types in the OBEXTransportEvent, and then pass the struct on to the session object once you
	/// have filled it out. This is how you can communicate with the session when events happen - if data is
	/// received, the type will be 'kOBEXTransportEventTypeDataReceived'. if an error has occurred on your transport,
	/// like the remote target died, you can send a status event with a non-zero value. Since session objects will
	/// receive this status code on their event handlers, you should try to pass a useful status/error code, such as
	/// an IOReturn value.
	/// </remarks>
	public enum OBEXTransportEventType : uint
	{
		DataReceived = 0x44617441, //'DatA',
		Status       = 0x53746154, //'StaT'
	}

	/// <summary>
	/// Codes for OBEX errors. If the return value was not in the following range, then it is most likely resulting
	/// from kernel code/IOKit, and you should consult IOReturn.h for those codes.
	/// </summary>
	public enum OBEXError : int
	{
		/// <summary>
		/// Minimum value in OBEX error range.
		/// </summary>
		ErrorRangeMin					= -21850,

		/// <summary>
		/// Maximum value in OBEX error range.
		/// </summary>
		ErrorRangeMax					= -21899,
		
		Success                      = 0,			// Success at whatever you were attempting.
		GeneralError                 = -21850,		// Catch all for misc. errors.
		NoResourcesError             = -21851,		// An allocation failed, etc.
		UnsupportedError             = -21852,		// operation or item is not supported.
		InternalError                = -21853,		// A problem has occurred in our internal code.
		BadArgumentError             = -21854,		// A bad argument was passed to an OBEX function.
		TimeoutError                 = -21855,		// timeout error
		BadRequestError              = -21856,		// bad request error
		CancelledError               = -21857,
		ForbiddenError               = -21858,		// operation was not allowed on remote device (wrong permissions, etc.).
		
		UnauthorizedError            = -21859,		// Unauthorized
		NotAcceptableError           = -21860,		// Not Acceptable
		ConflictError                = -21861,		// Conflict
		MethodNotAllowedError        = -21862,		// Method not allowed
		NotFoundError                = -21863,		// File/Folder not found
		NotImplementedError          = -21864,		// Not Implemented
		PreconditionFailedError      = -21865,		// Precondition failed
			
		SessionBusyError             = -21875,		// Session is busy with a command already.
		SessionNotConnectedError     = -21876,		// Session does not have an open connection.
		SessionBadRequestError       = -21877,		// Whatever you are trying to do is invalid (trying to send more data than the max packet size supports, e.g.).
		SessionBadResponseError      = -21878,		// The OBEX Server/client you are talking to has sent us a bad response (e.g. when a Connect Command was sent, we got back "0xA0", which is not correct).
		SessionNoTransportError      = -21879,		// The underlying transport (Bluetooth, etc.) is not open/available.
		SessionTransportDiedError    = -21880,		// The underlying transport connection (Bluetooth, etc.) died.
		SessionTimeoutError          = -21881,		// Timeout occurred performing an operation.
		SessionAlreadyConnectedError = -21882		// Connection over OBEX already established (returned from OBEXConnect).
	}

	/// <summary>
	/// When a new session event occurs, your selector (or C callback) will be given an OBEXSessionEvent pointer,
	/// and in it will be a 'type' field with one of the following types in it. Based on that type, you can then
	/// read the corresponding field in the union to get out interesting data for that event type. For example,
	/// if the type of an event is a 'ConnectCommandResponseReceived', you should look in
	/// the 'OBEXConnectCommandResponseData' part of the structure's union to find more information pased to you
	/// in the event. Note that some you will never see, depending on the type of session you are using - a client
	/// or server. If you are a client (most likely case), you will never see the "Command" events, but instead
	/// you will only receive the "CommandResponse" events since you will be the issuer oft he commands, not the
	/// receiver of them. Both types of sessions will receive error type events.
	/// </summary>
	public enum OBEXSessionEventType : uint
	{
		// Client event types.

		ConnectCommandResponseReceived		= 0x4F434543, //'OCEC'
		DisconnectCommandResponseReceived 	= 0x4F434544, //'OCED'
		PutCommandResponseReceived			= 0x4F434550, //'OCEP'
		GetCommandResponseReceived			= 0x4F434547, //'OCEG'
		SetPathCommandResponseReceived		= 0x4F434553, //'OCES'
		AbortCommandResponseReceived		= 0x4F434541, //'OCEA'

		// Server event types.
		
		ConnectCommandReceived				= 0x4F534543, //'OSEC'
		DisconnectCommandReceived 			= 0x4F534544, //'OSED'
		PutCommandReceived					= 0x4F534550, //'OSEP'
		GetCommandReceived					= 0x4F534547, //'OSEG'
		SetPathCommandReceived				= 0x4F534553, //'OSES'
		AbortCommandReceived				= 0x4F534541, //'OSEA'
		
		// Shared (Server/client) event types.
		
		Error								= 0x4F474545, //'OGEE'
	}

	/// <summary>
	/// The available/supported OBEX versions.
	/// </summary>
	public enum OBEXVersion
	{
		V10 = 0x10,
	}

	public enum BluetoothRFCOMMParityType
	{
		NoParity = 0,
		OddParity,
		EvenParity,
		MaxParity
	}

	public enum BluetoothRFCOMMLineStatus
	{
		NoError = 0,
		OverrunError,
		ParityError,
		FramingError
	}
}