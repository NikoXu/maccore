//
// NSFileSecurity.cs:
// Author:
//   DavidLechner
//
// Copyright 2014, David Lechner
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//
using System;
using MonoMac.ObjCRuntime;
using MonoMac.CoreFoundation;

namespace MonoMac.Foundation {
	public partial class NSFileSecurity {

		public NSUuid OwnerUuid {
			get {
				IntPtr uuidRef;
				if (!CFFileSecurity.CFFileSecurityCopyOwnerUUID (Handle, out uuidRef))
					return null;
				var uuid = CFType.GetCFObject<CFUUID> (uuidRef);
				return new NSUuid (uuid.ToString ());;
			}
			set {
				if (value != null) {
					var uuid = new CFUUID (value.StringValue);
					if (!CFFileSecurity.CFFileSecuritySetOwnerUUID (Handle, uuid.Handle))
						throw new Exception ("Failed to set owner uuid.");
				} else {
					ClearProperties (CFFileSecurityClearOptions.OwnerUUID);
				}
			}
		}

		public NSUuid GroupUuid {
			get {
				IntPtr uuidRef;
				if (!CFFileSecurity.CFFileSecurityCopyGroupUUID (Handle, out uuidRef))
					return null;
				var uuid = CFType.GetCFObject<CFUUID> (uuidRef);
				return new NSUuid (uuid.ToString ());
			}
			set {
				if (value != null) {
					var uuid = new CFUUID (value.StringValue);
					if (!CFFileSecurity.CFFileSecuritySetGroupUUID (Handle, uuid.Handle))
						throw new Exception ("Failed to set group uuid.");
				} else {
					ClearProperties (CFFileSecurityClearOptions.GroupUUID);
				}
			}
		}

		public ACL ACL {
			get {
				IntPtr acl;
				if (!CFFileSecurity.CFFileSecurityCopyAccessControlList (Handle, out acl))
					return null;
				return new ACL (acl);
			}
			set {
				if (value != null) {
					if (!CFFileSecurity.CFFileSecuritySetAccessControlList (Handle, value.Handle))
						throw new Exception ("Failed to set ACL.");
				} else {
					ClearProperties (CFFileSecurityClearOptions.AccessControlList);
				}
			}
		}

		public uint? Owner {
			get {
				uint owner;
				if (!CFFileSecurity.CFFileSecurityGetOwner (Handle, out owner))
					return null;
				return owner;
			}
			set {
				if (value.HasValue) {
					if (!CFFileSecurity.CFFileSecuritySetOwner (Handle, value.Value))
						throw new Exception ("Failed to set owner");
				} else {
					ClearProperties (CFFileSecurityClearOptions.Owner);
				}
			}
		}

		public uint? Group {
			get {
				uint group;
				if (!CFFileSecurity.CFFileSecurityGetGroup (Handle, out group))
					return null;
				return group;
			}
			set {
				if (value.HasValue) {
					if (!CFFileSecurity.CFFileSecuritySetGroup (Handle, value.Value))
						throw new Exception ("Failed to set group");
				} else {
					ClearProperties (CFFileSecurityClearOptions.Group);
				}
			}
		}

		public ushort? Mode {
			get {
				ushort mode;
				if (!CFFileSecurity.CFFileSecurityGetMode (Handle, out mode))
					return null;
				return mode;
			}
			set {
				if (value.HasValue) {
					if (!CFFileSecurity.CFFileSecuritySetMode (Handle, value.Value))
						throw new Exception ("Failed to set mode");
				} else {
					ClearProperties (CFFileSecurityClearOptions.Mode);
				}
			}
		}

		public void ClearProperties(CFFileSecurityClearOptions options)
		{
			if (!CFFileSecurity.CFFileSecurityClearProperties (Handle, options))
				throw new Exception ("Failed to clear properties");
		}
	}
}