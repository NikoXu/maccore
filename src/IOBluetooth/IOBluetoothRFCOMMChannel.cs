//
// IOBluetoothRFCOMMChannel.cs
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MonoMac.Foundation;
using MonoMac.IOKit;
using MonoMac.ObjCRuntime;

namespace MonoMac.IOBluetooth
{
	public partial class IOBluetoothRFCOMMChannel
	{
		const string channelOpenNotificationsSelector = "onChannelOpened:channel:";

		static NotificationHandler notificationHandler;

		static IOBluetoothRFCOMMChannel ()
		{
			notificationHandler = new NotificationHandler (HandleChannelOpened);
			registerForChannelOpenNotifications (notificationHandler, new Selector (channelOpenNotificationsSelector));
		}

		public void CloseChannel ()
		{
			var result = closeChannel ();
			IOObject.ThrowIfError (result);
		}

		public Task<IOBluetoothRFCOMMChannel> WriteAsync (byte[] data)
		{
			EnsureIOBluetoothRFCOMMChannelDelegate ();
			var dataHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
			var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement (data, 0);
			var completionSource = new TaskCompletionSource<IOBluetoothRFCOMMChannel> ();
			var completionSourceHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
			var result = writeAsync (dataPtr, (ushort)data.Length, completionSourceHandle.AddrOfPinnedObject ());
			dataHandle.Free ();
			if (result != IOReturn.Success) {
				completionSourceHandle.Free ();
				throw new IOReturnException (result);
			}
			return completionSource.Task;
		}

		public void Write (byte[] data)
		{
			var gcHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
			var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement (data, 0);
			var result = writeSync (dataPtr, (ushort)data.Length);
			gcHandle.Free ();
			IOObject.ThrowIfError (result);
		}

		public void SetSerialParameters (uint speed, byte nBits, BluetoothRFCOMMParityType parity, byte bitStop)
		{
			var result = setSerialParameters (speed, nBits, parity, bitStop);
			IOObject.ThrowIfError (result);
		}

		public void SendRemoteLineStatus (BluetoothRFCOMMLineStatus lineStatus)
		{
			var result = sendRemoteLineStatus (lineStatus);
			IOObject.ThrowIfError (result);
		}

//			public override void WriteCompleted (IOBluetoothRFCOMMChannel rfcommChannel, IntPtr refcon, IOReturn error)
//			{
//				var completionSourceHandle = GCHandle.FromIntPtr (refcon);
//				var completionSource = (TaskCompletionSource<IOBluetoothRFCOMMChannel>)completionSourceHandle.Target;
//				completionSourceHandle.Free ();
//				if (error == IOReturn.Success)
//					completionSource.TrySetResult (rfcommChannel);
//				else
//					completionSource.TrySetException (new IOReturnException (error));
//			}

		//
		// Events and properties from the delegate
		//

		_IOBluetoothRFCOMMChannelDelegate EnsureIOBluetoothRFCOMMChannelDelegate ()
		{
			var del = WeakDelegate;
			if (del == null) {
				del = new _IOBluetoothRFCOMMChannelDelegate ();
				WeakDelegate = del;
			}
			if (!(del is _IOBluetoothRFCOMMChannelDelegate)) {
				throw new InvalidOperationException ("Cannot use this method when a user-defined delegate has been assigned.");
			}
			return (_IOBluetoothRFCOMMChannelDelegate) del;
		}

		public static event EventHandler<ChannelOpenedEventArgs> ChannelOpened;
				
		public event EventHandler<DataReceivedEventArgs> DataReceived {
			add { EnsureIOBluetoothRFCOMMChannelDelegate ().dataReceived += value; }
			remove { EnsureIOBluetoothRFCOMMChannelDelegate ().dataReceived -= value; }
		}

		public event EventHandler<MonoMac.IOKit.IOReturnEventArgs> Opened {
			add { EnsureIOBluetoothRFCOMMChannelDelegate ().opened += value; }
			remove { EnsureIOBluetoothRFCOMMChannelDelegate ().opened -= value; }
		}

		public event EventHandler Closed {
			add { EnsureIOBluetoothRFCOMMChannelDelegate ().closed += value; }
			remove { EnsureIOBluetoothRFCOMMChannelDelegate ().closed -= value; }
		}

		public event EventHandler ControlSignalsChanged {
			add { EnsureIOBluetoothRFCOMMChannelDelegate ().controlSignalsChanged += value; }
			remove { EnsureIOBluetoothRFCOMMChannelDelegate ().controlSignalsChanged -= value; }
		}

		public event EventHandler FlowControlChanged {
			add { EnsureIOBluetoothRFCOMMChannelDelegate ().flowControlChanged += value; }
			remove { EnsureIOBluetoothRFCOMMChannelDelegate ().flowControlChanged -= value; }
		}

		public event EventHandler QueueSpaceBecameAvailable {
			add { EnsureIOBluetoothRFCOMMChannelDelegate ().queueSpaceBecameAvailable += value; }
			remove { EnsureIOBluetoothRFCOMMChannelDelegate ().queueSpaceBecameAvailable -= value; }
		}

