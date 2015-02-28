//
// IOSerialKeys.cs
//
// Author(s):
//       David Lechner <david@lechnology.com>
//
// Copyright (c) 2015 David Lechner
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

using MonoMac.CoreFoundation;

namespace MonoMac.IOKit
{
	public class IOSerialBSDClient : IOService
	{
		public const string TypeKey = "IOSerialBSDClientType";

		public const string AllTypes = "IOSerialStream";
		public const string ModemType = "IOModemSerialStream";
		public const string RS232Type = "IORS232SerialStream";

		public const string DeviceKey = "IOTTYDevice";
		public const string BaseNameKey = "IOTTYBaseName";
		public const string SuffixKey = "IOTTYSuffix";

		public const string CalloutDeviceKey = "IOCalloutDevice";
		public const string DialinDeviceKey = "IODialinDevice";

		public const string WaitForIdleKey = "IOTTYWaitForIdle";

		internal IOSerialBSDClient (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		public IOSerialBSDType IOSerialBSDType {
			get {
				var str = CFType.GetCFObject<CFString> (CreateCFProperty (TypeKey));
				CFType.Release (str.Handle);
				switch ((string)str) {
				case AllTypes:
					return IOSerialBSDType.All;
				case ModemType:
					return IOSerialBSDType.Modem;
				case RS232Type:
					return IOSerialBSDType.RS232;
				default:
					throw new Exception (string.Format ("Unknown IOSerialBSDType '{0}'", (string)str));
				}
			}
		}

		public string Device {
			get {
				var str = CFType.GetCFObject<CFString> (CreateCFProperty (DeviceKey));
				CFType.Release (str.Handle);
				return str.ToString ();
			}
		}

		public string BaseName {
			get {
				var str = CFType.GetCFObject<CFString> (CreateCFProperty (BaseNameKey));
				CFType.Release (str.Handle);
				return str.ToString ();
			}
		}

		public string Suffix {
			get {
				var str = CFType.GetCFObject<CFString> (CreateCFProperty (SuffixKey));
				CFType.Release (str.Handle);
				return str.ToString ();
			}
		}

		public string CalloutDevice {
			get {
				var str = CFType.GetCFObject<CFString> (CreateCFProperty (CalloutDeviceKey));
				CFType.Release (str.Handle);
				return str.ToString ();
			}
		}

		public string DialinDevice {
			get {
				var str = CFType.GetCFObject<CFString> (CreateCFProperty (DialinDeviceKey));
				CFType.Release (str.Handle);
				return str.ToString ();
			}
		}
	}

	public enum IOSerialBSDType {
		All,
		Modem,
		RS232
	}
}

