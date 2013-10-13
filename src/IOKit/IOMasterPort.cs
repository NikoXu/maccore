//
// IOMasterPort.cs
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
using System.Runtime.InteropServices;
using MonoMac;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.Kernel.Mach;
using MonoMac.ObjCRuntime;

using CFDictionaryRef = System.IntPtr;
using CFMutableDictionaryRef = System.IntPtr;
using IOOptionBits = System.UInt32;
using io_iterator_t = System.IntPtr;
using io_name_t = System.String;
using io_registry_entry_t = System.IntPtr;
using io_string_t = System.String;
using kern_return_t = MonoMac.IOKit.IOReturn;
using mach_port_t = System.IntPtr;
using mach_timespec_t = MonoMac.Kernel.Mach.TimeSpec;
using uint32_t = System.UInt32;

namespace MonoMac.IOKit
{
	/// <summary>
	/// Mach port used to initiate communication with IOKit.
	/// </summary>
	public class IOMasterPort : Port
	{
		Lazy<IORegistryEntry> rootEntry;

		public IOMasterPort () : this (Port.NULL)
		{
			rootEntry = new Lazy<IORegistryEntry> (() => IORegistryEntry.GetRootEntryForPort (Handle));
		}

		[DllImport (Constants.IOKitLibrary, EntryPoint = "IOMasterPort")]
		extern static kern_return_t NewIOMasterPort (uint bootstrapPort, out mach_port_t masterPort);

		public IOMasterPort (uint bootstrapPort) : base (IntPtr.Zero)
		{
			IntPtr masterPort;
			var error = NewIOMasterPort (bootstrapPort, out masterPort);
			if (error != IOReturn.Success)
				throw new IOReturnException (error);
			Handle = masterPort;
		}

		public bool IsBusy {
			get { return BusyState > 0; }
		}

		public uint BusyState
		{
			get { return IOService.GetBusyStateForPort (Handle); }
		}

		public IORegistryEntry RootEntry {
			get { return rootEntry.Value; }
		}

		public IONotificationPort CreateNotificationPort ()
		{
			return new IONotificationPort (this);
		}

		public IOService GetMatchingService (CFDictionaryRef matchingDictionary)
		{
			return IOService.GetMatchingService (Handle, matchingDictionary);
		}

		public IOIterator<T> GetMatchingServices<T> (CFDictionaryRef matchingDictionary) where T : IOService
		{
			return IOService.GetMatchingServices<T> (Handle, matchingDictionary);
		}

		public bool Wait (double seconds)
		{
			return IOService.WaitForPort (Handle, seconds);
		}

		public IORegistryEntry FromPath (string path)
		{
			return IORegistryEntry.GetEntryFromPath (Handle, path);
		}
	
		public IORegistryIterator<IOObject> CreateRootIterator (RegistryPlane plane, RegistryIteratorOptions options)
		{
			return IORegistryEntry.CreateRootIteratorForPort (Handle, plane, options);
		}

		public NSMutableDictionary CreateMatchingDictionaryForBSDName (string bsdName)
		{
			return IOService.CreateMatchingDictionaryForBSDNameWithPort (Handle, bsdName);
		}
	}
}
