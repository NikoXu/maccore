//
// IOConnection.cs
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
using MonoMac.Kernel.Mach;
using MonoMac.ObjCRuntime;

using CFStringRef = System.IntPtr;
using CFTypeRef = System.IntPtr;
using IOOptionBits = System.UInt32;
using io_connect_t = System.IntPtr;
using io_service_t = System.IntPtr;
using kern_return_t = MonoMac.IOKit.IOReturn;
using mach_port_t = System.IntPtr;
using task_port_t = System.IntPtr;
using uint32_t = System.UInt32;
using vm_address_t = System.IntPtr;
using vm_size_t = System.IntPtr;

namespace MonoMac.IOKit
{
	public class IOConnection : IOObject
	{
		IOService service;

		internal IOConnection (IntPtr handle, bool owns) : base (handle, false)
		{
			if (!owns)
				IOConnectAddRef (handle);
		}

		/// <summary>
		/// Gets the IOService that this instance was opened on.
		/// </summary>
		public IOService Service {
			get {
				ThrowIfDisposed ();
				if (service == null) {
					IntPtr serviceRef;
					var result = IOConnectGetService (Handle, out serviceRef);
					ThrowIfError (result);
					service = new IOService (serviceRef, true);
				}
				return service;
			}
		}

		/// <summary>
		/// Close a connection to an IOService.
		/// </summary>
		/// <remarks>A connection created with IOService.Open () should be closed
		/// when the connection is no longer to be used.</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been closed or disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public void Close ()
		{
			ThrowIfDisposed ();
			var result = IOServiceClose (Handle);
			ThrowIfError (result);
			Handle = IntPtr.Zero;
		}

		/// <summary>
		/// Set a port to receive family specific notifications.
		/// </summary>
		/// <param name="type">The type of notification requested, not interpreted by IOKit and family defined.</param>
		/// <param name="port">The port to which to send notifications.</param>
		/// <param name="reference">Some families may support passing a reference parameter for the callers use with the notification.</param>
		/// <remarks>This is a generic method to pass a mach port send right to be be used by family specific notifications.</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been closed or disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public void SetNotificationPort (uint type, Port port, UIntPtr reference = default (UIntPtr))
		{
			ThrowIfDisposed ();
			var result = IOConnectSetNotificationPort (Handle, type, port.Handle, reference);
			ThrowIfError (result);
		}

		/// <summary>
		/// Set CF container based properties on a connection.
		/// </summary>
		/// <param name="properties">A CF container - commonly a CFDictionary but this is not enforced.
		/// The container should consist of objects which are understood by IOKit - these are currently :
		/// CFDictionary, CFArray, CFSet, CFString, CFData, CFNumber, CFBoolean, and are passed in the kernel
		/// as the corresponding OSDictionary etc. objects.</param>
		/// <remarks>This is a generic method to pass a CF container of properties to the connection.
		/// The properties are interpreted by the family and commonly represent configuration settings,
		/// but may be interpreted as anything.</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been closed or disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public void SetCFProperties (INativeObject properties)
		{
			ThrowIfDisposed ();
			var result = IOConnectSetCFProperties (Handle, properties.Handle);
			ThrowIfError (result);
		}

		/// <summary>
		/// Set a CF container based property on a connection.
		/// </summary>
		/// <param name="propertyName">The name of the property.</param>
		/// <param name="property">A CF container - should consist of objects which are understood by IOKit -
		/// these are currently : CFDictionary, CFArray, CFSet, CFString, CFData, CFNumber, CFBoolean,
		/// and are passed in the kernel as the corresponding OSDictionary etc. objects.</param>
		/// <remarks>This is a generic method to pass a CF property to the connection.
		/// The property is interpreted by the family and commonly represent configuration settings,
		/// but may be interpreted as anything.</remarks>
		/// <exception cref="ObjectDisposedException">If this instance has already been closed or disposed.</exception>
		/// <exception cref="IOKitException">If the method call failed.</exception>
		public void SetCFProperty (string name, INativeObject value)
		{
			ThrowIfDisposed ();
			var nameCFString = new CFString (name);
			var result = IOConnectSetCFProperty (Handle, nameCFString.Handle, value.Handle);
			ThrowIfError (result);
		}

