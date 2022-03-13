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
using System.Text.Json;

namespace LightBoxController.Tests
{
    public class LightBoxClassTest
    {
        //https://gingter.org/2018/07/26/how-to-mock-httpclient-in-your-net-c-unit-tests/
        [Fact]
        public async Task Mock_GetInfo_ShouldNotReturnNull()
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
                    Content = new StringContent("{\"device\":{\"deviceName\":\"MyBleBoxdevicename\"," +
                    "\"product\":\"wLightBox_v3\",\"type\":\"wLightBox\",\"apiLevel\":\"20200518\"," +
                    "\"hv\":\"9.1d\",\"fv\":\"0.987\",\"id\":\"g650e32d2217\",\"ip\":\"192.168.1.11\"}}")
                })
                .Verifiable();
            // use real http client with mocked handler here
            HttpClient httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost.com/")
            };
            LightBoxClass controllerUnderTest  = new LightBoxClass(httpClient);

            //ACT
            var result = await controllerUnderTest.getInfoAsync("/api/test/whatever");

            // ASSERT
            //result.Should().NotBeNull(); // this is fluent assertions here...
            //result.Id.Should().Be(1);
            Assert.NotNull(result);

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

        [Fact]
        public async Task Mock_SetColorCorrectly_ShouldCallOnce()
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
                    Content = new StringContent("{\"rgbw\":{\"colorMode\":1,\"effectID\":2,\"desiredColor\":\"rrggbbww\"," +
                    "\"currentColor\":\"rrggbbww\",\"lastOnColor\":\"rrggbbww\"," +
                    "\"durationsMs\":{\"colorFade\":1000,\"effectFade\":1500,\"effectStep\":2000}}}")//content irrelevant
                })
                .Verifiable();
            // use real http client with mocked handler here
            HttpClient httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost.com/")
            };
            LightBoxClass controllerUnderTest = new LightBoxClass(httpClient);

            //ACT
            string colourStr = "#AAEECCBB";//hex exceeded and what?
            await controllerUnderTest.setColorAsync(colourStr);

            //ASSERT
            //since does not return anything, just check whether has been called
            //https://dev.to/gautemeekolsen/how-to-test-httpclient-with-moq-in-c-2ldp
            var expectedUri = new Uri("http://localhost.com/api/rgbw/set");
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a yourmamamamamamamma request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task Mock_SetEffectCorrectly_ShouldCallOnce()
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
                    Content = new StringContent("{\"rgbw\":{\"colorMode\":1,\"effectID\":2,\"desiredColor\":\"rrggbbww\"," +
                    "\"currentColor\":\"rrggbbww\",\"lastOnColor\":\"rrggbbww\"," +
                    "\"durationsMs\":{\"colorFade\":1000,\"effectFade\":1500,\"effectStep\":2000}}}")//content irrelevant
                })
                .Verifiable();
            // use real http client with mocked handler here
            HttpClient httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost.com/")
            };
            LightBoxClass controllerUnderTest = new LightBoxClass(httpClient);

            //ACT
            int effectInt = 10;
            await controllerUnderTest.setEffect(effectInt);

            //ASSERT
            //since does not return anything, just check whether has been called
            //https://dev.to/gautemeekolsen/how-to-test-httpclient-with-moq-in-c-2ldp
            var expectedUri = new Uri("http://localhost.com/api/rgbw/set");
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a yourmamamamamamamma request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }
        //Set incorrect id and then? unable to check, class shall keep an eye on it (but the user has preselected values only tho)

        [Fact]
        public async Task Mock_SetFadeTimeCorrectly_ShouldCallOnce()
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
                    Content = new StringContent("{\"rgbw\":{\"colorMode\":1,\"effectID\":2,\"desiredColor\":\"rrggbbww\"," +
                    "\"currentColor\":\"rrggbbww\",\"lastOnColor\":\"rrggbbww\"," +
                    "\"durationsMs\":{\"colorFade\":1000,\"effectFade\":1500,\"effectStep\":2000}}}")//content irrelevant
                })
                .Verifiable();
            // use real http client with mocked handler here
            HttpClient httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost.com/")
            };
            LightBoxClass controllerUnderTest = new LightBoxClass(httpClient);

            //ACT
            int effectInt = 11; //same as above
            await controllerUnderTest.setColorFade(effectInt);

            //ASSERT
            //since does not return anything, just check whether has been called
            //https://dev.to/gautemeekolsen/how-to-test-httpclient-with-moq-in-c-2ldp
            var expectedUri = new Uri("http://localhost.com/api/rgbw/set");
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a yourmamamamamamamma request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }

        //different approach (more generic)
        //https://peterdaugaardrasmussen.com/2018/10/11/c-how-to-mock-the-httpclient-for-tests/
        public class HttpMessageHandlerStub : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;
            public HttpMessageHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
            {
                _sendAsync = sendAsync;
            }
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return await _sendAsync(request, cancellationToken);
            }
        }
        [Fact]
        public async Task haendelStub_getInfo_ShouldReturnTheSame() //or just not null
        {
            // ARRANGE
            string expectedMessage = "{\"device\":{\"deviceName\":\"MyBleBoxdevicename\"," +
                    "\"product\":\"wLightBox_v3\",\"type\":\"wLightBox\",\"apiLevel\":\"20200518\"," +
                    "\"hv\":\"9.1d\",\"fv\":\"0.987\",\"id\":\"g650e32d2217\",\"ip\":\"192.168.1.11\"}}";
            var httpClient = new HttpClient(new HttpMessageHandlerStub(async (request, cancellationToken) =>
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedMessage)
                };
                return await Task.FromResult(responseMessage);
            }))
            {
                BaseAddress = new Uri("http://localhost.com/")
            };
            LightBoxClass controllerUnderTest = new LightBoxClass(httpClient);

            //ACT
            var result = await controllerUnderTest.getInfoAsync();

            // ASSERT
            //somehow ambiguous - deserialize and revert? check if serializer works correctly
            string resultString = JsonSerializer.Serialize<RootDevice>(result);
            Assert.Equal(expectedMessage, resultString);
        }

        [Fact]
        public async Task haendelStub_getState_IfResultNotAfflicted_ReturnTrue()
        {
            // ARRANGE
            string expectedMessage = "{\"rgbw\":{\"colorMode\":1,\"effectID\":2,\"desiredColor\":\"ff00300000\"," +
                "\"currentColor\":\"ff00300000\",\"lastOnColor\":\"ff00300000\"," +
                "\"durationsMs\":{\"colorFade\":1000,\"effectFade\":1500,\"effectStep\":2000}}}";
            var httpClient = new HttpClient(new HttpMessageHandlerStub(async (request, cancellationToken) =>
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedMessage)
                };
                return await Task.FromResult(responseMessage);
            }))
            {
                BaseAddress = new Uri("http://localhost.com/")
            };
            LightBoxClass controllerUnderTest = new LightBoxClass(httpClient);

            //ACT
            var result = await controllerUnderTest.getStateAsync();

            // ASSERT
            //somehow ambiguous - utilises serializer assuming it works correctly
            string resultString = JsonSerializer.Serialize<RootDeviceStateGet>(result);
            Assert.Equal(expectedMessage, resultString);
        }

        //[Fact]
        //public void IsDimmedColorComponentInRange_ReturnTrue()
        //{
        //    string color = "FFFFFF";
        //    var hex = controller.applyDim(color, 0, 100);

        //    Assert.InRange(hex, "00", "FF");
        //}
    }
}
