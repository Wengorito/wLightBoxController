using System;
using Xunit;
using LightBoxController;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using MockHttp;
using Moq;
using Moq.Protected;

namespace LightBoxController.Tests
{
    public class LightBoxClassTest
    {
        [Fact]
        //https://gingter.org/2018/07/26/how-to-mock-httpclient-in-your-net-c-unit-tests/
        public async Task Test_MockHttp()
        {
            // ARRANGE
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )             
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"device\":{\"deviceName\":\"My wLightBox\",\"type\":\"wLightBox\",\"product\":\"wLightBox\",\"hv\":\"5.2\",\"fv\":\"0.1022\",\"universe\":0,\"apiLevel\":\"20200229\",\"id\":\"a6cf12f8db4b\",\"ip\":\"\",\"timestampRequired\":1,\"availableFv\":null}}")
                })
                .Verifiable();
            // use real http client with mocked handler here
            HttpClient httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost.com/")
            };
            LightBoxClass controllerUnderTest  = new LightBoxClass(httpClient);

            //ACT
            var result = await controllerUnderTest.getInfo("/api/test/whatever");

            // ASSERT
            //result.Should().NotBeNull(); // this is fluent assertions here...
            //result.Id.Should().Be(1);
            string expectedName = "My wLightBox";
            Assert.Equal(expectedName, result.deviceName);

            // also check the 'http' call was like we expected it
            var expectedUri = new Uri("http://localhost.com/api/test/whatever");
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get  // we expected a GET request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }


        //obsolete
        LightBoxClass controller = new LightBoxClass();


        //[Fact]
        public async Task GetInfo_LocalHost_ReturnNull()
        {
            var myDevice = await controller.getInfo();

            Assert.NotNull(myDevice);
        }
        //[Fact]
        public async Task Color_SetCorrectly_ReturnTrue()
        {
            int fadeTime = 1000;
            await controller.setColorFade(fadeTime);
            string initialColour = "#ff000000";
            string desiredColour = "#ffabcaef";
            await controller.setColorAsync(initialColour);
            Thread.Sleep(fadeTime);
            await controller.setColorAsync(desiredColour);
            Thread.Sleep(fadeTime);
            desiredColour = desiredColour.Remove(0, 3).Insert(6, "ff");
            var myDevState = await controller.getStateAsync();

            Assert.Equal(desiredColour, myDevState.rgbw.currentColor);
        }
        //[Fact]
        public async Task Color_SetIncorrectly_ReturnFalse()
        {
            var myDevState = await controller.getStateAsync();
            var fadeTime = myDevState.rgbw.durationsMs.colorFade;
            string correctColour = "#ffabcdef";
            string incorrectColour = "#ffghijkl";
            await controller.setColorAsync(correctColour);
            await controller.setColorAsync(incorrectColour);
            Thread.Sleep(fadeTime);
            incorrectColour = incorrectColour.Remove(0, 3).Insert(6, "ff");
            myDevState = await controller.getStateAsync();

            Assert.False(incorrectColour == myDevState.rgbw.currentColor);
        }
        //[Fact]
        public async Task FadeTime_SetCorrectly_ReturnTrue()
        {
            var myDevState = await controller.getStateAsync();
            var currentFadeTime = myDevState.rgbw.durationsMs.colorFade;
            Random randomGenerator = new Random();
            int desiredFadeTime = currentFadeTime;
            while (desiredFadeTime == currentFadeTime)
            {
                desiredFadeTime = randomGenerator.Next(1000, 360000);
            }
            await controller.setColorFade(desiredFadeTime);
            myDevState = await controller.getStateAsync();
            if (desiredFadeTime == myDevState.rgbw.durationsMs.colorFade)
                Assert.Equal(desiredFadeTime, myDevState.rgbw.durationsMs.colorFade);
        }
        //[Fact]
        public async Task EffectId_SetCorrectly_ReturnTrue()
        {
            var myDevState = await controller.getStateAsync();
            var currentEffectId = myDevState.rgbw.effectID;
            Random randomGenerator = new Random();
            int desiredEffectId = currentEffectId;
            while (desiredEffectId == currentEffectId)
            {
                desiredEffectId = randomGenerator.Next(10);
            }
            await controller.setEffect(desiredEffectId);
            myDevState = await controller.getStateAsync();

            Assert.Equal(desiredEffectId, myDevState.rgbw.effectID);
        }
        //[Fact]
        public void IsDimmedColorComponentInRange_ReturnTrue()
        {
            string color = "FFFFFF";
            var hex = controller.applyDim(color, 0, 100);

            Assert.InRange(hex, "00", "FF");
        }
    }
}
