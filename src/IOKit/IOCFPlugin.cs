//
// IOCFPlugin.cs
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
using System.Runtime.InteropServices;
using MonoMac;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

using CFDictionaryRef = System.IntPtr;
using CFUUIDRef = System.IntPtr;
using IOCFPlugInInterfaceRefRef = System.IntPtr;
using SInt32 = System.Int32;
using UInt8 = System.Byte;
using io_service_t = System.IntPtr;
using kern_return_t = MonoMac.IOKit.IOReturn;

namespace MonoMac.IOKit
{
	public class IOCFPlugin : IOCFPlugin<IOCFPlugInInterface>
	{
		IOCFPlugin (IntPtr handle) : base (handle)
		{
		}

		public ushort Version {
			get {
				ThrowIfDisposed ();
				return Interface.version;
			}
		}

		public ushort Revision {
			get {
				ThrowIfDisposed ();
				return Interface.revision;
			}
		}

		public int Probe (NSDictionary propertyTable, IOService service)
		{
			ThrowIfDisposed ();
			int order;
			var result = Interface.probe (Handle, propertyTable.Handle,
			                                    service.Handle, out order);
			IOService.ThrowIfError (result);
			return order;
		}

		public void Start (NSDictionary propertyTable, IOService service)
		{
			ThrowIfDisposed ();
			var result = Interface.start (Handle, propertyTable.Handle,
			                                    service.Handle);
			IOService.ThrowIfError (result);
		}

		public void Stop ()
		{
			ThrowIfDisposed ();
			var result = Interface.stop (Handle);
			IOService.ThrowIfError (result);
		}
	}
	
	internal interface IIOCFPlugin<out T> where T : IUnknown
	{
		IntPtr Handle { get; }
		T Interface { get; }
	}

	public class IOCFPlugin<T> : IIOCFPlugin<T>, INativeObject, IDisposable where T : IUnknown
	{
		static Lazy<CFUUID> uuid;

		static IOCFPlugin ()
		{
			uuid = new Lazy<CFUUID> (() => typeof(T).GetUuid ());
		}

		protected IOCFPlugin (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("Handle cannot be IntPtr.Zero", "handle");
			Handle = handle;
			var interfaceRef = Marshal.ReadIntPtr (handle);
			Interface = (T)Marshal.PtrToStructure (interfaceRef, typeof(T));
		}

		~IOCFPlugin ()
		{
			Dispose (false);
		}

		public IntPtr Handle { get; private set; }

		public T Interface { get; private set; }

		public static CFUUID Uuid { get { return uuid.Value; } }

		public static IOCFPlugin<T2> CreateInterfaceForService<T2> (IOService service) where T2 : IUnknown
		{
			IntPtr interfaceRefRef;
			int score;
			var pluginType = typeof(T2).GetUuid ();
			var interfaceType = typeof(T).GetUuid ();
			var result = IOCFPluginStatic.IOCreatePlugInInterfaceForService (service.Handle, pluginType.Handle,
				interfaceType.Handle, out interfaceRefRef, out score);
			IOObject.ThrowIfError (result);
			return new IOCFPlugin<T2> (interfaceRefRef);
		}

		public IOCFPlugin<T2> QueryInterface<T2> () where T2 : IUnknown
		{
			ThrowIfDisposed ();
			IntPtr ppv;
			var uuid = typeof(T2).GetUuid ();
			var result = Interface.queryInterface (Handle, uuid.Bytes, out ppv);
			if (result.FAILED () || result.IS_ERROR ())
				throw new COMException ("QueryInterface failed", (int)result);
			return MarshalNativeObject<T2> (ppv);
		}

		public static IOCFPlugin<T2> MarshalNativeObject<T2> (IntPtr handle) where T2 : IUnknown
		{
			return new IOCFPlugin<T2> (handle);
		}

		public uint AddRef ()
		{
			ThrowIfDisposed ();
			return Interface.addRef (Handle);
		}

		public uint Release ()
		{
			ThrowIfDisposed ();
			return Interface.release (Handle);
		}

		protected void ThrowIfDisposed ()
		{
			if (Handle == IntPtr.Zero)
				throw new ObjectDisposedException (GetType ().Name);
		}

		protected void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				IOCFPluginStatic.IODestroyPlugInInterface (Handle);
				Handle = IntPtr.Zero;
			}
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		#endregion
	}

	[Guid ("C244E858-109C-11D4-91D4-0050E4C6426F")]
	[StructLayout (LayoutKind.Sequential)]
	public class IOCFPlugInInterface : IUnknown
	{
		/* IOCFPLUGINBASE */
		public UInt16 version;
		public UInt16 revision;
		public Probe probe;
		public Start start;
		public Stop stop;

		public delegate IOReturn Probe (IntPtr thisPointer, CFDictionaryRef propertyTable,
		                                io_service_t service, out SInt32 order);

		public delegate IOReturn Start (IntPtr thisPointer, CFDictionaryRef propertyTable,
		                                io_service_t service);

		public delegate IOReturn Stop (IntPtr thisPointer);
	}

	static class IOCFPluginStatic {
		// Have to put DllImports in separate class becuase compiler does not want them
		// in a generic type.

		[DllImport (Constants.IOKitLibrary)]
		public extern static kern_return_t IOCreatePlugInInterfaceForService (io_service_t service,
			CFUUIDRef pluginType,
			CFUUIDRef interfaceType,
			out IOCFPlugInInterfaceRefRef theInterface,
			out SInt32 theScore);

		[DllImport (Constants.IOKitLibrary)]
		public extern static kern_return_t IODestroyPlugInInterface (IOCFPlugInInterfaceRefRef @interface);
	}
}

