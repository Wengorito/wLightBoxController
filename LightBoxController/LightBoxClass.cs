using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace LightBoxController
{
    #region
    public class Device
    {
        public string deviceName { get; set; }
        public string product { get; set; }
        public string type { get; set; }
        public string apiLevel { get; set; }
        public string hv { get; set; }
        public string fv { get; set; }
        public string id { get; set; }
        public string ip { get; set; }
    }
    public class RootDevice
    {
        public Device device { get; set; }
    }

    public class DurationsMs
    {
        public int colorFade { get; set; }
        public int effectFade { get; set; }
        public int effectStep { get; set; }
    }
    public class Rgbw
    {
        public int colorMode { get; set; }
        public int effectID { get; set; }
        public string desiredColor { get; set; }
        public string currentColor { get; set; }
        public string lastOnColor { get; set; }
        public DurationsMs durationsMs { get; set; }
    }
    public class RootDeviceStateGet
    {
        public Rgbw rgbw { get; set; }
    }
    public class RootDeviceStateSet
    {
        public Rgbw rgbw { get; set; }
    }
    #endregion
    public class LightBoxClass
    {
        /*********helper methods***********/
        /// <summary>
        /// co robi?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>

        //niepotrzebna jednak
        private static void ParseIP(string ipAddress)
        {
            try
            {
                // Create an instance of IPAddress for the specified address string (in
                // dotted-quad, or colon-hexadecimal notation).
                IPAddress address = IPAddress.Parse(ipAddress);

                // Display the address in standard notation.
                Trace.WriteLine("Parsing your input string: " + "\"" + ipAddress + "\"" + " produces this address (shown in its standard notation): " + address.ToString());
            }

            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }

            catch (FormatException e)
            {
                Console.WriteLine("FormatException caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }

            catch (Exception e)
            {
                Console.WriteLine("Exception caught!!!");
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
            }
        }
        /***************************/

        HttpClient client = new HttpClient(); //readonly?
        //http://192.168.0.23/ adres mojego boxa

        //192.168.240.163 galaxy

        //trzeba jakos przeskanowac urzadzenia ip dostepne
        public async Task<Device> getInfo(string httpUri)
        {
            string requestUri = httpUri + "/info";
            RootDevice rootDevObj = new RootDevice();
            //EXCEPTIONS
            //może to ten client rzuca błędami?
            //jak to do gui przekazac?
            //enum errrcode?
            HttpResponseMessage result;
            try
            {
                result = await client.GetAsync(requestUri);
                Trace.WriteLine(result.StatusCode);
            }
            catch (HttpRequestException)
            {
                Trace.WriteLine("Host not responding"); //to trzebe przekazac "wyzej", do GUI
                //return result.StatusCode;
                //return Task<Device>(null);

                //POMOCY Z TYMI ERRORAMI
                //JAK WYkOKRZystac te domyslne
            }
            try
            {
                string responseBody = await client.GetStringAsync(requestUri);
                //korzystać z default
                rootDevObj = JsonSerializer.Deserialize<RootDevice>(responseBody);

                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(rootDevObj.device))
                {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(rootDevObj.device);
                    Trace.WriteLine($"{name} = {value}");
                }
            }
            catch (HttpRequestException e)
            {
                Trace.WriteLine("Host not responding");
                Trace.WriteLine("ArgumentNullException caught!!!");
                Trace.WriteLine("Source : " + e.Source);
                Trace.WriteLine("Message : " + e.Message);

            }
            catch (InvalidOperationException)
            {
                Trace.WriteLine("IP address incorrect");
            }
            catch (TaskCanceledException)
            {
                Trace.WriteLine("Timeout reached");
            }

            //    foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(rootDevObj.device))
            //    {
            //        string name = descriptor.Name;
            //        object value = descriptor.GetValue(rootDevObj.device);
            //        Trace.WriteLine($"{name} = {value}");
            //    }

            return new Device 
            { 
                deviceName = rootDevObj.device.deviceName, 
                product = rootDevObj.device.product, 
                apiLevel = rootDevObj.device.apiLevel
            };

            /*Type t = rootDevObj.device.GetType(); // Where obj is object whose properties you need.
            PropertyInfo[] pi = t.GetProperties();
            foreach (PropertyInfo p in pi)
            {
                Trace.WriteLine(p.Name + " : " + p.GetValue(rootDevObj.device));
            }*/
        }
        public async Task<RootDeviceStateGet> getState(string httpUri)
        {
            string requestUri = httpUri + "/api/rgbw/state";
            RootDeviceStateGet rootStateObj = new RootDeviceStateGet();
            HttpResponseMessage result;
            try
            {
                result = await client.GetAsync(requestUri);
                Trace.WriteLine(result.StatusCode);
            }
            catch (HttpRequestException)
            {
                Trace.WriteLine("Host not responding"); //to trzebe przekazac "wyzej", do GUI
            }
            try
            {
                var content = await client.GetStringAsync(requestUri);

                Trace.WriteLine("\ntu poczatek get state:\n" + content);
            }
            catch (HttpRequestException e)
            {
                Trace.WriteLine("\nException Caught!");
                Trace.WriteLine("Message :{0} ", e.Message);
            }

            try
            {
                string responseBody = await client.GetStringAsync(requestUri);
                rootStateObj = JsonSerializer.Deserialize<RootDeviceStateGet>(responseBody);
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(rootStateObj.rgbw))
                {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(rootStateObj.rgbw);
                    Trace.WriteLine($"{name} = {value}");
                }
            }
            catch (HttpRequestException e)
            {
                Trace.WriteLine("Host not responding");
                Trace.WriteLine("ArgumentNullException caught!!!");
                Trace.WriteLine("Source : " + e.Source);
                Trace.WriteLine("Message : " + e.Message);

            }
            catch (InvalidOperationException)
            {
                Trace.WriteLine("IP address incorrect");
            }
            catch (TaskCanceledException)
            {
                Trace.WriteLine("Timeout reached");
            }
            return new RootDeviceStateGet
            {

            };
        }
        public async void setState(string httpUri, string settings)
        {
            //pobrac z guia parametry
            //wpisac do objektu
            //serializowac do jsona
            //wyslac do device

            //w gui podzialke pole naglowki itd
        }
    }
}
