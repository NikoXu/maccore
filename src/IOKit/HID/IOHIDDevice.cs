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

namespace MonoMac.IOKit
{
	[Since (5,0)]
	public class IOHIDDevice : CFType
	{
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

		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeID IOHIDDeviceGetTypeID ();

		public static uint TypeID {
			get {
				return IOHIDDeviceGetTypeID ();
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDDeviceRef IOHIDDeviceCreate (
			CFAllocatorRef                  allocator, 
			io_service_t                    service);

		public IOHIDDevice (IOService service)
		{
			if (service == null)
				throw new ArgumentNullException ("service");
			Handle = IOHIDDeviceCreate (IntPtr.Zero, service.Handle);
			if (Handle == IntPtr.Zero)
				throw new Exception ("Could not create IOHIDDevice");
			Initalize ();
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static io_service_t IOHIDDeviceGetService(
			IOHIDDeviceRef                  device);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceOpen(
			IOHIDDeviceRef                  device, 
			IOOptionBits                    options);

		public void Open (IOHIDOptionsType options = IOHIDOptionsType.None) {
			ThrowIfDisposed ();
			var result = IOHIDDeviceOpen (Handle, (uint)options);
			IOObject.ThrowIfError (result);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceClose(
			IOHIDDeviceRef                  device, 
			IOOptionBits                    options);

		public void Close () {
			ThrowIfDisposed ();
			var result = IOHIDDeviceClose (Handle, 0);
			IOObject.ThrowIfError (result);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDDeviceConformsTo(
			IOHIDDeviceRef                  device, 
			uint32_t                        usagePage,
			uint32_t                        usage);

		public bool ConformsTo (uint usagePage, uint usage) {
			ThrowIfDisposed ();
			return IOHIDDeviceConformsTo (Handle, usagePage, usage);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeRef IOHIDDeviceGetProperty(
			IOHIDDeviceRef                  device, 
			CFStringRef                     key);

		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDDeviceSetProperty(
			IOHIDDeviceRef                  device,
			CFStringRef                     key,
			CFTypeRef                       property);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static CFArrayRef IOHIDDeviceCopyMatchingElements(
			IOHIDDeviceRef                  device, 
			CFDictionaryRef                 matching, 
			IOOptionBits                    options);

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
					elements [i] = new IOHIDElement(array.GetValue (i), true);
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
					elements [i] = new IOHIDElement (array.GetValue (i), true);
				return elements;
			}
		}
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceScheduleWithRunLoop(
			IOHIDDeviceRef                  device, 
			CFRunLoopRef                    runLoop, 
			CFStringRef                     runLoopMode);

		public void ScheduleWithRunLoop (CFRunLoop runLoop, NSString runLoopMode) {
			ThrowIfDisposed ();
			if (runLoop == null)
				throw new ArgumentNullException ("runLoop");
			if (runLoopMode == null)
				throw new ArgumentNullException ("runLoopMode");
			IOHIDDeviceScheduleWithRunLoop (Handle, runLoop.Handle, runLoopMode.Handle);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceUnscheduleFromRunLoop (
			IOHIDDeviceRef                  device, 
			CFRunLoopRef                    runLoop, 
			CFStringRef                     runLoopMode);

		public void UnscheduleFromRunLoop (CFRunLoop runLoop, NSString runLoopMode) {
			ThrowIfDisposed ();
			if (runLoop == null)
				throw new ArgumentNullException ("runLoop");
			if (runLoopMode == null)
				throw new ArgumentNullException ("runLoopMode");
			IOHIDDeviceUnscheduleFromRunLoop (Handle, runLoop.Handle, runLoopMode.Handle);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceRegisterRemovalCallback (
			IOHIDDeviceRef                  device, 
			IOHIDCallback                   callback, 
			IntPtr                          context);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceRegisterInputValueCallback (
			IOHIDDeviceRef                  device, 
			IOHIDValueCallback              callback, 
			IntPtr                          context);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceRegisterInputReportCallback ( 
			IOHIDDeviceRef                  device,
			[MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 2)]
			uint8_t[]                       report, 
			CFIndex                         reportLength,
			IOHIDReportCallback             callback, 
			IntPtr                          context);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceSetInputValueMatching(
			IOHIDDeviceRef                  device, 
			CFDictionaryRef                 matching);

		public void SetInputValueMatching (NSDictionary matchingDictionary) {
			ThrowIfDisposed ();
			var dictRef = matchingDictionary == null ? IntPtr.Zero : matchingDictionary.Handle;
			IOHIDDeviceSetInputValueMatching (Handle, dictRef);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDDeviceSetInputValueMatchingMultiple(
			IOHIDDeviceRef                  device, 
			CFArrayRef                      multiple);

		public void SetInputValueMatchingMultiple (NSDictionary[] matchingDictionaries) {
			ThrowIfDisposed ();
			var multRef = matchingDictionaries == null ? IntPtr.Zero : NSArray.FromNSObjects(matchingDictionaries).Handle;
			IOHIDDeviceSetInputValueMatchingMultiple (Handle, multRef);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceSetValue(
			IOHIDDeviceRef                  device, 
			IOHIDElementRef                 element, 
			IOHIDValueRef                   value);

		public void SetValue (IOHIDElement element, IOHIDValue value) {
			ThrowIfDisposed ();
			if (element == null)
				throw new ArgumentNullException ("element");
			if (value == null)
				throw new ArgumentNullException ("value");
			var result = IOHIDDeviceSetValue (Handle, element.Handle, value.Handle);
			IOObject.ThrowIfError (result);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceSetValueMultiple(
			IOHIDDeviceRef                  device, 
			CFDictionaryRef                 multiple);

		public void SetValueMultiple (IDictionary<IOHIDElement, IOHIDValue> dictonary) {
			ThrowIfDisposed ();
			if (dictonary == null)
				throw new ArgumentNullException ("multiple");
			var multiple = CFDictionary.FromObjectsAndKeys (dictonary.Values.ToArray (), dictonary.Keys.ToArray ());
			var result = IOHIDDeviceSetValueMultiple (Handle, multiple.Handle);
			CFType.Release (multiple.Handle);
			IOObject.ThrowIfError (result);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceSetValueWithCallback(
			IOHIDDeviceRef                  device, 
			IOHIDElementRef                 element, 
			IOHIDValueRef                   value, 
			CFTimeInterval                  timeout,
			IOHIDValueCallback              callback, 
			IntPtr                          context);

		// TODO: SetValueAsync


		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceSetValueMultipleWithCallback(
			IOHIDDeviceRef                  device, 
			CFDictionaryRef                 multiple,
			CFTimeInterval                  timeout,
			IOHIDValueMultipleCallback      callback, 
			IntPtr                          context);

		// TODO: SetValueMultipleAsync

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceGetValue(
			    IOHIDDeviceRef                  device, 
			    IOHIDElementRef                 element, 
			out IOHIDValueRef                   pValue);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceCopyValueMultiple(
			    IOHIDDeviceRef                  device, 
			    CFArrayRef                      elements, 
			ref CFDictionaryRef                 pMultiple);

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
				dict.Add (new IOHIDElement (keyRefs [i], true), new IOHIDValue (valueRefs [i], true));
			CFType.Release (multiple.Handle);
			return dict;
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceGetValueWithCallback (
			    IOHIDDeviceRef                  device, 
			    IOHIDElementRef                 element, 
			ref IOHIDValueRef                   pValue,
			    CFTimeInterval                  timeout,
			    IOHIDValueCallback              callback, 
			    IntPtr                          context);

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceCopyValueMultipleWithCallback (
			    IOHIDDeviceRef                  device, 
			    CFArrayRef                      elements, 
			ref CFDictionaryRef                 pMultiple,
			    CFTimeInterval                  timeout,
			    IOHIDValueMultipleCallback      callback, 
			    IntPtr                          context);

		// TODO: GetValueMultipleAsync

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceSetReport (
			IOHIDDeviceRef                  device,
			IOHIDReportType                 reportType,
			CFIndex                         reportID,
			[MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)]
			[In]uint8_t[]                   report,
			CFIndex                         reportLength);

		public void SendReport (IOHIDReportType type, int id, byte[] data)
		{
			ThrowIfDisposed ();
			var result = IOHIDDeviceSetReport (Handle, type, (CFIndex)id, data, (CFIndex)data.Length);
			IOObject.ThrowIfError (result);
		}

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

		// According to http://stackoverflow.com/questions/16116245
		// IOHIDDeviceSetReportWithCallback is not implemented. Verified on OS X 10.8.

		[DllImport (Constants.IOKitLibrary)]
		extern static IOReturn IOHIDDeviceGetReport (
			    IOHIDDeviceRef                  device,
			    IOHIDReportType                 reportType,
			    CFIndex                         reportID,
			    [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)]
			    uint8_t[]                       report,
			ref CFIndex                         pReportLength);

		// TODO: GetReport

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

		// TODO: GetReportAsync
	}
}

