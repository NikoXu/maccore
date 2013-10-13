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
		/* Property keys */
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

		/* Element Match Keys */
		public const string CookieMinKey         = "ElementCookieMin";
		public const string CookieMaxKey         = "ElementCookieMax";
		public const string UsageMinKey          = "UsageMin";
		public const string UsageMaxKey          = "UsageMax";

		public const string CalibrationMinKey            = "CalibrationMin";
		public const string CalibrationMaxKey            = "CalibrationMax";
		public const string CalibrationSaturationMinKey  = "CalibrationSaturationMin";
		public const string CalibrationSaturationMaxKey  = "CalibrationSaturationMax";
		public const string CalibrationDeadZoneMinKey    = "CalibrationDeadZoneMin";
		public const string CalibrationDeadZoneMaxKey    = "CalibrationDeadZoneMax";
		public const string CalibrationGranularityKey    = "CalibrationGranularity";

		internal IOHIDElement (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeID IOHIDElementGetTypeID ();

		public static uint TypeID {
			get {
				return IOHIDElementGetTypeID ();
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementRef IOHIDElementCreateWithDictionary (CFAllocatorRef allocator, CFDictionaryRef dictionary);

		public static IOHIDElement FromDictionary (NSDictionary dictionary) {
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			var elementRef = IOHIDElementCreateWithDictionary (IntPtr.Zero, dictionary.Handle);
			if (elementRef == null)
				return null;
			return new IOHIDElement (elementRef, true);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDDeviceRef IOHIDElementGetDevice (IOHIDElementRef element);

		public IOHIDDevice Device {
			get {
				ThrowIfDisposed ();
				var deviceRef = IOHIDElementGetDevice (Handle);
				if (deviceRef == IntPtr.Zero)
					return null;
				return GetCFObject<IOHIDDevice> (deviceRef);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementRef IOHIDElementGetParent (IOHIDElementRef element);

		public IOHIDElement Parent {
			get {
				ThrowIfDisposed ();
				var elementRef = IOHIDElementGetParent (Handle);
				if (elementRef == IntPtr.Zero)
					return null;
				return GetCFObject<IOHIDElement> (elementRef);
			}
		}
		[DllImport (Constants.IOKitLibrary)]
		extern static CFArrayRef IOHIDElementGetChildren (IOHIDElementRef element);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDElementAttach (IOHIDElementRef element, IOHIDElementRef toAttach);

		public void Attach (IOHIDElement element) {
			ThrowIfDisposed ();
			if (element == null)
				throw new ArgumentNullException ("element");
			IOHIDElementAttach (Handle, element.Handle);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IOHIDElementDetach (IOHIDElementRef element, IOHIDElementRef toDetach);

		public void Detatch (IOHIDElement element) {
			ThrowIfDisposed ();
			if (element == null)
				throw new ArgumentNullException ("element");
			IOHIDElementDetach (Handle, element.Handle);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFArrayRef IOHIDElementCopyAttached (IOHIDElementRef element);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementCookie IOHIDElementGetCookie (IOHIDElementRef element);

		public uint Cookie {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetCookie (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementType IOHIDElementGetType (IOHIDElementRef element);

		public IOHIDElementType Type {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetType (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static IOHIDElementCollectionType IOHIDElementGetCollectionType (IOHIDElementRef element);

		public IOHIDElementCollectionType CollectionType {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetCollectionType (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetUsagePage (IOHIDElementRef element);

		public uint UsagePage {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetUsagePage (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetUsage (IOHIDElementRef element);

		public uint Usage {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetUsage (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementIsVirtual (IOHIDElementRef element);

		public bool IsVirtual {
			get {
				ThrowIfDisposed ();
				return IOHIDElementIsVirtual (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementIsRelative (IOHIDElementRef element);

		public bool IsRelative {
			get {
				ThrowIfDisposed ();
				return IOHIDElementIsRelative (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementIsWrapping (IOHIDElementRef element);

		public bool IsWrapping {
			get {
				ThrowIfDisposed ();
				return IOHIDElementIsWrapping (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementIsArray (IOHIDElementRef element);

		public bool IsArray {
			get {
				ThrowIfDisposed ();
				return IOHIDElementIsArray (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementIsNonLinear (IOHIDElementRef element);

		public bool IsNonLinear {
			get {
				ThrowIfDisposed ();
				return IOHIDElementIsNonLinear (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementHasPreferredState (IOHIDElementRef element);

		public bool HasPreferredState {
			get {
				ThrowIfDisposed ();
				return IOHIDElementHasPreferredState (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementHasNullState (IOHIDElementRef element);

		public bool HasNullState {
			get {
				ThrowIfDisposed ();
				return IOHIDElementHasNullState (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFStringRef IOHIDElementGetName (IOHIDElementRef element);

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

		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetReportID (IOHIDElementRef element);

		public uint ReportId {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetReportID (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetReportSize (IOHIDElementRef element);

		public uint ReportSize {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetReportSize (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetReportCount (IOHIDElementRef element);

		public uint ReportCount {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetReportCount (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetUnit (IOHIDElementRef element);

		public uint Unit {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetUnit (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOHIDElementGetUnitExponent (IOHIDElementRef element);

		public uint UnitExponent {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetUnitExponent (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDElementGetLogicalMin (IOHIDElementRef element);

		public CFIndex LogicalMin {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetLogicalMin (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDElementGetLogicalMax (IOHIDElementRef element);

		public CFIndex LogicalMax {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetLogicalMax (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDElementGetPhysicalMin (IOHIDElementRef element);

		public CFIndex PhysicallMin {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetPhysicalMin (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFIndex IOHIDElementGetPhysicalMax (IOHIDElementRef element);

		public CFIndex PhysicalMax {
			get {
				ThrowIfDisposed ();
				return IOHIDElementGetPhysicalMax (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFTypeRef IOHIDElementGetProperty (IOHIDElementRef element, CFStringRef key);

		[DllImport (Constants.IOKitLibrary)]
		extern static Boolean IOHIDElementSetProperty (IOHIDElementRef element, CFStringRef key, CFTypeRef property);

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

