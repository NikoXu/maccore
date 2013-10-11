//
// IOHIDDevice.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;

using MonoMac.ObjCRuntime;
using CFAllocatorRef = System.IntPtr;
using CFArrayRef = System.IntPtr;
using CFDictionaryRef = System.IntPtr;
using CFIndex = System.IntPtr;
using CFRunLoopRef = System.IntPtr;
using CFStringRef = System.IntPtr;
using CFTimeInterval = System.Double;
using CFTypeID = System.UInt32;
using CFTypeRef = System.IntPtr;
using IOHIDDeviceRef = System.IntPtr;
using IOHIDElementRef = System.IntPtr;
using IOHIDValueRef = System.IntPtr;
using IOOptionBits = System.UInt32;
using io_service_t = System.IntPtr;
using uint32_t = System.UInt32;
using uint8_t = System.Byte;

namespace MonoMac.IOKit.HID
{
	[Since (5,0)]
	public class IOHIDDevice : CFType
	{
		/*!
			@defined HID Device Property Keys
			@abstract Keys that represent properties of a paticular device.
			@discussion Keys that represent properties of a paticular device.  Can be added
			to your matching dictionary when refining searches for HID devices.
			<br><br>
			<b>Please note:</b><br>
			kIOHIDPrimaryUsageKey and kIOHIDPrimaryUsagePageKey are no longer 
			rich enough to describe a device's capabilities.  Take, for example, a
			device that describes both a keyboard and a mouse in the same descriptor.  
			The previous behavior was to only describe the keyboard behavior with the 
			primary usage and usage page.   Needless to say, this would sometimes cause 
			a program interested in mice to skip this device when matching.  
			<br>
			Thus we have added 3 
			additional keys:
			<ul>
			    <li>kIOHIDDeviceUsageKey</li>
			    <li>kIOHIDDeviceUsagePageKey</li>
			    <li>kIOHIDDeviceUsagePairsKey</li>
			</ul>
			kIOHIDDeviceUsagePairsKey is used to represent an array of dictionaries containing 
			key/value pairs referenced by kIOHIDDeviceUsageKey and kIOHIDDeviceUsagePageKey.  
			These usage pairs describe all application type collections (behaviors) defined 
			by the device.
			<br><br>
			An application intersted in only matching on one criteria would only add the 
			kIOHIDDeviceUsageKey and kIOHIDDeviceUsagePageKey keys to the matching dictionary.
			If it is interested in a device that has multiple behaviors, the application would
			instead add an array or dictionaries referenced by kIOHIDDeviceUsagePairsKey to his 
			matching dictionary.
		*/
		public const string TransportKey                  = "Transport";
		public const string VendorIDKey                   = "VendorID";
		public const string VendorIDSourceKey             = "VendorIDSource";
		public const string ProductIDKey                  = "ProductID";
		public const string VersionNumberKey              = "VersionNumber";
		public const string ManufacturerKey               = "Manufacturer";
		public const string ProductKey                    = "Product";
		public const string SerialNumberKey               = "SerialNumber";
		public const string CountryCodeKey                = "CountryCode";
		public const string StandardTypeKey               = "StandardType";
		public const string DeviceKeyboardStandardTypeKey = "DeviceKeyboardStandardType";
		public const string LocationIDKey                 = "LocationID";
		public const string DeviceUsageKey                = "DeviceUsage";
		public const string DeviceUsagePageKey            = "DeviceUsagePage";
		public const string DeviceUsagePairsKey           = "DeviceUsagePairs";
		public const string PrimaryUsageKey               = "PrimaryUsage";
		public const string PrimaryUsagePageKey           = "PrimaryUsagePage";
		public const string MaxInputReportSizeKey         = "MaxInputReportSize";
		public const string MaxOutputReportSizeKey        = "MaxOutputReportSize";
		public const string MaxFeatureReportSizeKey       = "MaxFeatureReportSize";
		public const string ReportIntervalKey             = "ReportInterval";
		public const string ReportDescriptorKey           = "ReportDescriptor";
		public const string ResetKey                      = "Reset";
		public const string DeviceKeyboardLanguageKey     = "DeviceKeyboardLanguage";

		internal IOHIDDevice (IntPtr handle, bool owns) : base (handle, owns)
		{
			Initalize ();
		}

		void Initalize ()
		{
			IOHIDDeviceRegisterRemovalCallback (Handle, RemovedCallback, IntPtr.Zero);
			// next line is crashing for unknown reason
			//IOHIDDeviceRegisterInputValueCallback (Handle, InputValueReceivedCallback, IntPtr.Zero);
			IOHIDDeviceRegisterInputReportCallback (Handle, new byte[MaxInputReportSize],
			                                        (CFIndex)MaxInputReportSize,
			                                        InputReportReceivedCallback, IntPtr.Zero);
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				// unregister callbacks by registering NULL pointer.
				IOHIDDeviceRegisterRemovalCallback (Handle, null, IntPtr.Zero);
				IOHIDDeviceRegisterInputValueCallback (Handle, null, IntPtr.Zero);
				IOHIDDeviceRegisterInputReportCallback (Handle, null, (CFIndex)0, null, IntPtr.Zero);
			}
			base.Dispose (disposing);
		}

