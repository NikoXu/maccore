//
// IOHIDManager.cs
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
using MonoMac.CoreFoundation;
using MonoMac.ObjCRuntime;
using System.Runtime.InteropServices;
using MonoMac;
using MonoMac.IOKit;
using System.Runtime.CompilerServices;
using MonoMac.Foundation;
using System.Collections.Generic;

using CFTypeID = System.UInt32;
using IOOptionBits = System.UInt32;
using CFAllocatorRef = System.IntPtr;
using IOHIDManagerRef = System.IntPtr;
using CFTypeRef = System.IntPtr;
using CFStringRef = System.IntPtr;
using CFRunLoopRef = System.IntPtr;
using CFDictionaryRef = System.IntPtr;
using CFArrayRef = System.IntPtr;
using CFSetRef = System.IntPtr;
using System.Linq;

namespace MonoMac.IOKit.HID
{
	[Since (5,0)]
	public class IOHIDManager : CFType
	{
		internal IOHIDManager (IntPtr handle, bool owns)
			: base (handle, owns)
		{
			IOHIDManagerRegisterDeviceMatchingCallback (Handle, MatchingDeviceFoundCallback, IntPtr.Zero);
			IOHIDManagerRegisterDeviceRemovalCallback (Handle, DeviceRemovedCallback, IntPtr.Zero);
			IOHIDManagerRegisterInputValueCallback (Handle, InputValueReceivedCallback, IntPtr.Zero);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeID IOHIDManagerGetTypeID (); 

		public static uint TypeID {
			get { return IOHIDManagerGetTypeID (); }
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDManagerRef IOHIDManagerCreate (
			CFAllocatorRef                  allocator,
			IOOptionBits                    options);

		public IOHIDManager (IOHIDManagerOptions options = IOHIDManagerOptions.None)
			: this (IOHIDManagerCreate (IntPtr.Zero, (uint)options), true)
		{
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDManagerOpen (
			IOHIDManagerRef                 manager,
			IOOptionBits                    options);

		public void Open (IOHIDOptionsType options = IOHIDOptionsType.None)
		{
			ThrowIfDisposed ();
			var result = IOHIDManagerOpen (Handle, (uint)options);
			IOObject.ThrowIfError (result);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDManagerClose (
		                                IOHIDManagerRef                 manager,
		                                IOOptionBits                    options);

		public void Close (IOHIDOptionsType options = IOHIDOptionsType.None)
		{
			ThrowIfDisposed ();
			var result = IOHIDManagerClose (Handle, (uint)options);
			IOObject.ThrowIfError (result);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeRef IOHIDManagerGetProperty (
		                                IOHIDManagerRef                 manager,
		                                CFStringRef                     key);
		                                

		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDManagerSetProperty (
		                                IOHIDManagerRef                 manager,
		                                CFStringRef                     key,
		                                CFTypeRef                       value);

		[IndexerName ("Properties")]
		public NSObject this [string key] {
			get {
				ThrowIfDisposed ();
				if (key == null)
					throw new ArgumentNullException ("propertyName");
				using (var propertyNameString = new CFString (key)) {
					var valueRef = IOHIDManagerGetProperty (Handle, propertyNameString.Handle);
					if (valueRef == IntPtr.Zero)
						return null;
					return Runtime.GetNSObject (valueRef);
				}
			}
			set {
				ThrowIfDisposed ();
				if (key == null)
					throw new ArgumentNullException ("propertyName");
				using (var propertyNameString = new CFString (key)) {
					if (!IOHIDManagerSetProperty (Handle, propertyNameString.Handle, value.Handle))
						throw new Exception ("Failed to set property");
				}
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerScheduleWithRunLoop (
		                                IOHIDManagerRef                 manager,
		                                CFRunLoopRef                    runLoop, 
		                                CFStringRef                     runLoopMode);

		public void ScheduleWithRunLoop (CFRunLoop runLoop, NSString mode)
		{
			ThrowIfDisposed ();
			if (runLoop == null)
				throw new ArgumentNullException ("runLoop");
			if (mode == null)
				throw new ArgumentNullException ("mode");
			IOHIDManagerScheduleWithRunLoop (Handle, runLoop.Handle, mode.Handle);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerUnscheduleFromRunLoop (
		                                IOHIDManagerRef                 manager,
		                                CFRunLoopRef                    runLoop, 
		                                CFStringRef                     runLoopMode);

		public void UnscheduleFromRunLoop (CFRunLoop runLoop, NSString mode)
		{
			ThrowIfDisposed ();
			if (runLoop == null)
				throw new ArgumentNullException ("runLoop");
			if (mode == null)
				throw new ArgumentNullException ("mode");
			IOHIDManagerUnscheduleFromRunLoop (Handle, runLoop.Handle, mode.Handle);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerSetDeviceMatching (
		                                IOHIDManagerRef                 manager,
		                                CFDictionaryRef                 matching);

		public void SetDeviceMatching (IDictionary<string, ValueType> matchingDictionary)
		{
			SetMatching (matchingDictionary, IOHIDManagerSetDeviceMatching);
		}

		void SetMatching (IDictionary<string, ValueType> matchingDictionary, Action<IOHIDManagerRef,
		                  CFDictionaryRef> matchingFunction)
		{
			ThrowIfDisposed ();
			if (matchingDictionary == null)
				throw new ArgumentNullException ("matchingDictionary");
			using (var matching = NSDictionary.FromObjectsAndKeys (matchingDictionary.Values.ToArray (),
			                                                       matchingDictionary.Keys.ToArray ()))
				matchingFunction (Handle, matching.Handle);
		}

		public void ClearDeviceMatching ()
		{
			using (var matching = new NSDictionary ())
				IOHIDManagerSetDeviceMatching (Handle, matching.Handle);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerSetDeviceMatchingMultiple (
		                                IOHIDManagerRef                 manager,
		                                CFArrayRef                      multiple);

		public void SetDeviceMatchingMultiple (ICollection<IDictionary<string, ValueType>> matchingDictionaries)
		{
			SetMatchingMultiple (matchingDictionaries, IOHIDManagerSetDeviceMatchingMultiple);
		}

		void SetMatchingMultiple (ICollection<IDictionary<string, ValueType>> matchingDictionaries,
		                          Action<IOHIDManagerRef, CFDictionaryRef> matchingFunction)
		{
			ThrowIfDisposed ();
			if (matchingDictionaries == null) 
				throw new ArgumentNullException ("matchingDictionaries");
			var dictonaryPtrs = new IntPtr [matchingDictionaries.Count];
			var i = 0;
			foreach (var dict in matchingDictionaries)
				dictonaryPtrs [i++] = NSDictionary.FromObjectsAndKeys (dict.Values.ToArray (),
				                                                       dict.Keys.ToArray ()).Handle;
			var array = NSArray.FromIntPtrs (dictonaryPtrs);
			matchingFunction (Handle, array.Handle);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFSetRef IOHIDManagerCopyDevices (IOHIDManagerRef manager);

		public IOHIDDevice[] GetDevices ()
		{
			ThrowIfDisposed ();
			var setRef = IOHIDManagerCopyDevices (Handle);
			if (setRef == IntPtr.Zero)
				return new IOHIDDevice[0];
			using (var deviceSet = new CFSet (setRef, true)) {
				var devices = new IOHIDDevice[deviceSet.Count];
				int i = 0;
				foreach (var devicePtr in deviceSet.Values)
					devices [i++] = GetCFObject<IOHIDDevice> (devicePtr);
				return devices;
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerRegisterDeviceMatchingCallback(
		                                IOHIDManagerRef                 manager,
		                                IOHIDDeviceCallback             callback,
		                                IntPtr                          context);

		public event EventHandler<IOHIDDeviceEventArgs> MatchingDeviceFound;

		static void MatchingDeviceFoundCallback (IntPtr context, IOReturn result,
		                                         IntPtr senderRef, IntPtr deviceRef)
		{
			var manager = GetCFObject<IOHIDManager> (senderRef);
			var device = GetCFObject<IOHIDDevice> (deviceRef);
			manager.OnMatchingDeviceFound (result, device);
		}

		void OnMatchingDeviceFound (IOReturn result, IOHIDDevice device)
		{
			if (MatchingDeviceFound != null)
				MatchingDeviceFound (this, new IOHIDDeviceEventArgs (result, device));
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerRegisterDeviceRemovalCallback(
		                                IOHIDManagerRef                 manager,
		                                IOHIDDeviceCallback             callback,
		                                IntPtr                          context);

		public event EventHandler<IOHIDDeviceEventArgs> DeviceRemoved;

		static void DeviceRemovedCallback (IntPtr context, IOReturn result, IntPtr senderRef, IntPtr deviceRef)
		{
			var manager = GetCFObject<IOHIDManager> (senderRef);
			var device = GetCFObject<IOHIDDevice> (deviceRef);
			manager.OnDeviceRemoved (result, device);
		}

		void OnDeviceRemoved (IOReturn result, IOHIDDevice device)
		{
			if (DeviceRemoved != null)
				DeviceRemoved (this, new IOHIDDeviceEventArgs (result, device));
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerRegisterInputReportCallback( 
		                                    IOHIDManagerRef             manager,
		                                    IOHIDReportCallback         callback,
		                                    IntPtr                      context);

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerRegisterInputValueCallback (
			IOHIDManagerRef                 manager,
			IOHIDValueCallback              callback,
			IntPtr                          context);

		public event EventHandler<IOHIDValueEventArgs> InputValueReceived;

		static void InputValueReceivedCallback (IntPtr context, IOReturn result,
		                                        IntPtr senderRef, IntPtr valueRef)
		{
			return;
			// TODO: sender can be IOHIDQueue object
			var manager = GetCFObject<IOHIDManager> (senderRef);
			var value = GetCFObject<IOHIDValue> (valueRef);
			manager.OnInputValueReceived (result, value);
		}

		void OnInputValueReceived (IOReturn result, IOHIDValue value)
		{
			if (InputValueReceived != null)
				InputValueReceived (this, new IOHIDValueEventArgs (result, value));
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerSetInputValueMatching (
		                                IOHIDManagerRef                 manager,
		                                CFDictionaryRef                 matching);

		public void SetInputValueMatching (IDictionary<string, ValueType> matchingDictionary)
		{
			SetMatching (matchingDictionary, IOHIDManagerSetInputValueMatching);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerSetInputValueMatchingMultiple (
		                                               IOHIDManagerRef                 manager,
		                                               CFArrayRef                      multiple);

		public void SetInputValueMatchingMultiple (ICollection<IDictionary<string, ValueType>> matchingDictionaries)
		{
			SetMatchingMultiple (matchingDictionaries, IOHIDManagerSetInputValueMatchingMultiple);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerSaveToPropertyDomain(
		                                      IOHIDManagerRef                 manager,
		                                      CFStringRef                     applicationID,
		                                      CFStringRef                     userName,
		                                      CFStringRef                     hostName,
		                                      IOOptionBits                    options);
		//AVAILABLE_MAC_OS_X_VERSION_10_6_AND_LATER

	}
}

