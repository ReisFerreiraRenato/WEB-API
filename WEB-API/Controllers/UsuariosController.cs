using Microsoft.AspNetCore.Mvc;
using WEB_API.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WEB_API.Services;
using WEB_API.Utils; 

namespace WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ProdutosUsuariosContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogErroService _logErroService;

        public UsuariosController(ProdutosUsuariosContext context, IConfiguration configuration, ILogErroService logErroService)
        {
            _context = context;
            _configuration = configuration;
            _logErroService = logErroService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha))
                {
                    return Unauthorized(new { mensagem = Constantes.ErroLoginCredenciaisInvalidas, login = false });
                }

                var token = GenerateJwtToken(usuario);
                return Ok(new { mensagem = Constantes.LoginRealizadoSucesso, login = true, token = token });
            }
            catch (Exception ex)
            {
                _logErroService.LogErroAsync(ex, Request.Path, Request.Method, "", nameof(UsuariosController));
                return StatusCode(500, new { mensagem = Constantes.ErroLogin, login = false });
            }
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new(ClaimTypes.Name, usuario.Email ?? Constantes.EmailDesconhecido)
                ]),
                Expires = DateTime.UtcNow.AddHours(24),
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
    }
}