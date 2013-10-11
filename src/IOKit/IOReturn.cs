//
// IOReturn.cs
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

namespace MonoMac.IOKit
{
	public enum IOReturn : int
	{
		/* From IOKit/IOReturn.h */

		/// <summary>
		/// OK
		/// </summary>
		Success          = 0,

		/// <summary>
		/// general error
		/// </summary>
		Error            = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2bc,

		/// <summary>
		/// can't allocate memory
		/// </summary>
		NoMemory         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2bd,

		/// <summary>
		/// resource shortage
		/// </summary>
		NoResources      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2be,

		/// <summary>
		/// error during IPC
		/// </summary>
		IPCError         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2bf,

		/// <summary>
		/// no such device
		/// </summary>
		NoDevice         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c0,

		/// <summary>
		/// privilege violation
		/// </summary>
		NotPrivileged    = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c1,

		/// <summary>
		/// invalid argument
		/// </summary>
		BadArgument      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c2,

		/// <summary>
		/// device read locked
		/// </summary>
		LockedRead       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c3,

		/// <summary>
		/// device write locked
		/// </summary>
		LockedWrite      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c4,

		/// <summary>
		/// exclusive access and device already open
		/// </summary>
		ExclusiveAccess  = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c5,

		/// <summary>
		/// sent/received messages had different msg_id
		/// </summary>
		BadMessageID     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c6,

		/// <summary>
		/// unsupported function
		/// </summary>
		Unsupported      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c7,

		/// <summary>
		/// misc. VM failure 
		/// </summary>
		VMError          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c8,

		/// <summary>
		/// // internal error
		/// </summary>
		InternalError    = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c9,

		/// <summary>
		/// General I/O error
		/// </summary>
		IOError          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ca,

		/// <summary>
		/// ???
		/// </summary>
		//???Error       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cb,

		/// <summary>
		/// can't acquire lock
		/// </summary>
		CannotLock       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cc,

		/// <summary>
		/// device not open
		/// </summary>
		NotOpen          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cd,

		/// <summary>
		/// read not supported 
		/// </summary>
		NotReadable      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ce,

		/// <summary>
		/// write not supported
		/// </summary>
		NotWritable      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cf,

		/// <summary>
		/// alignment error
		/// </summary>
		NotAligned       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d0,

		/// <summary>
		/// Media Error
		/// </summary>
		BadMedia         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d1,

		/// <summary>
		/// device(s) still open
		/// </summary>
		StillOpen        = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d2,

		/// <summary>
		/// rld failure
		/// </summary>
		RLDError         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d3,

		/// <summary>
		/// DMA failure
		/// </summary>
		DMAError         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d4,

		/// <summary>
		/// Device Busy
		/// </summary>
		Busy             = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d5,

		/// <summary>
		/// I/O Timeout
		/// </summary>
		Timeout          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d6,

		/// <summary>
		/// device offline
		/// </summary>
		Offline          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d7,

		/// <summary>
		/// not ready
		/// </summary>
		NotReady         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d8,

		/// <summary>
		/// device not attached
		/// </summary>
		NotAttached      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d9,

		/// <summary>
		/// no DMA channels left
		/// </summary>
		NoChannels       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2da,

		/// <summary>
		/// no space for data
		/// </summary>
		NoSpace          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2db,

		/// <summary>
		/// ???
		/// </summary>
		//???Error       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2dc, 

		/// <summary>
		/// port already exists
		/// </summary>
		PortExists       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2dd,

		/// <summary>
		/// ???
		/// </summary>
		//???Error       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2dc, 

		/// <summary>
		/// can't wire down 
		/// </summary>
		CannotWire       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2de,
		//   physical memory

		/// <summary>
		/// no interrupt attached
		/// </summary>
		NoInterrupt      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2df,

		/// <summary>
		/// no DMA frames enqueued
		/// </summary>
		NoFrames         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e0,

		/// <summary>
		/// oversized msg received on interrupt port
		/// </summary>
		MessageTooLarge  = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e1,

		/// <summary>
		/// not permitted
		/// </summary>
		NotPermitted     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e2,

		/// <summary>
		/// no power to device
		/// </summary>
		NoPower          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e3,

		/// <summary>
		/// media not present
		/// </summary>
		NoMedia          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e4,

		/// <summary>
		/// media not formatted
		/// </summary>
		UnformattedMedia = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e5,

		/// <summary>
		/// no such mode
		/// </summary>
		UnsupportedMode  = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e6,

		/// <summary>
		/// data underrun
		/// </summary>
		Underrun         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e7,

		/// <summary>
		/// data overrun
		/// </summary>
		Overrun          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e8,

		/// <summary>
		/// the device is not working properly!
		/// </summary>
		DeviceError	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e9,

		/// <summary>
		/// a completion routine is required
		/// </summary>
		NoCompletion	 = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ea,

		/// <summary>
		/// operation aborted
		/// </summary>
		Aborted	         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2eb,

		/// <summary>
		/// bus bandwidth would be exceeded
		/// </summary>
		NoBandwidth	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ec,

