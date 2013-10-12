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

namespace MonoMac.IOKit
{
	public enum IOReturn : int
	{
		/* From IOKit/IOReturn.h */

		Success          = 0,

		[Message ("general error")]
		Error            = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2bc,

		[Message ("can't allocate memory")]
		NoMemory         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2bd,

		[Message ("resource shortage")]
		NoResources      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2be,

		[Message ("error during IPC")]
		IPCError         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2bf,

		[Message ("no such device")]
		NoDevice         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c0,

		[Message ("privilege violation")]
		NotPrivileged    = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c1,

		[Message ("invalid argument")]
		BadArgument      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c2,

		[Message ("device read locked")]
		LockedRead       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c3,

		[Message ("device write locked")]
		LockedWrite      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c4,

		[Message ("exclusive access and device already open")]
		ExclusiveAccess  = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c5,

		[Message ("sent/received messages had different msg_id")]
		BadMessageID     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c6,

		[Message ("unsupported function")]
		Unsupported      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c7,

		[Message ("misc. VM failure ")]
		VMError          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c8,

		[Message ("// internal error")]
		InternalError    = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2c9,

		[Message ("General I/O error")]
		IOError          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ca,

		//[Message ("???")]
		//???Error       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cb,

		[Message ("can't acquire lock")]
		CannotLock       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cc,

		[Message ("device not open")]
		NotOpen          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cd,

		[Message ("read not supported ")]
		NotReadable      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ce,

		[Message ("write not supported")]
		NotWritable      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2cf,

		[Message ("alignment error")]
		NotAligned       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d0,

		[Message ("Media Error")]
		BadMedia         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d1,

		[Message ("device(s) still open")]
		StillOpen        = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d2,

		[Message ("rld failure")]
		RLDError         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d3,

		[Message ("DMA failure")]
		DMAError         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d4,

		[Message ("Device Busy")]
		Busy             = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d5,

		[Message ("I/O Timeout")]
		Timeout          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d6,

		[Message ("device offline")]
		Offline          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d7,

		[Message ("not ready")]
		NotReady         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d8,

		[Message ("device not attached")]
		NotAttached      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2d9,

		[Message ("no DMA channels left")]
		NoChannels       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2da,

		[Message ("no space for data")]
		NoSpace          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2db,

		//[Message ("???")]
		//???Error       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2dc, 

		[Message ("port already exists")]
		PortExists       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2dd,

		//[Message ("???")]
		//???Error       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2dc, 

		[Message ("can't wire down physical memory")]
		CannotWire       = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2de,

		[Message ("no interrupt attached")]
		NoInterrupt      = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2df,

		[Message ("no DMA frames enqueued")]
		NoFrames         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e0,

		[Message ("oversized msg received on interrupt port")]
		MessageTooLarge  = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e1,

		[Message ("not permitted")]
		NotPermitted     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e2,

		[Message ("no power to device")]
		NoPower          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e3,

		[Message ("media not present")]
		NoMedia          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e4,

		[Message ("media not formatted")]
		UnformattedMedia = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e5,

		[Message ("no such mode")]
		UnsupportedMode  = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e6,

		[Message ("data underrun")]
		Underrun         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e7,

		[Message ("data overrun")]
		Overrun          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e8,

		[Message ("the device is not working properly!")]
		DeviceError	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2e9,

		[Message ("a completion routine is required")]
		NoCompletion	 = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ea,

		[Message ("operation aborted")]
		Aborted	         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2eb,

		[Message ("bus bandwidth would be exceeded")]
		NoBandwidth	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ec,

		[Message ("device not responding")]
		NotResponding	 = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ed,

		[Message ("isochronous I/O request for distant past!")]
		IsoTooOld	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ee,

		[Message ("isochronous I/O request for distant future")]
		IsoTooNew	     = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2ef,

		[Message ("data was not found")]
		NotFound         = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x2f0,

		[Message ("should never be seen")]
		Invalid          = IOKit.sys_iokit | IOKit.sub_iokit_common | 0x1,

		// IOUSBFamily error codes

