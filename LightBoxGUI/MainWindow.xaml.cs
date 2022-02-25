using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
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
            InitializeComboBox();

            //controller.getInfo();
            //controller.getState();
        }
        private void InitializeComboBox()
        {
            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                cmbColorList.Items.Add(ConfigurationManager.AppSettings.Get(key));
            }
            //dictionary?
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

                var colArrayRgbw = myDevice.rgbw.currentColor.ToCharArray();
                Trace.WriteLine("my color string: " + myDevice.rgbw.currentColor);

                char[] colArrayWrgb = new char[6];
                Array.Copy(colArrayRgbw, colArrayWrgb, 6);

                colArrayWrgb[colArrayWrgb.Length - 1] = colArrayRgbw.First();
                colArrayWrgb[colArrayWrgb.Length - 2] = colArrayRgbw[1];
                 
                colArrayWrgb[0] = colArrayRgbw.Last();
                colArrayWrgb[1] = colArrayRgbw[colArrayRgbw.Length - 2];
                string stringColor = new string(colArrayWrgb);
                string colorHashWrgb = stringColor.Insert(0, "#");

                Trace.WriteLine("Twoj kolor: " + colorHashWrgb);

                lblCurrentColor.Content = myDevice.rgbw.currentColor;
                var colour = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHashWrgb);
                var brushes = Brushes.Yellow;
                lblCurrentColor.Background = colour;//luminosity + rgb
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Set IP first");
            }
            
        } 
        public static String HexConverter(System.Drawing.Color c)
        {
            String rtn = String.Empty;
            try
            {
                rtn = "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
            }
            catch (Exception ex)
            {
                //doing nothing
            }

            return rtn;
        }

        private void btnSetState_Click(object sender, RoutedEventArgs e)
        {
            //string col = cmbColorList.SelectedValue.ToString();

            System.Drawing.Color colour = System.Drawing.Color.FromName(cmbColorList.SelectedItem.ToString());
            //RootDeviceStateSet myDevState = new();
            //var desCol = tbDesiredColour.Text;
            //var colour = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHashWrgb);

            //System.Drawing.Color colour = System.Drawing.Color.FromName(tbDesiredColour.Text);
            //aplha czanel?
            //System.Drawing.Color colour = System.Drawing.Color.FromName(col);
            //var colourHex = ColorTranslator.ToHtml(colour);
            string colHex = HexConverter(colour);

            try
            {
                controller.setState(httpUri, colHex);//, myDevState);
            }
            catch (Exception)
            {
                MessageBox.Show("Set IP first");
            }
        }
    }
}
