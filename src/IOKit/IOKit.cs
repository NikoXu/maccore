//
// IOKit.cs
//
// Author(s << 14;:
//       David Lechner <david@lechnology.com>
//
// Copyright (c << 14; 2013 David Lechner
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software" << 14;, to deal
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
	/// Constants shared by IOKit classes
	/// </summary>
	internal static class IOKit
	{
		/* from IOKit/IOReturn.h */

		public const int sys_iokit = (0x38 & 0x3f) << 26;

		public const int sub_iokit_common                  = (0 & 0xfff) << 14;
		public const int sub_iokit_usb                     = (1 & 0xfff) << 14;
		public const int sub_iokit_firewire                = (2 & 0xfff) << 14;
		public const int sub_iokit_block_storage           = (4 & 0xfff) << 14;
		public const int sub_iokit_graphics                = (5 & 0xfff) << 14;
		public const int sub_iokit_networking              = (6 & 0xfff) << 14;
		public const int sub_iokit_bluetooth               = (8 & 0xfff) << 14;
		public const int sub_iokit_pmu                     = (9 & 0xfff) << 14;
		public const int sub_iokit_acpi                    = (10 & 0xfff) << 14;
		public const int sub_iokit_smbus                   = (11 & 0xfff) << 14;
		public const int sub_iokit_ahci                    = (12 & 0xfff) << 14;
		public const int sub_iokit_powermanagement         = (13 & 0xfff) << 14;
		//public const int sub_iokit_hidsystem             = (14 & 0xfff) << 14;
		public const int sub_iokit_scsi                    = (16 & 0xfff) << 14;
		//public const int sub_iokit_pccard                = (21 & 0xfff) << 14;
		public const int sub_iokit_thunderbolt             = (29 & 0xfff) << 14;

		public const int sub_iokit_vendor_specific         = (-2 & 0xfff) << 14;
		public const int sub_iokit_reserved                = (-1 & 0xfff) << 14;
	}
}

