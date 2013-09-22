//
// IOService.cs
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
using MonoMac.ObjCRuntime;

using CFDictionaryRef = System.IntPtr;
using CFMutableDictionaryRef = System.IntPtr;
using IONotificationPortRef = System.IntPtr;
using boolean_t = System.Boolean;
using io_connect_t = System.IntPtr;
using io_iterator_t = System.IntPtr;
using io_name_t = System.String;
using io_object_t = System.IntPtr;
using io_service_t = System.IntPtr;
using kern_return_t = MonoMac.IOKit.IOReturn;
using mach_port_t = System.IntPtr;
using mach_timespec_t = MonoMac.Mach.TimeSpec;
using task_port_t = System.IntPtr;
using uint32_t = System.UInt32;
using uint64_t = System.UInt64;

namespace MonoMac.IOKit
{
	public class IOService : IORegistryEntry
	{
		static readonly IntPtr kIOMasterPortDefault = IntPtr.Zero;
		static List<Delegate> callbackStore; // to keep callback delegates from getting GCed

		static IOService ()
		{			
			callbackStore = new List<Delegate> ();
		}

		internal IOService (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		/// <summary>
		/// Gets a value indicating whether this instance is busy.
		/// </summary>
		/// <value><c>true</c> if this instance is busy with an asynchronous task; otherwise, <c>false</c>.</value>
		/// <exception cref="ObjectDisposedException">If this instance has already been disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public bool IsBusy {
			get { return BusyState > 0; }
		}

		/// <summary>
		/// Returns the busyState of an IOService.
		/// </summary>
		/// <returns>The busyState count.</returns>
		/// <remarks>Many activities in IOService are asynchronous. When registration, matching, or termination is in progress
		/// on an IOService, its busyState is increased by one. Change in busyState to or from zero also changes the IOService's
		/// provider's busyState by one, which means that an IOService is marked busy when any of the above activities is ocurring
		/// on it or any of its clients.</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public uint BusyState {
			get {
				ThrowIfDisposed ();
				uint busyState;
				var result = IOServiceGetBusyState (Handle, out busyState);
				ThrowIfError (result);
				return busyState;
			}
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
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public static bool IsAnyBusy {
			get { return AllBusyState > 0; }
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
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public static uint AllBusyState {
			get { return GetBusyStateForPort (kIOMasterPortDefault); }
		}

		internal static uint GetBusyStateForPort (IntPtr masterPortRef)
		{
			uint busyState;
			var result = IOKitGetBusyState (masterPortRef, out busyState);
			ThrowIfError (result);
			return busyState;
		}

		/// <summary>
		/// Look up a registered IOService object that matches a matching dictionary using the default master port.
		/// </summary>
		/// <returns>The first service matched is returned on success or null if no services matched.</returns>
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
		public static IOService GetMatchingService (NSDictionary matchingDictionary)
		{
			return GetMatchingService (kIOMasterPortDefault, matchingDictionary.Handle);
		}

		internal static IOService GetMatchingService (IntPtr masterPortRef, IntPtr matchingDictionary)
		{
			CFType.Retain (matchingDictionary);
			var service = IOServiceGetMatchingService (masterPortRef, matchingDictionary);
			if (service == IntPtr.Zero)
				return null;
			return new IOService (service, true);
		}

		/// <summary>
		/// Look up registered IOService objects that match a matching dictionary using the default master port.
		/// </summary>
		/// <returns>A list of matching IOService objects</returns>
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
		public static IOIterator<T> GetMatchingServices<T> (NSDictionary matchingDictionary) where T : IOService
		{
			return GetMatchingServices<T> (kIOMasterPortDefault, matchingDictionary.Handle);
		}

		internal static IOIterator<T> GetMatchingServices<T> (IntPtr masterPortRef, IntPtr matchingDictionaryRef) where T : IOService
		{
			IntPtr iterator;
			var result = IOServiceGetMatchingServices (masterPortRef, matchingDictionaryRef, out iterator);
			ThrowIfError (result);
			return new IOIterator<T> (iterator, true);
		}

		/// <summary>
		/// Look up registered IOService objects that match a matching dictionary, and install a notification request of new IOServices that match.
		/// </summary>
		/// <returns>An IOIterator of matching IOService objects. The notification is armed when the Enumerator is emptied
		/// by calls to MoveNext() - when no more objects are returned, the notification is armed. Note the notification is
		/// not armed when first created. The notification is unarmed when the IOIterator is disposed.</returns>
		/// <param name="notifyPort">A IONotificationPort object that controls how messages will be sent when the armed notification is fired.
		/// When the notification is delivered, the IOIterator<IOService> representing the notification should be iterated through
		/// to pick up all outstanding objects. When the iteration is finished the notification is rearmed.</param>
		/// <param name="notificationType">A <see cref="NotificationType"/></param>
		/// <param name="matchingDictionary">A CF dictionary containing matching information.
		/// IOKit can construct matching dictionaries for common criteria with helper methods
		/// such as <see cref="CreateMatchingDictionaryForClass"/>, <see cref="CreateMatchingDictionaryForName"/>
		/// and <see cref="CreateMatchingDictionaryForBSDName"/>.</param>
		/// <param name="callback">A callback function called when the notification fires.</param>
		/// <remarks>This is the preferred method of finding IOService objects that may arrive at any time.
		/// The type of notification specifies the state change the caller is interested in, on IOService's that match the match dictionary.
		/// The matching information used in the matching dictionary may vary depending on the class of service being looked up.</remarks>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public static IOIterator<T> AddMatchingNotification<T> (IONotificationPort notifyPort, NotificationType notificationType,
		                                                                NSDictionary matchingDictionary, MatchingCallback<T> callback) where T : IOService
		{
			var notificationTypeString = notificationType.GetKey ();
			IOServiceMatchingCallback nativeCallback = (refCon, iteratorRef) =>
			{
				var iterator = new IOIterator<T> (iteratorRef, false);
				callback.Invoke (iterator);
			};
			callbackStore.Add (nativeCallback);
			IntPtr iteratorRef2;
			CFType.Retain (matchingDictionary.Handle);
			var result = IOServiceAddMatchingNotification (notifyPort.Handle, notificationTypeString, matchingDictionary.Handle,
			                                               nativeCallback, IntPtr.Zero, out iteratorRef2);
			ThrowIfError (result);
			var iterator2 = new IOIterator<T> (iteratorRef2, true);
			iterator2 .Disposed += (sender, e) => callbackStore.Remove (nativeCallback);
			return iterator2; 
		}

