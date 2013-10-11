//
// Bluetooth.cs
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
using System.Runtime.InteropServices;
using System.Text;
using MonoMac.Foundation;
using MonoMac.IOKit;
using MonoMac.ObjCRuntime;

using BluetoothConnectionHandle = System.UInt16;
using BluetoothHCILinkQuality = System.Byte;
using BluetoothHCIRSSIValue = System.SByte;
using IOItemCount = System.UInt32;
using OBEXConstants = System.Byte;
using OBEXFlags = System.Byte;
using OBEXMaxPacketLength = System.UInt32;
using OBEXOpCode = System.Byte;
using OBEXSessionRef = System.IntPtr;
using size_t = System.UIntPtr;

namespace MonoMac.IOBluetooth
{
#if !COREBUILD
	public static class IOBluetooth
	{
		public const ushort ConnectionHandleNone = 0xffff;

		public static ushort GetSlotsFromSeconds(this double inSeconds)
		{
			return (ushort)(inSeconds / .000625);
		}

		public static double GetSecondsFromSlots(this ushort inSlots )
		{
			return inSlots * .000625;
		}

		[DllImport (Constants.IOBluetoothLibrary)]
		extern static IOReturn IOBluetoothNSStringToDeviceAddress (IntPtr inNameString, out BluetoothDeviceAddress outDeviceAddress);

		public static BluetoothDeviceAddress ToDeviceAddress (this NSString nameString)
		{
			BluetoothDeviceAddress deviceAddress;
			var result = IOBluetoothNSStringToDeviceAddress (nameString.Handle, out deviceAddress);
			IOObject.ThrowIfError (result);
			return deviceAddress;
		}

		public static BluetoothDeviceAddress ToDeviceAddress (this string nameString)
		{
			using (new NSString (nameString))
				return nameString.ToDeviceAddress ();
		}

		[DllImport (Constants.IOBluetoothLibrary)]
		extern static IntPtr IOBluetoothNSStringFromDeviceAddress ([MarshalAs (UnmanagedType.LPStruct)] BluetoothDeviceAddress deviceAddress);

		public static NSString ToNSString (this BluetoothDeviceAddress deviceAddress)
		{
			return (NSString)Runtime.GetNSObject(IOBluetoothNSStringFromDeviceAddress (deviceAddress));
		}

		[DllImport (Constants.IOBluetoothLibrary)]
		extern static Boolean IOBluetoothIsFileAppleDesignatedPIMData (NSString inFileName);

		public static bool IsFileAppleDesignatedPIMData (this NSString fileName)
		{
			return IOBluetoothIsFileAppleDesignatedPIMData (fileName);
		}

		[DllImport (Constants.IOBluetoothLibrary, EntryPoint = "IOBluetoothGetUniqueFileNameAndPath")]
		public extern static NSString GetUniqueFileNameAndPath (NSString name, NSString path);

		public static string GetUniqueFileNameAndPath (string name, string path)
		{
			using (var nameString = new NSString(name))
			using (var pathString = new NSString(path))
			using (var result = GetUniqueFileNameAndPath (nameString, pathString))
				return result.ToString ();
		}

		[DllImport (Constants.IOBluetoothLibrary, EntryPoint = "IOBluetoothLaunchHandsFreeAgent")]
		public extern static Boolean TryLaunchHandsFreeAgent (NSString deviceAddressString);

		public static bool TryLaunchHandsFreeAgent(string deviceAddressString)
		{
			using (var deviceAddress = new NSString (deviceAddressString))
				return TryLaunchHandsFreeAgent (deviceAddress);
		}

		// dont' know how to pinvoke variable arguments

//		extern long		IOBluetoothPackData( void *ioBuffer, const char *inFormat, ... );
//		extern long		IOBluetoothPackDataList( void *ioBuffer, const char *inFormat, va_list inArgs );

//		extern long		IOBluetoothUnpackData( ByteCount inBufferSize, const void *inBuffer, const char *inFormat, ... );
//		extern long		IOBluetoothUnpackDataList( ByteCount inBufferSize, const void *inBuffer, const char *inFormat, va_list inArgs );

