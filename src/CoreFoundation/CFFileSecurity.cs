//
// CFFileSecurity.cs
//
// Author(s):
//       David Lechner <david@lechnology.com>
//
// Copyright (c) 2014 David Lechner
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

using MonoMac.ObjCRuntime;

using CFAllocatorRef = System.IntPtr;
using CFFileSecurityRef = System.IntPtr;
using CFUUIDRef = System.IntPtr;
using acl_t = System.IntPtr;
using gid_t = System.UInt32;
using mode_t = System.UInt16;
using uid_t = System.UInt32;
using CFTypeID = System.UInt32;

namespace MonoMac.CoreFoundation
{
	public class ACL
	{

		public IntPtr Handle { get; private set; }

		public ACL () : this (10)
		{
		}

		public ACL (int count)
		{
			Handle = acl_init (count);
			if (Handle == IntPtr.Zero)
				throw new ExternalException ("Failed to allocate acl", Marshal.GetLastWin32Error ());
		}

		public ACL (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("Invalid handle.");
			Handle = handle;
		}

		~ACL ()
		{
			acl_free (Handle);
		}

		[DllImport ("libc.dylib", SetLastError = true)]
		static extern acl_t acl_init(int count);

		[DllImport ("libc.dylib")]
		static extern int acl_free(IntPtr obj_p);
	}

	[Since (6,0)] // 10,8
	[Flags]
	public enum CFFileSecurityClearOptions : ulong
	{
		Owner               = 1UL << 0,
		Group               = 1UL << 1,
		Mode                = 1UL << 2,
		OwnerUUID           = 1UL << 3,
		GroupUUID           = 1UL << 4,
		AccessControlList   = 1UL << 5
	}

	public class CFFileSecurity : CFType, ICloneable
	{
		//#define kCFFileSecurityRemoveACL (acl_t) _FILESEC_REMOVE_ACL

		internal CFFileSecurity (IntPtr handle, bool owns) : base (handle, owns)
		{
			Initalize ();
		}

		public CFFileSecurity ()
		{
			Handle = CFFileSecurityCreate (NULL);
			if (Handle == NULL)
				throw new Exception ("Failed to create CFFileSecurity");
			Initalize ();
		}

		public CFFileSecurity (CFAllocator allocator)
		{
			Handle = CFFileSecurityCreate (allocator.Handle);
			if (Handle == NULL)
				throw new Exception ("Failed to create CFFileSecurity");
			Initalize ();
		}

		void Initalize ()
		{
		}

		~CFFileSecurity ()
		{
			Dispose (false);
		}

		public static uint TypeID {
			get { return CFFileSecurityGetTypeID (); }
		}

		#region ICloneable implementation
		public object Clone ()
		{
			var copy = CFFileSecurityCreateCopy (NULL, Handle);
			if (copy == NULL)
				return null;
			return new CFFileSecurity (copy, true);
		}
		#endregion

		public CFUUID OwnerUuid {
			get {
				IntPtr uuidRef;
				if (CFFileSecurityCopyOwnerUUID (Handle, out uuidRef))
					return null;
				return new CFUUID (uuidRef, true);
			}
			set {
				if (value != null) {
					if (!CFFileSecuritySetOwnerUUID (Handle, value.Handle))
						throw new Exception ("Failed to set owner uuid.");
				} else {
					ClearProperties (CFFileSecurityClearOptions.OwnerUUID);
				}
			}
		}

		public CFUUID GroupUuid {
			get {
				IntPtr uuid;
				if (!CFFileSecurityCopyGroupUUID (Handle, out uuid))
					return null;
				return new CFUUID (uuid, true);
			}
			set {
				if (value != null) {
					if (!CFFileSecuritySetOwnerUUID (Handle, value.Handle))
						throw new Exception ("Failed to set group uuid.");
				} else {
					ClearProperties (CFFileSecurityClearOptions.GroupUUID);
				}
			}
		}

		public ACL ACL {
			get {
				IntPtr acl;
				if (!CFFileSecurityCopyAccessControlList (Handle, out acl))
					return null;
				return new ACL (acl);
			}
			set {
				if (value != null) {
					if (!CFFileSecuritySetAccessControlList (Handle, value.Handle))
						throw new Exception ("Failed to set ACL.");
				} else {
					ClearProperties (CFFileSecurityClearOptions.AccessControlList);
				}
			}
		}

