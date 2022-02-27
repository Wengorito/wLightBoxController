﻿using System;
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
    //osoban klaska
    //Model: get representation

    //enumy na kolory i mody

    //library przerzucac bledy htttp do ui (poszukac biblioteki enum z bledami http)

    //rester do firefox
    //https://stackoverflow.com/questions/6507889/how-to-ignore-a-property-in-class-if-null-using-json-net

    #region
    //wzorzec projektowy builder
    //fluent api
    //rootdevicestateset.builder().rgbw.colorMode
    //bottom up

    //poszukac jakiejs ladnego builder (lombok w javie)
    //   PostRepresentation.builder()
    //.rgbw
    //	.desiredColor("ff00300000")
    //	.durationMs
    //		.colorFade(1000)
    //		.build()
    //	.build()
    //.build();
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

        /*********helper methods***********/
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
            //wywala przy nieudanej probie polaecznia
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
        RootDeviceStateGet rootStateObj = new();
        public async Task<RootDeviceStateGet> getStateAsync(string httpUri)
        {
            string requestUri = httpUri + "/api/rgbw/state";
            //_ = new RootDeviceStateGet();
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
            //throw jakis dac i w metodzie ktora wola lapac, finally na pozioie ui shandlwoac i komunikowac

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
                rgbw = rootStateObj.rgbw
            };

        }
        private string applyDim(string col, int sub,  int dim)
        {
            string hexComponent = col.Substring(sub, 2);
            int intComponent = int.Parse(hexComponent, System.Globalization.NumberStyles.HexNumber);
            intComponent = intComponent * dim / 100;
            hexComponent = intComponent.ToString("X2");
            return hexComponent;
        }
        public async Task setColorAsync(string httpUri, string colour, int dim)//, RootDeviceStateSet rootStateObj)
        {
            //pobrac z guia parametry
            //wpisac do objektu
            //serializowac do jsona
            //wyslac do device

            string requestUri = httpUri + "/api/rgbw/set";
            //remove '#'
            colour = colour.Remove(0, 3);
            colour = string.Concat(
                applyDim(colour, 0, dim),
                applyDim(colour, 2, dim),
                applyDim(colour, 4, dim));

            colour = colour.Insert(6, "ff");

            RootDeviceStateSet myDevState = new RootDeviceStateSet();
            Rgbw myRgbw = new();
            DurationsMs myDuration = new();




            //bottom up

            Trace.WriteLine("current color: " + myRgbw.currentColor);
            myRgbw.desiredColor = colour;
            //myRgbw.durationsMs.colorFade = 1000;
            Trace.WriteLine("desired color: " + myRgbw.desiredColor);
            myDuration.colorFade = 5000;
            myRgbw.durationsMs = myDuration;
            myDevState.rgbw = myRgbw;

            //może trzeba pobrać get najpierw i do tego obiektu powpisywac?

            //rootStateObj.rgbw.colorMode = 4;
            //Trace.WriteLine(rootStateObj.rgbw.colorMode);

            //czy mozna ustawic deffaultowe zachowenia serializerwoi
            string stateJson = JsonSerializer.Serialize<RootDeviceStateSet>(myDevState);
            Trace.WriteLine(stateJson);

            HttpContent httpContent = new StringContent(stateJson, Encoding.UTF8, "application/json");
            await client.PostAsync(requestUri, httpContent);
        }
        public void SetEffect(string httpUri, int effectId)
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
        }
    }
}
