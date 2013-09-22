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
		static Dictionary<Type, Func<IntPtr, bool, object>> constructorCache;

		static IOObject ()
		{
			constructorCache = new Dictionary<Type, Func<IntPtr, bool, object>> ();
		}

		internal IOObject (IntPtr handle, bool owns)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("handle");
			if (!owns)
				IOObjectRetain (handle);
			Handle = handle;
		}

		~IOObject ()
		{
			Dispose (false);
		}

		/// <summary>
		/// Gets the class name of this IOObject.
		/// </summary>
		/// <exception cref="ObjectDisposedException">If this object has been disposed.</exception>
		[Since (4,0)]
		public string ClassName {
			get {
				ThrowIfDisposed ();
				var bundleIdentifierHandle = IOObjectCopyClass (Handle);
				var bundleIdentifier = new CFString (bundleIdentifierHandle);
				CFType.Release (bundleIdentifierHandle);
				return bundleIdentifier.ToString ();
			}
		}

		/// <summary>
		/// Gets the kernel retain count of an IOKit object.
		/// </summary>
		/// <remarks>This function may be used in diagnostics to determine the current retain count of the kernel object at the kernel level.</remarks>
		/// <exception cref="ObjectDisposedException">If this object has been disposed.</exception>
		[Since (6,0)]
		public uint KernelRetainCount {
			get {
				ThrowIfDisposed ();
				return IOObjectGetKernelRetainCount (Handle);
			}
		}

		/// <summary>
		/// Gets the retain count for the current process of an IOKit object.
		/// </summary>
		/// <remarks>This function may be used in diagnostics to determine the current retain count for the calling process of the kernel object.</remarks>
		/// <exception cref="ObjectDisposedException">If this object has been disposed.</exception>
		[Since (6,0)]
		public uint UserRetainCount {
			get {
				ThrowIfDisposed ();
				return IOObjectGetUserRetainCount (Handle);
			}
		}

		/// <summary>
		/// Gets the superclass name of the given class.
		/// </summary>
		/// <returns>The resulting superclass name. If there is no superclass, or a valid class name is not passed in, then <code>null</code> is returned.</returns>
		/// <param name="classname">The name of the class.</param>
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

		/// <summary>
		/// Gets the bundle identifier of the given class.
		/// </summary>
		/// <returns>The resulting bundle identifier. If a valid class name is not passed in, then <code>null</code> is returned.</returns>
		/// <param name="classname">The name of the class.</param>
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

		/// <summary>
		/// Performs an OSDynamicCast operation on an IOKit object.
		/// </summary>
		/// <returns>If the represents an object in the kernel that dynamic casts to the class <c>true</c> is returned,
		/// otherwise <c>false</c>.</returns>
		/// <param name="className">The name of the class.</param>
		/// <exception cref="ObjectDisposedException">If this object has been disposed.</exception>
		public bool ConformsTo (string className)
		{
			ThrowIfDisposed ();
			return IOObjectConformsTo (Handle, className);
		}

		public T Cast<T> () where T : IOObject
		{
			if (!ConformsTo (typeof(T).Name))
				throw new InvalidCastException ();
			return MarshalNativeObject<T> (Handle, false);
		}

		/// <summary>
		/// Releases all resource used by the <see cref="IOKit.IOObject"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="IOKit.IOObject"/>. The <see cref="Dispose"/>
		/// method leaves the <see cref="IOKit.IOObject"/> in an unusable state. After calling <see cref="Dispose"/>, you must
		/// release all references to the <see cref="IOKit.IOObject"/> so the garbage collector can reclaim the memory that
		/// the <see cref="IOKit.IOObject"/> was occupying.</remarks>
		public void Dispose ()
		{
			OnDisposing ();
			Dispose (true);
			GC.SuppressFinalize (this);
			OnDisposed ();
		}

		protected virtual void Dispose (bool disposing)
		{
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

		internal static void ThrowIfError (IOReturn result)
		{
			if (result != IOReturn.Success)
				throw new IOKitException (result);
		}

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

		internal static T MarshalNativeObject<T> (IntPtr handle, bool owns) where T : IOObject
		{
			if (!constructorCache.ContainsKey (typeof(T))) {
				var constructorInfo = typeof(T).GetConstructor (BindingFlags.Instance | BindingFlags.NonPublic, null,
				                                                new Type[] { typeof(IntPtr), typeof(bool) }, null);
				if (constructorInfo == null)
					throw new ArgumentException (string.Format ("Type '{0}' does not contain a constructor 'T(IntPtr, bool)'", typeof(T).Name));
				constructorCache.Add (typeof(T), (handle2, owns2) => (T)constructorInfo.Invoke (new object [] { handle2, owns2 }));
			}
			return (T)constructorCache [typeof(T)].Invoke (handle, owns);
		}

		/// <summary>
		/// Occurs when this instance is about to be disposed.
		/// </summary>
		public event EventHandler Disposing;

		/// <summary>
		/// Occurs when this instance has been disposed.
		/// </summary>
		public event EventHandler Disposed;

		/// <summary>
		/// Gets the handle of the native object.
		/// </summary>
		public IntPtr Handle { get; protected set; }

		/// <summary>
		/// Releases an object handle previously returned by IOKitLib.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="obj">The IOKit object to release.</param>
		/// <remarks>All objects returned by IOKitLib should be released with this function when access to them is no longer needed.
		/// Using the object after it has been released may or may not return an error,
		/// depending on how many references the task has to the same object in the kernel.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		internal extern static kern_return_t IOObjectRelease (io_object_t obj);

		/// <summary>
		/// Retains an object handle previously returned by IOKitLib.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="obj">The IOKit object to retain.</param>
		/// <remarks>Gives the caller an additional reference to an existing object handle previously returned by IOKitLib.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		internal extern static kern_return_t IOObjectRetain (io_object_t obj);

		/// <summary>
		/// Return the class name of an IOKit object.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="obj">The IOKit object.</param>
		/// <param name="className">Caller allocated buffer to receive the name string.</param>
		/// <remarks>This function uses the OSMetaClass system in the kernel to derive the name of the class the object is an instance of.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static CFStringRef IOObjectGetClass (io_object_t obj, io_name_t className);

		/// <summary>
		/// Return the class name of an IOKit object.
		/// </summary>
		/// <returns>The resulting CFStringRef. This should be released by the caller. If a valid object is not passed in, then NULL is returned.</returns>
		/// <param name="obj">The IOKit object.</param>
		/// <remarks>This function does the same thing as IOObjectGetClass, but returns the result as a CFStringRef.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static CFStringRef IOObjectCopyClass (io_object_t obj);

		/// <summary>
		/// Return the superclass name of the given class.
		/// </summary>
		/// <returns>The resulting CFStringRef. This should be released by the caller. If there is no superclass, or a valid class name is not passed in, then NULL is returned.</returns>
		/// <param name="classname">The name of the class as a CFString.</param>
		/// <remarks>This function uses the OSMetaClass system in the kernel to derive the name of the superclass of the class.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static CFStringRef IOObjectCopySuperclassForClass (CFStringRef classname);

		/// <summary>
		/// Return the bundle identifier of the given class.
		/// </summary>
		/// <returns>The resulting CFStringRef. This should be released by the caller. If a valid class name is not passed in, then NULL is returned.</returns>
		/// <param name="classname">The name of the class as a CFString.</param>
		/// <remarks>This function uses the OSMetaClass system in the kernel to derive the name of the kmod, which is the same as the bundle identifier.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static CFStringRef IOObjectCopyBundleIdentifierForClass (CFStringRef classname);

		/// <summary>
		/// Performs an OSDynamicCast operation on an IOKit object.
		/// </summary>
		/// <returns>If the object handle is valid, and represents an object in the kernel that dynamic casts to the class true is returned, otherwise false.</returns>
		/// <param name="obj">An IOKit object.</param>
		/// <param name="className">The name of the class, as a C-string.</param>
		/// <remarks>This function uses the OSMetaClass system in the kernel to determine if the object will dynamic cast to a class,
		/// specified as a C-string. In other words, if the object is of that class or a subclass.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static boolean_t IOObjectConformsTo (io_object_t obj, io_name_t className);

		/// <summary>
		/// Checks two object handles to see if they represent the same kernel object.
		/// </summary>
		/// <returns>If both object handles are valid, and represent the same object in the kernel true is returned, otherwise false.</returns>
		/// <param name="obj1">An IOKit object.</param>
		/// <param name="obj2">Another IOKit object.</param>
		/// <remarks>If two object handles are returned by IOKitLib functions, this function will compare them to see if they represent the same kernel object.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static boolean_t IOObjectIsEqualTo (io_object_t obj1, io_object_t obj2);

		/// <summary>
		/// Returns kernel retain count of an IOKit object.
		/// </summary>
		/// <returns>If the object handle is valid, the kernel objects retain count is returned, otherwise zero is returned.</returns>
		/// <param name="obj">An IOKit object.</param>
		/// <remarks>This function may be used in diagnostics to determine the current retain count of the kernel object at the kernel level.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOObjectGetKernelRetainCount (io_object_t obj);

		/// <summary>
		/// Returns the retain count for the current process of an IOKit object.
		/// </summary>
		/// <returns>If the object handle is valid, the objects user retain count is returned, otherwise zero is returned.</returns>
		/// <param name="obj">An IOKit object.</param>
		/// <remarks>This function may be used in diagnostics to determine the current retain count for the calling process of the kernel object.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOObjectGetUserRetainCount (io_object_t obj);

		/// <summary>
		/// Returns kernel retain count of an IOKit object. Identical to IOObjectGetKernelRetainCount() but available prior to Mac OS 10.6.
		/// </summary>
		/// <returns>If the object handle is valid, the kernel objects retain count is returned, otherwise zero is returned.</returns>
		/// <param name="obj">An IOKit object.</param>
		/// <remarks>This function may be used in diagnostics to determine the current retain count of the kernel object at the kernel level.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static uint32_t IOObjectGetRetainCount (io_object_t obj);
	}
}

