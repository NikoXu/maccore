//
// IOHIDElement.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

using CFAllocatorRef = System.IntPtr;
using CFArrayRef = System.IntPtr;
using CFDictionaryRef = System.IntPtr;
using CFStringRef = System.IntPtr;
using CFTypeID = System.UInt32;
using CFTypeRef = System.IntPtr;
using IOHIDDeviceRef = System.IntPtr;
using IOHIDElementCookie = System.UInt32;
using IOHIDElementRef = System.IntPtr;
using uint32_t = System.UInt32;

namespace MonoMac.IOKit.HID
{
	[Since (5,0)]
	public class IOHIDElement : CFType
	{

		/*!
		    @defined HID Element Dictionary Keys
		    @abstract Keys that represent properties of a particular elements.
		    @discussion These keys can also be added to a matching dictionary 
		        when searching for elements via copyMatchingElements.  
		*/
		public const string CookieKey                    = "ElementCookie";
		public const string TypeKey                      = "Type";
		public const string CollectionTypeKey            = "CollectionType";
		public const string UsageKey                     = "Usage";
		public const string UsagePageKey                 = "UsagePage";
		public const string MinKey                       = "Min";
		public const string MaxKey                       = "Max";
		public const string ScaledMinKey                 = "ScaledMin";
		public const string ScaledMaxKey                 = "ScaledMax";
		public const string SizeKey                      = "Size";
		public const string ReportSizeKey                = "ReportSize";
		public const string ReportCountKey               = "ReportCount";
		public const string ReportIDKey                  = "ReportID";
		public const string IsArrayKey                   = "IsArray";
		public const string IsRelativeKey                = "IsRelative";
		public const string IsWrappingKey                = "IsWrapping";
		public const string IsNonLinearKey               = "IsNonLinear";
		public const string HasPreferredStateKey         = "HasPreferredState";
		public const string HasNullStateKey              = "HasNullState";
		public const string FlagsKey                     = "Flags";
		public const string UnitKey                      = "Unit";
		public const string UnitExponentKey              = "UnitExponent";
		public const string NameKey                      = "Name";
		public const string ValueLocationKey             = "ValueLocation";
		public const string DuplicateIndexKey            = "DuplicateIndex";
		public const string ParentCollectionKey          = "ParentCollection";
		public const string VendorSpecificKey            = "VendorSpecific";

		/*!
		    @defined HID Element Match Keys
		    @abstract Keys used for matching particular elements.
		    @discussion These keys should only be used with a matching  
		        dictionary when searching for elements via copyMatchingElements.  
		*/
		public const string CookieMinKey         = "ElementCookieMin";
		public const string CookieMaxKey         = "ElementCookieMax";
		public const string UsageMinKey          = "UsageMin";
		public const string UsageMaxKey          = "UsageMax";

		/*!
		    @defined kIOHIDElementCalibrationMinKey
		    @abstract The minimum bounds for a calibrated value.  
		*/
		public const string CalibrationMinKey            = "CalibrationMin";

		/*!
		    @defined kIOHIDElementCalibrationMaxKey
		    @abstract The maximum bounds for a calibrated value.  
		*/
		public const string CalibrationMaxKey            = "CalibrationMax";

		/*!
		    @defined kIOHIDElementCalibrationSaturationMinKey
		    @abstract The mininum tolerance to be used when calibrating a logical element value. 
		    @discussion The saturation property is used to allow for slight differences in the minimum and maximum value returned by an element. 
		*/
		public const string CalibrationSaturationMinKey  = "CalibrationSaturationMin";

		/*!
		    @defined kIOHIDElementCalibrationSaturationMaxKey
		    @abstract The maximum tolerance to be used when calibrating a logical element value.  
		    @discussion The saturation property is used to allow for slight differences in the minimum and maximum value returned by an element. 
		*/
		public const string CalibrationSaturationMaxKey  = "CalibrationSaturationMax";

		/*!
		    @defined kIOHIDElementCalibrationDeadZoneMinKey
		    @abstract The minimum bounds near the midpoint of a logical value in which the value is ignored.  
		    @discussion The dead zone property is used to allow for slight differences in the idle value returned by an element. 
		*/
		public const string CalibrationDeadZoneMinKey    = "CalibrationDeadZoneMin";

		/*!
		    @defined kIOHIDElementCalibrationDeadZoneMinKey
		    @abstract The maximum bounds near the midpoint of a logical value in which the value is ignored.  
		    @discussion The dead zone property is used to allow for slight differences in the idle value returned by an element. 
		*/
		public const string CalibrationDeadZoneMaxKey    = "CalibrationDeadZoneMax";

