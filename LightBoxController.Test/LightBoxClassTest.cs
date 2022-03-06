using System;
using Xunit;
using LightBoxController;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace LightBoxController.Tests
{
    public class LightBoxClassTest : IDisposable
    {
        LightBoxClass controller;
        string httpUri;

        public LightBoxClassTest()
        {
            controller = new LightBoxClass();
            httpUri = "http://192.168.0.103";
        }
        public void Dispose()
        {
            //controller.dispose();
        }

        [Fact]
        public async Task EffectId_SetCorrectly_ReturnTrue()
        {
            var myDevState = await controller.getStateAsync(httpUri);
            var currentEffectId = myDevState.rgbw.effectID;
            Random randomGenerator = new Random();
            int desiredEffectId = currentEffectId;
            while (desiredEffectId == currentEffectId)
            {
                desiredEffectId = randomGenerator.Next(10);
            }
            await controller.setEffect(httpUri, desiredEffectId);
            myDevState = await controller.getStateAsync(httpUri);

            Assert.Equal(desiredEffectId, myDevState.rgbw.effectID);
        }
        [Fact]
        public async Task Color_SetCorrectly_ReturnTrue()
        {
            var myDevState = await controller.getStateAsync(httpUri);
            var fadeTime = myDevState.rgbw.durationsMs.colorFade;
            string desiredColour = "#ffabcdef";
            await controller.setColorAsync(httpUri, desiredColour);
            Thread.Sleep(fadeTime);
            desiredColour = desiredColour.Remove(0, 3).Insert(6, "ff");
            myDevState = await controller.getStateAsync(httpUri);

            Assert.Equal(desiredColour, myDevState.rgbw.currentColor);
        }
        [Fact]
        public async Task FadeTime_SetCorrectly_ReturnTrue()
        {
            var myDevState = await controller.getStateAsync(httpUri);
            var currentFadeTime = myDevState.rgbw.durationsMs.colorFade;
            Random randomGenerator = new Random();
            int desiredFadeTime = currentFadeTime;
            while (desiredFadeTime == currentFadeTime)
            {
                desiredFadeTime = randomGenerator.Next(1000, 360000);
            }
            await controller.setColorFade(httpUri, desiredFadeTime);
            myDevState = await controller.getStateAsync(httpUri);
            if (desiredFadeTime == myDevState.rgbw.durationsMs.colorFade)
            Assert.Equal(desiredFadeTime, myDevState.rgbw.durationsMs.colorFade);
        }
        [Fact]
        public void IsDimmedColorComponentInRange_ReturnTrue()
        {
            string color = "FFFFFF";
            var hex =  controller.applyDim(color, 0, 100);
            
            Assert.InRange(hex, "00", "FF");
        }
    }
}
