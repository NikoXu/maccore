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

using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using AbsoluteTime = System.UInt64;
using CFRunLoopSourceRef = System.IntPtr;
using IOUSBConfigurationDescriptorPtr = System.IntPtr;
using UInt8 = System.Byte;
using USBDeviceAddress = System.UInt16;
using io_iterator_t = System.IntPtr;
using mach_port_t = System.IntPtr;

namespace MonoMac.IOKit.USB
{
	public partial class IOUSBDevice : IOUSBNub
	{
		Lazy<IOCFPlugin<IOUSBDeviceUserClientType>> pluginInterface;
		Lazy<IIOCFPlugin<IOUSBDeviceInterface>> deviceInterface;

		Lazy<CFRunLoopSource> deviceAsyncEventSource;
		Lazy<Mach.Port> deviceAsyncPort;

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
			deviceAsyncPort = new Lazy<MonoMac.Mach.Port> (CreateAsyncPort);
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

		public Mach.Port AsyncPort {
			get { return deviceAsyncPort.Value; }
		}

		/// <summary>
		/// Gets the USB Device Class code (bDeviceClass).
		/// </summary>
		public DeviceClass Class {
			get {
				ThrowIfDisposed ();
				byte deviceClass;
				var result = DeviceInterface.GetDeviceClass (DeviceInterfaceRef, out deviceClass);
				IOObject.ThrowIfError (result);
				return (DeviceClass)deviceClass;
			}
		}

		/// <summary>
		/// Gets the USB Device Subclass code (bDeviceSubClass).
		/// </summary>
		public byte SubClass {
			get {
				ThrowIfDisposed ();
				byte deviceSubClass;
				var result = DeviceInterface.GetDeviceSubClass (DeviceInterfaceRef, out deviceSubClass);
				IOObject.ThrowIfError (result);
				return deviceSubClass;
			}
		}

		/// <summary>
		/// Gets the USB Protocol code (bDeviceProtocol).
		/// </summary>
		public InterfaceProtocol Protocol {
			get {
				ThrowIfDisposed ();
				byte protocol;
				var result = DeviceInterface.GetDeviceProtocol (DeviceInterfaceRef, out protocol);
				IOObject.ThrowIfError (result);
				return (InterfaceProtocol)protocol;
			}
		}

		/// <summary>
		/// Gets the vendor identifier (idVendor).
		/// </summary>
		public ushort VendorId {
			get {
				ThrowIfDisposed ();
				ushort vendor;
				var result = DeviceInterface.GetDeviceVendor (DeviceInterfaceRef, out vendor);
				IOObject.ThrowIfError (result);
				return vendor;
			}
		}

		/// <summary>
		/// Gets the product identifier (idProduct).
		/// </summary>
		public ushort ProductId {
			get {
				ThrowIfDisposed ();
				ushort product;
				var result = DeviceInterface.GetDeviceProduct (DeviceInterfaceRef, out product);
				IOObject.ThrowIfError (result);
				return product;
			}
		}

		/// <summary>
		/// Gets the device release number (bcdDevice).
		/// </summary>
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

		/// <summary>
		/// Gets the configuration count (bNumConfigurations).
		/// </summary>
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

		/// <summary>
		/// Gets the index of the manufacturer string (iManufacturer).
		/// </summary>
		[Since (0,4)]
		public byte ManufacturerStringIndex {
			get {
				ThrowIfDisposed ();
				byte index;
				var result = DeviceInterface.USBGetManufacturerStringIndex (DeviceInterfaceRef, out index);
				IOObject.ThrowIfError (result);
				return index;
			}
		}

		/// <summary>
		/// Gets the index of the product string (iProduct).
		/// </summary>
		[Since (0,4)]
		public byte ProductStringIndex {
			get {
				ThrowIfDisposed ();
				byte index;
				var result = DeviceInterface.USBGetProductStringIndex (DeviceInterfaceRef, out index);
				IOObject.ThrowIfError (result);
				return index;
			}
		}

