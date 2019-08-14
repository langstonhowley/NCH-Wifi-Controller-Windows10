using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WpfApp1
{
    /// <summmary>
    /// Interaction logic for MainWindow.xaml. Basically the UI controller.
    /// </summary>
    /// 
    /// 
    public partial class MainWindow : Window
    { 
        Button selectedButton; //The button selected by the user. 
        Bluetooth_Manager bm; //The object that handles all bluetooth activity.
        int row, col; //When placing the the nch buttons these are incremented. 
        int og_row, og_col; //The original row and column of the button selected by the user.
        int connected_row, connected_col; //The place where the selected button goes when a connection is made. (Middle column, Last row).
        TextBlock wifi_status_textblock = new TextBlock(); //The textblock that holds the wifi status when received from the NCH.
        Button wifi_status_button = new Button(); //The button that toggles the wifi based on the current wifi status.
        DoubleAnimation connected_text_fading_animation; //The animation that allows for text blinking. This is separate because 
                                                         //connected_text updates on every blink.
        double rowHeight, colWidth;

        /// <summary>
        /// The Constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            bm = new Bluetooth_Manager();

             
            this.KeyDown += new KeyEventHandler(Main_KeyDown); //This listens for key commands issued by the user.
            this.ContentRendered += MainWindow_ContentRendered;

            this.Background = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#34373A"));

            //This sets the "connected" placement for the selected button to 
            //the middle column and bottom row of the grid.
            connected_col = this.grid.ColumnDefinitions.Count / 2;
            connected_row = this.grid.RowDefinitions.Count - 1;
            

            wifi_status_button.Click += Buttonclicked;
        }

        /// <summary>
        /// When the MainWindow is rendered fully, this starts device discovery.
        /// </summary>
        /// 
        /// <see cref="Bluetooth_Manager"/>
        /// 
        /// <param name="sender">(Unused)</param>
        /// <param name="e">(Unused)</param>
        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Connected_text = "Press 'h' For Help";
            rowHeight = this.grid.RowDefinitions[0].ActualHeight;
            colWidth = this.grid.ColumnDefinitions[0].ActualWidth;
            bm.Start_discovery();
        }

        /// <summary>
        /// When  a key is pressed, this is called.
        /// </summary>
        /// 
        /// <param name="sender">(Unsused)</param>
        /// <param name="e">The object that holds which key the user pressed.</param>
        void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.S && !bm.discoverer.Is_searching() && !bm.connected)
            {
                //"s" key. Begin a new device search.
                bm.Start_discovery();
            }
            else if(e.Key == Key.S)
            {
                if (bm.discoverer.Is_searching())
                {
                    MessageBox.Show("You cannot start a new search because the program is already searching.", "Cannot Begin Search", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (bm.connected)
                {
                    MessageBox.Show("You cannot start a new search because you have connected to an nch. Please disconnect before attempting to start a new search.", "Cannot Disconnect", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if(e.Key == Key.H)
            {
                //"h" key. Bring up the help menu.
                string s1 = "-To begin a search for NCHs press 's'.";
                string s2 = "-To pop up this menu press 'h'.";
                string s3 = "-To make a connection to an NCH, press the button with the NCH's name on it.";
                string s4 = "-Once connected, press the button with the title 'Turn Wifi: ' to toggle the NCH's wifi.";
                string s5 = "-To disconnect press 'd' or the button with the NCH's name on it on the bottom of the window.";
                string full = s1 + "\n\n" + s2 + "\n\n" + s3 + "\n\n" + s4 + "\n\n" + s5;
                MessageBox.Show(full, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (e.Key == Key.D && bm.connected)
            {
                //"d" key. Disconnect user from the NCH.
                bm.disconnet();
            }
            else if(e.Key == Key.D)
            {
                MessageBox.Show("Cannot disconnect because there are no current connections.", "Cannot Disconnect", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// This is the the text that appears in the bottom Dock that denotes what the program
        /// is doing during loading time. Whenever it's set it updates the UI.
        /// </summary>
        public string Loading_text
        {
            set
            {
                //print("Loading text is now set to: " + value);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.lt.Text = value;

                    DoubleAnimation fade = new DoubleAnimation
                    {
                        From = 0.0,
                        To = 1.0,
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        Duration = new Duration(new TimeSpan(0, 0, 2))
                    };
                    lt.BeginAnimation(TextBlock.OpacityProperty, fade);
                });
                
            }
        }
        /// <summary>
        /// This is the text that both reminds the user to press h for the help menu and
        /// notify the user of current connections. Whenever it's set it updates the UI.
        /// </summary>
        /// 
        ///<see cref="CT_Fade_Completed(object, EventArgs)"/>
        public string Connected_text
        {
            set
            {
                Application.Current.Dispatcher.Invoke(() => {
                    this.ct.Text = value;

                    connected_text_fading_animation = new DoubleAnimation
                    {
                        From = 0.0,
                        To = 1.0,
                        AutoReverse = true,
                        Duration = new Duration(new TimeSpan(0, 0, 2))
                    };

                    //When the connected text fades away it is changed in some cases
                    //so it must have a special event listener.
                    connected_text_fading_animation.Completed += CT_Fade_Completed;
                    ct.BeginAnimation(TextBlock.OpacityProperty, connected_text_fading_animation);
                });
                
            }
        }

        /// <summary>
        /// When the Connected_text animation completes (aka it fades away) this chages the displayed text if needed
        /// and then restarts the animation.
        /// </summary>
        /// 
        /// <param name="sender">(Unused)</param>
        /// <param name="e">(Unused)</param>
        private void CT_Fade_Completed(object sender, EventArgs e)
        {
            //Local variables to compare to the current text within Connected_text
            string help = "Press 'h' For Help", connected = "Connected to " + bm.selected_nch.DeviceName;

            if (ct.Text.Equals(""))
            {
                ct.Text = help;
            }
            else if(ct.Text.Equals(help) && bm.connected)
            {
                ct.Text = connected;
            }
            else
            {
                ct.Text = help;
            }

            ct.BeginAnimation(TextBlock.OpacityProperty, connected_text_fading_animation);
        }

        /// <summary>
        /// This method creates a new button for the <paramref name="nch"/> and places it on te screen.
        /// This method is called once the <paramref name="nch"/> is found from device discovery.
        /// </summary>
        /// <param name="nch">The found NCH</param>
        public void UpdateNCHButtons(Device nch)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate{
                if(this.grid.Children.Count > 0)
                {
                    col++;
                }
                if(row > this.grid.RowDefinitions.Count)
                {
                    col = 0;
                    row++;
                }
                //If enough NCHs are found that the buttons would go off the screen
                //don't place it.
                if(row > this.grid.RowDefinitions.Count)
                {
                    return;
                }

                Button b = new Button()
                {
                    Content = string.Format(nch.DeviceName),
                    Tag = string.Format(nch.DeviceInfo.DeviceAddress.ToString())
                };

                if (this.grid.Children.Count % 2 == 0)
                {
                    //The Green Color
                    b.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00B23D"));
                }
                else
                {
                    //The Blue Color
                    b.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#29547D"));
                }

                b.Foreground = Brushes.White;
                b.FontSize = 17;
                b.FontFamily = new FontFamily("Helvetica Neue");
                b.Style = Application.Current.FindResource("CustomButton") as Style;
                b.Click += new RoutedEventHandler(Buttonclicked);

                
                this.grid.Children.Add(b);
                Grid.SetRow(b, row);
                Grid.SetColumn(b, col);
            });
        }

        /// <summary>
        /// Whenever a loading process is taking place this is called to show the indeterminate progress bar.
        /// </summary>
        public void LoadingLayout()
        {
            //print("Called Loading Layout");
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                this.lt.Visibility = Visibility.Visible;
                this.pb.Visibility = Visibility.Visible;
            });
        }

        /// <summary>
        /// This hides the indeterminate progress bar
        /// </summary>
        public void NoLoadingLayout()
        {
            //print("Called No Loading Layout");
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                this.pb.Visibility = Visibility.Hidden;
            });
        }

        /// <summary>
        /// When the user selects an NCH to connect to (aka selects a button) this hides all of the other buttons.
        /// </summary>
        public void HideAllButSelected()
        {

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
               foreach(UIElement b in this.grid.Children)
                {
                    if (!(b is Button))
                    {
                        continue;
                    }

                    if (b.Equals(selectedButton) || b.Equals(wifi_status_button))
                    {
                        continue;
                    }

                    DoubleAnimation d = new DoubleAnimation(1.0, 0.0, TimeSpan.FromSeconds(1));
                    b.BeginAnimation(Button.OpacityProperty, d);
                    d.Completed += (s, e) =>
                    {
                        b.IsEnabled = false;
                    };   
                }
            });
        }

        /// <summary>
        /// This removes the elements shown when a user initailly conects to an NCH.
        /// </summary>
        public void removeConnectedStreenElements()
        {
            wifi_status_textblock.Visibility = Visibility.Hidden;
            wifi_status_button.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// This makes all of the buttons visible after hiding them.
        /// </summary>
        public void MakeAllVisible()
        {
            Application.Current.Dispatcher.Invoke(() => {
                foreach(UIElement b in this.grid.Children)
                {
                    if(!(b is Button))
                    {
                        continue;
                    }

                    if (b.Equals(selectedButton) || b.Equals(wifi_status_button))
                    {
                        continue;
                    }

                    DoubleAnimation d = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(1));
                    b.BeginAnimation(Button.OpacityProperty, d);
                    d.Completed += (s, e) =>
                    {
                        b.IsEnabled = true;
                    };
                }
            });
        }

        /// <summary>
        /// Clears the screen of all buttons.
        /// </summary>
        public void ClearScreen()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                grid.Children.Clear();
                row = 0;
                col = 0;
            });
        }

        /// <summary>
        /// This moves the selected button to the "connected" position to indicate connection.
        /// </summary>
        public void moveSelectedButton()
        {
            Application.Current.Dispatcher.Invoke(() => {

                selectedButton.Content = selectedButton.Content + "\n(Press To Disconnect)";
                selectedButton.HorizontalContentAlignment = HorizontalAlignment.Center;
                selectedButton.VerticalContentAlignment = VerticalAlignment.Center;
                selectedButton.FontSize = 14;
                selectedButton.Arrange(new Rect(og_col * colWidth, og_row * rowHeight, colWidth, rowHeight));

                //print("Clicked: " + og_row + "," + og_col);
                //print("Moving to: " + connected_row + "," + connected_col);

                //print("From Point: " + og_col * 199 + "," + og_row * 78);
                //print("To Point: " + connected_col * 199 + "," + connected_row * 78);

                var xdiff = (connected_col * colWidth) - (og_col * colWidth);
                var ydiff = (connected_row * rowHeight) - (og_row * rowHeight);

                TranslateTransform trans = new TranslateTransform();
                selectedButton.RenderTransform = trans;
                DoubleAnimation anim1 = new DoubleAnimation(xdiff, TimeSpan.FromSeconds(1.5));
                DoubleAnimation anim2 = new DoubleAnimation(ydiff, TimeSpan.FromSeconds(1.5));
                trans.BeginAnimation(TranslateTransform.XProperty, anim1);
                trans.BeginAnimation(TranslateTransform.YProperty, anim2);

            });
        }

        /// <summary>
        /// This moves the selected button back to its original position in the grid.
        /// </summary>
        public void moveSelectedButtonBack()
        {
            selectedButton.Content = selectedButton.Content.ToString().Substring(0, selectedButton.Content.ToString().Length - 22);
            selectedButton.FontSize = 17;
            selectedButton.Arrange(new Rect(connected_col * colWidth, connected_row * rowHeight, colWidth, rowHeight));

            var xdiff = -((connected_col * colWidth) - (og_col * colWidth));
            var ydiff = -((connected_row * rowHeight) - (og_row * rowHeight));

            TranslateTransform trans = new TranslateTransform();
            selectedButton.RenderTransform = trans;
            DoubleAnimation anim1 = new DoubleAnimation(xdiff, TimeSpan.FromSeconds(1.5));
            DoubleAnimation anim2 = new DoubleAnimation(ydiff, TimeSpan.FromSeconds(1.5));
            trans.BeginAnimation(TranslateTransform.XProperty, anim1);
            trans.BeginAnimation(TranslateTransform.YProperty, anim2);

            selectedButton = null;
        }

        /// <summary>
        /// When any of the interactable buttons on the MAin Window is clicked, this is called.
        /// </summary>
        /// 
        /// <param name="sender">The Button that was clicked.</param>
        /// <param name="e">(Unused)</param>
        void Buttonclicked(object sender, RoutedEventArgs e)
        {
            if((sender as Button).Tag.Equals("ON"))
            {
                (sender as Button).IsEnabled = false;
                selectedButton.IsEnabled = false;
                Task.Run(async () => {
                    //Send "Wifi_Enable" to the NCH.
                    bm.toggle_wifi("1");
                });
                return;
            }
            else if ((sender as Button).Tag.Equals("OFF"))
            {
                (sender as Button).IsEnabled = false;
                selectedButton.IsEnabled = false;
                Task.Run(async () =>{
                    //Send "Wifi_Disable" to the NCH.
                    bm.toggle_wifi("0");
                });
                return;
            }

            //THE DISCONNECT CONDITION
            if (bm.connected && (sender as Button).Equals(selectedButton))
            {
                if (MessageBox.Show("Disconnect from " + bm.selected_nch.DeviceName + "?", "Disconnect", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bm.disconnet();
                }

                return;
            }

            //THE CONNECT CONDITION
            if (!bm.connected)
            {
                if (MessageBox.Show("Pair and Connect to " + (sender as Button).Content + "?", "Connect", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    selectedButton = (sender as Button);
                    bm.Pair((sender as Button).Content.ToString());
                    //og_index = this.grid.Children.IndexOf(selectedButton);
                    og_row = Grid.GetRow(selectedButton);
                    og_col = Grid.GetColumn(selectedButton);
                }

                
            }  
        }

        /// <summary>
        /// A helper function to print to the debug log.
        /// </summary>
        /// <param name="print">The text to pinted.</param>
        private void print(string print)
        {
            System.Diagnostics.Debug.WriteLine(print);
        }

        /// <summary>
        /// This sets up the "second screen" which displays the wifi status textblock and the wifi status button
        /// whose texts are determined by the wifi status received from the NCH.
        /// </summary>
        /// <param name="wifiStatus">The current wifi status.</param>
        public void setUpConnectScreen(String wifiStatus)
        {
            Application.Current.Dispatcher.Invoke(() => {
                ColorAnimation ca;

                if (wifiStatus.Equals("0"))
                {
                    wifi_status_textblock.Text = "Wifi Status: OFF";
                    wifi_status_button.Content = "Turn Wifi ON";
                    wifi_status_button.Tag = "ON";
                    ca = new ColorAnimation((Color)ColorConverter.ConvertFromString("#00B23D"), new TimeSpan(0,0,1));
                    
                }
                else
                {
                    wifi_status_textblock.Text = "Wifi Status: ON";
                    wifi_status_button.Content = "Turn Wifi OFF";
                    wifi_status_button.Tag = "OFF";
                    ca = new ColorAnimation((Color)ColorConverter.ConvertFromString("#C22121"), new TimeSpan(0,0,1));
                }

                this.wifi_status_button.Background = new SolidColorBrush(Colors.Transparent);
                this.wifi_status_button.Background.BeginAnimation(SolidColorBrush.ColorProperty,ca);
                wifi_status_button.Style = Application.Current.FindResource("CustomButton") as Style;

                wifi_status_textblock.Foreground = Brushes.White;
                wifi_status_textblock.VerticalAlignment = VerticalAlignment.Center;
                wifi_status_textblock.HorizontalAlignment = HorizontalAlignment.Center;
                wifi_status_textblock.FontSize = 20;
                wifi_status_textblock.FontFamily = new FontFamily("Arial");

                wifi_status_button.Foreground = Brushes.White;
                wifi_status_button.FontSize = 17;
                wifi_status_button.FontFamily = new FontFamily("Helvetica Neue");

                if (!this.grid.Children.Contains(wifi_status_textblock))
                {
                    this.grid.Children.Add(wifi_status_textblock);
                    Grid.SetRow(wifi_status_textblock, 3);
                    Grid.SetColumn(wifi_status_textblock, connected_col);
                }
                if (!this.grid.Children.Contains(wifi_status_button))
                {
                    this.grid.Children.Add(wifi_status_button);
                    Grid.SetRow(wifi_status_button, 4);
                    Grid.SetColumn(wifi_status_button, connected_col);
                }
                
                wifi_status_textblock.Visibility = Visibility.Visible;
                wifi_status_button.Visibility = Visibility.Visible;
                wifi_status_button.IsEnabled = true;
                selectedButton.IsEnabled = true;
                print("Button Tag: " + wifi_status_button.Tag);
                NoLoadingLayout();
                Loading_text = "";
                
            });

            

        }

    }
}

