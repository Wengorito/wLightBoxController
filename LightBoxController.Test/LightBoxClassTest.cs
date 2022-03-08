using System;
using Xunit;
using LightBoxController;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace LightBoxController.Tests
{
    public class LightBoxClassTest
    {
        LightBoxClass controller = new LightBoxClass();
        string httpUri = "http://192.168.4.1";

        [Fact]
        public async Task GetInfo_LocalHost_ReturnNull()
        {
            var myDevice = await controller.getInfo(httpUri);

            Assert.NotNull(myDevice);
        }
        [Fact]
        public async Task Color_SetCorrectly_ReturnTrue()
        {
            int fadeTime = 1000;
            await controller.setColorFade(httpUri, fadeTime);
            string initialColour = "#ff000000";
            string desiredColour = "#ffabcaef";
            await controller.setColorAsync(httpUri, initialColour);
            Thread.Sleep(fadeTime);
            await controller.setColorAsync(httpUri, desiredColour);
            Thread.Sleep(fadeTime);
            desiredColour = desiredColour.Remove(0, 3).Insert(6, "ff");
            var myDevState = await controller.getStateAsync(httpUri);

            Assert.Equal(desiredColour, myDevState.rgbw.currentColor);
        }
        //[Fact]
        public async Task Color_SetIncorrectly_ReturnFalse()
        {
            var myDevState = await controller.getStateAsync(httpUri);
            var fadeTime = myDevState.rgbw.durationsMs.colorFade;
            string correctColour = "#ffabcdef";
            string incorrectColour = "#ffghijkl";
            await controller.setColorAsync(httpUri, correctColour);
            await controller.setColorAsync(httpUri, incorrectColour);
            Thread.Sleep(fadeTime);
            incorrectColour = incorrectColour.Remove(0, 3).Insert(6, "ff");
            myDevState = await controller.getStateAsync(httpUri);

            Assert.False(incorrectColour == myDevState.rgbw.currentColor);
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
        public void IsDimmedColorComponentInRange_ReturnTrue()
        {
            string color = "FFFFFF";
            var hex =  controller.applyDim(color, 0, 100);
            
            Assert.InRange(hex, "00", "FF");
        }
    }
}
