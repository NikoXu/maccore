//
// iobluetooth.cs
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
using MonoMac.IOKit;
using MonoMac.ObjCRuntime;

using BluetoothClockOffset = System.UInt16;
using BluetoothConnectionHandle = System.UInt16;
using BluetoothHCIPageTimeout = System.UInt16;
using BluetoothHCIRSSIValue = System.SByte;
using BluetoothL2CAPMTU = System.UInt16;
using BluetoothNumericValue = System.UInt32;
using BluetoothPasskey = System.UInt32;
using BluetoothRFCOMMChannelID = System.Byte;
using BluetoothRFCOMMMTU = System.UInt16;
using BluetoothSDPDataElementSizeDescriptor = System.Byte;
using BluetoothSDPDataElementTypeDescriptor = System.Byte;
using BluetoothSDPServiceAttributeID = System.UInt16;
using BluetoothSDPServiceRecordHandle = System.IntPtr;
using BluetoothSDPUUID16 = System.UInt16;
using BluetoothSDPUUID32 = System.UInt32;
using ByteCount = System.UInt64;
using IOBluetoothDeviceRef = System.IntPtr;
using IOBluetoothL2CAPChannelRef = System.IntPtr;
using IOBluetoothObjectID = System.UInt64;
using IOBluetoothRFCOMMChannelRef = System.IntPtr;
using IOBluetoothSDPDataElementRef = System.IntPtr;
using IOBluetoothSDPServiceRecordRef = System.IntPtr;
using IOBluetoothSDPUUIDRef = System.IntPtr;
using OBEXConstants = System.Byte;
using OBEXFlags = System.Byte;
using OBEXMaxPacketLength = System.UInt32;
using OBEXOpCode = System.Byte;
using UInt8 = System.Byte;
using size_t = System.UIntPtr;
using uint16_t = System.UInt16;
using uint32_t = System.UInt32;
using uint8_t = System.Byte;

namespace MonoMac.IOBluetooth
{
	[Model]
	[Since (2,0)]
	[BaseType (typeof (NSObject))]
	interface IOBluetoothDeviceAsyncCallbacks {
		[Export("remoteNameRequestComplete:status:")]
		void RequestRemoteNameComplete (IOBluetoothDevice device, IOReturn status);

		[Export("connectionComplete:status:")]
		void ConnectionComplete (IOBluetoothDevice device, IOReturn status);

		[Export("sdpQueryComplete:status:")]
		void SdpQueryComplete (IOBluetoothDevice device, IOReturn status);
	}

	[Since (2,0)]
	[BaseType (typeof (IOBluetoothObject))]
	interface IOBluetoothDevice {
		[Since (7,0)]
		[Export ("classOfDevice")]
		BluetoothClassOfDevice ClassOfDevice { get; }

		[Since (7,0)]
		[Export ("serviceClassMajor")]
		BluetoothServiceClassMajor ServiceClassMajor { get; }

		[Since (7,0)]
		[Export ("deviceClassMajor")]
		BluetoothDeviceClassMajor DeviceClassMajor { get; }

		[Since (7,0)]
		[Export ("deviceClassMinor")]
		BluetoothDeviceClassMinor DeviceClassMinor { get; }

		[Since (6,0)]
		[Export ("name")]
		string Name { get; }

		[Since (6,0)]
		[Export ("nameOrAddress")]
		string NameOrAddress { get; }

		[Since (7,0)]
		[Export ("lastNameUpdate")]
		NSDate LastNameUpdate { get; }

		[Since (7,0)]
		[Export ("addressString")]
		string AddressString { get; }

		[Since (7,0)]
		[Export ("connectionHandle")]
		BluetoothConnectionHandle ConnectionHandle { get; }

		[Since (7,0)]
		[Export ("services")]
		IOBluetoothSDPServiceRecord[] Services { get; }

		[Static]
		[Export ("registerForConnectNotifications:selector:"), Internal]
		IOBluetoothUserNotification registerForConnectNotifications (NSObject observer, Selector inSelector);
		
		[Export ("registerForDisconnectNotification:selector:"), Internal]
		IOBluetoothUserNotification registerForDisconnectNotification (NSObject observer, Selector inSelector);

		[Static]
		[Since (7,0)]
		[Export ("deviceWithAddress:")]
		IOBluetoothDevice GetDeviceWithAddress (BluetoothDeviceAddress address);

//		[Static]
//		[Export ("withAddress:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothDevice WithAddress (BluetoothDeviceAddress address);

		[Static]
		[Since (7,0)]
		[Export ("deviceWithAddressString:")]
		IOBluetoothDevice GetDeviceWithAddress (string address);

//		[Static]
//		[Export ("withDeviceRef:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothDevice WithDeviceRef (IOBluetoothDeviceRef deviceRef);
//
//		[Export ("getDeviceRef")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothDeviceRef DeviceRef { get; }

		[Since (2,5)]
		[Export ("openL2CAPChannelSync:withPSM:delegate:"), Internal]
		IOReturn openL2CAPChannelSync (out IOBluetoothL2CAPChannel newChannel, BluetoothL2CAPPSM psm, [NullAllowed] NSObject channelDelegate);

		[Since (2,5)]
		[Export ("openL2CAPChannelAsync:withPSM:delegate:"), Internal]
		IOReturn openL2CAPChannelAsync (out IOBluetoothL2CAPChannel newChannel, BluetoothL2CAPPSM psm, [NullAllowed] NSObject channelDelegate);

//		[Export ("openL2CAPChannel:findExisting:newChannel:")]
//		[Obsolete ("Deprecated in OS X 5.0"), Internal]
//		IOReturn openL2CAPChannel (BluetoothL2CAPPSM psm, bool findExisting, IOBluetoothL2CAPChannel newChannel);

		[Export ("sendL2CAPEchoRequest:length:"), Internal]
		IOReturn sendL2CAPEchoRequest (IntPtr data, UInt16 length);

//		[Export ("openRFCOMMChannel:channel:")]
//		[Obsolete ("Deprecated in OS X 5.0")]
//		IOReturn OpenRFCOMMChannel (BluetoothRFCOMMChannelID channelID, IOBluetoothRFCOMMChannel rfcommChannel);

		[Since (2,5)]
		[Export ("openRFCOMMChannelSync:withChannelID:delegate:"), Internal]
		IOReturn openRFCOMMChannelSync (out IOBluetoothRFCOMMChannel rfcommChannel, BluetoothRFCOMMChannelID channelID, [NullAllowed] NSObject channelDelegate);

		[Since (2,5)]
		[Export ("openRFCOMMChannelAsync:withChannelID:delegate:"), Internal]
		IOReturn openRFCOMMChannelAsync (out IOBluetoothRFCOMMChannel rfcommChannel, BluetoothRFCOMMChannelID channelID, [NullAllowed] NSObject channelDelegate);

//		[Export ("getClassOfDevice")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothClassOfDevice GetClassOfDevice ();
//
//		[Export ("getServiceClassMajor")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothServiceClassMajor GetServiceClassMajor ();
//
//		[Export ("getDeviceClassMajor")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothDeviceClassMajor GetDeviceClassMajor ();
//
//		[Export ("getDeviceClassMinor")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothDeviceClassMinor GetDeviceClassMinor ();
//
//		[Export ("getName")]
//		[Obsolete ("Deprecated in OS X 6.0")]
//		string GetName ();
//
//		[Export ("getNameOrAddress")]
//		[Obsolete ("Deprecated in OS X 6.0")]
//		string GetNameOrAddress ();
//
//		[Export ("getLastNameUpdate")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		NSDate GetLastNameUpdate ();

		[Export ("getAddress"), Internal]
		/*BluetoothDeviceAddress*/ IntPtr address { get; }

//		[Export ("getAddressString")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		string GetAddressString ();

		[Export ("getPageScanRepetitionMode")]
		BluetoothPageScanRepetitionMode PageScanRepetitionMode { get; }

		[Export ("getPageScanPeriodMode")]
		BluetoothPageScanPeriodMode PageScanPeriodMode { get; }

