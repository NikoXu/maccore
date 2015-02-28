//
// IORegistryEntry.cs
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
using System.Text;
using MonoMac;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

using CFAllocatorRef = System.IntPtr;
using CFMutableDictionaryRef = System.IntPtr;
using CFStringRef = System.IntPtr;
using CFTypeRef = System.IntPtr;
using IOOptionBits = System.UInt32;
using boolean_t = System.Boolean;
using io_iterator_t = System.IntPtr;
using io_name_t = System.String; // 128 chars
using io_registry_entry_t = System.IntPtr;
using io_string_t = System.String; // 512 chars
using kern_return_t = MonoMac.IOKit.IOReturn;
using mach_port_t = System.IntPtr;
using uint32_t = System.UInt32;
using uint64_t = System.UInt64;

namespace MonoMac.IOKit
{
	public class IORegistryEntry : IOObject
	{
		static readonly IntPtr kIOMasterPortDefault = IntPtr.Zero;

		static Lazy<IORegistryEntry> rootEntry;

		static IORegistryEntry ()
		{
			rootEntry = new Lazy<IORegistryEntry> (() => GetRootEntryForPort (kIOMasterPortDefault));
		}

		internal IORegistryEntry (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		#region API Wrappers

		[DllImport (Constants.IOKitLibrary)]
		extern static io_registry_entry_t IORegistryGetRootEntry (mach_port_t masterPort);

		public static IORegistryEntry RootEntry {
			get { return rootEntry.Value; }
		}

		internal static IORegistryEntry GetRootEntryForPort (IntPtr machPort)
		{
			var registryEntryRef = IORegistryGetRootEntry (kIOMasterPortDefault);
			if (registryEntryRef == IntPtr.Zero)
				return null;
			return new IORegistryEntry (registryEntryRef, true);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static io_registry_entry_t IORegistryEntryFromPath (mach_port_t masterPort, io_string_t path);

		public static IORegistryEntry FromPath (string path)
		{
			return GetEntryFromPath (kIOMasterPortDefault, path);
		}

		internal static IORegistryEntry GetEntryFromPath (IntPtr machPort, string path)
		{
			var registryEntryRef = IORegistryEntryFromPath (machPort, path);
			if (registryEntryRef == IntPtr.Zero)
				return null;
			return new IORegistryEntry (registryEntryRef, true);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryCreateIterator (mach_port_t masterPort, io_name_t plane,
		                                                      IOOptionBits options, out io_iterator_t iterator);

		public static IORegistryIterator<IOObject> CreateRootIterator (RegistryPlane plane, RegistryIteratorOptions options)
		{
			return CreateRootIteratorForPort (kIOMasterPortDefault, plane, options);
		}

		internal static IORegistryIterator<IOObject> CreateRootIteratorForPort (IntPtr machPort, RegistryPlane plane, RegistryIteratorOptions options)
		{
			var planeString = plane.GetKey ();
			IntPtr iteratorRef;
			var result = IORegistryCreateIterator (machPort, planeString, (uint)options, out iteratorRef);
			ThrowIfError (result);
			return new IORegistryIterator<IOObject> (iteratorRef, true);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryCreateIterator (io_registry_entry_t entry, io_name_t plane,
		                                                           IOOptionBits options, out io_iterator_t iterator);

		public IORegistryIterator<IOObject> CreateIterator (RegistryPlane plane, RegistryIteratorOptions options)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			IntPtr iteratorRef;
			var result = IORegistryEntryCreateIterator (Handle, planeString, (uint)options, out iteratorRef);
			ThrowIfError (result);
			return new IORegistryIterator<IOObject> (iteratorRef, true);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetName(io_registry_entry_t entry, /*io_name_t*/ StringBuilder name);

		public string Name {
			get {
				ThrowIfDisposed ();
				var name = new StringBuilder (128);
				var result = IORegistryEntryGetName (Handle, name);
				ThrowIfError (result);
				return name.ToString ();
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetNameInPlane(io_registry_entry_t entry, io_name_t plane, /*io_name_t*/ StringBuilder name);

		public string GetNameInPlane (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			var name = new StringBuilder (128);
			var result = IORegistryEntryGetNameInPlane (Handle, planeString, name);
			ThrowIfError (result);
			return name.ToString ();
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetLocationInPlane(io_registry_entry_t entry, io_name_t plane, /*io_name_t*/ StringBuilder location);

		public string GetLocationInPlane (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			var location = new StringBuilder (128);
			var result = IORegistryEntryGetLocationInPlane (Handle, planeString, location);
			ThrowIfError (result);
			return location.ToString ();
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetPath(io_registry_entry_t entry, io_name_t plane, /*io_string_t*/ StringBuilder path);

		public string GetPathInPlane (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			var path = new StringBuilder (512);
			var result = IORegistryEntryGetPath (Handle, planeString, path);
			ThrowIfError (result);
			return path.ToString ();
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetRegistryEntryID(io_registry_entry_t entry, out uint64_t entryID);

		public ulong ID {
			get {
				ThrowIfDisposed ();
				ulong id;
				var result = IORegistryEntryGetRegistryEntryID (Handle, out id);
				ThrowIfError (result);
				return id;
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryCreateCFProperties(io_registry_entry_t entry, out CFMutableDictionaryRef properties,
		                                                              CFAllocatorRef allocator, IOOptionBits options);

		public NSMutableDictionary CreateCFProperties ()
		{
			ThrowIfDisposed ();
			CFMutableDictionaryRef propertiesRef;
			var result = IORegistryEntryCreateCFProperties (Handle, out propertiesRef, IntPtr.Zero, 0);
			ThrowIfError (result);
			var dict = new NSMutableDictionary (propertiesRef);
			CFType.Release (propertiesRef);
			return dict;
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeRef IORegistryEntryCreateCFProperty(io_registry_entry_t	entry, CFStringRef key,
		                                                        CFAllocatorRef allocator, IOOptionBits options);

		public CFTypeRef CreateCFProperty (string key)
		{
			ThrowIfDisposed ();
			var keyAsCFString = new CFString (key);
			return IORegistryEntryCreateCFProperty (Handle, keyAsCFString.Handle, IntPtr.Zero, 0);
		}

//		[DllImport (Constants.IOKitLibrary)]
//		extern static CFTypeRef IORegistryEntrySearchCFProperty(io_registry_entry_t	entry, io_name_t plane, CFStringRef key,
//		                                                        CFAllocatorRef allocator, IOOptionBits options);

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntrySetCFProperties(io_registry_entry_t entry, CFTypeRef properties);

		public void SetCFProperties (INativeObject properties)
		{
			ThrowIfDisposed ();
			var result = IORegistryEntrySetCFProperties (Handle, properties.Handle);
			ThrowIfError (result);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntrySetCFProperty(io_registry_entry_t entry, CFStringRef propertyName, CFTypeRef property);

		public void SetCFProperty (string propertyName, INativeObject properties)
		{
			ThrowIfDisposed ();
			var propertyNameAsCFString = new CFString (propertyName);
			var result = IORegistryEntrySetCFProperty (Handle, propertyNameAsCFString.Handle, properties.Handle);
			ThrowIfError (result);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetChildIterator(io_registry_entry_t entry, io_name_t plane, out io_iterator_t iterator);

		public IORegistryIterator<IOObject> GetChildIterator (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			IntPtr iteratorRef;
			var result = IORegistryEntryGetChildIterator (Handle, planeString, out iteratorRef);
			ThrowIfError (result);
			return new IORegistryIterator<IOObject> (iteratorRef, true);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetChildEntry(io_registry_entry_t entry, io_name_t plane, out io_registry_entry_t child);

		public IORegistryEntry GetChildEntry (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			IntPtr childRef;
			var result = IORegistryEntryGetChildEntry (Handle, planeString, out childRef);
			ThrowIfError (result);
			return new IORegistryEntry (childRef, true);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetParentIterator(io_registry_entry_t entry, io_name_t plane, out io_iterator_t iterator);

		public IORegistryIterator<IOObject> GetParentIterator (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			IntPtr iteratorRef;
			var result = IORegistryEntryGetParentIterator (Handle, planeString, out iteratorRef);
			ThrowIfError (result);
			return new IORegistryIterator<IOObject> (iteratorRef, true);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetParentEntry(io_registry_entry_t entry, io_name_t plane, out io_registry_entry_t parent);

		public IORegistryEntry GetParentEntry (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			IntPtr parentRef;
			var result = IORegistryEntryGetParentEntry (Handle, planeString, out parentRef);
			ThrowIfError (result);
			return new IORegistryEntry (parentRef, true);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static boolean_t IORegistryEntryInPlane(io_registry_entry_t entry, io_name_t plane);

		public bool IsInPlane (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			return IORegistryEntryInPlane (Handle, planeString);
		}

		#endregion
	}
}

