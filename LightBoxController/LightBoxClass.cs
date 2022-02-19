using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace LightBoxController
{
    public class DeviceInfo
    {
        public string deviceName { get; set; }
        public string product { get; set; }
        public string type { get; set; }
        public int apiLevel { get; set; }
        public string hv { get; set; }

        public string fv { get; set; }

        public string id { get; set; }

        public string ip { get; set; }
    }
    public class DeviceState
    {

    }
    public class LightBoxClass
    {
        /*********helper methods***********/
        /// <summary>
        /// co robi?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, obj);
            string retVal = Encoding.UTF8.GetString(ms.ToArray());
            return retVal;
        }

        public static T Deserialize<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            obj = (T)serializer.ReadObject(ms);
            ms.Close();
            return obj;
        }

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

        private static readonly HttpClient client = new HttpClient(); //readonly?
        
        //trzeba jakos przeskanowac urzadzenia ip dostepne
        public async void getInfo()
        {
            var deviceInfo = new DeviceInfo();
            //IPAddress address = IPAddress.Parse("http://192.168.0.23/info");
            //var deviceInfo = new DeviceInfo();
            var result = await client.GetAsync("http://192.168.0.23/info");
            Trace.WriteLine(result.StatusCode);
            string responseBody = await client.GetStringAsync("http://192.168.0.23/info");
            Trace.WriteLine(responseBody);

            //return deviceInfo;
        }
        public async void getState()
        {
            //var deviceState = new DeviceState();
            try
            {
                var content = await client.GetStringAsync("http://192.168.0.23//api/rgbw/state");

                Trace.WriteLine("\ntu poczatek get state:\n" + content);
            }
            catch (HttpRequestException e)
            {
                Trace.WriteLine("\nException Caught!");
                Trace.WriteLine("Message :{0} ", e.Message);
            }

            //return deviceState;
        }
        //public void setState
    }
}
