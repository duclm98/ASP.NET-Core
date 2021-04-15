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
    private readonly IConfiguration _config;

    public AuthController(DataContext context, IConfiguration config)
    {
      _context = context;
      _config = config;
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

      authInfo.AccessToken = AuthMethod.GenenateJSONWebToken(u, _config["Jwt:AccessTokenSecret"], Convert.ToDouble(_config["Jwt:AccessTokenExpires"]), _config["Jwt:Issuer"], _config["Jwt:Audience"]);

      var createNewRefreshToken = false;

      if (u.RefreshToken == null) {
        createNewRefreshToken = true;
      } else {
        var validateRefreshToken = AuthMethod.ValidateJSONWebToken(u.RefreshToken, _config["Jwt:RefreshTokenSecret"], _config["Jwt:Issuer"], _config["Jwt:Audience"]);
        if( validateRefreshToken == null){
          createNewRefreshToken = true;
        }
      }

      if(createNewRefreshToken == true) {
        var refreshToken = AuthMethod.GenenateJSONWebToken(u, _config["Jwt:RefreshTokenSecret"], Convert.ToDouble(_config["Jwt:RefreshTokenExpires"]), _config["Jwt:Issuer"], _config["Jwt:Audience"]);
        u.RefreshToken = refreshToken;

        _context.Entry(u).State = EntityState.Modified;

        try
        {
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          throw;
        }
      }

      authInfo.RefreshToken = u.RefreshToken;
      authInfo.User = u;

      return Ok(authInfo);
    }
  }
}