		[Export ("getPageScanMode")]
		BluetoothPageScanMode PageScanMode { get; }

		[Export ("getClockOffset")]
		BluetoothClockOffset ClockOffset { get; }

		[Export ("getLastInquiryUpdate")]
		NSDate LastInquiryUpdate { get; }

		[Since (7,0)]
		[Export ("RSSI")]
		BluetoothHCIRSSIValue RSSI { get; }

		[Since (7,0)]
		[Export ("rawRSSI")]
		BluetoothHCIRSSIValue RawRSSI { get; }

		[Export ("isConnected")]
		bool IsConnected { get; }

		[Export ("openConnection"), Internal]
		IOReturn openConnection ();

		[Export ("openConnection:"), Internal]
		IOReturn openConnection (NSObject target);

		[Since (2,7)]
		[Export ("openConnection:withPageTimeout:authenticationRequired:"), Internal]
		IOReturn openConnection (NSObject target, BluetoothHCIPageTimeout pageTimeoutValue, bool authenticationRequired);

		[Export ("closeConnection"), Internal]
		IOReturn closeConnection ();

		[Export ("remoteNameRequest:"), Internal]
		IOReturn remoteNameRequest (NSObject target);

		[Since (2,7)]
		[Export ("remoteNameRequest:withPageTimeout:"), Internal]
		IOReturn remoteNameRequest (NSObject target, BluetoothHCIPageTimeout pageTimeoutValue);

		[Export ("requestAuthentication"), Internal]
		IOReturn requestAuthentication ();

//		[Export ("getConnectionHandle")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothConnectionHandle GetConnectionHandle ();

		[Since (2,7)]
		[Export ("isIncoming")]
		bool IsIncoming { get; }

		[Export ("getLinkType")]
		BluetoothLinkType LinkType { get; }

		[Export ("getEncryptionMode")]
		BluetoothHCIEncryptionMode EncryptionMode { get; }

		[Export ("performSDPQuery:"), Internal]
		IOReturn performSDPQuery (NSObject target);

		[Since (7,0)]
		[Export ("performSDPQuery:uuids:"), Internal]
		IOReturn performSDPQuery (NSObject target, IOBluetoothSDPUUID[] uuids);

//		[Export ("getServices")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothSDPServiceRecord[] GetServices ();

		[Export ("getLastServicesUpdate")]
		NSDate LastServicesUpdate { get; }

		[Export ("getServiceRecordForUUID:")]
		IOBluetoothSDPServiceRecord GetServiceRecordForUUID (IOBluetoothSDPUUID sdpUUID);

		[Static]
		[Since (2,4)]
		[Export ("favoriteDevices")]
		IOBluetoothDevice[] FavoriteDevices { get; }

		[Since (2,4)]
		[Export ("isFavorite")]
		bool IsFavorite { get; }

		[Since (2,4)]
		[Export ("addToFavorites"), Internal]
		IOReturn addToFavorites ();

		[Since (2,4)]
		[Export ("removeFromFavorites"), Internal]
		IOReturn removeFromFavorites ();

		[Static]
		[Since (2,4)]
		[Export ("recentDevices:")]
		IOBluetoothDevice[] GetRecentDevices (ulong numDevices);

		[Since (2,4)]
		[Export ("recentAccessDate")]
		NSDate RecentAccessDate { get; }

		[Static]
		[Since (2,5)]
		[Export ("pairedDevices")]
		IOBluetoothDevice[] PairedDevices { get; }

		[Since (2,5)]
		[Export ("isPaired")]
		bool IsPaired { get; }

		[Since (6,0)]
		[Export ("setSupervisionTimeout:"), Internal]
		IOReturn setSupervisionTimeout (UInt16 timeout);

		[Since (6,0)]
		[Export ("openL2CAPChannelSync:withPSM:withConfiguration:delegate:"), Internal]
		IOReturn openL2CAPChannelSync (IOBluetoothL2CAPChannel newChannel, BluetoothL2CAPPSM psm, NSDictionary channelConfiguration, NSObject channelDelegate);

		[Since (6,0)]
		[Export ("openL2CAPChannelAsync:withPSM:withConfiguration:delegate:"), Internal]
		IOReturn openL2CAPChannelAsync (IOBluetoothL2CAPChannel newChannel, BluetoothL2CAPPSM psm, NSDictionary channelConfiguration, NSObject channelDelegate);

//		[Export ("awakeAfterUsingCoder:")]
//		NSObject AwakeAfterUsingCoder (NSCoder coder);
	}

	[Category]
	[BaseTypeAttribute (typeof(IOBluetoothDevice))]
	interface HandsFreeDeviceAdditions {
		[Bind ("handsFreeAudioGatewayDriverID")]
		string GetHandsFreeAudioGatewayDriverID ();

		[Bind ("handsFreeAudioGatewayServiceRecord")]
		IOBluetoothSDPServiceRecord GetHandsFreeAudioGatewayServiceRecord ();

		[Bind ("isHandsFreeAudioGateway")]
		bool GetIsHandsFreeAudioGateway ();

		[Bind ("handsFreeDeviceDriverID")]
		string GetHandsFreeDeviceDriverID ();

		[Bind ("handsFreeDeviceServiceRecord")]
		IOBluetoothSDPServiceRecord GetHandsFreeDeviceServiceRecord ();

		[Bind ("isHandsFreeDevice")]
		bool GetIsHandsFreeDevice ();
	}

	[BaseType (typeof (NSObject))]
	interface IOBluetoothDeviceInquiry {
		[Wrap ("WeakDelegate")]
		IOBluetoothDeviceInquiryDelegate Delegate { get; set; }

		[Export ("delegate")]
		NSObject WeakDelegate { get; set; }

		[Static]
		[Export ("inquiryWithDelegate:")]
		IOBluetoothDeviceInquiry CreateInquiryWithDelegate (IOBluetoothDeviceInquiryDelegate @delegate);

		[Export ("initWithDelegate:")]
		NSObject Constructor (IOBluetoothDeviceInquiryDelegate @delegate);

		[Export ("start")]
		IOReturn Start ();

		[Export ("stop")]
		IOReturn Stop ();

		[Export ("foundDevices")]
		IOBluetoothDevice[] FoundDevices { get; }

		[Export ("clearFoundDevices")]
		void ClearFoundDevices ();

		[Export ("setSearchCriteria:majorDeviceClass:minorDeviceClass:")]
		void SetSearchCriteria (BluetoothServiceClassMajor inServiceClassMajor, BluetoothDeviceClassMajor inMajorDeviceClass, BluetoothDeviceClassMinor inMinorDeviceClass);

		//Detected properties
		[Export ("inquiryLength")]
		uint8_t InquiryLength { get; set; }

		[Export ("searchType")]
		IOBluetoothDeviceSearchTypes SearchType { get; set; }

		[Export ("updateNewDeviceNames")]
		bool UpdateNewDeviceNames { get; set; }

	}

	[BaseType (typeof (NSObject))]
	[Model]
	interface IOBluetoothDeviceInquiryDelegate {
		[Export ("deviceInquiryStarted:"), EventArgs ("DeviceInquiryStarted")]
		void Started (IOBluetoothDeviceInquiry sender);

		[Export ("deviceInquiryDeviceFound:device:"), EventArgs ("DeviceInquiryDeviceFound")]
		void DeviceFound (IOBluetoothDeviceInquiry sender, IOBluetoothDevice device);

		[Export ("deviceInquiryUpdatingDeviceNamesStarted:devicesRemaining:"), EventArgs ("DeviceInquiryUpdatingDeviceNamesStarted")]
		void UpdatingDeviceNamesStarted (IOBluetoothDeviceInquiry sender, uint32_t devicesRemaining);

		[Export ("deviceInquiryDeviceNameUpdated:device:devicesRemaining:"), EventArgs ("DeviceInquiryDeviceNameUpdated")]
		void DeviceNameUpdated (IOBluetoothDeviceInquiry sender, IOBluetoothDevice device, uint32_t devicesRemaining);

