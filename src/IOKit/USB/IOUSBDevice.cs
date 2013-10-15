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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.Kernel.Mach;
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
	public class IOUSBDevice : IOUSBNub
	{
		IIOCFPlugin<IOUSBDeviceInterface> @interface;

		internal IOUSBDevice (IntPtr handle, bool owns) : base (handle, owns)
		{
			using (var pluginInterface = IOCFPlugin.CreateInterfaceForService<IOUSBDeviceUserClientType> (this)) {
				var bundleVersion = IOUSB.BundleVersion;
				if (bundleVersion >= new Version ("5.5.0"))
					@interface = pluginInterface.QueryInterface<IOUSBDeviceInterface550> ();
				else if (bundleVersion >= new Version ("3.2.0"))
					@interface = pluginInterface.QueryInterface<IOUSBDeviceInterface320> ();
				else if (bundleVersion >= new Version ("3.0.0"))
					@interface = pluginInterface.QueryInterface<IOUSBDeviceInterface300> ();
				else if (bundleVersion >= new Version ("2.4.5"))
					@interface = pluginInterface.QueryInterface<IOUSBDeviceInterface245> ();
				else if (bundleVersion >= new Version ("1.9.7"))
					@interface = pluginInterface.QueryInterface<IOUSBDeviceInterface197> ();
				else if (bundleVersion >= new Version ("1.8.7"))
					@interface = pluginInterface.QueryInterface<IOUSBDeviceInterface187> ();
				else if (bundleVersion >= new Version ("1.8.2"))
					@interface = pluginInterface.QueryInterface<IOUSBDeviceInterface182> ();
				else
					@interface = pluginInterface.QueryInterface<IOUSBDeviceInterface> ();
			}
		}

		IntPtr InterfaceRef {
			get { return @interface.Handle; }
		}

		IOUSBDeviceInterface Interface {
			get { return @interface.Interface; }
		}

		public CFRunLoopSource AsyncEventSource {
			get {
				ThrowIfDisposed ();
				IntPtr runLoopSourceRef = Interface.GetDeviceAsyncEventSource (InterfaceRef);
				if (runLoopSourceRef != IntPtr.Zero)
					return CFType.GetCFObject<CFRunLoopSource> (runLoopSourceRef);
				var result = Interface.CreateDeviceAsyncEventSource (InterfaceRef, out runLoopSourceRef);
				IOObject.ThrowIfError (result);
				return new CFRunLoopSource (runLoopSourceRef, true);
			}
		}

		public Port AsyncPort {
			get {
				ThrowIfDisposed ();
				IntPtr portRef = Interface.GetDeviceAsyncPort (InterfaceRef);
				if (portRef != IntPtr.Zero)
					return new Port(portRef);
				var result = Interface.CreateDeviceAsyncPort (InterfaceRef, out portRef);
				IOObject.ThrowIfError (result);
				return new Port (portRef);
			}
		}

		public DeviceClass Class {
			get {
				ThrowIfDisposed ();
				byte deviceClass;
				var result = Interface.GetDeviceClass (InterfaceRef, out deviceClass);
				IOObject.ThrowIfError (result);
				return (DeviceClass)deviceClass;
			}
		}

		public byte SubClass {
			get {
				ThrowIfDisposed ();
				byte deviceSubClass;
				var result = Interface.GetDeviceSubClass (InterfaceRef, out deviceSubClass);
				IOObject.ThrowIfError (result);
				return deviceSubClass;
			}
		}

		public InterfaceProtocol Protocol {
			get {
				ThrowIfDisposed ();
				byte protocol;
				var result = Interface.GetDeviceProtocol (InterfaceRef, out protocol);
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
				// TODO: does this need to be translated from BDC?
				IOObject.ThrowIfError (result);
				return releaseNumber;
			}
		}

		public ushort Address {
			get {
				ThrowIfDisposed ();
				ushort address;
				var result = Interface.GetDeviceAddress (InterfaceRef, out address);
				IOObject.ThrowIfError (result);
				return address;
			}
		}

		public uint BusPowerAvailable {
			get {
				ThrowIfDisposed ();
				uint busPowerAvailible;
				var result = Interface.GetDeviceBusPowerAvailable (InterfaceRef, out busPowerAvailible);
				IOObject.ThrowIfError (result);
				return busPowerAvailible * 2;
			}
		}

		public DeviceSpeed Speed {
			get {
				ThrowIfDisposed ();
				byte speed;
				var result = Interface.GetDeviceSpeed (InterfaceRef, out speed);
				IOObject.ThrowIfError (result);
				return (DeviceSpeed)speed;
			}
		}

		public byte ConfigurationCount {
			get {
				ThrowIfDisposed ();
				byte count;
				var result = Interface.GetNumberOfConfigurations (InterfaceRef, out count);
				IOObject.ThrowIfError (result);
				return count;
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

		public byte CurrentConfiguration {
			get {
				ThrowIfDisposed ();
				byte configuration;
				var result = Interface.GetConfiguration (InterfaceRef, out configuration);
				IOObject.ThrowIfError (result);
				return configuration;
			}
			set {
				ThrowIfDisposed ();
				var result = Interface.SetConfiguration (InterfaceRef, value);
				IOObject.ThrowIfError (result);
			}
		}

		[Since (0,4)]
		public string ManufacturerName {
			get {
				ThrowIfDisposed ();
				byte index;
				var result = Interface.USBGetManufacturerStringIndex (InterfaceRef, out index);
				IOObject.ThrowIfError (result);
				return GetStringDescriptor (index);
			}
		}

		[Since (0,4)]
		public string ProductName {
			get {
				ThrowIfDisposed ();
				byte index;
				var result = Interface.USBGetProductStringIndex (InterfaceRef, out index);
				IOObject.ThrowIfError (result);
				return GetStringDescriptor (index);
			}
		}

		[Since (0,4)]
		public string SerialNumber {
			get  {
				ThrowIfDisposed ();
				byte index;
				var result = Interface.USBGetSerialNumberStringIndex (InterfaceRef, out index);
				IOObject.ThrowIfError (result);
				return GetStringDescriptor (index);
			}
		}

		[Since (2,3)]
		public NumVersion IOUSBLibVersion {
			get  {
				ThrowIfDisposed ();
				NumVersion ioUSBLibVersion, usbFamilyVersion;
				var result = Interface.GetIOUSBLibVersion (InterfaceRef, out ioUSBLibVersion, out usbFamilyVersion);
				IOObject.ThrowIfError (result);
				return ioUSBLibVersion;
			}
		}

		[Since (2,3)]
		public NumVersion IOUSBFamilyVersion {
			get  {
				ThrowIfDisposed ();
				NumVersion ioUSBLibVersion, usbFamilyVersion;
				var result = Interface.GetIOUSBLibVersion (InterfaceRef, out ioUSBLibVersion, out usbFamilyVersion);
				IOObject.ThrowIfError (result);
				return usbFamilyVersion;
			}
		}

		[Since (5,4)]
		public DeviceInformation Information {
			get  {
				ThrowIfDisposed ();
				uint info;
				var result = Interface.GetUSBDeviceInformation (InterfaceRef, out info);
				IOObject.ThrowIfError (result);
				return (DeviceInformation)info;
			}
		}

		[Since (7,3)]
		public uint BandwidthAvailable {
			get  {
				ThrowIfDisposed ();
				uint bandwidth;
				var result = Interface.GetBandwidthAvailableForDevice (InterfaceRef, out bandwidth);
				IOObject.ThrowIfError (result);
				return bandwidth;
			}
		}

		public Port CreateAsyncPort ()
		{
			ThrowIfDisposed ();
			IntPtr portRef;
			var result = Interface.CreateDeviceAsyncPort (InterfaceRef, out portRef);
			IOObject.ThrowIfError (result);
			return new Port (portRef);
		}

		public void Open ()
		{
			ThrowIfDisposed ();
			var result = Interface.USBDeviceOpen (InterfaceRef);
			IOObject.ThrowIfError (result);
		}

		public void Close ()
		{
			ThrowIfDisposed ();
			var result = Interface.USBDeviceClose (InterfaceRef);
			IOObject.ThrowIfError (result);
		}

		public IOUSBConfigurationDescriptor GetConfigurationDescriptor (int index) {
			ThrowIfDisposed ();
			IntPtr configDescriptorRef;
			var result = Interface.GetConfigurationDescriptorPtr (InterfaceRef, (byte)(index - 1) , out configDescriptorRef);
			IOObject.ThrowIfError (result);
			return (IOUSBConfigurationDescriptor)Marshal.PtrToStructure (configDescriptorRef, typeof(IOUSBConfigurationDescriptor));
		}

		public IEnumerable<CultureInfo> SupportedLanguages {
			get {
				var languages = GetStringDescriptor (0, 0);
				foreach (int langID in languages) {
					CultureInfo info;
					try {
						info = CultureInfo.GetCultureInfo (langID);
					} catch (Exception) {
						continue;
					}
					yield return info;
				}
			}
		}

		public string GetStringDescriptor (byte index) {
			ThrowIfDisposed ();
			if (index == 0)
				return null;
			return new string (GetStringDescriptor (index, SupportedLanguages.First ().LCID));
		}

		public string GetStringDescriptor (byte index, CultureInfo language) {
			ThrowIfDisposed ();
			if (index == 0)
				return null;
			return new string (GetStringDescriptor (index, language.LCID));
		}

		char[] GetStringDescriptor (byte index, int language)
		{
			// based on http://oroboro.com/usb-serial-number-osx/
			const int maxUsbStringLength = 255;
			var buffer = Marshal.AllocHGlobal (maxUsbStringLength);
			try {
				var request = new IOUSBDeviceRequest () {
					Direction = EndpointDirection.In,
					DeviceRequestType = DeviceRequestType.Standard,
					Recipient = DeviceRequestRecipient.Device,
					RequestType = RequestType.GetDescriptor,
					Value = (ushort)((byte)DescriptorType.String << 8 | index),
					Index = (ushort)language,
					DataLength = maxUsbStringLength,
					Data = buffer
				};
				var result = Interface.DeviceRequest (InterfaceRef, ref request);
				ThrowIfError (result);
				var header = (IOUSBDescriptorHeader)Marshal.PtrToStructure (buffer, typeof(IOUSBDescriptorHeader));
				var dataLength = (header.Length - 1) / 2;
				var data = new char[dataLength];
				Marshal.Copy (buffer + 2, data, 0, dataLength);
				return data;
			} finally {
				Marshal.FreeHGlobal (buffer);
			}
		}

		public ulong GetBusFrameNumber (out ulong atTime) {
			ThrowIfDisposed ();
			ulong frame;
			var result = Interface.GetBusFrameNumber (InterfaceRef, out frame, out atTime);
			IOObject.ThrowIfError (result);
			return (ulong)IOUSB.USBToHostOrder ((long)frame);
		}

		public void Reset ()
		{
			ThrowIfDisposed ();
			var result = Interface.ResetDevice (InterfaceRef);
			IOObject.ThrowIfError (result);
		}

		public void SendRequest (ref IOUSBDeviceRequest request)
		{
			ThrowIfDisposed ();
			var result = Interface.DeviceRequest (InterfaceRef, ref request);
			IOObject.ThrowIfError (result);
		}

		public Task<int> SendRequestAsync (IOUSBDeviceRequest request)
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
			var result = Interface.DeviceRequestAsync (InterfaceRef, request, callback, IntPtr.Zero);
			IOObject.ThrowIfError (result);
			return completionSource.Task;
		}

		public IOIterator<IOUSBInterface> CreateInterfaceIterator (IOUSBFindInterfaceRequest request) {
			ThrowIfDisposed ();
			IntPtr iteratorRef;
			var result = Interface.CreateInterfaceIterator (InterfaceRef, request, out iteratorRef);
			IOObject.ThrowIfError (result);
			return new IOIterator<IOUSBInterface> (iteratorRef, true);
		}

		[Since (0,4)]
		public void OpenSeize ()
		{
			ThrowIfDisposed ();
			var result = Interface.USBDeviceOpenSeize (InterfaceRef);
			IOObject.ThrowIfError (result);
		}

		[Since (0,4)]
		public void RequestTimeout (ref IOUSBDevRequestTO request)
		{
			ThrowIfDisposed ();
			var result = Interface.DeviceRequestTO (InterfaceRef, ref request);
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
			var result = Interface.DeviceRequestAsyncTO (InterfaceRef, request, callback, IntPtr.Zero);
			IOObject.ThrowIfError (result);
			return completionSource.Task;
		}

		[Since (0,4)]
		public void Suspend (bool suspend)
		{
			ThrowIfDisposed ();
			var result = Interface.USBDeviceSuspend (InterfaceRef, suspend);
			IOObject.ThrowIfError (result);
		}

		[Since (0,4)]
		public void AbortPipeZero ()
		{
			ThrowIfDisposed ();
			var result = Interface.USBDeviceAbortPipeZero (InterfaceRef);
			IOObject.ThrowIfError (result);
		}

		[Since (1,2)]
		public void ReEnumerate (uint options)
		{
			ThrowIfDisposed ();
			var result = Interface.USBDeviceReEnumerate (InterfaceRef, options);
			IOObject.ThrowIfError (result);
		}

		[Since (2,3)]
		public ulong GetBusMicroFrameNumber (out ulong atTime) {
			ThrowIfDisposed ();
			ulong frame;
			var result = Interface.GetBusMicroFrameNumber (InterfaceRef, out frame, out atTime);
			IOObject.ThrowIfError (result);
			return (ulong)IOUSB.USBToHostOrder ((long)frame);
		}

		[Since (5,0)]
		public ulong GetBusFrameNumberWithTime (out AbsoluteTime atTime) {
			ThrowIfDisposed ();
			ulong frame;
			var result = Interface.GetBusFrameNumberWithTime (InterfaceRef, out frame, out atTime);
			IOObject.ThrowIfError (result);
			return (ulong)IOUSB.USBToHostOrder ((long)frame);
		}

		[Since (5,4)]
		public uint RequestExtraPower (PowerRequestType type, uint requestedPower) {
			ThrowIfDisposed ();
			uint powerAvailable;
			var result = Interface.RequestExtraPower (InterfaceRef, (uint)type, requestedPower, out powerAvailable);
			IOObject.ThrowIfError (result);
			return powerAvailable;
		}

		[Since (5,4)]
		public void ReturnExtraPower (PowerRequestType type, uint powerReturned) {
			ThrowIfDisposed ();
			var result = Interface.ReturnExtraPower (InterfaceRef, (uint)type, powerReturned);
			IOObject.ThrowIfError (result);
		}

		[Since (5,4)]
		public uint ExtraPowerAllocated (PowerRequestType type) {
			ThrowIfDisposed ();
			uint powerAllocated;
			var result = Interface.GetExtraPowerAllocated (InterfaceRef, (uint)type, out powerAllocated);
			IOObject.ThrowIfError (result);
			return powerAllocated;
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct IOUSBDeviceRequest
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
		_CreateDeviceAsyncEventSource createDeviceAsyncEventSource;
		_GetDeviceAsyncEventSource getDeviceAsyncEventSource;
		_CreateDeviceAsyncPort createDeviceAsyncPort;
		_GetDeviceAsyncPort getDeviceAsyncPort;
		_USBDeviceOpen usbDeviceOpen;
		_USBDeviceClose usbDeviceClose;
		_GetDeviceClass getDeviceClass;
		_GetDeviceSubClass getDeviceSubClass;
		_GetDeviceProtocol getDeviceProtocol;
		_GetDeviceVendor getDeviceVendor;
		_GetDeviceProduct getDeviceProduct;
		_GetDeviceReleaseNumber getDeviceReleaseNumber;
		_GetDeviceAddress getDeviceAddress;
		_GetDeviceBusPowerAvailable getDeviceBusPowerAvailable;
		_GetDeviceSpeed getDeviceSpeed;
		_GetNumberOfConfigurations getNumberOfConfigurations;
		_GetLocationID getLocationID;
		_GetConfigurationDescriptorPtr getConfigurationDescriptorPtr;
		_GetConfiguration getConfiguration;
		_SetConfiguration setConfiguration;
		_GetBusFrameNumber getBusFrameNumber;
		_ResetDevice resetDevice;
		_DeviceRequest deviceRequest;
		_DeviceRequestAsync deviceRequestAsync;
		_CreateInterfaceIterator createInterfaceIterator;

		public _CreateDeviceAsyncEventSource CreateDeviceAsyncEventSource {
			get { return createDeviceAsyncEventSource; }
		}

		public _GetDeviceAsyncEventSource GetDeviceAsyncEventSource {
			get { return getDeviceAsyncEventSource; }
		}

		public _CreateDeviceAsyncPort CreateDeviceAsyncPort {
			get { return createDeviceAsyncPort; }
		}

		public _GetDeviceAsyncPort GetDeviceAsyncPort {
			get { return getDeviceAsyncPort; }
		}

		public _USBDeviceOpen USBDeviceOpen {
			get { return usbDeviceOpen; }
		}

		public _USBDeviceClose USBDeviceClose {
			get { return usbDeviceClose; }
		}

		public _GetDeviceClass GetDeviceClass {
			get { return getDeviceClass; }
		}

		public _GetDeviceSubClass GetDeviceSubClass{
			get { return getDeviceSubClass; }
		}
		public _GetDeviceProtocol GetDeviceProtocol {
			get { return getDeviceProtocol; }
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

		public _GetDeviceAddress GetDeviceAddress {
			get { return getDeviceAddress; }
		}

		public _GetDeviceBusPowerAvailable GetDeviceBusPowerAvailable {
			get { return getDeviceBusPowerAvailable; }
		}

		public _GetDeviceSpeed GetDeviceSpeed {
			get { return getDeviceSpeed; }
		}

		public _GetNumberOfConfigurations GetNumberOfConfigurations {
			get { return getNumberOfConfigurations; }
		}

		public _GetLocationID GetLocationID {
			get { return getLocationID; }
		}

		public _GetConfigurationDescriptorPtr GetConfigurationDescriptorPtr {
			get { return getConfigurationDescriptorPtr; }
		}

		public _GetConfiguration GetConfiguration {
			get { return getConfiguration; }
		}

		public _SetConfiguration SetConfiguration {
			get { return setConfiguration; }
		}

		public _GetBusFrameNumber GetBusFrameNumber {
			get { return getBusFrameNumber; }
		}

		public _ResetDevice ResetDevice {
			get { return resetDevice; }
		}

		public _DeviceRequest DeviceRequest {
			get { return deviceRequest; }
		}

		public _DeviceRequestAsync DeviceRequestAsync {
			get { return deviceRequestAsync; }
		}

		public _CreateInterfaceIterator CreateInterfaceIterator {
			get { return createInterfaceIterator; }
		}

		public virtual _USBDeviceOpenSeize USBDeviceOpenSeize {
			get { throw new NotImplementedException (); }
		}

		public virtual _DeviceRequestTO DeviceRequestTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _DeviceRequestAsyncTO DeviceRequestAsyncTO {
			get { throw new NotImplementedException (); }
		}

		public virtual _USBDeviceSuspend USBDeviceSuspend {
			get { throw new NotImplementedException (); }
		}

		public virtual _USBDeviceAbortPipeZero USBDeviceAbortPipeZero {
			get { throw new NotImplementedException (); }
		}

		public virtual _USBGetManufacturerStringIndex USBGetManufacturerStringIndex {
			get { throw new NotImplementedException (); }
		}

		public virtual _USBGetProductStringIndex USBGetProductStringIndex {
			get { throw new NotImplementedException (); }
		}

		public virtual _USBGetSerialNumberStringIndex USBGetSerialNumberStringIndex {
			get { throw new NotImplementedException (); }
		}

		public virtual _USBDeviceReEnumerate USBDeviceReEnumerate {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetBusMicroFrameNumber GetBusMicroFrameNumber {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetIOUSBLibVersion GetIOUSBLibVersion {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetBusFrameNumberWithTime GetBusFrameNumberWithTime {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetUSBDeviceInformation GetUSBDeviceInformation {
			get { throw new NotImplementedException (); }
		}

		public virtual _RequestExtraPower RequestExtraPower {
			get { throw new NotImplementedException (); }
		}

		public virtual _ReturnExtraPower ReturnExtraPower {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetExtraPowerAllocated GetExtraPowerAllocated {
			get { throw new NotImplementedException (); }
		}

		public virtual _GetBandwidthAvailableForDevice GetBandwidthAvailableForDevice {
			get { throw new NotImplementedException (); }
		}

		public delegate IOReturn _CreateDeviceAsyncEventSource (IntPtr self, out CFRunLoopSourceRef source);
		public delegate CFRunLoopSourceRef _GetDeviceAsyncEventSource (IntPtr self);
		public delegate IOReturn _CreateDeviceAsyncPort (IntPtr self, out mach_port_t port); 
		public delegate mach_port_t _GetDeviceAsyncPort (IntPtr self);
		public delegate IOReturn _USBDeviceOpen (IntPtr self);
		public delegate IOReturn _USBDeviceClose (IntPtr self);
		public delegate IOReturn _GetDeviceClass (IntPtr self, out UInt8 devClass);
		public delegate IOReturn _GetDeviceSubClass (IntPtr self, out UInt8 devSubClass);
		public delegate IOReturn _GetDeviceProtocol (IntPtr self, out UInt8 devProtocol);
		public delegate IOReturn _GetDeviceVendor (IntPtr self, out UInt16 devVendor);
		public delegate IOReturn _GetDeviceProduct (IntPtr self, out UInt16 devProduct);
		public delegate IOReturn _GetDeviceReleaseNumber (IntPtr self, out UInt16 devRelNum);
		public delegate IOReturn _GetDeviceAddress (IntPtr self, out USBDeviceAddress addr);
		public delegate IOReturn _GetDeviceBusPowerAvailable (IntPtr self, out UInt32 powerAvailable);
		public delegate IOReturn _GetDeviceSpeed (IntPtr self, out UInt8 devSpeed);
		public delegate IOReturn _GetNumberOfConfigurations (IntPtr self, out UInt8 numConfig);
		public delegate IOReturn _GetLocationID (IntPtr self, out UInt32 locationID);
		public delegate IOReturn _GetConfigurationDescriptorPtr (IntPtr self, UInt8 configIndex, out IOUSBConfigurationDescriptorPtr desc);
		public delegate IOReturn _GetConfiguration (IntPtr self, out UInt8 configNum);
		public delegate IOReturn _SetConfiguration (IntPtr self, UInt8 configNum);
		public delegate IOReturn _GetBusFrameNumber (IntPtr self, out UInt64 frame, out AbsoluteTime atTime);
		public delegate IOReturn _ResetDevice (IntPtr self);
		public delegate IOReturn _DeviceRequest (IntPtr self, ref IOUSBDeviceRequest req);
		public delegate IOReturn _DeviceRequestAsync (IntPtr self, [MarshalAs (UnmanagedType.LPStruct)] IOUSBDeviceRequest req, IOAsyncCallback1 callback, IntPtr refCon);
		public delegate IOReturn _CreateInterfaceIterator (IntPtr self, [MarshalAs (UnmanagedType.LPStruct)] IOUSBFindInterfaceRequest req, out io_iterator_t iter);
		public delegate IOReturn _USBDeviceOpenSeize (IntPtr self);
		public delegate IOReturn _DeviceRequestTO (IntPtr self, ref IOUSBDevRequestTO req);
		public delegate IOReturn _DeviceRequestAsyncTO (IntPtr self, [MarshalAs (UnmanagedType.LPStruct)] IOUSBDevRequestTO req, IOAsyncCallback1 callback, IntPtr refCon);
		public delegate IOReturn _USBDeviceSuspend (IntPtr self, Boolean suspend);
		public delegate IOReturn _USBDeviceAbortPipeZero (IntPtr self);
		public delegate IOReturn _USBGetManufacturerStringIndex (IntPtr self, out UInt8 msi);
		public delegate IOReturn _USBGetProductStringIndex (IntPtr self, out UInt8 psi);
		public delegate IOReturn _USBGetSerialNumberStringIndex (IntPtr self, out UInt8 snsi);
		public delegate IOReturn _USBDeviceReEnumerate (IntPtr self, UInt32 options);
		public delegate IOReturn _GetBusMicroFrameNumber (IntPtr self, out UInt64 microFrame, out AbsoluteTime atTime);
		public delegate IOReturn _GetIOUSBLibVersion (IntPtr self, out NumVersion ioUSBLibVersion, out NumVersion usbFamilyVersion);
		public delegate IOReturn _GetBusFrameNumberWithTime (IntPtr self, out UInt64 frame, out AbsoluteTime atTime);
		public delegate IOReturn _GetUSBDeviceInformation (IntPtr self, out UInt32 info);
		public delegate IOReturn _RequestExtraPower (IntPtr self, UInt32 type, UInt32 requestedPower, out UInt32 powerAvailable);
		public delegate IOReturn _ReturnExtraPower (IntPtr self, UInt32 type, UInt32 powerReturned);
		public delegate IOReturn _GetExtraPowerAllocated (IntPtr self, UInt32 type, out UInt32 powerAllocated);
		public delegate IOReturn _GetBandwidthAvailableForDevice (IntPtr self, out UInt32 bandwidth);
	}

	[Guid ("152FC496-4891-11D5-9D52-000A27801E86")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface182 : IOUSBDeviceInterface
	{
		_USBDeviceOpenSeize usbDeviceOpenSeize;
		_DeviceRequestTO deviceRequestTO;
		_DeviceRequestAsyncTO deviceRequestAsyncTO;
		_USBDeviceSuspend usbDeviceSuspend;
		_USBDeviceAbortPipeZero usbeviceAbortPipeZero;
		_USBGetManufacturerStringIndex usbGetManufacturerStringIndex;
		_USBGetProductStringIndex usbGetProductStringIndex;
		_USBGetSerialNumberStringIndex usbGetSerialNumberStringIndex;

		public override _USBDeviceOpenSeize USBDeviceOpenSeize {
			get { return usbDeviceOpenSeize; }
		}

		public override _DeviceRequestTO DeviceRequestTO {
			get { return deviceRequestTO; }
		}

		public override _DeviceRequestAsyncTO DeviceRequestAsyncTO {
			get { return deviceRequestAsyncTO; }
		}

		public override _USBDeviceSuspend USBDeviceSuspend {
			get { return usbDeviceSuspend; }
		}

		public override _USBDeviceAbortPipeZero USBDeviceAbortPipeZero {
			get { return usbeviceAbortPipeZero; }
		}

		public override _USBGetManufacturerStringIndex USBGetManufacturerStringIndex {
			get { return usbGetManufacturerStringIndex; }
		}

		public override _USBGetProductStringIndex USBGetProductStringIndex {
			get { return usbGetProductStringIndex; }
		}

		public override _USBGetSerialNumberStringIndex USBGetSerialNumberStringIndex {
			get { return usbGetSerialNumberStringIndex; }
		}
	}

	[Guid ("3C9EE1EB-2402-11B2-8E7E-000A27801E86")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface187 : IOUSBDeviceInterface182
	{
		_USBDeviceReEnumerate usbDeviceReEnumerate;

		public override _USBDeviceReEnumerate USBDeviceReEnumerate {
			get { return usbDeviceReEnumerate; }
		}
	}

	[Guid ("C809B8D8-0884-11D7-BB96-0003933E3E3E")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface197 : IOUSBDeviceInterface187
	{
		_GetBusMicroFrameNumber getBusMicroFrameNumber;
		_GetIOUSBLibVersion getIOUSBLibVersion;

		public override _GetBusMicroFrameNumber GetBusMicroFrameNumber {
			get { return getBusMicroFrameNumber; }
		}

		public override _GetIOUSBLibVersion GetIOUSBLibVersion {
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
		_GetBusFrameNumberWithTime getBusFrameNumberWithTime;

		public override _GetBusFrameNumberWithTime GetBusFrameNumberWithTime {
			get { return getBusFrameNumberWithTime; }
		}
	}

	[Guid ("01A2D0E9-42F6-4A87-8B8B-77057C8CE0CE")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface320 : IOUSBDeviceInterface300
	{
		_GetUSBDeviceInformation getUSBDeviceInformation;
		_RequestExtraPower requestExtraPower;
		_ReturnExtraPower returnExtraPower;
		_GetExtraPowerAllocated getExtraPowerAllocated;

		public override _GetUSBDeviceInformation GetUSBDeviceInformation {
			get { return getUSBDeviceInformation; }
		}

		public override _RequestExtraPower RequestExtraPower {
			get { return requestExtraPower; }
		}

		public override _ReturnExtraPower ReturnExtraPower {
			get { return returnExtraPower; }
		}

		public override _GetExtraPowerAllocated GetExtraPowerAllocated {
			get { return getExtraPowerAllocated; }
		}
	}

	[Guid ("A33CF047-4B5B-48E2-B57D-0207FCEAE13B")]
	[StructLayout (LayoutKind.Sequential)]
	class IOUSBDeviceInterface550 : IOUSBDeviceInterface320
	{
		_GetBandwidthAvailableForDevice getBandwidthAvailableForDevice;
		
		public override _GetBandwidthAvailableForDevice GetBandwidthAvailableForDevice {
			get { return getBandwidthAvailableForDevice; }
		}
	}
}

