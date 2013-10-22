//
// IOObject.cs
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
using System.Reflection;
using System.Runtime.InteropServices;
using MonoMac;
using MonoMac.CoreFoundation;
using MonoMac.ObjCRuntime;

using CFStringRef = System.IntPtr;
using boolean_t = System.Boolean;
using io_name_t = System.String;
using io_object_t = System.IntPtr;
using kern_return_t = MonoMac.IOKit.IOReturn;
using uint32_t = System.UInt32;

namespace MonoMac.IOKit
{
	public class IOObject : INativeObject, IDisposable
	{
		static Dictionary<IntPtr, WeakReference> objectMap;
		static object lockObj = new object ();
		static Dictionary<Type, Func<IntPtr, bool, object>> constructorCache;

		static IOObject ()
		{
			objectMap = new Dictionary<CFStringRef, WeakReference> ();
			constructorCache = new Dictionary<Type, Func<IntPtr, bool, object>> ();
		}

		internal IOObject (IntPtr handle, bool owns)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("handle");
			if (!owns)
				IOObjectRetain (handle);
			Handle = handle;
			Register ();
		}

		~IOObject ()
		{
			Dispose (false);
		}

		void Register ()
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Invalid Handle.");
			lock (lockObj) {
				if (objectMap.ContainsKey (Handle) && objectMap [Handle].IsAlive)
					throw new InvalidOperationException ("Object is alread registered.");
				objectMap [Handle] = new WeakReference (this);
			}
		}

		void Unregister ()
		{
			lock (lockObj)
				objectMap.Remove (Handle);
		}

		#region INativeObject implementation

		public IntPtr Handle { get; protected set; }

		#endregion

		#region API Wrappers

		[DllImport (Constants.IOKitLibrary)]
		internal extern static kern_return_t IOObjectRelease (io_object_t obj);

		[DllImport (Constants.IOKitLibrary)]
		internal extern static kern_return_t IOObjectRetain (io_object_t obj);

//		[DllImport (Constants.IOKitLibrary)]
//		extern static CFStringRef IOObjectGetClass (io_object_t obj, io_name_t className);

		[DllImport (Constants.IOKitLibrary)]
		extern static CFStringRef IOObjectCopyClass (io_object_t obj);

		[Since (4,0)]
		public string ClassName {
			get {
				ThrowIfDisposed ();
				var classNameRef = IOObjectCopyClass (Handle);
				var className = new CFString (classNameRef);
				CFType.Release (classNameRef);
				return className.ToString ();
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFStringRef IOObjectCopySuperclassForClass (CFStringRef classname);

		[Since (4,0)]
		public static string GetSuperclassForClass (string className)
		{
			var classNameAsCFString = new CFString (className);
			var superclassNameRef = IOObjectCopySuperclassForClass (classNameAsCFString.Handle);
			if (superclassNameRef == IntPtr.Zero)
				return null;
			var bundleIdentifier = new CFString (superclassNameRef);
			CFType.Release (superclassNameRef);
			return bundleIdentifier.ToString ();
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFStringRef IOObjectCopyBundleIdentifierForClass (CFStringRef classname);

		[Since (4,0)]
		public static string GetBundleIdentifierForClass (string className)
		{
			var classNameAsCFString = new CFString (className);
			var bundleIdentifierRef = IOObjectCopyBundleIdentifierForClass (classNameAsCFString.Handle);
			var bundleIdentifier = new CFString (bundleIdentifierRef);
			if (bundleIdentifierRef == IntPtr.Zero)
				return null;
			CFType.Release (bundleIdentifierRef);
			return bundleIdentifier.ToString ();
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static boolean_t IOObjectIsEqualTo (io_object_t obj1, io_object_t obj2);

		public override bool Equals (object obj)
		{
			var ioObj = obj as IOObject;
			if (ioObj != null)
				return IOObjectIsEqualTo (Handle, ioObj.Handle);
			return base.Equals (obj);
		}

		public override int GetHashCode ()
		{
			return 0;
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOObjectGetKernelRetainCount (io_object_t obj);

		[Since (6,0)]
		public uint KernelRetainCount {
			get {
				ThrowIfDisposed ();
				return IOObjectGetKernelRetainCount (Handle);
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOObjectGetUserRetainCount (io_object_t obj);

		[Since (6,0)]
		public uint UserRetainCount {
			get {
				ThrowIfDisposed ();
				return IOObjectGetUserRetainCount (Handle);
			}
		}

//		[DllImport (Constants.IOKitLibrary)]
//		extern static uint32_t IOObjectGetRetainCount (io_object_t obj);

		[DllImport (Constants.IOKitLibrary)]
		extern static boolean_t IOObjectConformsTo (io_object_t obj, io_name_t className);

		public bool ConformsTo (string className)
		{
			ThrowIfDisposed ();
			return IOObjectConformsTo (Handle, className);
		}

		#endregion

		#region custom API

		public T Cast<T> () where T : IOObject
		{
			if (!ConformsTo (typeof(T).Name))
				throw new InvalidCastException ();
			return MarshalNativeObject<T> (Handle, false);
		}

		internal static void ThrowIfError (IOReturn result)
		{
			if (result != IOReturn.Success)
				throw result.ToNSErrorException ();
		}

		internal static T MarshalNativeObject<T> (IntPtr handle, bool owns) where T : IOObject
		{
			lock (lockObj) {
				if (objectMap.ContainsKey (handle) && objectMap [handle].IsAlive)
					return (T)objectMap [handle].Target;
			}
			if (!constructorCache.ContainsKey (typeof(T))) {
				var constructorInfo = typeof(T).GetConstructor (BindingFlags.Instance | BindingFlags.NonPublic, null,
				                                                new Type[] { typeof(IntPtr), typeof(bool) }, null);
				if (constructorInfo == null)
					throw new ArgumentException (string.Format ("Type '{0}' does not contain a constructor 'T(IntPtr, bool)'", typeof(T).Name));
				constructorCache.Add (typeof(T), (handle2, owns2) => (T)constructorInfo.Invoke (new object [] { handle2, owns2 }));
			}
			return (T)constructorCache [typeof(T)].Invoke (handle, owns);
		}

		public event EventHandler Disposing;

		public event EventHandler Disposed;

		#endregion

		#region IDisposable implementation

		public void Dispose ()
		{
			OnDisposing ();
			Dispose (true);
			GC.SuppressFinalize (this);
			OnDisposed ();
		}

		protected virtual void Dispose (bool disposing)
		{
			Unregister ();
			if (Handle != IntPtr.Zero) {
				IOObjectRelease (Handle);
				Handle = IntPtr.Zero;
			}
		}

		protected void ThrowIfDisposed ()
		{
			if (Handle == IntPtr.Zero)
				throw new ObjectDisposedException (GetType ().Name);
		}

		#endregion

		protected void OnDisposing ()
		{
			if (Disposing != null)
				Disposing (this, EventArgs.Empty);
		}

		protected void OnDisposed ()
		{
			if (Disposed != null)
				Disposed (this, EventArgs.Empty);
		}
	}
}