		[Export ("deviceInquiryComplete:error:aborted:"), EventArgs ("DeviceInquiryCompleted")]
		void Completed (IOBluetoothDeviceInquiry sender, IOReturn error, bool aborted);

	}

	[BaseType (typeof (NSObject))]
	interface IOBluetoothDevicePair {
		[Wrap ("WeakDelegate")]
		IOBluetoothDevicePairDelegate Delegate { get; set; }

		[Export ("delegate")]
		NSObject WeakDelegate { get; set; }

		[Static]
		[Export ("pairWithDevice:")]
		IOBluetoothDevicePair GetPairWithDevice (IOBluetoothDevice device);

		[Export ("start")]
		IOReturn Start ();

		[Export ("replyPINCode:PINCode:")]
		void ReplyPINCode (ByteCount PINCodeSize, BluetoothPINCode PINCode);

		[Export ("replyUserConfirmation:")]
		void ReplyUserConfirmation (bool reply);

		//Detected properties
		[Export ("device")]
		IOBluetoothDevice Device { get; set; }

	}

	[BaseType (typeof (NSObject))]
	[Model]
	interface IOBluetoothDevicePairDelegate {
		[Export ("devicePairingStarted:"), EventArgs ("DevicePairingStarted")]
		void Started (NSObject sender);

		[Export ("devicePairingConnecting:"), EventArgs ("DevicePairingConnecting")]
		void Connecting (NSObject sender);

		[Export ("devicePairingPINCodeRequest:"), EventArgs ("DevicePairingPINCodeRequested")]
		void PINCodeRequested (NSObject sender);

		[Export ("devicePairingUserConfirmationRequest:numericValue:"), EventArgs ("DevicePairingUserConfirmationRequested")]
		void UserConfirmationRequested (NSObject sender, BluetoothNumericValue numericValue);

		[Export ("devicePairingUserPasskeyNotification:passkey:"), EventArgs ("DevicePairingUserPasskeyNotificationReceived")]
		void UserPasskeyNotificationReceived (NSObject sender, BluetoothPasskey passkey);

		[Export ("devicePairingFinished:error:"), EventArgs ("DevicePairingFinished")]
		void Finished (NSObject sender, IOReturn error);

	}

	[BaseType (typeof (NSObject))]
	interface IOBluetoothHandsFree {
		[Export ("supportedFeatures")]
		uint32_t SupportedFeatures { get; set; }

		// not sure where IOBluetoothSCOAudioDevice is defined
//		[Export ("scoAudioDevice")]
//		IOBluetoothSCOAudioDevice ScoAudioDevice { get; }

		[Export ("inputVolume")]
		float InputVolume { get; set; }

		[Export ("inputMuted")]
		bool InputMuted { [Bind ("isInputMuted")] get; set; }

		[Export ("outputVolume")]
		float OutputVolume { get; set; }

		[Export ("outputMuted")]
		bool OutputMuted { [Bind ("isOutputMuted")] get; set; }

		[Export ("device")]
		IOBluetoothDevice Device { get; }

		[Export ("deviceSupportedFeatures")]
		uint32_t DeviceSupportedFeatures { get; }

		[Export ("deviceSupportedSMSServices")]
		uint32_t DeviceSupportedSMSServices { get; }

		[Export ("deviceCallHoldModes")]
		uint32_t DeviceCallHoldModes { get; }

		[Export ("SMSMode")]
		IOBluetoothSMSMode SMSMode { get; }

		[Export ("SMSEnabled")]
		bool SMSEnabled { [Bind ("isSMSEnabled")] get; }

		[Wrap ("WeakDelegate")]
		IOBluetoothHandsFreeDelegate Delegate { get; set; }

		[Export ("delegate")]
		NSObject WeakDelegate { get; set; }

		[Export ("indicator:")]
		int GetIndicator (string indicatorName);

		[Export ("setIndicator:value:")]
		void SetIndicator (string indicatorName, int indicatorValue);

		[Export ("initWithDevice:delegate:")]
		NSObject Constructor (IOBluetoothDevice device, IOBluetoothHandsFreeDelegate inDelegate);

		[Export ("connect")]
		void Connect ();

		[Export ("disconnect")]
		void Disconnect ();

		[Export ("isConnected")]
		bool IsConnected { get; }

		[Export ("connectSCO")]
		void ConnectSCO ();

		[Export ("disconnectSCO")]
		void DisconnectSCO ();

		[Export ("isSCOConnected")]
		bool IsSCOConnected { get; }

	}

	[Category]
	[BaseType (typeof(IOBluetoothSDPServiceRecord))]
	interface HandsFreeSDPServiceRecordAdditions {
		[Export ("handsFreeSupportedFeatures")]
		uint16_t GetHandsFreeSupportedFeatures ();
	}

	[BaseType (typeof (NSObject))]
	[Model]
	interface IOBluetoothHandsFreeDelegate {
		[Export ("handsFree:connected:")]
		void Connected (IOBluetoothHandsFree device, NSNumber status);

		[Export ("handsFree:disconnected:")]
		void Disconnected (IOBluetoothHandsFree device, NSNumber status);

		[Export ("handsFree:scoConnectionOpened:")]
		void ScoConnectionOpened (IOBluetoothHandsFree device, NSNumber status);

		[Export ("handsFree:scoConnectionClosed:")]
		void ScoConnectionClosed (IOBluetoothHandsFree device, NSNumber status);

	}

	[BaseType (typeof (IOBluetoothHandsFree))]
	interface IOBluetoothHandsFreeAudioGateway {
		[Export ("initWithDevice:delegate:")]
		NSObject Constructor (IOBluetoothDevice device, NSObject inDelegate);

		[Export ("createIndicator:min:max:currentValue:")]
		void CreateIndicator (string indicatorName, int minValue, int maxValue, int currentValue);

		[Export ("processATCommand:")]
		void ProcessATCommand (string atCommand);

		[Export ("sendOKResponse")]
		void SendOKResponse ();

		[Export ("sendResponse:")]
		void SendResponse (string response);

		[Export ("sendResponse:withOK:")]
		void SendResponse (string response, bool withOK);

	}

	[BaseType (typeof (NSObject))]
	[Model]
	interface IOBluetoothHandsFreeAudioGatewayDelegate {
		[Export ("handsFree:hangup:")]
		void Hangup (IOBluetoothHandsFreeAudioGateway device, NSNumber hangup);

		[Export ("handsFree:redial:")]
		void Redial (IOBluetoothHandsFreeAudioGateway device, NSNumber redial);

	}

	[BaseType (typeof (IOBluetoothHandsFree))]
	interface IOBluetoothHandsFreeDevice {
		[Export ("initWithDevice:delegate:")]
		NSObject Constructor (IOBluetoothDevice device, NSObject @delegate);

		[Export ("dialNumber:")]
		void DialNumber (string aNumber);

		[Export ("memoryDial:")]
		void MemoryDial (int memoryLocation);

		[Export ("redial")]
		void Redial ();

		[Export ("endCall")]
		void EndCall ();

		[Export ("acceptCall")]
		void AcceptCall ();

		[Export ("acceptCallOnPhone")]
		void AcceptCallOnPhone ();

		[Export ("sendDTMF:")]
		void SendDTMF (string character);

		[Export ("subscriberNumber")]
		void SubscriberNumber ();

		[Export ("currentCallList")]
		void GetCurrentCallList ();

		[Export ("releaseHeldCalls")]
		void ReleaseHeldCalls ();

		[Export ("releaseActiveCalls")]
		void ReleaseActiveCalls ();

		[Export ("releaseCall:")]
		void ReleaseCall (int index);

		[Export ("holdCall")]
		void HoldCall ();

		[Export ("placeAllOthersOnHold:")]
		void PlaceAllOthersOnHold (int index);

		[Export ("addHeldCall")]
		void AddHeldCall ();

		[Export ("callTransfer")]
		void TransferCall ();