		static void HandleChannelOpened (IOBluetoothRFCOMMChannel newChannel)
		{
			if (ChannelOpened != null)
				ChannelOpened (null, new ChannelOpenedEventArgs (newChannel));
		}

		public class ChannelOpenedEventArgs : EventArgs
		{
			public ChannelOpenedEventArgs (IOBluetoothRFCOMMChannel channel)
			{
				Channel = channel;
			}

			public IOBluetoothRFCOMMChannel Channel { get; set; }
		}

		public class DataReceivedEventArgs : EventArgs
		{
			public DataReceivedEventArgs (byte[] data)
			{
				Data = data;
			}

			public byte[] Data { get; set; }
		}

		class NotificationHandler : NSObject
		{
			Action<IOBluetoothRFCOMMChannel> handleChannelOpened;

			public NotificationHandler (Action<IOBluetoothRFCOMMChannel> handleChannelOpened)
			{
				this.handleChannelOpened = handleChannelOpened;
			}

			[Export (channelOpenNotificationsSelector)]
			void OnChannelOpened (IOBluetoothUserNotification notification, IOBluetoothRFCOMMChannel newChannel)
			{
				handleChannelOpened (newChannel);
			}
		}

		#pragma warning disable 672
		[Register]
		sealed class _IOBluetoothRFCOMMChannelDelegate : MonoMac.IOBluetooth.IOBluetoothRFCOMMChannelDelegate { 
			public _IOBluetoothRFCOMMChannelDelegate () {}

			internal EventHandler<DataReceivedEventArgs> dataReceived;
			[Preserve (Conditional = true)]
			public override void DataReceived (MonoMac.IOBluetooth.IOBluetoothRFCOMMChannel rfcommChannel, IntPtr dataPointer, UIntPtr dataLength)
			{
				EventHandler<DataReceivedEventArgs> handler = dataReceived;
				if (handler != null) {
					byte[] data = new byte[(int)dataLength];
					Marshal.Copy (dataPointer, data, 0, data.Length);
					var args = new DataReceivedEventArgs (data);
					handler (rfcommChannel, args);
				}
			}

			internal EventHandler<MonoMac.IOKit.IOReturnEventArgs> opened;
			[Preserve (Conditional = true)]
			public override void Opened (MonoMac.IOBluetooth.IOBluetoothRFCOMMChannel rfcommChannel, MonoMac.IOKit.IOReturn error)
			{
				EventHandler<MonoMac.IOKit.IOReturnEventArgs> handler = opened;
				if (handler != null){
					var args = new MonoMac.IOKit.IOReturnEventArgs (error);
					handler (rfcommChannel, args);
				}
			}

			internal EventHandler closed;
			[Preserve (Conditional = true)]
			public override void Closed (MonoMac.IOBluetooth.IOBluetoothRFCOMMChannel rfcommChannel)
			{
				EventHandler handler = closed;
				if (handler != null){
					handler (rfcommChannel, EventArgs.Empty);
				}
			}

			internal EventHandler controlSignalsChanged;
			[Preserve (Conditional = true)]
			public override void ControlSignalsChanged (MonoMac.IOBluetooth.IOBluetoothRFCOMMChannel rfcommChannel)
			{
				EventHandler handler = controlSignalsChanged;
				if (handler != null){
					handler (rfcommChannel, EventArgs.Empty);
				}
			}

			internal EventHandler flowControlChanged;
			[Preserve (Conditional = true)]
			public override void FlowControlChanged (MonoMac.IOBluetooth.IOBluetoothRFCOMMChannel rfcommChannel)
			{
				EventHandler handler = flowControlChanged;
				if (handler != null){
					handler (rfcommChannel, EventArgs.Empty);
				}
			}

			[Preserve (Conditional = true)]
			public override void WriteCompleted (MonoMac.IOBluetooth.IOBluetoothRFCOMMChannel rfcommChannel, IntPtr refcon, MonoMac.IOKit.IOReturn error)
			{
				var completionSourceHandle = GCHandle.FromIntPtr (refcon);
				var completionSource = (TaskCompletionSource<IOBluetoothRFCOMMChannel>)completionSourceHandle.Target;
				completionSourceHandle.Free ();
				if (error == IOReturn.Success)
					completionSource.TrySetResult (rfcommChannel);
				else
					completionSource.TrySetException (new IOReturnException (error));
			}

			internal EventHandler queueSpaceBecameAvailable;
			[Preserve (Conditional = true)]
			public override void QueueSpaceBecameAvailable (MonoMac.IOBluetooth.IOBluetoothRFCOMMChannel rfcommChannel)
			{
				EventHandler handler = queueSpaceBecameAvailable;
				if (handler != null){
					handler (rfcommChannel, EventArgs.Empty);
				}
			}
		}
		#pragma warning restore 672
	}
}