		/// <summary>
		/// Inform a connection of a second connection.
		/// </summary>
		/// <param name="client">Another IOConnection created by IOService.Open ().</param>
		/// <remarks>This is a generic method to inform a family connection of a second connection, and is rarely used.</remarks>
		public void AddClient (IOConnection client)
		{
			ThrowIfDisposed ();
			var result = IOConnectAddClient (Handle, client.Handle);
			ThrowIfError (result);
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				IOConnectRelease (Handle);
				Handle = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Close a connection to an IOService and destroy the connect handle.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="connect">The connect handle created by IOServiceOpen. It will be destroyed by this function, and should not be released with IOObjectRelease.</param>
		/// <remarks>A connection created with the IOServiceOpen should be closed when the connection is no longer to be used with IOServiceClose.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOServiceClose (io_connect_t connect);

		/// <summary>
		/// Adds a reference to the connect handle.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="connect">The connect handle created by IOServiceOpen.</param>
		/// <remarks>Adds a reference to the connect handle.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectAddRef (io_connect_t connect);

		/// <summary>
		/// Remove a reference to the connect handle.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="connect">The connect handle created by IOServiceOpen.</param>
		/// <remarks>Removes a reference to the connect handle.
		/// If the last reference is removed an implicit IOServiceClose is performed.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectRelease (io_connect_t connect);

		/// <summary>
		/// Returns the IOService a connect handle was opened on.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="connect">The connect handle created by IOServiceOpen.</param>
		/// <param name="service">On succes, the service handle the connection was opened on,
		/// which should be released with IOObjectRelease.</param>
		/// <remarks>Finds the service object a connection was opened on.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectGetService (io_connect_t connect, out io_service_t service);

		/// <summary>
		/// Set a port to receive family specific notifications.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="connect">The connect handle created by IOServiceOpen.</param>
		/// <param name="type">The type of notification requested, not interpreted by IOKit and family defined.</param>
		/// <param name="port">The port to which to send notifications.</param>
		/// <param name="reference">Some families may support passing a reference parameter for the callers use with the notification.</param>
		/// <remarks>This is a generic method to pass a mach port send right to be be used by family specific notifications. </remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectSetNotificationPort (io_connect_t connect, uint32_t type, mach_port_t port, UIntPtr reference);

		/// <summary>
		/// Map hardware or shared memory into the caller's task.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="connect">The connect handle created by IOServiceOpen.</param>
		/// <param name="memoryType">What is being requested to be mapped, not interpreted by IOKit and family defined.
		/// The family may support physical hardware or shared memory mappings.</param>
		/// <param name="intoTask">The task port for the task in which to create the mapping.
		/// This may be different to the task which the opened the connection.</param>
		/// <param name="atAddress">An in/out parameter - if the kIOMapAnywhere option is not set,
		/// the caller should pass the address where it requests the mapping be created, otherwise nothing need to set on input.
		/// The address of the mapping created is passed back on sucess.</param>
		/// <param name="ofSize">The size of the mapping created is passed back on success.</param>
		/// <param name="options">Options.</param>
		/// <remarks>This is a generic method to create a mapping in the callers task.
		/// The family will interpret the type parameter to determine what sort of mapping is being requested.
		/// Cache modes and placed mappings may be requested by the caller.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectMapMemory(io_connect_t connect, uint32_t memoryType,
		                                               task_port_t intoTask, ref vm_address_t atAddress,
		                                               out vm_size_t ofSize, IOOptionBits options);

		/// <summary>
		/// Remove a mapping made with IOConnectMapMemory.
		/// </summary>
		/// <returns>A kern_return_t error code.</returns>
		/// <param name="connect">The connect handle created by IOServiceOpen.</param>
		/// <param name="memoryType">The memory type originally requested in IOConnectMapMemory.</param>
		/// <param name="fromTask">The task port for the task in which to remove the mapping.
		/// This may be different to the task which the opened the connection.</param>
		/// <param name="atAddress">The address of the mapping to be removed.</param>
		/// <remarks>This is a generic method to remove a mapping in the callers task.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectUnmapMemory(io_connect_t connect, uint32_t memoryType,
		                                                 task_port_t fromTask, ref vm_address_t atAddress);

		/// <summary>
		/// Set CF container based properties on a connection.
		/// </summary>
		/// <returns>A kern_return_t error code returned by the family.</returns>
		/// <param name="connect">The connect handle created by IOServiceOpen.</param>
		/// <param name="properties">A CF container - commonly a CFDictionary but this is not enforced.
		/// The container should consist of objects which are understood by IOKit - these are currently :
		/// CFDictionary, CFArray, CFSet, CFString, CFData, CFNumber, CFBoolean, and are passed in the kernel
		/// as the corresponding OSDictionary etc. objects.</param>
		/// <remarks>This is a generic method to pass a CF container of properties to the connection.
		/// The properties are interpreted by the family and commonly represent configuration settings,
		/// but may be interpreted as anything.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectSetCFProperties (io_connect_t connect, CFTypeRef properties);

		/// <summary>
		/// Set a CF container based property on a connection.
		/// </summary>
		/// <returns>A kern_return_t error code returned by the object.</returns>
		/// <param name="connect">The connect handle created by IOServiceOpen.</param>
		/// <param name="propertyName">The name of the property as a CFString.</param>
		/// <param name="property">A CF container - should consist of objects which are understood by IOKit -
		/// these are currently : CFDictionary, CFArray, CFSet, CFString, CFData, CFNumber, CFBoolean,
		/// and are passed in the kernel as the corresponding OSDictionary etc. objects.</param>
		/// <remarks>This is a generic method to pass a CF property to the connection.
		/// The property is interpreted by the family and commonly represent configuration settings,
		/// but may be interpreted as anything.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectSetCFProperty (io_connect_t connect, CFStringRef propertyName, CFTypeRef property);

