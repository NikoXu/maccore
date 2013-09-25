//
// IOMasterPort.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MonoMac;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.Kernel.Mach;
using MonoMac.ObjCRuntime;

using CFDictionaryRef = System.IntPtr;
using CFMutableDictionaryRef = System.IntPtr;
using IOOptionBits = System.UInt32;
using io_iterator_t = System.IntPtr;
using io_name_t = System.String;
using io_registry_entry_t = System.IntPtr;
using io_string_t = System.String;
using kern_return_t = MonoMac.IOKit.IOReturn;
using mach_port_t = System.IntPtr;
using mach_timespec_t = MonoMac.Kernel.Mach.TimeSpec;
using uint32_t = System.UInt32;

namespace MonoMac.IOKit
{
	/// <summary>
	/// Mach port used to initiate communication with IOKit.
	/// </summary>
	public class IOMasterPort : Port
	{
		Lazy<IORegistryEntry> rootEntry;

		/// <summary>Creates a new default IOMasterPort port used to initiate communication with IOKit.</summary>
		/// <remarks>Methods that don't specify an existing object require the IOMasterPort to be passed.
		/// This constructor obtains that port.</remarks>
		/// <exception cref="IOKitException">If there was an error creating the port.</exception>
		public IOMasterPort () : this (Port.NULL)
		{
			rootEntry = new Lazy<IORegistryEntry> (() => IORegistryEntry.GetRootEntryForPort (Handle));
		}

		/// <summary>Creates a new IOMasterPort used to initiate communication with IOKit.</summary>
		/// <remarks>Methods that don't specify an existing object require the IOMasterPort to be passed.
		/// This constructor obtains that port.</remarks>
		/// <param name="bootstrapPort">The bootstrap port.</param>
		/// <exception cref="IOKitException">If there was an error creating the port.</exception>
		public IOMasterPort (uint bootstrapPort) : base (IntPtr.Zero)
		{
			IntPtr masterPort;
			var error = NewIOMasterPort (bootstrapPort, out masterPort);
			if (error != IOReturn.Success)
				throw new IOKitException (error);
			Handle = masterPort;
		}

		/// <summary>
		/// Returns <c>true</c> any IOServices are busy; otherwise <c>false</c>.
		/// </summary>
		/// <remarks>Many activities in IOService are asynchronous. When registration, matching,
		/// or termination is in progress on an IOService, its busyState is increased by one.
		/// Change in busyState to or from zero also changes the IOService's provider's busyState
		/// by one, which means that an IOService is marked busy when any of the above activities
		/// is ocurring on it or any of its clients. BusyState returns the busy state
		/// of the root of the service plane which reflects the busy state of all IOServices.</remarks>
		public bool IsBusy {
			get { return BusyState > 0; }
		}

		/// <summary>
		/// Returns the busyState of all IOServices.
		/// </summary>
		/// <returns>The busyState count.</returns>
		/// <remarks>Many activities in IOService are asynchronous. When registration, matching,
		/// or termination is in progress on an IOService, its busyState is increased by one.
		/// Change in busyState to or from zero also changes the IOService's provider's busyState
		/// by one, which means that an IOService is marked busy when any of the above activities
		/// is ocurring on it or any of its clients. BusyState returns the busy state
		/// of the root of the service plane which reflects the busy state of all IOServices.</remarks>
		public uint BusyState
		{
			get { return IOService.GetBusyStateForPort (Handle); }
		}

		/// <summary>Gets the registry root using this master port.</summary>
		/// <remarks>This method provides an accessor to the root of the registry for the machine.
		/// The root may be passed to a registry iterator when iterating a plane,
		/// and contains properties that describe the available planes, and diagnostic information for IOKit.</remarks>
		/// <returns>The IORegistryEntry root instance, or <c>null</c> on failure.</returns>
		public IORegistryEntry RootEntry {
			get { return rootEntry.Value; }
		}

		/// <summary>Creates and returns a IONotificationPort object for receiving IOKit notifications
		/// of new devices or state changes.</summary>
		/// <remarks>Creates the notification object to receive notifications from IOKit of new device arrivals or state changes.
		/// The notification object can be supply a CFRunLoopSource, or mach_port_t to be used to listen for events.</remarks>
		/// <returns>The notification port object.</returns>
		/// <exception cref="Exception">If the object was not created</exception>
		public IONotificationPort CreateNotificationPort ()
		{
			return new IONotificationPort (this);
		}


		/// <summary>
		/// Look up a registered IOService object that matches a matching dictionary.
		/// </summary>
		/// <returns>The first service matched is returned on success or null if no services matched.</returns>
		/// <param name="masterPort">The master port.</param>
		/// <param name="matching">A CF dictionary containing matching information.
		/// IOKitLib can construct matching dictionaries for common criteria with helper methods
		/// such as  <see cref="CreateMatchingDictionaryForClass"/>, <see cref="CreateMatchingDictionaryForName"/>
		/// and <see cref="CreateMatchingDictionaryForBSDName"/>.</param>
		/// <remarks>This is the preferred method of finding IOService objects currently registered by IOKit
		/// (that is, objects that have had their RegisterService() methods invoked). To find IOService objects
		/// that aren't yet registered, use an iterator as created by IORegistryEntry.CreateIterator().
		/// IOService.AddMatchingNotification can also supply this information and install a notification of new IOServices.
		/// The matching information used in the matching dictionary may vary depending on the class of service being looked up.
		/// </remarks>
		public IOService GetMatchingService (CFDictionaryRef matchingDictionary)
		{
			return IOService.GetMatchingService (Handle, matchingDictionary);
		}

