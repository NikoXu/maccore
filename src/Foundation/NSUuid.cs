//
// NSUuid.cs: support code for the NSUUID binding
//
// Authors:
//    MIguel de Icaza (miguel@xamarin.com)
//
// Copyright 2012-2013 Xamarin Inc
//
using System;
using System.Runtime.InteropServices;
using MonoMac.ObjCRuntime;

namespace MonoMac.Foundation {
	sealed partial class NSUuid {
		static unsafe IntPtr GetIntPtr (byte [] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			if (bytes.Length < 16)
				throw new ArgumentException ("length must be at least 16 bytes");
			
			IntPtr ret;
			fixed (byte *p = &bytes [0]){
				ret = (IntPtr) p;
			}
			return ret;
		}

		unsafe public NSUuid (byte [] bytes) : base (NSObjectFlag.Empty)
		{
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			if (bytes.Length != 16)
				throw new ArgumentException ("bytes must have length of 16.", "bytes");
			IntPtr ptr = GetIntPtr (bytes);

			if (IsDirectBinding) {
				Handle = Messaging.IntPtr_objc_msgSend_IntPtr (this.Handle, Selector.GetHandle ("initWithUUIDBytes:"), ptr);
			} else {
				Handle = Messaging.IntPtr_objc_msgSendSuper_IntPtr (this.SuperHandle, Selector.GetHandle ("initWithUUIDBytes:"), ptr);
			}
			if (Handle == IntPtr.Zero)
				throw new ArgumentException ("Could not create NSUuid from bytes: " + BitConverter.ToString (bytes), "bytes");
		}

		public NSUuid (string str)
			: base (NSObjectFlag.Empty)
		{
			if (str == null)
				throw new ArgumentNullException ("str");
			var nsstr = NSString.CreateNative (str);

			if (IsDirectBinding) {
				Handle = MonoMac.ObjCRuntime.Messaging.IntPtr_objc_msgSend_IntPtr (this.Handle, Selector.GetHandle ("initWithUUIDString:"), nsstr);
			} else {
				Handle = MonoMac.ObjCRuntime.Messaging.IntPtr_objc_msgSendSuper_IntPtr (this.SuperHandle, Selector.GetHandle ("initWithUUIDString:"), nsstr);
			}
			NSString.ReleaseNative (nsstr);
			if (Handle == IntPtr.Zero)
				throw new ArgumentException ("Could not create NSUuid from string: " + str, "str");
		}

		public byte [] GetBytes ()
		{
			byte [] ret = new byte [16];
			
			IntPtr buf = Marshal.AllocHGlobal (16);
			GetUuidBytes (buf);
			Marshal.Copy (buf, ret, 0, 16);
			Marshal.FreeHGlobal (buf);

			return ret;
		}

		public static NSUuid Empty { get { return new NSUuid (new byte[16]); } }

		public override string ToString ()
		{
			return StringValue;
		}

		public override bool Equals (object obj)
		{
			var otherUuid = obj as NSUuid;
			if (otherUuid != null)
				return StringValue == otherUuid.StringValue;
			return base.Equals (obj);
		}

		public override int GetHashCode ()
		{
			return 0;
		}

		public static bool operator == (NSUuid uuid1, NSUuid uuid2)
		{
			if ((object)uuid1 != null)
				return uuid1.Equals (uuid2);
			return (object)uuid2 == null;
		}

		public static bool operator != (NSUuid uuid1, NSUuid uuid2)
		{
			return !(uuid1 == uuid2);
		}
	}
}
