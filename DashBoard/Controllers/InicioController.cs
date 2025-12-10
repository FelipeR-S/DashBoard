using DashBoard.Models.Enums;
using DashBoard.Repositories;
using DashBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DashBoard.Controllers
{
    [Authorize]
    public class InicioController : GeralController
    {
        private readonly ICadastroRepository _cadastroRepository;
        public InicioController(IConfiguration configuration,
            ICadastroRepository cadastroRepository,
            IPermissoesRepository permissoesRepository,
            IEstadoRepository estadoRepository,
            ICidadeRepository cidadeRepository) : base(configuration, permissoesRepository, estadoRepository, cidadeRepository)
        {
            _cadastroRepository = cadastroRepository;
        }

        [HttpGet]
        [PermissionAuth(Atividade.Inicio, Acoes.Visualizar)]
        public async Task<IActionResult> Index()
        {
            await CacheSystem.AtualizaStatsInicio(_cadastroRepository);
            ViewData["META"] = User.FindFirst("Aviso")?.Value;
            return View(CacheSystem.StatsGerais);
        }

        [HttpPost]
        [PermissionAuth(Atividade.Inicio, Acoes.Visualizar)]
        public ActionResult AtualizaPeriodo(int meses)
        {
            CacheSystem.StatsGerais!.StatsPeriodo.FiltroDe = DateTime.Now.AddMonths(meses * -1);
            CacheSystem.StatsGerais!.StatsPeriodo.AtualizaPeriodo(_cadastroRepository);

            return Json(CacheSystem.StatsGerais!.StatsPeriodo.StatsPeriodoMesAno);
        }

        [HttpGet]
        public ActionResult GetEstados()
        {
            return Json(new
            {
                estados = CacheSystem.StatsGerais!.StatsEstadosCidades.CadastrosPorEstado.Select(x => new { estadoUF = x.EstadoUF, estadoNome = x.EstadoNome, quantidade = x.Quantidade })
            });
        }
    }
}