		/// <summary>
		/// Register for notification of state changes in an IOService.
		/// </summary>
		/// <returns>An IOObject.</returns>
		/// <param name="notifyPort">A IONotificationPortRef object that controls how messages will be sent when the notification is fired.
		/// See IONotificationPortCreate.</param>
		/// <param name="interestType">An <see cref="InterestType"/></param>
		/// <param name="callback">A callback function called when the notification fires, with messageType and messageArgument for the state change.</param>
		/// <remarks>IOService objects deliver notifications of their state changes to their clients via the IOService::message API,
		/// and to other interested parties including callers of this function. Message types are defined IOKit/IOMessage.h.</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public IOObject AddInterestNotification (IONotificationPort notifyPort, InterestType interestType, InterestCallback callback)
		{
			ThrowIfDisposed ();
			var interestTypeString = interestType.GetKey ();
			IOServiceInterestCallback nativeCallback = (refCon, service, messageType, messageArgument) =>
			{
				if (service != Handle)
					return;
				callback.Invoke (this, messageType, messageArgument);
			};
			callbackStore.Add (nativeCallback);
			IntPtr notification;
			var result = IOServiceAddInterestNotification (notifyPort.Handle, Handle, interestTypeString,
			                                               nativeCallback, IntPtr.Zero, out notification);
			ThrowIfError (result);
			return new IOObject (notification, true);
		}

		/// <summary>
		/// Match an IOService objects with matching dictionary.
		/// </summary>
		/// <returns><c>true</c> if this service object matches the matching dictionary, otherwise <c>false</c></returns>
		/// <param name="matching">A CF dictionary containing matching information.
		/// IOKit can construct matching dictionaries for common criteria with helper methods
		/// such as <see cref="CreateMatchingDictionaryForClass"/>, <see cref="CreateMatchingDictionaryForName"/>
		/// and <see cref="CreateMatchingDictionaryForBSDName"/>.</param>
		/// <exception cref="ObjectDisposedException">If this instance has already been disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public bool MatchPropertyTable (NSDictionary matchingDictionary)
		{
			ThrowIfDisposed ();
			bool matches;
			var result = IOServiceMatchPropertyTable (Handle, matchingDictionary.Handle, out matches);
			ThrowIfError (result);
			return matches;
		}