		[Message (" Pipe ref not recognized")]
		IOUSBUnknownPipeErr                               = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x61,
		[Message ("Too many pipes")]
		IOUSBTooManyPipesErr                              = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x60,
		[Message ("no async port")]
		IOUSBNoAsyncPortErr                               = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x5f,
		[Message ("not enough pipes in interface")]
		IOUSBNotEnoughPipesErr                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x5e,
		[Message ("not enough power for selected configuration")]
		IOUSBNotEnoughPowerErr                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x5d,
		[Message ("Endpoint Not found")]
		IOUSBEndpointNotFound                             = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x57,
		[Message ("Configuration Not found")]
		IOUSBConfigNotFound                               = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x56,
		[Message ("Transaction timed out")]
		IOUSBTransactionTimeout                           = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x51,
		[Message ("The transaction has been returned to the caller")]
		IOUSBTransactionReturned                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x50,
		[Message ("Pipe has stalled, error needs to be cleared")]
		IOUSBPipeStalled                                  = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4f,
		[Message ("Interface ref not recognized")]
		IOUSBInterfaceNotFound                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4e,
		[Message ("Attempted to use user land low latency isoc calls w/out calling PrepareBuffer (on the data buffer, first ")]
		IOUSBLowLatencyBufferNotPreviouslyAllocated       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4d,
		[Message ("Attempted to use user land low latency isoc calls w/out calling PrepareBuffer (on the frame list, first")]
		IOUSBLowLatencyFrameListNotPreviouslyAllocated    = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4c,
		[Message ("Error to hub on high speed bus trying to do split transaction")]
		IOUSBHighSpeedSplitError                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4b,
		[Message ("A synchronous USB request was made on the workloop thread (from a callback?,.  Only async requests are permitted in that case")]
		IOUSBSyncRequestOnWLThread                        = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x4a,
		[Message ("Name is deprecated, see below")]
		IOUSBDeviceNotHighSpeed                           = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x49,
		[Message ("The device has been tranferred to another controller for enumeration")]
		IOUSBDeviceTransferredToCompanion                 = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x49,
		[Message ("IOUSBPipe::ClearPipeStall should not be called recursively")]
		IOUSBClearPipeStallNotRecursive                   = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x48,
		[Message ("Port was not suspended")]
		IOUSBDevicePortWasNotSuspended                    = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x47,
		[Message ("The endpoint was not created because the controller cannot support more endpoints")]
		IOUSBEndpointCountExceeded                        = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x46,
		[Message ("The device cannot be enumerated because the controller cannot support more devices")]
		IOUSBDeviceCountExceeded                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x45,
		[Message ("The request cannot be completed because the XHCI controller does not support streams")]
		IOUSBStreamsNotSupported                          = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x44,
		[Message ("An endpoint found in a SuperSpeed device is invalid (usually because there is no Endpoint Companion Descriptor,")]
		IOUSBInvalidSSEndpoint                            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x43,

		[Message ("Link error")]
		IOUSBLinkErr           = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x10,
		[Message ("Transaction not sent")]
		IOUSBNotSent2Err       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0f,
		[Message ("Transaction not sent")]
		IOUSBNotSent1Err       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0e,
		[Message ("Buffer Underrun (Host hardware failure on data out, PCI busy?)")]
		IOUSBBufferUnderrunErr = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0d,
		[Message ("Buffer Overrun (Host hardware failure on data out, PCI busy?)")]
		IOUSBBufferOverrunErr  = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0c,
		[Message ("Reserved")]
		IOUSBReserved2Err      = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0b,
		[Message ("Reserved")]
		IOUSBReserved1Err      = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0a,
		[Message ("Pipe stall, Bad or wrong PID")]
		IOUSBWrongPIDErr       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x07,
		[Message ("Pipe stall, PID CRC error")]
		IOUSBPIDCheckErr       = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x06,
		[Message ("Pipe stall, Bad data toggle")]
		IOUSBDataToggleErr     = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x03,
		[Message ("Pipe stall, bitstuffing")]
		IOUSBBitstufErr        = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x02,
		[Message ("Pipe stall, bad CRC")]
		IOUSBCRCErr            = IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x01,

		/* from IOBluetoothTypes.h */

		[Message ("Device Reset Error")]
		IOBluetoothDeviceResetError           = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 1,
		[Message ("Connection Already Exists")]
		IOBluetoothConnectionAlreadyExists    = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 2,
		[Message ("No HCI Controller")]
		IOBluetoothNoHCIController            = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 3,
		[Message ("HCI Power States Not Supported")]
		IOBluetoothHCIPowerStatesNotSupported = IOKit.sys_iokit | IOKit.sub_iokit_bluetooth | 4,

		/* from mach/kern_return.h */

		[Message ("Specified address is not currently valid.")]
		KernInvalidAddress = 1,

		[Message ("Specified memory is valid, but does not permit the required forms of access.")]
		KernProtectionFailure = 2,

		[Message ("The address range specified is already in use, or no address range of the size specified could be found.")]
		KernNoSpace = 3,

		[Message ("The function requested was not applicable to this type of argument, or an argument is invalid.")]
		KernInvalidArgument = 4,

		[Message ("The function could not be performed.  A catch-all.")]
		KernFailure = 5,

		[Message ("A system resource could not be allocated to fulfill this request.  This failure may not be permanent.")]
		KernResourceShortage = 6,

		[Message ("The task in question does not hold receive rights for the port argument.")]
		KernNotReceiver = 7,

		[Message ("Bogus access restriction.")]
		KernNoAccess = 8,

		[Message ("During a page fault, the target address refers to a memory object that has been destroyed.  This failure is permanent.")]
		KernMemoryFailure = 9,

		[Message ("During a page fault, the memory object indicated that the data could not be returned.  This failure may be temporary; future attempts to access this same data may succeed, as defined by the memory object.")]
		KernMemoryError = 10,

		[Message ("The receive right is already a member of the portset.")]
		KernAlreadyInSet = 11,

		[Message ("The receive right is not a member of a port set.")]
		KernNotInSet = 12,

