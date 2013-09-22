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

	/// <summary>
	/// Defines message type constants for several IOKit messaging API's.
	/// </summary>
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


		/// <summary>
		/// Indicates the device is about to move to a lower power state.
		/// Sent to IOKit interest notification clients of type <c>kIOAppPowerStateInterest</c>
		/// and <code>kIOGeneralInterest</code>.
		/// </summary>
		DeviceWillPowerOff       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x210)),

		/// <summary>
		/// Indicates the device has just moved to a higher power state.
		/// Sent to IOKit interest notification clients of type <c>kIOAppPowerStateInterest</c>
		/// and <c>kIOGeneralInterest</c>.
		/// </summary>
		DeviceHasPoweredOn       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x230)),

		/* In-kernel system shutdown and restart notifications */

		/// <summary>
		/// Indicates an imminent system shutdown. Recipients have a limited 
		/// amount of time to respond, otherwise the system will timeout and 
		/// shutdown even without a response.
		/// Delivered to in-kernel IOKit drivers via <c>IOService::systemWillShutdown()),</c>)), 
		/// and to clients of <c>registerPrioritySleepWakeInterest()),</c>.
		/// Never delivered to user space notification clients.
		/// </summary>
		SystemWillPowerOff       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x250)),

		/// <summary>
		/// Indicates an imminent system restart. Recipients have a limited 
		/// amount of time to respond)), otherwise the system will timeout and 
		/// restart even without a response.
		/// Delivered to in-kernel IOKit drivers via <c>IOService::systemWillShutdown()),</c>)), 
		/// and to clients of <c>registerPrioritySleepWakeInterest()),</c>.
		/// Never delivered to user space notification clients.
		/// </summary>
		SystemWillRestart        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x310)),

		/// <summary>
		/// Indicates an imminent system shutdown, paging device now unavailable.
		/// Recipients have a limited amount of time to respond, otherwise the
		/// system will timeout and shutdown even without a response.
		/// Delivered to clients of <c>registerPrioritySleepWakeInterest()),</c>.
		/// Never delivered to user space notification clients.
		/// </summary>
		SystemPagingOff       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x255)),

		/* System sleep and wake notifications */

		/// <summary>
		/// Announces/Requests permission to proceed to system sleep.
		/// Delivered to in-kernel IOKit drivers via <c>kIOGeneralInterest</c>
		/// and <c>kIOPriorityPowerStateInterest</c>.
		/// Delivered to user clients of <c>IORegisterForSystemPower</c>.
		/// </summary>
		CanSystemSleep           = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x270)),

		/// <summary>
		/// Announces that the system has retracted a previous attempt to sleep, 
		/// it follows <code>kIOMessageCanSystemSleep</code>.
		/// Delivered to in-kernel IOKit drivers via <code>kIOGeneralInterest</code>
		/// and <code>kIOPriorityPowerStateInterest</code>.
		/// Delivered to user clients of <code>IORegisterForSystemPower</code>.
		/// </summary>
		SystemWillNotSleep       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x290)),

		/// <summary>
		/// Announces that sleep is beginning.
		/// Delivered to in-kernel IOKit drivers via <code>kIOGeneralInterest</code>
		/// and <code>kIOPriorityPowerStateInterest</code>.
		/// Delivered to user clients of <code>IORegisterForSystemPower</code>.
		/// </summary>
		SystemWillSleep          = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x280)),

		/// <summary>
		/// Announces that the system is beginning to power the device tree, most 
		/// devices are unavailable at this point..
		/// Delivered to in-kernel IOKit drivers via <code>kIOGeneralInterest</code>
		/// and <code>kIOPriorityPowerStateInterest</code>.
		/// Delivered to user clients of <code>IORegisterForSystemPower</code>.
		/// </summary>
		SystemWillPowerOn        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x320)),

		/// <summary>
		/// Announces that the system and its devices have woken up.
		/// Delivered to in-kernel IOKit drivers via <code>kIOGeneralInterest</code>
		/// and <code>kIOPriorityPowerStateInterest</code>.
		/// Delivered to user clients of <code>IORegisterForSystemPower</code>.
		/// </summary>
		SystemHasPoweredOn       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x300)),

		/* Unused and deprecated notifications */

		/// <summary>
		/// Delivered to <code>kIOAppPowerStateInterest</code> clients of 
		/// devices that implement their own idle timeouts.
		/// <remarks>This message type is almost never used.</remarks>
		/// </summary>
		CanDevicePowerOff        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x200)),

		/// <summary>
		/// This IOKit interest notification is largely unused, 
		/// it's not very interesting.
		/// </summary>
		DeviceWillNotPowerOff    = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x220)),

		[Obsolete ("This IOKit message is unused.")]
		SystemWillNotPowerOff    = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x260)),

		[Obsolete ("This IOKit message is unused.")]
		CanSystemPowerOff        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x240)),

		/// <summary>
		/// IOService power mgt does not send kIOMessageDeviceWillPowerOn.
		/// </summary>
		DeviceWillPowerOn        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x215)),

		/// <summary>
		/// IOService power mgt does not send kIOMessageDeviceHasPoweredOff.
		/// </summary>
		DeviceHasPoweredOff      = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_common | 0x225)),


		/* IOUSBFamily message codes */

		/// <summary>
		/// Message sent to a hub to reset a particular port.
		/// </summary>
		USBMessageHubResetPort                = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x01)),

		/// <summary>
		/// Message sent to a hub to suspend a particular port.
		/// </summary>
		USBMessageHubSuspendPort              = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x02)),

		/// <summary>
		/// Message sent to a hub to resume a particular port.
		/// </summary>
		USBMessageHubResumePort               = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x03)),

		/// <summary>
		/// Message sent to a hub to inquire whether a particular port has a device connected or not.
		/// </summary>
		USBMessageHubIsDeviceConnected        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x04)),

		/// <summary>
		/// Message sent to a hub to inquire whether a particular port is enabled or not.
		/// </summary>
		USBMessageHubIsPortEnabled            = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x05)),

		/// <summary>
		/// Message sent to a hub to reenumerate the device attached to a particular port
		/// </summary>
		USBMessageHubReEnumeratePort          = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x06)),

		/// <summary>
		/// Message sent to a device indicating that the port it is attached to has been reset.
		/// </summary>
		USBMessagePortHasBeenReset            = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0a)),

		/// <summary>
		/// Message sent to a device indicating that the port it is attached to has been resumed.
		/// </summary>
		USBMessagePortHasBeenResumed          = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0b)),

		/// <summary>
		/// Message sent to a hub to clear the transaction translator.
		/// </summary>
		USBMessageHubPortClearTT              = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0c)),

		/// <summary>
		/// Message sent to a device indicating that the port it is attached to has been suspended.
		/// </summary>
		USBMessagePortHasBeenSuspended        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0d)),

		/// <summary>
		/// Message sent from a third party.  Uses IOUSBThirdPartyParam to encode the sender's ID.
		/// </summary>
		USBMessageFromThirdParty              = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0e)),

		/// <summary>
		/// Message indicating that the hub driver received a resume request for a port that was not suspended
		/// </summary>
		USBMessagePortWasNotSuspended         = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x0f)),

		/// <summary>
		/// Message from a driver to a bus that an express card will disconnect on sleep and thus shouldn't wake.
		/// </summary>
		USBMessageExpressCardCantWake         = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x10)),

		/// <summary>
		/// Message from the composite driver indicating that it has finished re-configuring the device after a reset
		/// </summary>
		USBMessageCompositeDriverReconfigured = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x11)),

		/// <summary>
		/// Message sent to a hub to set the # of ms required when resuming a particular port.
		/// </summary>
		USBMessageHubSetPortRecoveryTime      = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x12)),

		/// <summary>
		/// Message sent to the clients of the device's hub parent, when a device causes an overcurrent condition.
		/// The message argument contains the locationID of the device.
		/// </summary>
		USBMessageOvercurrentCondition        = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x13)),

		/// <summary>
		/// Message sent to the clients of the device's hub parent, when a device causes an low power notice to be displayed.
		/// The message argument contains the locationID of the device
		/// </summary>
		USBMessageNotEnoughPower              = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x14)),

		/// <summary>
		/// Generic message sent from controller user client to controllers.
		/// </summary>
		USBMessageController                  = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x15)),

		/// <summary>
		///  Message from the HC Wakeup code indicating that a Root Hub port has a wake event.
		/// </summary>
		USBMessageRootHubWakeEvent            = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x16)),

		/// <summary>
		/// Message to ask any clients using extra current to release it if possible.
		/// </summary>
		USBMessageReleaseExtraCurrent         = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x17)),

		/// <summary>
		/// Message to ask any clients using extra current to attempt to allocate it some more.
		/// </summary>
		USBMessageReallocateExtraCurrent      = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x18)),

		/// <summary>
		/// Message sent to a device when endpoints cannot be created because the USB controller ran out of resources.
		/// </summary>
		USBMessageEndpointCountExceeded       = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x19)),

		/// <summary>
		/// Message sent by a hub when a device cannot be enumerated because the USB controller ran out of resources.
		/// </summary>
		USBMessageDeviceCountExceeded         = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x1a)),

		/// <summary>
		/// Message sent by a built-in hub when a device was disconnected.
		/// </summary>
		USBMessageHubPortDeviceDisconnected   = unchecked((uint)(IOKit.sys_iokit | IOKit.sub_iokit_usb | 0x1b)),
	}
}