		[DllImport (Constants.IOBluetoothLibrary)]
		extern static long IOBluetoothNumberOfAvailableHIDDevices ();

		public static long AvailableHIDDevicesCount {
			get { return IOBluetoothNumberOfAvailableHIDDevices (); }
		}

		[DllImport (Constants.IOBluetoothLibrary)]
		extern static long IOBluetoothNumberOfPointingHIDDevices ();

		public static long PointingHIDDevicesCount {
			get { return IOBluetoothNumberOfPointingHIDDevices (); }
		}

		[DllImport (Constants.IOBluetoothLibrary)]
		extern static long IOBluetoothNumberOfKeyboardHIDDevices ();

		public static long KeyboardHIDDevicesCount {
			get { return IOBluetoothNumberOfKeyboardHIDDevices (); }
		}

		[DllImport (Constants.IOBluetoothLibrary)]
		extern static long IOBluetoothNumberOfTabletHIDDevices ();

		public static long TabletHIDDevicesCount {
			get { return IOBluetoothNumberOfTabletHIDDevices (); }
		}

		[DllImport (Constants.IOBluetoothLibrary, EntryPoint = "IOBluetoothFindNumberOfRegistryEntriesOfClassName")]
		public extern static long GetRegistryEntriesCount (string className);
	}
#endif

	[StructLayout (LayoutKind.Sequential)]
	public class IOBluetoothDeviceSearchDeviceAttributes
	{
		BluetoothDeviceAddress address;
		[MarshalAs (UnmanagedType.LPArray, SizeConst = 256)]
		/*BluetoothDeviceName*/ byte[] name;
		BluetoothServiceClassMajor serviceClassMajor;
		BluetoothDeviceClassMajor deviceClassMajor;
		BluetoothDeviceClassMinor deviceClassMinor;

		public IOBluetoothDeviceSearchDeviceAttributes (
			BluetoothDeviceAddress address,
			string name,
			BluetoothServiceClassMajor serviceClassMajor,
			BluetoothDeviceClassMajor deviceClassMajor,
			BluetoothDeviceClassMinor deviceClassMinor)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (Encoding.UTF8.GetByteCount (name) > 256)
				throw new ArgumentOutOfRangeException ("name is too long");

			this.address = address;
			this.name = new byte[256];
			Encoding.UTF8.GetBytes (name, 0, name.Length, this.name, 0);
			this.serviceClassMajor = serviceClassMajor;
			this.deviceClassMajor = deviceClassMajor;
			this.deviceClassMinor = deviceClassMinor;
		}

		public BluetoothDeviceAddress Address { get { return address; } }

		public string Name { get { return Encoding.UTF8.GetString (name); } }

		public BluetoothServiceClassMajor ServiceClassMajor {
			get { return serviceClassMajor; }
		}

		public BluetoothDeviceClassMajor DeviceClassMajor {
			get { return deviceClassMajor; }
		}

