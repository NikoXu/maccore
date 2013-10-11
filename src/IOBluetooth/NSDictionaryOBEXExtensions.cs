//
// NSDictionaryOBEXExtensions.cs
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
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace MonoMac.IOBluetooth
{
	public static partial class NSDictionaryOBEXExtensions
	{
		public static NSMutableDictionary CreateDictionaryWithOBEXHeadersData (byte[] data)
		{
			var gcHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
			var dataPointer = gcHandle.AddrOfPinnedObject ();
			var result = createDictionaryWithOBEXHeadersData (null, dataPointer, (UIntPtr)data.Length);
			gcHandle.Free ();
			return result;
		}

		public static OBEXError AddTargetHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addTargetHeader);
		}

		public static OBEXError AddHTTPHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addHTTPHeader);
		}

		public static OBEXError AddBodyHeader (this NSMutableDictionary dictionary, byte[] data, bool isEndOfBody)
		{	if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			if (data == null)
				throw new ArgumentNullException ("data");
			var gcHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
			var dataPointer = gcHandle.AddrOfPinnedObject ();
			var result = addBodyHeader (dictionary, dataPointer, (uint)data.Length, isEndOfBody);
			gcHandle.Free ();
			return result;
		}

		public static OBEXError AddWhoHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addWhoHeader);
		}

		public static OBEXError AddConnectionIDHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addConnectionIDHeader);
		}

		public static OBEXError AddApplicationParameterHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addApplicationParameterHeader);
		}

		public static OBEXError AddByteSequenceHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addByteSequenceHeader);
		}

		public static OBEXError AddObjectClassHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addObjectClassHeader);
		}

		public static OBEXError AddAuthorizationChallengeHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addAuthorizationChallengeHeader);
		}

		public static OBEXError AddAuthorizationResponseHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addAuthorizationResponseHeader);
		}

		public static OBEXError AddTimeISOHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addTimeISOHeader);
		}

		public static OBEXError AddTime4ByteHeader (this NSMutableDictionary dictionary, byte[] time4Byte)
		{
			if (time4Byte == null)
				throw new ArgumentNullException ("time4Byte");
			if (time4Byte.Length != 4)
				throw new ArgumentException ("Must be 4 bytes", "time4Byte");
			return AddTime4ByteHeader (dictionary, BitConverter.ToUInt32 (time4Byte, 0));
		}

		public static OBEXError AddUserDefinedHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addUserDefinedHeader);
		}

		public static OBEXError AddImageDescriptorHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, addImageDescriptorHeader);
		}

		[Obsolete ("Deprecated in OS X 7.0", false)]
		public static NSMutableDictionary CreateWithOBEXHeadersData (byte[] data)
		{
			var gcHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
			var dataPointer = gcHandle.AddrOfPinnedObject ();
			var result = createWithOBEXHeadersData (null, dataPointer, (UIntPtr)data.Length);
			gcHandle.Free ();
			return result;
		}

		static OBEXError AddHeader (NSMutableDictionary dictionary, byte[] data, Func<NSMutableDictionary, IntPtr, uint, OBEXError> method)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			if (data == null)
				throw new ArgumentNullException ("data");
			var gcHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
			var dataPointer = gcHandle.AddrOfPinnedObject ();
			var result = method (dictionary, dataPointer, (uint)data.Length);
			gcHandle.Free ();
			return result;
		}
	}
}

