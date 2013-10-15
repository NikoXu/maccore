//
// IOUSBInterface.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MonoMac.CoreFoundation;
using MonoMac.Kernel.Mach;
using MonoMac.ObjCRuntime;

using AbsoluteTime = System.UInt64;
using CFRunLoopSourceRef = System.IntPtr;
using ICFRunLoopSourceRef = System.IntPtr;
using IOByteCount = System.UInt32;
using UInt8 = System.Byte;
using io_service_t = System.IntPtr;
using mach_port_t = System.IntPtr;
using System.Threading.Tasks;

namespace MonoMac.IOKit.USB
{
	public class IOUSBInterface : IOUSBNub
	{
		IIOCFPlugin<IOUSBInterfaceInterface> @interface;
		Lazy<PipeCollection> pipes;

		internal IOUSBInterface (IntPtr handle, bool owns) : base (handle, owns)
		{
			using (var pluginInterface = IOCFPlugin.CreateInterfaceForService<IOUSBInterfaceUserClientType> (this)) {

				var bundleVersion = IOUSB.BundleVersion;
				if (bundleVersion >= new Version ("5.5.0"))
					@interface = pluginInterface.QueryInterface<IOUSBInterfaceInterface550> ();
				else if (bundleVersion >= new Version ("5.0.0"))
					@interface = pluginInterface.QueryInterface<IOUSBInterfaceInterface500> ();
				else if (bundleVersion >= new Version ("3.0.0"))
					@interface = pluginInterface.QueryInterface<IOUSBInterfaceInterface300> ();
				else if (bundleVersion >= new Version ("2.4.5"))
					@interface = pluginInterface.QueryInterface<IOUSBInterfaceInterface245> ();
				else if (bundleVersion >= new Version ("2.2.0"))
					@interface = pluginInterface.QueryInterface<IOUSBInterfaceInterface220> ();
				else if (bundleVersion >= new Version ("1.9.7"))
					@interface = pluginInterface.QueryInterface<IOUSBInterfaceInterface197> ();
				else if (bundleVersion >= new Version ("1.9.2"))
					@interface = pluginInterface.QueryInterface<IOUSBInterfaceInterface192> ();
				else if (bundleVersion >= new Version ("1.9.0"))
					@interface = pluginInterface.QueryInterface<IOUSBInterfaceInterface190> ();
				else if (bundleVersion >= new Version ("1.8.3"))
					@interface = pluginInterface.QueryInterface<IOUSBInterfaceInterface183> ();
				else if (bundleVersion >= new Version ("1.8.2"))
					@interface = pluginInterface.QueryInterface<IOUSBInterfaceInterface182> ();
				else
					@interface = pluginInterface.QueryInterface<IOUSBInterfaceInterface> ();
			}
			pipes = new Lazy<PipeCollection>(() => new PipeCollection (this));
		}

		~IOUSBInterface ()
		{
			Dispose (false);
		}

		protected override void Dispose (bool disposing)
		{
			if (pipes.IsValueCreated) {
				foreach (var pipe in pipes.Value)
					pipe.Dispose ();
				pipes = null;
			}
			@interface = null;
			base.Dispose (disposing);
		}


		IntPtr InterfaceRef {
			get { return @interface.Handle; }
		}

		IOUSBInterfaceInterface Interface {
			get { return @interface.Interface; }
		}

		public CFRunLoopSource AsyncEventSource {
			get {
				ThrowIfDisposed ();
				IntPtr runLoopSourceRef = Interface.GetInterfaceAsyncEventSource (InterfaceRef);
				if (runLoopSourceRef == IntPtr.Zero) {
					var result = Interface.CreateInterfaceAsyncEventSource (InterfaceRef, out runLoopSourceRef);
					IOObject.ThrowIfError (result);
					return new CFRunLoopSource (runLoopSourceRef, true);
				}
				return CFType.GetCFObject<CFRunLoopSource> (runLoopSourceRef);
			}
		}

		public InterfaceClass Class {
			get {
				ThrowIfDisposed ();
				byte @class;
				var result = Interface.GetInterfaceClass (InterfaceRef, out @class);
				IOObject.ThrowIfError (result);
				return (InterfaceClass)@class;
			}
		}

		public InterfaceSubClass SubClass {
			get {
				ThrowIfDisposed ();
				byte subClass;
				var result = Interface.GetInterfaceSubClass (InterfaceRef, out subClass);
				IOObject.ThrowIfError (result);
				return (InterfaceSubClass)subClass;
			}
		}

		public InterfaceProtocol Protocol {
			get {
				ThrowIfDisposed ();
				byte protocol;
				var result = Interface.GetInterfaceProtocol (InterfaceRef, out protocol);
				IOObject.ThrowIfError (result);
				return (InterfaceProtocol)protocol;
			}
		}

		public ushort VendorId {
			get {
				ThrowIfDisposed ();
				ushort vendor;
				var result = Interface.GetDeviceVendor (InterfaceRef, out vendor);
				IOObject.ThrowIfError (result);
				return vendor;
			}
		}

		public ushort ProductId {
			get {
				ThrowIfDisposed ();
				ushort product;
				var result = Interface.GetDeviceProduct (InterfaceRef, out product);
				IOObject.ThrowIfError (result);
				return product;
			}
		}

		public ushort ReleaseNumber {
			get {
				ThrowIfDisposed ();
				ushort releaseNumber;
				var result = Interface.GetDeviceReleaseNumber (InterfaceRef, out releaseNumber);
				IOObject.ThrowIfError (result);
				return releaseNumber;
			}
		}

		public int ConfigurationValue {
			get {
				ThrowIfDisposed ();
				byte value;
				var result = Interface.GetConfigurationValue (InterfaceRef, out value);
				IOObject.ThrowIfError (result);
				return (int)value;
			}
		}

		public int Index {
			get {
				ThrowIfDisposed ();
				byte number;
				var result = Interface.GetInterfaceNumber (InterfaceRef, out number);
				IOObject.ThrowIfError (result);
				return (int)number;
			}
		}

		public int AlternateSetting {
			get {
				ThrowIfDisposed ();
				byte setting;
				var result = Interface.GetAlternateSetting (InterfaceRef, out setting);
				IOObject.ThrowIfError (result);
				return (int)setting;
			}
			set {
				ThrowIfDisposed ();
				var result = Interface.SetAlternateInterface (InterfaceRef, (byte)value);
				IOObject.ThrowIfError (result);
			}
		}

		public int EndpointCount {
			get {
				ThrowIfDisposed ();
				byte count;
				var result = Interface.GetNumEndpoints (InterfaceRef, out count);
				IOObject.ThrowIfError (result);
				return (int)count;
			}
		}

		public uint LocationID {
			get {
				ThrowIfDisposed ();
				uint locationId;
				var result = Interface.GetLocationID (InterfaceRef, out locationId);
				IOObject.ThrowIfError (result);
				return locationId;
			}
		}

		public IOUSBDevice Device {
			get {
				ThrowIfDisposed ();
				IntPtr deviceRef;
				var result = Interface.GetDevice (InterfaceRef, out deviceRef);
				IOObject.ThrowIfError (result);
				return new IOUSBDevice (deviceRef, true);
			}
		}

		[Since (0,4)]
		public string Name {
			get {
				ThrowIfDisposed ();
				byte index;
				var result = Interface.USBInterfaceGetStringIndex (InterfaceRef, out index);
				IOObject.ThrowIfError (result);
				return Device.GetStringDescriptor (index);
			}
		}

