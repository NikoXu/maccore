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
using MonoMac.Kernel.Mach;
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
using mach_timespec_t = MonoMac.Kernel.Mach.TimeSpec;
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

		#region API wrappers

		[DllImport (Constants.IOKitLibrary)]
		extern static io_service_t IOServiceGetMatchingService (mach_port_t masterPort, CFDictionaryRef matching);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceGetMatchingServices (mach_port_t masterPort, CFDictionaryRef matching, out io_iterator_t existing);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceAddMatchingNotification (IONotificationPortRef notifyPort, io_name_t notificationType, CFDictionaryRef matching,
		                                                              IOServiceMatchingCallback callback, IntPtr refCon, out io_iterator_t notification);

		public static IOIterator<T> AddMatchingNotification<T> (IONotificationPort notifyPort, NotificationType notificationType,
		                                                        NSDictionary matchingDictionary, MatchingCallback<T> callback) where T : IOService
		{
			var notificationTypeString = notificationType.GetKey ();
			IOServiceMatchingCallback nativeCallback = (refCon, iteratorRef) =>
			{
				var iterator = MarshalNativeObject<IOIterator<T>> (iteratorRef, false);
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

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceAddInterestNotification (IONotificationPortRef notifyPort, io_service_t service, io_name_t interestType,
		                                                              IOServiceInterestCallback callback, IntPtr refCon, out io_object_t notification);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceMatchPropertyTable (io_service_t service, CFDictionaryRef matching, out boolean_t matches);

		public bool MatchPropertyTable (NSDictionary matchingDictionary)
		{
			ThrowIfDisposed ();
			bool matches;
			var result = IOServiceMatchPropertyTable (Handle, matchingDictionary.Handle, out matches);
			ThrowIfError (result);
			return matches;
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceGetBusyState (io_service_t service, out uint32_t busyState);

		public bool IsBusy {
			get { return BusyState > 0; }
		}

		public uint BusyState {
			get {
				ThrowIfDisposed ();
				uint busyState;
				var result = IOServiceGetBusyState (Handle, out busyState);
				ThrowIfError (result);
				return busyState;
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceWaitQuiet (io_service_t service, mach_timespec_t waitTime);

		public bool Wait (double seconds)
		{
			ThrowIfDisposed ();
			var integerPart = (uint)Math.Floor (seconds);
			var decimalPart = seconds - integerPart;
			var waitTime = new TimeSpec () { Seconds = integerPart, Nanoseconds = (int)(decimalPart * 1e9) };
			var result = IOServiceWaitQuiet (Handle, waitTime);
			if (result == IOReturn.Timeout)
				return true;
			ThrowIfError (result);
			return false;
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOKitGetBusyState (mach_port_t masterPort, out uint32_t busyState);

		public static bool IsAnyBusy {
			get { return AllBusyState > 0; }
		}

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

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOKitWaitQuiet(mach_port_t masterPort, mach_timespec_t waitTime);

		public static bool WaitAll (double seconds)
		{
			return WaitForPort (kIOMasterPortDefault, seconds);
		}

		internal static bool WaitForPort (IntPtr masterPort, double seconds)
		{
			var integerPart = (uint)Math.Floor (seconds);
			var decimalPart = seconds - integerPart;
			var waitTime = new TimeSpec () { Seconds = integerPart, Nanoseconds = (int)(decimalPart * 1e9) };
			var result = IOKitWaitQuiet (masterPort, waitTime);
			if (result == IOReturn.Timeout)
				return true;
			IOObject.ThrowIfError (result);
			return false;
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceOpen (io_service_t service, task_port_t owningTask, uint32_t type, out io_connect_t connect);

		public IOConnection Open (Task owningTask, uint type)
		{
			ThrowIfDisposed ();
			IntPtr connectionRef;
			var result = IOServiceOpen (Handle, owningTask.Handle, type, out connectionRef);
			ThrowIfError (result);
			return new IOConnection (connectionRef, true);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceRequestProbe (io_service_t service, uint32_t options);

		public void RequestProbe (uint options)
		{
			ThrowIfDisposed ();
			var result = IOServiceRequestProbe (Handle, options);
			ThrowIfError (result);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFMutableDictionaryRef IOServiceMatching (io_name_t name);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static CFMutableDictionaryRef IOServiceNameMatching (io_name_t name);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static CFMutableDictionaryRef IOBSDNameMatching (mach_port_t masterPort, uint32_t options, io_name_t bsdName);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static CFMutableDictionaryRef IORegistryEntryIDMatching (uint64_t entryID);

		public static NSMutableDictionary CreateMatchingDictionaryForName (ulong entryId)
		{
			var dictRef = IORegistryEntryIDMatching (entryId);
			if (dictRef == IntPtr.Zero)
				return null;
			var dict = new NSMutableDictionary (dictRef);
			CFType.Release (dictRef);
			return dict;
		}

		delegate void IOServiceInterestCallback (IntPtr refcon, io_service_t service, UInt32 messageType, IntPtr messageArgument);

		delegate void IOServiceMatchingCallback (IntPtr refcon, io_iterator_t iterator);

		public delegate void InterestCallback (IOService service, UInt32 messageType, IntPtr messageArgument);

		public delegate void MatchingCallback<T> (IOIterator<T> matchingServices) where T : IOService;

		#endregion
	}

	internal delegate void IOAsyncCallback0 (IntPtr refcon, IOReturn result);

	internal delegate void IOAsyncCallback1 (IntPtr refcon, IOReturn result, IntPtr arg0);

	internal delegate void IOAsyncCallback2 (IntPtr refcon, IOReturn result, IntPtr arg0, IntPtr arg1);

	internal delegate void IOAsyncCallback (IntPtr refcon, IOReturn result, IntPtr[] args, uint32_t numArgs);
}