		[Export ("transferAudioToComputer")]
		void TransferAudioToComputer ();

		[Export ("transferAudioToPhone")]
		void TransferAudioToPhone ();

		[Export ("sendSMS:message:")]
		void SendSMSmessage (string aNumber, string aMessage);

		[Export ("sendATCommand:")]
		void SendATCommand (string atCommand);

		[Export ("sendATCommand:timeout:selector:target:")]
		void SendATCommand (string atCommand, float timeout, Selector selector, NSObject target);

	}

	[BaseType (typeof (IOBluetoothHandsFreeDelegate))]
	[Model]
	interface IOBluetoothHandsFreeDeviceDelegate {
		[Export ("handsFree:isServiceAvailable:")]
		void IsServiceAvailable (IOBluetoothHandsFreeDevice device, NSNumber isServiceAvailable);

		[Export ("handsFree:isCallActive:")]
		void IsCallActive (IOBluetoothHandsFreeDevice device, NSNumber isCallActive);

		[Export ("handsFree:callSetupMode:")]
		void CallSetupMode (IOBluetoothHandsFreeDevice device, NSNumber callSetupMode);

		[Export ("handsFree:callHoldState:")]
		void CallHoldState (IOBluetoothHandsFreeDevice device, NSNumber callHoldState);

		[Export ("handsFree:signalStrength:")]
		void SignalStrength (IOBluetoothHandsFreeDevice device, NSNumber signalStrength);

		[Export ("handsFree:isRoaming:")]
		void IsRoaming (IOBluetoothHandsFreeDevice device, NSNumber isRoaming);

		[Export ("handsFree:batteryCharge:")]
		void BatteryCharge (IOBluetoothHandsFreeDevice device, NSNumber batteryCharge);

		[Export ("handsFree:currentCall:")]
		void CurrentCall (IOBluetoothHandsFreeDevice device, NSDictionary currentCall);

		[Export ("handsFree:subscriberNumber:")]
		void SubscriberNumber (IOBluetoothHandsFreeDevice device, string subscriberNumber);

		[Export ("handsFree:incomingSMS:")]
		void IncomingSMS (IOBluetoothHandsFreeDevice device, NSDictionary sms);

		[Export ("handsFree:unhandledResultCode:")]
		void UnhandledResultCode (IOBluetoothHandsFreeDevice device, string resultCode);

	}

//	[BaseType (typeof (IOBluetoothRFCOMMAudioController))]
//	interface IOBluetoothHandsFreeGateway {
//		[Static]
//		[Export ("getRequiredSDPServiceRecordForDevice:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothSDPServiceRecord GetRequiredSDPServiceRecordForDevice (IOBluetoothDevice device);
//
//		[Static]
//		[Export ("getRequiredSDPRFCOMMChannelIDForDevice:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothRFCOMMChannelID GetRequiredSDPRFCOMMChannelIDForDevice (IOBluetoothDevice device);
//
//		[Export ("initWithIncomingDevice:incomingRFCOMMChannelID:supportedFeatures:delegate:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		NSObject InitWithIncomingDeviceincomingRFCOMMChannelIDsupportedFeaturesdelegate (IOBluetoothDevice device, BluetoothRFCOMMChannelID incomingRFCOMMChannelID, UInt32 supportedFeatures, NSObject inDelegate);
//
//		[Export ("initForConnectionToDevice:supportedFeatures:delegate:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		NSObject InitForConnectionToDevicesupportedFeaturesdelegate (IOBluetoothDevice device, UInt32 supportedFeatures, NSObject inDelegate);
//
//		[Export ("setGatewaySupportedFeatures:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		void SetGatewaySupportedFeatures (UInt32 gatewaySupportedFeatures);
//
//		[Export ("getGatewaySupportedFeatures")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		UInt32 GetGatewaySupportedFeatures ();
//
//		[Export ("getDeviceSupportedFeatures")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		UInt32 GetDeviceSupportedFeatures ();
//
//	}
//
//	[BaseType (typeof (IOBluetoothRFCOMMAudioController))]
//	interface IOBluetoothHeadsetDevice {
//		[Static]
//		[Export ("getRequiredSDPServiceRecordForDevice:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothSDPServiceRecord GetRequiredSDPServiceRecordForDevice (IOBluetoothDevice device);
//
//		[Static]
//		[Export ("getRequiredSDPRFCOMMChannelIDForDevice:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothRFCOMMChannelID GetRequiredSDPRFCOMMChannelIDForDevice (IOBluetoothDevice device);
//
//		[Export ("initWithIncomingDevice:incomingRFCOMMChannelID:delegate:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		NSObject InitWithIncomingDeviceincomingRFCOMMChannelIDdelegate (IOBluetoothDevice device, BluetoothRFCOMMChannelID incomingRFCOMMChannelID, NSObject inDelegate);
//
//		[Export ("initForConnectionToDevice:delegate:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		NSObject InitForConnectionToDevicedelegate (IOBluetoothDevice device, NSObject inDelegate);
//
//	}
//
//	[Category]
//	[BaseType (typeof (IOBluetoothDevice))]
//	interface HeadsetAdditions {
//		[Export ("isHeadsetAudioGateway")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		bool GetIsHeadsetAudioGateway ();
//
//		[Export ("headsetDeviceServiceRecord")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothSDPServiceRecord GetHeadsetDeviceServiceRecord ();
//
//		[Export ("isHeadsetDevice")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		bool GetIsHeadsetDevice ();
//
//	}

	[BaseType (typeof (NSObject))]
	interface IOBluetoothHostController {
		[Wrap ("WeakDelegate")]
		IOBluetoothHostControllerDelegate Delegate { get; set; }

		[Export ("delegate")]
		NSObject WeakDelegate { get; set; }

		[Export ("powerState")]
		BluetoothHCIPowerState PowerState { get; }

		[Static]
		[Export ("defaultController")]
		IOBluetoothHostController DefaultController { get; }

		[Export ("classOfDevice")]
		BluetoothClassOfDevice ClassOfDevice { get; }

		[Export ("setClassOfDevice:forTimeInterval:")]
		IOReturn SetClassOfDevice (BluetoothClassOfDevice classOfDevice, double seconds);

		[Export ("addressAsString")]
		string Address { get; }

		[Export ("nameAsString")]
		string Name { get; }

	}

	[BaseType (typeof (NSObject))]
	[Model]
	interface IOBluetoothHostControllerDelegate {
		[Export ("readRSSIForDeviceComplete:device:info:error:")]
		void ReadRSSIForDeviceCompleted (NSObject controller, IOBluetoothDevice device, BluetoothHCIRSSIInfo info, IOReturn error);

		[Export ("readLinkQualityForDeviceComplete:device:info:error:")]
		void ReadLinkQualityForDeviceCompleted (NSObject controller, IOBluetoothDevice device, BluetoothHCILinkQualityInfo info, IOReturn error);

	}

	[BaseType (typeof (IOBluetoothObject))]
	interface IOBluetoothL2CAPChannel {
		[Export ("outgoingMTU")]
		BluetoothL2CAPMTU OutgoingMTU { get; }

		[Export ("incomingMTU")]
		BluetoothL2CAPMTU IncomingMTU { get; }

		[Export ("device")]
		IOBluetoothDevice Device { get; }

		[Export ("objectID")]
		IOBluetoothObjectID ObjectID { get; }

		[Export ("PSM")]
		BluetoothL2CAPPSM PSM { get; }

		[Export ("localChannelID")]
		BluetoothL2CAPChannelID LocalChannelID { get; }

		[Export ("remoteChannelID")]
		BluetoothL2CAPChannelID RemoteChannelID { get; }

		[Static]
		[Export ("registerForChannelOpenNotifications:selector:"), Internal]
		IOBluetoothUserNotification RegisterForChannelOpenNotifications (NSObject @object, Selector selector);

		[Static]
		[Export ("registerForChannelOpenNotifications:selector:withPSM:direction:"), Internal]
		IOBluetoothUserNotification RegisterForChannelOpenNotifications (NSObject @object, Selector selector, BluetoothL2CAPPSM psm, IOBluetoothUserNotificationChannelDirection inDirection);