		[Since (2,0)]
		public uint BandwidthAvailible {
			get {
				ThrowIfDisposed ();
				uint bandwidth;
				var result = Interface.GetBandwidthAvailable (InterfaceRef, out bandwidth);
				IOObject.ThrowIfError (result);
				return bandwidth;
			}
		}

		[Since (2,5)]
		public uint FrameListTime {
			get {
				ThrowIfDisposed ();
				uint time;
				var result = Interface.GetFrameListTime (InterfaceRef, out time);
				IOObject.ThrowIfError (result);
				return time;
			}
		}

		[Since (2,5)]
		public NumVersion IOUSBLibVersion {
			get  {
				ThrowIfDisposed ();
				NumVersion ioUSBLibVersion, usbFamilyVersion;
				var result = Interface.GetIOUSBLibVersion (InterfaceRef, out ioUSBLibVersion, out usbFamilyVersion);
				IOObject.ThrowIfError (result);
				return ioUSBLibVersion;
			}
		}

		[Since (2,5)]
		public NumVersion IOUSBFamilyVersion {
			get  {
				ThrowIfDisposed ();
				NumVersion ioUSBLibVersion, usbFamilyVersion;
				var result = Interface.GetIOUSBLibVersion (InterfaceRef, out ioUSBLibVersion, out usbFamilyVersion);
				IOObject.ThrowIfError (result);
				return usbFamilyVersion;
			}
		}

		public void Open ()
		{
			ThrowIfDisposed ();
			var result = Interface.USBInterfaceOpen (InterfaceRef);
			IOObject.ThrowIfError (result);
		}

		public void Close ()
		{
			ThrowIfDisposed ();
			var result = Interface.USBInterfaceClose (InterfaceRef);
			IOObject.ThrowIfError (result);
		}

		public ulong GetBusFrameNumber (out ulong atTime) {
			ThrowIfDisposed ();
			ulong frame;
			var result = Interface.GetBusFrameNumber (InterfaceRef, out frame, out atTime);
			IOObject.ThrowIfError (result);
			return (ulong)IOUSB.USBToHostOrder ((long)frame);
		}

		[Since (1,0)]
		public void OpenSeize ()
		{
			ThrowIfDisposed ();
			var result = Interface.USBInterfaceOpenSeize (InterfaceRef);
			IOObject.ThrowIfError (result);
		}

		[Since (2,0)]
		public EndpointProperties GetEndpointProperties (byte alternateSetting, byte endpoint, EndpointDirection direction)
		{
			ThrowIfDisposed ();
			byte transferType;
			ushort maxPacketSize;
			byte interval;
			var result = Interface.GetEndpointProperties (InterfaceRef, alternateSetting, endpoint,
			                                              (byte)direction, out transferType,
			                                              out maxPacketSize, out interval);
			IOObject.ThrowIfError (result);
			return new EndpointProperties () {
				TransferType = (EndpointTransferType) transferType,
				MaxPacketSize = maxPacketSize,
				Interval = (Interval)interval
			};
		}

		[Since (2,5)]
		public ulong GetBusMicroFrameNumber (out ulong atTime)
		{
			ThrowIfDisposed ();
			ulong microFrame;
			var result = Interface.GetBusMicroFrameNumber (InterfaceRef, out microFrame, out atTime);
			IOObject.ThrowIfError (result);
			return microFrame;
		}

		[Since (4,0)]
		public IEnumerable<IIOUSBDescriptor> FindAssociatedDescriptors (DescriptorType type = DescriptorType.Any)
		{
			var descriptor = FindNextAssociatedDescriptor (null, type);
			if (descriptor != null) {
				yield return descriptor;
				while ((descriptor = FindNextAssociatedDescriptor (descriptor, type)) != null)
					yield return descriptor;
			}
		}

		IIOUSBDescriptor FindNextAssociatedDescriptor (IIOUSBDescriptor current, DescriptorType type = DescriptorType.Any)
		{
			ThrowIfDisposed ();
			IntPtr currentRef;
			if (current == null)
				currentRef = IntPtr.Zero;
			else {
				currentRef = Marshal.AllocHGlobal (Marshal.SizeOf (current));
				Marshal.StructureToPtr (current, currentRef, false);
			}
			var result = Interface.FindNextAssociatedDescriptor (InterfaceRef, currentRef, (byte)type);
			if (currentRef != IntPtr.Zero)
				Marshal.FreeHGlobal (currentRef);
			if (result == IntPtr.Zero)
				return null;
			var header = (IOUSBDescriptorHeader)Marshal.PtrToStructure (result, typeof(IOUSBDescriptorHeader));
			var descriptorType = header.DescriptorType.GetClassType ();
			return (IIOUSBDescriptor)Marshal.PtrToStructure (result, descriptorType);
		}

		[Since (4,0)]
		public IEnumerable<IIOUSBDescriptor> FindAlternateInterfaces (IOUSBFindInterfaceRequest request)
		{
			var descriptor = FindNextAlternateInterface (null, request);
			if (descriptor != null) {
				yield return descriptor;
				while ((descriptor = FindNextAlternateInterface (descriptor, request)) != null)
					yield return descriptor;
			}
		}

		IIOUSBDescriptor FindNextAlternateInterface (IIOUSBDescriptor current, IOUSBFindInterfaceRequest request)
		{
			ThrowIfDisposed ();
			IntPtr currentRef;
			if (current == null)
				currentRef = IntPtr.Zero;
			else {
				currentRef = Marshal.AllocHGlobal (Marshal.SizeOf (current));
				Marshal.StructureToPtr (current, currentRef, false);
			}
			var result = Interface.FindNextAltInterface (InterfaceRef, currentRef, request);
			if (currentRef != IntPtr.Zero)
				Marshal.FreeHGlobal (currentRef);
			if (result == IntPtr.Zero)
				return null;
			var header = (IOUSBDescriptorHeader)Marshal.PtrToStructure (result, typeof(IOUSBDescriptorHeader));
			var descriptorType = header.DescriptorType.GetClassType ();
			return (IIOUSBDescriptor)Marshal.PtrToStructure (result, descriptorType);
		}

		[Since (5,0)]
		public ulong GetBusFrameNumberWithTime (out ulong atTime)
		{
			ThrowIfDisposed ();
			ulong frame;
			var result = Interface.GetBusFrameNumberWithTime (InterfaceRef, out frame, out atTime);
			IOObject.ThrowIfError (result);
			return frame;
		}

		[Since (8,0)]
		public IOUSBEndpointProperties GetEndpointPropertiesV3 (byte alternateSetting, EndpointDirection diretion, byte endpoint) {
			ThrowIfDisposed ();
			IOUSBEndpointProperties properties = new IOUSBEndpointProperties {
				Version = EndpointPropertiesVersion.V3,
				AlternateSetting = alternateSetting,
				Direction = diretion,
				EndpointNumber = endpoint
			};
			var result = Interface.GetEndpointPropertiesV3 (InterfaceRef, ref properties);
			IOObject.ThrowIfError (result);
			return properties;
		}

		public PipeCollection Pipes { get { return pipes.Value; } }

		public class Pipe : IDisposable
		{
			IOUSBInterface instance;
			byte pipeIndex;
			Lazy<StreamCollection> streams;

			internal Pipe (IOUSBInterface instance, byte pipeIndex)
			{
				this.instance  = instance;
				this.pipeIndex = pipeIndex;
				streams = new Lazy<StreamCollection> (() => new StreamCollection (this));
			}

			~Pipe ()
			{
				Dispose (false);
			}
			
			void SendControlRequest (ref IOUSBDeviceRequest request)
			{
				ThrowIfDisposed ();
				var result = instance.Interface.ControlRequest
					(instance.InterfaceRef, pipeIndex, ref request);
				IOObject.ThrowIfError (result);
			}

