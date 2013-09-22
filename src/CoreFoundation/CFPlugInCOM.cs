//
// CFPlugInCOM.cs
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
using System.Runtime.InteropServices;

using LPVOID = System.IntPtr;
using REFIID = MonoMac.CoreFoundation.CFUUID.CFUUIDBytes;
using ULONG = System.UInt32;

namespace MonoMac.CoreFoundation
{
	public static class CFPlugInCOM
	{
		const uint SEVERITY_SUCCESS = 0;
		const uint SEVERITY_ERROR = 1;

		public static bool SUCCEEDED (this HRESULT result)
		{
			return (uint)result >= 0;
		}

		public static bool FAILED (this HRESULT result)
		{
			return (uint)result < 0;
		}

		public static bool IS_ERROR (this HRESULT result)
		{
			return (uint)result >> 31 == SEVERITY_ERROR;
		}

		public static ushort HRESULT_CODE (this HRESULT result)
		{
			return (ushort)((uint)result & 0xFFFF);
		}

		public static ushort HRESULT_FACILITY (this HRESULT result)
		{
			return (ushort)((uint)result >> 16 & 0x1FFF);
		}

		public static byte HRESULT_SEVERITY (this HRESULT result)
		{
			return (byte)((uint)result >> 31 & 0x1);
		}

		public static CFUUID GetUuid (this Type t)
		{
			var guidAttributes = t.GetCustomAttributes (typeof(GuidAttribute), true);
			foreach (GuidAttribute attribute in guidAttributes)
				return new CFUUID (attribute.Value, true);
			throw new ArgumentException (string.Format ("Type {0} does not contain a GuidAttribute", t.Name), "t");
		}
	}

	public enum HRESULT : uint
	{
		/* Pre-defined success HRESULTS */
		S_OK =           0x00000000U,
		S_FALSE =        0x00000001U,

		/* Common error HRESULTS */
		E_UNEXPECTED =   0x8000FFFFU,
		E_NOTIMPL =      0x80000001U,
		E_OUTOFMEMORY =  0x80000002U,
		E_INVALIDARG =   0x80000003U,
		E_NOINTERFACE =  0x80000004U,
		E_POINTER =      0x80000005U,
		E_HANDLE =       0x80000006U,
		E_ABORT =        0x80000007U,
		E_FAIL =         0x80000008U,
		E_ACCESSDENIED = 0x80000009U,
	}

	[StructLayout (LayoutKind.Sequential)]
	public abstract class IUnknown
	{
		/* IUNKNOWN_C_GUTS */
		public IntPtr reserved;
		public QueryInterface queryInterface;
		public AddRef addRef;
		public Release release;

		public delegate HRESULT QueryInterface (IntPtr thisPointer, REFIID iid, out LPVOID ppv);
		public delegate ULONG AddRef (IntPtr thisPointer);
		public delegate ULONG Release (IntPtr thisPointer);
	}
}