		[Static]
		[Export ("withObjectID:")]
		IOBluetoothL2CAPChannel GetChannelWithObjectID (IOBluetoothObjectID objectID);

		[Export ("closeChannel")]
		IOReturn CloseChannel ();

//		[Export ("getOutgoingMTU")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothL2CAPMTU GetOutgoingMTU ();
//
//		[Export ("getIncomingMTU")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothL2CAPMTU GetIncomingMTU ();

		[Export ("requestRemoteMTU:")]
		IOReturn RequestRemoteMTU (BluetoothL2CAPMTU remoteMTU);

		[Export ("writeAsync:length:refcon:")]
		IOReturn WriteAsync (IntPtr data, UInt16 length, IntPtr refcon);

		[Export ("writeSync:length:")]
		IOReturn WriteSync (IntPtr data, UInt16 length);

		[Export ("setDelegate:withConfiguration:")]
		IOReturn SetDelegate (NSObject channelDelegate, NSDictionary channelConfiguration);

//		[Export ("getDevice")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothDevice GetDevice ();
//
//		[Export ("getObjectID")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothObjectID GetObjectID ();
//
//		[Export ("getPSM")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothL2CAPPSM GetPSM ();
//
//		[Export ("getLocalChannelID")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothL2CAPChannelID GetLocalChannelID ();
//
//		[Export ("getRemoteChannelID")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothL2CAPChannelID GetRemoteChannelID ();

		[Export ("isIncoming")]
		bool IsIncoming { get; }

		[Export ("registerForChannelCloseNotification:selector:"), Internal]
		IOBluetoothUserNotification RegisterForChannelCloseNotifications (NSObject observer, Selector inSelector);

		//Detected properties
		[Wrap ("WeakDelegate")]
		IOBluetoothL2CAPChannelDelegate Delegate { get; set; }

		[Export ("delegate")]
		NSObject WeakDelegate { get; set; }

		[Export ("write:length:")]
		[Obsolete ("Deprecated in OS X 5.0")]
		IOReturn Write (IntPtr data, UInt16 length);

//		[Static]
//		[Export ("withL2CAPChannelRef:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothL2CAPChannel GetChannelWithL2CAPChannelRef (IOBluetoothL2CAPChannelRef l2capChannelRef);

//		[Export ("getL2CAPChannelRef")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothL2CAPChannelRef GetL2CAPChannelRef ();

	}

	[BaseType (typeof (NSObject))]
	[Model]
	interface IOBluetoothL2CAPChannelDelegate {
		[Export ("l2capChannelData:data:length:")]
		void DataReceived (IOBluetoothL2CAPChannel l2capChannel, IntPtr dataPointer, size_t dataLength);

		[Export ("l2capChannelOpenComplete:status:")]
		void OpenCompleted (IOBluetoothL2CAPChannel l2capChannel, IOReturn error);

		[Export ("l2capChannelClosed:")]
		void Closed (IOBluetoothL2CAPChannel l2capChannel);

		[Export ("l2capChannelReconfigured:")]
		void Reconfigured (IOBluetoothL2CAPChannel l2capChannel);

		[Export ("l2capChannelWriteComplete:refcon:status:")]
		void WriteCompleted (IOBluetoothL2CAPChannel l2capChannel, IntPtr refcon, IOReturn error);

		[Export ("l2capChannelQueueSpaceAvailable:")]
		void QueueSpaceAvailable (IOBluetoothL2CAPChannel l2capChannel);

	}

	[BaseType (typeof (OBEXSession))]
	interface IOBluetoothOBEXSession {
		[Static]
		[Export ("withSDPServiceRecord:")]
		IOBluetoothOBEXSession GetSessionWithSDPServiceRecord (IOBluetoothSDPServiceRecord inSDPServiceRecord);

		[Export ("withDevice:channelID:")]
		IOBluetoothOBEXSession GetSessionWithDevice (IOBluetoothDevice inDevice, BluetoothRFCOMMChannelID inRFCOMMChannelID);

		[Export ("withIncomingRFCOMMChannel:eventSelector:selectorTarget:refCon:")]
		IOBluetoothOBEXSession GetSessionWithIncomingRFCOMMChannel (IOBluetoothRFCOMMChannel inChannel, Selector inEventSelector, NSObject inEventSelectorTarget, IntPtr inUserRefCon);

		[Export ("initWithSDPServiceRecord:")]
		NSObject Constructor (IOBluetoothSDPServiceRecord inSDPServiceRecord);

		[Export ("initWithDevice:channelID:")]
		NSObject Constructor (IOBluetoothDevice inDevice, BluetoothRFCOMMChannelID inChannelID);

		[Export ("initWithIncomingRFCOMMChannel:eventSelector:selectorTarget:refCon:")]
		NSObject Constructor (IOBluetoothRFCOMMChannel inChannel, Selector inEventSelector, NSObject inEventSelectorTarget, IntPtr inUserRefCon);

		[Export ("getRFCOMMChannel")]
		IOBluetoothRFCOMMChannel RFCOMMChannel { get; }

		[Export ("getDevice")]
		IOBluetoothDevice Device { get; }

		[Export ("sendBufferTroughChannel")]
		IOReturn SendBufferTroughChannel ();

		[Export ("restartTransmission")]
		void RestartTransmission ();

		[Export ("isSessionTargetAMac")]
		bool IsSessionTargetAMac { get; }

		[Export ("openTransportConnection:selectorTarget:refCon:")]
		OBEXError OpenTransportConnection (Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("hasOpenTransportConnection")]
		bool HasOpenTransportConnection { get; }

		[Export ("closeTransportConnection")]
		OBEXError CloseTransportConnection ();

		[Export ("sendDataToTransport:dataLength:")]
		OBEXError SendDataToTransport (IntPtr inDataToSend, size_t inDataLength);

		[Export ("setOpenTransportConnectionAsyncSelector:target:refCon:")]
		void SetOpenTransportConnectionAsyncSelector (Selector inSelector, NSObject inSelectorTarget, NSObject inUserRefCon);

		[Export ("setOBEXSessionOpenConnectionCallback:refCon:")]
		void SetOBEXSessionOpenConnectionCallback (IOBluetoothOBEXSessionOpenConnectionCallback inCallback, IntPtr inUserRefCon);

	}

