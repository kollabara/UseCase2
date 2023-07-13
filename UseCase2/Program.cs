using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;using Stripe;
using UseCase2;
using UseCase2.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped(_ =>
  new BalanceService(new StripeClient(builder.Configuration.GetSection(StripeOptions.Stripe)[StripeOptions.Token])));

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapGet("/balance",
  async ([FromServices] BalanceService balanceService) => {
    var response = await balanceService.GetAsync();

    return Results.Ok(response);
  });

app.Run();