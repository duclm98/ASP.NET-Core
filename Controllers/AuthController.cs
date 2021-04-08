using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TodoAPI.Data;
using TodoAPI.Methods;
using TodoAPI.Models;

namespace TodoAPI.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly DataContext _context;
    private IConfiguration _config;
    private readonly IAuthMethod _authMethod;

    public AuthController(DataContext context, IConfiguration config, AuthMethod authMethod)
    {
      _context = context;
      _config = config;
      _authMethod = authMethod;
    }

    [Route("register")]
    [HttpPost]
    public async Task<ActionResult<User>> Register(User user)
    {
      var u = await _context.Users.SingleOrDefaultAsync(item => item.Username == user.Username);
      if (u != null)
      {
        return Conflict("Username is already existed!");
      }

      string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
      user.Password = passwordHash;

      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      return await _context.Users.FindAsync(user.Id);
    }

    [Route("login")]
    [HttpPost]
    public async Task<ActionResult<Auth>> Login(User user)
    {
      var authInfo = new Auth();

      var u = await _context.Users.SingleOrDefaultAsync(item => item.Username == user.Username);
      if (u == null)
      {
        return NotFound("Username is not existed!");
      }

      bool verified = BCrypt.Net.BCrypt.Verify(user.Password, u.Password);
      if (!verified) {
        return BadRequest("Incorrect Username or Password!");
      }

      authInfo.AccessToken = _authMethod.GenenateJSONWebToken(u, _config["Jwt:AccessTokenSecret"], Convert.ToDouble(_config["Jwt:AccessTokenExpires"]));

      return authInfo;
    }
  }
}
