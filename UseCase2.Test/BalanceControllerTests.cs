using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Stripe;

namespace UseCase2.Test;

public class BalanceControllerTests
{
  [Fact]
  public async Task GetBalanceList_ReturnsOK()
  {
    // Arrange
    var stripeClient = new Mock<IStripeClient>();

    stripeClient.Setup(x => x.RequestAsync<Balance>(
      It.IsAny<HttpMethod>(),
      It.IsAny<string>(),
      It.IsAny<BaseOptions>(),
      It.IsAny<RequestOptions>(),
      default)).ReturnsAsync(new Balance()
    {
      Object = "balance"
    });

    await using var app = new BalanceEndpointsTestsApp(x =>
    {
      var balanceService = new BalanceService(stripeClient.Object);

      x.AddSingleton(balanceService);
    });

    // Act
    var responseMessage = await app.CreateClient().GetAsync("/balance");

    // Assert
    Assert.True(responseMessage.IsSuccessStatusCode);

    var response = await responseMessage.Content.ReadFromJsonAsync<Balance>();
    Assert.NotNull(response);
    Assert.Equal("balance", response!.Object);
  }

  [Fact]
  public async Task GetBalanceTransactionsList_ReturnsOK()
  {
    // Arrange
    var stripeClient = new Mock<IStripeClient>();

    stripeClient.Setup(x => x.RequestAsync<StripeList<BalanceTransaction>>(
      It.IsAny<HttpMethod>(),
      It.IsAny<string>(),
      It.IsAny<BaseOptions>(),
      It.IsAny<RequestOptions>(),
      default)).ReturnsAsync(new StripeList<BalanceTransaction>()
    {
      Object = "balanceTransaction",
      Data = new List<BalanceTransaction>
      {
        new()
        {
          Object = "object1"
        },
        new()
        {
          Object = "object2"
        }
      }
    });

    await using var app = new BalanceEndpointsTestsApp(x =>
    {
      var balanceService = new BalanceTransactionService(stripeClient.Object);

      x.AddSingleton(balanceService);
    });

    // Act
    var responseMessage = await app.CreateClient().GetAsync("/balance/transactions");

    // Assert
    Assert.True(responseMessage.IsSuccessStatusCode);

    var rawContent = await responseMessage.Content.ReadAsStringAsync();
    Assert.NotNull(rawContent);
    Assert.Contains("object1", rawContent);
  }
}

internal class BalanceEndpointsTestsApp : WebApplicationFactory<Program>
{
  private readonly Action<IServiceCollection> _services;

  public BalanceEndpointsTestsApp(Action<IServiceCollection> services)
  {
    _services = services;
  }

  protected override IHost CreateHost(IHostBuilder builder)
  {
    builder.ConfigureServices(_services);

    return base.CreateHost(builder);
  }
}