	[BaseType (typeof (NSObject))]
	interface IOBluetoothObject {
	}

//	[BaseType (typeof (NSObject))]
//	interface IOBluetoothRFCOMMAudioController {
//		[Export ("delegate")]
//		NSObject Delegate { get; set; }
//
//		[Export ("initWithIncomingDevice:incomingRFCOMMChannelID:delegate:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		NSObject InitWithIncomingDeviceincomingRFCOMMChannelIDdelegate (IOBluetoothDevice device, BluetoothRFCOMMChannelID incomingRFCOMMChannelID, NSObject inDelegate);
//
//		[Export ("initForConnectionToDevice:delegate:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		NSObject InitForConnectionToDevicedelegate (IOBluetoothDevice device, NSObject inDelegate);
//
//		[Static]
//		[Export ("getDriverIDForDevice:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		string GetDriverIDForDevice (IOBluetoothDevice inDevice);
//
//		// requires CoreAudio
////		[Export ("getAudioDeviceID")]
////		[Obsolete ("Deprecated in OS X 7.0")]
////		AudioDeviceID GetAudioDeviceID ();
//
//		[Export ("getBluetoothDevice")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothDevice GetBluetoothDevice ();
//
//		[Export ("isDeviceConnected")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		bool IsDeviceConnected ();
//
//		[Export ("openDeviceConnection")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOReturn OpenDeviceConnection ();
//
//		[Export ("closeDeviceConnection")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOReturn CloseDeviceConnection ();
//
//		[Export ("getIncomingRFCOMMChannelID")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothRFCOMMChannelID GetIncomingRFCOMMChannelID ();
//
//		[Export ("getOutgoingRFCOMMChannelID")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		BluetoothRFCOMMChannelID GetOutgoingRFCOMMChannelID ();
//
//		[Export ("isRFCOMMChannelOpen")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		bool IsRFCOMMChannelOpen ();
//
//		[Export ("setRFCOMMChannel:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		void SetRFCOMMChannel (IOBluetoothRFCOMMChannel rfcommChannel);
//
//		[Export ("openRFCOMMChannel")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOReturn OpenRFCOMMChannel ();
//
//		[Export ("closeRFCOMMChannel")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOReturn CloseRFCOMMChannel ();
//
//		[Export ("sendRFCOMMData:length:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOReturn SendRFCOMMDatalength (IntPtr data, uint16_t length);
//
//		[Export ("handleIncomingRFCOMMChannelOpened:channel:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		void HandleIncomingRFCOMMChannelOpenedchannel (IOBluetoothUserNotification notification, IOBluetoothRFCOMMChannel channel);
//
//		[Export ("rfcommChannelOpenComplete:status:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		void RfcommChannelOpenCompletestatus (IOBluetoothRFCOMMChannel rfcommChannel, IOReturn status);
//
//		[Export ("rfcommChannelClosed:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		void RfcommChannelClosed (IOBluetoothRFCOMMChannel rfcommChannel);
//
//		[Export ("rfcommChannelData:data:length:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		void RfcommChannelDatadatalength (IOBluetoothRFCOMMChannel rfcommChannel, IntPtr data, size_t length);
//
//		[Export ("isSCOConnected")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		bool IsSCOConnected ();
//
//		[Export ("openSCOConnection")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOReturn OpenSCOConnection ();
//
//		[Export ("closeSCOConnection")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOReturn CloseSCOConnection ();
//
//	}
//
//	[BaseType (typeof (NSObject))]
//	[Model]
//	interface IOBluetoothRFCOMMAudioDelegate {
//		[Export ("audioDevice:deviceConnectionOpened:")]
//		void AudioDevicedeviceConnectionOpened (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:deviceConnectionClosed:")]
//		void AudioDevicedeviceConnectionClosed (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:rfcommChannelOpened:")]
//		void AudioDevicerfcommChannelOpened (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:rfcommChannelClosed:")]
//		void AudioDevicerfcommChannelClosed (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:serviceLevelConnectionOpened:")]
//		void AudioDeviceserviceLevelConnectionOpened (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:serviceLevelConnectionClosed:")]
//		void AudioDeviceserviceLevelConnectionClosed (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:scoConnectionOpening:")]
//		void AudioDevicescoConnectionOpening (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:scoConnectionOpened:")]
//		void AudioDevicescoConnectionOpened (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:scoConnectionClosed:")]
//		void AudioDevicescoConnectionClosed (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:scoDone:")]
//		void AudioDevicescoDone (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:scoAudioDeviceActive:")]
//		void AudioDevicescoAudioDeviceActive (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:scoAudioDeviceInactive:")]
//		void AudioDevicescoAudioDeviceInactive (NSObject device, IOReturn status);
//
//		[Export ("audioDevice:disconnectedError:")]
//		void AudioDevicedisconnectedError (NSObject device, IOReturn status);
//
//	}

	[BaseType (typeof (IOBluetoothObject), Delegates=new string [] { "WeakDelegate" })]
	interface IOBluetoothRFCOMMChannel {
		[Static]
		[Export ("registerForChannelOpenNotifications:selector:"), Internal]
		IOBluetoothUserNotification registerForChannelOpenNotifications (NSObject @object, Selector selector);

		[Static]
		[Export ("registerForChannelOpenNotifications:selector:withChannelID:direction:"), Internal]
		IOBluetoothUserNotification registerForChannelOpenNotifications (NSObject @object, Selector selector, BluetoothRFCOMMChannelID channelID, IOBluetoothUserNotificationChannelDirection inDirection);

//		[Static]
//		[Export ("withRFCOMMChannelRef:")]
//		IOBluetoothRFCOMMChannel GetChannelWithRFCOMMChannelRef (IOBluetoothRFCOMMChannelRef rfcommChannelRef);

		[Static]
		[Export ("withObjectID:")]
		IOBluetoothRFCOMMChannel GetChannelWithObjectID (IOBluetoothObjectID objectID);

//		[Export ("getRFCOMMChannelRef")]
//		IOBluetoothRFCOMMChannelRef GetRFCOMMChannelRef ();

		[Export ("closeChannel"), Internal]
		IOReturn closeChannel ();

		[Export ("isOpen")]
		bool IsOpen { get; }

		[Export ("getMTU")]
		BluetoothRFCOMMMTU MTU { get; }

		[Export ("isTransmissionPaused")]
		bool IsTransmissionPaused { get; }

//		[Export ("write:length:sleep:"), Internal]
//		[Obsolete ("Deprecated in OS X 5.0")]
//		IOReturn Write (IntPtr data, UInt16 length, bool sleep);

		[Export ("writeAsync:length:refcon:"), Internal]
		IOReturn writeAsync (IntPtr data, UInt16 length, IntPtr refcon);

		[Export ("writeSync:length:"), Internal]
		IOReturn writeSync (IntPtr data, UInt16 length);

//		[Export ("writeSimple:length:sleep:bytesSent:"), Internal]
//		[Obsolete ("Deprecated in OS X 5.0")]
//		IOReturn writeSimple (IntPtr data, UInt16 length, bool sleep, UInt32 numBytesSent);

		[Export ("setSerialParameters:dataBits:parity:stopBits:"), Internal]
		IOReturn setSerialParameters (UInt32 speed, UInt8 nBits, BluetoothRFCOMMParityType parity, UInt8 bitStop);

		[Export ("sendRemoteLineStatus:"), Internal]
		IOReturn sendRemoteLineStatus (BluetoothRFCOMMLineStatus lineStatus);

		[Export ("getChannelID")]
		BluetoothRFCOMMChannelID ChannelID { get; }

		[Export ("isIncoming")]
		bool IsIncoming { get; }

		[Export ("getDevice")]
		IOBluetoothDevice Device { get; }

		[Export ("getObjectID")]
		IOBluetoothObjectID ObjectID { get; }
		
		[Export ("registerForChannelCloseNotification:selector:"), Internal]
		IOBluetoothUserNotification registerForChannelCloseNotification (NSObject observer, Selector inSelector);

		//Detected properties
		[Wrap ("WeakDelegate"), NullAllowed]
		IOBluetoothRFCOMMChannelDelegate Delegate { get; set; }

		[Export ("delegate"), NullAllowed]
		NSObject WeakDelegate { get; set; }
	}

	[BaseType (typeof (NSObject))]
	[Model]
	interface IOBluetoothRFCOMMChannelDelegate {
		[Export ("rfcommChannelData:data:length:")]
		void DataReceived (IOBluetoothRFCOMMChannel rfcommChannel, IntPtr dataPointer, size_t dataLength);

		[Export ("rfcommChannelOpenComplete:status:")]
		void Opened (IOBluetoothRFCOMMChannel rfcommChannel, IOReturn error);

		[Export ("rfcommChannelClosed:")]
		void Closed (IOBluetoothRFCOMMChannel rfcommChannel);

		[Export ("rfcommChannelControlSignalsChanged:")]
		void ControlSignalsChanged (IOBluetoothRFCOMMChannel rfcommChannel);

		[Export ("rfcommChannelFlowControlChanged:")]
		void FlowControlChanged (IOBluetoothRFCOMMChannel rfcommChannel);

		[Export ("rfcommChannelWriteComplete:refcon:status:")]
		void WriteCompleted (IOBluetoothRFCOMMChannel rfcommChannel, IntPtr refcon, IOReturn error);

		[Export ("rfcommChannelQueueSpaceAvailable:")]
		void QueueSpaceBecameAvailable (IOBluetoothRFCOMMChannel rfcommChannel);
	}