		/*!
			@function   IOHIDDeviceGetTypeID
			@abstract   Returns the type identifier of all IOHIDDevice instances.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeID IOHIDDeviceGetTypeID ();
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public static uint TypeID {
			get {
				return IOHIDDeviceGetTypeID ();
			}
		}

		/*!
			@function   IOHIDDeviceCreate
			@abstract   Creates an element from an io_service_t.
		    @discussion The io_service_t passed in this method must reference an object 
		                in the kernel of type IOHIDDevice.
		    @param      allocator Allocator to be used during creation.
		    @param      io_service_t Reference to service object in the kernel.
		    @result     Returns a new IOHIDDeviceRef.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDDeviceRef IOHIDDeviceCreate (
			CFAllocatorRef                  allocator, 
			io_service_t                    service);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public IOHIDDevice (IOService service)
		{
			if (service == null)
				throw new ArgumentNullException ("service");
			Handle = IOHIDDeviceCreate (IntPtr.Zero, service.Handle);
			if (Handle == IntPtr.Zero)
				throw new Exception ("Could not create IOHIDDevice");
			Initalize ();
		}

		/*!
		    @function   IOHIDDeviceGetService
		    @abstract   Returns the io_service_t for an IOHIDDevice, if it has one.
		    @discussion If the IOHIDDevice references an object in the kernel, this is
		                used to get the io_service_t for that object.
		    @param      device Reference to an IOHIDDevice.
		    @result     Returns the io_service_t if the IOHIDDevice has one, or 
		                MACH_PORT_NULL if it does not.
		 */
		[DllImport (Constants.IOKitLibrary)]
		extern static io_service_t IOHIDDeviceGetService(
			IOHIDDeviceRef                  device);
		//AVAILABLE_MAC_OS_X_VERSION_10_6_AND_LATER;


		[Since(6,0)]
		public IOService Service {
			get {
				ThrowIfDisposed ();
				var serviceRef = IOHIDDeviceGetService (Handle);
				if (serviceRef == IntPtr.Zero)
					return null;
				return new IOService (serviceRef, true);
			}
		}

