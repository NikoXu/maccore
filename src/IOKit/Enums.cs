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
		[Key ("IOServicePublish")]
		Publish,

		[Key ("IOServiceFirstPublish")]
		FirstPublish,

		[Key ("IOServiceMatched")]
		Matched,

		[Key ("IOServiceFirstMatch")]
		FirstMatch,

		[Key ("IOServiceTerminate")]
		Terminated
	}

	public enum InterestType
	{
		[Key ("IOGeneralInterest")]
		General,

		[Key ("IOBusyInterest")]
		Busy,
		
		[Key ("IOAppPowerStateInterest")]
		AppPowerState,
		
		[Key ("IOPriorityPowerStateInterest")]
		PriorityPowerState
	}

	[Flags]
	public enum RegistryIteratorOptions
	{

		None           = 0x00000000,
		Recusive       = 0x00000001,
		IncludeParents = 0x00000002
	}

	public enum RegistryPlane
	{
		[Key ("IOService")]
		Service,

		[Key ("IOPower")]
		Power,

		[Key ("IODeviceTree")]
		DeviceTree,

		[Key ("IOAudio")]
		Audio,

		[Key ("IOFireWire")]
		FireWire,

		[Key ("IOUSB")]
		USB
	}

	public enum OSMessageID
	{
		Notification  = 53,
		AsyncComplete = 57
	}

	public static class EnumExtensions
	{
		public static string GetKey (this NotificationType value)
		{
			return GetKeyForEnum (value);
		}

		public static string GetKey (this InterestType value)
		{
			return GetKeyForEnum (value);
		}

		public static string GetKey (this RegistryPlane value)
		{
			return GetKeyForEnum (value);
		}

		static string GetKeyForEnum (Enum value)
		{
			var field = value.GetType ().GetField (value.ToString ());
			foreach (KeyAttribute attribute in field.GetCustomAttributes (typeof(KeyAttribute), false))
				return attribute.Key;
			throw new ArgumentException ("Enum does not have a KeyAttribute", "value");
		}
	}

	// used by GetKey () extension method to convert enum to string
	[System.AttributeUsage (System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	internal sealed class KeyAttribute : Attribute
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
}
