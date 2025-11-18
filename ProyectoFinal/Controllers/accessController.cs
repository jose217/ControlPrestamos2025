using ProyectoFinal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ProyectoFinal.Controllers
{
    public class accessController : Controller
    {
        // GET: access
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string Correo, string Contrasenia)
        {
            // Usa 'using' para asegurar que la conexión a la base de datos se cierre correctamente
            using (BibliotecaEntities2 db = new BibliotecaEntities2())
            {
                // 1. Busca al usuario usando LINQ to Entities (fácil y sin SQL)
                var usuario = db.usuarios.FirstOrDefault(u =>
                    u.correo == Correo &&
                    u.contrasenia == Contrasenia); // ¡IMPORTANTE! Ver nota de seguridad abajo.

                // 2. Verifica si el usuario existe
                if (usuario != null)
                {
                    // Crea el "ticket" de autenticación y la cookie
                    FormsAuthentication.SetAuthCookie(usuario.correo, false);

                    // Redirige a la página principal de la aplicación
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Si las credenciales son incorrectas
                    ViewBag.Error = "Credenciales inválidas.";
                    return View(); // Vuelve a mostrar el formulario de login con el error
                }
            }
        }
    }
}