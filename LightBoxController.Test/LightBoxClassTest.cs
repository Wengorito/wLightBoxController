using System;
using Xunit;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Moq;
using Moq.Protected;
using System.Text.Json;
using static LightBoxController.HttpObjects;

namespace LightBoxController.Tests
{
    public class LightBoxClassTest
    {
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
        public async Task Mock_GetInfoShouldNotReturnNull()
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
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost.com/")
            };
            LightBoxClass controllerUnderTest = new(httpClient);

            //ACT
            RootDevice result = await controllerUnderTest.GetInfoAsync("/api/test/whatever");

            // ASSERT
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
        public async Task Mock_SetColorCorrectlyShouldCallOnce()
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
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost.com/")
            };
            LightBoxClass controllerUnderTest = new LightBoxClass(httpClient);

            //ACT
            string colourString = "FFAAEECB";
            await controllerUnderTest.SetColorAsync(colourString);

            //ASSERT
            //since does not return anything, just check whether has been called
            //https://dev.to/gautemeekolsen/how-to-test-httpclient-with-moq-in-c-2ldp
            var expectedUri = new Uri("http://localhost.com/api/rgbw/set");
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a post request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }
        [Fact]
        public async Task Mock_SetEffectCorrectlyShouldCallOnce()
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
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost.com/")
            };
            LightBoxClass controllerUnderTest = new LightBoxClass(httpClient);

            //ACT
            int effectInt = 10;
            await controllerUnderTest.SetEffect(effectInt);

            //ASSERT
            //since does not return anything, just check whether has been called
            //https://dev.to/gautemeekolsen/how-to-test-httpclient-with-moq-in-c-2ldp
            var expectedUri = new Uri("http://localhost.com/api/rgbw/set");
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Post  // we expected a post request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }
        [Fact]
        public async Task Mock_SetFadeTimeCorrectlyShouldCallOnce()
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
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost.com/")
            };
            LightBoxClass controllerUnderTest = new LightBoxClass(httpClient);

            //ACT
            int effectInt = 11; //same as above
            await controllerUnderTest.SetColorFade(effectInt);

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
        [Fact]
        public async Task HandlerStub_GetInfoShouldReturnTheSame()
        {
            // ARRANGE
            string expectedMessage = "{\"device\":{\"deviceName\":\"MyBleBoxdevicename\"," +
                    "\"product\":\"wLightBox_v3\",\"apiLevel\":\"20200518\"}}";
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
            var controllerUnderTest = new LightBoxClass(httpClient);

            //ACT
            RootDevice result = await controllerUnderTest.GetInfoAsync();

            // ASSERT
            string resultString = JsonSerializer.Serialize(result);
            Assert.Equal(expectedMessage, resultString);
        }
        [Fact]
        public async Task HandlerStub_GetStateIfResultNotAfflictedReturnTrue()
        {
            // ARRANGE
            string expectedMessage = "{\"rgbw\":{\"colorMode\":2,\"effectID\":2,\"desiredColor\":\"ff00300000\"," +
                "\"currentColor\":\"ff00300000\",\"lastOnColor\":\"ff00300000\"," +
                "\"durationsMs\":{\"colorFade\":1000}}}";
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
            var controllerUnderTest = new LightBoxClass(httpClient);

            //ACT
            RootDeviceStateGet result = await controllerUnderTest.GetStateAsync();

            // ASSERT
            string resultString = JsonSerializer.Serialize(result);
            Assert.Equal(expectedMessage, resultString);
        }
    }
}