		public uint? Owner {
			get {
				uint owner;
				if (!CFFileSecurityGetOwner (Handle, out owner))
					return null;
				return owner;
			}
			set {
				if (value.HasValue) {
					if (!CFFileSecuritySetOwner (Handle, value.Value))
						throw new Exception ("Failed to set owner");
				} else {
					ClearProperties (CFFileSecurityClearOptions.Owner);
				}
			}
		}

		public uint? Group {
			get {
				uint group;
				if (!CFFileSecurityGetGroup (Handle, out group))
					return null;
				return group;
			}
			set {
				if (value.HasValue) {
					if (!CFFileSecuritySetGroup (Handle, value.Value))
						throw new Exception ("Failed to set group");
				} else {
					ClearProperties (CFFileSecurityClearOptions.Group);
				}
			}
		}

		public ushort? Mode {
			get {
				ushort mode;
				if (!CFFileSecurityGetMode (Handle, out mode))
					return null;
				return mode;
			}
			set {
				if (value.HasValue) {
					if (!CFFileSecuritySetMode (Handle, value.Value))
						throw new Exception ("Failed to set mode");
				} else {
					ClearProperties (CFFileSecurityClearOptions.Mode);
				}
			}
		}

		public void ClearProperties(CFFileSecurityClearOptions options)
		{
			if (!CFFileSecurityClearProperties (Handle, options))
				throw new Exception ("Failed to clear properties");
		}

		/*
		 *	Returns the type identifier for the CFFileSecurity opaque type.
		 *
		 *	Return Value
		 *		The type identifier for the CFFileSecurity opaque type.
		 */
		[Since (5, 0)] //10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		static extern CFTypeID CFFileSecurityGetTypeID();


