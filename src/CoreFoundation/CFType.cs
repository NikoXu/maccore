//
// Copyright 2012 Xamarin
//
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using System.Reflection;

namespace MonoMac.CoreFoundation {
	public abstract class CFType : INativeObject, IDisposable
	{
		protected static readonly IntPtr NULL = IntPtr.Zero;

		static object lockObject = new object ();
		static Dictionary<IntPtr, WeakReference> objectMap =
			new Dictionary<IntPtr, WeakReference> ();

		protected CFType ()
		{
		}

		[Preserve (Conditional = true)]
		internal CFType (IntPtr handle, bool owns)
		{
			if (!owns)
				Retain (handle);
			Handle = handle;
			if (Handle != IntPtr.Zero)
				Register ();
		}

		public IntPtr Handle { get; internal set; }

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static uint CFGetTypeID (IntPtr typeRef);

		public static uint GetTypeID (IntPtr handle) {
			return CFGetTypeID (handle);
		}

		public uint GetTypeID () {
			return CFGetTypeID (Handle);
		}

		[DllImport (Constants.CoreFoundationLibrary)]
		extern static IntPtr CFCopyDescription (IntPtr ptr);

		public static string GetTypeDescription (IntPtr handle)
		{
			using (var s = new CFString (CFCopyDescription (handle)))
				return s.ToString ();
		}

		public string Description {
			get {
				ThrowIfDisposed ();
				return GetTypeDescription (Handle);
			}
		}

		protected void Register ()
		{
			ThrowIfDisposed ();
			lock (lockObject) {
				if (objectMap.ContainsKey (Handle) && objectMap [Handle].IsAlive)
					throw new InvalidOperationException ("Object is already registered with this handle");
				objectMap [Handle] = new WeakReference (this);
			}
		}

		protected void Unregister ()
		{
			lock (lockObject)
				objectMap.Remove (Handle);
		}

		public static T GetCFObject<T> (IntPtr handle) where T : CFType
		{
			if (handle == IntPtr.Zero)
				return null;
			lock (lockObject) {
				WeakReference reference;
				if (objectMap.TryGetValue (handle, out reference) && reference.IsAlive)
					return (T)reference.Target;
				return (T)Activator.CreateInstance (typeof(T),
				                                    BindingFlags.NonPublic | BindingFlags.Instance,
				                                    null,
				                                    new object[] { handle, false },
				                                    null);
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				Unregister ();
				Release (Handle);
				Handle = IntPtr.Zero;
			}
		}

		protected void ThrowIfDisposed ()
		{
			if (Handle == IntPtr.Zero)
				throw new ObjectDisposedException (GetType ().Name);
		}

		[DllImport (Constants.CoreFoundationLibrary, EntryPoint = "CFRelease")]
		internal extern static void Release (IntPtr obj);

		[DllImport (Constants.CoreFoundationLibrary, EntryPoint = "CFRetain")]
		internal extern static IntPtr Retain (IntPtr obj);
	}
}
