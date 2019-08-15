# NCH Wifi Controller (For Windows 10)

This Windows 10 application allows users to connect to a Nextek NCH and toggle its wifi.

More specifically, a virtual serial port is made between the device and a user-selected NCH using the Bluetooth RFCOMM Protocol (more information on RFCOMM [here](https://en.wikipedia.org/wiki/List_of_Bluetooth_protocols#RFCOMM)). This allows for the user to toggle wifi by sending byte data through the serial port.

To see the macOS X version of this application [click here](https://github.com/langstonhowley/NCH-Wifi-Controller-MacOs) .

To see the Android version of this application [click here](https://github.com/langstonhowley/NCH-Wifi-Controller-Android) .

## Bluetooth Handling

The entire project depends on API provided by [32feet.NET](https://github.com/inthehand/32feet). Every Bluetooth event is controlled by the [Bluetooth_Manager](https://github.com/langstonhowley/NCH-Wifi-Controller-Windows10/blob/master/WpfApp1/Bluetooth_Manager.cs). Important bluetooth events include:

### Device Search:

The [Discoverer Class](https://github.com/langstonhowley/NCH-Wifi-Controller-Windows10/blob/master/WpfApp1/Discoverer.cs) once ```start()``` is called keeps track of the devices it finds (including NCHs) and when the discovery times out.

Example Code:
