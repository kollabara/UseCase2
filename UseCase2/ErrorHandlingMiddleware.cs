using Stripe;

namespace UseCase2;

public class ErrorHandlingMiddleware
{
  private readonly RequestDelegate _requestDelegate;

  public ErrorHandlingMiddleware(RequestDelegate requestDelegate)
  {
    _requestDelegate = requestDelegate;
  }

  public async Task InvokeAsync(HttpContext httpContext)
  {
    try
    {
      await _requestDelegate.Invoke(httpContext);
    }
    catch (StripeException ex)
    {
      Console.WriteLine(ex.Message);
      httpContext.Response.StatusCode = (int)ex.HttpStatusCode;
      await httpContext.Response.WriteAsync(ex.Message);
    }
  }
}