		[Message ("The name already denotes a right in the task.")]
		KernNameExists = 13,

		[Message ("The operation was aborted.  Ipc code will catch this and reflect it as a message error.")]
		KernAborted = 14,

		[Message ("The name doesn't denote a right in the task.")]
		KernInvalidName = 15,

		[Message ("Target task isn't an active task.")]
		KernInvalidTask = 16,

		[Message ("The name denotes a right, but not an appropriate right.")]
		KernInvalidRight = 17,

		[Message ("A blatant range error.")]
		KernInvalidValue = 18,

		[Message ("Operation would overflow limit on user-references.")]
		KernUserReferencesOverflow = 19,

		[Message ("The supplied (port) capability is improper.")]
		KernInvalidCapability = 20,

		[Message ("The task already has send or receive rights for the port under another name.")]
		KernRightExists = 21,

		[Message ("Target host isn't actually a host.")]
		KernInvalidHost = 22,

		[Message ("An attempt was made to supply \"precious\" data for memory that is already present in a memory object.")]
		KernMemoryPresent = 23,

		[Message ("A page was requested of a memory manager via memory_object_data_request for an object using a MEMORY_OBJECT_COPY_CALL strategy, with the VM_PROT_WANTS_COPY flag being used to specify that the page desired is for a copy of the object, and the memory manager has detected the page was pushed into a copy of the object while the kernel was walking the shadow chain from the copy to the object. This error code is delivered via memory_object_data_error and is handled by the kernel (it forces the kernel to restart the fault). It will not be seen by users.")]
		KernMemeoryDataWasMoved = 24,

		[Message ("A strategic copy was attempted of an object upon which a quicker copy is now possible. The caller should retry the copy using vm_object_copy_quickly. This error code is seen only by the kernel.")]
		KernMemoryRestartCopy = 25,

		[Message ("An argument applied to assert processor set privilege was not a processor set control port.")]
		KernInvalidProcessorSet =26,

		[Message ("The specified scheduling attributes exceed the thread's limits.")]
		KernPolicyLimit = 27,

		[Message ("The specified scheduling policy is not currently enabled for the processor set.")]
		KernINVALID_POLICY = 28,

		[Message ("The external memory manager failed to initialize the memory object.")]
		KernInvalidObject = 29,

		[Message ("A thread is attempting to wait for an event for which  there is already a waiting thread.")]
		KernAleadyWaiting = 30,

		[Message ("An attempt was made to destroy the default processor set.")]
		KernDefaultSet = 31,

		[Message ("An attempt was made to fetch an exception port that is protected, or to abort a thread while processing a protected exception.")]
		KernExceptionProtected = 32,

		[Message ("A ledger was required but not supplied.")]
		KernInvalidLedger = 33,

		[Message ("The port was not a memory cache control port.")]
		KernInvalidMemoryControl = 34,

		[Message ("An argument supplied to assert security privilege \t was not a host security port.")]
		KernInvalidSecurity = 35,

		[Message ("thread_depress_abort was called on a thread which was not currently depressed.")]
		KernNotDepressed = 36,

		[Message ("Object has been terminated and is no longer available")]
		KernTerminated = 37,

		[Message ("Lock set has been destroyed and is no longer available.")]
		KernLockSetDestroyed = 38,

		[Message ("The thread holding the lock terminated before releasing the lock")]
		KernLockUnstable = 39,

		[Message ("The lock is already owned by another thread")]
		KernLockOwned = 40,

		[Message ("The lock is already owned by the calling thread")]
		KernLockOwnedSelf = 41,

		[Message ("Semaphore has been destroyed and is no longer available.")]
		KernSemaphoreDestroyed = 42,

		[Message ("Return from RPC indicating the target server was  terminated before it successfully replied ")]
		KernRPCServerTerminated = 43,

		[Message ("Terminate an orphaned activation.")]
		KernRPCTerminateOrphan = 44,

		[Message ("Allow an orphaned activation to continue executing.")]
		KernRPCContinueOrphan = 45,

		[Message ("Empty thread activation (No thread linked to it)")]
		KernNotSupported = 46,

		[Message ("Remote node down or inaccessible.")]
		KernNodeDown = 47,

		[Message ("A signalled thread was not actually waiting.")]
		KernNotWaiting = 48,

		[Message ("Some thread-oriented operation (semaphore_wait) timed out")]
		KernOperationTimedOut = 49,

		[Message ("During a page fault, indicates that the page was rejected as a result of a signature check.")]
		KernCodeSignError = 50,
	}

	public static class IOReturnExtensions
	{
		public static string GetMessage (this IOReturn value)
		{
			var attribute = (MessageAttribute)Attribute.GetCustomAttribute
				(value.GetType ().GetField (value.ToString ()),
				 typeof(MessageAttribute));
			return attribute == null ? value.ToString () : attribute.Message;
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

	[AttributeUsage (AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	internal sealed class MessageAttribute : Attribute
	{
		readonly string message;

		public string Message {
			get {
				return message;
			}
		}

		public MessageAttribute (string message)
		{
			this.message = message;
		}
	}
}
