using Microsoft.AspNetCore.Mvc;
using WEB_API.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ProdutosUsuariosContext _context;
        private readonly IConfiguration _configuration;

        public UsuariosController(ProdutosUsuariosContext context, IConfiguration configuration) // Modifique o construtor
        {
            _context = context;
            _configuration = configuration; 
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginRequest request)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha))
            {
                return Unauthorized("Credenciais inválidas.");
            }

            var token = GenerateJwtToken(usuario);
            return Ok(token);
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new(ClaimTypes.Name, usuario.Email ?? "email_desconhecido")
                ]),
                Expires = DateTime.UtcNow.AddHours(24), // Token expira em 24 horas
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public class LoginRequest
        {
            public string? Email { get; set; }
            public string? Senha { get; set; }
        }

        public class TokenResponse
        {
            public string? Token { get; set; }
        }
    }
}