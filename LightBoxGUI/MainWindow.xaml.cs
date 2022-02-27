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
        public string ipAddress { get; set; }
        public string httpUri { get; set; }

        public LightBoxClass controller = new LightBoxClass();

        BackgroundWorker MyWorker = new ();

        private Timer timerStateUpdate;

        DispatcherTimer dTimer = new DispatcherTimer();



        public class ComboBoxColors
        {
            public string _Key { get; set; }
            public System.Windows.Media.Color _Value { get; set; }

            public ComboBoxColors(string _key, System.Windows.Media.Color _value)
            {
                _Key = _key;
                _Value = _value;
            }
        }
        public class ComboBoxEffects
        {
            public string _Key { get; set; }
            public int _Value { get; set; }

            public ComboBoxEffects(string _key, int _value)
            {
                _Key = _key;
                _Value = _value;
            }
        }

        public class LightingSettings
        {
            public System.Windows.Media.Color? _Colour { get; set; }
            public int _Mode { get; set; }
            public LightingSettings(System.Windows.Media.Color? _colour)
            {
                _Colour = _colour;
            }
        }
        //not very elegant
        public System.Windows.Media.Color? clr;
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeComboBox();
            ComboBoxColors cbp = (ComboBoxColors)cmbColor.SelectedItem;
            clr = cbp._Value;
            MyWorker.DoWork += MyWorker_DoWork;
            dTimer.Interval = TimeSpan.FromMilliseconds(200);
            dTimer.Tick += dTimer_Tick;

            //LightingSettings ledSet = new LightingSettings(clrPcker.SelectedColor);
            //controller.getInfo();
            //controller.getState();
        }
        async void dTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var myDevice = await controller.getStateAsync(httpUri);

                //odbieramy kolor w formacie RGBW
                var colArrayRgbw = myDevice.rgbw.currentColor.ToCharArray();

                char[] colArrayArgb = new char[6];
                Array.Copy(colArrayRgbw, colArrayArgb, 6);


                string stringColor = new string(colArrayArgb);
                string colorHashWrgb = stringColor.Insert(0, "#");

                Trace.WriteLine("Odebrany kolor: " + colorHashWrgb);

                lblCurrentColor.Content = myDevice.rgbw.currentColor;
                var colour = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHashWrgb);
                var brushes = Brushes.Yellow;
                rctColor.Fill = colour;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Source : " + ex.Source + "\nMessage : " + ex.Message);
            }
        }
        private void MyWorker_DoWork(object Sender, DoWorkEventArgs e)
        {

        }
        private void InitializeComboBox()
        {
            List<ComboBoxColors> cbColList = new List<ComboBoxColors>();

            cbColList.Add(new ComboBoxColors("Rot", System.Windows.Media.Color.FromArgb(255,255,0,0)));
            cbColList.Add(new ComboBoxColors("Grun", System.Windows.Media.Color.FromArgb(255, 0, 255, 0)));
            cbColList.Add(new ComboBoxColors("Blau", System.Windows.Media.Color.FromArgb(255, 0, 0, 255)));

            cmbColor.DisplayMemberPath = "_Key";
            cmbColor.SelectedValuePath = "_Value";
            cmbColor.ItemsSource = cbColList;
            cmbColor.SelectedIndex = 0;

            List<ComboBoxEffects> cbEffList = new List<ComboBoxEffects>();

            cbEffList.Add(new ComboBoxEffects("Chill", 1));
            cbEffList.Add(new ComboBoxEffects("Disco", 2));
            cbEffList.Add(new ComboBoxEffects("Techno", 3));

            cmbEffect.DisplayMemberPath = "_Key";
            cmbEffect.SelectedValuePath = "_Value";
            cmbEffect.ItemsSource = cbEffList;
            cmbEffect.SelectedIndex = 0;
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
                httpUri = string.Concat("http://", ipAddress);
                Trace.WriteLine($"IP add set as {tbIpAddress.Text}");
                dTimer.Start();
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
        private async void btnGetState_Click(object sender, EventArgs e)
        {
            try
            {
                var myDevice = await controller.getStateAsync(httpUri);

                //odbieramy kolor w formacie RGBW
                var colArrayRgbw = myDevice.rgbw.currentColor.ToCharArray();

                /*for (int i = colArrayRgbw.Length - 2; i > 1; i--)
                {
                    colArrayRgbw[i] = colArrayRgbw[i - 2];
                }
                colArrayRgbw[0] =  'f';
                colArrayRgbw[1] = 'f';*/

                char[] colArrayArgb = new char[6];
                Array.Copy(colArrayRgbw, colArrayArgb, 6);

                /*colArrayArgb[colArrayArgb.Length - 1] = colArrayRgbw.First();
                colArrayArgb[colArrayArgb.Length - 2] = colArrayRgbw[1];
                 
                colArrayArgb[0] = colArrayRgbw.Last();
                colArrayArgb[1] = colArrayRgbw[colArrayRgbw.Length - 2];*/
                string stringColor = new string(colArrayArgb);
                string colorHashWrgb = stringColor.Insert(0, "#");

                Trace.WriteLine("Odebrany kolor: " + colorHashWrgb);

                lblCurrentColor.Content = myDevice.rgbw.currentColor;
                var colour = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHashWrgb);
                var brushes = Brushes.Yellow;
                rctColor.Fill = colour;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Set IP first\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
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

        private async void btnSetState_Click(object sender, EventArgs e)
        {
            //string col = cmbColor.SelectedValue.ToString();
            try
            {
                string colHue = clr.ToString();
                int colValue = (int)sldValueHsv.Value;
                //System.Drawing.Color colour = System.Drawing.Color.FromName(cmbColor.SelectedItem.ToString());
                //RootDeviceStateSet myDevState = new();
                //var desCol = tbDesiredColour.Text;
                //var colour = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHashWrgb);

                //System.Drawing.Color colour = System.Drawing.Color.FromName(tbDesiredColour.Text);
                //aplha czanel?
                //System.Drawing.Color colour = System.Drawing.Color.FromName(col);
                //var colourHex = ColorTranslator.ToHtml(colour);
                //string colHex = HexConverter(colour);
                await controller.setColorAsync(httpUri, colHue, colValue);//, myDevState);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Set IP first\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
            }
        }
        private void sldValueHsv_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            btnSetState_Click(sender, e);
        }
        private void cmbColor_DropDownClosed(object sender, EventArgs e)
        {
            ComboBoxColors cbp = (ComboBoxColors)cmbColor.SelectedItem;
            clr = cbp._Value;
            btnSetState_Click(sender, e);
        }        
        private void cmbEffect_DropDownClosed(object sender, EventArgs e)
        {
            ComboBoxEffects cbe = (ComboBoxEffects)cmbEffect.SelectedItem;
            controller.SetEffect(httpUri, cbe._Value);
        }

        private void ClrPcker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            //co sie dzieje przy szybkim przelaczeniu kolorow???
            clr = clrPcker.SelectedColor;
            btnSetState_Click(sender, e);
        }

        private void clrPcker_Closed(object sender, RoutedEventArgs e)
        {
            //clr = clrPcker.SelectedColor;
            //btnSetState_Click(sender, e);
        }
    }
}
