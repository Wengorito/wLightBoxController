using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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
using LightBoxController;

namespace LightBoxGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string ipAddress { get; set; }
        public string httpUri { get; set; }

        public LightBoxClass controller = new LightBoxClass();


        public MainWindow()
        {
            InitializeComponent();

            //controller.getInfo();
            //controller.getState();
        }
        private void btnSetIP_Click(object sender, RoutedEventArgs e)
        {
            //traje i kacze WSZEDZIE
            //see if entered IP is valid
            ipAddress = tbIpAddress.Text;
            IPAddress ip;
            bool ValidateIP = IPAddress.TryParse(ipAddress, out ip);
            if (ValidateIP)
            {
                //MessageBox.Show("This is a valid ip address");
                httpUri = string.Concat("http://", ipAddress);
                Trace.WriteLine($"IP add set as {tbIpAddress.Text}");
            }
            else
                MessageBox.Show("This is not a valid ip address. Re-enter");

            //ewentualnie arp i z listy
        }
        private async void btnDeviceInfo_Click(object sender, RoutedEventArgs e)
        {
            //ErrorCode ErrorWrapper
            //jak zly ip to nie ma obiektu i wywala wyjatek - jak sie zabepieczyc?
            try
            {
                var myDevice = await controller.getInfo(httpUri);
                lblDeviceNameInfo.Content = myDevice.deviceName;// obj.device.deviceName;
                lblProductInfo.Content = myDevice.product;// obj.device.deviceName;
                lblApiLevelInfo.Content = myDevice.apiLevel;// obj.device.deviceName;
            }
            catch (Exception)
            {
                MessageBox.Show("Set IP first");
            }
        }
        private async void btnGetState_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var myDevice = await controller.getState(httpUri);
                StringBuilder sb = new StringBuilder(myDevice.rgbw.currentColor, 7);

                lblCurrentColor.Content = myDevice.rgbw.currentColor;
                lblCurrentColor.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#aa0000");//luminosity + rgb
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Set IP first");
            }
            
        }
        private void btnSetState_Click(object sender, RoutedEventArgs e)
        {

            //RootDeviceStateSet myDevState = new();

            try
            {
                controller.setState(httpUri);//, myDevState);
            }
            catch (Exception)
            {
                MessageBox.Show("Set IP first");
            }
        }
    }
}
