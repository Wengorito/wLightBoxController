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
    //Model: get representation
    //wzorzec projektowy builder
    //fluent api
    //bottom up
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
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// co robi?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>

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
            
        public async Task<Device> getInfo(string httpUri)
        {
            string requestUri = httpUri + "/info";
            RootDevice rootDevObj = new RootDevice();
            HttpResponseMessage result = await client.GetAsync(requestUri);
            //try
            //{
            //    result = 
            //    Trace.WriteLine(result.StatusCode);
            //}

            //catch (HttpRequestException)
            //{
            //    Trace.WriteLine("Host not responding");
            //}
            if (result.IsSuccessStatusCode)
            {
                try
                {
                    string responseBody = await client.GetStringAsync(requestUri);
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

                    return new Device();
                }
                catch (InvalidOperationException)
                {
                    Trace.WriteLine("IP address incorrect");
                }
                catch (TaskCanceledException)
                {
                    Trace.WriteLine("Timeout reached");
                }
                return new Device
                {
                    deviceName = rootDevObj.device.deviceName,
                    product = rootDevObj.device.product,
                    apiLevel = rootDevObj.device.apiLevel
                };
            }
            else
            {
                Trace.WriteLine("Host not responding");
                return new Device { };
            }

        }
        public async Task<RootDeviceStateGet> getStateAsync(string httpUri)
        {
            RootDeviceStateGet rootStateObj = new();
            string requestUri = httpUri + "/api/rgbw/state";
            HttpResponseMessage result;
            try
            {
                result = await client.GetAsync(requestUri);
                Trace.WriteLine(result.StatusCode);
            }
            catch (HttpRequestException)
            {
                Trace.WriteLine("Host not responding"); //throw up
            }
            //throw
            try
            {
                string responseBody = await client.GetStringAsync(requestUri);
                rootStateObj = JsonSerializer.Deserialize<RootDeviceStateGet>(responseBody);
                //output print
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(rootStateObj.rgbw))
                {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(rootStateObj.rgbw);
                    if (name == "durationsMs")
                    {
                        foreach (PropertyDescriptor descriptor1 in TypeDescriptor.GetProperties(rootStateObj.rgbw.durationsMs))
                        {
                            string name1 = descriptor1.Name;
                            object value1 = descriptor1.GetValue(rootStateObj.rgbw.durationsMs);
                            Trace.WriteLine($"{name1} = {value1}");
                        }
                    }
                    else Trace.WriteLine($"{name} = {value}");
                }
            }
            catch (HttpRequestException e)
            {
                Trace.WriteLine("HttpRequestException caught");
                Trace.WriteLine("Source : " + e.Source);
                Trace.WriteLine("Message : " + e.Message);
            }
            catch (InvalidOperationException e)
            {
                Trace.WriteLine("IP address incorrect");
                Trace.WriteLine("Source : " + e.Source);
                Trace.WriteLine("Message : " + e.Message);
            }
            catch (TaskCanceledException e)
            {
                Trace.WriteLine("Timeout reached");
                Trace.WriteLine("Source : " + e.Source);
                Trace.WriteLine("Message : " + e.Message);
            }
            return new RootDeviceStateGet
            {
                rgbw = rootStateObj.rgbw
            };
        }
        public string applyDim(string col, int sub,  int dim)
        {
            string hexComponent = col.Substring(sub, 2);
            int intComponent = int.Parse(hexComponent, System.Globalization.NumberStyles.HexNumber);
            intComponent = intComponent * dim / 100;
            hexComponent = intComponent.ToString("X2");
            return hexComponent;
        }
        public async Task setColorAsync(string httpUri, string colour, int dim)//, RootDeviceStateSet rootStateObj)
        {
            string requestUri = httpUri + "/api/rgbw/set";
            //remove '#' & white - TODO colorModes
            colour = colour.Remove(0, 3);
            colour = string.Concat(
                applyDim(colour, 0, dim),
                applyDim(colour, 2, dim),
                applyDim(colour, 4, dim));

            colour = colour.Insert(6, "ff");

            RootDeviceStateSet myDevState = new RootDeviceStateSet();
            Rgbw myRgbw = new();
            DurationsMs myDuration = new();

            myRgbw.desiredColor = colour;
            myDuration.colorFade = 2000;
            myRgbw.durationsMs = myDuration;
            myDevState.rgbw = myRgbw;

            string stateJson = JsonSerializer.Serialize<RootDeviceStateSet>(myDevState);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            await client.PostAsync(requestUri, httpContent);
        }
        public void setEffect(string httpUri, int effectId)
        {
            string requestUri = httpUri + "/api/rgbw/set";
            RootDeviceStateSet myDevState = new RootDeviceStateSet();
            Rgbw myRgbw = new();
            DurationsMs myDuration = new();
            myRgbw.effectID = effectId;
            myDuration.effectStep = 500;
            myDevState.rgbw = myRgbw;
            string stateJson = JsonSerializer.Serialize<RootDeviceStateSet>(myDevState);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            client.PostAsync(requestUri, httpContent);

            //await?
        }
        public void setEffectGetResult(string httpUri, int effectId)
        {
            string requestUri = httpUri + "/api/rgbw/set";
            RootDeviceStateSet myDevState = new RootDeviceStateSet();
            Rgbw myRgbw = new();
            DurationsMs myDuration = new();
            myRgbw.effectID = effectId;
            myDuration.effectStep = 500;
            myDevState.rgbw = myRgbw;
            string stateJson = JsonSerializer.Serialize<RootDeviceStateSet>(myDevState);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            client.PostAsync(requestUri, httpContent);

            //await?
        }
    }
}
