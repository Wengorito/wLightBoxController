using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

        //to be removed
        public void dispose()
        {
            client.Dispose();
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
                Trace.WriteLine("Host not responding");
            }
            //throw?
            try
            {
                string responseBody = await client.GetStringAsync(requestUri);
                rootStateObj = JsonSerializer.Deserialize<RootDeviceStateGet>(responseBody);
                //output print
                Trace.WriteLine(ObjectDumper.Dump(rootStateObj));

                //foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(rootStateObj.rgbw))
                //{
                //    string name = descriptor.Name;
                //    object value = descriptor.GetValue(rootStateObj.rgbw);
                //    if (name == "durationsMs")
                //    {
                //        foreach (PropertyDescriptor descriptor1 in TypeDescriptor.GetProperties(rootStateObj.rgbw.durationsMs))
                //        {
                //            string name1 = descriptor1.Name;
                //            object value1 = descriptor1.GetValue(rootStateObj.rgbw.durationsMs);
                //            Trace.WriteLine($"{name1} = {value1}");
                //        }
                //    }
                //    else Trace.WriteLine($"{name} = {value}");
                //}
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
        /// <summary>
        /// Accepts color argument as #ARGB hex string, e.g. "#FFABCDEF"
        /// </summary>
        /// <param name="httpUri"></param>
        /// <param name="colour"></param>
        /// <param name="dim"></param>
        /// <returns></returns>
        public async Task setColorAsync(string httpUri, string colour, int dim = 100)
        {
            string requestUri = httpUri + "/api/rgbw/set";
            //remove '#' and Alpha - TODO colorModes
            colour = colour.Remove(0, 3);
            colour = string.Concat(
                applyDim(colour, 0, dim),
                applyDim(colour, 2, dim),
                applyDim(colour, 4, dim),
                "ff");

            RootDeviceStateSet myDevState = new RootDeviceStateSet();
            Rgbw myRgbw = new();
            myRgbw.desiredColor = colour;
            myDevState.rgbw = myRgbw;

            string stateJson = JsonSerializer.Serialize<RootDeviceStateSet>(myDevState);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            await client.PostAsync(requestUri, httpContent);
        }
        public async Task setColorUnchangedAsync(string httpUri)
        {
            string requestUri = httpUri + "/api/rgbw/set";
            RootDeviceStateSet myDevState = new RootDeviceStateSet();
            Rgbw myRgbw = new();
            myRgbw.desiredColor = "--------";
            myDevState.rgbw = myRgbw;

            string stateJson = JsonSerializer.Serialize<RootDeviceStateSet>(myDevState);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            await client.PostAsync(requestUri, httpContent);
        }

        public async Task setEffect(string httpUri, int effectId)
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
            await client.PostAsync(requestUri, httpContent);
        }
        public async Task setColorFade(string httpUri, int fadeTime)
        {
            string requestUri = httpUri + "/api/rgbw/set";
            RootDeviceStateSet myDevState = new RootDeviceStateSet();
            Rgbw myRgbw = new();
            DurationsMs myDuration = new();
            myDuration.colorFade = fadeTime;
            myRgbw.durationsMs = myDuration;
            myDevState.rgbw = myRgbw;
            string stateJson = JsonSerializer.Serialize<RootDeviceStateSet>(myDevState);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            await client.PostAsync(requestUri, httpContent);
        }
    }
}