	[BaseType (typeof (NSObject))]
	interface IOBluetoothSDPDataElement {
		[Static]
		[Export ("withElementValue:")]
		IOBluetoothSDPDataElement GetElementWithElementValue (NSObject element);

		[Static]
		[Export ("withType:sizeDescriptor:size:value:")]
		IOBluetoothSDPDataElement GetElementWithType (BluetoothSDPDataElementTypeDescriptor type, BluetoothSDPDataElementSizeDescriptor newSizeDescriptor, uint32_t newSize, NSObject newValue);

//		[Static]
//		[Export ("withSDPDataElementRef:")]
//		IOBluetoothSDPDataElement GetElementWithSDPDataElementRef (IOBluetoothSDPDataElementRef sdpDataElementRef);

		[Export ("initWithElementValue:")]
		NSObject Constructor (NSObject element);

		[Export ("initWithType:sizeDescriptor:size:value:")]
		NSObject Constructor (BluetoothSDPDataElementTypeDescriptor newType, BluetoothSDPDataElementSizeDescriptor newSizeDescriptor, uint32_t newSize, NSObject newValue);

//		[Export ("getSDPDataElementRef")]
//		IOBluetoothSDPDataElementRef SDPDataElementRef { get; }

		[Export ("getTypeDescriptor")]
		BluetoothSDPDataElementTypeDescriptor TypeDescriptor { get; }

		[Export ("getSizeDescriptor")]
		BluetoothSDPDataElementSizeDescriptor SizeDescriptor { get; }

		[Export ("getSize")]
		uint32_t Size { get; }

		[Export ("getNumberValue")]
		NSNumber NumberValue { get; }

		[Export ("getDataValue")]
		NSData DataValue { get; }

		[Export ("getStringValue")]
		string StringValue { get; }

		[Export ("getArrayValue")]
		NSArray ArrayValue { get; }

		[Export ("getUUIDValue")]
		IOBluetoothSDPUUID UUIDValue { get; }

		[Export ("getValue")]
		NSObject Value { get; }

		[Export ("containsDataElement:")]
		bool ContainsDataElement (IOBluetoothSDPDataElement dataElement);

		[Export ("containsValue:")]
		bool ContainsValue (NSObject cmpValue);

	}

	[BaseType (typeof (NSObject))]
	interface IOBluetoothSDPServiceAttribute {
		[Static]
		[Export ("withID:attributeElementValue:")]
		IOBluetoothSDPServiceAttribute GetAttributeWithID (BluetoothSDPServiceAttributeID newAttributeID, NSObject attributeElementValue);

		[Static]
		[Export ("withID:attributeElement:")]
		IOBluetoothSDPServiceAttribute GetAttributeWithID (BluetoothSDPServiceAttributeID newAttributeID, IOBluetoothSDPDataElement attributeElement);

		[Export ("initWithID:attributeElementValue:")]
		NSObject Constructor (BluetoothSDPServiceAttributeID newAttributeID, NSObject attributeElementValue);

		[Export ("initWithID:attributeElement:")]
		NSObject Constructor (BluetoothSDPServiceAttributeID newAttributeID, IOBluetoothSDPDataElement attributeElement);

		[Export ("getAttributeID")]
		BluetoothSDPServiceAttributeID AttributeID { get; }

		[Export ("getDataElement")]
		IOBluetoothSDPDataElement DataElement { get; }

		[Export ("getIDDataElement")]
		IOBluetoothSDPDataElement IDDataElement { get; }

	}

	[BaseType (typeof (NSObject))]
	interface IOBluetoothSDPServiceRecord {
		[Export ("device")]
		IOBluetoothDevice Device { get; }

		[Export ("attributes")]
		NSDictionary Attributes { get; }

		[Export ("sortedAttributes")]
		IOBluetoothSDPServiceAttribute[] SortedAttributes { get; }

		[Static]
		[Export ("withServiceDictionary:device:")]
		IOBluetoothSDPServiceRecord GetRecordWithServiceDictionary (NSDictionary serviceDict, IOBluetoothDevice device);

		[Export ("initWithServiceDictionary:device:")]
		NSObject Constructor (NSDictionary serviceDict, IOBluetoothDevice device);

//		[Static]
//		[Export ("withSDPServiceRecordRef:")]
//		IOBluetoothSDPServiceRecord GetRecordWithSDPServiceRecordRef (IOBluetoothSDPServiceRecordRef sdpServiceRecordRef);

//		[Export ("getSDPServiceRecordRef")]
//		IOBluetoothSDPServiceRecordRef SDPServiceRecordRef { get; }

//		[Export ("getDevice")]
//		[Obsolete ("Deprecated in OS X 6.0")]
//		IOBluetoothDevice GetDevice ();
//
//		[Export ("getAttributes")]
//		[Obsolete ("Deprecated in OS X 6.0")]
//		NSDictionary GetAttributes ();

		[Export ("getAttributeDataElement:")]
		IOBluetoothSDPDataElement GetAttributeDataElement (BluetoothSDPServiceAttributeID attributeID);

		[Export ("getServiceName")]
		string ServiceName { get; }

		[Export ("getRFCOMMChannelID:"), Internal]
		IOReturn getRFCOMMChannelID (out BluetoothRFCOMMChannelID rfcommChannelID);

		[Export ("getL2CAPPSM:"), Internal]
		IOReturn getL2CAPPSM (out BluetoothL2CAPPSM outPSM);

//		[Export ("getServiceRecordHandle:"), Internal]
//		IOReturn getServiceRecordHandle (out BluetoothSDPServiceRecordHandle outServiceRecordHandle);

		[Export ("matchesUUID16:")]
		bool Matches (BluetoothSDPUUID16 uuid16);

		[Export ("matchesUUIDArray:")]
		bool Matches (IOBluetoothSDPUUID[] uuidArray);

//		[Export ("matchesSearchArray:")]
//		bool Matches (NSArray[] uuidArrays);

		[Export ("hasServiceFromArray:")]
		bool HasService (IOBluetoothSDPUUID[] serviceUuids);
	}

	[BaseType (typeof (NSData))]
	interface IOBluetoothSDPUUID {
		[Static]
		[Export ("uuidWithBytes:length:")]
		IOBluetoothSDPUUID CreateUuidWithBytes (IntPtr bytes, uint length);

		[Static]
		[Export ("uuidWithData:")]
		IOBluetoothSDPUUID CreateUuidWithData (NSData data);

		[Static]
		[Export ("uuid16:")]
		IOBluetoothSDPUUID FromUuid16 (BluetoothSDPUUID16 uuid16);

		[Static]
		[Export ("uuid32:")]
		IOBluetoothSDPUUID FromUuid32 (BluetoothSDPUUID32 uuid32);

//		[Static]
//		[Export ("withSDPUUIDRef:")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothSDPUUID FromSDPUUIDRef (IOBluetoothSDPUUIDRef sdpUUIDRef);

		[Export ("initWithUUID16:")]
		NSObject Constructor (BluetoothSDPUUID16 uuid16);

		[Export ("initWithUUID32:")]
		NSObject Constructor (BluetoothSDPUUID32 uuid32);

//		[Export ("getSDPUUIDRef")]
//		[Obsolete ("Deprecated in OS X 7.0")]
//		IOBluetoothSDPUUIDRef SDPUUIDRef { get; }

		[Export ("getUUIDWithLength:")]
		IOBluetoothSDPUUID GetUUIDWithLength (uint newLength);

		[Export ("isEqualToUUID:")]
		bool IsEqualToUUID (IOBluetoothSDPUUID otherUUID);

		[Export ("classForCoder")]
		Class ClassForCoder { get; }

		[Export ("classForArchiver")]
		Class ClassForArchiver { get; }

		[Export ("classForPortCoder")]
		Class ClassForPortCoder { get; }
	}

	[BaseType (typeof (NSObject)), Internal]
	interface IOBluetoothUserNotification {
		[Export ("unregister")]
		void Unregister ();
	}