			public Task<int> SendControlRequestAsync (IOUSBDeviceRequest request)
			{
				ThrowIfDisposed ();
				var completionSource = new TaskCompletionSource<int> ();
				GCHandle callbackHandle = new GCHandle ();
				IOAsyncCallback1 callback = (refCon, callbackResult, arg0) => {
					callbackHandle.Free ();
					if (callbackResult == IOReturn.Success)
					completionSource.TrySetResult ((int)arg0);
					else
					completionSource.TrySetException (new IOReturnException (callbackResult));
				};
				callbackHandle = GCHandle.Alloc (callback, GCHandleType.Pinned);
				var result = instance.Interface.ControlRequestAsync
					(instance.InterfaceRef, pipeIndex,
					 request, callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
				return completionSource.Task;
			}

			public PipeProperties Properties {
				get {
					ThrowIfDisposed ();
					byte direction, number, transferType, interval;
					ushort maxPacketSize;
					var result = instance.Interface.GetPipeProperties
						(instance.InterfaceRef, pipeIndex,
						 out direction, out number, out transferType,
						 out maxPacketSize, out interval);
					IOObject.ThrowIfError (result);
					return new PipeProperties () {
						Direction = (EndpointDirection)direction,
						Number = number,
						TransferType = (EndpointTransferType)transferType,
						MaxPacketSize = maxPacketSize,
						Interval = (Interval)interval
					};
				}
			}

			public byte Status {
				get {
					ThrowIfDisposed ();
					var result = instance.Interface.GetPipeStatus
						(instance.InterfaceRef, pipeIndex);
					// TODO: create enum for common return values.
					IOObject.ThrowIfError (result);
					return 0;
				}
			}

			public void Abort ()
			{
				ThrowIfDisposed ();
				var result = instance.Interface.AbortPipe
					(instance.InterfaceRef, pipeIndex);
				IOObject.ThrowIfError (result);
			}

			public void Reset ()
			{
				ThrowIfDisposed ();
				var result = instance.Interface.ResetPipe
					(instance.InterfaceRef, pipeIndex);
				IOObject.ThrowIfError (result);
			}

			public void ClearStall ()
			{
				ThrowIfDisposed ();
				var result = instance.Interface.ClearPipeStall
					(instance.InterfaceRef, pipeIndex);
				IOObject.ThrowIfError (result);
			}

			public byte[] Read (uint byteCount)
			{
				ThrowIfDisposed ();
				var buffer = new byte[byteCount];
				var result = instance.Interface.ReadPipe
					(instance.InterfaceRef, pipeIndex, buffer, ref byteCount);
				IOObject.ThrowIfError (result);
				return buffer;
			}

			public void Write (byte[] bytes)
			{
				ThrowIfDisposed ();
				var result = instance.Interface.WritePipe
					(instance.InterfaceRef, pipeIndex, bytes, (uint)bytes.Length);
				IOObject.ThrowIfError (result);
			}

			public Task<byte[]> ReadAsync (int byteCount)
			{
				ThrowIfDisposed ();
				var buffer = new byte[byteCount];
				var bufferHandle = GCHandle.Alloc (buffer, GCHandleType.Pinned);
				var completionSource = new TaskCompletionSource<byte[]> ();
				GCHandle callbackHandle = new GCHandle ();
				IOAsyncCallback1 callback = (refCon, callbackResult, arg0) => {
					bufferHandle.Free ();
					callbackHandle.Free ();
					if (callbackResult == IOReturn.Success) {
						Array.Resize<byte> (ref buffer, (int)arg0);
						completionSource.TrySetResult (buffer);
					} else
						completionSource.TrySetException (new IOReturnException (callbackResult));
				};
				callbackHandle = GCHandle.Alloc (callback, GCHandleType.Pinned);
				var result = instance.Interface.ReadPipeAsync
					(instance.InterfaceRef, pipeIndex, buffer,
					 (uint)byteCount, callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
				return completionSource.Task;
			}

			public Task<int> WriteAsync (byte[] bytes)
			{
				ThrowIfDisposed ();
				var completionSource = new TaskCompletionSource<int> ();
				GCHandle callbackHandle = new GCHandle ();
				IOAsyncCallback1 callback = (refCon, callbackResult, arg0) => {
					callbackHandle.Free ();
					if (callbackResult == IOReturn.Success)
					completionSource.TrySetResult ((int)arg0);
					else
					completionSource.TrySetException (new IOReturnException (callbackResult));
				};
				callbackHandle = GCHandle.Alloc (callback, GCHandleType.Pinned);
				var result = instance.Interface.WritePipeAsync
					(instance.InterfaceRef, pipeIndex, bytes,
					 (uint)bytes.Length,callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
				return completionSource.Task;
			}
			
			[Since (0,4)]
			void SendControlRequest (ref IOUSBDevRequestTO request)
			{
				ThrowIfDisposed ();
				var result = instance.Interface.ControlRequestTO
					(instance.InterfaceRef, pipeIndex, ref request);
				IOObject.ThrowIfError (result);
			}

			[Since (0,4)]
			public Task<int> SendControlRequestAsync (IOUSBDevRequestTO request)
			{
				ThrowIfDisposed ();
				var completionSource = new TaskCompletionSource<int> ();
				GCHandle callbackHandle = new GCHandle ();
				IOAsyncCallback1 callback = (refCon, callbackResult, arg0) => {
					callbackHandle.Free ();
					if (callbackResult == IOReturn.Success)
					completionSource.TrySetResult ((int)arg0);
					else
					completionSource.TrySetException (new IOReturnException (callbackResult));
				};
				callbackHandle = GCHandle.Alloc (callback, GCHandleType.Pinned);
				var result = instance.Interface.ControlRequestAsyncTO
					(instance.InterfaceRef, pipeIndex,
					 request, callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
				return completionSource.Task;
			}
			
			[Since (0,4)]
			public byte[] Read (uint byteCount, uint noDataTimeout, uint completionTimeout)
			{
				ThrowIfDisposed ();
				var buffer = new byte[byteCount];
				var result = instance.Interface.ReadPipeTO
					(instance.InterfaceRef, pipeIndex, buffer,
					 ref byteCount, noDataTimeout, completionTimeout);
				IOObject.ThrowIfError (result);
				return buffer;
			}
			
			[Since (0,4)]
			public void Write (byte[] bytes, uint noDataTimeout, uint completionTimeout)
			{
				ThrowIfDisposed ();
				var result = instance.Interface.WritePipeTO
					(instance.InterfaceRef, pipeIndex, bytes,
					 (uint)bytes.Length, noDataTimeout, completionTimeout);
				IOObject.ThrowIfError (result);
			}
			
			[Since (0,4)]
			public Task<byte[]> ReadAsync (uint byteCount, uint noDataTimeout, uint completionTimeout)
			{
				ThrowIfDisposed ();
				var buffer = new byte[byteCount];
				var bufferHandle = GCHandle.Alloc (buffer, GCHandleType.Pinned);
				var completionSource = new TaskCompletionSource<byte[]> ();
				GCHandle callbackHandle = new GCHandle ();
				IOAsyncCallback1 callback = (refCon, callbackResult, arg0) => {
					bufferHandle.Free ();
					callbackHandle.Free ();
					if (callbackResult == IOReturn.Success) {
						Array.Resize<byte> (ref buffer, (int)arg0);
						completionSource.TrySetResult (buffer);
					} else
						completionSource.TrySetException (new IOReturnException (callbackResult));
				};
				callbackHandle = GCHandle.Alloc (callback, GCHandleType.Pinned);
				var result = instance.Interface.ReadPipeAsyncTO
					(instance.InterfaceRef, pipeIndex, buffer,
					 byteCount, noDataTimeout,
					 completionTimeout, callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
				return completionSource.Task;
			}

			[Since (0,4)]
			public Task<int> WriteAsync (byte[] bytes, uint noDataTimeout, uint completionTimeout)
			{
				ThrowIfDisposed ();
				var completionSource = new TaskCompletionSource<int> ();
				GCHandle callbackHandle = new GCHandle ();
				IOAsyncCallback1 callback = (refCon, callbackResult, arg0) => {
					callbackHandle.Free ();
					if (callbackResult == IOReturn.Success)
					completionSource.TrySetResult ((int)arg0);
					else
					completionSource.TrySetException (new IOReturnException (callbackResult));
				};
				callbackHandle = GCHandle.Alloc (callback, GCHandleType.Pinned);
				var result = instance.Interface.WritePipeAsyncTO
					(instance.InterfaceRef, pipeIndex, bytes,
					 noDataTimeout, completionTimeout,
					 (uint)bytes.Length,callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
				return completionSource.Task;
			}

			[Since (2,0)]
			public void ClearStallBothEnds ()
			{
				ThrowIfDisposed ();
				var result = instance.Interface.ClearPipeStallBothEnds
					(instance.InterfaceRef, pipeIndex);
				IOObject.ThrowIfError (result);
			}

			[Since (2,0)]
			public void SetPipePolicy (ushort maxPacketSize, Interval maxInterval)
			{
				ThrowIfDisposed ();
				var result = instance.Interface.SetPipePolicy
					(instance.InterfaceRef, pipeIndex, maxPacketSize, (byte)maxInterval);
				IOObject.ThrowIfError (result);
			}

			[Since (7,3)]
			[Obsolete ("Use PropertiesV3")]
			public PipePropertiesV2 PropertiesV2 {
				get {
					ThrowIfDisposed ();
					byte direction, number, transferType, interval, maxBurst, mult;
					ushort maxPacketSize, bytesPerInterval;
					var result = instance.Interface.GetPipePropertiesV2
						(instance.InterfaceRef, pipeIndex,
						 out direction, out number, out transferType,
						 out maxPacketSize, out interval,
						 out maxBurst, out mult, out bytesPerInterval);
					IOObject.ThrowIfError (result);
					return new PipePropertiesV2 () {
						Direction = (EndpointDirection)direction,
						Number = number,
						TransferType = (EndpointTransferType)transferType,
						MaxPacketSize = maxPacketSize,
						Interval = (Interval)interval,
						MaxBurst = maxBurst,
						Mult = mult,
						BytesPerInterval = bytesPerInterval
					};
				}
			}

			[Since (8,0)]
			public IOUSBEndpointProperties PropertiesV3 {
				get {
					ThrowIfDisposed ();
					IOUSBEndpointProperties properties = new IOUSBEndpointProperties () {
						Version = EndpointPropertiesVersion.V3
					};
					var result = instance.Interface.GetPipePropertiesV3
						(instance.InterfaceRef, pipeIndex, ref properties);
					IOObject.ThrowIfError (result);
					return properties;
				}
			}

			[Since (8,0)]
			public uint SupportedStreamCount {
				get {
					ThrowIfDisposed ();
					uint count;
					var result = instance.Interface.SupportsStreams
						(instance.InterfaceRef, pipeIndex, out count);
					IOObject.ThrowIfError (result);
					return count;
				}
			}

			[Since (8,0)]
			public void CreateStreams (uint count) {
				ThrowIfDisposed ();
				if (count == 0)
					throw new ArgumentOutOfRangeException ("count");
				var result = instance.Interface.CreateStreams
					(instance.InterfaceRef, pipeIndex, count);
				IOObject.ThrowIfError (result);
			}

			[Since (8,0)]
			public void DisposeStreams () {
				ThrowIfDisposed ();
				var result = instance.Interface.CreateStreams
					(instance.InterfaceRef, pipeIndex, 0);
				IOObject.ThrowIfError (result);
				if (streams.IsValueCreated)
					foreach (var stream in Streams)
						stream.Dispose ();
			}

			[Since (8,0)]
			public StreamCollection Streams { get { return streams.Value; } }

			public class StreamCollection : IEnumerable<Stream>
			{
				Pipe instance;
				Dictionary<int, Stream> streams;

				internal StreamCollection (Pipe instance)
				{
					this.instance = instance;
					streams = new Dictionary<int, Stream> ();
				}

				public Stream this[int id] {
					get {
						instance.ThrowIfDisposed ();
						if (id < 1 || id > Count)
							throw new ArgumentOutOfRangeException ("id");
						if (!streams.ContainsKey (id))
							streams.Add (id, new Stream (instance, (uint)id));
						return streams [id];
					}
				}

				public int Count {
					get {
						instance.ThrowIfDisposed ();
						uint count;
						var result = instance.instance.Interface.GetConfiguredStreams
							(instance.instance.InterfaceRef, instance.pipeIndex, out count);
						IOObject.ThrowIfError (result);
						return (int)count;
					}
				}

				#region IEnumerable implementation

				public IEnumerator<Stream> GetEnumerator ()
				{
					return new StreamEnumerator (this);
				}

				#endregion

				#region IEnumerable implementation

				System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
				{
					return GetEnumerator ();
				}

				#endregion

				class StreamEnumerator : IEnumerator<Stream>
				{
					StreamCollection instance;
					int position;

					public StreamEnumerator (StreamCollection instance)
					{
						this.instance = instance;
						Reset ();
					}

					#region IEnumerator implementation

					public bool MoveNext ()
					{
						position++;
						return position <= instance.Count;
					}

					public void Reset ()
					{
						position = 0;
					}

					object System.Collections.IEnumerator.Current {
						get { return  Current; }
					}

					#endregion

					#region IDisposable implementation

					public void Dispose ()
					{
						instance = null;
					}

					#endregion

					#region IEnumerator implementation

					public Pipe.Stream Current {
						get { return instance [position]; }
					}

					#endregion
				}
			}

			protected void ThrowIfDisposed ()
			{
				if (instance == null || instance.InterfaceRef == IntPtr.Zero)
					throw new ObjectDisposedException (GetType ().Name);
			}

			public void Dispose ()
			{
				Dispose (true);
				GC.SuppressFinalize (this);
			}

			protected void Dispose (bool disposing)
			{
				if (streams.IsValueCreated) {
					if (Streams.Count > 0) {
						try {
							DisposeStreams ();
						} catch (Exception) {
							// no throwing exceptions in finalizer!
						}
					}
				}
				if (disposing) {
					instance = null;
					streams = null;
				}
			}

			public class Stream : System.IO.Stream, IDisposable
			{
				Pipe instance;
				uint id;
				List<AsyncCallback> callbacks;

				internal Stream (Pipe instance, uint id)
				{
					this.instance = instance;
					this.id = id;
					callbacks = new List<AsyncCallback> ();
				}

				~Stream ()
				{
					Dispose (false);
				}

				#region implemented abstract members of Stream

				public override void Flush ()
				{
				}

				public override int Read (byte[] buffer, int offset, int count)
				{
					ThrowIfDisposed ();
					byte[] buffer2;
					if (offset == 0)
						buffer2 = buffer;
					else {
						buffer2 = new byte[count];
						Array.Copy (buffer, offset, buffer2, 0, count);
					}
					uint size = (uint)count;
					var result = instance.instance.Interface.ReadStreamsPipeTO
						(instance.instance.InterfaceRef, instance.pipeIndex, id, buffer2, ref size,
						 (uint)this.WriteTimeout, (uint)this.WriteTimeout);
					IOObject.ThrowIfError (result);
					return (int)size;
				}

				public override long Seek (long offset, System.IO.SeekOrigin origin)
				{
					throw new NotSupportedException ();
				}

				public override void SetLength (long value)
				{
					throw new NotSupportedException ();
				}

				public override void Write (byte[] buffer, int offset, int count)
				{
					ThrowIfDisposed ();
					byte[] buffer2;
					if (offset == 0)
						buffer2 = buffer;
					else {
						buffer2 = new byte[count];
						Array.Copy (buffer, offset, buffer2, 0, count);
					}
					var result = instance.instance.Interface.WriteStreamsPipeTO
						(instance.instance.InterfaceRef, instance.pipeIndex, id,
						 buffer2, (uint)count, (uint)this.WriteTimeout,
						 (uint)this.WriteTimeout);
					IOObject.ThrowIfError (result);
				}

				public override bool CanRead {
					get {
						return true;
					}
				}

				public override bool CanSeek {
					get {
						return false;
					}
				}

				public override bool CanTimeout {
					get {
						return true;
					}
				}

				public override bool CanWrite {
					get {
						return true;
					}
				}

				public override long Length {
					get {
						ThrowIfDisposed ();
						throw new NotSupportedException ();
					}
				}

				public override long Position {
					get {
						ThrowIfDisposed ();
						throw new NotSupportedException ();
					}
					set {
						ThrowIfDisposed ();
						throw new NotSupportedException ();
					}
				}

				#endregion

				protected void ThrowIfDisposed ()
				{
					if (instance == null || instance.instance == null ||
					    instance.instance.InterfaceRef == IntPtr.Zero)
						throw new ObjectDisposedException (GetType ().Name);
				}

				public void Dispose ()
				{
					Dispose (true);
					GC.SuppressFinalize (this);
				}

				protected void Dispose (bool disposing)
				{
					if (disposing) {
						instance = null;
						callbacks = null;
					}
				}
			}
		}

		public class PipeCollection : IEnumerable<Pipe>
		{
			IOUSBInterface instance;
			Dictionary<int, Pipe> pipes;

			internal PipeCollection (IOUSBInterface instance)
			{
				this.instance = instance;
				pipes = new Dictionary<int, Pipe> ();
			}

			public Pipe this[int index] {
				get {
					instance.ThrowIfDisposed ();
					if (index < 1 || index > instance.EndpointCount)
						throw new ArgumentException ("Index must be between 1 and EndpointCount", "index");
					if (!pipes.ContainsKey (index))
						pipes.Add (index, new Pipe (instance, (byte)index));
					return pipes [index];
				}
			}

			public int Count { get { return instance.EndpointCount - 1; } }

			#region IEnumerable implementation

			public IEnumerator<Pipe> GetEnumerator ()
			{
				return new PipeEnumerator (this);
			}

			#endregion

			#region IEnumerable implementation

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
			{
				return GetEnumerator ();
			}

			#endregion

			class PipeEnumerator : IEnumerator<Pipe>
			{
				PipeCollection instance;
				int position;

				internal PipeEnumerator (PipeCollection instance)
				{
					this.instance = instance;
					Reset ();
				}

				#region IEnumerator implementation

				public bool MoveNext ()
				{
					position++;
					return position <= instance.instance.EndpointCount;
				}

				public void Reset ()
				{
					position = 0;
				}

				object System.Collections.IEnumerator.Current {
					get { return Current; }
				}

				#endregion

				#region IDisposable implementation

				public void Dispose ()
				{
					instance = null;
				}

				#endregion

				#region IEnumerator implementation

				public Pipe Current {
					get { return instance [position]; }
				}

				#endregion
			}
		}

	}
	
	public struct PipeProperties
	{
		public EndpointDirection Direction;
		public UInt8 Number;
		public EndpointTransferType TransferType;
		public UInt16 MaxPacketSize;
		public Interval Interval;
	}

	public struct PipePropertiesV2
	{
		public EndpointDirection Direction;
		public UInt8 Number;
		public EndpointTransferType TransferType;
		public UInt16 MaxPacketSize;
		public Interval Interval;
		public UInt8 MaxBurst;
		public UInt8 Mult;
		public UInt16 BytesPerInterval;
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBIsocFrame
	{
		public IOReturn Status;
		public UInt16   RequestedCount;
		public UInt16   ActualCount;
	} 

	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBLowLatencyIsocFrame
	{
		public IOReturn Status;
		public UInt16 RequestedCount;
		public UInt16 ActualCount;
		public AbsoluteTime TimeStamp;
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBEndpointProperties
	{
		public EndpointPropertiesVersion Version;
		public UInt8 AlternateSetting;
		public EndpointDirection Direction;
		public UInt8 EndpointNumber;
		public EndpointTransferType TransferType;
		public UInt8 UsageType;
		public IsocSyncType SyncType;
		public Interval Interval;
		public UInt16 MaxPacketSize;
		public UInt8 MaxBurst;
		public UInt8 MaxStreams;
		public UInt8 Mult;
		public UInt16 BytesPerInterval;
	}

	public struct EndpointProperties
	{
		public EndpointTransferType TransferType;
		public ushort MaxPacketSize;
		public Interval Interval;
	}

	[Guid ("2d9786c6-9ef3-11d4-ad51-000a27052861")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceUserClientType : IOCFPlugInInterface
	{
	}

	[Guid ("73c97ae8-9ef3-11d4-b1d0-000a27052861")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface : IUnknown
	{
		_CreateInterfaceAsyncEventSource createInterfaceAsyncEventSource;
		_GetInterfaceAsyncEventSource getInterfaceAsyncEventSource;
		_CreateInterfaceAsyncPort createInterfaceAsyncPort;
		_GetInterfaceAsyncPort getInterfaceAsyncPort;
		_USBInterfaceOpen usbInterfaceOpen;
		_USBInterfaceClose usbInterfaceClose;
		_GetInterfaceClass getInterfaceClass;
		_GetInterfaceSubClass getInterfaceSubClass;
		_GetInterfaceProtocol getInterfaceProtocol;
		_GetDeviceVendor getDeviceVendor;
		_GetDeviceProduct getDeviceProduct;
		_GetDeviceReleaseNumber getDeviceReleaseNumber;
		_GetConfigurationValue getConfigurationValue;
		_GetInterfaceNumber getInterfaceNumber;
		_GetAlternateSetting getAlternateSetting;
		_GetNumEndpoints getNumEndpoints;
		_GetLocationID getLocationID;
		_GetDevice getDevice;
		_SetAlternateInterface setAlternateInterface;
		_GetBusFrameNumber getBusFrameNumber;
		_ControlRequest controlRequest;
		_ControlRequestAsync controlRequestAsync;
		_GetPipeProperties getPipeProperties;
		_GetPipeStatus getPipeStatus;
		_AbortPipe abortPipe;
		_ResetPipe resetPipe;
		_ClearPipeStall clearPipeStall;
		_ReadPipe readPipe;
		_WritePipe writePipe;
		_ReadPipeAsync readPipeAsync;
		_WritePipeAsync writePipeAsync;
		_ReadIsochPipeAsync readIsochPipeAsync;
		_WriteIsochPipeAsync writeIsochPipeAsync;

		public _CreateInterfaceAsyncEventSource CreateInterfaceAsyncEventSource {
			get { return createInterfaceAsyncEventSource; }
		}

		public _GetInterfaceAsyncEventSource GetInterfaceAsyncEventSource {
			get { return getInterfaceAsyncEventSource; }
		}

		public _CreateInterfaceAsyncPort CreateInterfaceAsyncPort  {
			get { return createInterfaceAsyncPort; }
		}

		public _GetInterfaceAsyncPort GetInterfaceAsyncPort {
			get { return getInterfaceAsyncPort; }
		}

		public _USBInterfaceOpen USBInterfaceOpen {
			get { return usbInterfaceOpen; }
		}

		public _USBInterfaceClose USBInterfaceClose {
			get { return usbInterfaceClose; }
		}

		public _GetInterfaceClass GetInterfaceClass {
			get { return getInterfaceClass; }
		}

		public _GetInterfaceSubClass GetInterfaceSubClass{
			get { return getInterfaceSubClass; }
		}

		public _GetInterfaceProtocol GetInterfaceProtocol {
			get { return getInterfaceProtocol; }
		}

		public _GetDeviceVendor GetDeviceVendor {
			get { return getDeviceVendor; }
		}

		public _GetDeviceProduct GetDeviceProduct {
			get { return getDeviceProduct; }
		}

		public _GetDeviceReleaseNumber GetDeviceReleaseNumber {
			get { return getDeviceReleaseNumber; }
		}

		public _GetConfigurationValue GetConfigurationValue {
			get { return getConfigurationValue; }
		}

		public _GetInterfaceNumber GetInterfaceNumber {
			get { return getInterfaceNumber; }
		}

		public _GetAlternateSetting GetAlternateSetting {
			get { return getAlternateSetting; }
		}

		public _GetNumEndpoints GetNumEndpoints {
			get { return getNumEndpoints; }
		}

		public _GetLocationID GetLocationID {
			get { return getLocationID; }
		}

		public _GetDevice GetDevice {
			get { return getDevice; }
		}

		public _SetAlternateInterface SetAlternateInterface {
			get { return setAlternateInterface; }
		}

		public _GetBusFrameNumber GetBusFrameNumber {
			get { return getBusFrameNumber; }
		}

		public _ControlRequest ControlRequest {
			get { return controlRequest; }
		}

		public _ControlRequestAsync ControlRequestAsync {
			get { return controlRequestAsync; }
		}

		public _GetPipeProperties GetPipeProperties {
			get { return getPipeProperties; }
		}

		public _GetPipeStatus GetPipeStatus {
			get { return getPipeStatus; }
		}

		public _AbortPipe AbortPipe {
			get { return abortPipe; }
		}

		public _ResetPipe ResetPipe {
			get { return resetPipe; }
		}

		public _ClearPipeStall ClearPipeStall {
			get { return clearPipeStall; }
		}

		public _ReadPipe ReadPipe {
			get { return readPipe; }
		}

		public _WritePipe WritePipe {
			get { return writePipe; }
		}

		public _ReadPipeAsync ReadPipeAsync {
			get { return readPipeAsync; }
		}

		public _WritePipeAsync WritePipeAsync {
			get { return writePipeAsync; }
		}

		public _ReadIsochPipeAsync ReadIsochPipeAsync {
			get { return readIsochPipeAsync; }
		}

		public _WriteIsochPipeAsync WriteIsochPipeAsync {
			get { return writeIsochPipeAsync; }
		}

		public virtual _ControlRequestTO ControlRequestTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _ControlRequestAsyncTO ControlRequestAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _ReadPipeTO ReadPipeTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _WritePipeTO WritePipeTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _ReadPipeAsyncTO ReadPipeAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _WritePipeAsyncTO WritePipeAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _USBInterfaceGetStringIndex USBInterfaceGetStringIndex {
			get { throw new NotImplementedException (); }
		}

		public virtual _USBInterfaceOpenSeize USBInterfaceOpenSeize {
			get { throw new NotImplementedException (); }
		}

		public virtual _ClearPipeStallBothEnds ClearPipeStallBothEnds {
			get { throw new NotImplementedException (); }
		}

		public virtual _SetPipePolicy SetPipePolicy {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetBandwidthAvailable GetBandwidthAvailable {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetEndpointProperties GetEndpointProperties {
			get { throw new NotImplementedException (); }
		}

		public virtual _LowLatencyReadIsochPipeAsync LowLatencyReadIsochPipeAsync {
			get { throw new NotImplementedException (); }
		}

		public virtual _LowLatencyWriteIsochPipeAsync LowLatencyWriteIsochPipeAsync {
			get { throw new NotImplementedException (); }
		}

		public virtual _LowLatencyCreateBuffer LowLatencyCreateBuffer {
			get { throw new NotImplementedException (); }
		}

		public virtual _LowLatencyDestroyBuffer LowLatencyDestroyBuffer {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetBusMicroFrameNumber GetBusMicroFrameNumber {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetFrameListTime GetFrameListTime {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetIOUSBLibVersion GetIOUSBLibVersion {
			get { throw new NotImplementedException (); }
		}

		public virtual _FindNextAssociatedDescriptor FindNextAssociatedDescriptor {
			get { throw new NotImplementedException (); }
		}

		public virtual _FindNextAltInterface FindNextAltInterface {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetBusFrameNumberWithTime GetBusFrameNumberWithTime {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetPipePropertiesV2 GetPipePropertiesV2 {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetPipePropertiesV3 GetPipePropertiesV3 {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetEndpointPropertiesV3 GetEndpointPropertiesV3 {
			get { throw new NotImplementedException (); }
		}

		public virtual _SupportsStreams SupportsStreams {
			get { throw new NotImplementedException (); }
		}

		public virtual _CreateStreams CreateStreams {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetConfiguredStreams GetConfiguredStreams {
			get { throw new NotImplementedException (); }
		}

		public virtual _ReadStreamsPipeTO ReadStreamsPipeTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _WriteStreamsPipeTO WriteStreamsPipeTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _ReadStreamsPipeAsyncTO ReadStreamsPipeAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _WriteStreamsPipeAsyncTO WriteStreamsPipeAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _AbortStreamsPipe AbortStreamsPipe {
			get { throw new NotImplementedException (); }
		}

		public delegate IOReturn _CreateInterfaceAsyncEventSource (IntPtr self, out CFRunLoopSourceRef source);
		public delegate ICFRunLoopSourceRef _GetInterfaceAsyncEventSource (IntPtr self);
		public delegate IOReturn _CreateInterfaceAsyncPort (IntPtr self, out mach_port_t port);
		public delegate mach_port_t _GetInterfaceAsyncPort (IntPtr self);
		public delegate IOReturn _USBInterfaceOpen (IntPtr self);
		public delegate IOReturn _USBInterfaceClose (IntPtr self);
		public delegate IOReturn _GetInterfaceClass (IntPtr self, out UInt8 intfClass);
		public delegate IOReturn _GetInterfaceSubClass (IntPtr self, out UInt8 intfSubClass);
		public delegate IOReturn _GetInterfaceProtocol (IntPtr self, out UInt8 intfProtocol);
		public delegate IOReturn _GetDeviceVendor (IntPtr self, out UInt16 devVendor);
		public delegate IOReturn _GetDeviceProduct (IntPtr self, out UInt16 devProduct);
		public delegate IOReturn _GetDeviceReleaseNumber (IntPtr self, out UInt16 devRelNum);
		public delegate IOReturn _GetConfigurationValue (IntPtr self, out UInt8 configVal);
		public delegate IOReturn _GetInterfaceNumber (IntPtr self, out UInt8 intfNumber);
		public delegate IOReturn _GetAlternateSetting (IntPtr self, out UInt8 intfAltSetting);
		public delegate IOReturn _GetNumEndpoints (IntPtr self, out UInt8 intfNumEndpoints);
		public delegate IOReturn _GetLocationID (IntPtr self, out UInt32 locationID);
		public delegate IOReturn _GetDevice (IntPtr self, out io_service_t device);
		public delegate IOReturn _SetAlternateInterface (IntPtr self, UInt8 alternateSetting);
		public delegate IOReturn _GetBusFrameNumber (IntPtr self, out UInt64 frame, out AbsoluteTime atTime);
		public delegate IOReturn _ControlRequest (IntPtr self, UInt8 pipeRef, ref IOUSBDeviceRequest req);
		public delegate IOReturn _ControlRequestAsync (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPStruct)] IOUSBDeviceRequest req, IOAsyncCallback1 callback, IntPtr refCon);
		public delegate IOReturn _GetPipeProperties (IntPtr self, UInt8 pipeRef, out UInt8 direction, out UInt8 number, out UInt8 transferType, out UInt16 maxPacketSize, out UInt8 interval);
		public delegate IOReturn _GetPipeStatus (IntPtr self, UInt8 pipeRef);
		public delegate IOReturn _AbortPipe (IntPtr self, UInt8 pipeRef);
		public delegate IOReturn _ResetPipe (IntPtr self, UInt8 pipeRef);
		public delegate IOReturn _ClearPipeStall (IntPtr self, UInt8 pipeRef);
		public delegate IOReturn _ReadPipe (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, ref UInt32 size);
		public delegate IOReturn _WritePipe (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size);
		public delegate IOReturn _ReadPipeAsync (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size, IOAsyncCallback1 callback, IntPtr refcon);
		public delegate IOReturn _WritePipeAsync (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size, IOAsyncCallback1 callback, IntPtr refcon);
		public delegate IOReturn _ReadIsochPipeAsync (IntPtr self, UInt8 pipeRef, IntPtr buf, UInt64 frameStart, UInt32 numFrames, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] IOUSBIsocFrame[] frameList, IOAsyncCallback1 callback, IntPtr refcon);
		public delegate IOReturn _WriteIsochPipeAsync (IntPtr self, UInt8 pipeRef, IntPtr buf, UInt64 frameStart, UInt32 numFrames, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] IOUSBIsocFrame[] frameList, IOAsyncCallback1 callback, IntPtr refcon);
		public delegate IOReturn _ControlRequestTO (IntPtr self, UInt8 pipeRef, ref IOUSBDevRequestTO req);
		public delegate IOReturn _ControlRequestAsyncTO (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPStruct)] IOUSBDevRequestTO req, IOAsyncCallback1 callback, IntPtr refCon);
		public delegate IOReturn _ReadPipeTO (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, ref UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout);
		public delegate IOReturn _WritePipeTO (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout);
		public delegate IOReturn _ReadPipeAsyncTO (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout, IOAsyncCallback1 callback, IntPtr refcon);
		public delegate IOReturn _WritePipeAsyncTO (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout, IOAsyncCallback1 callback, IntPtr refcon);
		public delegate IOReturn _USBInterfaceGetStringIndex (IntPtr self, out UInt8 si);
		public delegate IOReturn _USBInterfaceOpenSeize (IntPtr self);
		public delegate IOReturn _ClearPipeStallBothEnds (IntPtr self, UInt8 pipeRef);
		public delegate IOReturn _SetPipePolicy (IntPtr self, UInt8 pipeRef, UInt16 maxPacketSize, UInt8 maxInterval);
		public delegate IOReturn _GetBandwidthAvailable (IntPtr self, out UInt32 bandwidth);
		public delegate IOReturn _GetEndpointProperties (IntPtr self, UInt8 alternateSetting, UInt8 endpointNumber, UInt8 direction, out UInt8 transferType, out UInt16 maxPacketSize, out UInt8 interval);
		public delegate IOReturn _LowLatencyReadIsochPipeAsync (IntPtr self, UInt8 pipeRef, IntPtr buf, UInt64 frameStart, UInt32 numFrames, UInt32 updateFrequency, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] IOUSBLowLatencyIsocFrame[] frameList, IOAsyncCallback1 callback, IntPtr refcon);
		public delegate IOReturn _LowLatencyWriteIsochPipeAsync (IntPtr self, UInt8 pipeRef, IntPtr buf, UInt64 frameStart, UInt32 numFrames, UInt32 updateFrequency, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] IOUSBLowLatencyIsocFrame[] frameList, IOAsyncCallback1 callback, IntPtr refcon);
		public delegate IOReturn _LowLatencyCreateBuffer (IntPtr self, out IntPtr buffer, IOByteCount size, UInt32 bufferType);
		public delegate IOReturn _LowLatencyDestroyBuffer (IntPtr self, IntPtr buffer);
		public delegate IOReturn _GetBusMicroFrameNumber (IntPtr self, out UInt64 microFrame, out AbsoluteTime atTime);
		public delegate IOReturn _GetFrameListTime (IntPtr self, out UInt32 microsecondsInFrame);
		public delegate IOReturn _GetIOUSBLibVersion (IntPtr self, out NumVersion ioUSBLibVersion, out NumVersion usbFamilyVersion);
		public delegate /*IOUSBDescriptorHeader*/ IntPtr _FindNextAssociatedDescriptor (IntPtr self, [In]IntPtr currentDescriptor, UInt8 descriptorType);
		public delegate /*IOUSBDescriptorHeader*/ IntPtr _FindNextAltInterface (IntPtr self, [In]IntPtr current, [MarshalAs (UnmanagedType.LPStruct)] IOUSBFindInterfaceRequest request);
		public delegate IOReturn _GetBusFrameNumberWithTime (IntPtr self, out UInt64 frame, out AbsoluteTime atTime);
		public delegate IOReturn _GetPipePropertiesV2 (IntPtr self, UInt8 pipeRef, out UInt8 direction, out UInt8 number, out UInt8 transferType, out UInt16 maxPacketSize, out UInt8 interval, out UInt8 maxBurst, out UInt8 mult, out UInt16 bytesPerInterval);
		public delegate IOReturn _GetPipePropertiesV3 (IntPtr self, UInt8 pipeRef, ref IOUSBEndpointProperties properties);
		public delegate IOReturn _GetEndpointPropertiesV3 (IntPtr self, ref IOUSBEndpointProperties properties);
		public delegate IOReturn _SupportsStreams (IntPtr self, UInt8 pipeRef, out UInt32 supportsStreams);
		public delegate IOReturn _CreateStreams (IntPtr self, UInt8 pipeRef, UInt32 streamID);
		public delegate IOReturn _GetConfiguredStreams (IntPtr self, UInt8 pipeRef, out UInt32 configuredStreams);
		public delegate IOReturn _ReadStreamsPipeTO (IntPtr self, UInt8 pipeRef, UInt32 streamID, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] buf, ref UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout);
		public delegate IOReturn _WriteStreamsPipeTO (IntPtr self, UInt8 pipeRef, UInt32 streamID, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] buf, UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout);
		public delegate IOReturn _ReadStreamsPipeAsyncTO (IntPtr self, UInt8 pipeRef, UInt32 streamID, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] buf, UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout, IOAsyncCallback1 callback, IntPtr refcon);
		public delegate IOReturn _WriteStreamsPipeAsyncTO (IntPtr self, UInt8 pipeRef, UInt32 streamID, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] buf, UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout, IOAsyncCallback1 callback, IntPtr refcon);
		public delegate IOReturn _AbortStreamsPipe (IntPtr self, UInt8 pipeRef, UInt32 streamID);
	}

	[Guid ("4923AC4C-4896-11D5-9208-000A27801E86")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface182 : IOUSBInterfaceInterface
	{
		_ControlRequestTO controlRequestTO;
		_ControlRequestAsyncTO controlRequestAsyncTO;
		_ReadPipeTO readPipeTO;
		_WritePipeTO writePipeTO;
		_ReadPipeAsyncTO readPipeAsyncTO;
		_WritePipeAsyncTO writePipeAsyncTO;
		_USBInterfaceGetStringIndex usbInterfaceGetStringIndex;

		public override _ControlRequestTO ControlRequestTO {
			get { return controlRequestTO; }
		}

		public override _ControlRequestAsyncTO ControlRequestAsyncTO {
			get { return controlRequestAsyncTO; }
		}

		public override _ReadPipeTO ReadPipeTO {
			get { return readPipeTO; }
		}

		public override _WritePipeTO WritePipeTO {
			get { return writePipeTO; }
		}

		public override _ReadPipeAsyncTO ReadPipeAsyncTO {
			get { return readPipeAsyncTO; }
		}

		public override _WritePipeAsyncTO WritePipeAsyncTO {
			get { return writePipeAsyncTO; }
		}

		public override _USBInterfaceGetStringIndex USBInterfaceGetStringIndex {
			get { return usbInterfaceGetStringIndex; }
		}
	}

	[Guid ("1C438356-74C4-11D5-92E6-000A27801E86")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface183 : IOUSBInterfaceInterface182
	{
		_USBInterfaceOpenSeize usbInterfaceOpenSeize;

		public override _USBInterfaceOpenSeize USBInterfaceOpenSeize {
			get { return usbInterfaceOpenSeize; }
		}
	}

	[Guid ("8FDB8455-74A6-11D6-97B1-003065D3608E")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface190 : IOUSBInterfaceInterface183
	{
		_ClearPipeStallBothEnds clearPipeStallBothEnds;
		_SetPipePolicy setPipePolicy;
		_GetBandwidthAvailable getBandwidthAvailable;
		_GetEndpointProperties getEndpointProperties;

		public override _ClearPipeStallBothEnds ClearPipeStallBothEnds {
			get { return clearPipeStallBothEnds; }
		}

		public override _SetPipePolicy SetPipePolicy {
			get { return setPipePolicy; }
		}

		public override _GetBandwidthAvailable GetBandwidthAvailable {
			get { return getBandwidthAvailable; }
		}

		public override _GetEndpointProperties GetEndpointProperties {
			get { return getEndpointProperties; }
		}
	}

	[Guid ("6C798A6E-D6E9-11D6-ADD6-0003933E3E3E")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface192 : IOUSBInterfaceInterface190
	{
		_LowLatencyReadIsochPipeAsync lowLatencyReadIsochPipeAsync;
		_LowLatencyWriteIsochPipeAsync lowLatencyWriteIsochPipeAsync;
		_LowLatencyCreateBuffer lowLatencyCreateBuffer;
		_LowLatencyDestroyBuffer lowLatencyDestroyBuffer;

		public override _LowLatencyReadIsochPipeAsync LowLatencyReadIsochPipeAsync {
			get { return lowLatencyReadIsochPipeAsync; }
		}

		public override _LowLatencyWriteIsochPipeAsync LowLatencyWriteIsochPipeAsync {
			get { return lowLatencyWriteIsochPipeAsync; }
		}

		public override _LowLatencyCreateBuffer LowLatencyCreateBuffer {
			get { return lowLatencyCreateBuffer; }
		}

		public override _LowLatencyDestroyBuffer LowLatencyDestroyBuffer {
			get { return lowLatencyDestroyBuffer; }
		}
	}

	[Guid ("C63D3C92-0884-11D7-9692-0003933E3E3E")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface197 : IOUSBInterfaceInterface192
	{
		_GetBusMicroFrameNumber getBusMicroFrameNumber;
		_GetFrameListTime getFrameListTime;
		_GetIOUSBLibVersion getIOUSBLibVersion;

		public override _GetBusMicroFrameNumber GetBusMicroFrameNumber {
			get { return getBusMicroFrameNumber; }
		}

		public override _GetFrameListTime GetFrameListTime {
			get { return getFrameListTime; }
		}

		public override _GetIOUSBLibVersion GetIOUSBLibVersion {
			get { return getIOUSBLibVersion; }
		}
	}

	[Guid ("770DE60C-2FE8-11D8-A582-000393DCB1D0")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface220 : IOUSBInterfaceInterface197
	{
		_FindNextAssociatedDescriptor findNextAssociatedDescriptor;
		_FindNextAltInterface findNextAltInterface;

		public override _FindNextAssociatedDescriptor FindNextAssociatedDescriptor {
			get { return findNextAssociatedDescriptor; }
		}

		public override _FindNextAltInterface FindNextAltInterface {
			get { return findNextAltInterface; }
		}
	}

	[Guid ("64BABDD2-0F6B-4B4F-8E3E-DC36046987AD")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface245 : IOUSBInterfaceInterface220
	{
	}

	[Guid ("BCEAADDC-884D-4F27-8340-36D69FAB90F6")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface300 : IOUSBInterfaceInterface245
	{
		_GetBusFrameNumberWithTime getBusFrameNumberWithTime;

		public override _GetBusFrameNumberWithTime GetBusFrameNumberWithTime {
			get { return getBusFrameNumberWithTime; }
		}
	}

	[Guid ("6C0D38C3-B093-4EA7-809B-09FB5DDDAC16")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface500 : IOUSBInterfaceInterface300
	{
		_GetPipePropertiesV2 getPipePropertiesV2;

		public override _GetPipePropertiesV2 GetPipePropertiesV2 {
			get { return getPipePropertiesV2; }
		}
	}

	[Guid ("6AE44D3F-EB45-487F-8E8E-B93B99F8EA9E")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface550 : IOUSBInterfaceInterface500
	{
		_GetPipePropertiesV3 getPipePropertiesV3;
		_GetEndpointPropertiesV3 getEndpointPropertiesV3;
		_SupportsStreams supportsStreams;
		_CreateStreams createStreams;
		_GetConfiguredStreams getConfiguredStreams;
		_ReadStreamsPipeTO readStreamsPipeTO;
		_WriteStreamsPipeTO writeStreamsPipeTO;
		_ReadStreamsPipeAsyncTO readStreamsPipeAsyncTO;
		_WriteStreamsPipeAsyncTO writeStreamsPipeAsyncTO;
		_AbortStreamsPipe abortStreamsPipe;

		public override _GetPipePropertiesV3 GetPipePropertiesV3 {
			get { return getPipePropertiesV3; }
		}

		public override _GetEndpointPropertiesV3 GetEndpointPropertiesV3 {
			get { return getEndpointPropertiesV3; }
		}

		public override _SupportsStreams SupportsStreams {
			get { return supportsStreams; }
		}

		public override _CreateStreams CreateStreams {
			get { return createStreams; }
		}

		public override _GetConfiguredStreams GetConfiguredStreams {
			get { return getConfiguredStreams; }
		}

		public override _ReadStreamsPipeTO ReadStreamsPipeTO {
			get { return readStreamsPipeTO; }
		}

		public override _WriteStreamsPipeTO WriteStreamsPipeTO {
			get { return writeStreamsPipeTO; }
		}

		public override _ReadStreamsPipeAsyncTO ReadStreamsPipeAsyncTO {
			get { return readStreamsPipeAsyncTO; }
		}

		public override _WriteStreamsPipeAsyncTO WriteStreamsPipeAsyncTO {
			get { return writeStreamsPipeAsyncTO; }
		}

		public override _AbortStreamsPipe AbortStreamsPipe {
			get { return abortStreamsPipe; }
		}
	}
}

