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

		/*!
			@function   IOHIDManagerGetTypeID
			@abstract   Returns the type identifier of all IOHIDManager instances.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeID IOHIDManagerGetTypeID (); 
		////AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		public static uint TypeID {
			get { return IOHIDManagerGetTypeID (); }
		}

		/*!
			@function   IOHIDManagerCreate
			@abstract   Creates an IOHIDManager object.
		    @discussion The IOHIDManager object is meant as a global management system
		                for communicating with HID devices.
		    @param      allocator Allocator to be used during creation.
		    @param      options Supply @link kIOHIDManagerOptionUsePersistentProperties @/link to load
		                properties from the default persistent property store. Otherwise supply
		                @link kIOHIDManagerOptionNone @/link (or 0).
		    @result     Returns a new IOHIDManagerRef.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDManagerRef IOHIDManagerCreate (
			CFAllocatorRef                  allocator,
			IOOptionBits                    options);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		public IOHIDManager (IOHIDManagerOptions options = IOHIDManagerOptions.None)
			: this (IOHIDManagerCreate (IntPtr.Zero, (uint)options), true)
		{
		}

		/*!
			@function   IOHIDManagerOpen
			@abstract   Opens the IOHIDManager.
		    @discussion This will open both current and future devices that are 
		                enumerated. To establish an exclusive link use the 
		                kIOHIDOptionsTypeSeizeDevice option. 
		    @param      manager Reference to an IOHIDManager.
		    @param      options Option bits to be sent down to the manager and device.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDManagerOpen (
			IOHIDManagerRef                 manager,
			IOOptionBits                    options);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		public void Open (IOHIDOptionsType options = IOHIDOptionsType.None)
		{
			ThrowIfDisposed ();
			var result = IOHIDManagerOpen (Handle, (uint)options);
			IOObject.ThrowIfError (result);
		}

		/*!
			@function   IOHIDManagerClose
			@abstract   Closes the IOHIDManager.
		    @discussion This will also close all devices that are currently enumerated.
		    @param      manager Reference to an IOHIDManager.
		    @param      options Option bits to be sent down to the manager and device.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDManagerClose (
		                                IOHIDManagerRef                 manager,
		                                IOOptionBits                    options);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		public void Close (IOHIDOptionsType options = IOHIDOptionsType.None)
		{
			ThrowIfDisposed ();
			var result = IOHIDManagerClose (Handle, (uint)options);
			IOObject.ThrowIfError (result);
		}

		/*!
			@function   IOHIDManagerGetProperty
			@abstract   Obtains a property of an IOHIDManager.
		    @discussion Property keys are prefixed by kIOHIDDevice and declared in 
		                <IOKit/hid/IOHIDKeys.h>.
		    @param      manager Reference to an IOHIDManager.
		    @param      key CFStringRef containing key to be used when querying the 
		                manager.
		    @result     Returns CFTypeRef containing the property.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeRef IOHIDManagerGetProperty (
		                                IOHIDManagerRef                 manager,
		                                CFStringRef                     key);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER
		                                
		/*!
			@function   IOHIDManagerSetProperty
			@abstract   Sets a property for an IOHIDManager.
		    @discussion Property keys are prefixed by kIOHIDDevice and kIOHIDManager and
		                declared in <IOKit/hid/IOHIDKeys.h>. This method will propagate 
		                any relevent properties to current and future devices that are 
		                enumerated.
		    @param      manager Reference to an IOHIDManager.
		    @param      key CFStringRef containing key to be used when modifiying the 
		                device property.
		    @param      value CFTypeRef containing the property value to be set.
		    @result     Returns TRUE if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDManagerSetProperty (
		                                IOHIDManagerRef                 manager,
		                                CFStringRef                     key,
		                                CFTypeRef                       value);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

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

		/*! @function   IOHIDManagerScheduleWithRunLoop
		    @abstract   Schedules HID manager with run loop.
		    @discussion Formally associates manager with client's run loop. Scheduling
		                this device with the run loop is necessary before making use of
		                any asynchronous APIs.  This will propagate to current and 
		                future devices that are enumerated.
		    @param      manager Reference to an IOHIDManager.
		    @param      runLoop RunLoop to be used when scheduling any asynchronous 
		                activity.
		    @param      runLoopMode Run loop mode to be used when scheduling any 
		                asynchronous activity.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerScheduleWithRunLoop (
		                                IOHIDManagerRef                 manager,
		                                CFRunLoopRef                    runLoop, 
		                                CFStringRef                     runLoopMode);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		public void ScheduleWithRunLoop (CFRunLoop runLoop, NSString mode)
		{
			ThrowIfDisposed ();
			if (runLoop == null)
				throw new ArgumentNullException ("runLoop");
			if (mode == null)
				throw new ArgumentNullException ("mode");
			IOHIDManagerScheduleWithRunLoop (Handle, runLoop.Handle, mode.Handle);
		}

		/*! @function   IOHIDManagerUnscheduleFromRunLoop
		    @abstract   Unschedules HID manager with run loop.
		    @discussion Formally disassociates device with client's run loop. This will 
		                propagate to current devices that are enumerated.
		    @param      manager Reference to an IOHIDManager.
		    @param      runLoop RunLoop to be used when unscheduling any asynchronous 
		                activity.
		    @param      runLoopMode Run loop mode to be used when unscheduling any 
		                asynchronous activity.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerUnscheduleFromRunLoop (
		                                IOHIDManagerRef                 manager,
		                                CFRunLoopRef                    runLoop, 
		                                CFStringRef                     runLoopMode);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		public void UnscheduleFromRunLoop (CFRunLoop runLoop, NSString mode)
		{
			ThrowIfDisposed ();
			if (runLoop == null)
				throw new ArgumentNullException ("runLoop");
			if (mode == null)
				throw new ArgumentNullException ("mode");
			IOHIDManagerUnscheduleFromRunLoop (Handle, runLoop.Handle, mode.Handle);
		}

		/*! @function   IOHIDManagerSetDeviceMatching
		    @abstract   Sets matching criteria for device enumeration.
		    @discussion Matching keys are prefixed by kIOHIDDevice and declared in 
		                <IOKit/hid/IOHIDKeys.h>.  Passing a NULL dictionary will result
		                in all devices being enumerated. Any subsequent calls will cause
		                the hid manager to release previously enumerated devices and 
		                restart the enuerate process using the revised criteria.  If 
		                interested in multiple, specific device classes, please defer to
		                using IOHIDManagerSetDeviceMatchingMultiple.
		    @param      manager Reference to an IOHIDManager.
		    @param      matching CFDictionaryRef containg device matching criteria.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerSetDeviceMatching (
		                                IOHIDManagerRef                 manager,
		                                CFDictionaryRef                 matching);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

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

		/*! @function   IOHIDManagerSetDeviceMatchingMultiple
		    @abstract   Sets multiple matching criteria for device enumeration.
		    @discussion Matching keys are prefixed by kIOHIDDevice and declared in 
		                <IOKit/hid/IOHIDKeys.h>.  This method is useful if interested 
		                in multiple, specific device classes.
		    @param      manager Reference to an IOHIDManager.
		    @param      multiple CFArrayRef containing multiple CFDictionaryRef objects
		                containg device matching criteria.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerSetDeviceMatchingMultiple (
		                                IOHIDManagerRef                 manager,
		                                CFArrayRef                      multiple);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

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

		/*! @function   IOHIDManagerCopyDevices
		    @abstract   Obtains currently enumerated devices.
		    @param      manager Reference to an IOHIDManager.
		    @result     CFSetRef containing IOHIDDeviceRefs.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFSetRef IOHIDManagerCopyDevices (IOHIDManagerRef manager);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

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

		/*! @function   IOHIDManagerRegisterDeviceMatchingCallback
		    @abstract   Registers a callback to be used when a device is enumerated.
		    @discussion Only a device matching the set criteria will be enumerated.
		    @param      manager Reference to an IOHIDManagerRef.
		    @param      callback Pointer to a callback method of type 
		                IOHIDDeviceCallback.
		    @param      context Pointer to data to be passed to the callback.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerRegisterDeviceMatchingCallback(
		                                IOHIDManagerRef                 manager,
		                                IOHIDDeviceCallback             callback,
		                                IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

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

		/*! @function   IOHIDManagerRegisterDeviceRemovalCallback
		    @abstract   Registers a callback to be used when any enumerated device is 
		                removed.
		    @discussion In most cases this occurs when a device is unplugged.
		    @param      manager Reference to an IOHIDManagerRef.
		    @param      callback Pointer to a callback method of type 
		                IOHIDDeviceCallback.
		    @param      context Pointer to data to be passed to the callback.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerRegisterDeviceRemovalCallback(
		                                IOHIDManagerRef                 manager,
		                                IOHIDDeviceCallback             callback,
		                                IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

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

		/*! @function   IOHIDManagerRegisterInputReportCallback
		    @abstract   Registers a callback to be used when an input report is issued by 
		                any enumerated device.
		    @discussion An input report is an interrupt driver report issued by a device.
		    @param      manager Reference to an IOHIDManagerRef.
		    @param      callback Pointer to a callback method of type IOHIDReportCallback.
		    @param      context Pointer to data to be passed to the callback.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerRegisterInputReportCallback( 
		                                    IOHIDManagerRef             manager,
		                                    IOHIDReportCallback         callback,
		                                    IntPtr                      context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		/*! @function   IOHIDManagerRegisterInputValueCallback
		    @abstract   Registers a callback to be used when an input value is issued by 
		                any enumerated device.
		    @discussion An input element refers to any element of type 
		                kIOHIDElementTypeInput and is usually issued by interrupt driven
		                reports.
		    @param      manager Reference to an IOHIDManagerRef.
		    @param      callback Pointer to a callback method of type IOHIDValueCallback.
		    @param      context Pointer to data to be passed to the callback.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerRegisterInputValueCallback (
			IOHIDManagerRef                 manager,
			IOHIDValueCallback              callback,
			IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

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

		/*! @function   IOHIDManagerSetInputValueMatching
		    @abstract   Sets matching criteria for input values received via 
		                IOHIDManagerRegisterInputValueCallback.
		    @discussion Matching keys are prefixed by kIOHIDElement and declared in 
		                <IOKit/hid/IOHIDKeys.h>.  Passing a NULL dictionary will result
		                in all devices being enumerated. Any subsequent calls will cause
		                the hid manager to release previously matched input elements and 
		                restart the matching process using the revised criteria.  If 
		                interested in multiple, specific device elements, please defer to
		                using IOHIDManagerSetInputValueMatchingMultiple.
		    @param      manager Reference to an IOHIDManager.
		    @param      matching CFDictionaryRef containg device matching criteria.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerSetInputValueMatching (
		                                IOHIDManagerRef                 manager,
		                                CFDictionaryRef                 matching);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		public void SetInputValueMatching (IDictionary<string, ValueType> matchingDictionary)
		{
			SetMatching (matchingDictionary, IOHIDManagerSetInputValueMatching);
		}

		/*! @function   IOHIDManagerSetInputValueMatchingMultiple
		    @abstract   Sets multiple matching criteria for input values received via 
		                IOHIDManagerRegisterInputValueCallback.
		    @discussion Matching keys are prefixed by kIOHIDElement and declared in 
		                <IOKit/hid/IOHIDKeys.h>.  This method is useful if interested 
		                in multiple, specific elements .
		    @param      manager Reference to an IOHIDManager.
		    @param      multiple CFArrayRef containing multiple CFDictionaryRef objects
		                containing input element matching criteria.
		*/

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDManagerSetInputValueMatchingMultiple (
		                                               IOHIDManagerRef                 manager,
		                                               CFArrayRef                      multiple);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER

		public void SetInputValueMatchingMultiple (ICollection<IDictionary<string, ValueType>> matchingDictionaries)
		{
			SetMatchingMultiple (matchingDictionaries, IOHIDManagerSetInputValueMatchingMultiple);
		}

		/*!
		 @abstract   Used to write out the current properties to a specific domain.
		 @discussion Using this function will cause the persistent properties to be saved out
		 replacing any properties that already existed in the specified domain. All arguments 
		 must be non-NULL except options.
		 @param     manager Reference to an IOHIDManager.
		 @param     applicationID Reference to a CFPreferences applicationID.
		 @param     userName Reference to a CFPreferences userName.
		 @param     hostName Reference to a CFPreferences hostName.
		 @param     options Reserved for future use.
		 */
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

