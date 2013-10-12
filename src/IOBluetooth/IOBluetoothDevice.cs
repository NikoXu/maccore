//
// IOBluetoothDevice.cs
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
using MonoMac.Foundation;
using MonoMac.IOKit;
using MonoMac.ObjCRuntime;
using System.Threading.Tasks;

namespace MonoMac.IOBluetooth
{
	public partial class IOBluetoothDevice
	{
		const string onConnectNotificationSelector = "onConnectNotification:device:";

		static ConnectNotificationObserver connectNotificationObserver;

		Lazy<DeviceAsyncCallbacks> ayncCallbacks =
			new Lazy<DeviceAsyncCallbacks> ();
		TaskCompletionSource<IOBluetoothDevice> connectionTaskCompletionSource;

		static IOBluetoothDevice ()
		{
			connectNotificationObserver = new ConnectNotificationObserver (OnDeviceConnected);
			registerForConnectNotifications (connectNotificationObserver, new Selector (onConnectNotificationSelector));
		}

		DeviceAsyncCallbacks AyncCallbacks {
			get { return ayncCallbacks.Value; }
		}

		[Since (2,5)]
		public IOBluetoothL2CAPChannel OpenL2CAPChannel
			(BluetoothL2CAPPSM psm = BluetoothL2CAPPSM.None,
			 NSObject channelDelegate = null)
		{
			IOBluetoothL2CAPChannel newChannel;
			var result = openL2CAPChannelSync (out newChannel, psm, channelDelegate);
			IOObject.ThrowIfError (result);
			newChannel.Release ();
			return newChannel;
		}

		[Since (2,5)]
		public IOBluetoothL2CAPChannel OpenL2CAPChannel
			(BluetoothL2CAPPSM psm,
			 IOBluetoothL2CAPChannelDelegate channelDelegate)
		{
			return OpenL2CAPChannel (psm, (NSObject)channelDelegate);
		}

		[Since (2,5)]
		public IOBluetoothL2CAPChannel OpenL2CAPChannelAsync
			(BluetoothL2CAPPSM psm = BluetoothL2CAPPSM.None,
			 NSObject channelDelegate = null)
		{
			IOBluetoothL2CAPChannel newChannel;
			var result = openL2CAPChannelAsync (out newChannel, psm, channelDelegate);
			IOObject.ThrowIfError (result);
			newChannel.Release ();
			return newChannel;
		}

		[Since (2,5)]
		public IOBluetoothL2CAPChannel OpenL2CAPChannelAsync
			(BluetoothL2CAPPSM psm,
			 IOBluetoothL2CAPChannelDelegate channelDelegate)
		{
			return OpenL2CAPChannelAsync (psm, (NSObject)channelDelegate);
		}

		public void SendL2CAPEchoRequest (byte[] data)
		{
			var gcHandle = GCHandle.Alloc (data, GCHandleType.Pinned);
			var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement (data, 0);
			var result = sendL2CAPEchoRequest (dataPtr, (ushort)data.Length);
			gcHandle.Free ();
			IOObject.ThrowIfError (result);
		}

		[Since (2,5)]
		public IOBluetoothRFCOMMChannel OpenRFCOMMChannel
			(byte channelID, NSObject channelDelegate = null)
		{
			IOBluetoothRFCOMMChannel rfcommChannel;
			var result = openRFCOMMChannelSync (out rfcommChannel, channelID, channelDelegate);
			IOObject.ThrowIfError (result);
			rfcommChannel.Release ();
			return rfcommChannel;
		}

		[Since (2,5)]
		public IOBluetoothRFCOMMChannel OpenRFCOMMChannel
			(byte channelID, IOBluetoothRFCOMMChannelDelegate channelDelegate)
		{
			return OpenRFCOMMChannel (channelID, channelDelegate);
		}

