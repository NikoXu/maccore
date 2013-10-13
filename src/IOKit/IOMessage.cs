//
// IOMessage.cs
//
// Author(s):
//       David Lechner <david@lechnology.com>
//
// Copyright (c) 2013 David Lechner
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software")), to deal
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
	public enum IOMessage : uint
	{
		/* from IOKit/IOMessage.h */

		ServiceIsTerminated      = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x010)),
		ServiceIsSuspended       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x020)),
		ServiceIsResumed         = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x030)),
		ServiceIsRequestingClose = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x100)),
		ServiceIsAttemptingOpen  = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x101)),
		ServiceWasClosed         = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x110)),
		ServiceBusyStateChange   = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x120)),
		ConsoleSecurityChange    = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x128)),
		ServicePropertyChange    = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x130)),
		CopyClientID             = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x330)),
		SystemCapabilityChange   = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x340)),
		DeviceSignaledWakeup     = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x350)),
		DeviceWillPowerOff       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x210)),
		DeviceHasPoweredOn       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x230)),

		/* In-kernel system shutdown and restart notifications */

		SystemWillPowerOff       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x250)),
		SystemWillRestart        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x310)),
		SystemPagingOff          = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x255)),

		/* System sleep and wake notifications */

		CanSystemSleep           = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x270)),
		SystemWillNotSleep       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x290)),
		SystemWillSleep          = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x280)),
		SystemWillPowerOn        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x320)),
		SystemHasPoweredOn       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x300)),

		/* Unused and deprecated notifications */

		CanDevicePowerOff        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x200)),
		DeviceWillNotPowerOff    = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x220)),
		[Obsolete ("This IOKit message is unused.")]
		SystemWillNotPowerOff    = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x260)),
		[Obsolete ("This IOKit message is unused.")]
		CanSystemPowerOff        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x240)),
		DeviceWillPowerOn        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x215)),
		DeviceHasPoweredOff      = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x225)),

		/* IOUSBFamily message codes */

		USBHubResetPort                = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x01)),
		USBHubSuspendPort              = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x02)),
		USBHubResumePort               = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x03)),
		USBHubIsDeviceConnected        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x04)),
		USBHubIsPortEnabled            = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x05)),
		USBHubReEnumeratePort          = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x06)),
		USBPortHasBeenReset            = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0a)),
		USBPortHasBeenResumed          = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0b)),
		USBHubPortClearTT              = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0c)),
		USBPortHasBeenSuspended        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0d)),
		USBFromThirdParty              = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0e)),
		USBPortWasNotSuspended         = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0f)),
		USBExpressCardCantWake         = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x10)),
		USBCompositeDriverReconfigured = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x11)),
		USBHubSetPortRecoveryTime      = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x12)),
		USBOvercurrentCondition        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x13)),
		USBNotEnoughPower              = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x14)),
		USBController                  = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x15)),
		USBRootHubWakeEvent            = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x16)),
		USBReleaseExtraCurrent         = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x17)),
		USBReallocateExtraCurrent      = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x18)),
		USBEndpointCountExceeded       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x19)),
		USBDeviceCountExceeded         = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x1a)),
		USBHubPortDeviceDisconnected   = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x1b)),
	}
}

