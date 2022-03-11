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
        private readonly HttpClient _httpClient;
        public LightBoxClass(HttpClient httpClient)//(HttpMessageHandler httpMessageHandler)
        {
            _httpClient = httpClient;
        }
        //to be removed
        public LightBoxClass()
        {
        }

        /*************************wrong implementation***************************/
        //HttpInvoke cos tam dispose jest
        public void dispose()
        {
            _httpClient.Dispose();
        }
            
        public async Task<Device> getInfo(string uri = "/info")
        {
            RootDevice rootDevObj = new RootDevice();
            HttpResponseMessage response = await _httpClient.GetAsync(uri);
            //try
            //{
            //    response = 
            //    Trace.WriteLine(response.StatusCode);
            //}

            //catch (HttpRequestException)
            //{
            //    Trace.WriteLine("Host not responding");
            //}
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    //string responseBody = await _httpClient.GetStringAsync(uri);
                    rootDevObj = JsonSerializer.Deserialize<RootDevice>(responseBody);

                    foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(rootDevObj.device))
                    {
                        string name = descriptor.Name;
                        object value = descriptor.GetValue(rootDevObj.device);
                        Trace.WriteLine($"{name} = {value}");
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Host not responding\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
                    return new Device();
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
        public async Task<RootDeviceStateGet> getStateAsync()
        {
            RootDeviceStateGet rootStateObj = new();
            //try
            //{
            //    Trace.WriteLine(response.StatusCode);
            //}
            //catch (HttpRequestException)
            //{
            //    Trace.WriteLine("Host not responding");
            //}
            HttpResponseMessage response = await _httpClient.GetAsync("/api/rgbw/state");
            response.EnsureSuccessStatusCode();
            //throw?
            //exceptions catching
            //in dll or gui?
            try
            {
                string responseBody = await _httpClient.GetStringAsync("/api/rgbw/state");
                rootStateObj = JsonSerializer.Deserialize<RootDeviceStateGet>(responseBody);
                //output print
                Trace.WriteLine(ObjectDumper.Dump(rootStateObj));
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Host not responding\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
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
        /// Accepts color argument as #ARGB hex string, e.g. "#FFABCDEF". Dim as optional int in range 0 - 100, default 100
        /// </summary>
        /// <param name="httpUri"></param>
        /// <param name="colour"></param>
        /// <param name="dim"></param>
        /// <returns></returns>
        public async Task setColorAsync(string colour, int dim = 100)
        {
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
            await _httpClient.PostAsync("/api/rgbw/set", httpContent);
        }
        //public async Task setColorUnchangedAsync(string httpUri)
        //{
        //    string requestUri = httpUri + "/api/rgbw/set";
        //    RootDeviceStateSet myDevState = new RootDeviceStateSet();
        //    Rgbw myRgbw = new();
        //    myRgbw.desiredColor = "--------";
        //    myDevState.rgbw = myRgbw;

        //    string stateJson = JsonSerializer.Serialize<RootDeviceStateSet>(myDevState);
        //    HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
        //    await _httpClient.PostAsync(requestUri, httpContent);
        //}

        public async Task setEffect(int effectId)
        {
            RootDeviceStateSet myDevState = new RootDeviceStateSet();
            Rgbw myRgbw = new();
            DurationsMs myDuration = new();
            myRgbw.effectID = effectId;
            myDuration.effectStep = 500;
            myDevState.rgbw = myRgbw;
            string stateJson = JsonSerializer.Serialize<RootDeviceStateSet>(myDevState);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("/api/rgbw/set", httpContent);
        }
        public async Task setColorFade(int fadeTime)
        {
            RootDeviceStateSet myDevState = new RootDeviceStateSet();
            Rgbw myRgbw = new();
            DurationsMs myDuration = new();
            myDuration.colorFade = fadeTime;
            myRgbw.durationsMs = myDuration;
            myDevState.rgbw = myRgbw;
            string stateJson = JsonSerializer.Serialize<RootDeviceStateSet>(myDevState);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("/api/rgbw/set", httpContent);
        }
    }
}
