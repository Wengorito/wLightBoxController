using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static LightBoxController.HttpObjects;

namespace LightBoxController
{
    public class LightBoxClass
    {
        /// <summary>
        /// To function library requires a HttpClient instantiated at the GUI side
        /// </summary>
        private HttpClient _httpClient;
        public LightBoxClass(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        /// <summary>
        /// Returns general information about device as RootDevice type object
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<RootDevice> GetInfoAsync(string uri = "/info")
        {
            RootDevice rootDev = new();
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
        /// <summary>
        /// Returns information about mode, selected effect, color and transition times as RootDeviceStateGet type object
        /// If both, desired color and desired effect are set, desired effect takes priority.
        /// </summary>
        /// <returns></returns>
        public async Task<RootDeviceStateGet> GetStateAsync()
        {
            RootDeviceStateGet rootStateGet;
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync("/api/rgbw/state");
                string responseBody = await response.Content.ReadAsStringAsync();
                rootStateGet = JsonSerializer.Deserialize<RootDeviceStateGet>(responseBody);
                Trace.WriteLine(ObjectDumper.Dump(rootStateGet));
            }
            catch (Exception ex)//catch here, to avoid messageboxes in the read loop
            {
                Trace.WriteLine("GET exception\n" + "Source : " + ex.Source + "\nMessage : " + ex.Message);
                return new RootDeviceStateGet { };
            }
            return new RootDeviceStateGet
            {
                rgbw = rootStateGet.rgbw
            };
        }
        /// <summary>
        /// Sets selected color. Accepts argument as RGBW hex string, e.g. "ABCDEFaa", Dim as optional integer in range 0 - 100 (default 100)
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="dim"></param>
        /// <returns></returns>
        public async Task SetColorAsync(string colour, int dim = 100)
        {
            string dimmedColour = "";
            for (int sub = 0; sub < 4; sub++)
            {
                string hexComponent = colour.Substring(sub * 2, 2);
                int intComponent = int.Parse(hexComponent, System.Globalization.NumberStyles.HexNumber);
                intComponent = intComponent * dim / 100;
                hexComponent = intComponent.ToString("X2");
                dimmedColour = string.Concat(dimmedColour, hexComponent);
            }
            RootDeviceStateSet rootStateSet = new();
            Rgbw myRgbw = new();
            myRgbw.desiredColor = dimmedColour;
            rootStateSet.rgbw = myRgbw;

            string stateJson = JsonSerializer.Serialize(rootStateSet);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("/api/rgbw/set", httpContent);
            Trace.WriteLine($"post response: {response}");
            if (response.IsSuccessStatusCode)
            {
                Trace.WriteLine($"Set color response status code OK. Colour: {dimmedColour}");
            }
            else
            {
                Trace.WriteLine($"Error: status code else than 200: {dimmedColour}");
                throw new Exception("setColorAsync has thrown an exception (!IsSuccessStatusCode)");
            }
        }
        /// <summary>
        /// Sets selected effect by the ID passed as an integer in range 0 - 10 (0 - no effect)
        /// </summary>
        /// <param name="effectId"></param>
        /// <returns></returns>
        public async Task SetEffect(int effectId)
        {
            RootDeviceStateSet myDevState = new();
            Rgbw myRgbw = new();
            DurationsMs myDuration = new();
            myRgbw.effectID = effectId;
            myDevState.rgbw = myRgbw;
            string stateJson = JsonSerializer.Serialize(myDevState);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("/api/rgbw/set", httpContent);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"setEffect has thrown an exception - Response Code: {response.StatusCode}");
        }
        /// <summary>
        /// Sets color fading time in miliseconds passed as integer in range 1000 - 360000
        /// </summary>
        /// <param name="fadeTime"></param>
        /// <returns></returns>
        public async Task SetColorFade(int fadeTime)
        {
            RootDeviceStateSet myDevState = new();
            Rgbw myRgbw = new();
            DurationsMs myDuration = new();
            myDuration.colorFade = fadeTime;
            myRgbw.durationsMs = myDuration;
            myDevState.rgbw = myRgbw;
            string stateJson = JsonSerializer.Serialize(myDevState);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("/api/rgbw/set", httpContent);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"setColorFade has thrown an exception - Response Code: {response.StatusCode}");
        }
    }
}
