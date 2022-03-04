using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

        private LightBoxClass controller = new LightBoxClass();

        DispatcherTimer dTimer = new DispatcherTimer();

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
            cbEffList.Add(new ComboBoxEffects("Chill", 1));
            cbEffList.Add(new ComboBoxEffects("Disco", 2));
            cbEffList.Add(new ComboBoxEffects("Techno", 3));

            cmbEffect.DisplayMemberPath = "_Key";
            cmbEffect.SelectedValuePath = "_Value";
            cmbEffect.ItemsSource = cbEffList;
            cmbEffect.SelectedIndex = 0;
        }
        private void dTimer_Tick(object sender, EventArgs e)
        {
            getState(sender, e);
        }
        private void btnSetIp_Click(object sender, RoutedEventArgs e)
        {
            ipAddress = tbIpAddress.Text;
            IPAddress ip;
            bool ValidateIP = IPAddress.TryParse(ipAddress, out ip);
            if (ValidateIP)
            {
                httpUri = string.Concat("http://", ipAddress);
                Trace.WriteLine($"IP address set: {tbIpAddress.Text}");
                btnSetIp.Background = Brushes.Green;
            }
            else
                MessageBox.Show("This is not a valid ip address. Re-enter");

            //find avaiable ip devices
        }
        private async void btnGetInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var myDevice = await controller.getInfo(httpUri);
                lblDeviceNameInfo.Content = myDevice.deviceName;
                lblProductInfo.Content = myDevice.product;
                lblApiLevelInfo.Content = myDevice.apiLevel;
            }
            catch (Exception)
            {
                MessageBox.Show("Set IP first");
            }
        }
        private async Task getState(object sender, EventArgs e)
        {
            try
            {
                var myDevice = await controller.getStateAsync(httpUri);
                if (myDevice.rgbw != null)
                {
                    string rgbColor = myDevice.rgbw.currentColor.Remove(6, 2);
                    rgbColor = rgbColor.Insert(0, "#");
                    Trace.WriteLine("Odebrany kolor: " + rgbColor);
                    var colour = (SolidColorBrush)new BrushConverter().ConvertFrom(rgbColor);
                    rctColor.Fill = colour;
                }
            }
            catch (Exception ex)
            {
                dTimer.Stop(); //so that the window won't pop up continously
                MessageBox.Show("Error\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
            }
        }
        private async void setState(object sender, EventArgs e)
        {
            if (tgbToggle.IsChecked == true)
            {
                try
                {
                    string colHue = clr.ToString();
                    int colValue = (int)sldValueHsv.Value;
                    await controller.setColorAsync(httpUri, colHue, colValue);
                    lblCurrentEffect.Content = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Set IP first\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Toggle the device");
            }
        }
        private void sldValueHsv_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {            
            setState(sender, e);

            if (btnRead.IsChecked == false)
                btnRead.IsChecked = true;
        }       
        private void cmbEffect_DropDownClosed(object sender, EventArgs e)
        {

            if (tgbToggle.IsChecked == true)
            {
                dTimer.Stop();
                btnRead.IsChecked = false;
                rctColor.Fill = Brushes.Gray;
                ComboBoxEffects cbe = (ComboBoxEffects)cmbEffect.SelectedItem;
                controller.setEffect(httpUri, cbe._Value);
                lblCurrentEffect.Content = "Wow effect nr: " + cbe._Value;
            }
            else
            {
                MessageBox.Show("Toggle the device");
            }
        }
        private void clrPcker_Closed(object sender, RoutedEventArgs e)
        {
            if (clrPcker.SelectedColor.HasValue)
            {
                clr = clrPcker.SelectedColor;
                setState(sender, e);
            }
            if (btnRead.IsChecked == false)
                btnRead.IsChecked = true;
        }
        private void btnRead_Check(object sender, EventArgs e)
        {
            if (tgbToggle.IsChecked == true)
            {
                dTimer.Start();
            }
            else
            {
                btnRead.IsChecked = false;
                MessageBox.Show("Toggle the device");
            }
        }
        private void btnRead_Uncheck(object sender, EventArgs e)
        {
            dTimer.Stop();
        }
        private async void tgbToggle_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var myDevice = await controller.getStateAsync(httpUri);
                if (myDevice.rgbw != null)
                {
                    string rgbColor = myDevice.rgbw.lastOnColor.Remove(6, 2);
                    rgbColor = rgbColor.Insert(0, "#");
                    Trace.WriteLine("Odebrany kolor: " + rgbColor);
                    var colour = (SolidColorBrush)new BrushConverter().ConvertFrom(rgbColor);
                    rctColor.Fill = colour;
                    clr = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(rgbColor);
                    btnRead_Check(sender, e);
                    btnRead.IsChecked = true;
                }
                else
                {
                    tgbToggle.IsChecked = false;
                    MessageBox.Show("Device unreachable");
                }
            }
            catch (Exception ex)
            {
                tgbToggle.IsChecked = false;
                MessageBox.Show("Device unreachable\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
            }

        }
        private void tgbToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            //TODO turn off the device, not sending black colour somehow..
            dTimer.Stop();            
            btnRead.IsChecked = false;
            rctColor.Fill = Brushes.Black;
        }
    }
}
