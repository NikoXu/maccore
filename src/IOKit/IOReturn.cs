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
using MonoMac.Kernel.Mach;
using MonoMac.Foundation;

namespace MonoMac.IOKit
{
	public enum IOReturn : int
	{
		/* From IOKit/IOReturn.h */

		Success          = 0,
		Error            = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2bc,
		NoMemory         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2bd,
		NoResources      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2be,
		IPCError         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2bf,
		NoDevice         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c0,
		NotPrivileged    = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c1,
		BadArgument      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c2,
		LockedRead       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c3,
		LockedWrite      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c4,
		ExclusiveAccess  = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c5,
		BadMessageID     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c6,
		Unsupported      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c7,
		VMError          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c8,
		InternalError    = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c9,
		IOError          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ca,
		//???Error       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cb,
		CannotLock       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cc,
		NotOpen          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cd,
		NotReadable      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ce,
		NotWritable      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cf,
		NotAligned       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d0,
		BadMedia         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d1,
		StillOpen        = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d2,
		RLDError         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d3,
		DMAError         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d4,
		Busy             = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d5,
		Timeout          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d6,
		Offline          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d7,
		NotReady         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d8,
		NotAttached      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d9,
		NoChannels       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2da,
		NoSpace          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2db,
		//???Error       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2dc, 
		PortExists       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2dd,
		//???Error       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2dc, 
		CannotWire       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2de,
		NoInterrupt      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2df,
		NoFrames         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e0,
		MessageTooLarge  = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e1,
		NotPermitted     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e2,
		NoPower          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e3,
		NoMedia          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e4,
		UnformattedMedia = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e5,
		UnsupportedMode  = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e6,
		Underrun         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e7,
		Overrun          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e8,
		DeviceError	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e9,
		NoCompletion	 = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ea,
		Aborted	         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2eb,
		NoBandwidth	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ec,
		NotResponding	 = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ed,
		IsoTooOld	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ee,
		IsoTooNew	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ef,
		NotFound         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2f0,
		Invalid          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x1,

		// IOUSBFamily error codes

		IOUSBUnknownPipeErr                               = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x61,
		IOUSBTooManyPipesErr                              = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x60,
		IOUSBNoAsyncPortErr                               = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x5f,
		IOUSBNotEnoughPipesErr                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x5e,
		IOUSBNotEnoughPowerErr                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x5d,
		IOUSBEndpointNotFound                             = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x57,
		IOUSBConfigNotFound                               = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x56,
		IOUSBTransactionTimeout                           = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x51,
		IOUSBTransactionReturned                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x50,
		IOUSBPipeStalled                                  = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4f,
		IOUSBInterfaceNotFound                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4e,
		IOUSBLowLatencyBufferNotPreviouslyAllocated       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4d,
		IOUSBLowLatencyFrameListNotPreviouslyAllocated    = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4c,
		IOUSBHighSpeedSplitError                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4b,
		IOUSBSyncRequestOnWLThread                        = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4a,
		IOUSBDeviceNotHighSpeed                           = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x49,
		IOUSBDeviceTransferredToCompanion                 = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x49,
		IOUSBClearPipeStallNotRecursive                   = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x48,
		IOUSBDevicePortWasNotSuspended                    = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x47,
		IOUSBEndpointCountExceeded                        = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x46,
		IOUSBDeviceCountExceeded                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x45,
		IOUSBStreamsNotSupported                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x44,
		IOUSBInvalidSSEndpoint                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x43,

		IOUSBLinkErr           = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x10,
		IOUSBNotSent2Err       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0f,
		IOUSBNotSent1Err       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0e,
		IOUSBBufferUnderrunErr = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0d,
		IOUSBBufferOverrunErr  = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0c,
		IOUSBReserved2Err      = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0b,
		IOUSBReserved1Err      = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0a,
		IOUSBWrongPIDErr       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x07,
		IOUSBPIDCheckErr       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x06,
		IOUSBDataToggleErr     = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x03,
		IOUSBBitstufErr        = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x02,
		IOUSBCRCErr            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x01,

		/* from IOBluetoothTypes.h */