		/// <summary>
		/// Wait until this instance is no longer busy.
		/// </summary>
		/// <returns>True if the method timed out.</returns>
		/// <param name="seconds">Number of seconds to wait.</param>
		/// <param name="nanoseconds">Number of nanoseconds to wait.</param>
		/// <remarks>Blocks the caller until an IOService is non busy, see <see cref="BusyState"/>.</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public bool Wait (int seconds, int nanoseconds = 0)
		{
			return Wait (new Mach.TimeSpec () { Seconds = (uint)seconds, Nanoseconds = nanoseconds });
		}

		/// <summary>
		/// Wait until this instance is no longer busy.
		/// </summary>
		/// <returns>True if the method timed out.</returns>
		/// <param name="waitTime">Specifies a maximum time to wait.</param>
		/// <remarks>Blocks the caller until an IOService is non busy, see <see cref="BusyState"/>.</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public bool Wait (Mach.TimeSpec waitTime)
		{
			ThrowIfDisposed ();
			var result = IOServiceWaitQuiet (Handle, waitTime);
			if (result == IOReturn.Timeout)
				return true;
			ThrowIfError (result);
			return false;
		}

		/// <summary>
		/// Wait until all IOServices are not busy.
		/// </summary>
		/// <returns><c>true</c> if the method timed out; otherwise <c>false</c></returns>
		/// <param name="seconds">Number of seconds to wait.</param>
		/// <param name="nanoseconds">Number of nanoseconds to wait.</param>
		/// <remarks>Blocks the caller until all IOServices are not busy, see <see cref="BusyState"/>.</remarks>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public static bool WaitAll (int seconds, int nanoseconds = 0)
		{
			return WaitAll (new Mach.TimeSpec () { Seconds = (uint)seconds, Nanoseconds = nanoseconds });
		}

		/// <summary>
		/// Wait until all IOServices are not busy.
		/// </summary>
		/// <returns><c>true</c> if the method timed out; otherwise <c>false</c></returns>
		/// <param name="waitTime">Specifies a maximum time to wait.</param>
		/// <remarks>Blocks the caller until all IOServices are not busy, see <see cref="BusyState"/>.</remarks>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public static bool WaitAll (Mach.TimeSpec waitTime)
		{
			return WaitForPort (kIOMasterPortDefault, waitTime);
		}

		internal static bool WaitForPort (IntPtr masterPort, Mach.TimeSpec waitTime)
		{
			var result = IOKitWaitQuiet (masterPort, waitTime);
			if (result == IOReturn.Timeout)
				return true;
			IOObject.ThrowIfError (result);
			return false;
		}

		/// <summary>
		/// A request to create a connection to an IOService.
		/// </summary>
		/// <returns>The IOConnection. </returns>
		/// <param name="owningTask">The mach task requesting the connection.</param>
		/// <param name="type">A constant specifying the type of connection to be created, interpreted only by the IOService's family.</param>
		/// <remarks>A non-kernel client may request a connection be opened via the IOService.Open() method,
		/// which will call IOService::newUserClient in the kernel. The rules & capabilities of user level clients
		/// are family dependent, the default IOService implementation returns <see cref="ReturnType.Unsupported"/> (throws IOKitException).</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public IOConnection Open (Mach.Task owningTask, uint type)
		{
			ThrowIfDisposed ();
			IntPtr connectionRef;
			var result = IOServiceOpen (Handle, owningTask.Handle, type, out connectionRef);
			ThrowIfError (result);
			return new IOConnection (connectionRef, true);
		}

		/// <summary>
		/// A request to rescan a bus for device changes.
		/// </summary>
		/// <param name="options">An options mask, interpreted only by the IOService's family.</param>
		/// <remarks>A non-kernel client may request a bus or controller rescan for added or removed devices,
		/// if the bus family does automatically notice such changes. For example, SCSI bus controllers do not notice device changes.
		/// The implementation of this routine is family dependent, and the default IOService implementation returns
		/// <see cref="ReturnType.Unsupported"/> (throws IOKitException).</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public void RequestProbe (uint options)
		{
			ThrowIfDisposed ();
			var result = IOServiceRequestProbe (Handle, options);
			ThrowIfError (result);
		}

