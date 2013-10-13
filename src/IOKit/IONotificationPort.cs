//
// IONotificationPort.cs
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
using System.Reflection;
using System.Runtime.InteropServices;
using MonoMac;
using MonoMac.CoreFoundation;
using MonoMac.Kernel.Mach;
using MonoMac.ObjCRuntime;

using mach_port_t = System.IntPtr;
using IONotificationPortRef = System.IntPtr;
using CFRunLoopSourceRef = System.IntPtr;
using dispatch_queue_t = System.IntPtr;

namespace MonoMac.IOKit
{
	public class IONotificationPort : INativeObject, IDisposable
	{
		static readonly IntPtr kIOMasterPortDefault = IntPtr.Zero;

		Port machPort;
		CFRunLoopSource runLoopSource;


		public IONotificationPort () : this (kIOMasterPortDefault)
		{
		}

		internal IONotificationPort (IOMasterPort masterPort) : this (masterPort.Handle)
		{
		}

		~IONotificationPort ()
		{
			Dispose (false);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		void ThrowIfDisposed ()
		{
			if (Handle == IntPtr.Zero)
				throw new ObjectDisposedException (GetType ().Name);
		}

		public IntPtr Handle { get; private set; }

		#region API wrappers

		[DllImport (Constants.IOKitLibrary)]
		extern static IntPtr IONotificationPortCreate (mach_port_t masterPort);

		IONotificationPort (IntPtr masterPort)
		{
			Handle = IONotificationPortCreate (masterPort);
			if (Handle == IntPtr.Zero)
				throw new Exception ("Could not create notification port");
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static void IONotificationPortDestroy (IONotificationPortRef notify);

		protected void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				IONotificationPortDestroy (Handle);
				Handle = IntPtr.Zero;
			}
		}

		[DllImport (Constants.IOKitLibrary)]
		extern static CFRunLoopSourceRef IONotificationPortGetRunLoopSource (IONotificationPortRef notify);

		public CFRunLoopSource RunLoopSource {
			get {
				ThrowIfDisposed ();
				if (runLoopSource == null) {
					var runLoopSourceRef = IONotificationPortGetRunLoopSource (Handle);
					runLoopSource = new CFRunLoopSource (runLoopSourceRef, false);
				}
				return runLoopSource;
			}
		}
	
		[DllImport (Constants.IOKitLibrary)]
		extern static mach_port_t IONotificationPortGetMachPort (IONotificationPortRef notify);

		public Port MachPort {
			get {
				ThrowIfDisposed ();
				if (machPort == null) {
					var machPortRef = IONotificationPortGetMachPort (Handle);
					machPort = new Port (machPortRef);
				}
				return machPort;
			}
		}
	
		[DllImport (Constants.IOKitLibrary)]
		extern static void IODispatchCalloutFromMessage(IntPtr unused, MessageHeader msg, IONotificationPortRef reference);

		public void DispatchCalloutFromMessage (MessageHeader messageHeader)
		{
			ThrowIfDisposed ();
			IODispatchCalloutFromMessage (IntPtr.Zero, messageHeader, Handle);
		}
	
		[DllImport (Constants.IOKitLibrary)]
		extern static void IONotificationPortSetDispatchQueue (IONotificationPortRef notify, dispatch_queue_t queue);

		[Since (6,0)]
		public void SetDispatchQueue (DispatchQueue queue)
		{
			ThrowIfDisposed ();
			IONotificationPortSetDispatchQueue (Handle, queue.Handle);
		}

		#endregion
	}
}

