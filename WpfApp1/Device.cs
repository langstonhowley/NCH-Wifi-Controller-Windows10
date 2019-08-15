//   Copyright 2019 Nextek Power Systems
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InTheHand.Net.Sockets;

namespace WpfApp1
{
    public class Device 
    {
        private string d_name;
        private bool is_a, is_c, rem;
        private ushort nap;
        private uint sap;
        private DateTime lu, ls;
        private BluetoothDeviceInfo b_info;

        public string DeviceName {get{return this.d_name;} set{ this.d_name = value;}}

        public bool IsAuthenticated {get{return this.is_a;} set{this.is_a = value;}}

        public bool IsConnected {get{return this.is_c;} set {this.is_c = value;}}

        public ushort Nap {get{return this.nap;} set{this.nap = value;}}

        public uint Sap {get{return this.sap;} set{this.sap = value;}}

        public DateTime LastUsed { get { return this.lu; } set { this.lu = value; } }

        public DateTime LastSeen { get { return this.ls; } set { this.ls = value; } }

        public bool Remembered { get { return this.rem; } set { this.rem = value; } }

        public BluetoothDeviceInfo DeviceInfo { get { return this.b_info; } set { this.b_info = value; } }

        public Device(BluetoothDeviceInfo device_info)
        {
            if(device_info != null)
            {
                DeviceInfo = device_info;
                IsAuthenticated = device_info.Authenticated;
                IsConnected = device_info.Connected;
                DeviceName = device_info.DeviceName;
                LastSeen = device_info.LastSeen;
                LastUsed = device_info.LastUsed;
                Nap = device_info.DeviceAddress.Nap;
                Sap = device_info.DeviceAddress.Sap;
                Remembered = device_info.Remembered;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("BLUETOOTH DEVICE INFO IS NULL");
            }
        }

        public override string ToString()
        {
            return DeviceName;
        }

    }
}