		/// <summary>
		/// Create a matching dictionary that specifies an IOService class match.
		/// </summary>
		/// <returns>The matching dictionary created, is returned on success, or <c>null</c> on failure.
		/// The dictionary is commonly passed to IOService.GetMatchingServices or IOService.AddNotification.
		/// </returns>
		/// <typeparam name="T">The class.</typeparam>
		/// Class matching is successful on IOService's of this class or any subclass.</param>
		/// <remarks>A very common matching criteria for IOService is based on its class.
		/// IOService.CreateMatchingDictionaryForClass will create a matching dictionary
		/// that specifies any IOService of a class, or its subclasses.</remarks>
		public static NSMutableDictionary CreateMatchingDictionaryForClass<T> () where T : IOService
		{
			var className = typeof(T).Name;
			var dictRef = IOServiceMatching (className);
			if (dictRef == IntPtr.Zero)
				return null;
			var dict = new NSMutableDictionary (dictRef);
			CFType.Release (dictRef);
			return dict;
		}

		/// <summary>
		/// Create a matching dictionary that specifies an IOService name match.
		/// </summary>
		/// <returns>The matching dictionary created, is returned on success, or <c>null</c> on failure.
		/// The dictionary is commonly passed to IOServiceGetMatchingServices or IOServiceAddNotification.
		/// </returns>
		/// <param name="name">The IOService name.</param>
		/// <remarks>A common matching criteria for IOService is based on its name.
		/// IOService.CreateMatchingDictionaryForName will create a matching dictionary that specifies an IOService with a given name.
		/// Some IOServices created from the device tree will perform name matching on the standard compatible,
		/// name, model properties.</remarks>
		public static NSMutableDictionary CreateMatchingDictionaryForName (string name)
		{
			var dictRef = IOServiceNameMatching (name);
			if (dictRef == IntPtr.Zero)
				return null;
			if (dictRef == IntPtr.Zero)
				return null;
			var dict = new NSMutableDictionary (dictRef);
			CFType.Release (dictRef);
			return dict;
		}

		/// <summary>
		/// Create a matching dictionary that specifies an IOService match based on BSD device name using the default master port.
		/// </summary>
		/// <returns>The matching dictionary created, is returned on success, or <c>null</c> on failure.
		/// The dictionary is commonly passed to IOServiceGetMatchingServices or IOServiceAddNotification.
		/// </returns>
		/// <param name="bsdName">The BSD name.</param>
		/// <remarks>IOServices that represent BSD devices have an associated BSD name.
		/// This function creates a matching dictionary that will match IOService's with a given BSD name.</remarks>
		public static NSMutableDictionary CreateMatchingDictionaryForBSDName (string bsdName)
		{
			return CreateMatchingDictionaryForBSDNameWithPort (kIOMasterPortDefault, bsdName);
		}

		internal static NSMutableDictionary CreateMatchingDictionaryForBSDNameWithPort (IntPtr masterPortRef, string bsdName)
		{
			var dictRef = IOBSDNameMatching (masterPortRef, 0, bsdName);
			if (dictRef == IntPtr.Zero)
				return null;
			var dict = new NSMutableDictionary (dictRef);
			CFType.Release (dictRef);
			return dict;
		}

		/// <summary>
		/// Create a matching dictionary that specifies an IOService match based on a registry entry ID.
		/// </summary>
		/// <returns>The matching dictionary created, is returned on success, or <c>null</c> on failure.
		/// The dictionary is commonly passed to IOServiceGetMatchingServices or IOServiceAddNotification.
		/// </returns>
		/// <param name="entryId">The registry entry ID to be found.</param>
		/// <remarks>This function creates a matching dictionary that will match a registered,
		/// active IOService found with the given registry entry ID. The entry ID for a registry entry is
		/// returned by IORegistryEntry.ID.</remarks>
		public static NSMutableDictionary CreateMatchingDictionaryForName (ulong entryId)
		{
			var dictRef = IORegistryEntryIDMatching (entryId);
			if (dictRef == IntPtr.Zero)
				return null;
			var dict = new NSMutableDictionary (dictRef);
			CFType.Release (dictRef);
			return dict;
		}
	

