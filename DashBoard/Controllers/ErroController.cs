using DashBoard.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DashBoard.Controllers
{
    public class ErroController : Controller
    {
        [Route("Erro/HandleError/{code:int}")]
        public IActionResult HandleError(int code)
        {
            var mensagem = "";
            if (code == 401) return RedirectToAction("Index", "Login");
            if (code == 403) mensagem = "Você não tem permissão para acessar esta página.";
            if (code == 404) mensagem = "Oops. A página que está procurando não existe ou não está disponível!";
            else if (code == 500) mensagem = "Oops. Ocorreram problemas com o servidor! Entre em contato com um administrador para resolver o problema.";
            else mensagem = "Oops. Um erro inesperado ocorreu! Entre em contato com um administrador para resolver o problema.";

            ViewData["CodeError"] = code;
            ViewData["MessageError"] = mensagem;
            return View("~/Views/Shared/HttpError.cshtml");
        }

        public IActionResult PermissaoError(string message)
        {
            return View("PermissaoError", message);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