		/// <summary>
		/// Gets the index of the serial number string (iSerialNumber).
		/// </summary>
		/// <value>The index of the serial number string.</value>
		[Since (0,4)]
		public byte SerialNumberStringIndex {
			get  {
				ThrowIfDisposed ();
				byte index;
				var result = DeviceInterface.USBGetSerialNumberStringIndex (DeviceInterfaceRef, out index);
				IOObject.ThrowIfError (result);
				return index;
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

		public Mach.Port CreateAsyncPort ()
		{
			ThrowIfDisposed ();
			IntPtr portRef;
			var result = DeviceInterface.CreateDeviceAsyncPort (DeviceInterfaceRef, out portRef);
			IOObject.ThrowIfError (result);
			return new Mach.Port (portRef);
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

		// TODO: replace IOAsyncCallback1 with a more suitable delegate
		public void SendRequestAsync (IOUSBDevRequest request, IOAsyncCallback1 callback)
		{
			ThrowIfDisposed ();
			var result = DeviceInterface.DeviceRequestAsync (DeviceInterfaceRef, request, callback, IntPtr.Zero);
			IOObject.ThrowIfError (result);
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

		// TODO: replace IOAsyncCallback1 with a managed delegate
		[Since (0,4)]
		public void RequestTimeoutAsync (IOUSBDevRequestTO request, IOAsyncCallback1 callback)
		{
			ThrowIfDisposed ();
			var result = DeviceInterface.DeviceRequestAsyncTO (DeviceInterfaceRef, request, callback, IntPtr.Zero);
			IOObject.ThrowIfError (result);
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

	/// <summary>
	/// Parameter block for control requests, using a simple pointer
	/// for the data to be transferred.
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBDevRequest
	{
		/// <summary>
		/// Request type: Standard, Class or Vendor
		/// </summary>
		public DeviceRequestType RequestType;
		/// <summary>
		/// Request code
		/// </summary>
		public UInt8 Request;
		/// <summary>
		/// 16 bit parameter for request, host endianess
		/// </summary>
		public UInt16 Value;
		/// <summary>
		/// 16 bit parameter for request, host endianess
		/// </summary>
		public UInt16 Index;
		/// <summary>
		/// Length of data part of request, 16 bits, host endianess
		/// </summary>
		public UInt16 Length;
		/// <summary>
		/// Pointer to data for request - data returned in bus endianess
		/// </summary>
		public IntPtr Data;
		/// <summary>
		/// Set by standard completion routine to number of data bytes
		/// </summary>
		public UInt32 LenDone;
	}

	/// <summary>
	/// Structure used with FindNextInterface.
	/// </summary>
	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBFindInterfaceRequest
	{
		public const ushort DontCare = 0xFFFF;

		public InterfaceClass interfaceClass;
		public InterfaceSubClass interfaceSubClass;
		public InterfaceProtocol interfaceProtocol;
		public UInt16 alternateSetting;

		/// <summary>
		/// requested class
		/// </summary>
		public InterfaceClass InterfaceClass { get { return interfaceClass; } }

		/// <summary>
		/// requested subclass
		/// </summary>
		public InterfaceSubClass InterfaceSubClass { get { return interfaceSubClass; } }

		/// <summary>
		/// requested protocol
		/// </summary>
		public InterfaceProtocol InterfaceProtocol { get { return interfaceProtocol; } }

		/// <summary>
		/// requested alt setting
		/// </summary>
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

	/// <summary>
	/// Numeric version part of 'vers' resource accessable in little endian format.
	/// </summary>
	public struct NumVersion
	{
		/// <summary>
		/// revision level of non-released version
		/// </summary>
		public UInt8 nonRelRev;

		/// <summary>
		/// stage code: dev, alpha, beta, final
		/// </summary>
		public UInt8 stage;

		/// <summary>
		/// 2nd & 3rd part of version number share a byte
		/// </summary>
		public UInt8 minorAndBugRev;

		/// <summary>
		/// 1st part of version number in BCD
		/// </summary>
		public UInt8 majorRev;
	}

	[Guid ("9dc7b780-9ec0-11d4-a54f-000a27052861")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceUserClientType : IOCFPlugInInterface
	{
	}

	/// <summary>
	/// The object you use to access USB devices from user space, returned by all versions of the IOUSBFamily
	/// currently shipping.
	/// </summary>
	/// <remarks>The functions listed here will work with any version of the IOUSBDeviceInterface, including
	/// the one shipped with Mac OS X version 10.0. </remarks>
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

	/// <summary>
	/// The object you use to access USB devices from user space, returned by the IOUSBFamily version 5.0.0 and above.
	/// </summary>
	/// <remarks>Available on Mac OS X version 10.7.3 and later.</remarks>
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

	/// <summary>
	/// The object you use to access USB devices from user space, returned by the IOUSBFamily version 5.0.0 and above.
	/// </summary>
	/// <remarks>Available on Mac OS X version 10.7.3 and later.</remarks>
	[Guid ("A33CF047-4B5B-48E2-B57D-0207FCEAE13B")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface550 : IOUSBDeviceInterface320
	{
		GetBandwidthAvailableForDevice getBandwidthAvailableForDevice;
		
		public override GetBandwidthAvailableForDevice GetBandwidthAvailableForDevice {
			get { return getBandwidthAvailableForDevice; }
		}
	}

	/*!
	 @interface IOUSBDeviceInterface
	 @abstract   The object you use to access USB devices from user space, returned by all versions of the IOUSBFamily
	 currently shipping.
	 @discussion The functions listed here will work with any version of the IOUSBDeviceInterface, including
	 the one shipped with Mac OS X version 10.0. 
	 */
	
	/*!
	@function CreateDeviceAsyncEventSource
	@abstract   Creates a run loop source for delivery of all asynchronous notifications on this device.
	@discussion The Mac OS X kernel does not spawn a thread to callback to the client. Instead it delivers 
	            completion notifications (see @link //apple_ref/C/instm/IOUSBInterfaceInterface/CreateInterfaceAsyncPort/ CreateInterfaceAsyncPort @/link). This routine 
	            wraps that port with the appropriate routing code so that the completion notifications can be 
	            automatically routed through the client's CFRunLoop.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      source Pointer to a CFRunLoopSourceRef to return the newly created run loop event source.
	@result     Returns kIOReturnSuccess if successful or a kern_return_t if unsuccessful.
	*/
	delegate IOReturn CreateDeviceAsyncEventSource (IntPtr self, out CFRunLoopSourceRef source);

	/*!
	@function GetDeviceAsyncEventSource
	@abstract   Returns the CFRunLoopSourceRef for this IOService instance.
	@param      self Pointer to the IOUSBDeviceInterface.
	@result     Returns the run loop source if one has been created, 0 otherwise.
	*/
	delegate CFRunLoopSourceRef GetDeviceAsyncEventSource (IntPtr self);

	/*!
	@function CreateDeviceAsyncPort
	@abstract   Creates and registers a mach_port_t for asynchronous communications.
	@discussion The Mac OS X kernel does not spawn a thread to callback to the client. Instead it delivers 
	            completion notifications on this mach port. After receiving a message on this port the 
	            client is obliged to call the IOKitLib.h IODispatchCalloutFromMessage() function for 
	            decoding the notification message.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      port Pointer to a mach_port_t to return the newly created port.
	@result     Returns kIOReturnSuccess if successful or a kern_return_t if unsuccessful.
	*/
	delegate IOReturn CreateDeviceAsyncPort (IntPtr self, out mach_port_t port); 

	/*!
	@function GetDeviceAsyncPort
	@abstract   Returns the mach_port_t port for this IOService instance.
	@param      self Pointer to the IOUSBDeviceInterface.
	@result     Returns the port if one exists, 0 otherwise.
	*/
	delegate mach_port_t GetDeviceAsyncPort (IntPtr self);

	/*!
	@function USBDeviceOpen
	@abstract   Opens the IOUSBDevice for exclusive access.
	@discussion Before the client can issue commands that change the state of the device, it 
	            must have succeeded in opening the device. This establishes an exclusive link 
	            between the client's task and the actual device.
	@param      self Pointer to the IOUSBDeviceInterface.
	@result     Returns kIOReturnExclusiveAccess if some other task has the device opened already,
	            kIOReturnError if the connection with the kernel cannot be established or kIOReturnSuccess if successful.
	*/
	delegate IOReturn USBDeviceOpen (IntPtr self);

	/*!
	@function USBDeviceClose
	@abstract   Closes the task's connection to the IOUSBDevice.
	@discussion Releases the client's exclusive access to the IOUSBDevice.
	@param      self Pointer to the IOUSBDeviceInterface.
	@result     Returns kIOReturnSuccess if successful, some other mach error if the connection is no longer valid.
	*/
	delegate IOReturn USBDeviceClose (IntPtr self);

	/*!
	@function GetDeviceClass
	@abstract   Returns the USB Class (bDeviceClass) of the device.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      devClass Pointer to UInt8 to hold the device Class.
	@result      Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetDeviceClass (IntPtr self, out UInt8 devClass);

	/*!
	@function GetDeviceSubClass
	@abstract   Returns the USB Subclass (bDeviceSubClass) of the device.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      devSubClass Pointer to UInt8 to hold the device Subclass.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetDeviceSubClass (IntPtr self, out UInt8 devSubClass);

	/*!
	@function GetDeviceProtocol
	@abstract   Returns the USB Protocol (bDeviceProtocol) of the interface.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      devProtocol Pointer to UInt8 to hold the device Protocol.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetDeviceProtocol (IntPtr self, out UInt8 devProtocol);

	/*!
	@function GetDeviceVendor
	@abstract   Returns the USB Vendor ID (idVendor) of the device.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      devVendor   Pointer to UInt16 to hold the vendorID.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetDeviceVendor (IntPtr self, out UInt16 devVendor);

	/*!
	@function GetDeviceProduct
	@abstract    Returns the USB Product ID (idProduct) of the device.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      devProduct  Pointer to UInt16 to hold the ProductID.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetDeviceProduct (IntPtr self, out UInt16 devProduct);

	/*!
	@function GetDeviceReleaseNumber
	@abstract   Returns the Device Release Number (bcdDevice) of the device.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      devRelNum   Pointer to UInt16 to hold the Device Release Number.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetDeviceReleaseNumber (IntPtr self, out UInt16 devRelNum);

	/*!
	@function GetDeviceAddress
	@abstract   Returns the address of the device on its bus.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      addr    Pointer to USBDeviceAddress to hold the result.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetDeviceAddress (IntPtr self, out USBDeviceAddress addr);

	/*!
	@function GetDeviceBusPowerAvailable
	@abstract   Returns the power available to the device.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      powerAvailable Pointer to UInt32 to hold the power available (in 2 mA increments).
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetDeviceBusPowerAvailable (IntPtr self, out UInt32 powerAvailable);

	/*!
	@function GetDeviceSpeed
	@abstract   Returns the speed of the device.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      devSpeed Pointer to UInt8 to hold the speed (kUSBDeviceSpeedLow, kUSBDeviceSpeedFull, kUSBDeviceSpeedHigh, or kUSBDeviceSpeedSuper).
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetDeviceSpeed (IntPtr self, out UInt8 devSpeed);

	/*!
	@function GetNumberOfConfigurations
	@abstract   Returns the number of supported configurations in this device.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      numConfig Pointer to UInt8 to hold the number of configurations.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetNumberOfConfigurations (IntPtr self, out UInt8 numConfig);

	/*!
	@function GetLocationID
	@abstract   Returns the location ID.
	@discussion The location ID is a 32 bit number which is unique among all USB devices in the system, and 
	            which will not change on a system reboot unless the topology of the bus itself changes. The 
	            device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      locationID Pointer to UInt32 to hold the location ID.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetLocationID (IntPtr self, out UInt32 locationID);

	/*!
	@function GetConfigurationDescriptorPtr
	@abstract   Returns a pointer to a configuration descriptor for a given index.
	@discussion Note that this will point to the data as received from the USB bus and hence will be in USB bus 
	            order (i.e. little endian).  The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      configIndex The index (zero based) of the desired config descriptor.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetConfigurationDescriptorPtr (IntPtr self, UInt8 configIndex, out IOUSBConfigurationDescriptorPtr desc);

	/*!
	@function GetConfiguration
	@abstract   Returns the currently selected configuration in the device.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      configNum Pointer to UInt8 to hold the configuration value.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetConfiguration (IntPtr self, out UInt8 configNum);

	/*!
	@function SetConfiguration
	@abstract   Sets the configuration in the device.
	@discussion Note that setting the configuration causes any existing IOUSBInterface objects attached to the 
	            IOUSBDevice to be destroyed, and all of the interfaces in the new configuration to be instantiated 
	            as new IOUSBInterface objects.  The device must be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      configNum The value of the desired configuration (from IOUSBConfigurationDescriptor.bConfigurationValue).
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, 
	            or kIOReturnNotOpen if the device is not open for exclusive access.
	*/
	delegate IOReturn SetConfiguration (IntPtr self, UInt8 configNum);

	/*!
	@function GetBusFrameNumber
	@abstract   Gets the current frame number of the bus to which the device is attached.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      frame Pointer to UInt64 to hold the frame number.
	@param      atTime Pointer to a returned AbsoluteTime, which is the system time ("wall time") when the frame number register was read. This
				system time could be the time at the beginning, middle, or end of the given frame.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetBusFrameNumber (IntPtr self, out UInt64 frame, out AbsoluteTime atTime);

	/*!
	@function ResetDevice
	@abstract   Tells the IOUSBFamily to issue a reset to the device.
	@discussion It will not reenumerate the device, which means that the cached device descriptor values will not 
	            be updated after the reset. (If you want the IOUSBFamily to reload the cached values, use the call
	            USBDeviceReEnumerate). Prior to version 1.8.5 of the IOUSBFamily, this call also sent a message to 
	            all clients of the IOUSBDevice (IOUSBInterfaces and their drivers).  The device must be open to use 
	            this function.  
	            
	            This behavior was eliminated in version 1.8.5 of the IOUSBFamily.
	@param      self Pointer to the IOUSBDeviceInterface.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
	            or kIOReturnNotOpen if the device is not open for exclusive access.
	*/
	delegate IOReturn ResetDevice (IntPtr self);

	/*!
	@function DeviceRequest
	@abstract   Sends a USB request on the default control pipe.
	@discussion The device must be open to issue this call. Care should be taken when issuing a device request which
	            changes the state of the device. Use the API, for example, to change the configuration of the device 
	            or to select an alternate setting on an interface.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      req Pointer to an IOUSBDevRequest containing the request.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
				kIOReturnAborted if the thread is interrupted before the call completes, 
	            or kIOReturnNotOpen if the device is not open for exclusive access.
	*/
	delegate IOReturn DeviceRequest (IntPtr self, ref IOUSBDevRequest req);

	/*!
	@function DeviceRequestAsync
	@abstract   Sends an asynchronous USB request on the default control pipe.
	@discussion The device must be open to issue this command. Care should be taken when issuing a device request which 
	            changes the state of the device. Use the API, for example, to change the configuration of the device or 
	            to select an alternate setting on an interface.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      req Pointer to an IOUSBDevRequest containing the request.
	@param      callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the number of bytes that were actually transferred.  
	 			A message addressed to this callback is posted to the Async port upon completion.
	@param      refCon Arbitrary pointer which is passed as a parameter to the callback routine.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
	            kIOReturnNotOpen if the device is not open for exclusive access, or kIOUSBNoAsyncPortErr if no Async 
	            port has been created for this interface.
	*/
	delegate IOReturn DeviceRequestAsync (IntPtr self, [MarshalAs (UnmanagedType.LPStruct)] IOUSBDevRequest req, IOAsyncCallback1 callback, IntPtr refCon);

	/*!
	@function CreateInterfaceIterator
	@abstract   Creates an iterator to iterate over some or all of the interfaces of a device.
	@discussion The device does not have to be open to use this function.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      req Pointer an IOUSBFindInterfaceRequest structure describing the desired interfaces.
	@param      iter Pointer to a an io_iterator_t to contain the new iterator.
	@result     Returns kIOReturnSuccess if successful or kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn CreateInterfaceIterator (IntPtr self, [MarshalAs (UnmanagedType.LPStruct)] IOUSBFindInterfaceRequest req, out io_iterator_t iter);

	/*!
	@interface IOUSBDeviceInterface182
	@abstract   The object you use to access USB devices from user space, returned by the IOUSBFamily version
	        1.8.2 and above.
	@discussion The functions listed here include all of the functions defined for the IOUSBDeviceInterface and
	        some new functions that are available on Mac OS X version 10.0.4 and later.
	@super IOUSBDeviceInterface
	*/

	/*!
	@function USBDeviceOpenSeize
	@abstract  Opens the IOUSBDevice for exclusive access.
	@discussion This function opens the IOUSBDevice for exclusive access. If another client 
	        has the device opened, an attempt is made to get that client to close it before 
	        returning.  Before the client can issue commands that change the state of the device, 
	        it must have succeeded in opening the device. This establishes an exclusive 
	        link between the client's task and the actual device.
	@availability This function is only available with IOUSBDeviceInterface182 and above.
	@param      self Pointer to the IOUSBDeviceInterface.
	@result     Returns kIOReturnExclusiveAccess if some other task has the device opened already and refuses
	        to close it, kIOReturnError if the connection with the kernel can not be established or kIOReturnSuccess if successful.
	*/
	delegate IOReturn USBDeviceOpenSeize (IntPtr self);

	/*!
	@function DeviceRequestTO
	@abstract   Sends a USB request on the default control pipe.
	@discussion This function sends a USB request on the default control pipe. The IOUSBDevRequestTO structure 
	        allows the client to specify timeout values for this request.  The device must be open to issue this command. 
	        Care should be taken when issuing a device request which changes the state of the device. Use the 
	        API, for example, to change the configuration of the device or to select an alternate setting on 
	        an interface.
	@availability This function is only available with IOUSBDeviceInterface182 and above.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      req Pointer to an IOUSBDevRequestTO containing the request.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
				kIOReturnAborted if the thread is interrupted before the call completes, 
	       or kIOReturnNotOpen if the device is not open for exclusive access.
	*/
	delegate IOReturn DeviceRequestTO (IntPtr self, ref IOUSBDevRequestTO req);

	/*!
	@function DeviceRequestAsyncTO
	@abstract   Sends an asynchronous USB request on the default control pipe.
	@discussion This function sends an asynchronous USB request on the default control pipe.  The IOUSBDevRequestTO 
	        structure allows the client to specify timeout values for this request.  The device must be open to 
	        issue this command. Care should be taken when issuing a device request which changes the state of 
	        the device. Use the API, for example, to change the configuration of the device or to select an 
	        alternate setting on an interface.
	@availability This function is only available with IOUSBDeviceInterface182 and above.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      req Pointer to an IOUSBDevRequestTO containing the request.
	 @param     callback An IOAsyncCallback1 method. Upon completion, the arg0 argument of the AsyncCallback1 will contain the number of bytes that were actually transferred
	 			in the DeviceRequest.  A message addressed to this callback is posted to the 
	        Async port upon completion.
	@param      refCon Arbitrary pointer which is passed as a parameter to the callback routine.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
	        kIOReturnNotOpen if the device is not open for exclusive access, orkIOUSBNoAsyncPortErr if no Async 
	        port has been created for this interface.
	*/
	delegate IOReturn DeviceRequestAsyncTO (IntPtr self, [MarshalAs (UnmanagedType.LPStruct)] IOUSBDevRequestTO req, IOAsyncCallback1 callback, IntPtr refCon);

	/*!
	@function USBDeviceSuspend
	@abstract   Tells the USB Family to either suspend or resume the port to which a device is attached.
	@discussion The device must be open to use this function.
	@availability This function is only available with IOUSBDeviceInterface182 and above.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      suspend TRUE to cause the port to be suspended, FALSE to cause it to be resumed.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
	        or kIOReturnNotOpen if the device is not open for exclusive access.
	*/
	delegate IOReturn USBDeviceSuspend (IntPtr self, Boolean suspend);

	/*!
	@function USBDeviceAbortPipeZero
	@abstract   Aborts a transaction on the default control pipe.
	@discussion The device must be open to use this function.
	@availability This function is only available with IOUSBDeviceInterface182 and above.
	@param      self Pointer to the IOUSBDeviceInterface.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
	        or kIOReturnNotOpen if the device is not open for exclusive access.
	*/
	delegate IOReturn USBDeviceAbortPipeZero (IntPtr self);

	/*!
	@function USBGetManufacturerStringIndex
	@abstract   Returns the manufacturer string index in the device descriptor.
	@discussion The device does not have to be open to use this function.
	@availability This function is only available with IOUSBDeviceInterface182 and above.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      msi Pointer to UInt8 to hold the string index.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn USBGetManufacturerStringIndex (IntPtr self, out UInt8 msi);

	/*!
    @function USBGetProductStringIndex
    @abstract   Returns the product string index in the device descriptor.
    @discussion The device does not have to be open to use this function.
    @availability This function is only available with IOUSBDeviceInterface182 and above.
    @param      self Pointer to the IOUSBDeviceInterface.
    @param      psi Pointer to UInt8 to hold the string index.
    @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn USBGetProductStringIndex (IntPtr self, out UInt8 psi);

	/*!
	@function USBGetSerialNumberStringIndex
	@abstract   Returns the serial number string index in the device descriptor.
	@discussion The device does not have to be open to use this function.
	@availability This function is only available with IOUSBDeviceInterface182 and above.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      snsi Pointer to UInt8 to hold the string index.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn USBGetSerialNumberStringIndex (IntPtr self, out UInt8 snsi);

	/*!
	@interface IOUSBDeviceInterface187
	@abstract   The object you use to access USB devices from user space, returned by the IOUSBFamily version
	        10.8.7 and above.
	@discussion The functions listed here include all of the functions defined for the IOUSBDeviceInterface,
	        IOUSBDeviceInterface182, and some new functions that are available on Mac OS X version 10.1.2 and later.
	@super IOUSBDeviceInterface182
	*/

	/*!
	@function USBDeviceReEnumerate
	@abstract   Tells the IOUSBFamily to reenumerate the device.
	@discussion This function will send a terminate message to all clients of the IOUSBDevice (such as 
	        IOUSBInterfaces and their drivers, as well as the current User Client), emulating an unplug 
	        of the device. The IOUSBFamily will then enumerate the device as if it had just 
	        been plugged in. This call should be used by clients wishing to take advantage 
	        of the Device Firmware Update Class specification.  The device must be open to use this function. 
	@availability This function is only available with IOUSBDeviceInterface187 and above.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      options A UInt32 reserved for future use. Ignored in current implementation. Set to zero.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService,
	        or kIOReturnNotOpen if the device is not open for exclusive access.
	*/
	delegate IOReturn USBDeviceReEnumerate (IntPtr self, UInt32 options);

	/*!
	@interface IOUSBDeviceInterface197
	@abstract   The object you use to access USB devices from user space, returned by the IOUSBFamily version
	        1.9.7 and above.
	@discussion The functions listed here include all of the functions defined for the IOUSBDeviceInterface,
	        IOUSBDeviceInterface182, IOUSBDeviceInterface187, and some new functions that are available 
	        on Mac OS X version 10.2.3 and later.
	@super IOUSBDeviceInterface187
	*/

	/*!
	@function GetBusMicroFrameNumber
	@abstract   Gets the current micro frame number of the bus to which the device is attached.
	@discussion The device does not have to be open to use this function.
	@availability This function is only available with IOUSBDeviceInterface197 and above.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      microFrame Pointer to UInt64 to hold the microframe number.
	@param      atTime Pointer to an AbsoluteTime, which should be within 1ms of the time when the bus 
	        frame number was acquired.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetBusMicroFrameNumber (IntPtr self, out UInt64 microFrame, out AbsoluteTime atTime);

	/*!
	@function GetIOUSBLibVersion
	@abstract   Returns the version of the IOUSBLib and the version of the IOUSBFamily.
	@discussion The device does not have to be open to use this function.
	@availability This function is only available with IOUSBDeviceInterface197 and above.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      ioUSBLibVersion Pointer to a NumVersion structure that on return will contain the version of 
	        the IOUSBLib.
	@param      usbFamilyVersion Pointer to a NumVersion structure that on return will contain the version of 
	        the IOUSBFamily.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	*/
	delegate IOReturn GetIOUSBLibVersion (IntPtr self, out NumVersion ioUSBLibVersion, out NumVersion usbFamilyVersion);

	/*!
	 @interface IOUSBDeviceInterface300
	 @abstract   The object you use to access USB devices from user space, returned by the IOUSBFamily version 3.0.0 and above.
	 @discussion The functions listed here include all of the functions defined for the IOUSBDeviceInterface,
	 IOUSBDeviceInterface182, IOUSBDeviceInterface187, IOUSBDeviceInterface197, IOUSBDeviceInterface245, 
	 and some new functions that are available on Mac OS X version 10.5 and later.
	 @super IOUSBDeviceInterface245
	 */

	/*!
	@function GetBusFrameNumberWithTime
	@abstract   Gets a recent frame number of the bus to which the device is attached, along with a system time corresponding to the start of that frame
	@discussion The device does not have to be open to use this function.
	@availability This function is only available with IOUSBDeviceInterface300 and above.
	@param      self Pointer to the IOUSBDeviceInterface.
	@param      frame Pointer to UInt64 to hold the frame number.
	@param      atTime Pointer to a returned AbsoluteTime, which is the system time ("wall time") as close as possible to the beginning of that USB frame. The jitter on this value may be as much as 200 microseconds.
	@result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or kIOReturnUnsupported is the bus doesn't support this function.
	*/
	delegate IOReturn GetBusFrameNumberWithTime (IntPtr self, out UInt64 frame, out AbsoluteTime atTime);

	/*!
	@interface IOUSBDeviceInterface320
	@abstract   The object you use to access USB devices from user space, returned by the IOUSBFamily version 3.2.0 and above.
	@discussion The functions listed here include all of the functions defined for the IOUSBDeviceInterface,
	IOUSBDeviceInterface182, IOUSBDeviceInterface187, IOUSBDeviceInterface197, IOUSBDeviceInterface245, or IOUSBDeviceInterface300
	and some new functions that are available on Mac OS X version 10.5.4 and later.
	@super IOUSBDeviceInterface300
	*/

	/*!
	 @function GetUSBDeviceInformation
	 @abstract 	Returns status information about the USB device, such as whether the device is captive or whether it is in the suspended state.
	 @discussion The device does not have to be open to use this function.
	 @availability This function is only available with IOUSBDeviceInterface320 and above.
	 @param      self Pointer to the IOUSBDeviceInterface.
	 @param      info Pointer to a buffer that returns a bit field of information on the device (see the USBDeviceInformationBits in USB.h).
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or kIOReturnUnsupported is the bus doesn't support this function.
	 */
	delegate IOReturn GetUSBDeviceInformation (IntPtr self, out UInt32 info);

	/*!
	 @function RequestExtraPower
	 @abstract				Clients can use this API to reserve extra power for use by this device while the machine is asleep or while it is awake.  Units are milliAmps (mA).
	 @discussion			The device has to be open to use this function.
	 @availability			This function is only available with IOUSBDeviceInterface320 and above.
	 @param self			Pointer to the IOUSBDeviceInterface.
	 @param type			Indicates whether the power is to be used during wake or sleep (One of kUSBPowerDuringSleep or kUSBPowerDuringWake)
	 @param requestedPower 	Amount of power desired, in mA
	 @param powerAvailable 	Amount of power that was reserved, in mA
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService, or kIOReturnUnsupported is the bus doesn't support this function.
	 */
	delegate IOReturn RequestExtraPower (IntPtr self, UInt32 type, UInt32 requestedPower, out UInt32 powerAvailable);

	/*!
	 @function ReturnExtraPower
	 @abstract				Clients can use this API to tell the system that they will not use power that was previously reserved by using the RequestExtraPower API.
	 @discussion			The device has to be open to use this function.
	 @availability			This function is only available with IOUSBDeviceInterface320 and above.
	 @param      self		Pointer to the IOUSBDeviceInterface.
	 @param type			Indicates whether the power is to be used during wake or sleep (One of kUSBPowerDuringSleep or kUSBPowerDuringWake)
	 @param powerReturned 	Amount of power to be returned, in mA.
	 @result				If the returnedPower was not previously allocated, an error will be returned.  This will include the case for power that was requested for sleep but was returned for wake. Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	 */
	delegate IOReturn ReturnExtraPower (IntPtr self, UInt32 type, UInt32 powerReturned);

	/*!
	 @function GetExtraPowerAllocated
	 @abstract				Clients can use this API to ask how much extra power has already been reserved by this device.  Units are milliAmps (mA).
	 @discussion			The device has to be open to use this function.
	 @availability			This function is only available with IOUSBDeviceInterface320 and above.
	 @param      self		Pointer to the IOUSBDeviceInterface.
	 @param type			Indicates whether the allocated power was to be used during wake or sleep (One of kUSBPowerDuringSleep or kUSBPowerDuringWake)
	 @param powerAllocated 	Amount of power to be returned, in mA.
	 @result				Value returned can be 0 if no power has been allocated. Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	 */
	delegate IOReturn GetExtraPowerAllocated (IntPtr self, UInt32 type, out UInt32 powerAllocated);

	/*!
	@interface IOUSBDeviceInterface500
	@abstract   The object you use to access USB devices from user space, returned by the IOUSBFamily version 3.2.0 and above.
	@discussion The functions listed here include all of the functions defined for the IOUSBDeviceInterface,
	IOUSBDeviceInterface182, IOUSBDeviceInterface187, IOUSBDeviceInterface197, IOUSBDeviceInterface245, IOUSBDeviceInterface300, or IOUSBDeviceInterface320
	and some new functions that are available on Mac OS X version 10.7.3 and later.
	@super IOUSBDeviceInterface320
	*/

	/*!
	 @function GetBandwidthAvailableForDevice
	 @abstract   Returns the amount of bandwidth available on the bus for allocation to 
	 periodic pipes.  If the device is a high or super speed device, it will be the number of bytes per microframe (125 µsecs). If it is a full
	 speed device, it will be the number of bytes per frame (1ms)
	 @discussion This function is useful for determining the correct AltInterface setting as well as for using 
	 SetPipePolicy. The interface does not have to be open to use this function.
	 @availability This function is only available with IOUSBDeviceInterface500 and above.
	 @param      self Pointer to the IOUSBInterfaceInterface.
	 @param      bandwidth Pointer to UInt32 to hold the amount of bandwidth available (in bytes per frame or microframe).
	 @result     Returns kIOReturnSuccess if successful, kIOReturnNoDevice if there is no connection to an IOService.
	 */
	delegate IOReturn GetBandwidthAvailableForDevice (IntPtr self, out UInt32 bandwidth);
}

