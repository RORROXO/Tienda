using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tienda.src.Dtos;
using tienda.src.Exceptions;

namespace tienda.src.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserDto dto)
        {
            // Si la validación falla, NO llega aquí; ASP.NET ya devolvió el 400 con el formato definido.
            // Simula éxito:
            return Created("", new { success = true, message = "Cuenta creada. Verifique su correo." });
        }


        [HttpGet("test-notfound")]
        public IActionResult TestNotFound()
        {
            throw new NotFoundException("Usuario no encontrado");
        }
    }


}