	[Category]
	[BaseType (typeof (NSMutableDictionary))]
	interface NSDictionaryOBEXExtensions {
		[Static]
		[Export ("dictionaryWithOBEXHeadersData:headersDataSize:"), Internal]
		NSMutableDictionary createDictionaryWithOBEXHeadersData (IntPtr inHeadersData, size_t inDataSize);

		[Static]
		[Export ("dictionaryWithOBEXHeadersData:")]
		NSMutableDictionary CreateDictionaryWithOBEXHeadersData (NSData inHeadersData);

		[Export ("getHeaderBytes")]
		NSMutableData GetHeaderBytes ();

		[Export ("addTargetHeader:length:"), Internal]
		OBEXError addTargetHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Export ("addHTTPHeader:length:"), Internal]
		OBEXError addHTTPHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Export ("addBodyHeader:length:endOfBody:"), Internal]
		OBEXError addBodyHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength, bool isEndOfBody);

		[Export ("addWhoHeader:length:"), Internal]
		OBEXError addWhoHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Export ("addConnectionIDHeader:length:"), Internal]
		OBEXError addConnectionIDHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Export ("addApplicationParameterHeader:length:"), Internal]
		OBEXError addApplicationParameterHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Export ("addByteSequenceHeader:length:"), Internal]
		OBEXError addByteSequenceHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Export ("addObjectClassHeader:length:"), Internal]
		OBEXError addObjectClassHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Export ("addAuthorizationChallengeHeader:length:"), Internal]
		OBEXError addAuthorizationChallengeHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Export ("addAuthorizationResponseHeader:length:"), Internal]
		OBEXError addAuthorizationResponseHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Export ("addTimeISOHeader:length:"), Internal]
		OBEXError addTimeISOHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Export ("addTypeHeader:")]
		OBEXError AddTypeHeader (string type);

		[Export ("addLengthHeader:")]
		OBEXError AddLengthHeader (uint32_t length);

		[Export ("addTime4ByteHeader:")]
		OBEXError AddTime4ByteHeader (uint32_t time4Byte);

		[Export ("addCountHeader:")]
		OBEXError AddCountHeader (uint32_t inCount);

		[Export ("addDescriptionHeader:")]
		OBEXError AddDescriptionHeader (string inDescriptionString);

		[Export ("addNameHeader:")]
		OBEXError AddNameHeader (string inNameString);

		[Export ("addUserDefinedHeader:length:"), Internal]
		OBEXError addUserDefinedHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Export ("addImageHandleHeader:")]
		OBEXError AddImageHandleHeader (string type);

		[Export ("addImageDescriptorHeader:length:"), Internal]
		OBEXError addImageDescriptorHeader (IntPtr inHeaderData, uint32_t inHeaderDataLength);

		[Static]
		[Export ("withOBEXHeadersData:headersDataSize:"), Internal]
		[Obsolete ("Deprecated in OS X 7.0")]
		NSMutableDictionary createWithOBEXHeadersData (IntPtr inHeadersData, size_t inDataSize);

	}

	[BaseType (typeof (NSObject))]
	interface OBEXFileTransferServices {
	}

	[BaseType (typeof (NSObject))]
	interface OBEXSession {
		[Export ("OBEXConnect:maxPacketLength:optionalHeaders:optionalHeadersLength:eventSelector:selectorTarget:refCon:")]
		OBEXError Connect (OBEXFlags inFlags, OBEXMaxPacketLength inMaxPacketLength, IntPtr inOptionalHeaders, size_t inOptionalHeadersLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("OBEXDisconnect:optionalHeadersLength:eventSelector:selectorTarget:refCon:")]
		OBEXError Disconnect (IntPtr inOptionalHeaders, size_t inOptionalHeadersLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("OBEXPut:headersData:headersDataLength:bodyData:bodyDataLength:eventSelector:selectorTarget:refCon:")]
		OBEXError Put (Boolean isFinalChunk, IntPtr inHeadersData, size_t inHeadersDataLength, IntPtr inBodyData, size_t inBodyDataLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("OBEXGet:headers:headersLength:eventSelector:selectorTarget:refCon:")]
		OBEXError Get (Boolean isFinalChunk, IntPtr inHeaders, size_t inHeadersLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("OBEXAbort:optionalHeadersLength:eventSelector:selectorTarget:refCon:")]
		OBEXError Abort (IntPtr inOptionalHeaders, size_t inOptionalHeadersLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("OBEXSetPath:constants:optionalHeaders:optionalHeadersLength:eventSelector:selectorTarget:refCon:")]
		OBEXError SetPath (OBEXFlags inFlags, OBEXConstants inConstants, IntPtr inOptionalHeaders, size_t inOptionalHeadersLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("OBEXConnectResponse:flags:maxPacketLength:optionalHeaders:optionalHeadersLength:eventSelector:selectorTarget:refCon:")]
		OBEXError ConnectResponse (OBEXOpCode inResponseOpCode, OBEXFlags inFlags, OBEXMaxPacketLength inMaxPacketLength, IntPtr inOptionalHeaders, size_t inOptionalHeadersLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("OBEXDisconnectResponse:optionalHeaders:optionalHeadersLength:eventSelector:selectorTarget:refCon:")]
		OBEXError DisconnectResponse (OBEXOpCode inResponseOpCode, IntPtr inOptionalHeaders, size_t inOptionalHeadersLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("OBEXPutResponse:optionalHeaders:optionalHeadersLength:eventSelector:selectorTarget:refCon:")]
		OBEXError PutResponse (OBEXOpCode inResponseOpCode, IntPtr inOptionalHeaders, size_t inOptionalHeadersLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("OBEXGetResponse:optionalHeaders:optionalHeadersLength:eventSelector:selectorTarget:refCon:")]
		OBEXError GetResponse (OBEXOpCode inResponseOpCode, IntPtr inOptionalHeaders, size_t inOptionalHeadersLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("OBEXAbortResponse:optionalHeaders:optionalHeadersLength:eventSelector:selectorTarget:refCon:")]
		OBEXError AbortResponse (OBEXOpCode inResponseOpCode, IntPtr inOptionalHeaders, size_t inOptionalHeadersLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("OBEXSetPathResponse:optionalHeaders:optionalHeadersLength:eventSelector:selectorTarget:refCon:")]
		OBEXError SetPathResponse (OBEXOpCode inResponseOpCode, IntPtr inOptionalHeaders, size_t inOptionalHeadersLength, Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("getAvailableCommandPayloadLength:")]
		OBEXMaxPacketLength GetAvailableCommandPayloadLength (OBEXOpCode inOpCode);

		[Export ("getAvailableCommandResponsePayloadLength:")]
		OBEXMaxPacketLength GetAvailableCommandResponsePayloadLength (OBEXOpCode inOpCode);

		[Export ("getMaxPacketLength")]
		OBEXMaxPacketLength MaxPacketLength { get; }

		[Export ("hasOpenOBEXConnection")]
		bool HasOpenConnection { get; }

		[Export ("setEventCallback:")]
		void SetEventCallback (OBEXSessionEventCallback inEventCallback);

		[Export ("setEventRefCon:")]
		void SetEventRefCon (IntPtr inRefCon);

		[Export ("setEventSelector:target:refCon:")]
		void SetEventSelector (Selector inEventSelector, NSObject inEventSelectorTarget, NSObject inUserRefCon);

		[Export ("serverHandleIncomingData:")]
		void ServerHandleIncomingData (OBEXTransportEvent @event);

		[Export ("clientHandleIncomingData:")]
		void ClientHandleIncomingData (OBEXTransportEvent @event);

		[Export ("sendDataToTransport:dataLength:")]
		OBEXError SendDataToTransport (IntPtr inDataToSend, size_t inDataLength);

		[Export ("openTransportConnection:selectorTarget:refCon:")]
		OBEXError OpenTransportConnection (Selector inSelector, NSObject inTarget, IntPtr inUserRefCon);

		[Export ("hasOpenTransportConnection")]
		Boolean HasOpenTransportConnection { get; }

		[Export ("closeTransportConnection")]
		OBEXError CloseTransportConnection ();

	}
}