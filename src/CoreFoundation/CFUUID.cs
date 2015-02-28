//
// CFUUID.cs
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
using MonoMac.CoreFoundation;
using MonoMac.ObjCRuntime;

using CFAllocatorRef = System.IntPtr;
using CFStringRef = System.IntPtr;
using CFTypeID = System.UInt32;
using CFUUIDRef = System.IntPtr;
using UInt8 = System.Byte;

namespace MonoMac.CoreFoundation
{
	/// <summary>
	/// The CFUUIDBytes struct is a 128-bit struct that contains the
	/// raw UUID.  A CFUUIDRef can provide such a struct from the
	/// CFUUIDGetUUIDBytes() function.  This struct is suitable for
	/// passing to APIs that expect a raw UUID.
	/// </summary>
	public class CFUUID : CFType
	{
		bool isConstant = false;
		Lazy<string> stringValue;
		Lazy<CFUUIDBytes> bytes;

		internal CFUUID (IntPtr handle, bool owns) : base (handle, owns)
		{
			Initalize ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IOKit.CFUUID"/> class with a unique UUID.
		/// </summary>
		public CFUUID ()
		{
			Handle = CFUUIDCreate (NULL);
			if (Handle == IntPtr.Zero)
				throw new Exception ("Failed to create CFUUID");
			Initalize ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IOKit.CFUUID"/> class from the specified bytes.
		/// </summary>
		/// <param name="bytes">Byte array.</param>
		/// <param name="isConstant">When true, the underlying CFUUIDRef will not be released
		/// when this instance is disposed</param>
		/// <exception cref="ArgumentException">Thrown if length of byte array is not 16</exception>
		public CFUUID (byte[] bytes, bool isConstant = false)
		{
			if (bytes.Length != 16)
				throw new ArgumentException ("Length of byte array must be 16", "bytes");
			this.isConstant = isConstant;
			if (isConstant)
				Handle = CFUUIDGetConstantUUIDWithBytes (NULL, bytes [0], bytes [1], bytes [2], bytes [3],
				                                         bytes [4], bytes [5], bytes [6], bytes [7],
				                                         bytes [8], bytes [9], bytes [10], bytes [11],
				                                         bytes [12], bytes [13], bytes [14], bytes [15]);
			else
				Handle = CFUUIDCreateWithBytes (NULL, bytes [0], bytes [1], bytes [2], bytes [3],
				                                bytes [4], bytes [5], bytes [6], bytes [7],
				                                bytes [8], bytes [9], bytes [10], bytes [11],
				                                bytes [12], bytes [13], bytes [14], bytes [15]);
			if (Handle == IntPtr.Zero)
				throw new Exception ("Failed to create CFUUID");
			Initalize ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IOKit.CFUUID"/> class from the specified Guid.
		/// </summary>
		/// <param name="guid">The guid.</param>
		/// <param name="isConstant">When true, the underlying CFUUIDRef will not be released
		/// when this instance is disposed</param>
		public CFUUID (Guid guid, bool isConstant = false)
		{
			var bytes = guid.ToByteArray ();
			this.isConstant = isConstant;
			// System.Guid has reverse byte order from CFUUID
			if (isConstant)
				Handle = CFUUIDGetConstantUUIDWithBytes (NULL, bytes [3], bytes [2], bytes [1], bytes [0],
				                                         bytes [5], bytes [4], bytes [7], bytes [6],
				                                         bytes [8], bytes [9], bytes [10], bytes [11],
				                                         bytes [12], bytes [13], bytes [14], bytes [15]);
			else
				Handle = CFUUIDCreateWithBytes (NULL, bytes [3], bytes [2], bytes [1], bytes [0],
				                                bytes [5], bytes [4], bytes [7], bytes [6],
				                                bytes [8], bytes [9], bytes [10], bytes [11],
				                                bytes [12], bytes [13], bytes [14], bytes [15]);
			if (Handle == IntPtr.Zero)
				throw new Exception ("Failed to create CFUUID");
			Initalize ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IOKit.CFUUID"/> class from the specified Guid.
		/// </summary>
		/// <param name="uuid">The uuid.</param>
		/// <param name="isConstant">When true, the underlying CFUUIDRef will not be released
		/// when this instance is disposed</param>
		/// <exception cref="ArgumentException">Thrown if length of byte array is not 16</exception>
		public CFUUID (string uuid, bool isConstant = false)
			: this (new Guid(uuid), isConstant)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IOKit.CFUUID"/> class from the specified string.
		/// </summary>
		/// <param name="uuid">String.</param>
		public CFUUID (string uuid)
		{
			using (var uuidCFString = new CFString (uuid)) {
				Handle = CFUUIDCreateFromString (NULL, uuidCFString.Handle);
			}
			if (Handle == IntPtr.Zero)
				throw new Exception ("Failed to create CFUUID");
			Initalize ();
		}

		void Initalize ()
		{
			stringValue = new Lazy<string> (GetStringValue);
			bytes = new Lazy<CFUUIDBytes> (() => CFUUIDGetUUIDBytes (Handle));
		}

		~CFUUID ()
		{
			Dispose (false);
		}

		protected override void Dispose (bool disposing)
		{
			if (isConstant) {
				if (Handle != IntPtr.Zero) {
					// constant references are not to be released.
					Unregister ();
					Handle = IntPtr.Zero;
				}
				return;
			}
			base.Dispose (disposing);
		}

		public CFUUIDBytes Bytes { get { return bytes.Value; } }

		public static uint TypeID {
			get { return CFUUIDGetTypeID (); }
		}

		string GetStringValue ()
		{
			var stringRef = CFUUIDCreateString (NULL, Handle);
			using (var cfString = new CFString (stringRef)) {
				Release (stringRef);
				return cfString.ToString ();
			}
		}

		public override string ToString ()
		{
			return stringValue.Value;
		}

		public byte[] ToByteArray ()
		{
			var newArray = new byte[16];
			Array.Copy (bytes.Value.bytes, newArray, 16);
			return newArray;
		}

		public Guid ToGuid ()
		{
			return new Guid (ToString ());
		}

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		extern static CFTypeID CFUUIDGetTypeID ();

		/// <summary>
		/// Create and return a brand new unique identifier
		/// </summary>
		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		extern static CFUUIDRef CFUUIDCreate (CFAllocatorRef alloc);

		/// <summary>
		/// Create and return an identifier with the given contents.
		/// This may return an existing instance with its ref count bumped because of uniquing.
		/// </summary>
		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		extern static CFUUIDRef CFUUIDCreateWithBytes (CFAllocatorRef alloc,
		                                              UInt8 byte0, UInt8 byte1, UInt8 byte2, UInt8 byte3,
		                                              UInt8 byte4, UInt8 byte5, UInt8 byte6, UInt8 byte7,
		                                              UInt8 byte8, UInt8 byte9, UInt8 byte10, UInt8 byte11,
		                                              UInt8 byte12, UInt8 byte13, UInt8 byte14, UInt8 byte15);

		/// <summary>
		/// Converts from a string representation to the UUID.
		/// This may return an existing instance with its ref count bumped because of uniquing.
		/// </summary>
		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		extern static CFUUIDRef CFUUIDCreateFromString (CFAllocatorRef alloc, CFStringRef uuidStr);

		/// <summary>
		/// Converts from a UUID to its string representation.
		/// </summary>
		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		extern static CFStringRef CFUUIDCreateString (CFAllocatorRef alloc, CFUUIDRef uuid);

		/// <summary>
		/// This returns an immortal CFUUIDRef that should not be released.
		/// It can be used in headers to declare UUID constants with #define.
		/// </summary>
		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		extern static CFUUIDRef CFUUIDGetConstantUUIDWithBytes (CFAllocatorRef alloc,
		                                                       UInt8 byte0, UInt8 byte1, UInt8 byte2, UInt8 byte3,
		                                                       UInt8 byte4, UInt8 byte5, UInt8 byte6, UInt8 byte7,
		                                                       UInt8 byte8, UInt8 byte9, UInt8 byte10, UInt8 byte11,
		                                                       UInt8 byte12, UInt8 byte13, UInt8 byte14, UInt8 byte15);

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		extern static CFUUIDBytes CFUUIDGetUUIDBytes (CFUUIDRef uuid);

		[DllImport (MonoMac.Constants.CoreFoundationLibrary)]
		extern static CFUUIDRef CFUUIDCreateFromUUIDBytes (CFAllocatorRef alloc, CFUUIDBytes bytes);

		[StructLayout (LayoutKind.Sequential)]
		public struct CFUUIDBytes
		{
			[MarshalAs (UnmanagedType.ByValArray, SizeConst = 16)]
			public UInt8[] bytes;
		}
	}
}

