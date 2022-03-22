using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LightBoxController;

namespace LightBoxGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LightBoxClass controller;

        private readonly DispatcherTimer dTimer = new();

        private readonly Ping pingSender = new();

        private class ComboBoxEffects
        {
            public string Key { get; set; }
            public int Value { get; set; }

            public ComboBoxEffects(string _key, int _value)
            {
                Key = _key;
                Value = _value;
            }
        }

        //global variable for keeping the colour, necessary for the Dim slider 
        private System.Windows.Media.Color? colour;
        private string colourLast;

        public MainWindow()
        {
            InitializeComponent();
            InitializeComboBox();
            dTimer.Interval = TimeSpan.FromMilliseconds(100);
            dTimer.Tick += DTimer_Tick;
        }
        private void InitializeComboBox()
        {
            List<ComboBoxEffects> comboEffList = new()
            {
                new ComboBoxEffects("No effect", 0),
                new ComboBoxEffects("Chill", 1),
                new ComboBoxEffects("Disco", 2),
                new ComboBoxEffects("Techno", 3),
                new ComboBoxEffects("Effect Axe", 4)
            };

            comboboxEffect.DisplayMemberPath = "_Key";
            comboboxEffect.SelectedValuePath = "_Value";
            comboboxEffect.ItemsSource = comboEffList;
            comboboxEffect.SelectedIndex = 0;
        }
        private void DTimer_Tick(object sender, EventArgs e)
        {
            GetColorState(sender, e);
        }
        /// <summary>
        /// A method to determine if an IP address is internal, as specified in RFC1918
        /// </summary>
        /// <param name="toTest">The IP address that will be tested</param>
        /// <returns>Returns true if the IP is internal, false if it is external</returns>
        private static bool IsInternal(IPAddress toTest)
        {
            if (IPAddress.IsLoopback(toTest)) return false;
            else if (toTest.ToString() == "::1") return false;

            byte[] bytes = toTest.GetAddressBytes();
            return bytes[0] switch
            {
                10 => true,
                172 => bytes[1] is < 32 and >= 16,
                192 => bytes[1] == 168,
                _ => false,
            };
        }
        private void ButtonSetIp_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = tbIpAddress.Text;
            //ValidateIP
            if (IPAddress.TryParse(ipAddress, out IPAddress ip))
            {
                //reject IPv6
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (IsInternal(ip))
                    {
                        PingReply reply = pingSender.Send(ip, 2000);
                        if (reply.Status == IPStatus.Success)
                        {
                            buttonSetIp.Background = Brushes.Green;
                            buttonGetInfo.IsEnabled = true;
                            tgbToggle.IsEnabled = true;

                            //controller.dispose();
                            //TODO singleton
                            HttpClient httpClient = new()
                            {
                                BaseAddress = new Uri(string.Concat("http://", ipAddress)),
                                Timeout = new TimeSpan(0, 0, 1)
                            };
                            controller = new LightBoxClass(httpClient);
                        }
                        else
                        {
                            buttonSetIp.Background = Brushes.Gray;
                            tgbToggle.IsEnabled = false;
                            buttonGetInfo.IsEnabled = false;
                            MessageBox.Show("Selected IP device not available");
                        }
                    }
                    else
                        MessageBox.Show("IP address not internal");
                }
                else
                    MessageBox.Show("Enter valid IPv4 address");
            }
            else
                MessageBox.Show("This is not a valid ip address. Re-enter");
        }

        private async void ButtonGetInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var myRootDevice = await controller.GetInfoAsync();
                labelDeviceNameInfo.Content = myRootDevice.device.deviceName;
                labelProductInfo.Content = myRootDevice.device.product;
                labelApiLevelInfo.Content = myRootDevice.device.apiLevel;
            }
            catch (Exception)
            {
                MessageBox.Show($"Selected host ({tbIpAddress.Text}) not responding.");
            }
        }
        private async Task GetColorState(object sender, EventArgs e)
        {
            try
            {
                var myDevice = await controller.GetStateAsync();
                if (myDevice?.rgbw != null)
                {
                    rectangleColor.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom($"#{myDevice.rgbw.currentColor.Remove(6, 2)}");
                }
            }
            catch (Exception ex)
            {
                dTimer.Stop(); //so that the window won't pop up continously
                MessageBox.Show("async Task getColorState\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
            }
        }
        private async void SetState()
        {
            try
            {
                await controller.SetColorAsync(colour.ToString(), (int)sliderDim.Value);
                labelCurrentEffect.Content = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Device Disco nnected\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
                rectangleColor.Fill = Brushes.Gray;
            }
        }
        private void SliderDim_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetState();
            if (buttonRead.IsChecked == false)
                buttonRead.IsChecked = true;
        }
        private async void ComboboxEffect_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                rectangleColor.Fill = Brushes.Gray;
                ComboBoxEffects selectedEffect = (ComboBoxEffects)comboboxEffect.SelectedItem;
                await controller.SetEffect(selectedEffect.Value);
                if (selectedEffect.Value != 0)
                    buttonRead.IsChecked = false;
                else
                    buttonRead.IsChecked = true;

                var myDevice = await controller.GetStateAsync();
                if (myDevice?.rgbw != null)
                {
                    if (selectedEffect.Value != 0)
                    {
                        labelCurrentEffect.Content = $"Wow effect no. {myDevice.rgbw.effectID}";
                        rectangleColor.Fill = Brushes.Gray;
                    }
                    else
                        labelCurrentEffect.Content = "Boring!";
                }
            }
            catch (Exception)
            {
                MessageBox.Show($"Host device ({tbIpAddress.Text}) not responding. Check the connection.");
            }
        }
        private void ColorPicker_Closed(object sender, RoutedEventArgs e)
        {

            if (colorPicker.SelectedColor.HasValue)
            {
                colour = colorPicker.SelectedColor;
                SetState();
                if (buttonRead.IsChecked == false)
                    buttonRead.IsChecked = true;
            }
        }
        private void ButtonRead_Check(object sender, EventArgs e)
        {
            dTimer.Start();
        }
        private void ButtonRead_Uncheck(object sender, EventArgs e)
        {
            dTimer.Stop();
        }
        private async void TgbToggle_Checked(object sender, RoutedEventArgs e)
        {
            //initial run - check last color which has been set before shutting down
            var myDevice = await controller.GetStateAsync();
            if (myDevice.rgbw != null)
            {
                if (string.IsNullOrEmpty(colourLast))
                {
                    string stringColor = myDevice.rgbw.lastOnColor.Remove(6, 2).Insert(0, "#FF");
                    await controller.SetColorAsync(stringColor);
                    colour = (Color)ColorConverter.ConvertFromString(stringColor);
                }
                else
                    await controller.SetColorAsync(colourLast.Remove(6, 2).Insert(0, "#FF"));

                buttonRead.IsEnabled = true;
                buttonRead.IsChecked = true;
                colorPicker.IsEnabled = true;
                comboboxEffect.IsEnabled = true;
                sliderDim.IsEnabled = true;
                textboxFadeTime.IsEnabled = true;
            }
            else
                tgbToggle.IsChecked = false;
        }
        private async void TgbToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            //TODO turn off the device, not sending black colour somehow..
            try
            {
                var myDevice = await controller.GetStateAsync();
                if (myDevice?.rgbw?.currentColor != null)
                {
                    colourLast = myDevice.rgbw.currentColor;
                }
                await controller.SetColorAsync("#FF000000");//max dim -> black

                labelCurrentEffect.Content = "";
                buttonRead.IsEnabled = false;
                colorPicker.IsEnabled = false;
                comboboxEffect.IsEnabled = false;
                sliderDim.IsEnabled = false;
                textboxFadeTime.IsEnabled = false;
            }
            catch (Exception)
            {
                MessageBox.Show($"Host device ({tbIpAddress.Text}) not responding. Check the connection.");
                rectangleColor.Fill = Brushes.Gray;
            }
        }
        private void TextboxFadeTime_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            textboxFadeTime.Clear();
        }
        private async void TextboxFadeTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!string.IsNullOrEmpty(textboxFadeTime.Text))
                {
                    if (int.TryParse(textboxFadeTime.Text, out int fadeTime) && fadeTime >= 1000 && fadeTime <= 3600000)
                    {
                        try
                        {
                            await controller.SetColorFade(fadeTime);
                            Keyboard.ClearFocus();
                        }
                        catch (Exception)
                        {
                            MessageBox.Show($"Host device ({tbIpAddress.Text}) not responding. Check the connection.");
                            rectangleColor.Fill = Brushes.Gray;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Enter integer time value in ms ranging from 1000 to 360000\nDouble-click to clear");
                    }
                }
            }
        }
        private void TbIpAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ButtonSetIp_Click(sender, e);
                Keyboard.ClearFocus();
            }
        }
    }
}
