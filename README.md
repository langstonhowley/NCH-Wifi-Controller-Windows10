# NCH Wifi Controller (For Windows 10)

This Windows 10 application allows users to connect to a Nextek NCH and toggle its wifi.

More specifically, a virtual serial port is made between the device and a user-selected NCH using the Bluetooth RFCOMM Protocol (more information on RFCOMM [here](https://en.wikipedia.org/wiki/List_of_Bluetooth_protocols#RFCOMM)). This allows for the user to toggle wifi by sending byte data through the serial port.

To see the macOS X version of this application [click here](https://github.com/langstonhowley/NCH-Wifi-Controller-MacOs) .

To see the Android version of this application [click here](https://github.com/langstonhowley/NCH-Wifi-Controller-Android) .

## Bluetooth Handling

The entire project depends on API provided by [32feet.NET](https://github.com/inthehand/32feet). Every Bluetooth event is controlled by the [Bluetooth_Manager](https://github.com/langstonhowley/NCH-Wifi-Controller-Windows10/blob/master/WpfApp1/Bluetooth_Manager.cs). Important bluetooth events include:

### Device Searching:

The [Discoverer Class](https://github.com/langstonhowley/NCH-Wifi-Controller-Windows10/blob/master/WpfApp1/Discoverer.cs) once ```start()``` is called keeps track of the devices it finds (including NCHs) and when the discovery times out.

Example Code:
```C#
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;


//Initialize a Bluetooth Client
BluetoothClient btc = 
  new BluetoothClient(BluetoothEndPoint(BluetoothRadio.PrimaryRadio.LocalAddress, 
                                        BluetoothService.SerialPort))
                                        
//Make a Bluetooth Component from that Bluetooth Client
BluetoothComponent btcomp = new BluetoothComponent(btc)

//Discover Devices Asynchronously
btcomp.DiscoverDevicesAsync(255,false,false,true,true,null)

//Event handler for finding a device.
btcomp.DiscoverDevicesProgress += new EventHandler<DiscoverDevicesEventArgs>(Progress)

//Event handler for the discovery ending.
btcomp.DiscoverDevicesProgress += new EventHandler<DiscoverDevicesEventArgs>(End)

private void Progress(object sender, DiscoverDevicesArgs arg){/* Handle device discovered */}
private void Progress(object sender, DiscoverDevicesArgs arg){/* Handle device discovery ending */}
```

### Device Pairing:

The [Pairer Class](https://github.com/langstonhowley/NCH-Wifi-Controller-Windows10/blob/master/WpfApp1/Pairer.cs) makes a pairing request to the selected device (aka NCH) if not already paired.

Example Code:
```C#
//Making a pairing request
BluetoothDeviceInfo selectedDevice = /*Let's say that this is initialized to a user-selected device.*/;

bool paired = BluetoothSecurity.PairRequest(selectedDevice.DeviceAddress, null);

if(paired){/* Device is paired */}
```

### Device Connection:

The [Connect Method](https://github.com/langstonhowley/NCH-Wifi-Controller-Windows10/blob/4392a337a510499df2a7995ef7b9e405cb9ca5b6/WpfApp1/Bluetooth_Manager.cs#L132) in the [Bluetooth_Manager class](https://github.com/langstonhowley/NCH-Wifi-Controller-Windows10/blob/master/WpfApp1/Bluetooth_Manager.cs) makes the connection to the selected device.

> "By far the easiest and best way to use a serial port connection is to use BluetoothClient with service class BluetoothService.SerialPort, i.e. using code very much like shown in General Bluetooth Data Connections. That gets one a .NET Stream to read and write from. Both BluetoothClient and virtual serial ports use the RFCOMM protocol so the two are equivalent." - 32Feet.NET Wiki

(For more info on the qoute, [click here](https://github.com/inthehand/32feet/wiki/Bluetooth-Serial-Ports))

Example Code:
```C#
try{
  //Make a new Bluetooth Client
  BluetoothClient btc2 = new BluetoothClient();

  //Turn the Serial Port (RFCOMM) service on (just in case it's off)
  selectedDevice.SetServiceState(BluetoothService.SerialPort, true)

  //Warning: Blocking call.
  btc2.Connect(new BluetoothEndpoint(selectedDevice.DeviceAddress, BluetoothService.SerialPort))
}
catch(Exception e){
  //Device failed to connect.
}

```

### Message I/O:

The [Stream_Manager Class](https://github.com/langstonhowley/NCH-Wifi-Controller-Windows10/blob/master/WpfApp1/Stream_Manager.cs) is responsible for communicating with the remote device (NCH)

Example Code:
```C#

//Create an instance of a NetworkStream from the connected BluetoothClient
NetworkStream nws = btc2.GetStream();

//Reading data into a byte buffer
byte[] bytes = new byte[256]
nws.read(bytes, 0, bytes.Length) //read in 256 bytes.

//Writing
var writeMe = new byte[]{0x20, 0x20, ...}
nws.Write(writeMe, 0, writeMe.Length)
```
