//
// CFSet.cs
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
using System.Runtime.InteropServices;
using MonoMac;
using MonoMac.ObjCRuntime;

using CFAllocatorRef = System.IntPtr;
using CFStringRef = System.IntPtr;
using CFHashCode = System.UInt32;
using CFTypeID = System.UInt32;
using CFMutableSetRef = System.IntPtr;
using CFSetRef = System.IntPtr;

namespace MonoMac.CoreFoundation
{
	/// <summary>
	/// Type of the callback function used by CFSets for retaining values.
	/// </summary>
	/// <param name="allocator">The allocator of the CFSet.</param>
	/// <param name="value">The value to retain.</param>
	/// <returns>
	/// The value to store in the set, which is usually the value
	// parameter passed to this callback, but may be a different
	// value if a different value should be stored in the set.
	/// </returns>
	internal delegate IntPtr CFSetRetainCallBack (CFAllocatorRef allocator, [In] IntPtr value);

	/// <summary>
	/// Type of the callback function used by CFSets for releasing a retain on values.
	/// </summary>
	/// <param name="allocator">The allocator of the CFSet.</param>
	/// <param name="value">The value to release.</param>
	internal delegate void CFSetReleaseCallBack (CFAllocatorRef allocator, [In] IntPtr value);

	/// <summary>
	/// Type of the callback function used by CFSets for describing values.
	/// </summary>
	/// <param name="value">The value to describe.</param>
	/// <returns>A description of the specified value.</returns>
	internal delegate CFStringRef CFSetCopyDescriptionCallBack ([In] IntPtr value);

	/// <summary>
	/// Type of the callback function used by CFSets for comparing values.
	/// </summary>
	/// <param name="value1">The first value to compare.</param>
	/// <param name="value2">The second value to compare.</param>
	/// <returns>True if the values are equal, otherwise false.</returns>
	internal delegate Boolean CFSetEqualCallBack ([In] IntPtr value1, [In] IntPtr value2);

	/// <summary>
	/// Type of the callback function used by CFSets for hashing values.
	/// </summary>
	/// <param name="value">The value to hash.</param>
	/// <returns>The hash of the value.</returns>
	internal delegate CFHashCode CFSetHashCallBack ([In] IntPtr value);

	/// <summary>
	/// Type of the callback function used by the apply functions of CFSets.
	/// </summary>
	/// <param name="value">The current value from the set.</param>
	/// <param name="context">The user-defined context parameter given to the apply
	/// function.</param>
	internal delegate void CFSetApplierFunction ([In] IntPtr value, IntPtr context);

	/// <summary>
	/// Structure containing the callbacks of a CFSet.
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	internal struct CFSetCallBacks
	{
		public CFIndex version;

		public CFSetRetainCallBack retain;

		public CFSetReleaseCallBack release;

		public CFSetCopyDescriptionCallBack	copyDescription;

		public CFSetEqualCallBack equal;

		public CFSetHashCallBack hash;
	}

	public class CFSet : CFType, ICloneable
	{
		internal static readonly CFSetCallBacks kCFTypeSetCallBacks;
		internal static readonly CFSetCallBacks kCFCopyStringSetCallBacks;

		static CFSet ()
		{
			var handle = Dlfcn.dlopen (Constants.CoreFoundationLibrary, 0);
			try {
				var kCFTypeSetCallBacksPtr = Dlfcn.GetIndirect (handle, "kCFTypeSetCallBacks");
				kCFTypeSetCallBacks = (CFSetCallBacks)Marshal.PtrToStructure (kCFTypeSetCallBacksPtr, typeof(CFSetCallBacks));
				var kCFCopyStringSetCallBacksPtr = Dlfcn.GetIndirect (handle, "kCFCopyStringSetCallBacks");
				kCFCopyStringSetCallBacks = (CFSetCallBacks)Marshal.PtrToStructure (kCFCopyStringSetCallBacksPtr, typeof(CFSetCallBacks));
			} finally {
				Dlfcn.dlclose (handle);
			}
		}