		/// <summary>
		/// Look up a registered IOService object that matches a matching dictionary.
		/// </summary>
		/// <returns>The first service matched is returned on success. The service must be released by the caller.</returns>
		/// <param name="masterPort">The master port obtained from IOMasterPort(). Pass kIOMasterPortDefault to look up the default master port.</param>
		/// <param name="matching">A CF dictionary containing matching information, of which one reference is always consumed
		/// by this function (Note prior to the Tiger release there was a small chance that the dictionary might not be released
		/// if there was an error attempting to serialize the dictionary). IOKitLib can construct matching dictionaries for common criteria
		/// with helper functions such as IOServiceMatching, IOServiceNameMatching, IOBSDNameMatching.</param>
		/// <remarks>This is the preferred method of finding IOService objects currently registered by IOKit
		/// (that is, objects that have had their registerService() methods invoked). To find IOService objects
		/// that aren't yet registered, use an iterator as created by IORegistryEntryCreateIterator().
		/// IOServiceAddMatchingNotification can also supply this information and install a notification of new IOServices.
		/// The matching information used in the matching dictionary may vary depending on the class of service being looked up.
		/// </remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static io_service_t IOServiceGetMatchingService (mach_port_t masterPort, CFDictionaryRef matching);

		/// <summary>
		/// Look up registered IOService objects that match a matching dictionary.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="masterPort">The master port obtained from IOMasterPort(). Pass kIOMasterPortDefault to look up the default master port.</param>
		/// <param name="matching">A CF dictionary containing matching information, of which one reference is always consumed by this function
		/// (Note prior to the Tiger release there was a small chance that the dictionary might not be released if there was an error attempting
		/// to serialize the dictionary). IOKitLib can construct matching dictionaries for common criteria with helper functions
		/// such as IOServiceMatching, IOServiceNameMatching, IOBSDNameMatching.</param>
		/// <param name="existing">An iterator handle is returned on success, and should be released by the caller when the iteration is finished.</param>
		/// <remarks>This is the preferred method of finding IOService objects currently registered by IOKit
		/// (that is, objects that have had their registerService() methods invoked). To find IOService objects that aren't yet registered,
		/// use an iterator as created by IORegistryEntryCreateIterator(). IOServiceAddMatchingNotification can also supply this information
		/// and install a notification of new IOServices. The matching information used in the matching dictionary may vary depending
		/// on the class of service being looked up.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceGetMatchingServices (mach_port_t masterPort, CFDictionaryRef matching, out io_iterator_t existing);

		/// <summary>
		/// Look up registered IOService objects that match a matching dictionary, and install a notification request of new IOServices that match.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="notifyPort">A IONotificationPortRef object that controls how messages will be sent when the armed notification is fired.
		/// When the notification is delivered, the io_iterator_t representing the notification should be iterated through to pick up all outstanding objects.
		/// When the iteration is finished the notification is rearmed. See IONotificationPortCreate.</param>
		/// <param name="notificationType">A notification type from IOKitKeys.h
		/// <list type="bullet">
		/// <item>kIOPublishNotification Delivered when an IOService is registered.</item>
		/// <item>kIOFirstPublishNotification Delivered when an IOService is registered, but only once per IOService instance.
		/// Some IOService's may be reregistered when their state is changed.</item>
		/// <item>kIOMatchedNotification Delivered when an IOService has had all matching drivers in the kernel probed and started.</item>
		/// <item>kIOFirstMatchNotification Delivered when an IOService has had all matching drivers in the kernel probed and started,
		/// but only once per IOService instance. Some IOService's may be reregistered when their state is changed.</item>
		/// <item>kIOTerminatedNotification Delivered after an IOService has been terminated.</item>
		/// </list></param>
		/// <param name="matching">A CF dictionary containing matching information, of which one reference is always consumed by this function
		/// (Note prior to the Tiger release there was a small chance that the dictionary might not be released if there was an error attempting to serialize the dictionary).
		/// IOKitLib can construct matching dictionaries for common criteria with helper functions such as IOServiceMatching, IOServiceNameMatching, IOBSDNameMatching.</param>
		/// <param name="callback">A callback function called when the notification fires.</param>
		/// <param name="refCon">A reference constant for the callbacks use.</param>
		/// <param name="notification">An iterator handle is returned on success, and should be released by the caller when the notification is to be destroyed.
		/// The notification is armed when the iterator is emptied by calls to IOIteratorNext - when no more objects are returned, the notification is armed.
		/// Note the notification is not armed when first created.</param>
		/// <remarks>This is the preferred method of finding IOService objects that may arrive at any time.
		/// The type of notification specifies the state change the caller is interested in, on IOService's that match the match dictionary.
		/// Notification types are identified by name, and are defined in IOKitKeys.h. The matching information used in the matching dictionary
		/// may vary depending on the class of service being looked up.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceAddMatchingNotification (IONotificationPortRef notifyPort, io_name_t notificationType, CFDictionaryRef matching,
		                                                              IOServiceMatchingCallback callback, IntPtr refCon, out io_iterator_t notification);