		[Since (2,5)]
		public Task<IOBluetoothRFCOMMChannel> OpenRFCOMMChannelAsync
			(byte channelID)
		{
			TaskCompletionSource<IOBluetoothRFCOMMChannel> taskCompletionSource =
				new TaskCompletionSource<IOBluetoothRFCOMMChannel> ();
			IOBluetoothRFCOMMChannel rfcommChannel;
			var result = openRFCOMMChannelAsync (out rfcommChannel, channelID, null);
			IOObject.ThrowIfError (result);
			EventHandler<IOReturnEventArgs> handler = null;
			handler = (sender, e) => {
				rfcommChannel.Opened -= handler;
				var channel = sender as IOBluetoothRFCOMMChannel;
				channel.Release ();
				if (e.Result == IOReturn.Success)
					taskCompletionSource.TrySetResult (channel);
				else
					taskCompletionSource.TrySetException (new IOReturnException (e.Result));
			};
			rfcommChannel.Opened += handler;
			return taskCompletionSource.Task;
		}

		[Since (2,5)]
		public IOBluetoothRFCOMMChannel OpenRFCOMMChannelAsync
			(byte channelID, NSObject channelDelegate)
		{
			IOBluetoothRFCOMMChannel rfcommChannel;
			var result = openRFCOMMChannelAsync (out rfcommChannel, channelID, channelDelegate);
			IOObject.ThrowIfError (result);
			rfcommChannel.Release ();
			return rfcommChannel;
		}

		[Since (2,5)]
		public IOBluetoothRFCOMMChannel OpenRFCOMMChannelAsync
			(byte channelID, IOBluetoothRFCOMMChannelDelegate channelDelegate)
		{
			return OpenRFCOMMChannelAsync (channelID, channelDelegate);
		}

		public BluetoothDeviceAddress Address {
			 get {
				var addressPtr = address;
				return (BluetoothDeviceAddress)Marshal.PtrToStructure (addressPtr, typeof(BluetoothDeviceAddress));
			}
		}

		public void OpenConnection ()
		{
			var result = openConnection ();
			IOObject.ThrowIfError (result);
		}

		public void OpenConnection (NSObject target)
		{
			var result = openConnection (target);
			IOObject.ThrowIfError (result);
		}

		public Task<IOBluetoothDevice> OpenConnectionAsync ()
		{
			if (connectionTaskCompletionSource != null)
				throw new Exception ("Connection is already in progress.");
			connectionTaskCompletionSource = new TaskCompletionSource<IOBluetoothDevice> ();
			try {
				OpenConnection (AyncCallbacks);
			} catch (Exception) {
				connectionTaskCompletionSource = null;
				throw;
			}
			return connectionTaskCompletionSource.Task;
		}

		[Since (2,7)]
		public void OpenConnection (NSObject target,
		                            ushort pageTimeoutValue,
		                            bool authenticationRequired)
		{
			var result = openConnection (target, pageTimeoutValue, authenticationRequired);
			IOObject.ThrowIfError (result);
		}

		public Task<IOBluetoothDevice> OpenConnectionAsync (ushort pageTimeoutValue,
		                                                    bool authenticationRequired)
		{
			if (connectionTaskCompletionSource != null)
				throw new Exception ("Connection is already in progress.");
			connectionTaskCompletionSource = new TaskCompletionSource<IOBluetoothDevice> ();
			try {
				OpenConnection (AyncCallbacks, pageTimeoutValue, authenticationRequired);
			} catch (Exception) {
				connectionTaskCompletionSource = null;
				throw;
			}
			return connectionTaskCompletionSource.Task;
		}

		public void CloseConnection ()
		{
			var result = closeConnection ();
			IOObject.ThrowIfError (result);
		}

		public void RequestRemoteName (NSObject target)
		{
			var result = remoteNameRequest (target);
			IOObject.ThrowIfError (result);
		}

		[Since (2,7)]
		public void RequestRemoteName (NSObject target, ushort pageTimeoutValue)
		{
			var result = remoteNameRequest (target, pageTimeoutValue);
			IOObject.ThrowIfError (result);
		}

		public void RequestAuthentication ()
		{
			var result = requestAuthentication ();
			IOObject.ThrowIfError (result);
		}

		public void PerformSDPQuery (NSObject target)
		{
			var result = performSDPQuery (target);
			IOObject.ThrowIfError (result);
		}

