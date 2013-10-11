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
	public static class NSDictionaryOBEXExtensions
	{
		const string selDictionaryWithOBEXHeadersDataHeadersDataSize_ = "dictionaryWithOBEXHeadersData:headersDataSize:";
		static readonly IntPtr selDictionaryWithOBEXHeadersDataHeadersDataSize_Handle = Selector.GetHandle (selDictionaryWithOBEXHeadersDataHeadersDataSize_);
		const string selDictionaryWithOBEXHeadersData_ = "dictionaryWithOBEXHeadersData:";
		static readonly IntPtr selDictionaryWithOBEXHeadersData_Handle = Selector.GetHandle (selDictionaryWithOBEXHeadersData_);
		const string selGetHeaderBytes = "getHeaderBytes";
		static readonly IntPtr selGetHeaderBytesHandle = Selector.GetHandle (selGetHeaderBytes);
		const string selAddTargetHeaderLength_ = "addTargetHeader:length:";
		static readonly IntPtr selAddTargetHeaderLength_Handle = Selector.GetHandle (selAddTargetHeaderLength_);
		const string selAddHTTPHeaderLength_ = "addHTTPHeader:length:";
		static readonly IntPtr selAddHTTPHeaderLength_Handle = Selector.GetHandle (selAddHTTPHeaderLength_);
		const string selAddBodyHeaderLengthEndOfBody_ = "addBodyHeader:length:endOfBody:";
		static readonly IntPtr selAddBodyHeaderLengthEndOfBody_Handle = Selector.GetHandle (selAddBodyHeaderLengthEndOfBody_);
		const string selAddWhoHeaderLength_ = "addWhoHeader:length:";
		static readonly IntPtr selAddWhoHeaderLength_Handle = Selector.GetHandle (selAddWhoHeaderLength_);
		const string selAddConnectionIDHeaderLength_ = "addConnectionIDHeader:length:";
		static readonly IntPtr selAddConnectionIDHeaderLength_Handle = Selector.GetHandle (selAddConnectionIDHeaderLength_);
		const string selAddApplicationParameterHeaderLength_ = "addApplicationParameterHeader:length:";
		static readonly IntPtr selAddApplicationParameterHeaderLength_Handle = Selector.GetHandle (selAddApplicationParameterHeaderLength_);
		const string selAddByteSequenceHeaderLength_ = "addByteSequenceHeader:length:";
		static readonly IntPtr selAddByteSequenceHeaderLength_Handle = Selector.GetHandle (selAddByteSequenceHeaderLength_);
		const string selAddObjectClassHeaderLength_ = "addObjectClassHeader:length:";
		static readonly IntPtr selAddObjectClassHeaderLength_Handle = Selector.GetHandle (selAddObjectClassHeaderLength_);
		const string selAddAuthorizationChallengeHeaderLength_ = "addAuthorizationChallengeHeader:length:";
		static readonly IntPtr selAddAuthorizationChallengeHeaderLength_Handle = Selector.GetHandle (selAddAuthorizationChallengeHeaderLength_);
		const string selAddAuthorizationResponseHeaderLength_ = "addAuthorizationResponseHeader:length:";
		static readonly IntPtr selAddAuthorizationResponseHeaderLength_Handle = Selector.GetHandle (selAddAuthorizationResponseHeaderLength_);
		const string selAddTimeISOHeaderLength_ = "addTimeISOHeader:length:";
		static readonly IntPtr selAddTimeISOHeaderLength_Handle = Selector.GetHandle (selAddTimeISOHeaderLength_);
		const string selAddTypeHeader_ = "addTypeHeader:";
		static readonly IntPtr selAddTypeHeader_Handle = Selector.GetHandle (selAddTypeHeader_);
		const string selAddLengthHeader_ = "addLengthHeader:";
		static readonly IntPtr selAddLengthHeader_Handle = Selector.GetHandle (selAddLengthHeader_);
		const string selAddTime4ByteHeader_ = "addTime4ByteHeader:";
		static readonly IntPtr selAddTime4ByteHeader_Handle = Selector.GetHandle (selAddTime4ByteHeader_);
		const string selAddCountHeader_ = "addCountHeader:";
		static readonly IntPtr selAddCountHeader_Handle = Selector.GetHandle (selAddCountHeader_);
		const string selAddDescriptionHeader_ = "addDescriptionHeader:";
		static readonly IntPtr selAddDescriptionHeader_Handle = Selector.GetHandle (selAddDescriptionHeader_);
		const string selAddNameHeader_ = "addNameHeader:";
		static readonly IntPtr selAddNameHeader_Handle = Selector.GetHandle (selAddNameHeader_);
		const string selAddUserDefinedHeaderLength_ = "addUserDefinedHeader:length:";
		static readonly IntPtr selAddUserDefinedHeaderLength_Handle =
			Selector.GetHandle (selAddUserDefinedHeaderLength_);
		const string selAddImageHandleHeader_ = "addImageHandleHeader:";
		static readonly IntPtr selAddImageHandleHeader_Handle =
			Selector.GetHandle (selAddImageHandleHeader_);
		const string selAddImageDescriptorHeaderLength_ =
			"addImageDescriptorHeader:length:";
		static readonly IntPtr selAddImageDescriptorHeaderLength_Handle =
			Selector.GetHandle (selAddImageDescriptorHeaderLength_);
		const string selWithOBEXHeadersDataHeadersDataSize_ =
			"withOBEXHeadersData:headersDataSize:";
		static readonly IntPtr selWithOBEXHeadersDataHeadersDataSize_Handle =
			Selector.GetHandle (selWithOBEXHeadersDataHeadersDataSize_);

		static readonly IntPtr class_ptr = Class.GetHandle ("NSMutableDictionary");

		public static NSMutableDictionary CreateDictionaryWithOBEXHeadersData (byte[] data)
		{
			var gcHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
			var dataPointer = gcHandle.AddrOfPinnedObject ();
			var result = (NSMutableDictionary) Runtime.GetNSObject
				(Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr
				 (class_ptr,
				 selDictionaryWithOBEXHeadersDataHeadersDataSize_Handle,
				 dataPointer, (IntPtr)data.Length));
			gcHandle.Free ();
			return result;
		}

		public static NSMutableDictionary CreateDictionaryWithOBEXHeadersData (NSData inHeadersData)
		{
			if (inHeadersData == null)
				throw new ArgumentNullException ("inHeadersData");
			return (NSMutableDictionary) Runtime.GetNSObject (MonoMac.ObjCRuntime.Messaging.IntPtr_objc_msgSend_IntPtr (class_ptr, selDictionaryWithOBEXHeadersData_Handle, inHeadersData.Handle));
		}

		public static NSMutableData GetHeaderBytes (this NSMutableDictionary dictionary)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			return (NSMutableData) Runtime.GetNSObject (MonoMac.ObjCRuntime.Messaging.IntPtr_objc_msgSend (dictionary.Handle, selGetHeaderBytesHandle));
		}

		public static OBEXError AddTargetHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddTargetHeaderLength_Handle);
		}

		public static OBEXError AddHTTPHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddHTTPHeaderLength_Handle);
		}

		public static OBEXError AddBodyHeader (this NSMutableDictionary dictionary, byte[] data, bool isEndOfBody)
		{
			return AddHeader (dictionary, data, selAddBodyHeaderLengthEndOfBody_Handle);
		}

		public static OBEXError AddWhoHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddWhoHeaderLength_Handle);
		}

		public static OBEXError AddConnectionIDHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddConnectionIDHeaderLength_Handle);
		}

		public static OBEXError AddApplicationParameterHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddApplicationParameterHeaderLength_Handle);
		}

		public static OBEXError AddByteSequenceHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddByteSequenceHeaderLength_Handle);
		}

		public static OBEXError AddObjectClassHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddObjectClassHeaderLength_Handle);
		}

		public static OBEXError AddAuthorizationChallengeHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddAuthorizationChallengeHeaderLength_Handle);
		}

		public static OBEXError AddAuthorizationResponseHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddAuthorizationResponseHeaderLength_Handle);
		}

		public static OBEXError AddTimeISOHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddTimeISOHeaderLength_Handle);
		}

		public static OBEXError AddTypeHeader (this NSMutableDictionary dictionary, string type)
		{
			return AddHeader (dictionary, type, selAddTypeHeader_Handle);
		}

		public static OBEXError AddLengthHeader (this NSMutableDictionary dictionary, int length)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			return (OBEXError) MonoMac.ObjCRuntime.Messaging.int_objc_msgSend_int (dictionary.Handle, selAddLengthHeader_Handle, length);
		}

		public static OBEXError AddTime4ByteHeader (this NSMutableDictionary dictionary, byte[] time4Byte)
		{
			if (time4Byte == null)
				throw new ArgumentNullException ("time4Byte");
			if (time4Byte.Length != 4)
				throw new ArgumentException ("Must be 4 bytes", "time4Byte");
			return AddTime4ByteHeader (dictionary, BitConverter.ToInt32 (time4Byte, 0));
		}

		public static OBEXError AddTime4ByteHeader (this NSMutableDictionary dictionary, int time4Byte)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			return (OBEXError) MonoMac.ObjCRuntime.Messaging.int_objc_msgSend_int (dictionary.Handle, selAddTime4ByteHeader_Handle, time4Byte);
		}

		public static OBEXError AddCountHeader (this NSMutableDictionary dictionary, int inCount)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			return (OBEXError) MonoMac.ObjCRuntime.Messaging.int_objc_msgSend_int (dictionary.Handle, selAddCountHeader_Handle, inCount);
		}

		public static OBEXError AddDescriptionHeader (this NSMutableDictionary dictionary, string inDescriptionString)
		{
			return AddHeader (dictionary, inDescriptionString, selAddDescriptionHeader_Handle);
		}

		public static OBEXError AddNameHeader (this NSMutableDictionary dictionary, string inNameString)
		{
			return AddHeader (dictionary, inNameString, selAddNameHeader_Handle);
		}

		public static OBEXError AddUserDefinedHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddUserDefinedHeaderLength_Handle);
		}

		public static OBEXError AddImageHandleHeader (this NSMutableDictionary dictionary, string type)
		{
			return AddHeader (dictionary, type, selAddImageHandleHeader_Handle);
		}

		public static OBEXError AddImageDescriptorHeader (this NSMutableDictionary dictionary, byte[] data)
		{
			return AddHeader (dictionary, data, selAddImageDescriptorHeaderLength_Handle);
		}

		[Obsolete ("Deprecated in OS X 7.0", false)]
		public static NSMutableDictionary CreateWithOBEXHeadersData (byte[] data)
		{
			var gcHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
			var dataPointer = gcHandle.AddrOfPinnedObject ();
			var result = (NSMutableDictionary) Runtime.GetNSObject
				(Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr
				 (class_ptr,
				 selWithOBEXHeadersDataHeadersDataSize_Handle,
				 dataPointer, (IntPtr)data.Length));
			gcHandle.Free ();
			return result;
		}

		static OBEXError AddHeader (NSMutableDictionary dictionary, byte[] data, IntPtr selectorHandle)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			if (data == null)
				throw new ArgumentNullException ("data");
			var gcHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
			var dataPointer = gcHandle.AddrOfPinnedObject ();
			var result = (OBEXError) MonoMac.ObjCRuntime.Messaging.int_objc_msgSend_IntPtr_int
				(dictionary.Handle,
				 selectorHandle,
				 dataPointer,
				 data.Length);
			gcHandle.Free ();
			return result;
		}

		static OBEXError AddHeader (NSMutableDictionary dictionary, string data, IntPtr selectorHandle)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			if (data == null)
				throw new ArgumentNullException ("data");
			var dataHandle = NSString.CreateNative (data);
			var result = (OBEXError) MonoMac.ObjCRuntime.Messaging.int_objc_msgSend_IntPtr
				(dictionary.Handle,
				 selectorHandle,
				 dataHandle);
			NSString.ReleaseNative (dataHandle);
			return result;
		}
	}
}

