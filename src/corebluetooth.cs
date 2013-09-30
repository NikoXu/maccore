//
// corebluetooth.cs
//
// Author:
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
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace MonoMac.CoreBluetooth
{
	[BaseType (typeof (NSObject), Delegates=new string [] {"WeakDelegate"}, Events=new Type [] { typeof (CBCentralManagerDelegate)})]
	interface CBCentralManager {
		[Export ("delegate", ArgumentSemantic.Assign), NullAllowed]
		NSObject WeakDelegate { get; set; }

		[Wrap ("WeakDelegate")]
		CBCentralManagerDelegate Delegate { get; set; }
		
		[Export ("state")]
		CBCentralManagerState State { get; }

		[Export ("initWithDelegate:queue:")]
		CBCentralManager Constructor (CBCentralManagerDelegate delegateOrNull, [NullAllowed] DispatchQueue queueOrNull);

		[Export ("retrievePeripherals:"), Protected]
		void RetrievePeripherals (CBUUID[] peripheralUUIDs);

		[Export ("retrieveConnectedPeripherals")]
		void RetrieveConnectedPeripherals ();

		[Export ("scanForPeripheralsWithServices:options:")]
		void ScanForPeripheralsWithServices ([NullAllowed] CBUUID[] serviceUUIDs, [NullAllowed] NSDictionary options);

		[Export ("stopScan")]
		void StopScan ();

		[Export ("connectPeripheral:options:")]
		void ConnectPeripheral (CBPeripheral peripheral, NSDictionary options);

		[Export ("cancelPeripheralConnection:")]
		void CancelPeripheralConnection (CBPeripheral peripheral);
	}

	[BaseType (typeof (NSObject))]
	[Model]
	interface CBCentralManagerDelegate {
		[Abstract]
		[Export ("centralManagerDidUpdateState:"), EventArgs ("CBCentralManager")]
		void DidUpdateState (CBCentralManager central);

		[Export ("centralManager:didRetrievePeripherals:"), EventArgs ("CBCentralManagerPeripherals")]
		void DidRetrievePeripherals (CBCentralManager central, CBPeripheral[] peripherals);

		[Export ("centralManager:didRetrieveConnectedPeripherals:"), EventArgs ("CBCentralManagerPeripherals")]
		void DidRetrieveConnectedPeripherals (CBCentralManager central, CBPeripheral[] peripherals);

		[Export ("centralManager:didDiscoverPeripheral:advertisementData:RSSI:"), EventArgs ("CBCentralManagerPeripheralDiscovery")]
		void DidDiscoverPeripheral (CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData, NSNumber RSSI);

		[Export ("centralManager:didConnectPeripheral:"), EventArgs ("CBCentralManagerPeripheral")]
		void DidConnectPeripheral (CBCentralManager central, CBPeripheral peripheral);

		[Export ("centralManager:didFailToConnectPeripheral:error:"), EventArgs ("CBCentralManagerPeripheralWithError")]
		void DidFailToConnectPeripheral (CBCentralManager central, CBPeripheral peripheral, NSError error);

		[Export ("centralManager:didDisconnectPeripheral:error:"), EventArgs ("CBCentralManagerPeripheralWithError")]
		void DidDisconnectPeripheral (CBCentralManager central, CBPeripheral peripheral, NSError error);
	}

	[BaseType (typeof (NSObject))]
	interface CBCharacteristic {
		[Export ("service")]
		CBService Service { get; }

		[Export ("UUID")]
		CBUUID UUID { get; }

		[Export ("properties")]
		CBCharacteristicProperties Properties { get; }

		[Export ("value")]
		NSData Value { get; }

		[Export ("descriptors")]
		CBDescriptor[] Descriptors { get; }

		[Export ("isBroadcasted")]
		bool IsBroadcasted { get; }

		[Export ("isNotifying")]
		bool IsNotifying { get; }
	}

	[BaseType (typeof (NSObject))]
	interface CBDescriptor {
		[Export ("characteristic")]
		CBCharacteristic Characteristic { get; }

		[Export ("UUID")]
		CBUUID UUID { get; }

		[Export ("value")]
		NSObject Value { get; }
	}

	[BaseType (typeof (NSObject), Delegates=new string [] {"WeakDelegate"}, Events=new Type [] { typeof (CBPeripheralDelegate)})]
	interface CBPeripheral {
		[Export ("delegate", ArgumentSemantic.Assign), NullAllowed]
		NSObject WeakDelegate { get; set; }

		[Wrap ("WeakDelegate")]
		CBPeripheralDelegate Delegate { get; set; }

		[Export ("name")]
		string Name { get; }

		[Export ("RSSI")]
		NSNumber RSSI { get; }

		[Export ("isConnected")]
		bool IsConnected { get; }

		[Export ("services")]
		CBService[] Services { get; }

		[Export ("readRSSI")]
		void ReadRSSI ();

		[Export ("discoverServices:")]
		void DiscoverServices (CBUUID[] serviceUUIDs);

		[Export ("discoverIncludedServices:forService:")]
		void DiscoverIncludedServices (CBUUID[] includedServiceUUIDs, CBService service);

		[Export ("discoverCharacteristics:forService:")]
		void DiscoverCharacteristics (CBUUID[] characteristicUUIDs, CBService service);

		[Export ("readValueForCharacteristic:")]
		void ReadValue (CBCharacteristic characteristic);

		[Export ("writeValue:forCharacteristic:type:")]
		void WriteValue (NSData data, CBCharacteristic characteristic, CBCharacteristicWriteType type);

		[Export ("reliablyWriteValues:forCharacteristics:")]
		void ReliablyWriteValues (NSObject[] values, CBCharacteristic[] characteristics);

		[Export ("setBroadcastValue:forCharacteristic:")]
		void SetBroadcastValue (bool broadcastValue, CBCharacteristic characteristic);

		[Export ("setNotifyValue:forCharacteristic:")]
		void SetNotifyValue (bool notifyValue, CBCharacteristic characteristic);

		[Export ("discoverDescriptorsForCharacteristic:")]
		void DiscoverDescriptors (CBCharacteristic characteristic);

		[Export ("readValueForDescriptor:")]
		void ReadValue (CBDescriptor descriptor);

		[Export ("writeValue:forDescriptor:")]
		void WriteValue (NSData data, CBDescriptor descriptor);
	}

	[BaseType (typeof (NSObject))]
	[Model]
	interface CBPeripheralDelegate {
		[Abstract]
		[Export ("peripheralDidUpdateRSSI:error:"), EventArgs ("CBPeripheral")]
		void DidUpdateRSSI (CBPeripheral peripheral, NSError error);

		[Abstract]
		[Export ("peripheral:didDiscoverServices:"), EventArgs ("CBPeripheral")]
		void DidDiscoverServices (CBPeripheral peripheral, NSError error);

		[Abstract]
		[Export ("peripheral:didDiscoverIncludedServicesForService:error:"), EventArgs ("CBPeripheralService")]
		void DidDiscoverIncludedServicesForService (CBPeripheral peripheral, CBService service, NSError error);

		[Abstract]
		[Export ("peripheral:didDiscoverCharacteristicsForService:error:"), EventArgs ("CBPeripheralService")]
		void DidDiscoverCharacteristicsForService (CBPeripheral peripheral, CBService service, NSError error);

		[Abstract]
		[Export ("peripheral:didUpdateValueForCharacteristic:error:"), EventArgs ("CBPeripheralCahrictaristic")]
		void DidUpdateValueForCharacteristic (CBPeripheral peripheral, CBCharacteristic characteristic, NSError error);

		[Abstract]
		[Export ("peripheral:didWriteValueForCharacteristic:error:"), EventArgs ("CBPeripheralCahrictaristic")]
		void DidWriteValueForCharacteristic (CBPeripheral peripheral, CBCharacteristic characteristic, NSError error);

		[Abstract]
		[Export ("peripheral:didReliablyWriteValuesForCharacteristics:error:"), EventArgs ("CBPeripheralCharictaristics")]
		void DidReliablyWriteValuesForCharacteristics (CBPeripheral peripheral, CBCharacteristic[] characteristics, NSError error);

		[Abstract]
		[Export ("peripheral:didUpdateBroadcastStateForCharacteristic:error:"), EventArgs ("CBPeripheralCahrictaristic")]
		void DidUpdateBroadcastStateForCharacteristic (CBPeripheral peripheral, CBCharacteristic characteristic, NSError error);

		[Abstract]
		[Export ("peripheral:didUpdateNotificationStateForCharacteristic:error:"), EventArgs ("CBPeripheralCahrictaristic")]
		void DidUpdateNotificationStateForCharacteristic (CBPeripheral peripheral, CBCharacteristic characteristic, NSError error);

		[Abstract]
		[Export ("peripheral:didDiscoverDescriptorsForCharacteristic:error:"), EventArgs ("CBPeripheralCahrictaristic")]
		void DidDiscoverDescriptorsForCharacteristic (CBPeripheral peripheral, CBCharacteristic characteristic, NSError error);

		[Abstract]
		[Export ("peripheral:didUpdateValueForDescriptor:error:"), EventArgs ("CBPeripheralDescriptor")]
		void DidUpdateValueForDescriptor (CBPeripheral peripheral, CBDescriptor descriptor, NSError error);

		[Abstract]
		[Export ("peripheral:didWriteValueForDescriptor:error:"), EventArgs ("CBPeripheralDescriptor")]
		void DidWriteValueForDescriptor (CBPeripheral peripheral, CBDescriptor descriptor, NSError error);
	}
	
	[BaseType (typeof (NSObject))]
	interface CBService {
		[Export ("peripheral")]
		CBPeripheral Peripheral { get; }

		[Export ("UUID")]
		CBUUID UUID { get; }

		[Export ("includedServices")]
		CBService[] IncludedServices { get; }

		[Export ("characteristics")]
		CBCharacteristic[] Characteristics { get; }
	}
	
	[BaseType (typeof (NSObject))]
	interface CBUUID {
		[Export ("data")]
		NSData Data { get; }

		[Static]
		[Export ("UUIDWithString:")]
		CBUUID UUIDWithString (string theString);

		[Static]
		[Export ("UUIDWithData:")]
		CBUUID UUIDWithData (NSData theData);
	}
}

