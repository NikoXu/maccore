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

using OBEXSessionRef = System.IntPtr;
using size_t = System.UIntPtr;
using OBEXOpCode = System.Byte;
using OBEXMaxPacketLength = System.UInt32;
using OBEXFlags = System.Byte;
using OBEXConstants = System.Byte;
using BluetoothHCIRSSIValue = System.SByte;
using BluetoothHCILinkQuality = System.Byte;

namespace MonoMac.IOBluetooth
{
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
		[MarshalAs (UnmanagedType.LPArray, SizeConst = 6)]
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