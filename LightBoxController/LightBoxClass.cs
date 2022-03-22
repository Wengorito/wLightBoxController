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
        private HttpClient _httpClient;
        public LightBoxClass(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
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
        private static string ApplyDim(string col, int sub, int dim)
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
        /// <param name="colour"></param>
        /// <param name="dim"></param>
        /// <returns></returns>
        public async Task SetColorAsync(string colour, int dim = 100)
        {
            //TODO - RGBWW
            colour = colour.Remove(0, 3);
            colour = string.Concat(
                ApplyDim(colour, 0, dim),
                ApplyDim(colour, 2, dim),
                ApplyDim(colour, 4, dim),
                "ff");
            RootDeviceStateSet rootStateSet = new();
            Rgbw myRgbw = new();
            myRgbw.desiredColor = colour;
            rootStateSet.rgbw = myRgbw;

            string stateJson = JsonSerializer.Serialize(rootStateSet);
            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("/api/rgbw/set", httpContent);
            Trace.WriteLine($"post response: {response}");
            if (response.IsSuccessStatusCode)
            {
                Trace.WriteLine($"Set color response status code OK. Colour: {colour}");
            }
            else
            {
                Trace.WriteLine($"Error: status code else than 200: {colour}");
                throw new Exception("setColorAsync has thrown an exception (!IsSuccessStatusCode)");
            }
        }
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
