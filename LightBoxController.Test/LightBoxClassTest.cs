using System;
using Xunit;
using LightBoxController;
using System.Threading.Tasks;

namespace LightBoxController.Tests
{
    public class LightBoxClassTest
    {
        [Fact]
        public async Task IsEffectInRange_ReturnTrue()
        {
            LightBoxClass controller = new LightBoxClass();
            string httpUri = "http://192.168.4.1/api/rgbw/state";
            RootDeviceStateGet dev = new();
            dev = await controller.getStateAsync(httpUri);

            Assert.InRange(dev.rgbw.effectID, 0, 10);
        }
        [Fact]
        public async void EffectId_SetCorrectly_ReturnTrue()
        {
            LightBoxClass controller = new LightBoxClass();
            string httpUri = "http://192.168.4.1/api/rgbw/state";
            int effectId = 2;
            controller.setEffect(httpUri, 2);
            var myDevState = await controller.getStateAsync(httpUri);

            Assert.Equal(myDevState.rgbw.effectID, effectId);
        }

    }
}
