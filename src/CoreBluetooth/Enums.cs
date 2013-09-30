//
// Enums.cs
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

namespace MonoMac.CoreBluetooth
{
	public enum CBCentralManagerState {
		/// <summary>
		/// State unknown, update imminent.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// The connection with the system service was momentarily lost, update imminent.
		/// </summary>
		Resetting,

		/// <summary>
		/// The platform doesn't support Bluetooth Low Energy.
		/// </summary>
		Unsupported,

		/// <summary>
		/// The app is not authorized to use Bluetooth Low Energy.
		/// </summary>
		Unauthorized,

		/// <summary>
		/// Bluetooth is currently powered off.
		/// </summary>
		PoweredOff,

		/// <summary>
		/// Bluetooth is currently powered on and available to use.
		/// </summary>
		PoweredOn,
	}

	[Flags]
	public enum CBCharacteristicProperties {
		Broadcast                 = 0x01,
		Read                      = 0x02,
		WriteWithoutResponse      = 0x04,
		Write                     = 0x08,
		Notify                    = 0x10,
		Indicate                  = 0x20,
		AuthenticatedSignedWrites = 0x40,
		ExtendedProperties        = 0x80,
	}

	public enum CBATTError {
		InvalidHandle                 = 0x01,
		ReadNotPermitted              = 0x02,
		WriteNotPermitted             = 0x03,
		InvalidPdu                    = 0x04,
		InsufficientAuthentication    = 0x05,
		RequestNotSupported	          = 0x06,
		InvalidOffset                 = 0x07,
		InsufficientAuthorization     = 0x08,
		PrepareQueueFull              = 0x09,
		AttributeNotFound             = 0x0A,
		AttributeNotLong              = 0x0B,
		InsufficientEncryptionKeySize = 0x0C,
		InvalidAttributeValueLength   = 0x0D,
		UnlikelyError                 = 0x0E,
		InsufficientEncryption        = 0x0F,
		UnsupportedGroupType          = 0x10,
		InsufficientResources         = 0x11,
	}

	/// <summary>
	/// Specifies which type of write is to be performed on a CBCharacteristic.
	/// </summary>
	public enum CBCharacteristicWriteType {
		CBCharacteristicWriteWithResponse = 0,
		CBCharacteristicWriteWithoutResponse,
	}
}