		// Combined LP64 & ILP32 Extended IOUserClient::externalMethod

//		kern_return_t
//			IOConnectCallMethod(
//				mach_port_t	 connection,		// In
//				uint32_t	 selector,		// In
//				const uint64_t	*input,			// In
//				uint32_t	 inputCnt,		// In
//				const void      *inputStruct,		// In
//				size_t		 inputStructCnt,	// In
//				uint64_t	*output,		// Out
//				uint32_t	*outputCnt,		// In/Out
//				void		*outputStruct,		// Out
//				size_t		*outputStructCnt)	// In/Out
//				AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;
//
//		kern_return_t
//			IOConnectCallAsyncMethod(
//				mach_port_t	 connection,		// In
//				uint32_t	 selector,		// In
//				mach_port_t	 wake_port,		// In
//				uint64_t	*reference,		// In
//				uint32_t	 referenceCnt,		// In
//				const uint64_t	*input,			// In
//				uint32_t	 inputCnt,		// In
//				const void	*inputStruct,		// In
//				size_t		 inputStructCnt,	// In
//				uint64_t	*output,		// Out
//				uint32_t	*outputCnt,		// In/Out
//				void		*outputStruct,		// Out
//				size_t		*outputStructCnt)	// In/Out
//				AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;
//
//		kern_return_t
//			IOConnectCallStructMethod(
//				mach_port_t	 connection,		// In
//				uint32_t	 selector,		// In
//				const void	*inputStruct,		// In
//				size_t		 inputStructCnt,	// In
//				void		*outputStruct,		// Out
//				size_t		*outputStructCnt)	// In/Out
//				AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;
//
//		kern_return_t
//			IOConnectCallAsyncStructMethod(
//				mach_port_t	 connection,		// In
//				uint32_t	 selector,		// In
//				mach_port_t	 wake_port,		// In
//				uint64_t	*reference,		// In
//				uint32_t	 referenceCnt,		// In
//				const void	*inputStruct,		// In
//				size_t		 inputStructCnt,	// In
//				void		*outputStruct,		// Out
//				size_t		*outputStructCnt)	// In/Out
//				AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;
//
//		kern_return_t
//			IOConnectCallScalarMethod(
//				mach_port_t	 connection,		// In
//				uint32_t	 selector,		// In
//				const uint64_t	*input,			// In
//				uint32_t	 inputCnt,		// In
//				uint64_t	*output,		// Out
//				uint32_t	*outputCnt)		// In/Out
//				AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;
//
//		kern_return_t
//			IOConnectCallAsyncScalarMethod(
//				mach_port_t	 connection,		// In
//				uint32_t	 selector,		// In
//				mach_port_t	 wake_port,		// In
//				uint64_t	*reference,		// In
//				uint32_t	 referenceCnt,		// In
//				const uint64_t	*input,			// In
//				uint32_t	 inputCnt,		// In
//				uint64_t	*output,		// Out
//				uint32_t	*outputCnt)		// In/Out
//				AVAILABLE_MAC_OS_X_VERSION_10_5_AND_LATER;

//		kern_return_t
//			IOConnectTrap0(io_connect_t	connect,
//			               uint32_t		index );
//
//		kern_return_t
//			IOConnectTrap1(io_connect_t	connect,
//			               uint32_t		index,
//			               uintptr_t	p1 );
//
//		kern_return_t
//			IOConnectTrap2(io_connect_t	connect,
//			               uint32_t		index,
//			               uintptr_t	p1,
//			               uintptr_t	p2);
//
//		kern_return_t
//			IOConnectTrap3(io_connect_t	connect,
//			               uint32_t		index,
//			               uintptr_t	p1,
//			               uintptr_t	p2,
//			               uintptr_t	p3);
//
//		kern_return_t
//			IOConnectTrap4(io_connect_t	connect,
//			               uint32_t		index,
//			               uintptr_t	p1,
//			               uintptr_t	p2,
//			               uintptr_t	p3,
//			               uintptr_t	p4);
//
//		kern_return_t
//			IOConnectTrap5(io_connect_t	connect,
//			               uint32_t		index,
//			               uintptr_t	p1,
//			               uintptr_t	p2,
//			               uintptr_t	p3,
//			               uintptr_t	p4,
//			               uintptr_t	p5);
//
//		kern_return_t
//			IOConnectTrap6(io_connect_t	connect,
//			               uint32_t		index,
//			               uintptr_t	p1,
//			               uintptr_t	p2,
//			               uintptr_t	p3,
//			               uintptr_t	p4,
//			               uintptr_t	p5,
//			               uintptr_t	p6);

		/// <summary>
		/// Inform a connection of a second connection.
		/// </summary>
		/// <returns>A kern_return_t error code returned by the family.</returns>
		/// <param name="connect">The connect handle created by IOServiceOpen.</param>
		/// <param name="client">Another connect handle created by IOServiceOpen.</param>
		/// <remarks>This is a generic method to inform a family connection of a second connection, and is rarely used.</remarks>
		[DllImport (Constants.IOKitLibrary)]
		extern static kern_return_t IOConnectAddClient (io_connect_t connect, io_connect_t client);
	}
}

