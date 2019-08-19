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

using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace NCHWifiController
{

    class Bluetooth_Manager : IDiscovererObserver { 
        // UUID: 00001101-0000-1000-8000-00805F9B34FB
        BluetoothClient btc;
        BluetoothComponent btcomp;
        public Discoverer discoverer;
        Stream_Manager streamManager;
        MainWindow window = Application.Current.Windows[0] as MainWindow;
        public Device selected_nch = new Device(null);
        Pairer p;

        int Complete_code { get; set; }

        private bool added = false;
        public bool pairingOrConnected = false;
        public bool connected = false;

        BluetoothClient cli = new BluetoothClient();

        private readonly Guid uuid = new Guid("00001101-0000-1000-8000-00805F9B34FB");

        //public bool paired = false;

        public Bluetooth_Manager()
        {
            init();
        }

        public void init()
        {
            BluetoothRadio.PrimaryRadio.Mode = RadioMode.Discoverable;

            btc = new BluetoothClient(
                new InTheHand.Net.BluetoothEndPoint(BluetoothRadio.PrimaryRadio.LocalAddress, BluetoothService.SerialPort));
            btcomp = new BluetoothComponent(btc);

            discoverer = new Discoverer(btcomp);
        }


        public void Start_discovery()
        {
            init();
            discoverer.addObserver(this);
            discoverer.start();
        }

        
        public void Pair(string nch_name)
        {
            pairingOrConnected = true;
            window.Loading_text = "Pairing With " + nch_name;
            window.LoadingLayout();

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args) {
                pairAsync(nch_name);
            });
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.RunWorkerAsync();

        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                window.Loading_text = "";
                window.NoLoadingLayout();
            });   
        }
        
        private void pairAsync(string nch_name)
        {
            foreach (Device nch in discoverer.Nchs())
            {
                if (nch == null)
                {
                    continue;
                }

                if (nch_name.Equals(nch.DeviceName))
                {
                    selected_nch = nch;
                    break;
                }
            }

            p = new Pairer(btc, selected_nch, uuid, this);
            p.isPaired();
        }

        private void onConnectFailed()
        {
            if(MessageBox.Show("Attempt another connection?", "Conection Failed", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes){
                if (!connect(selected_nch))
                {
                    onConnectFailed();
                }
            }
        }

        public void onPairFailed()
        {
            if (connected || !p.pairingRequested)
            {
                return; 
            }

            if (MessageBox.Show("Attempt another pair?", "Pairing Failed", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                Pair(selected_nch.DeviceName);
            }
        }

        public void onPairSucess()
        {
            if (!connect(selected_nch))
            {
                onConnectFailed();
            }
        }

        public Boolean connect(Device nch)
        {
            window.Loading_text = "Connecting to " + nch.DeviceName;
            window.LoadingLayout();

            Task.Run(async () => {
                try
                {
                    cli = new BluetoothClient();
                    nch.DeviceInfo.SetServiceState(BluetoothService.SerialPort, true);

                    
                    cli.Connect(new InTheHand.Net.BluetoothEndPoint(nch.DeviceInfo.DeviceAddress, BluetoothService.SerialPort));
                    System.Diagnostics.Debug.WriteLine("CONNECTED!");
                    connected = true;

                    streamManager = new Stream_Manager(cli, 1);

                    window.HideAllButSelected();
                    window.moveSelectedButton();

                    window.Connected_text = "Connected to " + nch.DeviceName;
                    window.NoLoadingLayout();
                    window.Loading_text = "";
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR ON CONNECT: " + e.Message);
                    connected = false;
                }
            }).GetAwaiter().GetResult();

            if (!connected)
            {
                print("Not connected G");
                return connected;
            }

            var wifi = get_wifi();
            System.Diagnostics.Debug.WriteLine("----The current wifi status is " + wifi + "----");

            window.Loading_text = "";
            window.NoLoadingLayout();

            window.setUpConnectScreen(wifi);

            return true;
        }

        public void disconnet()
        {
            connected = false;
            pairingOrConnected = false;
            cli.Close();
            window.Connected_text = "Press 'h' For Help";
            window.Loading_text = "Press 's' To Search Again";
            window.moveSelectedButtonBack();
            window.removeConnectedStreenElements();
            window.MakeAllVisible();
            window.NoLoadingLayout();
        }


        public String get_wifi()
        {
            window.Loading_text = "Fetching Data From " + selected_nch.DeviceName;
            window.LoadingLayout();
            streamManager.setMode(1);
            streamManager.clearReturn();
            streamManager.send_message("Get_Wifi");
            streamManager.Read_message();

            Task.Run(async () =>
            {
                while (streamManager.get_return() == null)
                {
                    //Do Nothing
                }
            }).GetAwaiter().GetResult();

            print("Called GetWifi. Return: " + streamManager.get_return());
            return streamManager.get_return(); 
        }

        public int toggle_wifi(String wifi)
        {
            window.Loading_text = "Sending Command To " + selected_nch.DeviceName;
            window.LoadingLayout();
            streamManager.setMode(-1);
            streamManager.clearReturn();

            if (wifi.Equals("0"))
            {
                streamManager.send_message("Wifi_Disable");
            }
            else
            {
                streamManager.send_message("Wifi_Enable");
            }
            streamManager.Read_message();
            Task.Run(async () =>
            {
                while (streamManager.get_return() == null)
                {
                    //Do Nothing
                }
            }).GetAwaiter().GetResult();

            var a = get_wifi();
            window.setUpConnectScreen(a);
            return 1;
        }

        public void OnDeviceFound(Device nch)
        {
            //print("Sending NCH to window: " + nch.DeviceName);
            window.UpdateNCHButtons(nch);
        }

        public void onDiscoveryStart()
        {
            //print("On Discovery Start for the bluetooth manager");
            window.ClearScreen();
            window.Loading_text = "Searching For Nearby NCHs...";
            window.LoadingLayout();
        }

        public void onDiscoveryFinished()
        {
            if (pairingOrConnected) { return; }
            window.Loading_text = "Press 's' To Search Again";
            window.NoLoadingLayout();
        }

        private void print(string print)
        {
            System.Diagnostics.Debug.WriteLine(print);
        }
    }
}
