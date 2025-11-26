using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProyectoFinal.Models;

namespace ProyectoFinal.Controllers
{
    public class librosController : Controller
    {
        private BibliotecaEntities2 db = new BibliotecaEntities2();

        // GET: libros
        // GET: libros con búsqueda mejorada
        public ActionResult Index(string searchString)
        {
            var libros = from l in db.libros
                         select l;

            if (!String.IsNullOrEmpty(searchString))
            {
                libros = libros.Where(l => l.titulo.Contains(searchString)
                                        || l.autor.Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;
            return View(libros.ToList());
        }

        // GET: libros/Details/5
        public ActionResult Details(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            libros libros = db.libros.Find(id);
            if (libros == null)
            {
                return HttpNotFound();
            }
            return View(libros);
        }

        // GET: libros/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: libros/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,titulo,genero,estado,cantidad,autor,fechaPublicacion,imagen")] libros libros, HttpPostedFileBase imagenFile)
        {
            if (ModelState.IsValid)
            {
                // Procesar la imagen si se subió
                if (imagenFile != null && imagenFile.ContentLength > 0)
                {
                    // Validar el tipo de archivo
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(imagenFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("imagen", "Solo se permiten archivos de imagen (JPG, PNG, GIF)");
                        return View(libros);
                    }

                    // Validar el tamaño (máximo 5MB)
                    if (imagenFile.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("imagen", "La imagen no puede superar los 5MB");
                        return View(libros);
                    }

                    // Generar nombre único para evitar sobrescribir archivos
                    string fileName = Guid.NewGuid().ToString() + extension;
                    string path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);

                    // Crear el directorio si no existe
                    string directory = Path.GetDirectoryName(path);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Guardar el archivo
                    imagenFile.SaveAs(path);

                    // Guardar solo el nombre del archivo en la base de datos
                    libros.imagen = fileName;
                }
                else
                {
                    // Imagen por defecto si no se sube ninguna
                    libros.imagen = "default-book.jpg";
                }

                db.libros.Add(libros);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(libros);
        }

        // GET: libros/Edit/5
        public ActionResult Edit(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            libros libros = db.libros.Find(id);
            if (libros == null)
            {
                return HttpNotFound();
            }
            return View(libros);
        }

        // POST: libros/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,titulo,genero,estado,cantidad,autor,fechaPublicacion,imagen")] libros libros, HttpPostedFileBase imagenFile)
        {
            if (ModelState.IsValid)
            {
                // Si se sube una nueva imagen
                if (imagenFile != null && imagenFile.ContentLength > 0)
                {
                    // Validaciones
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(imagenFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("imagen", "Solo se permiten archivos de imagen (JPG, PNG, GIF)");
                        return View(libros);
                    }

                    if (imagenFile.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("imagen", "La imagen no puede superar los 5MB");
                        return View(libros);
                    }

                    // Obtener el libro actual de la BD para eliminar la imagen anterior
                    var libroActual = db.libros.AsNoTracking().FirstOrDefault(l => l.ID == libros.ID);
                    if (libroActual != null && !string.IsNullOrEmpty(libroActual.imagen) && libroActual.imagen != "default-book.jpg")
                    {
                        string oldImagePath = Path.Combine(Server.MapPath("~/Content/images/"), libroActual.imagen);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Guardar nueva imagen
                    string fileName = Guid.NewGuid().ToString() + extension;
                    string path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);

                    string directory = Path.GetDirectoryName(path);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    imagenFile.SaveAs(path);
                    libros.imagen = fileName;
                }
                else
                {
                    // Si no se sube imagen, mantener la que ya tenía
                    var libroActual = db.libros.AsNoTracking().FirstOrDefault(l => l.ID == libros.ID);
                    if (libroActual != null)
                    {
                        libros.imagen = libroActual.imagen;
                    }
                }

                db.Entry(libros).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(libros);
        }

        // GET: libros/Delete/5
        public ActionResult Delete(decimal id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            libros libros = db.libros.Find(id);
            if (libros == null)
            {
                return HttpNotFound();
            }
            return View(libros);
        }

        // POST: libros/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(decimal id)
        {
            libros libros = db.libros.Find(id);

            // Eliminar la imagen del servidor si existe
            if (!string.IsNullOrEmpty(libros.imagen) && libros.imagen != "default-book.jpg")
            {
                string imagePath = Path.Combine(Server.MapPath("~/Content/images/"), libros.imagen);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            db.libros.Remove(libros);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}