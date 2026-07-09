using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/usuarios")]
    
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IConfiguration configuration;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public UsuariosController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, IConfiguration configuration
            , IServicioUsuarios servicioUsuarios, ApplicationDbContext context, IMapper mapper)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.servicioUsuarios = servicioUsuarios;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet(Name = "ObtenerUsuarioV1")]
        [Authorize(Policy = "esAdmin")]
        public async Task<IEnumerable<UsuarioDTO>> Get()
        {
            var usuarios = context.Users.ToList();
            var usuariosDTO = mapper.Map<IEnumerable<UsuarioDTO>>(usuarios);
            return usuariosDTO;
        }


        [HttpPost("registro",Name = "RegistroUsuarioV1")] // api/usuarios/registro
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var usuario = new Usuario
            {
                UserName = credencialesUsuarioDTO.Email,
                Email = credencialesUsuarioDTO.Email
            };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuarioDTO.password!);

            if (resultado.Succeeded)
            {
                var respuestaAutenticacion = await contruirToken(credencialesUsuarioDTO);
                return respuestaAutenticacion;
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return ValidationProblem();
            }
        }

        private async Task<RespuestaAutenticacionDTO> contruirToken(CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuarioDTO.Email),
                new Claim("lo que yo quiera", "cualquier valor")
            };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);
            var claimsDB = await userManager.GetClaimsAsync(usuario!);

            claims.AddRange(claimsDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]!));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var tokenDeSeguridad = new JwtSecurityToken(issuer: null, audience: null,
                claims: claims, expires: expiracion, signingCredentials: credenciales);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion
            };
        }

        [HttpPost("login",Name = "LoginUsuarioV1")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var usuario = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);

            if (usuario is null)
            {
                return RetornarLoginIncorrecto();
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario, credencialesUsuarioDTO.password!, lockoutOnFailure: false);

            if (resultado.Succeeded)
                return await contruirToken(credencialesUsuarioDTO);
            else
                return RetornarLoginIncorrecto();


        }

        [HttpPost("renovar-token",Name = "RenovarTokenV1")]
        [Authorize]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> RenovarToken()
        {
            var usuario = await servicioUsuarios.ObtenerUsuario();
            if (usuario is null)
                return NotFound();

            var credencialesUsuarioDTO = new CredencialesUsuarioDTO { Email = usuario.Email! };

            var respuestaAutenticacion = await contruirToken(credencialesUsuarioDTO);

            return respuestaAutenticacion;
        }

        [HttpPut(Name = "ActualizarUsuarioV1")]
        [Authorize]
        public async Task<ActionResult> Put(ActualizarUsuarioDTO actualizarUsuarioDTO)
        {
            var usuario = await servicioUsuarios.ObtenerUsuario();
            if (usuario is null)
                return NotFound();

            usuario.FechaNacimiento = actualizarUsuarioDTO.FechaNacimiento;
            await userManager.UpdateAsync(usuario);

            return NoContent();
        }

        [HttpPost("hacer-admin",Name = "ConcederAdminUsuarioV1")]
        [Authorize(Policy = "esAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);
            if (usuario is null)
                return NotFound();

            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "true"));
            return NoContent();
        }

        [HttpPost("remover-admin",Name ="RevocarAdminUsuarioV1")]
        [Authorize(Policy = "esAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);
            if (usuario is null)
                return NotFound();

            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "true"));
            return NoContent();
        }


        private ActionResult RetornarLoginIncorrecto()
        {
            ModelState.AddModelError(string.Empty, "Login incorrecto");
            return ValidationProblem();
        }
   

    }


}