		/// <summary>
		/// Register for notification of state changes in an IOService.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="notifyPort">A IONotificationPortRef object that controls how messages will be sent when the notification is fired.
		/// See IONotificationPortCreate.</param>
		/// <param name="service">The IOService object to match.</param>
		/// <param name="interestType">A notification type from IOKitKeys.h
		/// <list type="bullet">
		/// <item>kIOGeneralInterest General state changes delivered via the IOService::message API.</item>
		/// <item>kIOBusyInterest Delivered when the IOService changes its busy state to or from zero.
		/// The message argument contains the new busy state causing the notification.</item>
		/// </list></param>
		/// <param name="callback">A callback function called when the notification fires, with messageType and messageArgument for the state change.</param>
		/// <param name="refCon">A reference constant for the callbacks use.</param>
		/// <param name="notification">An object handle is returned on success, and should be released by the caller when the notification is to be destroyed.</param>
		/// <remarks>IOService objects deliver notifications of their state changes to their clients via the IOService::message API,
		/// and to other interested parties including callers of this function. Message type s are defined IOKit/IOMessage.h.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceAddInterestNotification (IONotificationPortRef notifyPort, io_service_t service, io_name_t interestType,
		                                                              IOServiceInterestCallback callback, IntPtr refCon, out io_object_t notification);

		/// <summary>
		/// Match an IOService objects with matching dictionary.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="service">The IOService object to match.</param>
		/// <param name="matching">A CF dictionary containing matching information.
		/// IOKitLib can construct matching dictionaries for common criteria with helper functions such as IOServiceMatching,
		/// IOServiceNameMatching, IOBSDNameMatching.</param>
		/// <param name="matches">The boolean result is returned.</param>
		/// <remarks>This function calls the matching method of an IOService object and returns the boolean result.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceMatchPropertyTable (io_service_t service, CFDictionaryRef matching, out boolean_t matches);

		/// <summary>
		/// Returns the busyState of an IOService.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="service">The IOService whose busyState to return.</param>
		/// <param name="busyState">The busyState count is returned.</param>
		/// <remarks>Many activities in IOService are asynchronous. When registration, matching, or termination is in progress
		/// on an IOService, its busyState is increased by one. Change in busyState to or from zero also changes the IOService's
		/// provider's busyState by one, which means that an IOService is marked busy when any of the above activities is ocurring
		/// on it or any of its clients.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceGetBusyState (io_service_t service, out uint32_t busyState);

		/// <summary>
		/// Wait for an IOService's busyState to be zero.
		/// </summary>
		/// <returns>Returns an error code if mach synchronization primitives fail, kIOReturnTimeout, or kIOReturnSuccess.</returns>
		/// <param name="service">The IOService wait on.</param>
		/// <param name="waitTime">Specifies a maximum time to wait.</param>
		/// <remarks>Blocks the caller until an IOService is non busy, see IOServiceGetBusyState.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceWaitQuiet (io_service_t service, mach_timespec_t waitTime);


		/// <summary>
		/// Returns the busyState of all IOServices.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="masterPort">The master port obtained from IOMasterPort().
		/// Pass kIOMasterPortDefault to look up the default master port.</param>
		/// <param name="busyState">The busyState count is returned.</param>
		/// <remarks>Many activities in IOService are asynchronous. When registration, matching,
		/// or termination is in progress on an IOService, its busyState is increased by one.
		/// Change in busyState to or from zero also changes the IOService's provider's busyState
		/// by one, which means that an IOService is marked busy when any of the above activities
		/// is ocurring on it or any of its clients. IOKitGetBusyState returns the busy state
		/// of the root of the service plane which reflects the busy state of all IOServices.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOKitGetBusyState(mach_port_t masterPort, out uint32_t busyState);

		/// <summary>
		/// Wait for a all IOServices' busyState to be zero.
		/// </summary>
		/// <returns>Returns an error code if mach synchronization primitives fail, kIOReturnTimeout, or kIOReturnSuccess.</returns>
		/// <param name="masterPort">The master port obtained from IOMasterPort().
		/// Pass kIOMasterPortDefault to look up the default master port.</param>
		/// <param name="waitTime">Specifies a maximum time to wait.</param>
		/// <remarks>Blocks the caller until all IOServices are non busy, see IOKitGetBusyState.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOKitWaitQuiet(mach_port_t masterPort, mach_timespec_t waitTime);

		/// <summary>
		/// A request to create a connection to an IOService.
		/// </summary>
		/// <returns>A return code generated by IOService::newUserClient. </returns>
		/// <param name="service">The IOService object to open a connection to, usually obtained via the IOServiceGetMatchingServices or IOServiceAddNotification APIs.</param>
		/// <param name="owningTask">The mach task requesting the connection.</param>
		/// <param name="type">A constant specifying the type of connection to be created,  interpreted only by the IOService's family.</param>
		/// <param name="connect">An io_connect_t handle is returned on success, to be used with the IOConnectXXX APIs. It should be destroyed with IOServiceClose().</param>
		/// <remarks>A non kernel client may request a connection be opened via the IOServiceOpen() library function,
		/// which will call IOService::newUserClient in the kernel. The rules & capabilities of user level clients
		/// are family dependent, the default IOService implementation returns kIOReturnUnsupported.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceOpen (io_service_t service, task_port_t owningTask, uint32_t type, out io_connect_t connect);

		/// <summary>
		/// A request to rescan a bus for device changes.
		/// </summary>
		/// <returns>A return code generated by IOService::requestProbe.</returns>
		/// <param name="service">The IOService object to request a rescan,
		/// usually obtained via the IOServiceGetMatchingServices or IOServiceAddNotification APIs.</param>
		/// <param name="options">An options mask, interpreted only by the IOService's family.</param>
		/// <remarks>A non kernel client may request a bus or controller rescan for added or removed devices,
		/// if the bus family does automatically notice such changes. For example, SCSI bus controllers do not notice device changes.
		/// The implementation of this routine is family dependent, and the default IOService implementation returns kIOReturnUnsupported.
		/// </remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceRequestProbe (io_service_t service, uint32_t options);


		/// <summary>
		/// Create a matching dictionary that specifies an IOService class match.
		/// </summary>
		/// <returns>The matching dictionary created, is returned on success, or zero on failure.
		/// The dictionary is commonly passed to IOServiceGetMatchingServices or
		/// IOServiceAddNotification which will consume a reference, otherwise it should be released with CFRelease by the caller. </returns>
		/// <param name="name">The class name, as a const C-string. Class matching is successful on IOService's of this class or any subclass.</param>
		/// <remarks>A very common matching criteria for IOService is based on its class.
		/// IOServiceMatching will create a matching dictionary that specifies any IOService of a class,
		/// or its subclasses. The class is specified by C-string name.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static CFMutableDictionaryRef IOServiceMatching (io_name_t name);

		/// <summary>
		/// Create a matching dictionary that specifies an IOService name match.
		/// </summary>
		/// <returns>The matching dictionary created, is returned on success, or zero on failure.
		/// The dictionary is commonly passed to IOServiceGetMatchingServices or IOServiceAddNotification
		/// which will consume a reference, otherwise it should be released with CFRelease by the caller.</returns>
		/// <param name="name">The IOService name, as a const C-string.</param>
		/// <remarks>A common matching criteria for IOService is based on its name.
		/// IOServiceNameMatching will create a matching dictionary that specifies an IOService with a given name.
		/// Some IOServices created from the device tree will perform name matching on the standard compatible,
		/// name, model properties.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static CFMutableDictionaryRef IOServiceNameMatching (io_name_t name);

		/// <summary>
		/// Create a matching dictionary that specifies an IOService match based on BSD device name.
		/// </summary>
		/// <returns>The matching dictionary created, is returned on success, or zero on failure.
		/// The dictionary is commonly passed to IOServiceGetMatchingServices or IOServiceAddNotification
		/// which will consume a reference, otherwise it should be released with CFRelease by the caller.</returns>
		/// <param name="masterPort">The master port obtained from IOMasterPort(). Pass kIOMasterPortDefault to look up the default master port.</param>
		/// <param name="options">No options are currently defined.</param>
		/// <param name="bsdName">The BSD name, as a const char *.</param>
		/// <remarks>IOServices that represent BSD devices have an associated BSD name.
		/// This function creates a matching dictionary that will match IOService's with a given BSD name.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static CFMutableDictionaryRef IOBSDNameMatching (mach_port_t masterPort, uint32_t options, io_name_t bsdName);

		/// <summary>
		/// Create a matching dictionary that specifies an IOService match based on a registry entry ID.
		/// </summary>
		/// <returns>The matching dictionary created, is returned on success, or zero on failure.
		/// The dictionary is commonly passed to IOServiceGetMatchingServices or IOServiceAddNotification
		/// which will consume a reference, otherwise it should be released with CFRelease by the caller.</returns>
		/// <param name="entryID">The registry entry ID to be found.</param>
		/// <remarks>This function creates a matching dictionary that will match a registered,
		/// active IOService found with the given registry entry ID. The entry ID for a registry entry is
		/// returned by IORegistryEntryGetRegistryEntryID().</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static CFMutableDictionaryRef IORegistryEntryIDMatching (uint64_t entryID);

		/// <summary>Callback function to be notified of changes in state of an IOService.</summary>
		/// <param name="refcon">The refcon passed when the notification was installed.</param>
		/// <param name="service">The IOService whose state has changed.</param>
		/// <param name="messageType">An enum, defined by <see cref="IOKit.IOMessage"/> or by the IOService's family.</param>
		/// <param name="messageArgument">An argument for the message, dependent on the messageType.
		/// If the message data is larger than sizeof(void*), then messageArgument contains a pointer to the message data;
		/// otherwise, messageArgument contains the message data.</param>
		delegate void IOServiceInterestCallback (IntPtr refcon, io_service_t service, UInt32 messageType, IntPtr messageArgument);

		/// <summary>Callback function to be notified of IOService publication.</summary>
		/// <param name="refcon">The refcon passed when the notification was installed.</param>
		/// <param name="iterator">The notification iterator which now has new objects.</param>
		delegate void IOServiceMatchingCallback (IntPtr refcon, io_iterator_t iterator);

		/// <summary>Callback function to be notified of changes in state of an IOService.</summary>
		/// <param name="service">The IOService whose state has changed.</param>
		/// <param name="messageType">An enum, defined by <see cref="IOKit.IOMessage"/> or by the IOService's family.</param>
		/// <param name="messageArgument">An argument for the message, dependent on the messageType.
		/// If the message data is larger than sizeof(void*), then messageArgument contains a pointer to the message data;
		/// otherwise, messageArgument contains the message data.</param>
		public delegate void InterestCallback (IOService service, UInt32 messageType, IntPtr messageArgument);

		/// <summary>Callback function to be notified of IOService publication.</summary>
		/// <param name="matchingServices">The notification iterator which now has new objects.</param>
		public delegate void MatchingCallback<T> (IOIterator<T> matchingServices) where T : IOService;
	}

    /// <summary> standard callback function for asynchronous I/O requests with
	/// no extra arguments beyond a refcon and result code.</summary>
	/// <param="refcon">The refcon passed into the original I/O request</param>
	/// <param="result">The result of the I/O operation</param>
	public delegate void IOAsyncCallback0 (IntPtr refcon, IOReturn result);

    /// <summary> standard callback function for asynchronous I/O requests with
	/// one extra argument beyond a refcon and result code.
	/// This is often a count of the number of bytes transferred</summary>
	/// <param="refcon The refcon passed into the original I/O request</param>
	/// <param="result">The result of the I/O operation</param>
	/// <param="arg0">Extra argument</param>
	public delegate void IOAsyncCallback1 (IntPtr refcon, IOReturn result, IntPtr arg0);

    /// <summary> standard callback function for asynchronous I/O requests with
	/// two extra arguments beyond a refcon and result code.</summary>
	/// <param="refcon">The refcon passed into the original I/O request
	/// <param="result">The result of the I/O operation</param>
	/// <param="arg0">Extra argument</param>
	/// <param="arg1">Extra argument</param>
	public delegate void IOAsyncCallback2 (IntPtr refcon, IOReturn result, IntPtr arg0, IntPtr arg1);

    /// <summary> standard callback function for asynchronous I/O requests with
	/// lots of extra arguments beyond a refcon and result code.</summary>
	/// <param="refcon">The refcon passed into the original I/O request</param>
	/// <param="result">The result of the I/O operation</param>
	/// <param="args">Array of extra arguments</param>
	/// <param="numArgs">Number of extra arguments</param>
	public delegate void IOAsyncCallback (IntPtr refcon, IOReturn result, IntPtr[] args,
                                          uint32_t numArgs);

}