		/*!
			@function   IOHIDDeviceOpen
			@abstract   Opens a HID device for communication.
		    @discussion Before the client can issue commands that change the state of 
		                the device, it must have succeeded in opening the device. This 
		                establishes a link between the client's task and the actual 
		                device.  To establish an exclusive link use the 
		                kIOHIDOptionsTypeSeizeDevice option. 
		    @param      device Reference to an IOHIDDevice.
		    @param      options Option bits to be sent down to the device.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceOpen(
			IOHIDDeviceRef                  device, 
			IOOptionBits                    options);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public void Open (IOHIDOptionsType options = IOHIDOptionsType.None) {
			ThrowIfDisposed ();
			var result = IOHIDDeviceOpen (Handle, (uint)options);
			IOObject.ThrowIfError (result);
		}

		/*!
			@function   IOHIDDeviceClose
			@abstract   Closes communication with a HID device.
		    @discussion This closes a link between the client's task and the actual 
		                device.
		    @param      device Reference to an IOHIDDevice.
		    @param      options Option bits to be sent down to the device.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceClose(
			IOHIDDeviceRef                  device, 
			IOOptionBits                    options);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public void Close () {
			ThrowIfDisposed ();
			var result = IOHIDDeviceClose (Handle, 0);
			IOObject.ThrowIfError (result);
		}

		/*!
			@function   IOHIDDeviceConformsTo
			@abstract   Convenience function that scans the Application Collection 
		                elements to see if it conforms to the provided usagePage 
		                and usage.
		    @discussion Examples of Application Collection usages pairs are:
		                <br>
		                    usagePage = kHIDPage_GenericDesktop  <br>
		                    usage = kHIDUsage_GD_Mouse
		                <br>
		                <b>or</b>
		                <br>
		                    usagePage = kHIDPage_GenericDesktop  <br>
		                    usage = kHIDUsage_GD_Keyboard
		    @param      device Reference to an IOHIDDevice.
		    @param      usagePage Device usage page
		    @param      usage Device usage
		    @result     Returns TRUE if device conforms to provided usage.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDDeviceConformsTo(
			IOHIDDeviceRef                  device, 
			uint32_t                        usagePage,
			uint32_t                        usage);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public bool ConformsTo (uint usagePage, uint usage) {
			ThrowIfDisposed ();
			return IOHIDDeviceConformsTo (Handle, usagePage, usage);
		}

		/*!
			@function   IOHIDDeviceGetProperty
			@abstract   Obtains a property from an IOHIDDevice.
		    @discussion Property keys are prefixed by kIOHIDDevice and declared in 
		                <IOKit/hid/IOHIDKeys.h>.
		    @param      device Reference to an IOHIDDevice.
		    @param      key CFStringRef containing key to be used when querying the 
		                device.
		    @result     Returns CFTypeRef containing the property.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeRef IOHIDDeviceGetProperty(
			IOHIDDeviceRef                  device, 
			CFStringRef                     key);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		/*!
			@function   IOHIDDeviceSetProperty
			@abstract   Sets a property for an IOHIDDevice.
		    @discussion Property keys are prefixed by kIOHIDDevice and declared in 
		                <IOKit/hid/IOHIDKeys.h>.
		    @param      device Reference to an IOHIDDevice.
		    @param      key CFStringRef containing key to be used when modifiying the 
		                device property.
		    @param      property CFTypeRef containg the property to be set.
		    @result     Returns TRUE if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDDeviceSetProperty(
			IOHIDDeviceRef                  device,
			CFStringRef                     key,
			CFTypeRef                       property);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		[IndexerName ("Properties")]
		public NSObject this [string key] {
			get {
				ThrowIfDisposed ();
				if (key == null)
					throw new ArgumentNullException ("key");
				using (var keyString = new CFString (key)) {
					var result = IOHIDDeviceGetProperty (Handle, keyString.Handle);
					if (result == IntPtr.Zero)
						return null;
					return Runtime.GetNSObject (result);
				}
			}
			set {
				ThrowIfDisposed ();
				if (key == null)
					throw new ArgumentNullException ("key");
				using (var keyString = new CFString (key)) {
					if (!IOHIDDeviceSetProperty (Handle, keyString.Handle, value.Handle))
						throw new Exception ("Failed to set property");
				}
			}
		}

//		public const string TransportKey                  = "Transport";

		public uint VendorID {
			get {
				var value = this [VendorIDKey] as NSNumber;
				if (value == null)
					throw new NotSupportedException ();
				return value.UInt16Value;
			}
		}
		
//		public const string VendorIDSourceKey             = "VendorIDSource";

		public uint ProductID {
			get {
				var value = this [ProductIDKey] as NSNumber;
				if (value == null)
					throw new NotSupportedException ();
				return value.UInt16Value;
			}
		}

		public uint VersionNumber {
			get {
				var value = this [VersionNumberKey] as NSNumber;
				if (value == null)
					throw new NotSupportedException ();
				return value.UInt16Value;
			}
		}

		public string Manufacturer {
			get { return this[ManufacturerKey] as NSString; }
		}

		public string Product {
			get { return this[ProductKey] as NSString; }
		}

		public string SerialNumber {
			get { return this[SerialNumberKey] as NSString; }
		}

//		public const string CountryCodeKey                = "CountryCode";
//		public const string StandardTypeKey               = "StandardType";
//		public const string DeviceKeyboardStandardTypeKey = "DeviceKeyboardStandardType";
//		public const string LocationIDKey                 = "LocationID";

		public uint DeviceUsage {
			get {
				var value = this [DeviceUsageKey] as NSNumber;
				if (value == null)
					throw new NotSupportedException ();
				return value.UInt16Value;
			}
		}

		public UsagePage DeviceUsagePage {
			get {
				var value = this [DeviceUsagePageKey] as NSNumber;
				if (value == null)
					throw new NotSupportedException ();
				return (UsagePage)value.UInt16Value;
			}
		}

//		public const string DeviceUsagePairsKey           = "DeviceUsagePairs";

		public uint PrimaryUsage {
			get {
				var value = this [PrimaryUsageKey] as NSNumber;
				if (value == null)
					throw new NotSupportedException ();
				return value.UInt16Value;
			}
		}

		public UsagePage PrimaryUsagePage {
			get {
				var value = this [PrimaryUsagePageKey] as NSNumber;
				if (value == null)
					throw new NotSupportedException ();
				return (UsagePage)value.UInt16Value;
			}
		}

		public int MaxInputReportSize {
			get {
				var value = this [MaxInputReportSizeKey] as NSNumber;
				if (value == null)
					throw new NotSupportedException ();
				return value.IntValue;
			}
		}

		public int MaxOutputReportSize {
			get {
				var value = this [MaxOutputReportSizeKey] as NSNumber;
				if (value == null)
					throw new NotSupportedException ();
				return value.IntValue;
			}
		}

		public int MaxFeatureReportSize {
			get {
				var value = this [MaxFeatureReportSizeKey] as NSNumber;
				if (value == null)
					throw new NotSupportedException ();
				return value.IntValue;
			}
		}

//		public const string ReportIntervalKey             = "ReportInterval";
//		public const string ReportDescriptorKey           = "ReportDescriptor";
//		public const string ResetKey                      = "Reset";
//		public const string DeviceKeyboardLanguageKey     = "DeviceKeyboardLanguage";

		/*!
			@function   IOHIDDeviceCopyMatchingElements
		    @abstract   Obtains HID elements that match the criteria contained in the 
		                matching dictionary.
		    @discussion Matching keys are prefixed by kIOHIDElement and declared in 
		                <IOKit/hid/IOHIDKeys.h>.  Passing a NULL dictionary will result
		                in all device elements being returned.
		    @param      device Reference to an IOHIDDevice.
		    @param      matching CFDictionaryRef containg element matching criteria.
		    @param      options Reserved for future use.
		    @result     Returns CFArrayRef containing multiple IOHIDElement object.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFArrayRef IOHIDDeviceCopyMatchingElements(
			IOHIDDeviceRef                  device, 
			CFDictionaryRef                 matching, 
			IOOptionBits                    options);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public IOHIDElement[] GetMatchingElements (IDictionary<string, ValueType> matchingDictionary) {
			ThrowIfDisposed ();
			if (matchingDictionary == null)
				throw new ArgumentNullException ("matchingDictionary");
			var matching = NSDictionary.FromObjectsAndKeys (matchingDictionary.Values.ToArray (),
			                                                matchingDictionary.Keys.ToArray ());
			var arrayRef = IOHIDDeviceCopyMatchingElements (Handle, matching.Handle, 0);
			if (arrayRef == IntPtr.Zero)
				return null;
			using (var array = new CFArray (arrayRef, true)) {
				var elements = new IOHIDElement[array.Count];
				for (int i = 0; i < elements.Length; i++)
					elements [i] = GetCFObject<IOHIDElement> (array.GetValue (i));
				return elements;
			}
		}

		public IOHIDElement[] GetAllElements () {
			ThrowIfDisposed ();
			var arrayRef = IOHIDDeviceCopyMatchingElements (Handle, IntPtr.Zero, 0);
			if (arrayRef == IntPtr.Zero)
				return null;
			using (var array = new CFArray (arrayRef, true)) {
				var elements = new IOHIDElement[array.Count];
				for (int i = 0; i < elements.Length; i++)
					elements [i] = GetCFObject<IOHIDElement> (array.GetValue (i));
				return elements;
			}
		}

		/*! @function   IOHIDDeviceScheduleWithRunLoop
		    @abstract   Schedules HID device with run loop.
		    @discussion Formally associates device with client's run loop. Scheduling
		                this device with the run loop is necessary before making use of
		                any asynchronous APIs.
		    @param      device Reference to an IOHIDDevice.
		    @param      runLoop RunLoop to be used when scheduling any asynchronous 
		                activity.
		    @param      runLoopMode Run loop mode to be used when scheduling any 
		                asynchronous activity.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceScheduleWithRunLoop(
			IOHIDDeviceRef                  device, 
			CFRunLoopRef                    runLoop, 
			CFStringRef                     runLoopMode);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public void ScheduleWithRunLoop (CFRunLoop runLoop, NSString runLoopMode) {
			ThrowIfDisposed ();
			if (runLoop == null)
				throw new ArgumentNullException ("runLoop");
			if (runLoopMode == null)
				throw new ArgumentNullException ("runLoopMode");
			IOHIDDeviceScheduleWithRunLoop (Handle, runLoop.Handle, runLoopMode.Handle);
		}

		/*! @function   IOHIDDeviceUnscheduleFromRunLoop
		    @abstract   Unschedules HID device with run loop.
		    @discussion Formally disassociates device with client's run loop.
		    @param      device Reference to an IOHIDDevice.
		    @param      runLoop RunLoop to be used when unscheduling any asynchronous 
		                activity.
		    @param      runLoopMode Run loop mode to be used when unscheduling any 
		                asynchronous activity.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceUnscheduleFromRunLoop (
			IOHIDDeviceRef                  device, 
			CFRunLoopRef                    runLoop, 
			CFStringRef                     runLoopMode);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public void UnscheduleFromRunLoop (CFRunLoop runLoop, NSString runLoopMode) {
			ThrowIfDisposed ();
			if (runLoop == null)
				throw new ArgumentNullException ("runLoop");
			if (runLoopMode == null)
				throw new ArgumentNullException ("runLoopMode");
			IOHIDDeviceUnscheduleFromRunLoop (Handle, runLoop.Handle, runLoopMode.Handle);
		}

		/*! @function   IOHIDDeviceRegisterRemovalCallback
		    @abstract   Registers a callback to be used when a IOHIDDevice is removed.
		    @discussion In most cases this occurs when a device is unplugged.
		    @param      device Reference to an IOHIDDevice.
		    @param      callback Pointer to a callback method of type IOHIDCallback.
		    @param      context Pointer to data to be passed to the callback.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceRegisterRemovalCallback (
			IOHIDDeviceRef                  device, 
			IOHIDCallback                   callback, 
			IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public event EventHandler<IOReturnEventArgs> Removed;

		static void RemovedCallback (IntPtr context, IOReturn result, IntPtr senderRef)
		{
			var device = GetCFObject<IOHIDDevice> (senderRef);
			device.OnRemoved (result);
		}

		void OnRemoved (IOReturn result)
		{
			if (Removed != null)
				Removed (this, new IOReturnEventArgs (result));
		}

		/*! @function   IOHIDDeviceRegisterInputValueCallback
		    @abstract   Registers a callback to be used when an input value is issued by 
		                the device.
		    @discussion An input element refers to any element of type 
		                kIOHIDElementTypeInput and is usually issued by interrupt driven
		                reports.  If more specific element values are desired, you can 
		                specify matching criteria via IOHIDDeviceSetInputValueMatching
		                and IOHIDDeviceSetInputValueMatchingMultiple.
		    @param      device Reference to an IOHIDDevice.
		    @param      callback Pointer to a callback method of type IOHIDValueCallback.
		    @param      context Pointer to data to be passed to the callback.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceRegisterInputValueCallback (
			IOHIDDeviceRef                  device, 
			IOHIDValueCallback              callback, 
			IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public event EventHandler<IOHIDValueEventArgs> InputValueReceived;

		static void InputValueReceivedCallback (IntPtr context, IOReturn result, IntPtr senderRef, IntPtr valueRef)
		{
			var device = GetCFObject<IOHIDDevice> (senderRef);
			var value = GetCFObject<IOHIDValue> (valueRef);
			device.OnValueReceived (result, value);
		}

		void OnValueReceived (IOReturn result, IOHIDValue value)
		{
			if (InputValueReceived != null)
				InputValueReceived (this, new IOHIDValueEventArgs (result, value));
		}

		/*! @function   IOHIDDeviceRegisterInputReportCallback
		    @abstract   Registers a callback to be used when an input report is issued 
		                by the device.
		    @discussion An input report is an interrupt driver report issued by the 
		                device.
		    @param      device Reference to an IOHIDDevice.
		    @param      report Pointer to preallocated buffer in which to copy inbound
		                report data.
		    @param      reportLength Length of preallocated buffer.
		    @param      callback Pointer to a callback method of type 
		                IOHIDReportCallback.
		    @param      context Pointer to data to be passed to the callback.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceRegisterInputReportCallback ( 
			IOHIDDeviceRef                  device,
			[MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 2)]
			uint8_t[]                       report, 
			CFIndex                         reportLength,
			IOHIDReportCallback             callback, 
			IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public event EventHandler<IOHIDReportEventArgs> InputReportReceived;

		void InputReportReceivedCallback (IntPtr context, IOReturn result, IntPtr senderRef,
		                            IOHIDReportType type, uint id, byte[] data, CFIndex length)
		{
			var device = GetCFObject<IOHIDDevice> (senderRef);
			device.OnInputReportReceived (result, type, (int)id, data);
		}

		void OnInputReportReceived (IOReturn result, IOHIDReportType type, int id, byte[] data)
		{
			if (InputReportReceived != null)
				InputReportReceived (this, new IOHIDReportEventArgs (result, type, id, data));
		}

		/*! @function   IOHIDDeviceSetInputValueMatching
		    @abstract   Sets matching criteria for input values received via 
		                IOHIDDeviceRegisterInputValueCallback.
		    @discussion Matching keys are prefixed by kIOHIDElement and declared in 
		                <IOKit/hid/IOHIDKeys.h>.  Passing a NULL dictionary will result
		                in all devices being enumerated. Any subsequent calls will cause
		                the hid manager to release previously matched input elements and 
		                restart the matching process using the revised criteria.  If 
		                interested in multiple, specific device elements, please defer to
		                using IOHIDDeviceSetInputValueMatchingMultiple.
		    @param      manager Reference to an IOHIDDevice.
		    @param      matching CFDictionaryRef containg device matching criteria.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceSetInputValueMatching(
			IOHIDDeviceRef                  device, 
			CFDictionaryRef                 matching);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public void SetInputValueMatching (NSDictionary matchingDictionary) {
			ThrowIfDisposed ();
			var dictRef = matchingDictionary == null ? IntPtr.Zero : matchingDictionary.Handle;
			IOHIDDeviceSetInputValueMatching (Handle, dictRef);
		}

		/*! @function   IOHIDDeviceSetInputValueMatchingMultiple
		    @abstract   Sets multiple matching criteria for input values received via 
		                IOHIDDeviceRegisterInputValueCallback.
		    @discussion Matching keys are prefixed by kIOHIDElement and declared in 
		                <IOKit/hid/IOHIDKeys.h>.  This method is useful if interested 
		                in multiple, specific elements .
		    @param      manager Reference to an IOHIDDevice.
		    @param      multiple CFArrayRef containing multiple CFDictionaryRef objects
		                containg input element matching criteria.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceSetInputValueMatchingMultiple(
			IOHIDDeviceRef                  device, 
			CFArrayRef                      multiple);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public void SetInputValueMatchingMultiple (NSDictionary[] matchingDictionaries) {
			ThrowIfDisposed ();
			var multRef = matchingDictionaries == null ? IntPtr.Zero : NSArray.FromNSObjects(matchingDictionaries).Handle;
			IOHIDDeviceSetInputValueMatchingMultiple (Handle, multRef);
		}

		/*! @function   IOHIDDeviceSetValue
		    @abstract   Sets a value for an element.
		    @discussion This method behaves synchronously and will block until the
		                report has been issued to the device.  It is only relevent for 
		                either output or feature type elements.  If setting values for 
		                multiple elements you may want to consider using 
		                IOHIDDeviceSetValueMultiple or IOHIDTransaction.
		    @param      device Reference to an IOHIDDevice.
		    @param      element IOHIDElementRef whose value is to be modified.
		    @param      value IOHIDValueRef containing value to be set.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceSetValue(
			IOHIDDeviceRef                  device, 
			IOHIDElementRef                 element, 
			IOHIDValueRef                   value);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;


		public void SetValue (IOHIDElement element, IOHIDValue value) {
			ThrowIfDisposed ();
			if (element == null)
				throw new ArgumentNullException ("element");
			if (value == null)
				throw new ArgumentNullException ("value");
			var result = IOHIDDeviceSetValue (Handle, element.Handle, value.Handle);
			IOObject.ThrowIfError (result);
		}

		/*! @function   IOHIDDeviceSetValueMultiple
		    @abstract   Sets multiple values for multiple elements.
		    @discussion This method behaves synchronously and will block until the
		                report has been issued to the device.  It is only relevent for 
		                either output or feature type elements.
		    @param      device Reference to an IOHIDDevice.
		    @param      muliple CFDictionaryRef where key is IOHIDElementRef and
		                value is IOHIDValueRef.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceSetValueMultiple(
			IOHIDDeviceRef                  device, 
			CFDictionaryRef                 multiple);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public void SetValueMultiple (IDictionary<IOHIDElement, IOHIDValue> dictonary) {
			ThrowIfDisposed ();
			if (dictonary == null)
				throw new ArgumentNullException ("multiple");
			var multiple = CFDictionary.FromObjectsAndKeys (dictonary.Values.ToArray (), dictonary.Keys.ToArray ());
			var result = IOHIDDeviceSetValueMultiple (Handle, multiple.Handle);
			CFType.Release (multiple.Handle);
			IOObject.ThrowIfError (result);
		}

		/*! @function   IOHIDDeviceSetValueWithCallback
		    @abstract   Sets a value for an element and returns status via a completion
		                callback.
		    @discussion This method behaves asynchronously and will invoke the callback
		                once the report has been issued to the device.  It is only 
		                relevent for either output or feature type elements.  
		                If setting values for multiple elements you may want to 
		                consider using IOHIDDeviceSetValueWithCallback or 
		                IOHIDTransaction.
		    @param      device Reference to an IOHIDDevice.
		    @param      element IOHIDElementRef whose value is to be modified.
		    @param      value IOHIDValueRef containing value to be set.
		    @param      timeout CFTimeInterval containing the timeout.
		    @param      callback Pointer to a callback method of type 
		                IOHIDValueCallback.
		    @param      context Pointer to data to be passed to the callback.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceSetValueWithCallback(
			IOHIDDeviceRef                  device, 
			IOHIDElementRef                 element, 
			IOHIDValueRef                   value, 
			CFTimeInterval                  timeout,
			IOHIDValueCallback              callback, 
			IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		// TODO: SetValueAsync

		/*! @function   IOHIDDeviceSetValueMultipleWithCallback
		    @abstract   Sets multiple values for multiple elements and returns status 
		                via a completion callback.
		    @discussion This method behaves asynchronously and will invoke the callback
		                once the report has been issued to the device.  It is only 
		                relevent for either output or feature type elements.  
		    @param      device Reference to an IOHIDDevice.
		    @param      muliple CFDictionaryRef where key is IOHIDElementRef and
		                value is IOHIDValueRef.
		    @param      timeout CFTimeInterval containing the timeout.
		    @param      callback Pointer to a callback method of type 
		                IOHIDValueMultipleCallback.
		    @param      context Pointer to data to be passed to the callback.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceSetValueMultipleWithCallback(
			IOHIDDeviceRef                  device, 
			CFDictionaryRef                 multiple,
			CFTimeInterval                  timeout,
			IOHIDValueMultipleCallback      callback, 
			IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		// TODO: SetValueMultipleAsync

		/*! @function   IOHIDDeviceGetValue
		    @abstract   Gets a value for an element.
		    @discussion This method behaves synchronously and return back immediately
		                for input type element.  If requesting a value for a feature
		                element, this will block until the report has been issued to the
		                device.  If obtaining values for multiple elements you may want 
		                to consider using IOHIDDeviceCopyValueMultiple or IOHIDTransaction.
		    @param      device Reference to an IOHIDDevice.
		    @param      element IOHIDElementRef whose value is to be obtained.
		    @param      pValue Pointer to IOHIDValueRef to be obtained.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceGetValue(
			    IOHIDDeviceRef                  device, 
			    IOHIDElementRef                 element, 
			out IOHIDValueRef                   pValue);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public IOHIDValue GetValue (IOHIDElement element) {
			ThrowIfDisposed ();
			if (element == null)
				throw new ArgumentNullException ("element");
			IntPtr valueRef;
			var result = IOHIDDeviceGetValue (Handle, element.Handle, out valueRef);
			IOObject.ThrowIfError (result);
			if (valueRef == IntPtr.Zero)
				return null;
			return new IOHIDValue (valueRef, true);
		}

		/*! @function   IOHIDDeviceCopyValueMultiple
		    @abstract   Copies a values for multiple elements.
		    @discussion This method behaves synchronously and return back immediately
		                for input type element.  If requesting a value for a feature
		                element, this will block until the report has been issued to the
		                device.
		    @param      device Reference to an IOHIDDevice.
		    @param      elements CFArrayRef containing multiple IOHIDElementRefs whose 
		                values are to be obtained.
		    @param      pMultiple Pointer to CFDictionaryRef where the keys are the 
		                provided elements and the values are the requested values.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceCopyValueMultiple(
			    IOHIDDeviceRef                  device, 
			    CFArrayRef                      elements, 
			ref CFDictionaryRef                 pMultiple);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public IDictionary<IOHIDElement, IOHIDValue> GetValues (IOHIDElement[] elements) {
			ThrowIfDisposed ();
			if (elements == null)
				throw new ArgumentNullException ("elements");
			var elementArray = CFArray.FromNativeObjects (elements);
			IOHIDElement[] keys = new IOHIDElement[elements.Length];
			IOHIDValue[] values = new IOHIDValue[elements.Length];
			CFDictionary multiple = CFDictionary.FromObjectsAndKeys (values, keys);
			var multipleRef = multiple.Handle;
			var result = IOHIDDeviceCopyValueMultiple (Handle, elementArray.Handle, ref multipleRef);
			IOObject.ThrowIfError (result);
			var dict = new Dictionary<IOHIDElement, IOHIDValue> (multiple.Count);
			IntPtr[] keyRefs, valueRefs;
			multiple.GetKeysAndValues (out keyRefs, out valueRefs);
			for (int i = 0; i < multiple.Count; i++)
				dict.Add (GetCFObject<IOHIDElement> (keyRefs [i]), GetCFObject<IOHIDValue> (valueRefs [i]));
			CFType.Release (multiple.Handle);
			return dict;
		}

		/*! @function   IOHIDDeviceGetValueWithCallback
		    @abstract   Gets a value for an element and returns status via a completion
		                callback.
		    @discussion This method behaves asynchronusly and is only relevent for 
		                either output or feature type elements. If obtaining values for 
		                multiple elements you may want to consider using 
		                IOHIDDeviceCopyValueMultipleWithCallback or IOHIDTransaction.
		    @param      device Reference to an IOHIDDevice.
		    @param      element IOHIDElementRef whose value is to be obtained.
		    @param      pValue Pointer to IOHIDValueRef to be passedback.
		    @param      timeout CFTimeInterval containing the timeout.
		    @param      callback Pointer to a callback method of type 
		                IOHIDValueCallback.
		    @param      context Pointer to data to be passed to the callback.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceGetValueWithCallback (
			    IOHIDDeviceRef                  device, 
			    IOHIDElementRef                 element, 
			ref IOHIDValueRef                   pValue,
			    CFTimeInterval                  timeout,
			    IOHIDValueCallback              callback, 
			    IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		// TODO: GetValueAsync

		/*! @function   IOHIDDeviceCopyValueMultipleWithCallback
		    @abstract   Copies a values for multiple elements and returns status via a 
		                completion callback.
		    @discussion This method behaves asynchronusly and is only relevent for 
		                either output or feature type elements.
		    @param      device Reference to an IOHIDDevice.
		    @param      elements CFArrayRef containing multiple IOHIDElementRefs whose 
		                values are to be obtained.
		    @param      pMultiple Pointer to CFDictionaryRef where the keys are the 
		                provided elements and the values are the requested values.
		    @param      timeout CFTimeInterval containing the timeout.
		    @param      callback Pointer to a callback method of type 
		                IOHIDValueMultipleCallback.
		    @param      context Pointer to data to be passed to the callback.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceCopyValueMultipleWithCallback (
			    IOHIDDeviceRef                  device, 
			    CFArrayRef                      elements, 
			ref CFDictionaryRef                 pMultiple,
			    CFTimeInterval                  timeout,
			    IOHIDValueMultipleCallback      callback, 
			    IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		// TODO: GetValueMultipleAsync

		/*! @function   IOHIDDeviceSetReport
		    @abstract   Sends a report to the device.
		    @discussion This method behaves synchronously and will block until the
		                report has been issued to the device.  It is only relevent for 
		                either output or feature type reports.
		    @param      device Reference to an IOHIDDevice.
		    @param      reportType Type of report being sent.
		    @param      reportID ID of the report being sent.  If the device supports
		                multiple reports, this should also be set in the first byte of
		                the report.
		    @param      report The report bytes to be sent to the device.
		    @param      reportLength The length of the report to be sent to the device.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceSetReport (
			IOHIDDeviceRef                  device,
			IOHIDReportType                 reportType,
			CFIndex                         reportID,
			[MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)]
			[In]uint8_t[]                   report,
			CFIndex                         reportLength);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public void SendReport (IOHIDReportType type, int id, byte[] data)
		{
			ThrowIfDisposed ();
			var result = IOHIDDeviceSetReport (Handle, type, (CFIndex)id, data, (CFIndex)data.Length);
			IOObject.ThrowIfError (result);
		}

		/*! @function   IOHIDDeviceSetReportWithCallback
		    @abstract   Sends a report to the device.
		    @discussion This method behaves asynchronously and will block until the
		                report has been issued to the device.  It is only relevent for 
		                either output or feature type reports.
		    @param      device Reference to an IOHIDDevice.
		    @param      reportType Type of report being sent.
		    @param      reportID ID of the report being sent.  If the device supports
		                multiple reports, this should also be set in the first byte of
		                the report.
		    @param      report The report bytes to be sent to the device.
		    @param      reportLength The length of the report to be sent to the device.
		    @param      timeout CFTimeInterval containing the timeout.
		    @param      callback Pointer to a callback method of type 
		                IOHIDReportCallback.
		    @param      context Pointer to data to be passed to the callback.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceSetReportWithCallback (
			IOHIDDeviceRef                  device,
			IOHIDReportType                 reportType,
			CFIndex                         reportID,
			[MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)]
			[In]uint8_t[]                   report,
			CFIndex                         reportLength,
			CFTimeInterval                  timeout,
			IOHIDReportCallback             callback,
			IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		// According to http://stackoverflow.com/questions/16116245
		// IOHIDDeviceSetReportWithCallback is not implemented. Verified on OS X 10.8.

		/*! @function   IOHIDDeviceGetReport
		    @abstract   Obtains a report from the device.
		    @discussion This method behaves synchronously and will block until the
		                report has been received from the device.  This is only intended
		                for feature reports because of sporadic device support for
		                polling input reports.  Please defer to using 
		                IOHIDDeviceRegisterInputReportCallback for obtaining input 
		                reports.
		    @param      device Reference to an IOHIDDevice.
		    @param      reportType Type of report being requested.
		    @param      reportID ID of the report being requested.
		    @param      report Pointer to preallocated buffer in which to copy inbound
		                report data.
		    @param      pReportLength Pointer to length of preallocated buffer.  This
		                value will be modified to refect the length of the returned 
		                report.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceGetReport (
			    IOHIDDeviceRef                  device,
			    IOHIDReportType                 reportType,
			    CFIndex                         reportID,
			    [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)]
			    uint8_t[]                       report,
			ref CFIndex                         pReportLength);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		// TODO: GetReport

		/*! @function   IOHIDDeviceGetReportWithCallback
		    @abstract   Obtains a report from the device.
		    @discussion This method behaves asynchronously and will block until the
		                report has been received from the device.  This is only intended 
		                for feature reports because of sporadic devicesupport for 
		                polling input reports.  Please defer to using 
		                IOHIDDeviceRegisterInputReportCallback for obtaining input 
		                reports.
		    @param      device Reference to an IOHIDDevice.
		    @param      reportType Type of report being requested.
		    @param      reportID ID of the report being requested.
		    @param      report Pointer to preallocated buffer in which to copy inbound
		                report data.
		    @param      pReportLength Pointer to length of preallocated buffer.
		    @param      pReportLength Pointer to length of preallocated buffer.  This
		                value will be modified to refect the length of the returned 
		                report.
		    @param      callback Pointer to a callback method of type 
		                IOHIDReportCallback.
		    @param      context Pointer to data to be passed to the callback.
		    @result     Returns kIOReturnSuccess if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceGetReportWithCallback (
			    IOHIDDeviceRef                  device,
			    IOHIDReportType                 reportType,
			    CFIndex                         reportID,
			    [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)]
			    uint8_t[]                       report,
			ref CFIndex                         pReportLength,
			    CFTimeInterval                  timeout,
			    IOHIDReportCallback             callback,
			    IntPtr                          context);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		// TODO: GetReportAsync
	}
}

