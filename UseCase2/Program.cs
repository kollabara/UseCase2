using Microsoft.AspNetCore.Mvc;
using Stripe;
using UseCase2;
using UseCase2.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IStripeClient>(_ =>
  new StripeClient(builder.Configuration.GetSection(StripeOptions.Stripe)[StripeOptions.Token]));

builder.Services.AddScoped(serviceProvider =>
  new BalanceService(serviceProvider.GetRequiredService<IStripeClient>()));

builder.Services.AddScoped(serviceProvider =>
  new BalanceTransactionService(serviceProvider.GetRequiredService<IStripeClient>()));

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapGet("/balance",
  async ([FromServices] BalanceService balanceService) =>
  {
    var response = await balanceService.GetAsync();

    return Results.Ok(response);
  });

app.MapGet("/balance/transactions",
  async ([FromServices] BalanceTransactionService balanceTransactionService, string? endingBefore, string? startingAfter) =>
  {
    var response = await balanceTransactionService.ListAsync(new BalanceTransactionListOptions
    {
      EndingBefore = endingBefore,
      StartingAfter = startingAfter
    });

    return Results.Ok(response);
  });

app.Run();