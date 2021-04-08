using Microsoft.EntityFrameworkCore;

namespace TodoAPI.Models
{
  [Keyless]
  public class Auth
  {
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public User User { get; set; }
  }
}