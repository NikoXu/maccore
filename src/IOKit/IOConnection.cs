//
// IOConnection.cs
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
using System.Runtime.InteropServices;
using MonoMac;
using MonoMac.CoreFoundation;
using MonoMac.Kernel.Mach;
using MonoMac.ObjCRuntime;

using CFStringRef = System.IntPtr;
using CFTypeRef = System.IntPtr;
using IOOptionBits = System.UInt32;
using io_connect_t = System.IntPtr;
using io_service_t = System.IntPtr;
using kern_return_t = MonoMac.IOKit.IOReturn;
using mach_port_t = System.IntPtr;
using task_port_t = System.IntPtr;
using uint32_t = System.UInt32;
using vm_address_t = System.IntPtr;
using vm_size_t = System.IntPtr;

namespace MonoMac.IOKit
{
	public class IOConnection : IOObject
	{
		IOService service;

		#region API wrappers

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceClose (io_connect_t connect);

		public void Close ()
		{
			ThrowIfDisposed ();
			var result = IOServiceClose (Handle);
			ThrowIfError (result);
			Handle = IntPtr.Zero;
		}


		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectAddRef (io_connect_t connect);

		internal IOConnection (IntPtr handle, bool owns) : base (handle, false)
		{
			if (!owns)
				IOConnectAddRef (handle);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectRelease (io_connect_t connect);

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				IOConnectRelease (Handle);
				Handle = IntPtr.Zero;
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectGetService (io_connect_t connect, out io_service_t service);

		public IOService Service {
			get {
				ThrowIfDisposed ();
				if (service == null) {
					IntPtr serviceRef;
					var result = IOConnectGetService (Handle, out serviceRef);
					ThrowIfError (result);
					service = new IOService (serviceRef, true);
				}
				return service;
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectSetNotificationPort (io_connect_t connect, uint32_t type, mach_port_t port, UIntPtr reference);

		public void SetNotificationPort (uint type, Port port, UIntPtr reference = default (UIntPtr))
		{
			ThrowIfDisposed ();
			var result = IOConnectSetNotificationPort (Handle, type, port.Handle, reference);
			ThrowIfError (result);
		}


//		[DllImport (Constants.IOKitLibrary)]
//		extern static kern_return_t IOConnectMapMemory(io_connect_t connect, uint32_t memoryType,
//		                                               task_port_t intoTask, ref vm_address_t atAddress,
//		                                               out vm_size_t ofSize, IOOptionBits options);

//		[DllImport (Constants.IOKitLibrary)]
//		extern static kern_return_t IOConnectUnmapMemory(io_connect_t connect, uint32_t memoryType,
//		                                                 task_port_t fromTask, ref vm_address_t atAddress);

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectSetCFProperties (io_connect_t connect, CFTypeRef properties);

		public void SetCFProperties (INativeObject properties)
		{
			ThrowIfDisposed ();
			var result = IOConnectSetCFProperties (Handle, properties.Handle);
			ThrowIfError (result);
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectSetCFProperty (io_connect_t connect, CFStringRef propertyName, CFTypeRef property);

		public void SetCFProperty (string name, INativeObject value)
		{
			ThrowIfDisposed ();
			var nameCFString = new CFString (name);
			var result = IOConnectSetCFProperty (Handle, nameCFString.Handle, value.Handle);
			ThrowIfError (result);
		}


		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectAddClient (io_connect_t connect, io_connect_t client);

		public void AddClient (IOConnection client)
		{
			ThrowIfDisposed ();
			var result = IOConnectAddClient (Handle, client.Handle);
			ThrowIfError (result);
		}

		#endregion
	}
}

