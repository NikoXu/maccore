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

		/// <summary>Gets the registry root using the default master port.</summary>
		/// <remarks>This method provides an accessor to the root of the registry for the machine.
		/// The root may be passed to a registry iterator when iterating a plane,
		/// and contains properties that describe the available planes, and diagnostic information for IOKit.</remarks>
		/// <returns>The IORegistryEntry root instance, or <c>null</c> on failure.</returns>
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

		/// <summary>Returns the name assigned to this registry entry.</summary>
		/// <remarks>Registry entries can be named in a particular plane, or globally.
		/// This function returns the entry's global name. The global name defaults
		/// to the entry's meta class name if it has not been named.</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public string Name {
			get {
				ThrowIfDisposed ();
				var name = new StringBuilder (128);
				var result = IORegistryEntryGetName (Handle, name);
				ThrowIfError (result);
				return name.ToString ();
			}
		}

		/// <summary>Gets an ID for the registry entry that is global to all tasks.</summary>
		/// <remarks>The entry ID returned by IORegistryEntry.EntryID can be used to identify
		/// a registry entry across all tasks. A registry entry may be looked up by its EntryID by creating
		/// a matching dictionary with IORegistryEntry.CreateIDMatchingDictionary () to be used with the IOKit matching methods.
		/// The ID is valid only until the machine reboots.</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public ulong ID {
			get {
				ThrowIfDisposed ();
				ulong id;
				var result = IORegistryEntryGetRegistryEntryID (Handle, out id);
				ThrowIfError (result);
				return id;
			}
		}

		/// <summary>Looks up a registry entry by path using the default master port.</summary>
		/// <remarks>This function parses paths to lookup registry entries. The path should begin with '<plane name>:'
		/// If there are characters remaining unparsed after an entry has been looked up, this is considered an invalid lookup.
		/// Paths are further documented in IORegistryEntry.h</remarks>
		/// <param="path">The path.</param>
		/// <returns>The IORegistryEntry witch was found with the path, or <c>null</c> on failure.</returns>
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

		/// <summary>Create an iterator rooted at the registry root using the default master port.</summary>
		/// <remarks>This method creates an IORegistryIterator in the kernel that is set up with options
		/// to iterate children of the registry root entry, and to recurse automatically into entries as they are returned,
		/// or only when instructed with calls to IORegistryIterator.EnterEntry (). The iterator object keeps track of entries
		/// that have been recursed into previously to avoid loops.</remarks>
		/// <param="plane">The registry plane.</param>
		/// <param="options">RegistryIteratorOptions.Recursive may be set to recurse automatically
		/// into each entry as it is returned from IOIteratorNext calls on the registry iterator.</param>
		/// <returns>The iterator.</returns>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
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

		/// <summary>Create an iterator rooted at a given registry entry.</summary>
		/// <remarks>This method creates an IORegistryIterator in the kernel that is set up with options
		/// to iterate children or parents of a root entry, and to recurse automatically into entries as they are returned,
		/// or only when instructed with calls to IORegistryIterator.EnterEntry (). The iterator object keeps track of entries
		/// that have been recursed into previously to avoid loops.</remarks>
		/// <param="plane">The registry plane.</param>
		/// <param="options">RegistryIteratorOptions.Recursive may be set to recurse automatically
		/// into each entry as it is returned from IOIterator.MoveNext () calls on the registry iterator.
		/// RegistryIteratorOptions.IncludeParents may be set to iterate the parents of each entry,
		/// by default the children are iterated.</param>
		/// <returns>The iterator.</returns>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public IORegistryIterator<IOObject> CreateIterator (RegistryPlane plane, RegistryIteratorOptions options)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			IntPtr iteratorRef;
			var result = IORegistryEntryCreateIterator (Handle, planeString, (uint)options, out iteratorRef);
			ThrowIfError (result);
			return new IORegistryIterator<IOObject> (iteratorRef, true);
		}

		/// <summary>Gets the name assigned to a registry entry, in a specified plane.</summary>
		/// <remarks>Registry entries can be named in a particular plane, or globally.
		/// This function returns the entry's name in the specified plane or global name
		/// if it has not been named in that plane. The global name defaults to the entry's
		/// meta class name if it has not been named.</remarks>
		/// <param="plane">The registry plane.</param>
		/// <returns>The name.</returns>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public string GetNameInPlane (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			var name = new StringBuilder (128);
			var result = IORegistryEntryGetNameInPlane (Handle, planeString, name);
			ThrowIfError (result);
			return name.ToString ();
		}

		/// <summary>Returns the location assigned to a registry entry, in a specified plane.</summary>
		/// <remarks>Registry entries can given a location string in a particular plane, or globally.
		/// If the entry has had a location set in the specified plane that location string will be returned,
		/// otherwise the global location string is returned. If no global location string has been set, an error is returned.</remarks>
		/// <param="plane">The registry plane.</param>
		/// <returns>The location.</returns>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public string GetLocationInPlane (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			var location = new StringBuilder (128);
			var result = IORegistryEntryGetLocationInPlane (Handle, planeString, location);
			ThrowIfError (result);
			return location.ToString ();
		}

		/// <summary>Create a path for a registry entry.</summary>
		/// <remarks>The path describes the entry's attachment in a particular plane, which must be specified.
		/// The path begins with the plane name followed by a colon, and then followed by '/' separated path components
		/// for each of the entries between the root and the registry entry. An alias may also exist for the entry,
		/// and will be returned if available.</remarks>
		/// <param="plane">The registry plane.</param>
		/// <returns>The path.</returns>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public string GetPathInPlane (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			var path = new StringBuilder (512);
			var result = IORegistryEntryGetPath (Handle, planeString, path);
			ThrowIfError (result);
			return path.ToString ();
		}

		/// <summary>Create a CF dictionary representation of a registry entry's property table.</summary>
		/// <remarks>This function creates an instantaneous snapshot of a registry entry's property table,
		/// creating a CFDictionary analogue in the caller's task. Not every object available in the kernel is
		/// represented as a CF container; currently OSDictionary, OSArray, OSSet, OSSymbol, OSString, OSData,
		/// OSNumber, OSBoolean are created as their CF counterparts. </remarks>
		/// <returns>A NSMutableDictionary.</returns>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public NSMutableDictionary CreateCFProperties ()
		{
			ThrowIfDisposed ();
			CFMutableDictionaryRef propertiesRef;
			var result = IORegistryEntryCreateCFProperties (Handle, out propertiesRef, CFAllocator.Default.Handle, 0);
			ThrowIfError (result);
			var dict = new NSMutableDictionary (propertiesRef);
			CFType.Release (propertiesRef);
			return dict;
		}

		/// <summary>Create a CF representation of a registry entry's property.</summary>
		/// <remarks>This function creates an instantaneous snapshot of a registry entry property,
		/// creating a CF container analogue in the caller's task. Not every object available in the kernel is
		/// represented as a CF container; currently OSDictionary, OSArray, OSSet, OSSymbol, OSString, OSData,
		/// OSNumber, OSBoolean are created as their CF counterparts. </remarks>
		/// <param="key">The property name.</param>
		/// <returns>A CF container is created and returned the caller on success.
		/// The caller should release with CFRelease.</returns>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		public CFTypeRef CreateCFProperty (string key)
		{
			ThrowIfDisposed ();
			var keyAsCFString = new CFString (key);
			return IORegistryEntryCreateCFProperty (Handle, keyAsCFString.Handle, CFAllocator.Default.Handle, 0);
		}

		/// <summary>Set CF container based properties in a registry entry.
		/// <remarks>This is a generic method to pass a CF container of properties to an object in the registry.
		/// Setting properties in a registry entry is not generally supported, it is more common to support IOConnection.SetCFProperties
		/// for connection based property setting. The properties are interpreted by the object.</remarks>
		/// <param="properties">A CF container - commonly a CFDictionary but this is not enforced.
		/// The container should consist of objects which are understood by IOKit - these are currently : CFDictionary, CFArray,
		/// CFSet, CFString, CFData, CFNumber, CFBoolean, and are passed in the kernel as the corresponding
		/// OSDictionary etc. objects.</param>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public void SetCFProperties (INativeObject properties)
		{
			ThrowIfDisposed ();
			var result = IORegistryEntrySetCFProperties (Handle, properties.Handle);
			ThrowIfError (result);
		}

		/// <summary>Set a CF container based property in a registry entry.</summary>
		/// <remarks>This is a generic method to pass a CF container as a property to an object in the registry.
		/// Setting properties in a registry entry is not generally supported, it is more common to support
		/// IOConnecttion.SetCFProperty for connection based property setting. The property is interpreted by the object.</remarks>
		/// <param="propertyName">The name of the property.</param>
		/// <param="property">A CF container - should consist of objects which are understood by IOKit -
		/// these are currently : CFDictionary, CFArray, CFSet, CFString, CFData, CFNumber, CFBoolean, and are passed
		/// in the kernel as the corresponding OSDictionary etc. objects.</param>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public void SetCFProperty (string propertyName, INativeObject properties)
		{
			ThrowIfDisposed ();
			var propertyNameAsCFString = new CFString (propertyName);
			var result = IORegistryEntrySetCFProperty (Handle, propertyNameAsCFString.Handle, properties.Handle);
			ThrowIfError (result);
		}

		/// <summary>Returns an iterator over an registry entry's child entries in a plane.</summary>
		/// <remarks>This method creates an iterator which will return each of a registry entry's child entries in a specified plane.</remarks>
		/// <param="plane">The registry plane.</param>
		/// <returns>The created iterator over the children of the entry.</returns>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public IORegistryIterator<IOObject> GetChildIterator (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			IntPtr iteratorRef;
			var result = IORegistryEntryGetChildIterator (Handle, planeString, out iteratorRef);
			ThrowIfError (result);
			return new IORegistryIterator<IOObject> (iteratorRef, true);
		}

		/// <summary>Returns the first child of a registry entry in a plane.</summary>
		/// <remarks>This function will return the child which first attached to a registry entry in a plane.</remarks>
		/// <param="plane">The registry plane.</param>
		/// <returns>The first child of the registry entry.</returns>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public IORegistryEntry GetChildEntry (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			IntPtr childRef;
			var result = IORegistryEntryGetChildEntry (Handle, planeString, out childRef);
			ThrowIfError (result);
			return new IORegistryEntry (childRef, true);
		}

		/// <summary>Returns an iterator over an registry entry's parent entries in a plane.</summary>
		/// <remarks>This method creates an iterator which will return each of a registry entry's parent entries in a specified plane.</remarks>
		/// <param="plane">The registry plane.</param>
		/// <returns>The created iterator over the parents of the entry.</returns>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public IORegistryIterator<IOObject> GetParentIterator (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			IntPtr iteratorRef;
			var result = IORegistryEntryGetParentIterator (Handle, planeString, out iteratorRef);
			ThrowIfError (result);
			return new IORegistryIterator<IOObject> (iteratorRef, true);
		}

		/// <summary>Returns the first parent of a registry entry in a plane.</summary>
		/// <remarks>This function will return the parent to which the registry entry was first attached in a plane.</remarks>
		/// <param="plane">The registry plane.</param>
		/// <returns>The first parent of the registry entry.</returns>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has already been disposed.</exception>
		/// <exception cref="IOReturnException">Thrown if the external method call failed.</exception>
		public IORegistryEntry GetParentEntry (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			IntPtr parentRef;
			var result = IORegistryEntryGetParentEntry (Handle, planeString, out parentRef);
			ThrowIfError (result);
			return new IORegistryEntry (parentRef, true);
		}

		/// <summary>Determines if this registry entry is attached in a plane.</summary>
		/// <remarks>This method determines if this entry is attached in a plane to any other entry.</remarks>
		/// <param="plane">The registry plane.</param>
		/// <returns>If the entry has a parent in the plane, true is returned, otherwise false is returned.</returns>
		public bool IsInPlane (RegistryPlane plane)
		{
			ThrowIfDisposed ();
			var planeString = plane.GetKey ();
			return IORegistryEntryInPlane (Handle, planeString);
		}

		/// <summary>Return a handle to the registry root.</summary>
		/// <remarks>This method provides an accessor to the root of the registry for the machine.
		/// The root may be passed to a registry iterator when iterating a plane,
		/// and contains properties that describe the available planes, and diagnostic information for IOKit.</remarks>
		/// <param="masterPort">The master port obtained from IOMasterPort().
		/// Pass kIOMasterPortDefault to look up the default master port.</param>
		/// <returns>A handle to the IORegistryEntry root instance, to be released with IOObjectRelease by the caller,
		/// or MACH_PORT_NULL on failure.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static io_registry_entry_t IORegistryGetRootEntry (mach_port_t masterPort);

		/// <summary>Looks up a registry entry by path.</summary>
		/// <remarks>This function parses paths to lookup registry entries. The path should begin with '<plane name>:'
		/// If there are characters remaining unparsed after an entry has been looked up, this is considered an invalid lookup.
		/// Paths are further documented in IORegistryEntry.h</remarks>
		/// <param="masterPort">The master port obtained from IOMasterPort().
		/// Pass kIOMasterPortDefault to look up the default master port.</param>
		/// <param="path">A C-string path.</param>
		/// <returns>A handle to the IORegistryEntry witch was found with the path, to be released with IOObjectRelease by the caller,
		/// or MACH_PORT_NULL on failure.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static io_registry_entry_t IORegistryEntryFromPath (mach_port_t masterPort, io_string_t path);

		/// <summary>Create an iterator rooted at the registry root.</summary>
		/// <remarks>This method creates an IORegistryIterator in the kernel that is set up with options
		/// to iterate children of the registry root entry, and to recurse automatically into entries as they are returned,
		/// or only when instructed with calls to IORegistryIteratorEnterEntry. The iterator object keeps track of entries
		/// that have been recursed into previously to avoid loops.</remarks>
		/// <param="masterPort">The master port obtained from IOMasterPort().
		/// Pass kIOMasterPortDefault to look up the default master port.</param>
		/// <param="plane">The name of an existing registry plane. Plane names are defined
		/// in IOKitKeys.h, eg. kIOServicePlane.</param>
		/// <param="options">kIORegistryIterateRecursively may be set to recurse automatically
		/// into each entry as it is returned from IOIteratorNext calls on the registry iterator.</param>
		/// <param="iterator">A created iterator handle, to be released by the caller when it has finished with it.</param>
		/// <returns>A kern_return_t error code.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryCreateIterator (mach_port_t masterPort, io_name_t plane,
		                                                      IOOptionBits options, out io_iterator_t iterator);

		/// <summary>Create an iterator rooted at a given registry entry.</summary>
		/// <remarks>This method creates an IORegistryIterator in the kernel that is set up with options
		/// to iterate children or parents of a root entry, and to recurse automatically into entries as they are returned,
		/// or only when instructed with calls to IORegistryIteratorEnterEntry. The iterator object keeps track of entries
		/// that have been recursed into previously to avoid loops.</remarks>
		/// <param="entry">The root entry to begin the iteration at.</param>
		/// <param="plane The name of an existing registry plane. Plane names are defined in IOKitKeys.h, eg. kIOServicePlane.
		/// <param="options">kIORegistryIterateRecursively may be set to recurse automatically
		/// into each entry as it is returned from IOIteratorNext calls on the registry iterator.
		/// kIORegistryIterateParents may be set to iterate the parents of each entry, by default the children are iterated.</param>
		/// <param="iterator">A created iterator handle, to be released by the caller when it has finished with it.</param>
		/// <returns>A kern_return_t error code.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryCreateIterator (io_registry_entry_t entry, io_name_t plane,
		                                                           IOOptionBits options, out io_iterator_t iterator);


		/// <summary>Returns a C-string name assigned to a registry entry.</summary>
		/// <remarks>Registry entries can be named in a particular plane, or globally.
		/// This function returns the entry's global name. The global name defaults
		/// to the entry's meta class name if it has not been named.</remarks>
		/// <param="entry">The registry entry handle whose name to look up.</param>
		/// <param="name">The caller's buffer to receive the name.</param>
		/// <returns>A kern_return_t error code.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetName(io_registry_entry_t entry, /*io_name_t*/ StringBuilder name);

		/// <summary>Returns a C-string name assigned to a registry entry, in a specified plane.</summary>
		/// <remarks>Registry entries can be named in a particular plane, or globally.
		/// This function returns the entry's name in the specified plane or global name
		/// if it has not been named in that plane. The global name defaults to the entry's
		/// meta class name if it has not been named.</remarks>
		/// <param="entry">The registry entry handle whose name to look up.</param>
		/// <param="plane">The name of an existing registry plane. Plane names are defined in IOKitKeys.h, eg. kIOServicePlane.</param>
		/// <param="name">The caller's buffer to receive the name.</param>
		/// <returns>A kern_return_t error code.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetNameInPlane(io_registry_entry_t entry, io_name_t plane, /*io_name_t*/ StringBuilder name);

		/// <summary>Returns a C-string location assigned to a registry entry, in a specified plane.</summary>
		/// <remarks>Registry entries can given a location string in a particular plane, or globally.
		/// If the entry has had a location set in the specified plane that location string will be returned,
		/// otherwise the global location string is returned. If no global location string has been set, an error is returned.</remarks>
		/// <param="entry">The registry entry handle whose name to look up.</param>
		/// <param="plane">The name of an existing registry plane. Plane names are defined in IOKitKeys.h, eg. kIOServicePlane.</param>
		/// <param="location">The caller's buffer to receive the location string.</param>
		/// <returns>A kern_return_t error code.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetLocationInPlane(io_registry_entry_t entry, io_name_t plane, /*io_name_t*/ StringBuilder location);

		/// <summary>Create a path for a registry entry.</summary>
		/// <remarks>The path for a registry entry is copied to the caller's buffer.
		/// The path describes the entry's attachment in a particular plane, which must be specified.
		/// The path begins with the plane name followed by a colon, and then followed by '/' separated path components
		/// for each of the entries between the root and the registry entry. An alias may also exist for the entry,
		/// and will be returned if available.</remarks>
		/// <param="entry">The registry entry handle whose path to look up.</param>
		/// <param="plane">The name of an existing registry plane. Plane names are defined in IOKitKeys.h, eg. kIOServicePlane.</param>
		/// <param="path">A char buffer allocated by the caller.</param>
		/// <returns>IORegistryEntryGetPath will fail if the entry is not attached in the plane,
		/// or if the buffer is not large enough to contain the path.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetPath(io_registry_entry_t entry, io_name_t plane, /*io_string_t*/ StringBuilder path);

		/// <summary>Returns an ID for the registry entry that is global to all tasks.</summary>
		/// <remarks>The entry ID returned by IORegistryEntryGetRegistryEntryID can be used to identify
		/// a registry entry across all tasks. A registry entry may be looked up by its entryID by creating
		/// a matching dictionary with IORegistryEntryIDMatching() to be used with the IOKit matching functions.
		/// The ID is valid only until the machine reboots.</remarks>
		/// <param="entry">The registry entry handle whose ID to look up.</param>
		/// <param="entryID">The resulting ID.</param>
		/// <returns>A kern_return_t error code.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetRegistryEntryID(io_registry_entry_t entry, out uint64_t entryID);

		/// <summary>Create a CF dictionary representation of a registry entry's property table.</summary>
		/// <remarks>This function creates an instantaneous snapshot of a registry entry's property table,
		/// creating a CFDictionary analogue in the caller's task. Not every object available in the kernel is
		/// represented as a CF container; currently OSDictionary, OSArray, OSSet, OSSymbol, OSString, OSData,
		/// OSNumber, OSBoolean are created as their CF counterparts. </remarks>
		/// <param="entry">The registry entry handle whose property table to copy.</param>
		/// <param="properties">A CFDictionary is created and returned the caller on success.
		/// The caller should release with CFRelease.</param>
		/// <param="allocator">The CF allocator to use when creating the CF containers.</param>
		/// <param="options">No options are currently defined.</param>
		/// <returns>A kern_return_t error code.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryCreateCFProperties(io_registry_entry_t entry, out CFMutableDictionaryRef properties,
		                                                              CFAllocatorRef allocator, IOOptionBits options);

		/// <summary>Create a CF representation of a registry entry's property.</summary>
		/// <remarks>This function creates an instantaneous snapshot of a registry entry property,
		/// creating a CF container analogue in the caller's task. Not every object available in the kernel is
		/// represented as a CF container; currently OSDictionary, OSArray, OSSet, OSSymbol, OSString, OSData,
		/// OSNumber, OSBoolean are created as their CF counterparts. </remarks>
		/// <param="entry">The registry entry handle whose property to copy.</param>
		/// <param="key">A CFString specifying the property name.</param>
		/// <param="allocator">The CF allocator to use when creating the CF container.</param>
		/// <param="options">No options are currently defined.</param>
		/// <returns>A CF container is created and returned the caller on success.
		/// The caller should release with CFRelease.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeRef IORegistryEntryCreateCFProperty(io_registry_entry_t	entry, CFStringRef key,
		                                                        CFAllocatorRef allocator, IOOptionBits options);

		/// <summary>Create a CF representation of a registry entry's property.</summary>
		/// <remarks>This function creates an instantaneous snapshot of a registry entry property,
		/// creating a CF container analogue in the caller's task. Not every object available in the kernel is
		/// represented as a CF container; currently OSDictionary, OSArray, OSSet, OSSymbol, OSString, OSData,
		/// OSNumber, OSBoolean are created as their CF counterparts. 
		/// This function will search for a property, starting first with specified registry entry's property table,
		/// then iterating recusively through either the parent registry entries or the child registry entries of this entry.
		/// Once the first occurrence is found, it will lookup and return the value of the property, using the same semantics
		/// as IORegistryEntryCreateCFProperty. The iteration keeps track of entries that have been recursed into previously
		/// to avoid loops.</remarks>
		/// <param="entry">The registry entry at which to start the search.</param>
		/// <param="plane">The name of an existing registry plane. Plane names are defined in IOKitKeys.h, eg. kIOServicePlane.</param>
		/// <param="key">A CFString specifying the property name.</param>
		/// <param="allocator">The CF allocator to use when creating the CF container.</param>
		/// <param="options">kIORegistryIterateRecursively may be set to recurse automatically into the registry hierarchy.
		/// Without this option, this method degenerates into the standard IORegistryEntryCreateCFProperty() call.
		/// kIORegistryIterateParents may be set to iterate the parents of the entry, in place of the children.</param>
		/// <returns>A CF container is created and returned the caller on success. The caller should release with CFRelease.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeRef IORegistryEntrySearchCFProperty(io_registry_entry_t	entry, io_name_t plane, CFStringRef key,
		                                                        CFAllocatorRef allocator, IOOptionBits options);

		/// <summary>Set CF container based properties in a registry entry.
		/// <remarks>This is a generic method to pass a CF container of properties to an object in the registry.
		/// Setting properties in a registry entry is not generally supported, it is more common to support IOConnectSetCFProperties
		/// for connection based property setting. The properties are interpreted by the object.</remarks>
		/// <param="entry">The registry entry whose properties to set.</param>
		/// <param="properties">A CF container - commonly a CFDictionary but this is not enforced.
		/// The container should consist of objects which are understood by IOKit - these are currently : CFDictionary, CFArray,
		/// CFSet, CFString, CFData, CFNumber, CFBoolean, and are passed in the kernel as the corresponding
		/// OSDictionary etc. objects.</param>
		/// <returns>A kern_return_t error code returned by the object.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntrySetCFProperties(io_registry_entry_t entry, CFTypeRef properties);

		/// <summary>Set a CF container based property in a registry entry.</summary>
		/// <remarks>This is a generic method to pass a CF container as a property to an object in the registry.
		/// Setting properties in a registry entry is not generally supported, it is more common to support
		/// IOConnectSetCFProperty for connection based property setting. The property is interpreted by the object.</remarks>
		/// <param="entry">The registry entry whose property to set.</param>
		/// <param="propertyName">The name of the property as a CFString.</param>
		/// <param="property">A CF container - should consist of objects which are understood by IOKit -
		/// these are currently : CFDictionary, CFArray, CFSet, CFString, CFData, CFNumber, CFBoolean, and are passed
		/// in the kernel as the corresponding OSDictionary etc. objects.</param>
		/// <returns>A kern_return_t error code returned by the object.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntrySetCFProperty(io_registry_entry_t entry, CFStringRef propertyName, CFTypeRef property);

		/// <summary>Returns an iterator over an registry entry's child entries in a plane.</summary>
		/// <remarks>This method creates an iterator which will return each of a registry entry's child entries in a specified plane.</remarks>
		/// <param="entry The registry entry whose children to iterate over.</param>
		/// <param="plane">The name of an existing registry plane. Plane names are defined in IOKitKeys.h, eg. kIOServicePlane.</param>
		/// <param="iterator">The created iterator over the children of the entry, on success.
		/// The iterator must be released when the iteration is finished.</param>
		/// <returns>A kern_return_t error code.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetChildIterator(io_registry_entry_t entry, io_name_t plane, out io_iterator_t iterator);

		/// <summary>Returns the first child of a registry entry in a plane.</summary>
		/// <remarks>This function will return the child which first attached to a registry entry in a plane.</remarks>
		/// <param="entry">The registry entry whose child to look up.</param>
		/// <param="plane">The name of an existing registry plane. Plane names are defined in IOKitKeys.h, eg. kIOServicePlane.</param>
		/// <param="child">The first child of the registry entry, on success. The child must be released by the caller.</param>
		/// <returns>A kern_return_t error code.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetChildEntry(io_registry_entry_t entry, io_name_t plane, out io_registry_entry_t child);

		/// <summary>Returns an iterator over an registry entry's parent entries in a plane.</summary>
		/// <remarks>This method creates an iterator which will return each of a registry entry's parent entries in a specified plane.</remarks>
		/// <param="entry">The registry entry whose parents to iterate over.</param>
		/// <param="plane">The name of an existing registry plane. Plane names are defined in IOKitKeys.h, eg. kIOServicePlane.</param>
		/// <param="iterator">The created iterator over the parents of the entry, on success.
		/// The iterator must be released when the iteration is finished.</param>
		/// <returns>A kern_return_t error.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetParentIterator(io_registry_entry_t entry, io_name_t plane, out io_iterator_t iterator);

		/// <summary>Returns the first parent of a registry entry in a plane.</summary>
		/// <remarks>This function will return the parent to which the registry entry was first attached in a plane.</remarks>
		/// <param="entry">The registry entry whose parent to look up.</param>
		/// <param="plane">The name of an existing registry plane. Plane names are defined in IOKitKeys.h, eg. kIOServicePlane.</param>
		/// <param="parent">The first parent of the registry entry, on success. The parent must be released by the caller.</param>
		/// <returns>A kern_return_t error code.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IORegistryEntryGetParentEntry(io_registry_entry_t entry, io_name_t plane, out io_registry_entry_t parent);

		/// <summary>Determines if the registry entry is attached in a plane.</summary>
		/// <remarks>This method determines if the entry is attached in a plane to any other entry.</remarks>
		/// <param="entry">The registry entry.</param>
		/// <param="plane">The name of an existing registry plane. Plane names are defined in IOKitKeys.h, eg. kIOServicePlane.</param>
		/// <returns>If the entry has a parent in the plane, true is returned, otherwise false is returned.</returns>
		[DllImport (Constants.IOKitLibrary)]
		extern static boolean_t IORegistryEntryInPlane(io_registry_entry_t entry, io_name_t plane);
	}
}