		internal CFSet (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		~CFSet ()
		{
			Dispose (false);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFTypeID CFSetGetTypeID ();

		public static uint TypeID {
			get { return CFSetGetTypeID (); }
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFSetRef CFSetCreate (CFAllocatorRef allocator,
		                                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)][In] IntPtr[] values,
		                                    CFIndex numValues,
		                                    [MarshalAs (UnmanagedType.LPStruct)][In] CFSetCallBacks callBacks);

		public static CFSet FromIntPtrs (params IntPtr[] values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");
			var setRef = CFSetCreate (IntPtr.Zero, values, (CFIndex)values.Length, kCFTypeSetCallBacks);
			if (setRef == IntPtr.Zero)
				return null;
			return new CFSet (setRef, true);
		}

		public static CFSet FromNativeObjects (params INativeObject[] values)
		{
			var valuePtrs = new IntPtr[values.Length];
			for (int i = 0; i < values.Length; i++)
				valuePtrs [i] = values [i].Handle;
			return FromIntPtrs (valuePtrs);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFSetRef CFSetCreateCopy(CFAllocatorRef allocator, CFSetRef theSet);

		#region ICloneable implementation

		public object Clone ()
		{
			ThrowIfDisposed ();
			var setRef = CFSetCreateCopy (IntPtr.Zero, Handle);
			return new CFSet (setRef, true);
		}

		#endregion

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFIndex CFSetGetCount(CFSetRef theSet);

		public int Count {
			get {
				ThrowIfDisposed ();
				return CFSetGetCount (Handle);
			}
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFIndex CFSetGetCountOfValue(CFSetRef theSet, [In] IntPtr value);

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static Boolean CFSetContainsValue(CFSetRef theSet, [In] IntPtr value);

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static IntPtr CFSetGetValue(CFSetRef theSet, [In] IntPtr value);

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static Boolean CFSetGetValueIfPresent(CFSetRef theSet, [In] IntPtr candidate, ref IntPtr value);

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static void CFSetGetValues(CFSetRef theSet, IntPtr values);

		internal IntPtr[] Values {
			get {
				ThrowIfDisposed ();
				var valuesPtr = Marshal.AllocHGlobal (IntPtr.Size * Count);
				CFSetGetValues (Handle, valuesPtr);
				var values = new IntPtr[Count];
				for (int i = 0; i < Count; i++)
					values [i] = Marshal.ReadIntPtr (valuesPtr, i * IntPtr.Size);
				Marshal.FreeHGlobal (valuesPtr);
				return values;
			}
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static void CFSetApplyFunction(CFSetRef theSet, CFSetApplierFunction applier, IntPtr context);
	}

	public class CFMutableSet : CFSet
	{
		internal CFMutableSet (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		~CFMutableSet ()
		{
			Dispose (false);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFMutableSetRef CFSetCreateMutable(CFAllocatorRef allocator,
		                                                 CFIndex capacity,
		                                                 [MarshalAs (UnmanagedType.LPStruct)][In] CFSetCallBacks callBacks);

		public CFMutableSet (int initalCapacity = 0) 
			: base (CFSetCreateMutable (IntPtr.Zero, (CFIndex)initalCapacity, kCFTypeSetCallBacks), true)
		{
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static CFMutableSetRef CFSetCreateMutableCopy(CFAllocatorRef allocator, CFIndex capacity, CFSetRef theSet);

		#region ICloneable implementation

		public new object Clone ()
		{
			ThrowIfDisposed ();
			var setRef = CFSetCreateMutableCopy (IntPtr.Zero, 0, Handle);
			return new CFSet (setRef, true);
		}

		#endregion

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static void CFSetAddValue(CFMutableSetRef theSet, [In] IntPtr value);

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static void CFSetReplaceValue(CFMutableSetRef theSet, [In] IntPtr value);

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static void CFSetSetValue(CFMutableSetRef theSet, [In] IntPtr value);

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static void CFSetRemoveValue(CFMutableSetRef theSet, [In] IntPtr value);

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static void CFSetRemoveAllValues(CFMutableSetRef theSet);
	}
}

