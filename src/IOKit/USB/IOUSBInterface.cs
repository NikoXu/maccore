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

namespace MonoMac.IOKit.USB
{
	public class IOUSBInterface : IOUSBNub
	{
		Lazy<IOCFPlugin<IOUSBInterfaceUserClientType>> pluginInterface;
		Lazy<IIOCFPlugin<IOUSBInterfaceInterface>> interfaceInterface;

		Lazy<CFRunLoopSource> interfaceAsyncEventSource;
		Lazy<Port> interfaceAsyncPort;
		Dictionary<int, Pipe> pipes;

		internal IOUSBInterface (IntPtr handle, bool owns) : base (handle, owns)
		{
			pluginInterface = new Lazy<IOCFPlugin<IOUSBInterfaceUserClientType>>
				(() => IOCFPlugin.CreateInterfaceForService<IOUSBInterfaceUserClientType> (this));

			var bundleVersion = IOUSB.BundleVersion;
			if (bundleVersion >= new Version ("5.5.0"))
				interfaceInterface = new Lazy<IIOCFPlugin<IOUSBInterfaceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBInterfaceInterface550> ());
			else if (bundleVersion >= new Version ("5.0.0"))
				interfaceInterface = new Lazy<IIOCFPlugin<IOUSBInterfaceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBInterfaceInterface500> ());
			else if (bundleVersion >= new Version ("3.0.0"))
				interfaceInterface = new Lazy<IIOCFPlugin<IOUSBInterfaceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBInterfaceInterface300> ());
			else if (bundleVersion >= new Version ("2.4.5"))
				interfaceInterface = new Lazy<IIOCFPlugin<IOUSBInterfaceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBInterfaceInterface245> ());
			else if (bundleVersion >= new Version ("2.2.0"))
				interfaceInterface = new Lazy<IIOCFPlugin<IOUSBInterfaceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBInterfaceInterface220> ());
			else if (bundleVersion >= new Version ("1.9.7"))
				interfaceInterface = new Lazy<IIOCFPlugin<IOUSBInterfaceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBInterfaceInterface197> ());
			else if (bundleVersion >= new Version ("1.9.2"))
				interfaceInterface = new Lazy<IIOCFPlugin<IOUSBInterfaceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBInterfaceInterface192> ());
			else if (bundleVersion >= new Version ("1.9.0"))
				interfaceInterface = new Lazy<IIOCFPlugin<IOUSBInterfaceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBInterfaceInterface190> ());
			else if (bundleVersion >= new Version ("1.8.3"))
				interfaceInterface = new Lazy<IIOCFPlugin<IOUSBInterfaceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBInterfaceInterface183> ());
			else if (bundleVersion >= new Version ("1.8.2"))
				interfaceInterface = new Lazy<IIOCFPlugin<IOUSBInterfaceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBInterfaceInterface182> ());
			else
				interfaceInterface = new Lazy<IIOCFPlugin<IOUSBInterfaceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBInterfaceInterface> ());

			interfaceAsyncEventSource = new Lazy<CFRunLoopSource> (CreateAsyncEventSource);
			interfaceAsyncPort = new Lazy<Port> (CreateAsyncPort);
			pipes = new Dictionary<int, Pipe> ();
		}

		IntPtr InterfaceInterfaceRef {
			get { return interfaceInterface.Value.Handle; }
		}

		IOUSBInterfaceInterface InterfaceInterface {
			get { return interfaceInterface.Value.Interface; }
		}

		public CFRunLoopSource AsyncEventSource {
			get { return interfaceAsyncEventSource.Value; }
		}

		public Port AsyncPort {
			get { return interfaceAsyncPort.Value; }
		}

		public InterfaceClass Class {
			get {
				ThrowIfDisposed ();
				byte @class;
				var result = InterfaceInterface.GetInterfaceClass (InterfaceInterfaceRef, out @class);
				IOObject.ThrowIfError (result);
				return (InterfaceClass)@class;
			}
		}

		public InterfaceSubClass SubClass {
			get {
				ThrowIfDisposed ();
				byte subClass;
				var result = InterfaceInterface.GetInterfaceSubClass (InterfaceInterfaceRef, out subClass);
				IOObject.ThrowIfError (result);
				return (InterfaceSubClass)subClass;
			}
		}

		public InterfaceProtocol Protocol {
			get {
				ThrowIfDisposed ();
				byte protocol;
				var result = InterfaceInterface.GetInterfaceProtocol (InterfaceInterfaceRef, out protocol);
				IOObject.ThrowIfError (result);
				return (InterfaceProtocol)protocol;
			}
		}

		public ushort VendorId {
			get {
				ThrowIfDisposed ();
				ushort vendor;
				var result = InterfaceInterface.GetDeviceVendor (InterfaceInterfaceRef, out vendor);
				IOObject.ThrowIfError (result);
				return vendor;
			}
		}

		public ushort ProductId {
			get {
				ThrowIfDisposed ();
				ushort product;
				var result = InterfaceInterface.GetDeviceProduct (InterfaceInterfaceRef, out product);
				IOObject.ThrowIfError (result);
				return product;
			}
		}

		public ushort ReleaseNumber {
			get {
				ThrowIfDisposed ();
				ushort releaseNumber;
				var result = InterfaceInterface.GetDeviceReleaseNumber (InterfaceInterfaceRef, out releaseNumber);
				IOObject.ThrowIfError (result);
				return releaseNumber;
			}
		}

		public int ConfigurationValue {
			get {
				ThrowIfDisposed ();
				byte value;
				var result = InterfaceInterface.GetConfigurationValue (InterfaceInterfaceRef, out value);
				IOObject.ThrowIfError (result);
				return (int)value;
			}
		}

		public int InterfaceNumber {
			get {
				ThrowIfDisposed ();
				byte number;
				var result = InterfaceInterface.GetInterfaceNumber (InterfaceInterfaceRef, out number);
				IOObject.ThrowIfError (result);
				return (int)number;
			}
		}

		public int AlternateSetting {
			get {
				ThrowIfDisposed ();
				byte setting;
				var result = InterfaceInterface.GetAlternateSetting (InterfaceInterfaceRef, out setting);
				IOObject.ThrowIfError (result);
				return (int)setting;
			}
			set {
				ThrowIfDisposed ();
				var result = InterfaceInterface.SetAlternateInterface (InterfaceInterfaceRef, (byte)value);
				IOObject.ThrowIfError (result);
			}
		}

		public int EndpointCount {
			get {
				ThrowIfDisposed ();
				byte count;
				var result = InterfaceInterface.GetNumEndpoints (InterfaceInterfaceRef, out count);
				IOObject.ThrowIfError (result);
				return (int)count;
			}
		}

		public uint LocationID {
			get {
				ThrowIfDisposed ();
				uint locationId;
				var result = InterfaceInterface.GetLocationID (InterfaceInterfaceRef, out locationId);
				IOObject.ThrowIfError (result);
				return locationId;
			}
		}

		public IOUSBDevice Device {
			get {
				ThrowIfDisposed ();
				IntPtr deviceRef;
				var result = InterfaceInterface.GetDevice (InterfaceInterfaceRef, out deviceRef);
				IOObject.ThrowIfError (result);
				return new IOUSBDevice (deviceRef, true);
			}
		}

		public Pipe DefaultPipe {
			get { return this [0]; }
		}

		[IndexerName ("Pipes")]
		public Pipe this[int index] {
			get {
				ThrowIfDisposed ();
				if (index < 0 || index > EndpointCount)
					throw new ArgumentException ("Index must be between 0 and EndpointCount", "index");
				if (!pipes.ContainsKey (index))
					pipes.Add (index, new Pipe (InterfaceInterface, InterfaceInterfaceRef, (byte)index));
				return pipes [index];
			}
		}
		
		[Since (2,0)]
		public uint BandwidthAvailible {
			get {
				ThrowIfDisposed ();
				uint bandwidth;
				var result = InterfaceInterface.GetBandwidthAvailable (InterfaceInterfaceRef, out bandwidth);
				IOObject.ThrowIfError (result);
				return bandwidth;
			}
		}

		[Since (2,5)]
		public uint FrameListTime {
			get {
				ThrowIfDisposed ();
				uint time;
				var result = InterfaceInterface.GetFrameListTime (InterfaceInterfaceRef, out time);
				IOObject.ThrowIfError (result);
				return time;
			}
		}

		[Since (2,5)]
		public NumVersion IOUSBLibVersion {
			get  {
				ThrowIfDisposed ();
				NumVersion ioUSBLibVersion, usbFamilyVersion;
				var result = InterfaceInterface.GetIOUSBLibVersion (InterfaceInterfaceRef, out ioUSBLibVersion, out usbFamilyVersion);
				IOObject.ThrowIfError (result);
				return ioUSBLibVersion;
			}
		}

		[Since (2,5)]
		public NumVersion IOUSBFamilyVersion {
			get  {
				ThrowIfDisposed ();
				NumVersion ioUSBLibVersion, usbFamilyVersion;
				var result = InterfaceInterface.GetIOUSBLibVersion (InterfaceInterfaceRef, out ioUSBLibVersion, out usbFamilyVersion);
				IOObject.ThrowIfError (result);
				return usbFamilyVersion;
			}
		}

		public CFRunLoopSource CreateAsyncEventSource ()
		{
			ThrowIfDisposed ();
			IntPtr runLoopSourceRef;
			var result = InterfaceInterface.CreateInterfaceAsyncEventSource (InterfaceInterfaceRef, out runLoopSourceRef);
			IOObject.ThrowIfError (result);
			var runLoopSource = new CFRunLoopSource (runLoopSourceRef, false);
			CFType.Release (runLoopSourceRef);
			return runLoopSource;
		}

		public Port CreateAsyncPort ()
		{
			ThrowIfDisposed ();
			IntPtr portRef;
			var result = InterfaceInterface.CreateInterfaceAsyncPort (InterfaceInterfaceRef, out portRef);
			IOObject.ThrowIfError (result);
			return new Port (portRef);
		}

		public void Open ()
		{
			ThrowIfDisposed ();
			var result = InterfaceInterface.USBInterfaceOpen (InterfaceInterfaceRef);
			IOObject.ThrowIfError (result);
		}

		public void Close ()
		{
			ThrowIfDisposed ();
			var result = InterfaceInterface.USBInterfaceClose (InterfaceInterfaceRef);
			IOObject.ThrowIfError (result);
		}

		public ulong GetBusFrameNumber (out ulong atTime) {
			ThrowIfDisposed ();
			ulong frame;
			var result = InterfaceInterface.GetBusFrameNumber (InterfaceInterfaceRef, out frame, out atTime);
			IOObject.ThrowIfError (result);
			return (ulong)IOUSB.USBToHostOrder ((long)frame);
		}

		[Since (1,0)]
		public void OpenSeize ()
		{
			ThrowIfDisposed ();
			var result = InterfaceInterface.USBInterfaceOpenSeize (InterfaceInterfaceRef);
			IOObject.ThrowIfError (result);
		}

		[Since (2,0)]
		public EndpointProperties GetEndpointProperties (byte alternateSetting, byte endpoint, EndpointDirection direction)
		{
			ThrowIfDisposed ();
			byte transferType;
			ushort maxPacketSize;
			byte interval;
			var result = InterfaceInterface.GetEndpointProperties (InterfaceInterfaceRef, alternateSetting, endpoint,
			                                                       (byte)direction, out transferType,
			                                                       out maxPacketSize, out interval);
			IOObject.ThrowIfError (result);
			return new EndpointProperties () {
				TransferType = (EndpointType) transferType,
				MaxPacketSize = maxPacketSize,
				Interval = (Interval)interval
			};
		}

		[Since (2,5)]
		public ulong GetBusMicroFrameNumber (out ulong atTime)
		{
			ThrowIfDisposed ();
			ulong microFrame;
			var result = InterfaceInterface.GetBusMicroFrameNumber (InterfaceInterfaceRef, out microFrame, out atTime);
			IOObject.ThrowIfError (result);
			return microFrame;
		}

		[Since (4,0)]
		public IIOUSBDescriptor FindFirstAssociatedDescriptor (DescriptorType type = DescriptorType.Any)
		{
			return FindNextAssociatedDescriptor (null, type);
		}

		[Since (4,0)]
		public IIOUSBDescriptor FindNextAssociatedDescriptor (IIOUSBDescriptor current, DescriptorType type = DescriptorType.Any)
		{
			ThrowIfDisposed ();
			IntPtr currentRef;
			if (current == null)
				currentRef = IntPtr.Zero;
			else {
				currentRef = Marshal.AllocHGlobal (Marshal.SizeOf (current));
				Marshal.StructureToPtr (current, currentRef, false);
			}
			var result = InterfaceInterface.FindNextAssociatedDescriptor (InterfaceInterfaceRef, currentRef, (byte)type);
			if (currentRef != IntPtr.Zero)
				Marshal.FreeHGlobal (currentRef);
			if (result == IntPtr.Zero)
				return null;
			var header = (IOUSBDescriptorHeader)Marshal.PtrToStructure (result, typeof(IOUSBDescriptorHeader));
			switch (header.DescriptorType) {
			case DescriptorType.Configuration:
				return (IOUSBConfigurationDescriptor)Marshal.PtrToStructure (result, typeof(IOUSBConfigurationDescriptor));
			default:
				throw new NotImplementedException ();
			}
		}

		[Since (4,0)]
		public IIOUSBDescriptor FindFirstAlternateInterface (IOUSBFindInterfaceRequest request)
		{
			return FindNextAlternateInterface (null, request);
		}

		[Since (4,0)]
		public IIOUSBDescriptor FindNextAlternateInterface (IIOUSBDescriptor current, IOUSBFindInterfaceRequest request)
		{
			ThrowIfDisposed ();
			IntPtr currentRef;
			if (current == null)
				currentRef = IntPtr.Zero;
			else {
				currentRef = Marshal.AllocHGlobal (Marshal.SizeOf (current));
				Marshal.StructureToPtr (current, currentRef, false);
			}
			var result = InterfaceInterface.FindNextAltInterface (InterfaceInterfaceRef, currentRef, request);
			if (currentRef != IntPtr.Zero)
				Marshal.FreeHGlobal (currentRef);
			if (result == IntPtr.Zero)
				return null;
			var header = (IOUSBDescriptorHeader)Marshal.PtrToStructure (result, typeof(IOUSBDescriptorHeader));
			switch (header.DescriptorType) {
			case DescriptorType.Configuration:
				return (IOUSBConfigurationDescriptor)Marshal.PtrToStructure (result, typeof(IOUSBConfigurationDescriptor));
			default:
				throw new NotImplementedException ();
			}
		}

		[Since (5,0)]
		public ulong GetBusFrameNumberWithTime (out ulong atTime)
		{
			ThrowIfDisposed ();
			ulong frame;
			var result = InterfaceInterface.GetBusFrameNumberWithTime (InterfaceInterfaceRef, out frame, out atTime);
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
			var result = InterfaceInterface.GetEndpointPropertiesV3 (InterfaceInterfaceRef, ref properties);
			IOObject.ThrowIfError (result);
			return properties;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			foreach (var pipe in pipes.Values)
				pipe.Dispose ();
		}

		public class Pipe : IDisposable
		{
			IOUSBInterfaceInterface @interface;
			IntPtr interfaceRef;
			byte pipeIndex;
			Dictionary<uint, Stream> streams;

			internal Pipe (IOUSBInterfaceInterface @interface, IntPtr interfaceRef, byte pipeIndex)
			{
				this.@interface = @interface;
				this.interfaceRef = interfaceRef;
				this.pipeIndex = pipeIndex;
				streams = new Dictionary<uint, Stream> ();
			}

			~Pipe ()
			{
				Dispose (false);
			}
			
			void SendControlRequest (ref IOUSBDevRequest request)
			{
				ThrowIfDisposed ();
				var result = @interface.ControlRequest (interfaceRef, pipeIndex, ref request);
				IOObject.ThrowIfError (result);
			}

			// TODO: replace IOAsyncCallback1 with a more suitable delegate
			void SendControlRequestAsync (IOUSBDevRequest request, IOAsyncCallback1 callback)
			{
				ThrowIfDisposed ();
				var result = @interface.ControlRequestAsync (interfaceRef, pipeIndex,
				                                             request, callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
			}

			public PipeProperties Properties {
				get {
					ThrowIfDisposed ();
					byte direction, number, transferType, interval;
					ushort maxPacketSize;
					var result = @interface.GetPipeProperties (interfaceRef, pipeIndex,
					                                           out direction, out number, out transferType,
					                                           out maxPacketSize, out interval);
					IOObject.ThrowIfError (result);
					return new PipeProperties () {
						Direction = (EndpointDirection)direction,
						Number = number,
						TransferType = (EndpointType)transferType,
						MaxPacketSize = maxPacketSize,
						Interval = (Interval)interval
					};
				}
			}

			public byte Status {
				get {
					ThrowIfDisposed ();
					var result = @interface.GetPipeStatus (interfaceRef, pipeIndex);
					// TODO: create enum for common return values.
					IOObject.ThrowIfError (result);
					return 0;
				}
			}

			public void Abort ()
			{
				ThrowIfDisposed ();
				var result = @interface.AbortPipe (interfaceRef, pipeIndex);
				IOObject.ThrowIfError (result);
			}

			public void Reset ()
			{
				ThrowIfDisposed ();
				var result = @interface.ResetPipe (interfaceRef, pipeIndex);
				IOObject.ThrowIfError (result);
			}

			public void ClearStall ()
			{
				ThrowIfDisposed ();
				var result = @interface.ClearPipeStall (interfaceRef, pipeIndex);
				IOObject.ThrowIfError (result);
			}

			public byte[] Read (uint byteCount)
			{
				ThrowIfDisposed ();
				var buffer = new byte[byteCount];
				var result = @interface.ReadPipe (interfaceRef, pipeIndex, buffer, ref byteCount);
				IOObject.ThrowIfError (result);
				return buffer;
			}

			public void Write (byte[] bytes)
			{
				ThrowIfDisposed ();
				var result = @interface.WritePipe (interfaceRef, pipeIndex, bytes, (uint)bytes.Length);
				IOObject.ThrowIfError (result);
			}

			public void ReadAsync (uint byteCount, IOAsyncCallback1 callback)
			{
				ThrowIfDisposed ();
				var buffer = new byte[byteCount];
				var result = @interface.ReadPipeAsync (interfaceRef, pipeIndex, buffer,
				                                       byteCount, callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
			}

			public void WriteAsync (byte[] bytes, IOAsyncCallback1 callback)
			{
				ThrowIfDisposed ();
				var result = @interface.WritePipeAsync (interfaceRef, pipeIndex, bytes,
				                                        (uint)bytes.Length,callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
			}
			
			[Since (0,4)]
			void SendControlRequest (ref IOUSBDevRequestTO request)
			{
				ThrowIfDisposed ();
				var result = @interface.ControlRequestTO (interfaceRef, pipeIndex, ref request);
				IOObject.ThrowIfError (result);
			}

			// TODO: replace IOAsyncCallback1 with a more suitable delegate
			[Since (0,4)]
			void SendControlRequestAsync (IOUSBDevRequestTO request, IOAsyncCallback1 callback)
			{
				ThrowIfDisposed ();
				var result = @interface.ControlRequestAsyncTO (interfaceRef, pipeIndex,
				                                             request, callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
			}
			
			[Since (0,4)]
			public byte[] Read (uint byteCount, uint noDataTimeout, uint completionTimeout)
			{
				ThrowIfDisposed ();
				var buffer = new byte[byteCount];
				var result = @interface.ReadPipeTO (interfaceRef, pipeIndex, buffer,
				                                    ref byteCount, noDataTimeout,
				                                    completionTimeout);
				IOObject.ThrowIfError (result);
				return buffer;
			}
			
			[Since (0,4)]
			public void Write (byte[] bytes, uint noDataTimeout, uint completionTimeout)
			{
				ThrowIfDisposed ();
				var result = @interface.WritePipeTO (interfaceRef, pipeIndex, bytes,
				                                     (uint)bytes.Length, noDataTimeout,
				                                     completionTimeout);
				IOObject.ThrowIfError (result);
			}
			
			[Since (0,4)]
			public void ReadAsync (uint byteCount, uint noDataTimeout, uint completionTimeout, IOAsyncCallback1 callback)
			{
				ThrowIfDisposed ();
				var buffer = new byte[byteCount];
				var result = @interface.ReadPipeAsyncTO (interfaceRef, pipeIndex, buffer,
				                                         byteCount, noDataTimeout,
				                                         completionTimeout, callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
			}

			[Since (0,4)]
			public void WriteAsync (byte[] bytes, uint noDataTimeout, uint completionTimeout, IOAsyncCallback1 callback)
			{
				ThrowIfDisposed ();
				var result = @interface.WritePipeAsyncTO (interfaceRef, pipeIndex, bytes,
				                                          noDataTimeout, completionTimeout,
				                                          (uint)bytes.Length,callback, IntPtr.Zero);
				IOObject.ThrowIfError (result);
			}

			[Since (2,0)]
			public void ClearStallBothEnds ()
			{
				ThrowIfDisposed ();
				var result = @interface.ClearPipeStallBothEnds (interfaceRef, pipeIndex);
				IOObject.ThrowIfError (result);
			}

			[Since (2,0)]
			public void SetPipePolicy (ushort maxPacketSize, Interval maxInterval)
			{
				ThrowIfDisposed ();
				var result = @interface.SetPipePolicy (interfaceRef, pipeIndex, maxPacketSize, (byte)maxInterval);
				IOObject.ThrowIfError (result);
			}

			[Since (7,3)]
			[Obsolete ("Use PropertiesV3")]
			public PipePropertiesV2 PropertiesV2 {
				get {
					ThrowIfDisposed ();
					byte direction, number, transferType, interval, maxBurst, mult;
					ushort maxPacketSize, bytesPerInterval;
					var result = @interface.GetPipePropertiesV2 (interfaceRef, pipeIndex,
					                                             out direction, out number, out transferType,
					                                             out maxPacketSize, out interval,
					                                             out maxBurst, out mult, out bytesPerInterval);
					IOObject.ThrowIfError (result);
					return new PipePropertiesV2 () {
						Direction = (EndpointDirection)direction,
						Number = number,
						TransferType = (EndpointType)transferType,
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
					var result = @interface.GetPipePropertiesV3 (interfaceRef, pipeIndex, ref properties);
					IOObject.ThrowIfError (result);
					return properties;
				}
			}

			[Since (8,0)]
			public uint SupportedStreamCount {
				get {
					ThrowIfDisposed ();
					uint count;
					var result = @interface.SupportsStreams (interfaceRef, pipeIndex, out count);
					IOObject.ThrowIfError (result);
					return count;
				}
			}

			[Since (8,0)]
			public void CreateStreams (uint count) {
				ThrowIfDisposed ();
				if (count == 0)
					throw new ArgumentOutOfRangeException ("count");
				var result = @interface.CreateStreams (interfaceRef, pipeIndex, count);
				IOObject.ThrowIfError (result);
			}

			[Since (8,0)]
			public void DisposeStreams () {
				ThrowIfDisposed ();
				var result = @interface.CreateStreams (interfaceRef, pipeIndex, 0);
				IOObject.ThrowIfError (result);
				foreach (var stream in streams.Values)
					stream.Dispose ();
				streams.Clear ();
			}

			[Since (8,0)]
			public uint ConfiguredStreamCount {
				get {
					ThrowIfDisposed ();
					uint count;
					var result = @interface.GetConfiguredStreams (interfaceRef, pipeIndex, out count);
					IOObject.ThrowIfError (result);
					return count;
				}
			}

			[Since (8,0)]
			[IndexerName ("Streams")]
			public Stream this[uint id] {
				get {
					ThrowIfDisposed ();
					if (id < 1 || id > ConfiguredStreamCount)
						throw new ArgumentOutOfRangeException ("id");
					if (!streams.ContainsKey (id))
						streams.Add (id, new Stream (@interface, interfaceRef, pipeIndex, id));
					return streams [id];
				}
			}

			protected void ThrowIfDisposed ()
			{
				if (interfaceRef == IntPtr.Zero)
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
					@interface = null;
					streams = null;
				}
				if (ConfiguredStreamCount > 0)
					DisposeStreams ();
				interfaceRef = IntPtr.Zero;
			}

			public class Stream : System.IO.Stream, IDisposable
			{
				IOUSBInterfaceInterface @interface;
				IntPtr interfaceRef;
				byte pipeIndex;
				uint id;
				List<AsyncCallback> callbacks;

				internal Stream (IOUSBInterfaceInterface @interface, IntPtr interfaceRef, byte pipeIndex, uint id)
				{
					this.@interface = @interface;
					this.interfaceRef = interfaceRef;
					this.pipeIndex = pipeIndex;
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
					var result = @interface.ReadStreamsPipeTO (interfaceRef, pipeIndex, id, buffer2, ref size, (uint)this.WriteTimeout, (uint)this.WriteTimeout);
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
					var result = @interface.WriteStreamsPipeTO (interfaceRef, pipeIndex, id, buffer2, (uint)count, (uint)this.WriteTimeout, (uint)this.WriteTimeout);
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
					if (interfaceRef == IntPtr.Zero)
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
						@interface = null;
						callbacks = null;
					}
					interfaceRef = IntPtr.Zero;
				}
			}
		}
	}
	
	public struct PipeProperties
	{
		public EndpointDirection Direction;
		public UInt8 Number;
		public EndpointType TransferType;
		public UInt16 MaxPacketSize;
		public Interval Interval;
	}

	public struct PipePropertiesV2
	{
		public EndpointDirection Direction;
		public UInt8 Number;
		public EndpointType TransferType;
		public UInt16 MaxPacketSize;
		public Interval Interval;
		public UInt8 MaxBurst;
		public UInt8 Mult;
		public UInt16 BytesPerInterval;
	}
	
	/// <summary>
	/// Structure used to encode information about each isoc frame.
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBIsocFrame
	{
		/// <summary>
		/// Returns status associated with the frame.
		/// </summary>
		public IOReturn Status;

		/// <summary>
		/// Input specifiying how many bytes to read or write.
		/// </summary>
		public UInt16   RequestedCount;

		/// <summary>
		/// Actual # of bytes transferred.
		/// </summary>
		public UInt16   ActualCount;
	} 

	/// <summary>
	/// Structure used to encode information about each isoc frame that is processed
	/// at hardware interrupt time (low latency).
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBLowLatencyIsocFrame
	{
		/// <summary>
		/// Returns status associated with the frame.
		/// </summary>
		public IOReturn Status;

		/// <summary>
		/// Input specifiying how many bytes to read or write.
		/// </summary>
		public UInt16 RequestedCount;

		/// <summary>
		/// Actual # of bytes transferred.
		/// </summary>
		public UInt16 ActualCount;

		/// <summary>
		/// Time stamp that indicates time when frame was procesed.
		/// </summary>
		public AbsoluteTime TimeStamp;
	}

	/// <summary>
	/// Structure used with the IOUSBLib GetEndpointPropertiesV3 and GetPipePropertiesV3 API.
	/// Most of the fields are taken directly from corresponding Standard Endpoint Descriptor and
	/// SuperSpeed Endpoint Companion Descriptor. BytesPerInterval will be synthesized
	/// for High Speed High Bandwidth Isochronous endpoints.
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBEndpointProperties
	{
		/// <summary>
		/// Version of the structure.
		/// Currently kUSBEndpointPropertiesVersion3.
		/// Need to set this when using this structure
		/// </summary>
		public EndpointPropertiesVersion Version;

		/// <summary>
		/// Used as an input for GetEndpointPropertiesV3.
		/// Used as an output for GetPipePropertiesV3
		/// </summary>
		public UInt8 AlternateSetting;

		/// <summary>
		/// Used as an input for GetEndpointPropertiesV3.
		/// Used as an output for GetPipePropertiesV3.
		/// </summary>
		public EndpointDirection Direction;

		/// <summary>
		/// Used as an input for GetEndpointPropertiesV3.
		/// Used as an output for GetPipePropertiesV3
		/// </summary>
		public UInt8 EndpointNumber;

		/// <summary>
		/// Endpoint transfer type.
		/// </summary>
		public EndpointType TransferType;

		/// <summary>
		/// For interrupt endpoints, <see cref="InterruptUsageType"/> and
		/// for isoc endpoints, <see cref="IsocUsageType"/>.
		/// For Bulk endpoints of the UAS Mass Storage Protocol, the pipe ID.
		/// </summary>
		public UInt8 UsageType;

		/// <summary>
		/// For isoc endpoints only
		/// </summary>
		public IsocSyncType SyncType;

		/// <summary>
		/// The Interval field from the Standard Endpoint descriptor.
		/// </summary>
		public Interval Interval;

		/// <summary>
		/// The meaning of this value depends on whether this is called
		/// with GetPipePropertiesV3 or GetEndpointPropertiesV3.
		/// See the documentation of those calls for more info.
		/// </summary>
		public UInt16 MaxPacketSize;

		/// <summary>
		/// For SuperSpeed endpoints, maximum number of packets the endpoint
		/// can send or receive as part of a burst
		/// </summary>
		public UInt8 MaxBurst;

		/// <summary>
		/// For SuperSpeed bulk endpoints, maximum number of streams this
		/// endpoint supports.
		/// </summary>
		public UInt8 MaxStreams;

		/// <summary>
		/// For SuperSpeed isoc endpoints, this is the mult value from the
		/// SuperSpeed Endpoint Companion Descriptor. For High Speed isoc and
		/// interrupt endpoints, this is bits 11 and 12 of the Standard Endpoint
		/// Descriptor, which represents a similar value.
		/// </summary>
		public UInt8 Mult;

		/// <summary>
		/// For SuperSpeed interrupt and isoc endpoints, this is the
		/// BytesPerInterval from the SuperSpeed Endpoint Companion Descriptor.
		/// For High Speed High Bandwidth isoc endpoints, this will be equal to
		/// MaxPacketSize * (Mult + 1).
		/// </summary>
		public UInt16 BytesPerInterval;
	}

	public struct EndpointProperties
	{
		public EndpointType TransferType;
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
		CreateInterfaceAsyncEventSource createInterfaceAsyncEventSource;
		GetInterfaceAsyncEventSource getInterfaceAsyncEventSource;
		CreateInterfaceAsyncPort createInterfaceAsyncPort;
		GetInterfaceAsyncPort getInterfaceAsyncPort;
		USBInterfaceOpen usbInterfaceOpen;
		USBInterfaceClose usbInterfaceClose;
		GetInterfaceClass getInterfaceClass;
		GetInterfaceSubClass getInterfaceSubClass;
		GetInterfaceProtocol getInterfaceProtocol;
		GetDeviceVendor getDeviceVendor;
		GetDeviceProduct getDeviceProduct;
		GetDeviceReleaseNumber getDeviceReleaseNumber;
		GetConfigurationValue getConfigurationValue;
		GetInterfaceNumber getInterfaceNumber;
		GetAlternateSetting getAlternateSetting;
		GetNumEndpoints getNumEndpoints;
		GetLocationID getLocationID;
		GetDevice getDevice;
		SetAlternateInterface setAlternateInterface;
		GetBusFrameNumber getBusFrameNumber;
		ControlRequest controlRequest;
		ControlRequestAsync controlRequestAsync;
		GetPipeProperties getPipeProperties;
		GetPipeStatus getPipeStatus;
		AbortPipe abortPipe;
		ResetPipe resetPipe;
		ClearPipeStall clearPipeStall;
		ReadPipe readPipe;
		WritePipe writePipe;
		ReadPipeAsync readPipeAsync;
		WritePipeAsync writePipeAsync;
		ReadIsochPipeAsync readIsochPipeAsync;
		WriteIsochPipeAsync writeIsochPipeAsync;

		public CreateInterfaceAsyncEventSource CreateInterfaceAsyncEventSource {
			get { return createInterfaceAsyncEventSource; }
		}

		public GetInterfaceAsyncEventSource GetInterfaceAsyncEventSource {
			get { return getInterfaceAsyncEventSource; }
		}

		public CreateInterfaceAsyncPort  CreateInterfaceAsyncPort  {
			get { return createInterfaceAsyncPort; }
		}

		public GetInterfaceAsyncPort GetInterfaceAsyncPort {
			get { return getInterfaceAsyncPort; }
		}

		public USBInterfaceOpen USBInterfaceOpen {
			get { return usbInterfaceOpen; }
		}

		public USBInterfaceClose USBInterfaceClose {
			get { return usbInterfaceClose; }
		}

		public GetInterfaceClass GetInterfaceClass {
			get { return getInterfaceClass; }
		}

		public GetInterfaceSubClass GetInterfaceSubClass{
			get { return getInterfaceSubClass; }
		}

		public GetInterfaceProtocol GetInterfaceProtocol {
			get { return getInterfaceProtocol; }
		}

		public GetDeviceVendor GetDeviceVendor {
			get { return getDeviceVendor; }
		}

		public GetDeviceProduct GetDeviceProduct {
			get { return getDeviceProduct; }
		}

		public GetDeviceReleaseNumber GetDeviceReleaseNumber {
			get { return getDeviceReleaseNumber; }
		}

		public GetConfigurationValue GetConfigurationValue {
			get { return getConfigurationValue; }
		}

		public GetInterfaceNumber GetInterfaceNumber {
			get { return getInterfaceNumber; }
		}

		public GetAlternateSetting GetAlternateSetting {
			get { return getAlternateSetting; }
		}

		public GetNumEndpoints GetNumEndpoints {
			get { return getNumEndpoints; }
		}

		public GetLocationID GetLocationID {
			get { return getLocationID; }
		}

		public GetDevice GetDevice {
			get { return getDevice; }
		}

		public SetAlternateInterface SetAlternateInterface {
			get { return setAlternateInterface; }
		}

		public GetBusFrameNumber GetBusFrameNumber {
			get { return getBusFrameNumber; }
		}

		public ControlRequest ControlRequest {
			get { return controlRequest; }
		}

		public ControlRequestAsync ControlRequestAsync {
			get { return controlRequestAsync; }
		}

		public GetPipeProperties GetPipeProperties {
			get { return getPipeProperties; }
		}

		public GetPipeStatus GetPipeStatus {
			get { return getPipeStatus; }
		}

		public AbortPipe AbortPipe {
			get { return abortPipe; }
		}

		public ResetPipe ResetPipe {
			get { return resetPipe; }
		}

		public ClearPipeStall ClearPipeStall {
			get { return clearPipeStall; }
		}

		public ReadPipe ReadPipe {
			get { return readPipe; }
		}

		public WritePipe WritePipe {
			get { return writePipe; }
		}

		public ReadPipeAsync ReadPipeAsync {
			get { return readPipeAsync; }
		}

		public WritePipeAsync WritePipeAsync {
			get { return writePipeAsync; }
		}

		public ReadIsochPipeAsync ReadIsochPipeAsync {
			get { return readIsochPipeAsync; }
		}

		public WriteIsochPipeAsync WriteIsochPipeAsync {
			get { return writeIsochPipeAsync; }
		}

		public virtual ControlRequestTO ControlRequestTO {
			get { throw new NotImplementedException (); }
		}

		public virtual ControlRequestAsyncTO ControlRequestAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual ReadPipeTO ReadPipeTO {
			get { throw new NotImplementedException (); }
		}

		public virtual WritePipeTO WritePipeTO {
			get { throw new NotImplementedException (); }
		}

		public virtual ReadPipeAsyncTO ReadPipeAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual WritePipeAsyncTO WritePipeAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual USBInterfaceGetStringIndex USBInterfaceGetStringIndex {
			get { throw new NotImplementedException (); }
		}

		public virtual USBInterfaceOpenSeize USBInterfaceOpenSeize {
			get { throw new NotImplementedException (); }
		}

		public virtual ClearPipeStallBothEnds ClearPipeStallBothEnds {
			get { throw new NotImplementedException (); }
		}

		public virtual SetPipePolicy SetPipePolicy {
			get { throw new NotImplementedException (); }
		}

		public virtual GetBandwidthAvailable GetBandwidthAvailable {
			get { throw new NotImplementedException (); }
		}

		public virtual GetEndpointProperties GetEndpointProperties {
			get { throw new NotImplementedException (); }
		}

		public virtual LowLatencyReadIsochPipeAsync LowLatencyReadIsochPipeAsync {
			get { throw new NotImplementedException (); }
		}

		public virtual LowLatencyWriteIsochPipeAsync LowLatencyWriteIsochPipeAsync {
			get { throw new NotImplementedException (); }
		}

		public virtual LowLatencyCreateBuffer LowLatencyCreateBuffer {
			get { throw new NotImplementedException (); }
		}

		public virtual LowLatencyDestroyBuffer LowLatencyDestroyBuffer {
			get { throw new NotImplementedException (); }
		}

		public virtual GetBusMicroFrameNumber GetBusMicroFrameNumber {
			get { throw new NotImplementedException (); }
		}

		public virtual GetFrameListTime GetFrameListTime {
			get { throw new NotImplementedException (); }
		}

		public virtual GetIOUSBLibVersion GetIOUSBLibVersion {
			get { throw new NotImplementedException (); }
		}

		public virtual FindNextAssociatedDescriptor FindNextAssociatedDescriptor {
			get { throw new NotImplementedException (); }
		}

		public virtual FindNextAltInterface FindNextAltInterface {
			get { throw new NotImplementedException (); }
		}

		public virtual GetBusFrameNumberWithTime GetBusFrameNumberWithTime {
			get { throw new NotImplementedException (); }
		}

		public virtual GetPipePropertiesV2 GetPipePropertiesV2 {
			get { throw new NotImplementedException (); }
		}

		public virtual GetPipePropertiesV3 GetPipePropertiesV3 {
			get { throw new NotImplementedException (); }
		}

		public virtual GetEndpointPropertiesV3 GetEndpointPropertiesV3 {
			get { throw new NotImplementedException (); }
		}

		public virtual SupportsStreams SupportsStreams {
			get { throw new NotImplementedException (); }
		}

		public virtual CreateStreams CreateStreams {
			get { throw new NotImplementedException (); }
		}

		public virtual GetConfiguredStreams GetConfiguredStreams {
			get { throw new NotImplementedException (); }
		}

		public virtual ReadStreamsPipeTO ReadStreamsPipeTO {
			get { throw new NotImplementedException (); }
		}

		public virtual WriteStreamsPipeTO WriteStreamsPipeTO {
			get { throw new NotImplementedException (); }
		}

		public virtual ReadStreamsPipeAsyncTO ReadStreamsPipeAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual WriteStreamsPipeAsyncTO WriteStreamsPipeAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual AbortStreamsPipe AbortStreamsPipe {
			get { throw new NotImplementedException (); }
		}
	}

	[Guid ("4923AC4C-4896-11D5-9208-000A27801E86")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface182 : IOUSBInterfaceInterface
	{
		ControlRequestTO controlRequestTO;
		ControlRequestAsyncTO controlRequestAsyncTO;
		ReadPipeTO readPipeTO;
		WritePipeTO writePipeTO;
		ReadPipeAsyncTO readPipeAsyncTO;
		WritePipeAsyncTO writePipeAsyncTO;
		USBInterfaceGetStringIndex usbInterfaceGetStringIndex;

		public override ControlRequestTO ControlRequestTO {
			get { return controlRequestTO; }
		}

		public override ControlRequestAsyncTO ControlRequestAsyncTO {
			get { return controlRequestAsyncTO; }
		}

		public override ReadPipeTO ReadPipeTO {
			get { return readPipeTO; }
		}

		public override WritePipeTO WritePipeTO {
			get { return writePipeTO; }
		}

		public override ReadPipeAsyncTO ReadPipeAsyncTO {
			get { return readPipeAsyncTO; }
		}

		public override WritePipeAsyncTO WritePipeAsyncTO {
			get { return writePipeAsyncTO; }
		}

		public override USBInterfaceGetStringIndex USBInterfaceGetStringIndex {
			get { return usbInterfaceGetStringIndex; }
		}
	}

	[Guid ("1C438356-74C4-11D5-92E6-000A27801E86")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface183 : IOUSBInterfaceInterface182
	{
		USBInterfaceOpenSeize usbInterfaceOpenSeize;

		public override USBInterfaceOpenSeize USBInterfaceOpenSeize {
			get { return usbInterfaceOpenSeize; }
		}
	}

	[Guid ("8FDB8455-74A6-11D6-97B1-003065D3608E")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface190 : IOUSBInterfaceInterface183
	{
		ClearPipeStallBothEnds clearPipeStallBothEnds;
		SetPipePolicy setPipePolicy;
		GetBandwidthAvailable getBandwidthAvailable;
		GetEndpointProperties getEndpointProperties;

		public override ClearPipeStallBothEnds ClearPipeStallBothEnds {
			get { return clearPipeStallBothEnds; }
		}

		public override SetPipePolicy SetPipePolicy {
			get { return setPipePolicy; }
		}

		public override GetBandwidthAvailable GetBandwidthAvailable {
			get { return getBandwidthAvailable; }
		}

		public override GetEndpointProperties GetEndpointProperties {
			get { return getEndpointProperties; }
		}
	}

	[Guid ("6C798A6E-D6E9-11D6-ADD6-0003933E3E3E")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface192 : IOUSBInterfaceInterface190
	{
		LowLatencyReadIsochPipeAsync lowLatencyReadIsochPipeAsync;
		LowLatencyWriteIsochPipeAsync lowLatencyWriteIsochPipeAsync;
		LowLatencyCreateBuffer lowLatencyCreateBuffer;
		LowLatencyDestroyBuffer lowLatencyDestroyBuffer;

		public override LowLatencyReadIsochPipeAsync LowLatencyReadIsochPipeAsync {
			get { return lowLatencyReadIsochPipeAsync; }
		}

		public override LowLatencyWriteIsochPipeAsync LowLatencyWriteIsochPipeAsync {
			get { return lowLatencyWriteIsochPipeAsync; }
		}

		public override LowLatencyCreateBuffer LowLatencyCreateBuffer {
			get { return lowLatencyCreateBuffer; }
		}

		public override LowLatencyDestroyBuffer LowLatencyDestroyBuffer {
			get { return lowLatencyDestroyBuffer; }
		}
	}

	[Guid ("C63D3C92-0884-11D7-9692-0003933E3E3E")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface197 : IOUSBInterfaceInterface192
	{
		GetBusMicroFrameNumber getBusMicroFrameNumber;
		GetFrameListTime getFrameListTime;
		GetIOUSBLibVersion getIOUSBLibVersion;

		public override GetBusMicroFrameNumber GetBusMicroFrameNumber {
			get { return getBusMicroFrameNumber; }
		}

		public override GetFrameListTime GetFrameListTime {
			get { return getFrameListTime; }
		}

		public override GetIOUSBLibVersion GetIOUSBLibVersion {
			get { return getIOUSBLibVersion; }
		}
	}

	[Guid ("770DE60C-2FE8-11D8-A582-000393DCB1D0")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface220 : IOUSBInterfaceInterface197
	{
		FindNextAssociatedDescriptor findNextAssociatedDescriptor;
		FindNextAltInterface findNextAltInterface;

		public override FindNextAssociatedDescriptor FindNextAssociatedDescriptor {
			get { return findNextAssociatedDescriptor; }
		}

		public override FindNextAltInterface FindNextAltInterface {
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
		GetBusFrameNumberWithTime getBusFrameNumberWithTime;

		public override GetBusFrameNumberWithTime GetBusFrameNumberWithTime {
			get { return getBusFrameNumberWithTime; }
		}
	}

	[Guid ("6C0D38C3-B093-4EA7-809B-09FB5DDDAC16")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface500 : IOUSBInterfaceInterface300
	{
		GetPipePropertiesV2 getPipePropertiesV2;

		public override GetPipePropertiesV2 GetPipePropertiesV2 {
			get { return getPipePropertiesV2; }
		}
	}

	[Guid ("6AE44D3F-EB45-487F-8E8E-B93B99F8EA9E")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBInterfaceInterface550 : IOUSBInterfaceInterface500
	{
		GetPipePropertiesV3 getPipePropertiesV3;
		GetEndpointPropertiesV3 getEndpointPropertiesV3;
		SupportsStreams supportsStreams;
		CreateStreams createStreams;
		GetConfiguredStreams getConfiguredStreams;
		ReadStreamsPipeTO readStreamsPipeTO;
		WriteStreamsPipeTO writeStreamsPipeTO;
		ReadStreamsPipeAsyncTO readStreamsPipeAsyncTO;
		WriteStreamsPipeAsyncTO writeStreamsPipeAsyncTO;
		AbortStreamsPipe abortStreamsPipe;

		public override GetPipePropertiesV3 GetPipePropertiesV3 {
			get { return getPipePropertiesV3; }
		}

		public override GetEndpointPropertiesV3 GetEndpointPropertiesV3 {
			get { return getEndpointPropertiesV3; }
		}

		public override SupportsStreams SupportsStreams {
			get { return supportsStreams; }
		}

		public override CreateStreams CreateStreams {
			get { return createStreams; }
		}

		public override GetConfiguredStreams GetConfiguredStreams {
			get { return getConfiguredStreams; }
		}

		public override ReadStreamsPipeTO ReadStreamsPipeTO {
			get { return readStreamsPipeTO; }
		}

		public override WriteStreamsPipeTO WriteStreamsPipeTO {
			get { return writeStreamsPipeTO; }
		}

		public override ReadStreamsPipeAsyncTO ReadStreamsPipeAsyncTO {
			get { return readStreamsPipeAsyncTO; }
		}

		public override WriteStreamsPipeAsyncTO WriteStreamsPipeAsyncTO {
			get { return writeStreamsPipeAsyncTO; }
		}

		public override AbortStreamsPipe AbortStreamsPipe {
			get { return abortStreamsPipe; }
		}
	}

	/*!
	@interface IOUSBInterfaceInterface
	@abstract   The object you use to access a USB device interface from user space, returned by all versions
	        of the IOUSBFamily currently shipping.
	@discussion The functions listed here will work with any version of the IOUSBInterfaceInterface, including
	        the one shipped with Mac OS X version 10.0.
	*/

	/*!
	@function CreateInterfaceAsyncEventSource
	@abstract   Creates a run loop source for delivery of all asynchronous notifications on this device.
	@discussion The Mac OS X kernel does not spawn a thread to callback to the client. Instead 
	        it delivers completion notifications on a Mach port (see @link //apple_ref/C/instm/IOUSBInterfaceInterface/CreateInterfaceAsyncPort/ CreateInterfaceAsyncPort @/link). This 
	        routine wraps that port with the appropriate routing code so that 
	        the completion notifications can be automatically routed through the client's 
	        CFRunLoop.
	@param      self Pointer to the IOUSBInterfaceInterface.
	@param      source Pointer to a CFRunLoopSourceRef to return the newly created run loop event source.
	@result     Returns kIOReturnSuccess if successful or a kern_return_t if failed.
	*/
	delegate IOReturn CreateInterfaceAsyncEventSource (IntPtr self, out CFRunLoopSourceRef source);

	/*!
    @function GetInterfaceAsyncEventSource
    @abstract   Returns the CFRunLoopSourceRef for this IOService instance.
    @discussion (description)
    @param      self Pointer to the IOUSBInterfaceInterface.
    @result     Returns the run loop source if one has been created, 0 otherwise.
	*/
	delegate ICFRunLoopSourceRef GetInterfaceAsyncEventSource (IntPtr self);

	/*!
    @function CreateInterfaceAsyncPort
    @abstract   Creates and registers a mach_port_t for asynchronous communications.
    @discussion The Mac OS X kernel does not spawn a thread to callback to the client. Instead 
                it delivers completion notifications on this Mach port. After receiving a message 
                on this port the client is obliged to call the IOKitLib.h: IODispatchCalloutFromMessage() 
                function for decoding the notification message.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @result     Returns kIOReturnSuccess if successful or a kern_return_t if failed.
	*/
	delegate IOReturn CreateInterfaceAsyncPort (IntPtr self, out mach_port_t port);

	/*!
    @function GetInterfaceAsyncPort
    @abstract   Returns the mach_port_t port for this IOService instance.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @result     Returns the port if one exists, 0 otherwise.
	*/
	delegate mach_port_t GetInterfaceAsyncPort (IntPtr self);

	/*!
    @function USBInterfaceOpen
    @abstract   Opensthe IOUSBInterface for exclusive access.
    @discussion Before the client can transfer data to and from the interface, it must have 
                succeeded in opening the interface. This establishes an exclusive link between 
                the client's task and the actual interface device. Opening the interface causes 
                pipes to be created on each endpoint contained in the interface. If the interface 
                contains isochronous endpoints, an attempt is made to allocate bandwidth on 
                the bus for each of those pipes. If there is not enough bandwidth available, 
                an isochronous pipe may be created with a bandwidth of zero. The software must 
                then call SetPipePolicy to change the size of that pipe before it can be used 
                for I/O.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @result     Returns kIOReturnExclusiveAccess if some other task has the device opened already,
                kIOReturnError if the connection with the kernel cannot be established or
                kIOReturnSuccess if successful.
	*/
	delegate IOReturn USBInterfaceOpen (IntPtr self);

	/*!
    @function USBInterfaceClose
    @abstract   Closes the task's connection to the IOUSBInterface.
    @discussion Releases the client's exclusive access to the IOUSBInterface.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn USBInterfaceClose (IntPtr self);

	/*!
    @function GetInterfaceClass
    @abstract   Returns the USB Class of the interface  (bInterfaceClass).
    @discussion The interface does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      intfClass Pointer to UInt8 to hold the interface Class.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetInterfaceClass (IntPtr self, out UInt8 intfClass);

	/*!
    @function GetInterfaceSubClass
    @abstract   Returns the USB Subclass of the interface (bInterfaceSubClass).
    @discussion The interface does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      intfSubClass Pointer to UInt8 to hold the interface Subclass.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetInterfaceSubClass (IntPtr self, out UInt8 intfSubClass);

	/*!
    @function GetInterfaceProtocol
    @abstract   Returns the USB Protocol of the interface (bInterfaceProtocol).
    @discussion The interface does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      intfProtocol Pointer to UInt8 to hold the interface Protocol.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetInterfaceProtocol (IntPtr self, out UInt8 intfProtocol);

	/*!
    @function GetDeviceVendor
    @abstract   Returns the USB Vendor ID (idVendor) of the device of which this interface is a part.
    @discussion The interface does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      devVendor Pointer to UInt16 to hold the vendorID.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
//	delegate IOReturn GetDeviceVendor (IntPtr self, out UInt16 devVendor);

	/*!
    @function GetDeviceProduct
    @abstract   Returns the USB Product ID (idProduct) of the device of which this interface is a part.
    @discussion The interface does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      devProduct Pointer to UInt16 to hold the ProductID.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
//	delegate IOReturn GetDeviceProduct (IntPtr self, out UInt16 devProduct);

	/*!
    @function GetDeviceReleaseNumber
    @abstract   Returns the Device Release Number (bcdDevice) of the device of which this interface is a part.
    @discussion The interface does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      devRelNum Pointer to UInt16 to hold the Release Number.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
//	delegate IOReturn GetDeviceReleaseNumber (IntPtr self, out UInt16 devRelNum);

	/*!
    @function GetConfigurationValue
    @abstract   Returns the current configuration value set in the device (the interface will be part of that configuration.)
    @discussion The interface does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      configVal Pointer to UInt8 to hold the configuration value.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetConfigurationValue (IntPtr self, out UInt8 configVal);

	/*!
    @function GetInterfaceNumber
    @abstract   Returns the interface number (zero-based index) of this interface within the current configuration of the device.
    @discussion The interface does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      intfNumber Pointer to UInt8 to hold the interface number.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetInterfaceNumber (IntPtr self, out UInt8 intfNumber);

	/*!
    @function GetAlternateSetting
    @abstract   Returns the alternate setting currently selected in this interface.
    @discussion The interface does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      intfAltSetting Pointer to UInt8 to hold the alternate setting value.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetAlternateSetting (IntPtr self, out UInt8 intfAltSetting);

	/*!
    @function GetNumEndpoints
    @abstract   Returns the number of endpoints in this interface.
    @discussion The interface does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      intfNumEndpoints Pointer to UInt8 to hold the number of endpoints.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetNumEndpoints (IntPtr self, out UInt8 intfNumEndpoints);

	/*!
    @function GetLocationID
    @abstract   Returns the location ID.
    @discussion The location ID is a 32 bit number which is unique among all USB devices in the system, and which 
                will not change on a system reboot unless the topology of the bus itself changes.  The interface 
                does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      locationID Pointer to UInt32 to hold the location ID.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
//	delegate IOReturn GetLocationID (IntPtr self, out UInt32 locationID);

	/*!
    @function GetDevice
    @abstract   Returns the device of which this interface is part.
    @discussion The interface does not have to be open to use this function. The returned device can be used to 
                create a CFPlugin to talk to the device.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      device Pointer to io_service_t to hold the result.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetDevice (IntPtr self, out io_service_t device);

	/*!
    @function SetAlternateInterface
    @abstract   Changes the AltInterface setting.
    @discussion The interface must be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      alternateSetting The new alternate setting for the interface.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
                or kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn SetAlternateInterface (IntPtr self, UInt8 alternateSetting);

	/*!
    @function GetBusFrameNumber
    @abstract   Gets the current frame number of the bus to which the interface and its device are attached.
    @discussion The interface does not have to be open to use this function.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      frame Pointer to UInt64 to hold the frame number.
    @param      atTime Pointer to an AbsoluteTime, which should be within 1ms of the time when the bus frame 
                number was attained.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
//	delegate IOReturn GetBusFrameNumber (IntPtr self, out UInt64 frame, out AbsoluteTime atTime);

	/*!
    @function ControlRequest
    @abstract   Sends a USB request on a control pipe.
    @discussion If the request is a standard request which will change the state of the device, the device must 
                be open, which means you should be using the IOUSBDeviceInterface for this command.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index of the control pipe to use. Use zero for the default control pipe on the device.
    @param      req Pointer to an IOUSBDevRequest containing the request.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
				kIOReturnAborted if the thread is interrupted before the call completes, 
                or kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn ControlRequest (IntPtr self, UInt8 pipeRef, ref IOUSBDevRequest req);

	/*!
    @function ControlRequestAsync
    @abstract   Sends an asynchronous USB request on a control pipe. 
    @discussion Use pipeRef=0 for the default device control pipe.  If the request is a standard request which will 
                change the state of the device, the device must be open, which means you should be using the 
                IOUSBDeviceInterface for this command.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index of the control pipe to use. Use zero for the default control pipe on the device.
    @param      req Pointer to an IOUSBDevRequest containing the request.
	 @param     callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the number of bytes that were actually transferred.
	 			A message addressed to this callback is posted to the Async 
                port upon completion.
    @param      refCon Arbitrary pointer which is passed as a parameter to the callback routine.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
                kIOReturnNotOpen if the interface is not open for exclusive access, or kIOUSBNoAsyncPortErr if no 
                Async port has been created for this interface.
	*/
	delegate IOReturn ControlRequestAsync (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPStruct)] IOUSBDevRequest req, IOAsyncCallback1 callback, IntPtr refCon);

	/*!
    @function GetPipeProperties
    @abstract   Gets the properties for a pipe.
    @discussion Once an interface is opened, all of the pipes in that interface get created by the kernel. The number
                of pipes can be retrieved by GetNumEndpoints. The client can then get the properties of any pipe 
                using an index of 1 to GetNumEndpoints. Pipe 0 is the default control pipe in the device.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      direction Pointer to an UInt8 to get the direction of the pipe.
    @param      number Pointer to an UInt8 to get the pipe number.
    @param      transferType Pointer to an UInt8 to get the transfer type of the pipe.
	@param      maxPacketSize Pointer to an UInt16 to get the maxPacketSize of the pipe. This maxPacketSize is the FULL maxPacketSize, which takes into account the multipler for HS Isoc pipes
				and the burst and the multiplier for SS Isoc pipes. It could also have been adjusted by SetPipePolicy.
    @param      interval Pointer to an UInt8 to get the interval for polling the pipe for data (in milliseconds).
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
                or kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn GetPipeProperties (IntPtr self, UInt8 pipeRef, out UInt8 direction, out UInt8 number, out UInt8 transferType, out UInt16 maxPacketSize, out UInt8 interval);

	/*!
    @function GetPipeStatus
    @abstract   Gets the current status of a pipe.
    @discussion The interface must be open for the pipe to exist.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @result     Returns kIOReturnNoDevice if there is no connection to an IOService, or kIOReturnNotOpen
                if the interface is not open for exclusive access. Otherwise, the status of the pipe is returned. 
                Returns kIOUSBPipeStalled if the pipe is stalled. See @link //apple_ref/C/instm/IOUSBInterfaceInterface/ClearPipeStall/ ClearPipeStall @/link 
                or @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link for
                more information.
	*/
	delegate IOReturn GetPipeStatus (IntPtr self, UInt8 pipeRef);

	/*!
    @function AbortPipe
    @abstract   Aborts any outstanding transactions on the pipe with status kIOReturnAborted.
    @discussion If there are outstanding asynchronous transactions on the pipe, the callbacks will happen. 
                Note that this command will also clear the halted bit on the endpoint
                in the controller, but will NOT clear the data toggle bit.  If you want to clear the data toggle bit as well, see @link //apple_ref/C/instm/IOUSBInterfaceInterface/ClearPipeStall/ ClearPipeStall @/link or 
                @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link for more information.  The interface must be open for the pipe to exist.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
                or kIOReturnNotOpen if the interface is not open for exclusive access.

	*/
	delegate IOReturn AbortPipe (IntPtr self, UInt8 pipeRef);

		/*!
    @function ResetPipe
    @abstract   Equivalent to ClearPipeStall.
    @discussion The interface must be open for the pipe to exist.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
                or kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn ResetPipe (IntPtr self, UInt8 pipeRef);

	/*!
    @function ClearPipeStall
    @abstract   Clears the halted bit and the data toggle bit on the pipe's endpoint in the controller.
    @discussion This function also returns any outstanding transactions on the pipe with status kIOUSBTransactionReturned.
                If there are outstanding asynchronous transactions on the pipe, the callbacks will happen. The data 
                toggle may need to be resynchronized. The driver may handle this by sending a ClearFeature(ENDPOINT_HALT) 
                to the default control pipe, specifying the device's endpoint for this pipe. See also 
                @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
                or kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn ClearPipeStall (IntPtr self, UInt8 pipeRef);

	/*!
    @function ReadPipe
    @abstract   Reads data on a <b>BULK IN</b> or an <b>INTERRUPT</b> pipe.
    @discussion The interface must be open for the pipe to exist.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints). 
    @param      buf Buffer to hold the data.
    @param      size On entry: a pointer to the size of the buffer pointed to by buf.
                On exit: a pointer to the number of bytes actually read from the device.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
                or kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn ReadPipe (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, ref UInt32 size);

	/*!
    @function WritePipe
    @abstract   Writes data on a <b>BULK OUT</b> or <b>INTERRUPT OUT</b> pipe.
    @discussion The interface must be open for the pipe to exist.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      buf Buffer to hold the data.
    @param      size The size of the data buffer.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
				kIOReturnAborted if the thread is interrupted before the call completes, 
                or kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn WritePipe (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size);

	/*!
    @function ReadPipeAsync
    @abstract   Performs an asynchronous read on a <b>BULK IN</b> or an <b>INTERRUPT</b> pipe.
    @discussion The interface must be open for the pipe to exist.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      buf Buffer to hold the data.
    @param      size The size of the buffer pointed to by buf.
	@param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the number of bytes that were actually read.
	 			A message addressed to this callback is posted to the Async 
                port upon completion.
    @param      refcon Arbitrary pointer which is passed as a parameter to the callback routine.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
				kIOReturnAborted if the thread is interrupted before the call completes, 
                or kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn ReadPipeAsync (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size, IOAsyncCallback1 callback, IntPtr refcon);

		/*!
    @function WritePipeAsync
    @abstract   Performs an asynchronous write on a <b>BULK OUT</b> or <b>INTERRUPT OUT</b> pipe.
    @discussion The interface must be open for the pipe to exist.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      buf Buffer to hold the data.
    @param      size The size of the buffer pointed to by buf.
    @param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the number of bytes that were actually written.
	 			A message addressed to this callback is posted to the Async 
                port upon completion.
    @param      refcon Arbitrary pointer which is passed as a parameter to the callback routine.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
                or kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn WritePipeAsync (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size, IOAsyncCallback1 callback, IntPtr refcon);

	/*!
    @function ReadIsochPipeAsync
    @abstract   Performs a read on an <b>ISOCHRONOUS</b> pipe.
    @discussion The interface must be open for the pipe to exist.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      buf Buffer to hold the data.
    @param      frameStart The bus frame number on which to start the read (obtained from GetBusFrameNumber).
    @param      numFrames The number of frames for which to transfer data.
    @param      frameList A pointer to an array of IOUSBIsocFrame structures describing the frames.
    @param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the framelist pointer, which can be used to associate the completion with a particular request.
	 			A message addressed to this callback is posted to the Async port upon completion.
    @param      refcon Arbitrary pointer which is passed as a parameter to the callback routine.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
                or kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn ReadIsochPipeAsync (IntPtr self, UInt8 pipeRef, IntPtr buf, UInt64 frameStart, UInt32 numFrames,
	                                      [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] IOUSBIsocFrame[] frameList,
	                                      IOAsyncCallback1 callback, IntPtr refcon);

	/*!
    @function WriteIsochPipeAsync
    @abstract   Performs an asynchronous write on an <b>ISOCHRONOUS</b> pipe.
    @discussion The interface must be open for the pipe to exist.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      buf Buffer to hold the data.
    @param      frameStart The bus frame number on which to start the write (obtained from GetBusFrameNumber).
    @param      numFrames The number of frames for which to transfer data.
    @param      frameList A pointer to an array of IOUSBIsocFrame structures describing the frames.
    @param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the framelist pointer, which can be used to associate the completion with a particular request.
	 			A message addressed to this callback is posted to the Async 
                port upon completion.
    @param      refcon Arbitrary pointer which is passed as a parameter to the callback routine.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
                or kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn WriteIsochPipeAsync (IntPtr self, UInt8 pipeRef, IntPtr buf, UInt64 frameStart, UInt32 numFrames,
	                                       [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] IOUSBIsocFrame[] frameList,
	                                       IOAsyncCallback1 callback, IntPtr refcon);

	/*!
    @interface IOUSBInterfaceInterface182
    @abstract   The object you use to access a USB device interface from user space, returned by the IOUSBFamily
                version 1.8.2 and above.
    @discussion The functions listed here include all of the functions defined for the IOUSBInterfaceInterface and
                some new functions that are available on Mac OS X version 10.0.4 and later.
    @super  IOUSBInterfaceInterface
	*/

	/*!
    @function ControlRequestTO
    @abstract   Sends a USB request on a control pipe.
    @discussion The IOUSBDevRequestTO structure allows the client to specify timeout values for this request.  If 
                the request is a standard request which will change the state of the device, the device must be open,
                which means you should be using the IOUSBDeviceInterface for this command.
    @availability This function is only available with IOUSBInterfaceInterface182 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index of the control pipe to use. Use zero for the default control pipe on the device.
    @param      req Pointer to an IOUSBDevRequestTO containing the request.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
				kIOReturnAborted if the thread is interrupted before the call completes, 
                kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn ControlRequestTO (IntPtr self, UInt8 pipeRef, ref IOUSBDevRequestTO req);

	/*!
    @function ControlRequestAsyncTO
    @abstract   Sends an asynchronous USB request on a control pipe.
    @discussion The IOUSBDevRequestTO structure allows the client to specify timeout values for this request. Use 
                pipeRef=0 for the default device control pipe.  If the request is a standard request which will 
                change the state of the device, the device must be open, which means you should be using the 
                IOUSBDeviceInterface for this command.
    @availability This function is only available with IOUSBInterfaceInterface182 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index of the control pipe to use. Use zero for the default control pipe on the device.
    @param      req Pointer to an IOUSBDevRequestTO containing the request.
    @param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the number of bytes that were actually transferred.
	 			A message addressed to this callback is posted to the Async 
                port upon completion.
    @param      refCon Arbitrary pointer which is passed as a parameter to the callback routine.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
                kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn ControlRequestAsyncTO (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPStruct)] IOUSBDevRequestTO req, IOAsyncCallback1 callback, IntPtr refCon);

	/*!
    @function ReadPipeTO
    @abstract   Performs a read on a <b>BULK IN</b> pipe, specifying timeout values.
    @discussion The interface must be open for the pipe to exist.
    
                If a timeout is specified and the request times out, the driver may need to resynchronize the data 
                toggle. See @link //apple_ref/C/instm/IOUSBInterfaceInterface/ClearPipeStall/ ClearPipeStall @/link 
                or @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link.

                Timeouts do not apply to interrupt pipes, so you should use the ReadPipe API to perform a read from 
                an interrupt pipe.
    @availability This function is only available with IOUSBInterfaceInterface182 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      buf Buffer to hold the data.
    @param      size Pointer to the size of the buffer pointed to by buf.
    @param      noDataTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if no 
                data is transferred in this amount of time, the request will be aborted and returned.
    @param      completionTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if 
                the entire request is not completed in this amount of time, the request will be aborted and returned.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
				kIOReturnAborted if the thread is interrupted before the call completes, or
                kIOReturnNotOpen if the interface is not open for exclusive access.  Returns kIOReturnBadArgument if timeout 
                values are specified for an interrupt pipe.  If an error is returned, the size parameter is not updated and the buffer will
				NOT contain any valid data.
	*/
	delegate IOReturn ReadPipeTO (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, ref UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout);

	/*!
    @function WritePipeTO
    @abstract   Performs a write on a <b>BULK OUT</b> pipe, with specified timeout values.
    @discussion The interface must be open for the pipe to exist.
    
                If a timeout is specified and the request times out, the driver may need to resynchronize the data 
                toggle. See @link //apple_ref/C/instm/IOUSBInterfaceInterface/ClearPipeStall/ ClearPipeStall @/link 
                or @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link.
    @availability This function is only available with IOUSBInterfaceInterface182 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      buf Buffer to hold the data.
    @param      size The size of the buffer pointed to by buf.
    @param      noDataTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if no 
                data is transferred in this amount of time, the request will be aborted and returned.
    @param      completionTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if 
                the entire request is not completed in this amount of time, the request will be aborted and returned.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
				kIOReturnAborted if the thread is interrupted before the call completes, or
                kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn WritePipeTO (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout);

	/*!
    @function ReadPipeAsyncTO
    @abstract   Performs an asynchronous read on a <b>BULK IN </b>pipe, with specified timeout values.
    @discussion The interface must be open for the pipe to exist.
    
                If a timeout is specified and the request times out, the driver may need to resynchronize the data 
                toggle. See @link //apple_ref/C/instm/IOUSBInterfaceInterface/ClearPipeStall/ ClearPipeStall @/link 
                or @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link.
                
                Timeouts do not apply to interrupt pipes, so you should use the ReadPipeAsync API to perform an 
                asynchronous read from an interrupt pipe.
    @availability This function is only available with IOUSBInterfaceInterface182 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      buf Buffer to hold the data.
    @param      size The size of the buffer pointed to by buf.
    @param      noDataTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if no 
                data is transferred in this amount of time, the request will be aborted and returned.
    @param      completionTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if 
                the entire request is not completed in this amount of time, the request will be aborted and returned.
    @param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the number of bytes that were actually read.
	 			A message addressed to this callback is posted to the Async port 
                upon completion.
    @param      refcon Arbitrary pointer which is passed as a parameter to the callback routine.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
                kIOReturnNotOpen if the interface is not open for exclusive access.  Returns kIOReturnBadArgument if timeout 
	 values are specified for an interrupt pipe.  If an error is returned, the size parameter is not updated and the buffer will
	 NOT contain any valid data.
	*/
	delegate IOReturn ReadPipeAsyncTO (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout, IOAsyncCallback1 callback, IntPtr refcon);

	/*!
    @function WritePipeAsyncTO
    @abstract   Performs an asynchronous write on a <b>BULK OUT</b> pipe, with specified timeout values.
    @discussion The interface must be open for the pipe to exist.
    
                If a timeout is specified and the request times out, the driver may need to resynchronize the data 
                toggle. See @link //apple_ref/C/instm/IOUSBInterfaceInterface/ClearPipeStall/ ClearPipeStall @/link 
                or @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link.
    @availability This function is only available with IOUSBInterfaceInterface182 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      buf Buffer to hold the data.
    @param      size The size of the buffer pointed to by buf.
    @param      noDataTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if no 
                data is transferred in this amount of time, the request will be aborted and returned.
    @param      completionTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if 
                the entire request is not completed in this amount of time, the request will be aborted and returned.
    @param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the number of bytes that were actually written.
	   			A message addressed to this callback is posted to the Async port 
                upon completion.
    @param      refcon Arbitrary pointer which is passed as a parameter to the callback routine.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
                kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn WritePipeAsyncTO (IntPtr self, UInt8 pipeRef, [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] buf, UInt32 size, UInt32 noDataTimeout, UInt32 completionTimeout, IOAsyncCallback1 callback, IntPtr refcon);

	/*!
    @function USBInterfaceGetStringIndex
    @abstract   Returns the string index in the interface descriptor.
    @discussion The interface does not have to be open to use this function.
    @availability This function is only available with IOUSBInterfaceInterface182 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      si Pointer to UInt8 to hold the string index.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn USBInterfaceGetStringIndex (IntPtr self, out UInt8 si);

	/*!
    @interface IOUSBInterfaceInterface183
    @abstract   The object you use to access a USB device interface from user space, returned by the IOUSBFamily
                version 1.8.3 and above.
    @discussion The functions listed here include all of the functions defined for the IOUSBInterfaceInterface, 
                IOUSBInterfaceInterface182, and some new functions that are available on Mac OS X version 10.1 
                and later.
    @super IOUSBInterfaceInterface182
	*/

	/*!
    @function USBInterfaceOpenSeize
    @abstract   Opens the IOUSBInterface for exclusive access.
    @discussion If another client has the device open, an attempt is made to get that client to close it before 
                returning.
                
                Before the client can issue commands that change the state of the device, it must have succeeded 
                in opening the device. This establishes an exclusive link between the clients task and the actual 
                device.
    @availability This function is only available with IOUSBInterfaceInterface183 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @result     Returns kIOReturnExclusiveAccess if some other task has the interface open already and refuses to 
                close it, kIOReturnError if the connection with the kernel cannot be established or kIOReturnSuccess
                if successful.
	*/
	delegate IOReturn USBInterfaceOpenSeize (IntPtr self);

	/*!
    @interface IOUSBInterfaceInterface190
    @abstract   The object you use to access a USB device interface from user space, returned by the IOUSBFamily
                version 1.9 and above.
    @discussion The functions listed here include all of the functions defined for the IOUSBInterfaceInterface, 
                IOUSBInterfaceInterface182, IOUSBInterfaceInterface183, and some new functions that are available 
                on Mac OS X version 10.2 and later.
    @super IOUSBInterfaceInterface183
	*/

	/*!
    @function ClearPipeStallBothEnds
    @abstract   Equivalent to ClearPipeStall.
    @discussion This function is equivalent to ClearPipeStall except that it also attempts to clear the halt and
                toggle bits on the device's endpoint for the pipe by sending a ClearFeature(ENDPOINT_HALT) to the 
                default control pipe in the device, specifying the endpoint for the pipe represented by pipeRef. For
                most devices, this resynchronizes the data toggle between the two endpoints to ensure that there is 
                no loss of data.
    @availability This function is only available with IOUSBInterfaceInterface190 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
                kIOReturnNotOpen if the interface is not open for exclusive access.
	*/
	delegate IOReturn ClearPipeStallBothEnds (IntPtr self, UInt8 pipeRef);

	/*!
    @function SetPipePolicy
    @abstract   Changes the amount of bandwidth of an isochronous pipe or interrupt pipe, or the polling interval of an interrupt pipe.
    @discussion A pipe may be made smaller or larger (up to the maxPacketSize specified in the endpoint descriptor).
                When an interface is first opened, all pipes are created with their descriptor-supplied maxPacketSize.
                For isochronous or interrupt pipes, if there is not enough bandwidth on the bus to allocate to the pipe, the pipe
                is created with a reserved bandwidth of zero. Any attempts to transfer data on a pipe with zero 
                bandwidth will result in a kIOReturnNoBandwidth error. The pipe must first be given some bandwidth 
                using this call.  This can also be used to return bandwidth for an isochronous or an interrupt pipe.  If the driver
				knows that the device will not be sending the maxPacketSize data, it can use this call to return that unused bandwidth to the
				system.  If an interrupt pipe wants to change the polling interval, it can do so with this call.
                
                The interface must be open for the pipe to exist.
    @availability This function is only available with IOUSBInterfaceInterface190 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      maxPacketSize The desired size for the isochronous or interrupt pipe. Valid values are 0 through the maxPacketSize 
                defined in the endpoint descriptor.   
	@param      maxInterval  the desired polling interval in milliseconds, up to a maximum of 128 ms.  The
				system can only poll devices powers of 2 (1, 2, 4, 8, 16, 32, 64, or 128 ms).  A value of 0 is illegal.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
                kIOReturnNotOpen if the interface is not open for exclusive access.  May also return kIOReturnNoBandwidth 
                if there is not enough bandwidth available on the bus, or kIOReturnBadArgument if the desired 
                maxPacketSize is outside of the allowed range.
	*/
	delegate IOReturn SetPipePolicy (IntPtr self, UInt8 pipeRef, UInt16 maxPacketSize, UInt8 maxInterval);

	/*!
    @function GetBandwidthAvailable
    @abstract   Returns the amount of bandwidth available on the bus for allocation to 
                isochronous pipes.  If the device is a high speed device, it will be the number of bytes per microframe (125 µsecs). If it is a full
				speed device, it will be the number of bytes per frame (1ms)
    @discussion This function is useful for determining the correct AltInterface setting as well as for using 
                SetPipePolicy.
                
                The interface does not have to be open to use this function.
    @availability This function is only available with IOUSBInterfaceInterface190 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      bandwidth Pointer to UInt32 to hold the amount of bandwidth available (in bytes per 1ms frame).
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetBandwidthAvailable (IntPtr self, out UInt32 bandwidth);

		/*!
    @function GetEndpointProperties
    @abstract   Returns the transfer type, max packet size, and interval of a specified endpoint, whether or not 
                the endpoint has a pipe currently established.
    @discussion This function may be useful for determining which alternate interface to select when trying to 
                balance bandwidth allocations among isochronous pipes.
                
                The interface does not have to be open to use this function.
    @availability This function is only available with IOUSBInterfaceInterface190 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      alternateSetting Specifies the alternate setting within the current interface.
    @param      endpointNumber Specifies the desired endpoint number.
    @param      direction Specifies the desired direction.
    @param      transferType Pointer to UInt8 to hold the endpoint's transfer type (kUSBControl, kUSBIsoc, etc).
    @param      maxPacketSize Pointer to UInt16 to hold the maxPacketSize of the endpoint.
    @param      interval Pointer to UInt8 to hold the polling interval for interrupt endpoints.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetEndpointProperties (IntPtr self, UInt8 alternateSetting, UInt8 endpointNumber, UInt8 direction, out UInt8 transferType, out UInt16 maxPacketSize, out UInt8 interval);

	/*!
    @interface IOUSBInterfaceInterface192
    @abstract   The object you use to access a USB device interface from user space, returned by the IOUSBFamily
                version 1.9.2 and above.
    @discussion The functions listed here include all of the functions defined for the IOUSBInterfaceInterface, 
                IOUSBInterfaceInterface182, IOUSBInterfaceInterface183, IOUSBInterfaceInterface190, and some new 
                functions that are available on Mac OS X version 10.2.3 and later.
    @super IOUSBInterfaceInterface190
	*/

	/*!
    @function LowLatencyReadIsochPipeAsync
    @abstract   Performs an asynchronous read on a isochronous pipe and updates the frame list at primary interrupt time.
    @discussion The LowLatencyReadIsochPipeAsync() and LowLatencyWriteIsochPipeAsync() 
                calls are analogous to ReadIsochPipeAsync() and WriteIsochPipeAsync(). They differ in that the frame
                list data is updated at <em>primary interrupt time</em>. This means that the client can inspect the
                frStatus and frActCount fields as soon as the transaction completes, without having to wait for the 
                callback to happen (depending on the value of updateFrequency). The callback will still happen when 
                all the frames have been received.
                
                The client can specify how often the USB stack should update the frame list data by specifying the 
                updateFrequency: this value can range from 0 - 8. If the value is between 1 and 8, the 
                frame list will be updated every updateFrequency milliseconds. If the value is 0, the 
                frame list will only be updated once all the frames in the transfer have been received. For example,
                consider a transfer with numFrames equal to 64. If the update frequency is 4, the frame 
                list data will be updated every 4 milliseconds. If the update frequency is 0, the frame list will 
                only be updated at the end of the transfer, after the 64 frames have been sent or received. The 
                difference between using an update frequency of 0 and using the non-low latency isoch calls is that 
                in the former case, the frame list will be updated at primary interrupt time, while in the latter, 
                it will be updated at secondary interrupt time. Regardless of the value of updateFrequency, 
                the frame list will <em>always</em> be updated on the last frame of a transfer.
                
                The rationale for adding this call is that because completion routines run on the USB Workloop, they 
                can be scheduled to run a number of milliseconds after the USB transfer has finished. This latency 
                is variable and depends on what other higher priority threads are running on the system. This latency 
                presents a problem for applications, such as audio processing, that depend on receiving data, 
                processing it, and sending it back out, and need to do this as fast as possible. Since applications 
                that use isochronous data know when the data should be available, they can look at the frame list at 
                the expected time and note the frActCount and frStatus (and frTimeStamp
                if needed) and determine how many valid bytes are in their data buffer and whether there was an 
                error. They can then access their data buffer and process the actual data.
                
                In order to update the frame list at primary interrupt time and to allow the client to see that 
                update, the frame list buffer needs to be shared between the kernel and user space. The same thing 
                applies to the data buffer. This is a difference between the low latency isoch calls and the regular 
                isoch calls. The LowLatencyCreateBuffer() call is used to pre-allocate the buffers. The 
                client <em>must</em> use that call to allocate the data and the frame list buffers. The client can 
                pass a portion of the buffer that was previously allocated. The USB stack will range-check the data 
                and frame list buffers to make sure they are within the ranges of the buffers previously allocated. 
                This allows the client, if it so desires, to allocate a large data buffer and pass portions of it to 
                the read or write calls. The same applies to the frame list buffers. Of course, the client can also 
                pre-allocate several data buffers and several frame list buffers and use those for each transfer. 
                Once the transfer completes, the buffers can be reused in subsequent calls. When all transfers are 
                finished, the client needs to call LowLatencyDestroyBuffer() for each buffer that was 
                created with LowLatencyCreateBuffer().
                
                The interface must be open for the pipe to exist. The buf pointer and the frameList 
                pointer need to be pre-allocated using LowLatencyCreateBuffer(). 
                After using them, they should be freed using LowLatencyDestroyBuffer().
    @availability This function is only available with IOUSBInterfaceInterface192 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      buf Buffer to hold the data, previously allocated with LowLatencyCreateBuffer() 
                using a kUSBLowLatencyReadBuffer type.
    @param      frameStart The bus frame number on which to start the read (obtained from GetBusFrameNumber).
    @param      numFrames The number of frames for which to transfer data.
    @param      updateFrequency Specifies how often, in milliseconds, the frame list data should be updated. Valid 
                range is 0 - 8. If 0, it means that the framelist should be updated at the end of the transfer.
    @param      frameList A pointer to an array of IOUSBLowLatencyIsocFrame structures describing the frames.
    @param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the framelist pointer, which can be used to associate the completion with a particular request.
	 			A message addressed to this callback is posted to 
                the Async port upon completion.
    @param      refcon Arbitrary pointer which is passed as a parameter to the callback routine.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
                kIOReturnNotOpen if the interface is not open for exclusive access.  Will return kIOUSBLowLatencyBufferNotPreviouslyAllocated 
                or kIOUSBLowLatencyFrameListNotPreviouslyAllocated if the buffer or the frameList were 
                not previously allocated using LowLatencyCreateBuffer().
	*/
	delegate IOReturn LowLatencyReadIsochPipeAsync (IntPtr self, UInt8 pipeRef, IntPtr buf, UInt64 frameStart, UInt32 numFrames, UInt32 updateFrequency,
	                                                [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] IOUSBLowLatencyIsocFrame[] frameList,
		                                            IOAsyncCallback1 callback, IntPtr refcon);

	/*!
    @function LowLatencyWriteIsochPipeAsync
    @abstract   Performs an asynchronous write on an isochronous pipe and updates the frame list at primary interrupt time.
    @discussion The LowLatencyReadIsochPipeAsync() and LowLatencyWriteIsochPipeAsync() 
                calls are analogous to ReadIsochPipeAsync() and WriteIsochPipeAsync(). 
                They differ in that the frame list data is updated at <em>primary interrupt time</em>. This means that 
                the client can inspect the frStatus and frActCount fields as soon as the 
                transaction completes, without having to wait for the callback to happen (depending on the value of 
                updateFrequency). The callback will still happen when the all the frames have been received.
                
                The client can specify how often the USB stack should update the frame list data by specifying the 
                updateFrequency: this value can range from 0 - 8. If the value is between 1 and 8, the 
                frame list will be updated every updateFrequency milliseconds. If the value is 0, the 
                frame list will only be updated once all the frames in the transfer have been received. For example, 
                consider a transfer with numFrames equal to 64. If the update frequency is 4, the frame 
                list data will be updated every 4 milliseconds. If the update frequency is 0, the frame list will 
                only be updated at the end of the transfer, after the 64 frames have been sent or received. The 
                difference between using an update frequency of 0 and using the non-low latency isoch calls is that 
                in the former case, the frame list will be updated at primary interrupt time, while in the latter, 
                it will be updated at secondary interrupt time. Regardless of the value of updateFrequency, 
                the frame list will <em>always</em> be updated on the last frame of a transfer.
                
                The rationale for adding this call is that because completion routines run on the USB Workloop, 
                they can be scheduled to run a number of milliseconds after the USB transfer has finished. This 
                latency is variable and depends on what other higher priority threads are running on the system. 
                This latency presents a problem for applications, such as audio processing, that depend on receiving 
                data, processing it, and sending it back out, and need to do this as fast as possible. Since applications 
                that use isochronous data know when the data should be available, they can look at the frame list at 
                the expected time and note the frActCount and frStatus (and frTimeStamp 
                if needed) and determine how many valid bytes are in their data buffer and whether there was an error. 
                They can then access their data buffer and process the actual data.
                
                In order to update the frame list at primary interrupt time and to allow the client to see that 
                update, the frame list buffer needs to be shared between the kernel and user space. The same thing 
                applies to the data buffer. This is a difference between the low latency isoch calls and the regular 
                isoch calls. The LowLatencyCreateBuffer() call is used to pre-allocate the buffers. The 
                <em>client</em> must use that call to allocate the data and the frame list buffers. The client can 
                pass a portion of the buffer that was previously allocated. The USB stack will range-check the data 
                and frame list buffers to make sure they are within the ranges of the buffers previously allocated. 
                This allows the client, if it so desires, to allocate a large data buffer and pass portions of it to 
                the read or write calls. The same applies to the frame list buffers. Of course, the client can also 
                pre-allocate several data buffers and several frame list buffers and use those for each transfer. 
                Once the transfer completes, the buffers can be reused in subsequent calls. When all transfers are 
                finished, the client needs to call LowLatencyDestroyBuffer() for each buffer that was 
                created with LowLatencyCreateBuffer().
                
                 The interface must be open for the pipe to exist. The buf pointer and the frameList 
                pointer need to be pre-allocated using LowLatencyCreateBuffer(). After using them, they 
                should be freed using LowLatencyDestroyBuffer().
    @availability This function is only available with IOUSBInterfaceInterface192 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
    @param      buf Buffer to hold the data, previously allocated with LowLatencyCreateBuffer() 
                using a kUSBLowLatencyWriteBuffer type.
    @param      frameStart The bus frame number on which to start the write (obtained from GetBusFrameNumber).
    @param      numFrames The number of frames for which to transfer data.
    @param      updateFrequency Specifies how often, in milliseconds, should the frame list data be updated. Valid 
                range is 0 - 8. If 0, it means that the framelist should be updated at the end of the transfer.
    @param      frameList A pointer to an array of IOUSBLowLatencyIsocFrame structures describing the frames.
    @param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the framelist pointer, which can be used to associate the completion with a particular request.
	 			A message addressed to this callback is posted to 
                the Async port upon completion.
    @param      refcon Arbitrary pointer which is passed as a parameter to the callback routine.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
                kIOReturnNotOpen if the interface is not open for exclusive access.  Will return kIOUSBLowLatencyBufferNotPreviouslyAllocated 
                or kIOUSBLowLatencyFrameListNotPreviouslyAllocated if the buffer or the frameList were 
                not previously allocated using LowLatencyCreateBuffer().
	*/
	delegate IOReturn LowLatencyWriteIsochPipeAsync (IntPtr self, UInt8 pipeRef, IntPtr buf, UInt64 frameStart, UInt32 numFrames, UInt32 updateFrequency,
	                                                 [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] IOUSBLowLatencyIsocFrame[] frameList,
		                                             IOAsyncCallback1 callback, IntPtr refcon);

	/*!
    @function LowLatencyCreateBuffer
    @abstract   Allocates a buffer of type bufferType.
    @discussion This function allocates a buffer of type bufferType. The buffer can then be used with 
                the LowLatencyIsochReadPipeAsync() or LowLatencyIsochWritePipeAsync() calls.
                
                The LowLatencyIsochReadPipeAsync() or LowLatencyIsochWritePipeAsync() calls 
                require the clients to pre-allocate the data buffer and the frame list buffer parameters. This call 
                is used to allocate those buffers. After the client is done using the buffers, they need to be 
                released through the LowLatencyDestroyBuffer() call.
                
                If the buffer is to be used for reading data, the type passed in should be kUSBLowLatencyReadBuffer.
                If the buffer is to be used for writing data, the type should be kUSBLowLatencyWriteBuffer. For
                frame list data, the type should be kUSBLowLatencyFrameListBuffer.
                
                The client can create multiple data and frame list buffers, or it can allocate a large buffer and 
                then use only a portion of the buffer in calls to LowLatencyReadIsochPipeAsync() 
                or LowLatencyWriteIsochPipeAsync().
                
                The interface must be open for the pipe to exist.
    @availability This function is only available with IOUSBInterfaceInterface192 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      buffer Pointer to a pointer that will receive the pointer to the buffer created by this call.
    @param      size The size of the buffer to be created in bytes.
    @param      bufferType Type of buffer: one of kUSBLowLatencyWriteBuffer, kUSBLowLatencyReadBuffer, 
                or kUSBLowLatencyFrameListBuffer. See the documentation for USB.h.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
                kIOReturnNotOpen if the interface is not open for exclusive access.  If the buffer can't be allocated, 
                it will return kIOReturnNoMemory.
	*/
	delegate IOReturn LowLatencyCreateBuffer (IntPtr self, out IntPtr buffer, IOByteCount size, UInt32 bufferType);

	/*!
    @function LowLatencyDestroyBuffer
    @abstract   Releases a buffer that was previously allocated using LowLatencyCreateBuffer().
    @discussion The interface must be open for the pipe to exist.
    @availability This function is only available with IOUSBInterfaceInterface192 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      buffer Pointer to the buffer previously allocated using LowLatencyCreateBuffer().
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
                kIOReturnNotOpen if the interface is not open for exclusive access.  If the buffer was not previously 
                allocated using LowLatencyCreateBuffer() it will return kIOReturnBadArgument. 
	*/
	delegate IOReturn LowLatencyDestroyBuffer (IntPtr self, IntPtr buffer);

	/*!
    @interface IOUSBInterfaceInterface197
    @abstract   The object you use to access a USB device interface from user space, returned by the IOUSBFamily
                version 1.9.7 and above.
    @discussion The functions listed here include all of the functions defined for the IOUSBInterfaceInterface, 
                IOUSBInterfaceInterface182, IOUSBInterfaceInterface183, IOUSBInterfaceInterface190, IOUSBInterfaceInterface192, 
                and some new functions that are available on Mac OS X version 10.2.5 and later.
    @super IOUSBInterfaceInterface192
	*/

	/*!
    @function GetBusMicroFrameNumber
    @abstract   Gets the current micro frame number of the bus to which the interface and its device are attached.
    @discussion The interface does not have to be open to use this function.
    @availability This function is only available with IOUSBInterfaceInterface197 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      microFrame Pointer to UInt64 to hold the microrame number.
    @param      atTime Pointer to an AbsoluteTime, which should be within 1ms of the time when the bus frame number 
                was attained.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	//	delegate IOReturn GetBusMicroFrameNumber (IntPtr self, oFor SuperSpeed interrupt and isoc endpoints, this is the wBytesPerInterval from the SuperSpeed Endpoint Companion Descriptor. For High Speed High Bandwidth isoc endpoints, this will be equal to wMaxPacketSize * (bMult+1).ut UInt64 microFrame, out AbsoluteTime atTime);

	/*!
    @function GetFrameListTime
    @abstract   Returns the number of microseconds in each USB Frame.
    @discussion This function can be used to determine whether the device is functioning in full speed or a high 
                speed. In the case of a full speed device, the returned value will be kUSBFullSpeedMicrosecondsInFrame.  
                In the case of a high speed device, the return value will be kUSBHighSpeedMicrosecondsInFrame.  
                (This API should really be called GetUSBFrameTime).
                
                The interface does not have to be open to use this function.
    @availability This function is only available with IOUSBInterfaceInterface197 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      microsecondsInFrame Pointer to UInt32 to hold the number of microseconds in each USB frame.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetFrameListTime (IntPtr self, out UInt32 microsecondsInFrame);

	/*!
    @function GetIOUSBLibVersion
    @abstract   Returns the version of the IOUSBLib and the version of the IOUSBFamily.
    @discussion The interface does not have to be open to use this function.
    @availability This function is only available with IOUSBInterfaceInterface197 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      ioUSBLibVersion Pointer to a NumVersion structure that on return will contain 
                the version of the IOUSBLib.
    @param      usbFamilyVersion Pointer to a NumVersion structure that on return will contain 
                the version of the IOUSBFamily.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
//	delegate IOReturn GetIOUSBLibVersion (IntPtr self, out NumVersion ioUSBLibVersion, out NumVersion usbFamilyVersion);

	/*!
     @function FindNextAssociatedDescriptor
     @abstract   Find the next descriptor of the requested type associated with the interface.
     @discussion The interface does not have to be open to use this function.
     @availability This function is only available with IOUSBInterfaceInterface220 and above.
     @param self Pointer to the IOUSBInterfaceInterface.
     @param currentDescriptor Descriptor to start searching from, NULL to start from beginning of list.
     @param descriptorType Descriptor type to search for, or kUSBAnyDesc to return any descriptor type.
     @result Pointer to the descriptor, or NULL if no matching descriptors found.
     */
	delegate /*IOUSBDescriptorHeader*/ IntPtr FindNextAssociatedDescriptor (IntPtr self, [In]IntPtr currentDescriptor, UInt8 descriptorType);

	/*!
     @function FindNextAltInterface
     @discussion return alternate interface descriptor satisfying the requirements specified in request, or NULL if there aren't any.
     discussion request is updated with the properties of the returned interface.
     @param self Pointer to the IOUSBInterfaceInterface.
     @param current interface descriptor to start searching from, NULL to start at alternate interface 0.
     @param request specifies what properties an interface must have to match.
     @result Pointer to a matching interface descriptor, or NULL if none match.
     */

	delegate /*IOUSBDescriptorHeader*/ IntPtr FindNextAltInterface (IntPtr self, [In]IntPtr current,
	                                                                [MarshalAs (UnmanagedType.LPStruct)] IOUSBFindInterfaceRequest request);

	/* IOUSBInterfaceStruct245 */

	/* IOUSBInterfaceStruct300 */

	/*!
    @function GetBusFrameNumberWithTime
    @abstract   Gets a recent frame number of the bus to which the device is attached, along with a system time corresponding to the start of that frame
    @discussion The device does not have to be open to use this function.
    @availability This function is only available with IOUSBInterfaceInterface300 and above.
    @param      self Pointer to the IOUSBInterfaceInterface.
    @param      frame Pointer to UInt64 to hold the frame number.
    @param      atTime Pointer to a returned AbsoluteTime, which is the system time ("wall time") as close as possible to the beginning of that USB frame. The jitter on this value may be as much as 200 microseconds.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or kIOReturnUnsupported is the bus doesn't support this function.
	*/
//	delegate IOReturn GetBusFrameNumberWithTime (IntPtr self, out UInt64 frame,
//	                                             out AbsoluteTime atTime);

	/* IOUSBInterfaceStruct500 */

	/*!
	 @function GetPipePropertiesV2
	 @abstract   This function is deprecated. See GetPipePropertiesV3
	 */
	delegate IOReturn GetPipePropertiesV2 (IntPtr self, UInt8 pipeRef, out UInt8 direction,
	                                       out UInt8 number, out UInt8 transferType,
	                                       out UInt16 maxPacketSize, out UInt8 interval,
	                                       out UInt8 maxBurst, out UInt8 mult,
	                                       out UInt16 bytesPerInterval);

	/* IOUSBInterfaceStruct550 */

	/*!
	 @function GetPipePropertiesV3
	 @abstract   Gets the different properties for a pipe.  This API uses a pointer to IOUSBEndpointProperties to return all the different properties.
	 @discussion Once an interface is opened, all of the pipes in that interface get created by the kernel. The number
	 of pipes can be retrieved by GetNumEndpoints. The client can then get the properties of any pipe
	 using an index of 1 to GetNumEndpoints. Pipe 0 is the default control pipe in the device.
	 @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
	 @param properties  pointer to a IOUSBEndpointProperties that will contain all the endpoint parameters.  Initialize the bVersion field with the appropriate version.  (See USBGetEndpointVersion in USB.h).  The bMaxStreams
	 field, if valid, is the actual number of streams that are supported for this pipe (e.g. it takes into account what the USB controller supports, as well as what the endpoint supports). The wMaxPacketSize is the 
	 current FULL maxPacketSize for this pipe, which includes both the mult and the burst. It may have been changed by SetPipePolicy, or it could be 0 as a result of a lack of bandwidth.
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
	 or kIOReturnNotOpen if the interface is not open for exclusive access.
	 */
	delegate IOReturn GetPipePropertiesV3 (IntPtr self, UInt8 pipeRef, ref IOUSBEndpointProperties properties);

	/*!
	 @function GetEndpointPropertiesV3
	 @abstract Returns the properties of an endpoint, possibly in an alternate interface, including any information from the SuperSpeed Companion Descriptor
	 @param properties  pointer to a IOUSBEndpointProperties that will contain all the endpoint parameters.  Initialize the bVersion field with the appropriate version.  (See USBGetEndpointVersion in USB.h).  Initialize
	       the bAlternateSetting, bEndpointNumber, and bDirection fields of the structure with the desired values for the endpoint.
	 @param properties  pointer to a IOUSBEndpointProperties that will contain all the endpoint parameters.  Initialize the bVersion field with the appropriate version.  (See USBGetEndpointVersion in USB.h).  You also NEED
		to initialize the bAlternateSetting, the bDirection (kUSBIn or kUSBOut), and the bEndPointNumber with the desired values for the endpoint. The bMaxStreams field, if valid, is the number of streams found in the Super Speed
		Companion Descriptor. The wMaxPacketSize is the BASE maxPacketSize as found in the endpoint descriptor, and has not been multiplied to take into account burst and mult.
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
	 or kIOReturnNotOpen if the interface is not open for exclusive access.
	 */
	delegate IOReturn GetEndpointPropertiesV3 (IntPtr self, ref IOUSBEndpointProperties properties);

	/*!
	 @function SupportsStreams
	 @abstract   Returns non zero if the pipe supports streams, nonZero for the maximum supported streams
	 @discussion The interface does not have to be open to use this function.
	 @param      self Pointer to the IOUSBInterfaceInterface.
	 @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
	 @param		 supportsStreams 0 if streams not supported, non-zero value indicates maximum stream number
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	 */
	delegate IOReturn SupportsStreams (IntPtr self, UInt8 pipeRef, out UInt32 supportsStreams);

	/*!
	 @function CreateStreams
	 @abstract   Creates the streams for the pipe
	 @discussion The interface does not have to be open to use this function.
	 @param      self Pointer to the IOUSBInterfaceInterface.
	 @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
	 @param		 streamID pass 0 if you want to destroy all streams, non-zero value indicates streamID of the highest stream to create
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	 */
	delegate IOReturn CreateStreams (IntPtr self, UInt8 pipeRef, UInt32 streamID);

	/*!
	 @function GetConfiguredStreams
	 @abstract   Get the number of streams which have been configured for the endpoint with CreateStreams
	 @discussion The interface does not have to be open to use this function.
	 @param      self Pointer to the IOUSBInterfaceInterface.
	 @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
	 @param		 configuredStreams  Number of streams that have been configured with CreateStreams
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	 */
	delegate IOReturn GetConfiguredStreams (IntPtr self, UInt8 pipeRef, out UInt32 configuredStreams);

	/*!
	 @function ReadStreamsPipeTO
	 @abstract   Performs a read on a stream in a <b>BULK IN</b> pipe, specifying timeout values.
	 @discussion The interface must be open for the pipe to exist.
	 
	 If a timeout is specified and the request times out, the driver may need to resynchronize the data
	 toggle. See @link //apple_ref/C/instm/IOUSBInterfaceInterface/ClearPipeStall/ ClearPipeStall @/link
	 or @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link.
	 
	 
	 Timeouts do not apply to interrupt pipes, so you should use the ReadPipe API to perform a read from
	 an interrupt pipe.
	 @availability This function is only available with IOUSBInterfaceInterface182 and above.
	 @param      self Pointer to the IOUSBInterfaceInterface.
	 @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
     @param 	 streamID ID of the stream to read from
	 @param      buf Buffer to hold the data.
	 @param      size Pointer to the size of the buffer pointed to by buf.
	 @param      noDataTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if no
	 data is transferred in this amount of time, the request will be aborted and returned.
	 @param      completionTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if
	 the entire request is not completed in this amount of time, the request will be aborted and returned.
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
	 kIOReturnAborted if the thread is interrupted before the call completes, or
	 kIOReturnNotOpen if the interface is not open for exclusive access.  Returns kIOReturnBadArgument if timeout
	 values are specified for an interrupt pipe.  If an error is returned, the size parameter is not updated and the buffer will
	 NOT contain any valid data.
	 */
	delegate IOReturn ReadStreamsPipeTO (IntPtr self, UInt8 pipeRef, UInt32 streamID,
	                                     [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] buf,
	                                     ref UInt32 size, UInt32 noDataTimeout,
	                                     UInt32 completionTimeout);

		/*!
	 @function WriteStreamsPipeTO
	 @abstract   Performs an write on a stream on a<b>BULK OUT</b> pipe, with specified timeout values.
	 @discussion The interface must be open for the pipe to exist.
	 
	 If a timeout is specified and the request times out, the driver may need to resynchronize the data
	 toggle. See @link //apple_ref/C/instm/IOUSBInterfaceInterface/ClearPipeStall/ ClearPipeStall @/link
	 or @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link.
	 @availability This function is only available with IOUSBInterfaceInterface182 and above.
	 @param      self Pointer to the IOUSBInterfaceInterface.
	 @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
     @param 	 streamID ID of the stream to write to
	 @param      buf Buffer to hold the data.
	 @param      size The size of the buffer pointed to by buf.
	 @param      noDataTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if no
	 data is transferred in this amount of time, the request will be aborted and returned.
	 @param      completionTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if
	 the entire request is not completed in this amount of time, the request will be aborted and returned.
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
	 kIOReturnAborted if the thread is interrupted before the call completes, or
	 kIOReturnNotOpen if the interface is not open for exclusive access.
	 */
	delegate IOReturn WriteStreamsPipeTO (IntPtr self, UInt8 pipeRef, UInt32 streamID,
	                                      [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] buf,
	                                      UInt32 size, UInt32 noDataTimeout,
	                                      UInt32 completionTimeout);

	/*!
	 @function ReadPipeAsyncTO
	 @abstract   Performs an asynchronous read on a stream on a <b>BULK IN </b>pipe, with specified timeout values.
	 @discussion The interface must be open for the pipe to exist.
	 
	 If a timeout is specified and the request times out, the driver may need to resynchronize the data
	 toggle. See @link //apple_ref/C/instm/IOUSBInterfaceInterface/ClearPipeStall/ ClearPipeStall @/link
	 or @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link.
	 
	 Timeouts do not apply to interrupt pipes, so you should use the ReadPipeAsync API to perform an
	 asynchronous read from an interrupt pipe.
	 @availability This function is only available with IOUSBInterfaceInterface182 and above.
	 @param      self Pointer to the IOUSBInterfaceInterface.
	 @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
     @param 	 streamID ID of the stream to read from
	 @param      buf Buffer to hold the data.
	 @param      size The size of the buffer pointed to by buf.
	 @param      noDataTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if no
	 data is transferred in this amount of time, the request will be aborted and returned.
	 @param      completionTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if
	 the entire request is not completed in this amount of time, the request will be aborted and returned.
	 @param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the number of bytes that were actually read.
	 A message addressed to this callback is posted to the Async port
	 upon completion.
	 @param      refcon Arbitrary pointer which is passed as a parameter to the callback routine.
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
	 kIOReturnNotOpen if the interface is not open for exclusive access.  Returns kIOReturnBadArgument if timeout
	 values are specified for an interrupt pipe.  If an error is returned, the size parameter is not updated and the buffer will
	 NOT contain any valid data.
	 */
	delegate IOReturn ReadStreamsPipeAsyncTO (IntPtr self, UInt8 pipeRef, UInt32 streamID,
	                                          [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] buf,
	                                          UInt32 size, UInt32 noDataTimeout,
	                                          UInt32 completionTimeout, IOAsyncCallback1 callback,
	                                          IntPtr refcon);

	/*!
	 @function WritePipeAsyncTO
	 @abstract   Performs an asynchronous write on a stream on a <b>BULK OUT</b> pipe, with specified timeout values.
	 @discussion The interface must be open for the pipe to exist.
	 
	 If a timeout is specified and the request times out, the driver may need to resynchronize the data
	 toggle. See @link //apple_ref/C/instm/IOUSBInterfaceInterface/ClearPipeStall/ ClearPipeStall @/link
	 or @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link.
	 @availability This function is only available with IOUSBInterfaceInterface182 and above.
	 @param      self Pointer to the IOUSBInterfaceInterface.
	 @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
     @param 	 streamID ID of the stream to write to
	 @param      buf Buffer to hold the data.
	 @param      size The size of the buffer pointed to by buf.
	 @param      noDataTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if no
	 data is transferred in this amount of time, the request will be aborted and returned.
	 @param      completionTimeout Specifies a time value in milliseconds. Once the request is queued on the bus, if
	 the entire request is not completed in this amount of time, the request will be aborted and returned.
	 @param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the number of bytes that were actually written.
	 A message addressed to this callback is posted to the Async port
	 upon completion.
	 @param      refcon Arbitrary pointer which is passed as a parameter to the callback routine.
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or
	 kIOReturnNotOpen if the interface is not open for exclusive access.
	 */
	delegate IOReturn WriteStreamsPipeAsyncTO (IntPtr self, UInt8 pipeRef, UInt32 streamID,
	                                           [MarshalAs (UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] buf,
	                                           UInt32 size, UInt32 noDataTimeout,
	                                           UInt32 completionTimeout, IOAsyncCallback1 callback,
	                                           IntPtr refcon);

	/*!
	 @function AbortStreamsPipe
	 @abstract   This method causes all outstanding I/O on a stream of a pipe to complete with return code kIOReturnAborted.
	 @discussion If there are outstanding asynchronous transactions on the pipe, the callbacks will happen.
	 Note that this command will also clear the halted bit on the endpoint
	 in the controller, but will NOT clear the data toggle bit.  If you want to clear the data toggle bit as well, see @link //apple_ref/C/instm/IOUSBInterfaceInterface/ClearPipeStall/ ClearPipeStall @/link or
	 @link //apple_ref/C/instm/IOUSBInterfaceInterface190/ClearPipeStallBothEnds/ ClearPipeStallBothEnds @/link for more information.  The interface must be open for the pipe to exist.
	 @param      self Pointer to the IOUSBInterfaceInterface.
	 @param      pipeRef Index for the desired pipe (1 - GetNumEndpoints).
     @param streamID ID of the stream to abort
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
	 or kIOReturnNotOpen if the interface is not open for exclusive access.	 
	 */
	delegate IOReturn AbortStreamsPipe (IntPtr self, UInt8 pipeRef, UInt32 streamID);
}

