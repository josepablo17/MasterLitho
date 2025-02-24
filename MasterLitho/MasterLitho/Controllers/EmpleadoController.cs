using MasterLitho.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MasterLitho.Controllers
{
    public class EmpleadoController : Controller
    {
        [HttpGet]
        public ActionResult ConsultarEmpleados()
        {
            using (var context = new LithoManagerEntities())
            {
                var empleadosActivos = (from empleado in context.EMPLEADO
                                        join departamento in context.DEPARTAMENTO
                                            on empleado.ID_DEPARTAMENTO equals departamento.ID_DEPARTAMENTO
                                        join usuario in context.USUARIO
                                            on empleado.ID_USUARIO equals usuario.ID_USUARIO
                                        where empleado.ES_EMPLEADO == true && empleado.ESTADO == true
                                        select new tEmpleado
                                        {
                                            ID_EMPLEADO = empleado.ID_EMPLEADO,
                                            NOMBRE_USUARIO = usuario.NOMBRE_USUARIO,
                                            DIAS_VACACIONES_DISPONIBLES = empleado.DIAS_VACACIONES_DISPONIBLES,
                                            FECHA_CONTRATACION = empleado.FECHA_CONTRATACION,
                                            NOMBRE_DEPARTAMENTO = departamento.NOMBRE_DEPARTAMENTO
                                        }).ToList();

                var empleadosInactivos = (from empleado in context.EMPLEADO
                                          join departamento in context.DEPARTAMENTO
                                              on empleado.ID_DEPARTAMENTO equals departamento.ID_DEPARTAMENTO
                                          join usuario in context.USUARIO
                                              on empleado.ID_USUARIO equals usuario.ID_USUARIO
                                          where empleado.ES_EMPLEADO == true && empleado.ESTADO == false
                                          select new tEmpleado
                                          {
                                              ID_EMPLEADO = empleado.ID_EMPLEADO,
                                              NOMBRE_USUARIO = usuario.NOMBRE_USUARIO,
                                              DIAS_VACACIONES_DISPONIBLES = empleado.DIAS_VACACIONES_DISPONIBLES,
                                              FECHA_CONTRATACION = empleado.FECHA_CONTRATACION,
                                              NOMBRE_DEPARTAMENTO = departamento.NOMBRE_DEPARTAMENTO
                                          }).ToList();

                var viewModel = new tEmpleado
                {
                    EmpleadosActivos = empleadosActivos,
                    EmpleadosInactivos = empleadosInactivos
                };

                return View(viewModel);
            }
        }


        [HttpGet]
        public ActionResult RegistrarEmpleado(int idUsuario)
        {
            using (var context = new LithoManagerEntities())
            {
                var usuario = context.USUARIO.Find(idUsuario);
                if (usuario == null)
                {
                    return RedirectToAction("GestionUsuarios", "Usuarios");
                }

                var model = new tEmpleado
                {
                    ID_EMPLEADO = usuario.ID_USUARIO
                };

                CargarDepartamentos();
                return View(model);
            }
        }

        [HttpPost]
        public ActionResult RegistrarEmpleado(int idUsuario, tEmpleado empleado)
        {
            using (var context = new LithoManagerEntities())
            {
                var usuario = context.USUARIO.Find(idUsuario);
                if (usuario == null || usuario.ID_USUARIO == 0)
                {
                    Console.WriteLine("El usuario no existe o su ID no es válido.");
                    return RedirectToAction("GestionUsuarios", "Usuarios");
                }

                Console.WriteLine($"ID_USUARIO obtenido: {usuario.ID_USUARIO}");

                var tabla = new EMPLEADO
                {
                    ID_EMPLEADO = usuario.ID_USUARIO,
                    ID_USUARIO = usuario.ID_USUARIO,
                    DIAS_VACACIONES_DISPONIBLES = 14,
                    FECHA_CONTRATACION = empleado.FECHA_CONTRATACION,
                    ID_DEPARTAMENTO = empleado.ID_DEPARTAMENTO,
                    ES_EMPLEADO = true,
                    ESTADO = true,
                    Salario = empleado.SALARIO
                };

                Console.WriteLine($"ID_EMPLEADO a insertar: {tabla.ID_EMPLEADO}");

                context.EMPLEADO.Add(tabla);
                context.SaveChanges();

                return RedirectToAction("GestionUsuarios", "Usuarios");
            }
        }

        
        private void CargarDepartamentos()
        {
            using (var context = new LithoManagerEntities())
            {
                var datos = context.DEPARTAMENTO.ToList();

                var departamento = new List<SelectListItem>();

                departamento.Add(new SelectListItem
                {
                    Text = "Seleccione",
                    Value = string.Empty
                });

                foreach (var item in datos)
                {
                    departamento.Add(new SelectListItem
                    {
                        Text = item.NOMBRE_DEPARTAMENTO,
                        Value = item.ID_DEPARTAMENTO.ToString()
                    });
                }

                ViewBag.Departamentos = departamento;
            }
        }

    }
}