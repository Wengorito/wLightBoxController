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
        public LightBoxClass(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        //to be removed
        public LightBoxClass()
        {
        }
            
        public async Task<RootDevice> getInfoAsync(string uri = "/info")
        {
            RootDevice rootDev = new ();
            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                rootDev = JsonSerializer.Deserialize<RootDevice>(responseBody);
            }
            else
            {
                Trace.WriteLine("Status code: not great success");
            }
            return new RootDevice
            {
                device = rootDev.device
            };
        }
        public async Task<RootDeviceStateGet> getStateAsync()
        {
            RootDeviceStateGet rootStateGet = new();
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync("/api/rgbw/state");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("GET exception\nSource : " + ex.Source + "\nMessage : " + ex.Message);
                return new RootDeviceStateGet { };
            }
            //response.EnsureSuccessStatusCode();
            //throw?
            //exceptions catching
            //in dll or gui?
            try
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                rootStateGet = JsonSerializer.Deserialize<RootDeviceStateGet>(responseBody);
                //output print
                Trace.WriteLine(ObjectDumper.Dump(rootStateGet));
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Host not responding\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
            }
            return new RootDeviceStateGet
            {
                rgbw = rootStateGet.rgbw
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
            RootDeviceStateSet rootStateSet = new RootDeviceStateSet();
            Rgbw myRgbw = new();
            myRgbw.desiredColor = colour;
            rootStateSet.rgbw = myRgbw;

            string stateJson = JsonSerializer.Serialize<RootDeviceStateSet>(rootStateSet);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            //Alternatively, you can omit the Content-Length header for your POST request
            //and use the Transfer-Encoding header instead.
            //If the Content-Length and Transfer-Encoding headers are missing,
            //the connection MUST be closed at the end of the response. 
            HttpResponseMessage response = await _httpClient.PostAsync("/api/rgbw/set", httpContent);
            Trace.WriteLine($"post response: {response}");
            if (response.IsSuccessStatusCode)
            {
                Trace.WriteLine($"Set color response status code OK. Colour: {colour}");
                //throw new Exception("Wyjątkowo nie udało się ustawić wyjątkowego koloru");
            }
            else
            {

                Trace.WriteLine($"Error: status code else than 200: {colour}");
                throw new Exception("Wyjątkowo nie udało się ustawić wyjątkowego koloru");

            }
            //response.EnsureSuccessStatusCode();
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