		public BluetoothDeviceClassMinor DeviceClassMinor {
			get { return deviceClassMinor; }
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public class IOBluetoothDeviceSearchAttributes
	{
		IOBluetoothDeviceSearchOptions options;
		IOItemCount maxResults;
		IOItemCount deviceAttributeCount;
		/*IOBluetoothDeviceSearchDeviceAttributes*/ IntPtr attributeList;

		public IOBluetoothDeviceSearchAttributes (
			IOBluetoothDeviceSearchOptions options = IOBluetoothDeviceSearchOptions.None,
			uint maxResults = 0,
			IOBluetoothDeviceSearchDeviceAttributes[] attributes = null)
		{
			this.options = options;
			this.maxResults = maxResults;
			if (attributes == null) {
				this.deviceAttributeCount = 0;
				this.attributeList = IntPtr.Zero;
			} else {
				this.deviceAttributeCount = (uint)attributes.Length;
				int attributeSize = Marshal.SizeOf (typeof(IOBluetoothDeviceSearchDeviceAttributes));
				var pos = this.attributeList = Marshal.AllocHGlobal (attributeSize * attributes.Length);
				foreach (var item in attributes) {
					Marshal.StructureToPtr (item, pos, false);
					pos += attributeSize;
				}
			}
		}

		~IOBluetoothDeviceSearchAttributes ()
		{
			if (attributeList != IntPtr.Zero)
				Marshal.FreeHGlobal (attributeList);
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct BluetoothClassOfDevice
	{
		uint bluetoothClassOfDevice;

		public BluetoothDeviceClassMajor DeviceClassMajor {
			get { return (BluetoothDeviceClassMajor)((bluetoothClassOfDevice & 0x00001F00) >> 8); }
		}

		public BluetoothDeviceClassMinor DeviceClassMinor {
			get { return (BluetoothDeviceClassMinor)((bluetoothClassOfDevice & 0x000000FC) >> 2); }
		}

		public BluetoothServiceClassMajor ServiceClassMajor {
			get { return (BluetoothServiceClassMajor)((bluetoothClassOfDevice & 0x00FFE000) >> 13); }
		}

		public BluetoothClassOfDevice (BluetoothServiceClassMajor inServiceClassMajor, BluetoothDeviceClassMajor inDeviceClassMajor, BluetoothDeviceClassMinor inDeviceClassMinor)
		{
			bluetoothClassOfDevice = (((uint)inServiceClassMajor << 13) & 0x00FFE000) | (((uint)inDeviceClassMajor << 8) & 0x00001F00) | (((uint)inDeviceClassMinor << 2) & 0x000000FC);
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct BluetoothDeviceAddress
	{
		[MarshalAs (UnmanagedType.ByValArray, SizeConst = 6)]
		byte[] data;

		public byte[] Data { get { return data; } }

		public BluetoothDeviceAddress (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length != 6)
				throw new ArgumentException ("Address must be 6 bytes.", "data");
			this.data = data;
		}
#if !COREBUILD
		public override string ToString ()
		{
			using (var nameString = this.ToNSString ())
				return nameString.ToString ();
		}
#endif
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct BluetoothPINCode
	{
		[MarshalAs (UnmanagedType.LPArray, SizeConst = 16)]
		byte[] data;

		public byte[] Data { get { return data; } }

		public BluetoothPINCode (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");
			if (data.Length != 16)
				throw new ArgumentException ("PIN code must be 16 bytes.", "data");
			this.data = data;
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct BluetoothHCIRSSIInfo
	{
		BluetoothConnectionHandle		handle;
		BluetoothHCIRSSIValue			RSSIValue;
	}

	public struct BluetoothHCILinkQualityInfo
	{
		BluetoothConnectionHandle		handle;
		BluetoothHCILinkQuality			qualityValue;
	}

	/// <remarks>
	/// You will need to construcy these when data is received, and then pass a pointer to it to one of the
	/// incoming data methods defined below. Pass 0 as your status if data was received OK. Otherwise, you can
	/// put your own error code in there. For the transport type, be sure to use one of the defined types above.
	/// </remarks>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXTransportEvent
	{	
		OBEXTransportEventType	type;
		OBEXError				status;	
		IntPtr					dataPtr;
		UIntPtr					dataLength;
	}

	/// <summary>
	/// When a new session event occurs, your selector (or C callback) will be given an OBEXSessionEvent pointer,
	/// and in it will be information you might find interesting so that you can then reply back appropriately.
	/// For example, of you receive a kOBEXSessionEventTypeConnectCommandResponseReceived event, you can then
	/// parse out the information related to that event, and if all looks well to you, you could them send a
	/// "Get" command to get a file off of the OBEX server you just connected to.
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXSessionEvent
	{
		OBEXSessionEventType	type;
		OBEXSessionRef			session;
		IntPtr					refCon;
		Boolean					isEndOfEventData;
		IntPtr					reserved1;
		IntPtr					reserved2;
		OBEXCommandResponseDataUnion u;

		[StructLayout (LayoutKind.Explicit)]
		struct OBEXCommandResponseDataUnion
		{
			// Client session events.

			[FieldOffset (0)]
			OBEXConnectCommandResponseData			connectCommandResponseData;
			[FieldOffset (0)]
			OBEXDisconnectCommandResponseData		disconnectCommandResponseData;
			[FieldOffset (0)]
			OBEXPutCommandResponseData				putCommandResponseData;
			[FieldOffset (0)]
			OBEXGetCommandResponseData				getCommandResponseData;
			[FieldOffset (0)]
			OBEXSetPathCommandResponseData			setPathCommandResponseData;
			[FieldOffset (0)]
			OBEXAbortCommandResponseData			abortCommandResponseData;

			// Server session events.
			
			[FieldOffset (0)]
			OBEXConnectCommandData					connectCommandData;
			[FieldOffset (0)]
			OBEXDisconnectCommandData				disconnectCommandData;
			[FieldOffset (0)]
			OBEXPutCommandData						putCommandData;
			[FieldOffset (0)]
			OBEXGetCommandData						getCommandData;
			[FieldOffset (0)]
			OBEXSetPathCommandData					setPathCommandData;
			[FieldOffset (0)]
			OBEXAbortCommandData					abortCommandData;

			// Client & Server Session events.
			
			[FieldOffset (0)]
			OBEXErrorData							errorData;
		}
	}

	//	OBEX Session Types

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is of type
	/// kOBEXSessionEventTypeConnectCommandResponseReceived (see OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXConnectCommandResponseData
	{
		OBEXOpCode			serverResponseOpCode;
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
		OBEXMaxPacketLength	maxPacketSize;
		OBEXVersion			version;
		OBEXFlags			flags;
	}

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypeDisconnectCommandResponseReceived (see
	/// OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXDisconnectCommandResponseData
	{
		OBEXOpCode			serverResponseOpCode;
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
	}

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypePutCommandResponseReceived (see OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXPutCommandResponseData
	{
		OBEXOpCode			serverResponseOpCode;
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
	}

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypeGetCommandResponseReceived (see OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXGetCommandResponseData
	{
		OBEXOpCode			serverResponseOpCode;
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
	}

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypeSetPathCommandResponseReceived (see
	/// OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXSetPathCommandResponseData
	{
		OBEXOpCode			serverResponseOpCode;
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
		OBEXFlags			flags;
		OBEXConstants		constants;
	}

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypeAbortCommandResponseReceived (see
	/// OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXAbortCommandResponseData
	{
		OBEXOpCode			serverResponseOpCode;
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
	}

	//	Server Session Types

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypeConnectCommandReceived (see
	/// OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXConnectCommandData
	{
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
		OBEXMaxPacketLength	maxPacketSize;
		OBEXVersion			version;
		OBEXFlags			flags;
	}

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypeDisconnectCommandReceived (see OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXDisconnectCommandData
	{
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
	}

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypePutCommandReceived (see OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXPutCommandData
	{
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
		size_t				bodyDataLeftToSend;
	}

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypeGetCommandReceived (see OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXGetCommandData
	{
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
	}

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypeSetPathCommandReceived (see OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXSetPathCommandData
	{
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
		OBEXFlags			flags;
		OBEXConstants		constants;
	}

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypeAbortCommandReceived (see OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXAbortCommandData
	{
		IntPtr 				headerDataPtr;
		size_t 				headerDataLength;
	}

	/// <summary>
	/// Part of the OBEXSessionEvent structure. Is readable when the event is
	/// of type kOBEXSessionEventTypeError (see OBEXSessionEventTypes).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct OBEXErrorData
	{
		OBEXError			error;
		IntPtr				dataPtr;			// If data was associated with the error, it will be included here if possible.
		size_t				dataLength;			// Check the size to see if there is data to be examined.
	}

	public delegate void OBEXSessionEventCallback (OBEXSessionEvent inEvent);
	public delegate void IOBluetoothOBEXSessionOpenConnectionCallback (OBEXSessionRef session, OBEXError status, IntPtr refCon);
}