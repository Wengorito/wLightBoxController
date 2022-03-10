using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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
        private string ipAddress { get; set; }
        private string httpUri { get; set; }

        private LightBoxClass controller;

        DispatcherTimer dTimer = new DispatcherTimer();

        Ping pingSender = new Ping();

        private class ComboBoxEffects
        {
            public string _Key { get; set; }
            public int _Value { get; set; }

            public ComboBoxEffects(string _key, int _value)
            {
                _Key = _key;
                _Value = _value;
            }
        }
        
        private System.Windows.Media.Color? clr;
        private string? clrLast;
        
        public MainWindow()
        {            
            InitializeComponent();
            InitializeComboBox();
            dTimer.Interval = TimeSpan.FromMilliseconds(100);
            dTimer.Tick += dTimer_Tick;
        }
        private void InitializeComboBox()
        {
            List<ComboBoxEffects> cbEffList = new List<ComboBoxEffects>();
            cbEffList.Add(new ComboBoxEffects("No effect", 0));
            cbEffList.Add(new ComboBoxEffects("Chill", 1));
            cbEffList.Add(new ComboBoxEffects("Disco", 2));
            cbEffList.Add(new ComboBoxEffects("Techno", 3));
            cbEffList.Add(new ComboBoxEffects("Effect Axe", 4));

            cmbEffect.DisplayMemberPath = "_Key";
            cmbEffect.SelectedValuePath = "_Value";
            cmbEffect.ItemsSource = cbEffList;
            cmbEffect.SelectedIndex = 0;
        }
        private void dTimer_Tick(object sender, EventArgs e)
        {
            getStateColor(sender, e);
        }
        private void btnSetIp_Click(object sender, RoutedEventArgs e)
        {
            ipAddress = tbIpAddress.Text;
            IPAddress ip;
            bool ValidateIP = IPAddress.TryParse(ipAddress, out ip);
            if (ValidateIP)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    PingReply reply = pingSender.Send(ip, 2000);
                    if (reply.Status == IPStatus.Success)
                    {
                        httpUri = string.Concat("http://", ipAddress);
                        Trace.WriteLine($"IP address set: {tbIpAddress.Text}");
                        btnSetIp.Background = Brushes.Green;
                        btnGetInfo.IsEnabled = true;
                        tgbToggle.IsEnabled = true;

                        //controller.dispose();
                        //TODO singleton
                        //Timeout wywala
                        controller = new LightBoxClass(httpUri);

                    }
                    else
                    {
                        btnSetIp.Background = Brushes.Gray;
                        MessageBox.Show("Selected IP device not available");
                    }
                }
                else
                    MessageBox.Show("Enter valid IPv4 address");
            }
            else
                MessageBox.Show("This is not a valid ip address. Re-enter");
        }

        private async void btnGetInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var myDevice = await controller.getInfo();
                lblDeviceNameInfo.Content = myDevice.deviceName;
                lblProductInfo.Content = myDevice.product;
                lblApiLevelInfo.Content = myDevice.apiLevel;
            }
            catch (Exception)
            {
                MessageBox.Show("Host not responding. Set correct IP");
            }
        }
        private async Task getStateColor(object sender, EventArgs e)
        {
            try
            {
                var myDevice = await controller.getStateAsync();
                if (myDevice?.rgbw != null)
                {
                    rctColor.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom($"#{myDevice.rgbw.currentColor.Remove(6, 2)}");
                }
            }
            catch (Exception ex)
            {
                dTimer.Stop(); //so that the window won't pop up continously
                MessageBox.Show("Error\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
            }
        }
        private async void setState()
        {
                try
                {
                    await controller.setColorAsync(clr.ToString(), (int)sldValueHsv.Value);
                    lblCurrentEffect.Content = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
                }
        }
        private void sldValueHsv_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
                setState();
                if (btnRead.IsChecked == false)
                    btnRead.IsChecked = true;
        }       
        private async void cmbEffect_DropDownClosed(object sender, EventArgs e)
        {
                dTimer.Stop();
                rctColor.Fill = Brushes.Gray;
                ComboBoxEffects cbe = (ComboBoxEffects)cmbEffect.SelectedItem;
                await controller.setEffect(cbe._Value);
                if (cbe._Value != 0)
                    btnRead.IsChecked = false;
                else
                    btnRead.IsChecked = true;
                try
                {
                    var myDevice = await controller.getStateAsync();
                    if (myDevice?.rgbw != null)
                    {
                    if (cbe._Value != 0)
                    {
                        lblCurrentEffect.Content = $"Wow effect no. {myDevice.rgbw.effectID}";
                        rctColor.Fill = Brushes.Gray;
                    }
                    else
                        lblCurrentEffect.Content = "Boring!";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
                }
        }
        private void clrPcker_Closed(object sender, RoutedEventArgs e)
        {
                if (clrPcker.SelectedColor.HasValue)
                {
                    clr = clrPcker.SelectedColor;
                    //clr = clrPcker.SelectedColorText;
                    setState();
                    if (btnRead.IsChecked == false)
                        btnRead.IsChecked = true;
                }
        }
        private void btnRead_Check(object sender, EventArgs e)
        {
            dTimer.Start();
        }
        private void btnRead_Uncheck(object sender, EventArgs e)
        {
            dTimer.Stop();
        }
        private async void tgbToggle_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var myDevice = await controller.getStateAsync();

                if (String.IsNullOrEmpty(clrLast))
                    //await controller.setColorUnchangedAsync(httpUri);
                    await controller.setColorAsync(myDevice.rgbw.lastOnColor.Remove(6, 2).Insert(0, "#FF"));//clrLast.Remove(6, 2).Insert(0, "#FF"));

                else
                    await controller.setColorAsync(clrLast.Remove(6, 2).Insert(0, "#FF"));

                btnRead.IsEnabled = true;
                btnRead.IsChecked = true;
                clrPcker.IsEnabled = true;
                cmbEffect.IsEnabled = true;
                sldValueHsv.IsEnabled = true;
                tbFadeTime.IsEnabled = true;
            }
            catch (Exception ex)
            {
                tgbToggle.IsChecked = false;
                MessageBox.Show("Device unreachable\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
            }

        }
        private async void tgbToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            //TODO turn off the device, not sending black colour somehow..
            try
            {
                var myDevice = await controller.getStateAsync();
                if (myDevice?.rgbw != null)
                {
                    clrLast = myDevice.rgbw.currentColor;
                }
                //desiredColor as "--------" won't affect too
                await controller.setColorAsync(clrLast.Remove(6, 2).Insert(0, "#FF"), 0);
                lblCurrentEffect.Content = "";

                //dTimer.Stop();

                //btnRead.IsChecked = false;
                btnRead.IsEnabled = false;
                clrPcker.IsEnabled = false;
                cmbEffect.IsEnabled = false;
                sldValueHsv.IsEnabled = false;
                tbFadeTime.IsEnabled = false;
                //Thread.Sleep(1000);//(myDevice.rgbw.durationsMs.colorFade);
                //rctColor.Fill = Brushes.Black;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
            }
        }

        private void tbFadeTime_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            tbFadeTime.Clear();
        }
        private async void tbFadeTime_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!String.IsNullOrEmpty(tbFadeTime.Text))
                {

                    if (Int32.TryParse(tbFadeTime.Text, out int fadeTime) && fadeTime >= 1000 && fadeTime <= 3600000)
                    {
                        await controller.setColorFade(fadeTime);
                        Keyboard.ClearFocus();
                    }
                    else
                    {
                        MessageBox.Show("Enter integer time value in ms ranging from 1000 to 360000\nDouble-click to clear");
                    }
                }       
            }
        }

        private void tbIpAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                btnSetIp_Click(sender, e);
                Keyboard.ClearFocus();
            }
        }
    }
}