		/// <summary>
		/// Look up registered IOService objects that match a matching dictionary.
		/// </summary>
		/// <returns>A list of matching IOService objects</returns>
		/// <param name="masterPort">The IOMasterPort.</param>
		/// <param name="matching">A CF dictionary containing matching information.
		/// IOKit can construct matching dictionaries for common criteria with helper methods
		/// such as <see cref="CreateMatchingDictionaryForClass"/>, <see cref="CreateMatchingDictionaryForName"/>
		/// and <see cref="CreateMatchingDictionaryForBSDName"/>.</param>
		/// <remarks>This is the preferred method of finding IOService objects currently registered by IOKit
		/// (that is, objects that have had their RegisterService() methods invoked). To find IOService objects that aren't yet registered,
		/// use an iterator as created by IORegistryEntry.CreateIterator(). IOService.AddMatchingNotification can also supply this information
		/// and install a notification of new IOServices. The matching information used in the matching dictionary may vary depending
		/// on the class of service being looked up.</remarks>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public IOIterator<T> GetMatchingServices<T> (CFDictionaryRef matchingDictionary) where T : IOService
		{
			return IOService.GetMatchingServices<T> (Handle, matchingDictionary);
		}

		/// <summary>
		/// Wait until all IOServices are not busy.
		/// </summary>
		/// <returns><c>true</c> if the method timed out; otherwise <c>false</c></returns>
		/// <param name="seconds">Number of seconds to wait.</param>
		/// <param name="nanoseconds">Number of nanoseconds to wait.</param>
		/// <remarks>Blocks the caller until all IOServices are not busy, see <see cref="BusyState"/>.</remarks>
		public bool Wait (int seconds, int nanoseconds = 0)
		{
			return Wait (new TimeSpec () { Seconds = (uint)seconds, Nanoseconds = nanoseconds });
		}

		/// <summary>
		/// Wait until all IOServices are not busy.
		/// </summary>
		/// <returns><c>true</c> if the method timed out; otherwise <c>false</c></returns>
		/// <param name="waitTime">Specifies a maximum time to wait.</param>
		/// <remarks>Blocks the caller until all IOServices are not busy, see <see cref="BusyState"/>.</remarks>
		public bool Wait (TimeSpec waitTime)
		{
			return IOService.WaitForPort (Handle, waitTime);
		}

		/// <summary>Looks up a registry entry by path using this master port.</summary>
		/// <remarks>This function parses paths to lookup registry entries. The path should begin with '<plane name>:'
		/// If there are characters remaining unparsed after an entry has been looked up, this is considered an invalid lookup.
		/// Paths are further documented in IORegistryEntry.h</remarks>
		/// <param="path">The path.</param>
		/// <returns>The IORegistryEntry witch was found with the path, or <c>null</c> on failure.</returns>
		public IORegistryEntry FromPath (string path)
		{
			return IORegistryEntry.GetEntryFromPath (Handle, path);
		}

		/// <summary>Create an iterator rooted at the registry root using this master port.</summary>
		/// <remarks>This method creates an IORegistryIterator in the kernel that is set up with options
		/// to iterate children of the registry root entry, and to recurse automatically into entries as they are returned,
		/// or only when instructed with calls to IORegistryIterator.EnterEntry (). The iterator object keeps track of entries
		/// that have been recursed into previously to avoid loops.</remarks>
		/// <param="plane">The type of plane.</param>
		/// <param="options">RegistryIteratorOptions.Recursive may be set to recurse automatically
		/// into each entry as it is returned from IOIteratorNext calls on the registry iterator.</param>
		/// <returns>The iterator.</returns>
		/// <exception cref="IOKitException">Thrown if the external method call failed.</exception>
		public IORegistryIterator<IOObject> CreateRootIterator (RegistryPlane plane, RegistryIteratorOptions options)
		{
			return IORegistryEntry.CreateRootIteratorForPort (Handle, plane, options);
		}

		/// <summary>
		/// Create a matching dictionary that specifies an IOService match based on BSD device name using this master port.
		/// </summary>
		/// <returns>The matching dictionary created, is returned on success, or zero on failure.
		/// The dictionary is commonly passed to IOServiceGetMatchingServices or IOServiceAddNotification.
		/// It should be released with CFRelease by the caller.</returns>
		/// <param name="bsdName">The BSD name.</param>
		/// <remarks>IOServices that represent BSD devices have an associated BSD name.
		/// This function creates a matching dictionary that will match IOService's with a given BSD name.</remarks>
		public NSMutableDictionary CreateMatchingDictionaryForBSDName (string bsdName)
		{
			return IOService.CreateMatchingDictionaryForBSDNameWithPort (Handle, bsdName);
		}

		/// <summary>
		/// Returns the mach port used to initiate communication with IOKit.
		/// </summary>
		/// <returns> A kern_return_t error code.</returns>
		/// <param name="bootstrapPort">Pass MACH_PORT_NULL for the default.</param>
		/// <param name="masterPort">The master port is returned.</param>
		/// <remarks>Functions that don't specify an existing object require the IOKit master port to be passed.
		/// This function obtains that port.</remarks>
		[DllImport (Constants.IOKitLibrary, EntryPoint = "IOMasterPort")]
		extern static kern_return_t NewIOMasterPort (uint bootstrapPort, out mach_port_t masterPort);
	}
}