		[Since (7,0)]
		public void PerformSDPQuery (NSObject target, IOBluetoothSDPUUID[] uuids)
		{
			var result = performSDPQuery (target, uuids);
			IOObject.ThrowIfError (result);
		}

		[Since (6,0)]
		public void SetSupervisionTimeout (UInt16 timeout)
		{
			var result = setSupervisionTimeout (timeout);
			IOObject.ThrowIfError (result);
		}

		[Since (2,4)]
		public void AddToFavorites ()
		{
			var result = addToFavorites ();
			IOObject.ThrowIfError (result);
		}

		[Since (2,4)]
		public void RemoveFromFavorites ()
		{
			var result = removeFromFavorites ();
			IOObject.ThrowIfError (result);
		}

		[Since (6,0)]
		public IOBluetoothL2CAPChannel OpenL2CAPChannel
			(BluetoothL2CAPPSM psm,
			 NSDictionary channelConfiguration,
			 NSObject channelDelegate = null)
		{
			IOBluetoothL2CAPChannel newChannel;
			var result = openL2CAPChannelSync (out newChannel, psm, channelConfiguration, channelDelegate);
			IOObject.ThrowIfError (result);
			return newChannel;
		}

		[Since (6,0)]
		public IOBluetoothL2CAPChannel OpenL2CAPChannel
			(BluetoothL2CAPPSM psm,
			 NSDictionary channelConfiguration,
			 IOBluetoothL2CAPChannelDelegate channelDelegate)
		{
			return OpenL2CAPChannel (psm, channelConfiguration, (NSObject)channelDelegate);
		}

		[Since (6,0)]
		public IOBluetoothL2CAPChannel OpenL2CAPChannelAsync
			(BluetoothL2CAPPSM psm,
			 NSDictionary channelConfiguration,
			 NSObject channelDelegate = null)
		{
			IOBluetoothL2CAPChannel newChannel;
			var result = openL2CAPChannelAsync (out newChannel, psm, channelConfiguration, channelDelegate);
			IOObject.ThrowIfError (result);
			return newChannel;
		}

		[Since (6,0)]
		public IOBluetoothL2CAPChannel OpenL2CAPChannelAsync
			(BluetoothL2CAPPSM psm,
			 NSDictionary channelConfiguration,
			 IOBluetoothL2CAPChannelDelegate channelDelegate)
		{
			return OpenL2CAPChannelAsync (psm, channelConfiguration, (NSObject)channelDelegate);
		}

		public static event EventHandler<DeviceConnectedEventArgs> DeviceConnected;

		static void OnDeviceConnected (IOBluetoothDevice device)
		{
			if (DeviceConnected != null)
				DeviceConnected (null, new DeviceConnectedEventArgs (device));
		}

		class ConnectNotificationObserver : NSObject
		{
			Action<IOBluetoothDevice> onDeviceConnected;

			public ConnectNotificationObserver (Action<IOBluetoothDevice> onDeviceConnected)
			{
				this.onDeviceConnected = onDeviceConnected;
			}

			[Export (onConnectNotificationSelector)]
			void OnConnectNotification (IOBluetoothUserNotification notification, IOBluetoothDevice device)
			{
				onDeviceConnected (device);
			}
		}

		class DeviceAsyncCallbacks : IOBluetoothDeviceAsyncCallbacks
		{
			public override void ConnectionComplete (IOBluetoothDevice device, IOReturn status)
			{
				// TODO: log errors here
				if (device == null || device.connectionTaskCompletionSource == null)
					return;
				if (status == IOReturn.Success)
					device.connectionTaskCompletionSource.TrySetResult (device);
				else
					device.connectionTaskCompletionSource.TrySetException (new IOReturnException (status));
				device.connectionTaskCompletionSource = null;
			}
		}
	}

	public sealed class DeviceConnectedEventArgs : EventArgs
	{
		public DeviceConnectedEventArgs (IOBluetoothDevice device)
		{
			Device = device;
		}

		public IOBluetoothDevice Device { get; private set; }
	}
}

