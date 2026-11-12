using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Infrastructure.Configuration;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests;

[TestFixture]
public class CollectionPeriodApiClientTests
{
    [Test]
    public async Task GetOpenCollectionPeriods_returns_null_when_api_returns_no_content()
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NoContent)));
        var configuration = new Mock<IEarningsBridgeConfiguration>();
        configuration.SetupGet(x => x.CollectionPeriodApiKey).Returns("test-key");
        var logger = new Mock<ILogger<CollectionPeriodApiClient>>();

        var client = new CollectionPeriodApiClient(httpClient, configuration.Object, logger.Object);

        var result = await client.GetOpenCollectionPeriods("2526");

        result.Should().BeNull();
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StubHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