		/*
		 *	Creates an CFFileSecurity object.
		 *
		 *	Parameters
		 *		allocator
		 *			The allocator to use to allocate memory for the new object. Pass
		 *			NULL or kCFAllocatorDefault to use the current default allocator.
		 *	Return Value
		 *		A new CFFileSecurity object, or NULL if there was a problem creating the
		 *		object. Ownership follows the Create Rule.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		static extern CFFileSecurityRef CFFileSecurityCreate(CFAllocatorRef allocator);


		/*
		 *  Creates a copy of a CFFileSecurity object.
		 *
		 *  Parameters
		 *		allocator
		 *			The allocator to use to allocate memory for the new object. Pass
		 *			NULL or kCFAllocatorDefault to use the current default allocator.
		 *		fileSec
		 *			The CFFileSecurity object to copy.
		 *	Return Value
		 *		A copy of fileSec, or NULL if there was a problem creating the object.
		 *		Ownership follows the Create Rule.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		static extern CFFileSecurityRef CFFileSecurityCreateCopy(CFAllocatorRef allocator, CFFileSecurityRef fileSec);


		/*
		 *	This routine copies the owner UUID associated with an CFFileSecurity object.
		 *  
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		ownerUUID
		 *			A pointer to storage for the owner UUID.
		 *	Return Value
		 *		true if ownerUUID is successfully returned; false if there is no owner
		 *		UUID property associated with an CFFileSecurity object.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecurityCopyOwnerUUID(CFFileSecurityRef fileSec, out CFUUIDRef ownerUUID);

		/*
		 *	This routine sets the owner UUID associated with an CFFileSecurity object.
		 *  
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		ownerUUID
		 *			The owner UUID.
		 *	Return Value
		 *		true if the owner UUID was successfully set; otherwise, false.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecuritySetOwnerUUID(CFFileSecurityRef fileSec, CFUUIDRef ownerUUID);

		/*
		 *	This routine copies the group UUID associated with an CFFileSecurity object.
		 *  
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		groupUUID
		 *			A pointer to storage for the group UUID.
		 *	Return Value
		 *		true if groupUUID is successfully returned; false if there is no group
		 *		UUID property associated with an CFFileSecurity object.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecurityCopyGroupUUID(CFFileSecurityRef fileSec, out CFUUIDRef groupUUID);


		/*
		 *	This routine sets the group UUID associated with an CFFileSecurity object.
		 *  
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		groupUUID
		 *			The group UUID.
		 *	Return Value
		 *		true if the group UUID was successfully set; otherwise, false.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecuritySetGroupUUID(CFFileSecurityRef fileSec, CFUUIDRef groupUUID);


		/*
		 *	This routine copies the access control list (acl_t) associated with an
		 *	CFFileSecurity object. The acl_t returned by this routine is a copy and must
		 *	be released using acl_free(3). The acl_t is meant to be manipulated using
		 *	the acl calls defined in <sys/acl.h>.
		 *  
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		accessControlList
		 *			A pointer to storage for an acl_t. The acl_t be released using
		 *			acl_free(3)
		 *	Return Value
		 *		true if the access control list is successfully copied; false if there is
		 *		no access control list property associated with the CFFileSecurity object.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecurityCopyAccessControlList(CFFileSecurityRef fileSec, out acl_t accessControlList);

		/*
		 *	This routine will set the access control list (acl_t) associated with an
		 *	CFFileSecurityRef. To request removal of an access control list from a
		 *	filesystem object, pass in kCFFileSecurityRemoveACL as the acl_t and set
		 *	the fileSec on the target object using CFURLSetResourcePropertyForKey and
		 *	the kCFURLFileSecurityKey. Setting the accessControlList to NULL will result
		 *	in the property being unset.
		 *
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		accessControlList
		 *			The acl_t to set, or kCFFileSecurityRemoveACL to remove the access
		 *			control list, or NULL to unset the accessControlList.
		 *	Return Value
		 *		true if the access control list is successfully set; otherwise, false.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecuritySetAccessControlList(CFFileSecurityRef fileSec, acl_t accessControlList);


		/*
		 *	This routine gets the owner uid_t associated with an CFFileSecurity object.
		 *  
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		owner
		 *			A pointer to where the owner uid_t will be put.
		 *	Return Value
		 *		true if owner uid_t is successfully obtained; false if there is no owner
		 *		property associated with an CFFileSecurity object.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecurityGetOwner(CFFileSecurityRef fileSec, out uid_t owner);


		/*
		 *	This routine sets the owner uid_t associated with an CFFileSecurity object.
		 *  
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		owner
		 *			The owner uid_t.
		 *	Return Value
		 *		true if the owner uid_t was successfully set; otherwise, false.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecuritySetOwner(CFFileSecurityRef fileSec, uid_t owner);


		/*
		 *	This routine gets the group gid_t associated with an CFFileSecurity object.
		 *  
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		owner
		 *			A pointer to where the group gid_t will be put.
		 *	Return Value
		 *		true if group gid_t is successfully obtained; false if there is no group
		 *		property associated with an CFFileSecurity object.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecurityGetGroup(CFFileSecurityRef fileSec, out gid_t group);


		/*
		 *	This routine sets the group gid_t associated with an CFFileSecurity object.
		 *  
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		owner
		 *			The group gid_t.
		 *	Return Value
		 *		true if the group gid_t was successfully set; otherwise, false.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecuritySetGroup(CFFileSecurityRef fileSec, gid_t group);


		/*
		 *	This routine gets the mode_t associated with an CFFileSecurity object.
		 *  
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		owner
		 *			A pointer to where the mode_t will be put.
		 *	Return Value
		 *		true if mode_t is successfully obtained; false if there is no mode
		 *		property associated with an CFFileSecurity object.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecurityGetMode(CFFileSecurityRef fileSec, out mode_t mode);


		/*
		 *	This routine sets the mode_t associated with an CFFileSecurity object.
		 *  
		 *	Parameters
		 *		fileSec
		 *			The CFFileSecurity object.
		 *		owner
		 *			The mode_t.
		 *	Return Value
		 *		true if the mode_t was successfully set; otherwise, false.
		 */
		[Since (5,0)] // 10,7
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecuritySetMode(CFFileSecurityRef fileSec, mode_t mode);

		/*
		 *	This routine clears file security properties in the CFFileSecurity object.
		 *  
		 *	Parameters
		 *		clearPropertyMask
		 *			The file security properties to clear.
		 *	Return Value
		 *		true if the file security properties were successfully cleared; otherwise, false.
		 */
		[Since (6,0)] // 10,8
		[DllImport (Constants.CoreFoundationLibrary)]
		internal static extern Boolean CFFileSecurityClearProperties(CFFileSecurityRef fileSec, CFFileSecurityClearOptions clearPropertyMask);

	}
}