		/// <summary>
		/// device not responding
		/// </summary>
		NotResponding	 = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ed,

		/// <summary>
		/// isochronous I/O request for distant past!
		/// </summary>
		IsoTooOld	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ee,

		/// <summary>
		/// isochronous I/O request for distant future
		/// </summary>
		IsoTooNew	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ef,

		/// <summary>
		/// data was not found
		/// </summary>
		NotFound         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2f0,

		/// <summary>
		/// should never be seen
		/// </summary>
		Invalid          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x1,

		// IOUSBFamily error codes
		// Note that the '= IOKit.sys_iokit | IOKit.sub_iokit_usb | x' translates to 0xe0004xxx, where xxx is the value in parenthesis as a hex number.

		USBUnknownPipeErr                               = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x61,	// 0xe0004061  Pipe ref not recognized
		USBTooManyPipesErr                              = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x60,	// 0xe0004060  Too many pipes
		USBNoAsyncPortErr                               = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x5f,	// 0xe000405f  no async port
		USBNotEnoughPipesErr                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x5e,	// 0xe000405e  not enough pipes in interface
		USBNotEnoughPowerErr                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x5d,	// 0xe000405d  not enough power for selected configuration
		USBEndpointNotFound                             = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x57,	// 0xe0004057  Endpoint Not found
		USBConfigNotFound                               = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x56,	// 0xe0004056  Configuration Not found
		USBTransactionTimeout                           = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x51,	// 0xe0004051  Transaction timed out
		USBTransactionReturned                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x50,	// 0xe0004050  The transaction has been returned to the caller
		USBPipeStalled                                  = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4f,	// 0xe000404f  Pipe has stalled, error needs to be cleared
		USBInterfaceNotFound                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4e,	// 0xe000404e  Interface ref not recognized
		USBLowLatencyBufferNotPreviouslyAllocated       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4d,	// 0xe000404d  Attempted to use user land low latency isoc calls w/out calling PrepareBuffer (on the data buffer, first 
		USBLowLatencyFrameListNotPreviouslyAllocated    = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4c,	// 0xe000404c  Attempted to use user land low latency isoc calls w/out calling PrepareBuffer (on the frame list, first
		USBHighSpeedSplitError                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4b,	// 0xe000404b  Error to hub on high speed bus trying to do split transaction
		USBSyncRequestOnWLThread                        = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4a,	// 0xe000404a  A synchronous USB request was made on the workloop thread (from a callback?,.  Only async requests are permitted in that case
		USBDeviceNotHighSpeed                           = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x49,	// 0xe0004049  Name is deprecated, see below
		USBDeviceTransferredToCompanion                 = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x49,	// 0xe0004049  The device has been tranferred to another controller for enumeration
		USBClearPipeStallNotRecursive                   = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x48,	// 0xe0004048  IOUSBPipe::ClearPipeStall should not be called recursively
		USBDevicePortWasNotSuspended                    = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x47,	// 0xe0004047  Port was not suspended
		USBEndpointCountExceeded                        = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x46,	// 0xe0004046  The endpoint was not created because the controller cannot support more endpoints
		USBDeviceCountExceeded                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x45,	// 0xe0004045  The device cannot be enumerated because the controller cannot support more devices
		USBStreamsNotSupported                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x44,	// 0xe0004044  The request cannot be completed because the XHCI controller does not support streams
		USBInvalidSSEndpoint                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x43,	// 0xe0004043  An endpoint found in a SuperSpeed device is invalid (usually because there is no Endpoint Companion Descriptor,

		USBLinkErr           = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x10,		// 0xe0004010
		USBNotSent2Err       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0f,		// 0xe000400f Transaction not sent
		USBNotSent1Err       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0e,		// 0xe000400e Transaction not sent
		USBBufferUnderrunErr = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0d,		// 0xe000400d Buffer Underrun (Host hardware failure on data out, PCI busy?)
		USBBufferOverrunErr  = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0c,		// 0xe000400c Buffer Overrun (Host hardware failure on data out, PCI busy?)
		USBReserved2Err      = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0b,		// 0xe000400b Reserved
		USBReserved1Err      = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0a,		// 0xe000400a Reserved
		USBWrongPIDErr       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x07,		// 0xe0004007 Pipe stall, Bad or wrong PID
		USBPIDCheckErr       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x06,		// 0xe0004006 Pipe stall, PID CRC error
		USBDataToggleErr     = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x03,		// 0xe0004003 Pipe stall, Bad data toggle
		USBBitstufErr        = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x02,		// 0xe0004002 Pipe stall, bitstuffing
		USBCRCErr            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x01,		// 0xe0004001 Pipe stall, bad CRC


		/* from IOBluetoothTypes.h */

		IOBluetoothDeviceResetError           = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 1,
		IOBluetoothConnectionAlreadyExists    = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 2,
		IOBluetoothNoHCIController            = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 3,
		IOBluetoothHCIPowerStatesNotSupported = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 4,
	}
}
