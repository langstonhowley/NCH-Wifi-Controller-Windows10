using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// The Pairer class is responsible for handling the pairing between the user's device and the NCH.
    /// </summary>
    class Pairer
    {
        BluetoothDeviceInfo[] pairedDevices; //A list of the devices paired with the user's device.
        Device nch; //The NCH to pair to.
        Guid uuid; //The UUID recognized by the NCH.
        public bool pairingRequested = false; //A variable that can be accessed globally which holds whether a pairing request
                                              //has been made.
        Bluetooth_Manager bm; //The instance of the Bluetooth Manager.

        /// <summary>
        /// Constructor.
        /// </summary>
        /// 
        /// <param name="client">The user's device so a list of paired devices can be extracted.</param>
        /// <param name="nch">The NCH attempted to be paired to.</param>
        /// <param name="uuid">The UUID recognized by an NCH</param>
        /// <param name="bm">The Bluetooth Manager</param>
        public Pairer(BluetoothClient client, Device nch, Guid uuid, Bluetooth_Manager bm)
        {
            pairedDevices = client.DiscoverDevices(255, false, true, false, false);
            this.nch = nch;
            this.uuid = uuid;
            this.bm = bm;
        }

        /// <summary>
        /// This checks if the NCH is in the device's paired devices list and if not
        /// makes a pairing request to the NCH. When pairing occurs the Bluetooth_Maager's onPairSuccess()
        /// is called, if it fails onPairFailed() is called.
        /// </summary>
        public void isPaired()
        {
            foreach (BluetoothDeviceInfo dev in pairedDevices)
            {
                if (dev.Equals(nch.DeviceInfo))
                {
                    System.Diagnostics.Debug.WriteLine("Pairer: Paired");
                    bm.onPairSucess();
                    return;
                }
            }

            bool b = false;
            System.Diagnostics.Debug.WriteLine("It's not in paired devices so starting a pair request");
            pairingRequested = true;
            Task.Run(async () => {
                if (BluetoothSecurity.PairRequest(nch.DeviceInfo.DeviceAddress, null))
                {
                    System.Diagnostics.Debug.WriteLine("Pairer: Paired");
                    bm.onPairSucess();
                    b = true;
                    return;

                }
            }).GetAwaiter().GetResult();

            pairingRequested = false;

            if (!b)
            {
                bm.onPairFailed();
            }
            
        }


    }
}
