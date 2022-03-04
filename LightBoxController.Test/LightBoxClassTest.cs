using System;
using Xunit;
using LightBoxController;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace LightBoxController.Tests
{
    public class TestsFixture : IDisposable
    {
        public TestsFixture()
        {        
            // Do "global" initialization here; Only called once.
            LightBoxClass controller = new LightBoxClass();

        }

        public void Dispose()
        {
            // Do "global" teardown here; Only called once.            
        }
    }
    public class DummyTests : IClassFixture<TestsFixture>
    {
        public DummyTests(TestsFixture data)
        {
        }
    }
    public class LightBoxClassTest
    {


        [Fact]
        public async void IsEffectInRange_ReturnTrue()
        {
            LightBoxClass controller = new LightBoxClass();
            string httpUri = "http://192.168.4.1/api/rgbw/state";
            RootDeviceStateGet dev = await controller.getStateAsync(httpUri);
            Trace.WriteLine(dev.rgbw.effectID);
            Assert.InRange(dev.rgbw.effectID, 0, 10);
        }
        [Fact]
        public async void EffectId_SetCorrectly_ReturnTrue()
        {
            LightBoxClass controller = new LightBoxClass();
            string httpUri = "http://192.168.4.1/api/rgbw/state";
            int effectId = 5;
            controller.setEffect(httpUri, 2);
            //could use some await over here
            var myDevState = await controller.getStateAsync(httpUri);

            Assert.Equal(myDevState.rgbw.effectID, effectId);
        }
        [Fact]
        public async void IsDimmedColorComponentInRange_ReturnTrue()
        {
            LightBoxClass controller = new LightBoxClass();
            string color = "FFFFFF";
            var hex = controller.applyDim(color, 0, 50);
            
            Assert.InRange(hex, "00", "FF");
        }
    }
}