		IOBluetoothDeviceResetError           = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 1,
		IOBluetoothConnectionAlreadyExists    = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 2,
		IOBluetoothNoHCIController            = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 3,
		IOBluetoothHCIPowerStatesNotSupported = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 4,

		/* from OBEX.h */

		OBEXGeneralError                 = -21850,
		OBEXNoResourcesError             = -21851,
		OBEXUnsupportedError             = -21852,
		OBEXInternalError                = -21853,
		OBEXBadArgumentError             = -21854,
		OBEXTimeoutError                 = -21855,
		OBEXBadRequestError              = -21856,
		OBEXCancelledError               = -21857,
		OBEXForbiddenError               = -21858,

		OBEXUnauthorizedError            = -21859,
		OBEXNotAcceptableError           = -21860,
		OBEXConflictError                = -21861,
		OBEXMethodNotAllowedError        = -21862,
		OBEXNotFoundError                = -21863,
		OBEXNotImplementedError          = -21864,
		OBEXPreconditionFailedError      = -21865,

		OBEXSessionBusyError             = -21875,
		OBEXSessionNotConnectedError     = -21876,
		OBEXSessionBadRequestError       = -21877,
		OBEXSessionBadResponseError      = -21878,
		OBEXSessionNoTransportError      = -21879,
		OBEXSessionTransportDiedError    = -21880,
		OBEXSessionTimeoutError          = -21881,
		OBEXSessionAlreadyConnectedError = -21882,

		/* from mach/kern_return.h */

		KernInvalidAddress         = 1,
		KernProtectionFailure      = 2,
		KernNoSpace                = 3,
		KernInvalidArgument        = 4,
		KernFailure                = 5,
		KernResourceShortage       = 6,
		KernNotReceiver            = 7,
		KernNoAccess               = 8,
		KernMemoryFailure          = 9,
		KernMemoryError            = 10,
		KernAlreadyInSet           = 11,
		KernNotInSet               = 12,
		KernNameExists             = 13,
		KernAborted                = 14,
		KernInvalidName            = 15,
		KernInvalidTask            = 16,
		KernInvalidRight           = 17,
		KernInvalidValue           = 18,
		KernUserReferencesOverflow = 19,
		KernInvalidCapability      = 20,
		KernRightExists            = 21,
		KernInvalidHost            = 22,
		KernMemoryPresent          = 23,
		KernMemeoryDataWasMoved    = 24,
		KernMemoryRestartCopy      = 25,
		KernInvalidProcessorSet    = 26,
		KernPolicyLimit            = 27,
		KernINVALID_POLICY         = 28,
		KernInvalidObject          = 29,
		KernAleadyWaiting          = 30,
		KernDefaultSet             = 31,
		KernExceptionProtected     = 32,
		KernInvalidLedger          = 33,
		KernInvalidMemoryControl   = 34,
		KernInvalidSecurity        = 35,
		KernNotDepressed           = 36,
		KernTerminated             = 37,
		KernLockSetDestroyed       = 38,
		KernLockUnstable           = 39,
		KernLockOwned              = 40,
		KernLockOwnedSelf          = 41,
		KernSemaphoreDestroyed     = 42,
		KernRPCServerTerminated    = 43,
		KernRPCTerminateOrphan     = 44,
		KernRPCContinueOrphan      = 45,
		KernNotSupported           = 46,
		KernNodeDown               = 47,
		KernNotWaiting             = 48,
		KernOperationTimedOut      = 49,
		KernCodeSignError          = 50,
	}

#if !COREBUILD
	public static class IOReturnExtensions
	{
		public static NSError ToNSError (this IOReturn value)
		{
			return NSError.FromDomain (NSError.MachErrorDomain, (int)value);
		}

		public static NSErrorException ToNSErrorException (this IOReturn value)
		{
			return new NSErrorException (value.ToNSError ());
		}

		public static ErrorSystem GetSystem (this IOReturn value)
		{
			return Error.GetSystem ((int)value);
		}

		public static int GetSubSystem (this IOReturn value)
		{
			return Error.GetSubSystem ((int)value);
		}

		public static int GetCode (this IOReturn value)
		{
			return Error.GetCode ((int)value);
		}
	}
#endif
}
