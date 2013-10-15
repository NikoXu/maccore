//
// IOUSBDevice.cs
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
using MonoMac.CoreFoundation;
using MonoMac.Kernel.Mach;

using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using AbsoluteTime = System.UInt64;
using CFRunLoopSourceRef = System.IntPtr;
using IOUSBConfigurationDescriptorPtr = System.IntPtr;
using UInt8 = System.Byte;
using USBDeviceAddress = System.UInt16;
using io_iterator_t = System.IntPtr;
using mach_port_t = System.IntPtr;
using System.Threading.Tasks;
using System.Text;

namespace MonoMac.IOKit.USB
{
	public class IOUSBDevice : IOUSBNub
	{
		Lazy<IOCFPlugin<IOUSBDeviceUserClientType>> pluginInterface;
		Lazy<IIOCFPlugin<IOUSBDeviceInterface>> deviceInterface;

		Lazy<CFRunLoopSource> deviceAsyncEventSource;
		Lazy<Port> deviceAsyncPort;

		internal IOUSBDevice (IntPtr handle, bool owns) : base (handle, owns)
		{
			pluginInterface = new Lazy<IOCFPlugin<IOUSBDeviceUserClientType>>
				(() => IOCFPlugin.CreateInterfaceForService<IOUSBDeviceUserClientType> (this));

			var bundleVersion = IOUSB.BundleVersion;
			if (bundleVersion >= new Version ("5.5.0"))
				deviceInterface = new Lazy<IIOCFPlugin<IOUSBDeviceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBDeviceInterface550> ());
			else if (bundleVersion >= new Version ("3.2.0"))
				deviceInterface = new Lazy<IIOCFPlugin<IOUSBDeviceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBDeviceInterface320> ());
			else if (bundleVersion >= new Version ("3.0.0"))
				deviceInterface = new Lazy<IIOCFPlugin<IOUSBDeviceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBDeviceInterface300> ());
			else if (bundleVersion >= new Version ("2.4.5"))
				deviceInterface = new Lazy<IIOCFPlugin<IOUSBDeviceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBDeviceInterface245> ());
			else if (bundleVersion >= new Version ("1.9.7"))
				deviceInterface = new Lazy<IIOCFPlugin<IOUSBDeviceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBDeviceInterface197> ());
			else if (bundleVersion >= new Version ("1.8.7"))
				deviceInterface = new Lazy<IIOCFPlugin<IOUSBDeviceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBDeviceInterface187> ());
			else if (bundleVersion >= new Version ("1.8.2"))
				deviceInterface = new Lazy<IIOCFPlugin<IOUSBDeviceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBDeviceInterface182> ());
			else
				deviceInterface = new Lazy<IIOCFPlugin<IOUSBDeviceInterface>>
					(() =>	pluginInterface.Value.QueryInterface<IOUSBDeviceInterface> ());

			deviceAsyncEventSource = new Lazy<CFRunLoopSource> (CreateAsyncEventSource);
			deviceAsyncPort = new Lazy<Port> (CreateAsyncPort);
		}

		IntPtr DeviceInterfaceRef {
			get { return deviceInterface.Value.Handle; }
		}

		IOUSBDeviceInterface DeviceInterface {
			get { return deviceInterface.Value.Interface; }
		}

		public CFRunLoopSource AsyncEventSource {
			get { return deviceAsyncEventSource.Value; }
		}

		public Port AsyncPort {
			get { return deviceAsyncPort.Value; }
		}

		public DeviceClass Class {
			get {
				ThrowIfDisposed ();
				byte deviceClass;
				var result = DeviceInterface.GetDeviceClass (DeviceInterfaceRef, out deviceClass);
				IOObject.ThrowIfError (result);
				return (DeviceClass)deviceClass;
			}
		}

		public byte SubClass {
			get {
				ThrowIfDisposed ();
				byte deviceSubClass;
				var result = DeviceInterface.GetDeviceSubClass (DeviceInterfaceRef, out deviceSubClass);
				IOObject.ThrowIfError (result);
				return deviceSubClass;
			}
		}

		public InterfaceProtocol Protocol {
			get {
				ThrowIfDisposed ();
				byte protocol;
				var result = DeviceInterface.GetDeviceProtocol (DeviceInterfaceRef, out protocol);
				IOObject.ThrowIfError (result);
				return (InterfaceProtocol)protocol;
			}
		}

		public ushort VendorId {
			get {
				ThrowIfDisposed ();
				ushort vendor;
				var result = DeviceInterface.GetDeviceVendor (DeviceInterfaceRef, out vendor);
				IOObject.ThrowIfError (result);
				return vendor;
			}
		}

		public ushort ProductId {
			get {
				ThrowIfDisposed ();
				ushort product;
				var result = DeviceInterface.GetDeviceProduct (DeviceInterfaceRef, out product);
				IOObject.ThrowIfError (result);
				return product;
			}
		}

		public ushort ReleaseNumber {
			get {
				ThrowIfDisposed ();
				ushort releaseNumber;
				var result = DeviceInterface.GetDeviceReleaseNumber (DeviceInterfaceRef, out releaseNumber);
				// TODO: does this need to be translated from BDC?
				IOObject.ThrowIfError (result);
				return releaseNumber;
			}
		}

		public ushort Address {
			get {
				ThrowIfDisposed ();
				ushort address;
				var result = DeviceInterface.GetDeviceAddress (DeviceInterfaceRef, out address);
				IOObject.ThrowIfError (result);
				return address;
			}
		}

		public uint BusPowerAvailable {
			get {
				ThrowIfDisposed ();
				uint busPowerAvailible;
				var result = DeviceInterface.GetDeviceBusPowerAvailable (DeviceInterfaceRef, out busPowerAvailible);
				IOObject.ThrowIfError (result);
				return busPowerAvailible * 2;
			}
		}

		public DeviceSpeed Speed {
			get {
				ThrowIfDisposed ();
				byte speed;
				var result = DeviceInterface.GetDeviceSpeed (DeviceInterfaceRef, out speed);
				IOObject.ThrowIfError (result);
				return (DeviceSpeed)speed;
			}
		}

		public byte ConfigurationCount {
			get {
				ThrowIfDisposed ();
				byte count;
				var result = DeviceInterface.GetNumberOfConfigurations (DeviceInterfaceRef, out count);
				IOObject.ThrowIfError (result);
				return count;
			}
		}

		public uint LocationID {
			get {
				ThrowIfDisposed ();
				uint locationId;
				var result = DeviceInterface.GetLocationID (DeviceInterfaceRef, out locationId);
				IOObject.ThrowIfError (result);
				return locationId;
			}
		}

		public byte CurrentConfiguration {
			get {
				ThrowIfDisposed ();
				byte configuration;
				var result = DeviceInterface.GetConfiguration (DeviceInterfaceRef, out configuration);
				IOObject.ThrowIfError (result);
				return configuration;
			}
			set {
				ThrowIfDisposed ();
				var result = DeviceInterface.SetConfiguration (DeviceInterfaceRef, value);
				IOObject.ThrowIfError (result);
			}
		}

		[Since (0,4)]
		public string ManufacturerName {
			get {
				ThrowIfDisposed ();
				byte index;
				var result = DeviceInterface.USBGetManufacturerStringIndex (DeviceInterfaceRef, out index);
				IOObject.ThrowIfError (result);
				return GetStringDescriptor (index);
			}
		}

		[Since (0,4)]
		public string ProductName {
			get {
				ThrowIfDisposed ();
				byte index;
				var result = DeviceInterface.USBGetProductStringIndex (DeviceInterfaceRef, out index);
				IOObject.ThrowIfError (result);
				return GetStringDescriptor (index);
			}
		}

		[Since (0,4)]
		public string SerialNumber {
			get  {
				ThrowIfDisposed ();
				byte index;
				var result = DeviceInterface.USBGetSerialNumberStringIndex (DeviceInterfaceRef, out index);
				IOObject.ThrowIfError (result);
				return GetStringDescriptor (index);
			}
		}

		[Since (2,3)]
		public NumVersion IOUSBLibVersion {
			get  {
				ThrowIfDisposed ();
				NumVersion ioUSBLibVersion, usbFamilyVersion;
				var result = DeviceInterface.GetIOUSBLibVersion (DeviceInterfaceRef, out ioUSBLibVersion, out usbFamilyVersion);
				IOObject.ThrowIfError (result);
				return ioUSBLibVersion;
			}
		}

		[Since (2,3)]
		public NumVersion IOUSBFamilyVersion {
			get  {
				ThrowIfDisposed ();
				NumVersion ioUSBLibVersion, usbFamilyVersion;
				var result = DeviceInterface.GetIOUSBLibVersion (DeviceInterfaceRef, out ioUSBLibVersion, out usbFamilyVersion);
				IOObject.ThrowIfError (result);
				return usbFamilyVersion;
			}
		}

		[Since (5,4)]
		public DeviceInformation Information {
			get  {
				ThrowIfDisposed ();
				uint info;
				var result = DeviceInterface.GetUSBDeviceInformation (DeviceInterfaceRef, out info);
				IOObject.ThrowIfError (result);
				return (DeviceInformation)info;
			}
		}

		[Since (7,3)]
		public uint BandwidthAvailable {
			get  {
				ThrowIfDisposed ();
				uint bandwidth;
				var result = DeviceInterface.GetBandwidthAvailableForDevice (DeviceInterfaceRef, out bandwidth);
				IOObject.ThrowIfError (result);
				return bandwidth;
			}
		}

		public CFRunLoopSource CreateAsyncEventSource ()
		{
			ThrowIfDisposed ();
			IntPtr runLoopSourceRef;
			var result = DeviceInterface.CreateDeviceAsyncEventSource (DeviceInterfaceRef, out runLoopSourceRef);
			IOObject.ThrowIfError (result);
			var runLoopSource = new CFRunLoopSource (runLoopSourceRef, false);
			CFType.Release (runLoopSourceRef);
			return runLoopSource;
		}

		public Port CreateAsyncPort ()
		{
			ThrowIfDisposed ();
			IntPtr portRef;
			var result = DeviceInterface.CreateDeviceAsyncPort (DeviceInterfaceRef, out portRef);
			IOObject.ThrowIfError (result);
			return new Port (portRef);
		}

		public void Open ()
		{
			ThrowIfDisposed ();
			var result = DeviceInterface.USBDeviceOpen (DeviceInterfaceRef);
			IOObject.ThrowIfError (result);
		}

		public void Close ()
		{
			ThrowIfDisposed ();
			var result = DeviceInterface.USBDeviceClose (DeviceInterfaceRef);
			IOObject.ThrowIfError (result);
		}

		public IOUSBConfigurationDescriptor GetConfigurationDescriptor (byte index) {
			ThrowIfDisposed ();
			IntPtr configDescriptorRef;
			var result = DeviceInterface.GetConfigurationDescriptorPtr (DeviceInterfaceRef, index , out configDescriptorRef);
			IOObject.ThrowIfError (result);
			return (IOUSBConfigurationDescriptor)Marshal.PtrToStructure (configDescriptorRef, typeof(IOUSBConfigurationDescriptor));
		}

		public string GetStringDescriptor (byte index) {
			ThrowIfDisposed ();
			if (index == 0)
				return null;
			// based on http://oroboro.com/usb-serial-number-osx/
			var buffer = Marshal.AllocHGlobal (64);
			try {
				var request = new IOUSBDevRequest () {
					Direction = EndpointDirection.In,
					DeviceRequestType = DeviceRequestType.Standard,
					Recipient = DeviceRequestRecipient.Device,
					RequestType = RequestType.RqGetDescriptor,
					Value = (ushort)((byte)DescriptorType.String << 8 | index),
					Index = 0x409, // english
					DataLength = 64,
					Data = buffer
				};
				var result = DeviceInterface.DeviceRequest (DeviceInterfaceRef, ref request);
				var header = (IOUSBDescriptorHeader)Marshal.PtrToStructure (buffer, typeof(IOUSBDescriptorHeader));
				return Marshal.PtrToStringUni (buffer + 2, (header.Length - 1) / 2);
			} finally {
				Marshal.FreeHGlobal (buffer);
			}
		}

		public ulong GetBusFrameNumber (out ulong atTime) {
			ThrowIfDisposed ();
			ulong frame;
			var result = DeviceInterface.GetBusFrameNumber (DeviceInterfaceRef, out frame, out atTime);
			IOObject.ThrowIfError (result);
			return (ulong)IOUSB.USBToHostOrder ((long)frame);
		}

		public void Reset ()
		{
			ThrowIfDisposed ();
			var result = DeviceInterface.ResetDevice (DeviceInterfaceRef);
			IOObject.ThrowIfError (result);
		}

		public void SendRequest (ref IOUSBDevRequest request)
		{
			ThrowIfDisposed ();
			var result = DeviceInterface.DeviceRequest (DeviceInterfaceRef, ref request);
			IOObject.ThrowIfError (result);
		}

		public Task<int> SendRequestAsync (IOUSBDevRequest request)
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
			var result = DeviceInterface.DeviceRequestAsync (DeviceInterfaceRef, request, callback, IntPtr.Zero);
			IOObject.ThrowIfError (result);
			return completionSource.Task;
		}

		public IOIterator<IOUSBInterface> CreateInterfaceIterator (IOUSBFindInterfaceRequest request) {
			ThrowIfDisposed ();
			IntPtr iteratorRef;
			var result = DeviceInterface.CreateInterfaceIterator (DeviceInterfaceRef, request, out iteratorRef);
			IOObject.ThrowIfError (result);
			return new IOIterator<IOUSBInterface> (iteratorRef, true);
		}

		[Since (0,4)]
		public void OpenSeize ()
		{
			ThrowIfDisposed ();
			var result = DeviceInterface.USBDeviceOpenSeize (DeviceInterfaceRef);
			IOObject.ThrowIfError (result);
		}

		[Since (0,4)]
		public void RequestTimeout (ref IOUSBDevRequestTO request)
		{
			ThrowIfDisposed ();
			var result = DeviceInterface.DeviceRequestTO (DeviceInterfaceRef, ref request);
			IOObject.ThrowIfError (result);
		}

		[Since (0,4)]
		public Task<int> RequestTimeoutAsync (IOUSBDevRequestTO request)
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
			var result = DeviceInterface.DeviceRequestAsyncTO (DeviceInterfaceRef, request, callback, IntPtr.Zero);
			IOObject.ThrowIfError (result);
			return completionSource.Task;
		}

		[Since (0,4)]
		public void Suspend (bool suspend)
		{
			ThrowIfDisposed ();
			var result = DeviceInterface.USBDeviceSuspend (DeviceInterfaceRef, suspend);
			IOObject.ThrowIfError (result);
		}

		[Since (0,4)]
		public void AbortPipeZero ()
		{
			ThrowIfDisposed ();
			var result = DeviceInterface.USBDeviceAbortPipeZero (DeviceInterfaceRef);
			IOObject.ThrowIfError (result);
		}

		[Since (1,2)]
		public void ReEnumerate (uint options)
		{
			ThrowIfDisposed ();
			var result = DeviceInterface.USBDeviceReEnumerate (DeviceInterfaceRef, options);
			IOObject.ThrowIfError (result);
		}

		[Since (2,3)]
		public ulong GetBusMicroFrameNumber (out ulong atTime) {
			ThrowIfDisposed ();
			ulong frame;
			var result = DeviceInterface.GetBusMicroFrameNumber (DeviceInterfaceRef, out frame, out atTime);
			IOObject.ThrowIfError (result);
			return (ulong)IOUSB.USBToHostOrder ((long)frame);
		}

		[Since (5,0)]
		public ulong GetBusFrameNumberWithTime (out AbsoluteTime atTime) {
			ThrowIfDisposed ();
			ulong frame;
			var result = DeviceInterface.GetBusFrameNumberWithTime (DeviceInterfaceRef, out frame, out atTime);
			IOObject.ThrowIfError (result);
			return (ulong)IOUSB.USBToHostOrder ((long)frame);
		}

		[Since (5,4)]
		public uint RequestExtraPower (PowerRequestType type, uint requestedPower) {
			ThrowIfDisposed ();
			uint powerAvailable;
			var result = DeviceInterface.RequestExtraPower (DeviceInterfaceRef, (uint)type, requestedPower, out powerAvailable);
			IOObject.ThrowIfError (result);
			return powerAvailable;
		}

		[Since (5,4)]
		public void ReturnExtraPower (PowerRequestType type, uint powerReturned) {
			ThrowIfDisposed ();
			var result = DeviceInterface.ReturnExtraPower (DeviceInterfaceRef, (uint)type, powerReturned);
			IOObject.ThrowIfError (result);
		}

		[Since (5,4)]
		public uint ExtraPowerAllocated (PowerRequestType type) {
			ThrowIfDisposed ();
			uint powerAllocated;
			var result = DeviceInterface.GetExtraPowerAllocated (DeviceInterfaceRef, (uint)type, out powerAllocated);
			IOObject.ThrowIfError (result);
			return powerAllocated;
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBDevRequest
	{
		UInt8       bmRequestType;
		UInt8       bRequest;
		UInt16      wValue;
		UInt16      wIndex;
		UInt16      wLength;
		IntPtr      pData;
		UInt32      wLenDone;

		public EndpointDirection Direction {
			get {
				return (EndpointDirection)(bmRequestType >> 7 & 0x01);
			}
			set {
				bmRequestType &= 0x7F;
				bmRequestType = (byte)(bmRequestType | (((int)value & 0x01) << 7));
			}
		}

		public DeviceRequestRecipient Recipient {
			get {
					return (DeviceRequestRecipient)(bmRequestType >> 5 & 0x03);
			}
			set {
				bmRequestType &= 0x9F;
				bmRequestType = (byte)(bmRequestType | (((int)value & 0x03) << 5));
			}
		}

		public DeviceRequestType DeviceRequestType {
			get {
				return (DeviceRequestType)(bmRequestType & 0x1F);
			}
			set {
				bmRequestType &= 0xE0;
				bmRequestType = (byte)(bmRequestType | (((int)value & 0x1F)));
			}
		}

		public RequestType RequestType {
			get {
				return (RequestType)bRequest;
			}
			set {
				bRequest = (byte)value;
			}
		}

		public ushort Value {
			get { return wValue; }
			set { wValue = value; }
		}

		public ushort Index {
			get { return wIndex; }
			set { wIndex = value; }
		}

		public int DataLength {
			get { return (int)wLength; }
			set { wLength = (ushort)value; }
		}

		public IntPtr Data {
			get { return pData; }
			set { pData = value; }
		}

		public int DataLengthOnReturn {
			get { return (int)wLenDone; }
				set { wLenDone = (uint)value; }
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBFindInterfaceRequest
	{
		public const ushort DontCare = 0xFFFF;

		InterfaceClass interfaceClass;
		InterfaceSubClass interfaceSubClass;
		InterfaceProtocol interfaceProtocol;
		UInt16 alternateSetting;

		public InterfaceClass InterfaceClass { get { return interfaceClass; } }

		public InterfaceSubClass InterfaceSubClass { get { return interfaceSubClass; } }

		public InterfaceProtocol InterfaceProtocol { get { return interfaceProtocol; } }

		public UInt16 AlternateSetting { get { return alternateSetting; } }

		public IOUSBFindInterfaceRequest (InterfaceClass interfaceClass = (InterfaceClass)DontCare,
		                                  InterfaceSubClass interfaceSubClass = (InterfaceSubClass)DontCare,
		                                  InterfaceProtocol interfaceProtocol = (InterfaceProtocol)DontCare,
		                                  ushort alternateSetting = DontCare)
		{
			this.interfaceClass = interfaceClass;
			this.interfaceSubClass = interfaceSubClass;
			this.interfaceProtocol = interfaceProtocol;
			this.alternateSetting = alternateSetting;
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBDevRequestTO
	{
		public DeviceRequestType RequestType;
		public UInt8             Request;
		public UInt16            Value;
		public UInt16            Index;
		public UInt16            Length;
		public IntPtr            Data;
		public UInt32            LenDone;
		public UInt32            DataTimeout;
		public UInt32            completionTimeout;
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct NumVersion
	{
		public UInt8 nonRelRev;
		public UInt8 stage;
		public UInt8 minorAndBugRev;
		public UInt8 majorRev;
	}

	[Guid ("9dc7b780-9ec0-11d4-a54f-000a27052861")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceUserClientType : IOCFPlugInInterface
	{
	}

	[Guid ("5c8187d0-9ef3-11d4-8b45-000a27052861")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface : IUnknown
	{
		CreateDeviceAsyncEventSource createDeviceAsyncEventSource;
		GetDeviceAsyncEventSource getDeviceAsyncEventSource;
		CreateDeviceAsyncPort createDeviceAsyncPort;
		GetDeviceAsyncPort getDeviceAsyncPort;
		USBDeviceOpen usbDeviceOpen;
		USBDeviceClose usbDeviceClose;
		GetDeviceClass getDeviceClass;
		GetDeviceSubClass getDeviceSubClass;
		GetDeviceProtocol getDeviceProtocol;
		GetDeviceVendor getDeviceVendor;
		GetDeviceProduct getDeviceProduct;
		GetDeviceReleaseNumber getDeviceReleaseNumber;
		GetDeviceAddress getDeviceAddress;
		GetDeviceBusPowerAvailable getDeviceBusPowerAvailable;
		GetDeviceSpeed getDeviceSpeed;
		GetNumberOfConfigurations getNumberOfConfigurations;
		GetLocationID getLocationID;
		GetConfigurationDescriptorPtr getConfigurationDescriptorPtr;
		GetConfiguration getConfiguration;
		SetConfiguration setConfiguration;
		GetBusFrameNumber getBusFrameNumber;
		ResetDevice resetDevice;
		DeviceRequest deviceRequest;
		DeviceRequestAsync deviceRequestAsync;
		CreateInterfaceIterator createInterfaceIterator;

		public CreateDeviceAsyncEventSource CreateDeviceAsyncEventSource {
			get { return createDeviceAsyncEventSource; }
		}

		public GetDeviceAsyncEventSource GetDeviceAsyncEventSource {
			get { return getDeviceAsyncEventSource; }
		}

		public CreateDeviceAsyncPort CreateDeviceAsyncPort {
			get { return createDeviceAsyncPort; }
		}

		public GetDeviceAsyncPort GetDeviceAsyncPort {
			get { return getDeviceAsyncPort; }
		}

		public USBDeviceOpen USBDeviceOpen {
			get { return usbDeviceOpen; }
		}

		public USBDeviceClose USBDeviceClose {
			get { return usbDeviceClose; }
		}

		public GetDeviceClass GetDeviceClass {
			get { return getDeviceClass; }
		}

		public GetDeviceSubClass GetDeviceSubClass{
			get { return getDeviceSubClass; }
		}
		public GetDeviceProtocol GetDeviceProtocol {
			get { return getDeviceProtocol; }
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

		public GetDeviceAddress GetDeviceAddress {
			get { return getDeviceAddress; }
		}

		public GetDeviceBusPowerAvailable GetDeviceBusPowerAvailable {
			get { return getDeviceBusPowerAvailable; }
		}

		public GetDeviceSpeed GetDeviceSpeed {
			get { return getDeviceSpeed; }
		}

		public GetNumberOfConfigurations GetNumberOfConfigurations {
			get { return getNumberOfConfigurations; }
		}

		public GetLocationID GetLocationID {
			get { return getLocationID; }
		}

		public GetConfigurationDescriptorPtr GetConfigurationDescriptorPtr {
			get { return getConfigurationDescriptorPtr; }
		}

		public GetConfiguration GetConfiguration {
			get { return getConfiguration; }
		}

		public SetConfiguration SetConfiguration {
			get { return setConfiguration; }
		}

		public GetBusFrameNumber GetBusFrameNumber {
			get { return getBusFrameNumber; }
		}

		public ResetDevice ResetDevice {
			get { return resetDevice; }
		}

		public DeviceRequest DeviceRequest {
			get { return deviceRequest; }
		}

		public DeviceRequestAsync DeviceRequestAsync {
			get { return deviceRequestAsync; }
		}

		public CreateInterfaceIterator CreateInterfaceIterator {
			get { return createInterfaceIterator; }
		}

		public virtual USBDeviceOpenSeize USBDeviceOpenSeize {
			get { throw new NotImplementedException (); }
		}

		public virtual DeviceRequestTO DeviceRequestTO {
			get { throw new NotImplementedException (); }
		}

		public virtual DeviceRequestAsyncTO DeviceRequestAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual USBDeviceSuspend USBDeviceSuspend {
			get { throw new NotImplementedException (); }
		}

		public virtual USBDeviceAbortPipeZero USBDeviceAbortPipeZero {
			get { throw new NotImplementedException (); }
		}

		public virtual USBGetManufacturerStringIndex USBGetManufacturerStringIndex {
			get { throw new NotImplementedException (); }
		}

		public virtual USBGetProductStringIndex USBGetProductStringIndex {
			get { throw new NotImplementedException (); }
		}

		public virtual USBGetSerialNumberStringIndex USBGetSerialNumberStringIndex {
			get { throw new NotImplementedException (); }
		}

		public virtual USBDeviceReEnumerate USBDeviceReEnumerate {
			get { throw new NotImplementedException (); }
		}

		public virtual GetBusMicroFrameNumber GetBusMicroFrameNumber {
			get { throw new NotImplementedException (); }
		}

		public virtual GetIOUSBLibVersion GetIOUSBLibVersion {
			get { throw new NotImplementedException (); }
		}

		public virtual GetBusFrameNumberWithTime GetBusFrameNumberWithTime {
			get { throw new NotImplementedException (); }
		}

		public virtual GetUSBDeviceInformation GetUSBDeviceInformation {
			get { throw new NotImplementedException (); }
		}

		public virtual RequestExtraPower RequestExtraPower {
			get { throw new NotImplementedException (); }
		}

		public virtual ReturnExtraPower ReturnExtraPower {
			get { throw new NotImplementedException (); }
		}

		public virtual GetExtraPowerAllocated GetExtraPowerAllocated {
			get { throw new NotImplementedException (); }
		}

		public virtual GetBandwidthAvailableForDevice GetBandwidthAvailableForDevice {
			get { throw new NotImplementedException (); }
		}
	}

	[Guid ("152FC496-4891-11D5-9D52-000A27801E86")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface182 : IOUSBDeviceInterface
	{
		USBDeviceOpenSeize usbDeviceOpenSeize;
		DeviceRequestTO deviceRequestTO;
		DeviceRequestAsyncTO deviceRequestAsyncTO;
		USBDeviceSuspend usbDeviceSuspend;
		USBDeviceAbortPipeZero usbeviceAbortPipeZero;
		USBGetManufacturerStringIndex usbGetManufacturerStringIndex;
		USBGetProductStringIndex usbGetProductStringIndex;
		USBGetSerialNumberStringIndex usbGetSerialNumberStringIndex;

		public override USBDeviceOpenSeize USBDeviceOpenSeize {
			get { return usbDeviceOpenSeize; }
		}

		public override DeviceRequestTO DeviceRequestTO {
			get { return deviceRequestTO; }
		}

		public override DeviceRequestAsyncTO DeviceRequestAsyncTO {
			get { return deviceRequestAsyncTO; }
		}

		public override USBDeviceSuspend USBDeviceSuspend {
			get { return usbDeviceSuspend; }
		}

		public override USBDeviceAbortPipeZero USBDeviceAbortPipeZero {
			get { return usbeviceAbortPipeZero; }
		}

		public override USBGetManufacturerStringIndex USBGetManufacturerStringIndex {
			get { return usbGetManufacturerStringIndex; }
		}

		public override USBGetProductStringIndex USBGetProductStringIndex {
			get { return usbGetProductStringIndex; }
		}

		public override USBGetSerialNumberStringIndex USBGetSerialNumberStringIndex {
			get { return usbGetSerialNumberStringIndex; }
		}
	}

	[Guid ("3C9EE1EB-2402-11B2-8E7E-000A27801E86")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface187 : IOUSBDeviceInterface182
	{
		USBDeviceReEnumerate usbDeviceReEnumerate;

		public override USBDeviceReEnumerate USBDeviceReEnumerate {
			get { return usbDeviceReEnumerate; }
		}
	}

	[Guid ("C809B8D8-0884-11D7-BB96-0003933E3E3E")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface197 : IOUSBDeviceInterface187
	{
		GetBusMicroFrameNumber getBusMicroFrameNumber;
		GetIOUSBLibVersion getIOUSBLibVersion;

		public override GetBusMicroFrameNumber GetBusMicroFrameNumber {
			get { return getBusMicroFrameNumber; }
		}

		public override GetIOUSBLibVersion GetIOUSBLibVersion {
			get { return getIOUSBLibVersion; }
		}
	}

	[Guid ("FE2FD52F-3B5A-473B-978B-AD99001EB3ED")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface245 : IOUSBDeviceInterface197
	{
	}

	[Guid ("396104F7-943D-4893-90F1-69BD6CF5C2EB")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface300 : IOUSBDeviceInterface245
	{
		GetBusFrameNumberWithTime getBusFrameNumberWithTime;

		public override GetBusFrameNumberWithTime GetBusFrameNumberWithTime {
			get { return getBusFrameNumberWithTime; }
		}
	}

	[Guid ("01A2D0E9-42F6-4A87-8B8B-77057C8CE0CE")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface320 : IOUSBDeviceInterface300
	{
		GetUSBDeviceInformation getUSBDeviceInformation;
		RequestExtraPower requestExtraPower;
		ReturnExtraPower returnExtraPower;
		GetExtraPowerAllocated getExtraPowerAllocated;

		public override GetUSBDeviceInformation GetUSBDeviceInformation {
			get { return getUSBDeviceInformation; }
		}

		public override RequestExtraPower RequestExtraPower {
			get { return requestExtraPower; }
		}

		public override ReturnExtraPower ReturnExtraPower {
			get { return returnExtraPower; }
		}

		public override GetExtraPowerAllocated GetExtraPowerAllocated {
			get { return getExtraPowerAllocated; }
		}
	}

	[Guid ("A33CF047-4B5B-48E2-B57D-0207FCEAE13B")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface550 : IOUSBDeviceInterface320
	{
		GetBandwidthAvailableForDevice getBandwidthAvailableForDevice;
		
		public override GetBandwidthAvailableForDevice GetBandwidthAvailableForDevice {
			get { return getBandwidthAvailableForDevice; }
		}
	}

	delegate IOReturn CreateDeviceAsyncEventSource (IntPtr self, out CFRunLoopSourceRef source);

	delegate CFRunLoopSourceRef GetDeviceAsyncEventSource (IntPtr self);

	delegate IOReturn CreateDeviceAsyncPort (IntPtr self, out mach_port_t port); 

	delegate mach_port_t GetDeviceAsyncPort (IntPtr self);

	delegate IOReturn USBDeviceOpen (IntPtr self);

	delegate IOReturn USBDeviceClose (IntPtr self);

	delegate IOReturn GetDeviceClass (IntPtr self, out UInt8 devClass);

	delegate IOReturn GetDeviceSubClass (IntPtr self, out UInt8 devSubClass);

	delegate IOReturn GetDeviceProtocol (IntPtr self, out UInt8 devProtocol);

	delegate IOReturn GetDeviceVendor (IntPtr self, out UInt16 devVendor);

	delegate IOReturn GetDeviceProduct (IntPtr self, out UInt16 devProduct);

	delegate IOReturn GetDeviceReleaseNumber (IntPtr self, out UInt16 devRelNum);

	delegate IOReturn GetDeviceAddress (IntPtr self, out USBDeviceAddress addr);

	delegate IOReturn GetDeviceBusPowerAvailable (IntPtr self, out UInt32 powerAvailable);

	delegate IOReturn GetDeviceSpeed (IntPtr self, out UInt8 devSpeed);

	delegate IOReturn GetNumberOfConfigurations (IntPtr self, out UInt8 numConfig);

	delegate IOReturn GetLocationID (IntPtr self, out UInt32 locationID);

	delegate IOReturn GetConfigurationDescriptorPtr (IntPtr self, UInt8 configIndex, out IOUSBConfigurationDescriptorPtr desc);

	delegate IOReturn GetConfiguration (IntPtr self, out UInt8 configNum);

	delegate IOReturn SetConfiguration (IntPtr self, UInt8 configNum);

	delegate IOReturn GetBusFrameNumber (IntPtr self, out UInt64 frame, out AbsoluteTime atTime);

	delegate IOReturn ResetDevice (IntPtr self);

	delegate IOReturn DeviceRequest (IntPtr self, ref IOUSBDevRequest req);

	delegate IOReturn DeviceRequestAsync (IntPtr self, [MarshalAs (UnmanagedType.LPStruct)] IOUSBDevRequest req, IOAsyncCallback1 callback, IntPtr refCon);

	delegate IOReturn CreateInterfaceIterator (IntPtr self, [MarshalAs (UnmanagedType.LPStruct)] IOUSBFindInterfaceRequest req, out io_iterator_t iter);

	delegate IOReturn USBDeviceOpenSeize (IntPtr self);

	delegate IOReturn DeviceRequestTO (IntPtr self, ref IOUSBDevRequestTO req);

	delegate IOReturn DeviceRequestAsyncTO (IntPtr self, [MarshalAs (UnmanagedType.LPStruct)] IOUSBDevRequestTO req, IOAsyncCallback1 callback, IntPtr refCon);

	delegate IOReturn USBDeviceSuspend (IntPtr self, Boolean suspend);

	delegate IOReturn USBDeviceAbortPipeZero (IntPtr self);

	delegate IOReturn USBGetManufacturerStringIndex (IntPtr self, out UInt8 msi);

	delegate IOReturn USBGetProductStringIndex (IntPtr self, out UInt8 psi);

	delegate IOReturn USBGetSerialNumberStringIndex (IntPtr self, out UInt8 snsi);

	delegate IOReturn USBDeviceReEnumerate (IntPtr self, UInt32 options);

	delegate IOReturn GetBusMicroFrameNumber (IntPtr self, out UInt64 microFrame, out AbsoluteTime atTime);

	delegate IOReturn GetIOUSBLibVersion (IntPtr self, out NumVersion ioUSBLibVersion, out NumVersion usbFamilyVersion);

	delegate IOReturn GetBusFrameNumberWithTime (IntPtr self, out UInt64 frame, out AbsoluteTime atTime);

	delegate IOReturn GetUSBDeviceInformation (IntPtr self, out UInt32 info);

	delegate IOReturn RequestExtraPower (IntPtr self, UInt32 type, UInt32 requestedPower, out UInt32 powerAvailable);

	delegate IOReturn ReturnExtraPower (IntPtr self, UInt32 type, UInt32 powerReturned);

	delegate IOReturn GetExtraPowerAllocated (IntPtr self, UInt32 type, out UInt32 powerAllocated);

	delegate IOReturn GetBandwidthAvailableForDevice (IntPtr self, out UInt32 bandwidth);
}

