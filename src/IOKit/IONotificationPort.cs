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

		Mach.Port machPort;
		CFRunLoopSource runLoopSource;

		/// <summary>Creates and returns a IONotificationPort object using the default master port
		/// for receiving IOKit notifications of new devices or state changes.</summary>
		/// <remarks>Creates the notification object to receive notifications from IOKit of new device arrivals or state changes.
		/// The notification object can be supply a CFRunLoopSource, or mach_port_t to be used to listen for events.</remarks>
		/// <returns>The notification object.</returns>
		/// <exception cref="Exception">If the object was not created</exception>
		public IONotificationPort () : this (kIOMasterPortDefault)
		{
		}

		/// <summary>Creates and returns a IONotificationPort object for receiving IOKit notifications
		/// of new devices or state changes.</summary>
		/// <remarks>Creates the notification object to receive notifications from IOKit of new device arrivals or state changes.
		/// The notification object can be supply a CFRunLoopSource, or mach_port_t to be used to listen for events.</remarks>
		/// <param name="masterPort">The master port obtained from IOMasterPort().</param>
		/// <returns>The notification object.</returns>
		/// <exception cref="Exception">If the object was not created</exception>
		internal IONotificationPort (IOMasterPort masterPort) : this (masterPort.Handle)
		{
		}

		IONotificationPort (IntPtr masterPort)
		{
			Handle = IONotificationPortCreate (masterPort);
			if (Handle == IntPtr.Zero)
				throw new Exception ("Could not create notification port");
		}

		~IONotificationPort ()
		{
			Dispose (false);
		}

		/// <summary>
		/// CFRunLoopSource to be used to listen for notifications.
		/// </summary>
		/// <remarks>A notification object may deliver notifications to a CFRunLoop 
		/// by adding the run loop source returned by this function to the run loop.
		/// 
		/// The caller should not release this CFRunLoopSource. It will be releaed when the IONotificationPort
		/// object is disposed./remarks>
		/// <exception cref="ObjectDisposedException">If the object has been disposed (Handle == 0)</exception>
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

		/// <summary>
		/// Gets a mach_port to be used to listen for notifications.
		/// </summary>
		/// <remarks>A notification object may deliver notifications to a mach messaging client 
		/// if they listen for messages on the port obtained from this function. 
		/// Callbacks associated with the notifications may be delivered by calling 
		/// IODispatchCalloutFromMessage with messages received.
		///
		/// The caller should not release this Mach.Port. It will be releaed when the IONotificationPort
		/// object is disposed./remarks>
		/// <exception cref="ObjectDisposedException">If the object has been disposed (Handle == 0)</exception>
		public Mach.Port MachPort {
			get {
				ThrowIfDisposed ();
				if (machPort == null) {
					var machPortRef = IONotificationPortGetMachPort (Handle);
					machPort = new Mach.Port (machPortRef);
				}
				return machPort;
			}
		}

		/// <summary>
		/// Dispatches callback notifications from a mach message.
		/// </summary>
		/// <param name="messageHeader">The message received.</param>
		/// <param name="reference">Pass the IONotificationPortRef for the object.</param>
		/// <remarks>A notification object may deliver notifications to a mach messaging client, 
		/// which should call this function to generate the callbacks associated with the notifications arriving on the port.
		/// </remarks>
		/// <exception cref="ObjectDisposedException">If the object has been disposed (Handle == 0)</exception>
		public void DispatchCalloutFromMessage (Mach.MessageHeader messageHeader)
		{
			ThrowIfDisposed ();
			IODispatchCalloutFromMessage (IntPtr.Zero, messageHeader, Handle);
		}

		/// <summary>
		/// Sets a dispatch queue to be used to listen for notifications.
		/// </summary>
		/// <param name="queue">A dispatch queue.</param>
		/// <remarks>A notification object may deliver notifications to a dispatch client.</remarks>
		/// <exception cref="ObjectDisposedException">If the object has been disposed (Handle == 0)</exception>
		[Since (6,0)]
		public void SetDispatchQueue (DispatchQueue queue)
		{
			ThrowIfDisposed ();
			IONotificationPortSetDispatchQueue (Handle, queue.Handle);
		}

		/// <summary>
		/// Releases all resource used by the <see cref="IOKit.IONotificationPort"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="IOKit.IONotificationPort"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="IOKit.IONotificationPort"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="IOKit.IONotificationPort"/> so the
		/// garbage collector can reclaim the memory that the <see cref="IOKit.IONotificationPort"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				IONotificationPortDestroy (Handle);
				Handle = IntPtr.Zero;
			}
		}

		void ThrowIfDisposed ()
		{
			if (Handle == IntPtr.Zero)
				throw new ObjectDisposedException (GetType ().Name);
		}

		public IntPtr Handle { get; private set; }

		/// <summary>
		/// Creates and returns a notification object for receiving IOKit notifications of new devices or state changes.
		/// </summary>
		/// <returns>A reference to the notification object.</returns>
		/// <param name="masterPort">The master port obtained from IOMasterPort(). Pass kIOMasterPortDefault to look up the default master port.</param>
		/// <remarks>Creates the notification object to receive notifications from IOKit of new device arrivals or state changes.
		/// The notification object can be supply a CFRunLoopSource, or mach_port_t to be used to listen for events.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static IntPtr IONotificationPortCreate (mach_port_t masterPort);

		/// <summary>
		/// Destroys a notification object created with IONotificationPortCreate.
		/// Also destroys any mach_port's or CFRunLoopSources obatined from 
		/// <see cref="IONotificationPortGetRunLoopSource"/>
		/// or <see cref="IONotificationPortGetMachPort"/>
		/// </summary>
		/// <param name="notify">A reference to the notification object.</param>
		[DllImport (Constants.IOKitLibrary)]
		extern static void IONotificationPortDestroy (IONotificationPortRef notify);

		/// <summary>
		/// Returns a CFRunLoopSource to be used to listen for notifications.
		/// </summary>
		/// <returns>A CFRunLoopSourceRef for the notification object.</returns>
		/// <param name="notify">The notification object.</param>
		/// <remarks>A notification object may deliver notifications to a CFRunLoop 
		/// by adding the run loop source returned by this function to the run loop.
		///
		/// The caller should not release this CFRunLoopSource. Just call 
		/// <see cref="IONotificationPortDestroy"/> to dispose of the
		///	IONotificationPortRef and the CFRunLoopSource when done.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static CFRunLoopSourceRef IONotificationPortGetRunLoopSource (IONotificationPortRef notify);

		/// <summary>
		/// Returns a mach_port to be used to listen for notifications.
		/// </summary>
		/// <returns>A mach_port for the notification object.</returns>
		/// <param name="notify">The notification object.</param>
		/// <remarks>A notification object may deliver notifications to a mach messaging client 
		/// if they listen for messages on the port obtained from this function. 
		/// Callbacks associated with the notifications may be delivered by calling 
		/// IODispatchCalloutFromMessage with messages received.
		///
		/// The caller should not release this mach_port_t. Just call 
		/// <see cref="IONotificationPortDestroy"/> to dispose of the
		/// mach_port_t and IONotificationPortRef when done.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static mach_port_t IONotificationPortGetMachPort (IONotificationPortRef notify);

		/// <summary>
		/// Dispatches callback notifications from a mach message.
		/// </summary>
		/// <param name="unused">Not used, set to zero.</param>
		/// <param name="msg">A pointer to the message received.</param>
		/// <param name="reference">Pass the IONotificationPortRef for the object.</param>
		/// <remarks>A notification object may deliver notifications to a mach messaging client, 
		/// which should call this function to generate the callbacks associated with the notifications arriving on the port.
		/// </remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static void IODispatchCalloutFromMessage(IntPtr unused, Mach.MessageHeader msg, IONotificationPortRef reference);

		/// <summary>
		/// Sets a dispatch queue to be used to listen for notifications.
		/// </summary>
		/// <param name="notify">The notification object.</param>
		/// <param name="queue">A dispatch queue.</param>
		/// <remarks>A notification object may deliver notifications to a dispatch client.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static void IONotificationPortSetDispatchQueue (IONotificationPortRef notify, dispatch_queue_t queue);
	}
}

