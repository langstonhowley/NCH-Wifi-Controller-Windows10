using InTheHand.Net.Bluetooth;
using System;
using System.Collections;
using System.Linq;

namespace WpfApp1
{
    /// <summary>
    /// The Discoverer class is responsible for finding all of the NCHs around and notifying the Bluetooth
    /// Manager about it.
    /// </summary>
    class Discoverer
    {
        BluetoothComponent btcomp; //The Bluetooth Component responsible for finding devices.
        Device[] nchs = new Device[100]; //A list of NCHs found arbitrarily capped at 100
        bool isSearching = false; //True if searching, false if not.
        ArrayList observers = new ArrayList(); //Hold a list of objects that will e notified when an NCH is found.
        int index = 0; //The index of the nchs array to add a newly found NCH.

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="component">The Bluetooth Component.</param>
        public Discoverer(BluetoothComponent component)
        {
            this.btcomp = component;
        }

        /// <summary>
        /// Begins searching for NCHs.
        /// </summary>
        public void start()
        {
            index = 0;
            Array.Clear(nchs, 0, nchs.Count());
            isSearching = true;
            btcomp.DiscoverDevicesAsync(255, false, false, true, true, null);
            btcomp.DiscoverDevicesProgress += new EventHandler<DiscoverDevicesEventArgs>(Progress);
            btcomp.DiscoverDevicesComplete += new EventHandler<DiscoverDevicesEventArgs>(End);

            for (int i = 0; i < observers.Count; i++)
            {
                ((IDiscovererObserver)observers[i]).onDiscoveryStart();
            }
        }

        /// <summary>
        /// Progress is an event handler that is triggered when a device is found.
        /// </summary>
        /// <param name="sender">(Unused)</param>
        /// <param name="args">Holds the device found.</param>
        private void Progress(object sender, DiscoverDevicesEventArgs args)
        {

            for(int i = 0; i < args.Devices.Length; i++)
            {
                Device nch = new Device(args.Devices[i]);
                if(!nchs.Contains<Device>(nch) && nch.DeviceName.ToLower().Contains("nch"))
                {
                    print("Adding " + nch.DeviceName);
                    nchs[index] = nch;
                    index++;

                    foreach(IDiscovererObserver observer in observers)
                    {
                        observer.OnDeviceFound(nch);
                    }
                }
            }
        }

        /// <summary>
        /// End is an event handler that is triggered when the search stops.
        /// </summary>
        /// <param name="sender">(Unused)</param>
        /// <param name="args">(Unused)</param>
        private void End(object sender, DiscoverDevicesEventArgs args)
        {
            for (int i = 0; i < observers.Count; i++)
            {
                ((IDiscovererObserver)observers[i]).onDiscoveryFinished();
            }

            isSearching = false;
        }

        /// <summary>
        /// Returns whether or not the Discoverer is searching.
        /// </summary>
        /// <returns>isSearching.</returns>
        public bool Is_searching()
        {
            return isSearching;
        }

        /// <summary>
        /// Returns the list of NCHs found.
        /// </summary>
        /// <returns>NCH list.</returns>
        public Device[] Nchs()
        {
            return nchs;
        }

        /// <summary>
        /// Adds the object passed in to the observer list.
        /// </summary>
        /// <param name="observer">The Object to add.</param>
        public void addObserver(IDiscovererObserver observer)
        {
            //print("Added Observer!!!");
            this.observers.Add(observer);
        }
        
        public void removeObserver(IDiscovererObserver observer)
        {
            observers.Remove(observer);
        }

        private void print(string print)
        {
            System.Diagnostics.Debug.WriteLine(print);
        }
    }

    public interface IDiscovererObserver
    {
        void OnDeviceFound(Device nch);
        void onDiscoveryStart();
        void onDiscoveryFinished();
    }
}

