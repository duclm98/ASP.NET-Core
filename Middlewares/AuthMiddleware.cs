using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TodoAPI.Data;

namespace TodoAPI.Middleware
{
  public class AuthMiddleware
  {
    private readonly RequestDelegate _next;
    private IConfiguration _config;

    public AuthMiddleware(RequestDelegate next, IConfiguration config)
    {
      _next = next;
      _config = config;
    }

    public async Task Auth(HttpContext context, DataContext dataContext)
    {
      var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

      if (token != null)
      {

      }

      await _next(context);
    }
  }
}