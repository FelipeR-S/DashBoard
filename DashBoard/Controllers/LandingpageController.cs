using DashBoard.Models;
using DashBoard.Models.Enums;
using DashBoard.Repositories;
using DashBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Runtime.Serialization;

namespace DashBoard.Controllers
{
    public class LandingpageController : Controller
    {
        private readonly ICadastroRepository _cadastroRepository;
        private readonly INewsLetterRepository _newsLetterRepository;
        private readonly ICidadeRepository _cidadeRepository;
        private readonly IEstadoRepository _estadoRepository;

        public LandingpageController(ICadastroRepository cadastroRepository,
            INewsLetterRepository newsLetterRepository,
            IEstadoRepository estadoRepository, ICidadeRepository cidadeRepository)
        {
            _cadastroRepository = cadastroRepository;
            _newsLetterRepository = newsLetterRepository;
            _estadoRepository = estadoRepository;
            _cidadeRepository = cidadeRepository;
        }

        public IActionResult Index()
        {
            CarregaViewBagListas();
            return View();
        }

        private void CarregaViewBagListas()
        {
            ViewBag.Renda = CacheSystem.RendaLandingpage;
            ViewBag.Genero = CacheSystem.GeneroLandingpage;
            ViewBag.EstadoCivil = CacheSystem.EstadoCivilLandingpage;
            ViewBag.Filhos = CacheSystem.FilhosLandingpage;
            ViewBag.Estados = CacheSystem.GeraSelectEstado();
        }

        [HttpPost]
        public ActionResult AtualizaCidades(int estadoId)
        {
            bool sucesso = true;
            var listaCidades = new List<Cidade>();
            try
            {
                listaCidades = CacheSystem.Cidades!.Where(x => x.EstadoId == estadoId).ToList();
            }
            catch
            {
                listaCidades = new List<Cidade>();
                sucesso = false;
            }
            return Json(new
            {
                sucesso = sucesso,
                cidades = listaCidades
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastraCliente(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                TempData["Resposta"] = await _cadastroRepository.CadastraCliente(cliente);
                return RedirectToAction("Index");
            }
            TempData["Resposta"] = "Informações incorretas no cadastro.\n Favor verificar!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CadastraNewsLetter(NewsLetter emailCadastro)
        {
            if (ModelState.IsValid)
            {
                TempData["Resposta"] = await _newsLetterRepository.CadastraEmail(emailCadastro);
                return RedirectToAction("Index");
            }
            TempData["Resposta"] = "E-mail incorreto.\n Favor verificar!";
            return RedirectToAction("Index");
        }
    }
}