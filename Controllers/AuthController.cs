using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoAPI.Data;
using TodoAPI.Models;

namespace TodoAPI.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly DataContext _context;

    public AuthController(DataContext context)
    {
      _context = context;
    }

    private async Task<ActionResult<User>> GetUserByUsername(string username)
    {
      var user = await _context.Users.FindAsync(username);

      if (user == null)
      {
        return NotFound();
      }

      return user;
    }

    [Route("register")]
    [HttpPost]
    public async Task<ActionResult<User>> Register(User user)
    {
      string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
      user.Password = passwordHash;

      _context.Users.Add(user);
      await _context.SaveChangesAsync();

      return await _context.Users.FindAsync();
    }

    // [Route("login")]
    // [HttpPost]
    // public async Task<ActionResult<Product>> Login(User user)
    // {
    //   string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
    //   user.Password = passwordHash;

    //   _context.Users.Add(user);
    //   await _context.SaveChangesAsync();

    //   return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    // }
  }
}
