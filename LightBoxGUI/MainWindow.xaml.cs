using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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

        public MainWindow()
        {
            InitializeComponent();

            //LightBoxClass controller = new LightBoxClass();
            //controller.getInfo();
            //controller.getState();
        }

        private void btnDeviceInfo_Click(object sender, RoutedEventArgs e)
        {
            //ErrorCode ErrorWrapper
            LightBoxClass controller = new LightBoxClass();
            Device device = new Device();


            controller.getInfo(device, httpUri);

            //Trace.WriteLine(Excepti);

            //lblDeviceName.Content = "Device name: " + device.Summary;
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
    }
}
