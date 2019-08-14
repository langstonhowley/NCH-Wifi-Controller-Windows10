using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WpfApp1
{
    /// <summary>
    /// This class is responsible for handling the messages sent between the application and the NCH.
    /// </summary>
    class Stream_Manager
    {
        NetworkStream s; //The socket that allows for i/o.
        string nch_return; //The message received from the NCH.
        string previous_wifi_reading = ""; //The previous wifi reading received.
                                           //This is stored to prevent old data from beng used in the app.
        int mode = -1; //The mode the Stream manager is in. See read_message() for more detail.

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="btc">The Bluetooth Client responsible for making the initial connection.</param>
        /// <param name="mode">The mode of the stream manager.</param>
        public Stream_Manager(BluetoothClient btc, int mode)
        {
            s = btc.GetStream();
            this.mode = mode;
        }

        /// <summary>
        /// This reads the stream asynchronously until the NCH returns a new value.
        /// </summary>
        /// 
        ///     <list type="bullet">
        ///         <item>If the mode = 1, the nch return is considered.</item>
        ///         <item>If the mode = -1, the nch return is considered. 
        ///         Rather, the nch_return is used as a notifier to let the application know that the command was proccessed.</item>
        ///     </list>
        public void Read_message()
        { 

            Task.Run(async () =>
            {
                while (true)
                {
                    byte[] bytes = new byte[1024];
                    s.Read(bytes, 0, bytes.Length);

                    if (bytes != null && bytes.Length > 0)
                    {
                        nch_return = null;
                        switch (mode)
                        {
                            case 1:
                                if(nch_return == null && !Regex.IsMatch(BitConverter.ToString(bytes), @"[a-zA-Z]"))
                                {
                                    //The NCH's message is only considered if it is different from the previous wifi reading
                                    //and doesn't contain letters.
                                    nch_return = Encoding.ASCII.GetString(bytes).Substring(0, 1);
                                    System.Diagnostics.Debug.WriteLine("Received!!!!!!!!!!!: " + nch_return);
                                    System.Diagnostics.Debug.WriteLine("Previous: " + previous_wifi_reading);
                                    if (!nch_return.Equals(previous_wifi_reading))
                                    {
                                        System.Diagnostics.Debug.WriteLine("Got a new reading: " + nch_return + ". Previous was: " + previous_wifi_reading);
                                        previous_wifi_reading = nch_return;
                                    }
                                }
                                break;
                            case -1:
                                //Just checking when the message is recieved and parsed
                                nch_return = " ";
                                break;
                            default:
                                System.Diagnostics.Debug.WriteLine("Got to the default case. Setting to NULL");
                                nch_return = null;
                                break;
                        }

                        
                    }
                    else
                    {
                        nch_return = null;
                    }

                    if(nch_return != null)
                    {
                        break;
                    }
                }
            });
        }

        /// <summary>
        /// This sends a message through the stream to the NCH.
        /// </summary>
        /// <param name="buffer">The command to send the NCH</param>
        public void send_message(string buffer)
        {
            s.Write(Encoding.UTF8.GetBytes(buffer), 0, Encoding.UTF8.GetBytes(buffer).Length);
        }
        
        /// <summary>
        /// Returns the NCH output.
        /// </summary>
        /// <returns>The NCH output.</returns>
        public String get_return()
        {
            return nch_return;
        } 

        /// <summary>
        /// Clears the output once used.
        /// </summary>
        public void clearReturn()
        {
            nch_return = null;
        }

        /// <summary>
        /// Sets the mode of the Stream_Manager
        /// </summary>
        /// <param name="mode">The mode to set it to (1 or -1)</param>
        public void setMode(int mode)
        {
            this.mode = mode;
        }

    }
}
