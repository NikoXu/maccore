//
// Enums.cs
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
	public enum NotificationType
	{
		/// <summary>
		/// Delivered when an IOService is registered.
		/// </summary>
		[Key ("IOServicePublish")]
		Publish,

		/// <summary>
		/// Delivered when an IOService is registered, but only once per IOService instance.
		/// Some IOService's may be reregistered when their state is changed.
		/// </summary>
		[Key ("IOServiceFirstPublish")]
		FirstPublish,

		/// <summary>
		/// Delivered when an IOService has had all matching drivers in the kernel probed and started.
		/// </summary>
		[Key ("IOServiceMatched")]
		Matched,

		/// <summary>
		/// Delivered when an IOService has had all matching drivers in the kernel probed and started,
		/// but only once per IOService instance. Some IOService's may be reregistered when their state is changed.
		/// </summary>
		[Key ("IOServiceFirstMatch")]
		FirstMatch,

		/// <summary>
		/// Delivered after an IOService has been terminated.
		/// </summary>
		[Key ("IOServiceTerminate")]
		Terminated
	}

	public enum InterestType
	{
		/// <summary>
		///  General state changes delivered via the IOService::message API.
		/// </summary>
		[Key ("IOGeneralInterest")]
		General,

		/// <summary>
		/// Delivered when the IOService changes its busy state to or from zero.
		/// The message argument contains the new busy state causing the notification.
		/// </summary>
		[Key ("IOBusyInterest")]
		Busy,
		
		[Key ("IOAppPowerStateInterest")]
		AppPowerState,
		
		[Key ("IOPriorityPowerStateInterest")]
		PriorityPowerState
	}

	/// <summary>
	/// options for IORegistry.CreateIterator (), IORegistryEntry.CreateIterator () and
	/// IORegistryEntry.SearchCFProperty ()
	/// </summary>
	[Flags]
	public enum RegistryIteratorOptions
	{
		/// <summary>
		/// No flags are set.
		/// </summary>
		None = 0x00000000,

		/// <summary>
		/// kIORegistryIterateRecursively
		/// </summary>
		Recusive = 0x00000001,

		/// <summary>
		/// kIORegistryIterateParents
		/// </summary>
		IncludeParents = 0x00000002
	};

	public enum RegistryPlane
	{
		/// <summary>
		/// kIOServicePlane
		/// </summary>
		[Key ("IOService")]
		Service,

		/// <summary>
		/// kIOPowerPlane.
		/// </summary>
		[Key ("IOSPower")]
		Power,

		/// <summary>
		/// kIODeviceTreePlane
		/// </summary>
		[Key ("IODeviceTree")]
		DeviceTree,

		/// <summary>
		/// kIOAudioPlane
		/// </summary>
		[Key ("IOAudio")]
		Audio,

		/// <summary>
		/// kIOFireWirePlane
		/// </summary>
		[Key ("IOFireWire")]
		FireWire,

		/// <summary>
		/// kIOUSBPlane
		/// </summary>
		[Key ("IOUSB")]
		USB
	}

	/// <summary>
	/// Used by IOReceivePort
	/// </summary>
	public enum OSMessageID
	{
		/// <summary>
		/// kOSNotificationMessageID
		/// </summary>
		Notification  = 53,

		/// <summary>
		/// kOSAsyncCompleteMessageID
		/// </summary>
		AsyncComplete = 57
	}
}

public static class EnumUtil
{
	/// <summary>
	/// Gets the key from the KeyAttribute of an Enum value.
	/// </summary>
	/// <returns>The key.</returns>
	/// <param name="value">Value.</param>
	public static string GetKey (this Enum value)
	{
		var field = value.GetType ().GetField (value.ToString ());
		foreach (KeyAttribute attribute in field.GetCustomAttributes (typeof(KeyAttribute), false))
			return attribute.Key;
		throw new ArgumentException ("Enum does not have a KeyAttribute", "value");
	}
}

/// <summary>
/// Allows enums to be converted to the corresponding key string.
/// </summary>
[System.AttributeUsage (System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class KeyAttribute : Attribute
{
	readonly string key;

	public string Key {
		get {
			return key;
		}
	}

	public KeyAttribute (string key)
	{
		this.key = key;
	}
}
