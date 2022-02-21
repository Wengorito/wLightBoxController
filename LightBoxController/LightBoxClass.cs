using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Windows;
using Newtonsoft.Json;

namespace LightBoxController
{
    /*public class Device
    {        
        public Int32 upTimeS { get; set; }
        //public string deviceName { get; set; }
        public string type { get; set; }
        public string product { get; set; }
        public string hv { get; set; }
        public string fv { get; set; }
        public string universe { get; set; }
        public string apiLevel { get; set; }
        public string id { get; set; }
        public string ip { get; set; }
        public string availableFv { get; set; }

    }*/
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

    public class Root
    {
        public Device device { get; set; }
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
        //public static string Serialize<T>(T obj)
        //{
        //    DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
        //    MemoryStream ms = new MemoryStream();
        //    serializer.WriteObject(ms, obj);
        //    string retVal = Encoding.UTF8.GetString(ms.ToArray());
        //    return retVal;
        //}

        //public static T Deserialize<T>(string json)
        //{
        //    T obj = Activator.CreateInstance<T>();
        //    MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
        //    DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
        //    obj = (T)serializer.ReadObject(ms);
        //    ms.Close();
        //    return obj;
        //}

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

        //trzeba jakos przeskanowac urzadzenia ip dostepne
        public async void getInfo(Device device, string httpUri)
        {
            string jsonString =
@"{
  ""Date"": ""2019-08-01T00:00:00-07:00"",
  ""TemperatureCelsius"": 25,
  ""Summary"": ""Hot"",
  ""DatesAvailable"": [
    ""2019-08-01T00:00:00-07:00"",
    ""2019-08-02T00:00:00-07:00""
  ],
  ""TemperatureRanges"": {
                ""Cold"": {
                    ""High"": 20,
      ""Low"": -10
                },
    ""Hot"": {
                    ""High"": 60,
      ""Low"": 20
    }
            },
  ""SummaryWords"": [
    ""Cool"",
    ""Windy"",
    ""Humid""
  ]
}
";



            string requestUri = httpUri + "/info";
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
                Trace.WriteLine("Host not responding");
                //return result.StatusCode;

                //POMOCY Z TYMI ERRORAMI
                //JAK WY
            }
            string responseBody = await client.GetStringAsync(requestUri);
            Trace.WriteLine(responseBody);
            //DeserializeAsync

            //var dec = JsonSerializer.Deserialize<dynamic>(responseBody);
            //device = JsonSerializer.Deserialize<Device>(responseBody);
            device = JsonConvert.DeserializeObject<Device>(responseBody);


            //Trace.WriteLine($"API level: {device.apiLevel}");
            Trace.WriteLine($"Devize name: {device.deviceName}");
            Trace.WriteLine(device.deviceName);
            //Trace.WriteLine($"fv: {device.fv}");
            //Trace.WriteLine(device.fv);
            //return Device;
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
