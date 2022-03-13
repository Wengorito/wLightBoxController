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
        private string ipAddress { get; set; }
        private string httpUri { get; set; }

        private LightBoxClass controller;

        //private HttpClient httpClient = new();

        private DispatcherTimer dTimer = new DispatcherTimer();

        private Ping pingSender = new Ping();

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
        //global variable for keeping the colour, allows the Dim slider 
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
            getColorState(sender, e);
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
                        //inject HttpClient here, to enable testing
                        HttpClient httpClient = new()
                        {
                            BaseAddress = new Uri(httpUri),
                            Timeout = new TimeSpan(0, 0, 3)
                        };
                        controller = new LightBoxClass(httpClient);
                    }
                    else
                    {
                        btnSetIp.Background = Brushes.Gray;
                        MessageBox.Show("Selected IP device not available");
                        tgbToggle.IsEnabled = false;
                        btnGetInfo.IsEnabled = false;
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
                var myRootDevice = await controller.getInfoAsync();
                lblDeviceNameInfo.Content = myRootDevice.device.deviceName;
                lblProductInfo.Content = myRootDevice.device.product;
                lblApiLevelInfo.Content = myRootDevice.device.apiLevel;
            }
            catch (Exception)
            {
                MessageBox.Show($"Selected host ({tbIpAddress.Text}) not responding.");
            }
        }
        private async Task getColorState(object sender, EventArgs e)
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
                MessageBox.Show("async Task getColorState\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
            }
        }
        private async void setState()//some of the methods use this some directly call setColorAsync... brothel
        {
                try
                {
                    await controller.setColorAsync(clr.ToString(), (int)sldValueHsv.Value);
                    lblCurrentEffect.Content = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gui setState() method\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
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
                MessageBox.Show("async void cmbEffect_DropDownClosed\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
            }
        }
        private void clrPcker_Closed(object sender, RoutedEventArgs e)
        {

            if (clrPcker.SelectedColor.HasValue)
            {
                clr = clrPcker.SelectedColor;
                //clr = clrPcker.SelectedColorText;
                setState();
                //if (btnRead.IsChecked == false)
                    //btnRead.IsChecked = true;
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
                //first run - check last color which has been set before shutting down
                var myDevice = await controller.getStateAsync();
                //if (myDevice.rgbw != null)
                //{
                    if (String.IsNullOrEmpty(clrLast))                //both empty then what (tgl uncheck nulls clrLast)
                    {
                        string stringColor = myDevice.rgbw.lastOnColor.Remove(6, 2).Insert(0, "#FF");
                        //await controller.setColorUnchangedAsync(httpUri);
                        await controller.setColorAsync(stringColor);//clrLast.Remove(6, 2).Insert(0, "#FF"));
                        clr = (Color)System.Windows.Media.ColorConverter.ConvertFromString(stringColor);
                    }
                    else
                        await controller.setColorAsync(clrLast.Remove(6, 2).Insert(0, "#FF"));

                    btnRead.IsEnabled = true;
                    btnRead.IsChecked = true;
                    clrPcker.IsEnabled = true;
                    cmbEffect.IsEnabled = true;
                    sldValueHsv.IsEnabled = true;
                    tbFadeTime.IsEnabled = true;
                //}
                //else
                //    tgbToggle.IsChecked = false;
            }
            catch (Exception ex)
            {
                tgbToggle.IsEnabled = false;
                btnGetInfo.IsEnabled = false;
                btnSetIp.Background = Brushes.Gray;
                MessageBox.Show("tgbToggle_Checked: Device unreachable\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
                //failure procedure: proceed ping and if fails, disable all the controls
            }

        }
        private async void tgbToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            //TODO turn off the device, not sending black colour somehow..
            try
            {
                var myDevice = await controller.getStateAsync();
                if (myDevice?.rgbw?.currentColor != null)
                {
                    clrLast = myDevice.rgbw.currentColor;
                }
                //desiredColor as "--------" won't affect too
                await controller.setColorAsync("#FF000000");//max dim -> black
                //await controller.setColorAsync(clrLast.Remove(6, 2).Insert(0, "#FF"), 0);//max dim -> black
                //await controller.setColorAsync(clr.ToString(), 0);//max dim -> black
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
                //MessageBox.Show("tgbToggle_Unchecked\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
                MessageBox.Show($"Host device ({tbIpAddress.Text}) not responding");                btnRead.IsEnabled = false;

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