		/*!
		    @defined kIOHIDElementCalibrationGranularityKey
		    @abstract The scale or level of detail returned in a calibrated element value.  
		    @discussion Values are rounded off such that if granularity=0.1, values after calibration are 0, 0.1, 0.2, 0.3, etc.
		*/
		public const string CalibrationGranularityKey    = "CalibrationGranularity";

		internal IOHIDElement (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		/*!
			@function   IOHIDElementGetTypeID
			@abstract   Returns the type identifier of all IOHIDElement instances.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeID IOHIDElementGetTypeID ();
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public static uint TypeID {
			get {
				return IOHIDElementGetTypeID ();
			}
		}

		/*!
			@function   IOHIDElementCreateWithDictionary
			@abstract   Creates an element from a dictionary.
		    @discussion The dictionary should contain keys defined in IOHIDKeys.h and start with kIOHIDElement.
		                This call is meant be used by a IOHIDDeviceDeviceInterface object.
		    @param      allocator Allocator to be used during creation.
		    @param      dictionary dictionary containing values in which to create element.
		    @result     Returns a new IOHIDElementRef.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementRef IOHIDElementCreateWithDictionary (CFAllocatorRef allocator, CFDictionaryRef dictionary);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public static IOHIDElement FromDictionary (NSDictionary dictionary) {
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			var elementRef = IOHIDElementCreateWithDictionary (IntPtr.Zero, dictionary.Handle);
			if (elementRef == null)
				return null;
			return new IOHIDElement (elementRef, true);
		}

		/*!
			@function   IOHIDElementGetDevice
			@abstract   Obtain the device associated with the element.
		    @param      element IOHIDElement to be queried. 
		    @result     Returns the a reference to the device.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDDeviceRef IOHIDElementGetDevice (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public IOHIDDevice Device {
			get {
				ThrowIfDisposed ();
				var deviceRef = IOHIDElementGetDevice (Handle);
				if (deviceRef == IntPtr.Zero)
					return null;
				return GetCFObject<IOHIDDevice> (deviceRef);
			}
		}

		/*!
			@function   IOHIDElementGetParent
			@abstract   Returns the parent for the element.
		    @discussion The parent element can be an element of type kIOHIDElementTypeCollection.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns an IOHIDElementRef referencing the parent element.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementRef IOHIDElementGetParent (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public IOHIDElement Parent {
			get {
				ThrowIfDisposed ();
				var elementRef = IOHIDElementGetParent (Handle);
				if (elementRef == IntPtr.Zero)
					return null;
				return GetCFObject<IOHIDElement> (elementRef);
			}
		}

		/*!
			@function   IOHIDElementGetChildren
			@abstract   Returns the children for the element.
		    @discussion An element of type kIOHIDElementTypeCollection usually contains children.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns an CFArrayRef containing element objects of type IOHIDElementRef.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFArrayRef IOHIDElementGetChildren (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public IOHIDElement[] Children {
			get {
				ThrowIfDisposed ();
				var elementArrayRef = IOHIDElementGetChildren (Handle);
				if (elementArrayRef == IntPtr.Zero)
					return null;
				using (var array = new CFArray (elementArrayRef, false)) {
					var managedArray = new IOHIDElement [array.Count];
					for (int i = 0; i < array.Count; i++)
						managedArray [i] = GetCFObject<IOHIDElement> (array.GetValue (i));
					return managedArray;
				}
			}
		}

		/*!
			@function   IOHIDElementAttach
			@abstract   Establish a relationship between one or more elements.
		    @discussion This is useful for grouping HID elements with related functionality.
		    @param      element The element to be modified. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @param      toAttach The element to be attached. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDElementAttach (IOHIDElementRef element, IOHIDElementRef toAttach);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public void Attach (IOHIDElement element) {
			ThrowIfDisposed ();
			if (element == null)
				throw new ArgumentNullException ("element");
			IOHIDElementAttach (Handle, element.Handle);
		}

		/*!
			@function   IOHIDElementDetach
			@abstract   Remove a relationship between one or more elements.
		    @discussion This is useful for grouping HID elements with related functionality.
		    @param      element The element to be modified. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @param      toDetach The element to be detached. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDElementDetach (IOHIDElementRef element, IOHIDElementRef toDetach);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public void Detatch (IOHIDElement element) {
			ThrowIfDisposed ();
			if (element == null)
				throw new ArgumentNullException ("element");
			IOHIDElementDetach (Handle, element.Handle);
		}

		/*!
			@function   IOHIDElementCopyAttached
			@abstract   Obtain attached elements.
		    @discussion Attached elements are those that have been grouped via IOHIDElementAttach.
		    @param      element The element to be modified. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns a copy of the current attached elements.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFArrayRef IOHIDElementCopyAttached (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public IOHIDElement[] AttachedElements {
			get {
				ThrowIfDisposed ();
				var elementArrayRef = IOHIDElementCopyAttached (Handle);
				if (elementArrayRef == IntPtr.Zero)
					return null;
				using (var array = new CFArray (elementArrayRef, true)) {
					var managedArray = new IOHIDElement [array.Count];
					for (int i = 0; i < array.Count; i++)
						managedArray [i] = GetCFObject<IOHIDElement> (array.GetValue (i));
					return managedArray;
				}
			}
		}

		/*!
			@function   IOHIDElementGetCookie
			@abstract   Retrieves the cookie for the element.
		    @discussion The IOHIDElementCookie represent a unique identifier for an element within a device.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the IOHIDElementCookie for the element.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementCookie IOHIDElementGetCookie (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;


		public uint Cookie {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetCookie (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetType
			@abstract   Retrieves the type for the element.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the IOHIDElementType for the element.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementType IOHIDElementGetType (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public IOHIDElementType Type {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetType (Handle);
			}
		}

		/*!
			@function   IOHIDElementCollectionType
			@abstract   Retrieves the collection type for the element.
		    @discussion The value returned by this method only makes sense if the element type is kIOHIDElementTypeCollection.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the IOHIDElementCollectionType for the element.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementCollectionType IOHIDElementGetCollectionType (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public IOHIDElementCollectionType CollectionType {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetCollectionType (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetUsagePage
			@abstract   Retrieves the usage page for an element.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the usage page for the element.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetUsagePage (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public uint UsagePage {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetUsagePage (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetUsage
			@abstract   Retrieves the usage for an element.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the usage for the element.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetUsage (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public uint Usage {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetUsage (Handle);
			}
		}

		/*!
			@function   IOHIDElementIsVirtual
			@abstract   Returns the virtual property for the element.
		    @discussion Indicates whether the element is a virtual element.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the TRUE if virtual or FALSE if not.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementIsVirtual (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public bool IsVirtual {
			get {
				ThrowIfDisposed ();
				return IOHIDElementIsVirtual (Handle);
			}
		}

		/*!
			@function   IOHIDElementIsRelative
			@abstract   Returns the relative property for the element.
		    @discussion Indicates whether the data is relative (indicating the change in value from the last report) or absolute 
		                (based on a fixed origin).
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns TRUE if relative or FALSE if absolute.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementIsRelative (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public bool IsRelative {
			get {
				ThrowIfDisposed ();
				return IOHIDElementIsRelative (Handle);
			}
		}

		/*!
			@function   IOHIDElementIsWrapping
			@abstract   Returns the wrap property for the element.
		    @discussion Wrap indicates whether the data "rolls over" when reaching either the extreme high or low value.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns TRUE if wrapping or FALSE if non-wrapping.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementIsWrapping (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public bool IsWrapping {
			get {
				ThrowIfDisposed ();
				return IOHIDElementIsWrapping (Handle);
			}
		}

		/*!
			@function   IOHIDElementIsArray
			@abstract   Returns the array property for the element.
		    @discussion Indicates whether the element represents variable or array data values. Variable values represent data from a 
		                physical control.  An array returns an index in each field that corresponds to the pressed button 
		                (like keyboard scan codes).
		                <br>
		                <b>Note:</b> The HID Manager will represent most elements as "variable" including the possible usages of an array.  
		                Array indices will remain as "array" elements with a usage of 0xffffffff.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns TRUE if array or FALSE if variable.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementIsArray (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public bool IsArray {
			get {
				ThrowIfDisposed ();
				return IOHIDElementIsArray (Handle);
			}
		}

		/*!
			@function   IOHIDElementIsNonLinear
			@abstract   Returns the linear property for the element.
		    @discussion Indicates whether the value for the element has been processed in some way, and no longer represents a linear 
		                relationship between what is measured and the value that is reported.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns TRUE if non linear or FALSE if linear.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementIsNonLinear (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public bool IsNonLinear {
			get {
				ThrowIfDisposed ();
				return IOHIDElementIsNonLinear (Handle);
			}
		}

		/*!
			@function   IOHIDElementHasPreferredState
			@abstract   Returns the preferred state property for the element.
		    @discussion Indicates whether the element has a preferred state to which it will return when the user is not physically 
		                interacting with the control.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns TRUE if preferred state or FALSE if no preferred state.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementHasPreferredState (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public bool HasPreferredState {
			get {
				ThrowIfDisposed ();
				return IOHIDElementHasPreferredState (Handle);
			}
		}

		/*!
			@function   IOHIDElementHasNullState
			@abstract   Returns the null state property for the element.
		    @discussion Indicates whether the element has a state in which it is not sending meaningful data. 
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns TRUE if null state or FALSE if no null state.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementHasNullState (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public bool HasNullState {
			get {
				ThrowIfDisposed ();
				return IOHIDElementHasNullState (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetName
			@abstract   Returns the name for the element.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns CFStringRef containing the element name.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFStringRef IOHIDElementGetName (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public string Name {
			get {
				ThrowIfDisposed ();
				var nameRef = IOHIDElementGetName (Handle);
				if (nameRef == IntPtr.Zero)
					return null;
				using (var name = new CFString (nameRef, true)) {
					return name.ToString ();
				}
			}
		}

		/*!
			@function   IOHIDElementGetReportID
			@abstract   Returns the report ID for the element.
		    @discussion The report ID represents what report this particular element belongs to.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the report ID.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetReportID (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public uint ReportId {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetReportID (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetReportSize
			@abstract   Returns the report size in bits for the element.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the report size.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetReportSize (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public uint ReportSize {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetReportSize (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetReportCount
			@abstract   Returns the report count for the element.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the report count.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetReportCount (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;


		public uint ReportCount {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetReportCount (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetUnit
			@abstract   Returns the unit property for the element.
		    @discussion The unit property is described in more detail in Section 6.2.2.7 of the 
		              = "Device Class Definition for Human Interface Devices(HID)" Specification, Version 1.11.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the unit.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetUnit (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public uint Unit {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetUnit (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetUnitExponent
			@abstract   Returns the unit exponenet in base 10 for the element.
		    @discussion The unit exponent property is described in more detail in Section 6.2.2.7 of the 
		              = "Device Class Definition for Human Interface Devices(HID)" Specification, Version 1.11.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the unit exponent.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetUnitExponent (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public uint UnitExponent {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetUnitExponent (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetLogicalMin
			@abstract   Returns the minimum value possible for the element.
		    @discussion This corresponds to the logical minimun, which indicates the lower bounds of a variable element.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the logical minimum.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDElementGetLogicalMin (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public CFIndex LogicalMin {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetLogicalMin (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetMax
			@abstract   Returns the maximum value possible for the element.
		    @discussion This corresponds to the logical maximum, which indicates the upper bounds of a variable element.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the logical maximum.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDElementGetLogicalMax (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public CFIndex LogicalMax {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetLogicalMax (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetPhysicalMin
			@abstract   Returns the scaled minimum value possible for the element.
		    @discussion Minimum value for the physical extent of a variable element. This represents the value for the logical minimum with units applied to it.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the physical minimum.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDElementGetPhysicalMin (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public CFIndex PhysicallMin {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetPhysicalMin (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetPhysicalMax
			@abstract   Returns the scaled maximum value possible for the element.
		    @discussion Maximum value for the physical extent of a variable element.  This represents the value for the logical maximum with units applied to it.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @result     Returns the physical maximum.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDElementGetPhysicalMax (IOHIDElementRef element);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		public CFIndex PhysicalMax {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetPhysicalMax (Handle);
			}
		}

		/*!
			@function   IOHIDElementGetProperty
			@abstract   Returns the an element property.
		    @discussion Property keys are prefixed by kIOHIDElement and declared in IOHIDKeys.h.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @param      key The key to be used when querying the element.
		    @result     Returns the property.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeRef IOHIDElementGetProperty (IOHIDElementRef element, CFStringRef key);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		/*!
			@function   IOHIDElementSetProperty
			@abstract   Sets an element property.
		    @discussion This method can be used to set arbitrary element properties, such as application specific references.
		    @param      element The element to be queried. If this parameter is not a valid IOHIDElementRef, the behavior is undefined.
		    @param      key The key to be used when querying the element.
		    @result     Returns TRUE if successful.
		*/
		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementSetProperty (IOHIDElementRef element, CFStringRef key, CFTypeRef property);
		//AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

		[IndexerName ("Properties")]
		public NSObject this[string key] {
			get {
				ThrowIfDisposed ();
				using (var keyString = new CFString (key)) {
					var propertyRef = IOHIDElementGetProperty (Handle, keyString.Handle);
					return Runtime.GetNSObject (propertyRef);
				}
			}
			set {
				ThrowIfDisposed ();
				using (var keyString = new CFString (key)) {
					if (!IOHIDElementSetProperty (Handle, keyString.Handle, value.Handle))
					    throw new Exception ("Failed to set property");
				}
			}
		}
	}
}

