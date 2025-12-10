using DashBoard.Models;
using DashBoard.Models.Enums;
using DashBoard.Repositories;
using DashBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;

namespace DashBoard.Controllers
{
	[Authorize]
	public class TabelaController : GeralController
    {
        private readonly ICadastroRepository _cadastroRepository;
        private ClienteFiltroModelo _sessionFiltro
        {
            get => HttpContext.Session.Get<ClienteFiltroModelo>("sessionFiltroClientePaginado")!;
            set => HttpContext.Session.Set("sessionFiltroClientePaginado", value);
        }
        public TabelaController(IConfiguration configuration,
            ICadastroRepository cadastroRepository,
            IPermissoesRepository permissoesRepository,
            IEstadoRepository estadoRepository,
            ICidadeRepository cidadeRepository) : base(configuration, permissoesRepository, estadoRepository, cidadeRepository)
        {
            _cadastroRepository = cadastroRepository;
        }

        [HttpPost]
        [HttpGet]
        [PermissionAuth(Atividade.Tabelas, Acoes.Visualizar)]
        public async Task<IActionResult> Index()
        {
            var filtro = _sessionFiltro ?? new();
            TempData["FiltroAtual"] = filtro;
            return View(await _cadastroRepository.GetDataTable(filtro));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTabelas(ClienteFiltroModelo clienteFiltro)
        {
            bool sucesso = true;
            string html = string.Empty;
            string htmlFiltro = string.Empty;
            string erro = string.Empty;
            try
            {
                clienteFiltro.Pagina = 0;
                clienteFiltro.QuantidadePorPagina = 10;
                _sessionFiltro = clienteFiltro;
                var clientesDB = await _cadastroRepository.GetDataTable(clienteFiltro);
                html = await this.RenderViewAsync("_PartialListaCliente", clientesDB, true);
                htmlFiltro = await this.RenderViewAsync("_PartialPaginador", new PaginadoInfo()
                {
                    TotalDeRegistros = clientesDB?.FirstOrDefault()?.QuantidadeDeRegistros ?? 0,
                    PaginaAtual = clienteFiltro.Pagina,
                    QuantidadePorPagina = clienteFiltro.QuantidadePorPagina
                }, true);
            }
            catch (Exception ex)
            {
                sucesso = false;
                erro = ex.Message;
            }
            return Json(new
            {
                sucesso = sucesso,
                html = html,
                htmlFiltro = htmlFiltro,
                erro = erro
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuscarPaginas([FromBody] PaginadoInfo paginado, CancellationToken ct)
        {
            bool sucesso = true;
            string html = string.Empty;
            string htmlFiltro = string.Empty;
            string erro = string.Empty;
            try
            {
                var filtro = _sessionFiltro != null ? _sessionFiltro : new();
                filtro.Pagina = paginado.PaginaAtual;
                filtro.QuantidadePorPagina = paginado.QuantidadePorPagina;
                var clientesDB = await _cadastroRepository.GetDataTable(filtro, ct);
                html = await this.RenderViewAsync("_PartialListaCliente", clientesDB, true);
                paginado.TotalDeRegistros = clientesDB?.FirstOrDefault()?.QuantidadeDeRegistros ?? 0;
                htmlFiltro = await this.RenderViewAsync("_PartialPaginador", paginado, true);
            }
            catch (Exception ex)
            {
                sucesso = false;
                erro = ex.Message;
            }
            return Json(new
            {
                sucesso = sucesso,
                html = html,
                htmlFiltro = htmlFiltro,
                erro = erro
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Tabelas, Acoes.Excluir)]
        public async Task<IActionResult> DeletaCliente([FromBody] string[] array, CancellationToken ct)
        {
            bool sucesso = true;
            string html = string.Empty;
            string erro = string.Empty;
            string response = string.Empty;
            try
            {
                if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
                if (array.Length <= 0) array = await _cadastroRepository.GetEmailsPorFiltro(_sessionFiltro, ct);
                response = await _cadastroRepository.DeletaCliente(array, ct);
                _sessionFiltro.Pagina = 0;
                _sessionFiltro.QuantidadePorPagina = 10;
                var clientesDB = await _cadastroRepository.GetDataTable(_sessionFiltro);
                html = await this.RenderViewAsync("_PartialListaCliente", clientesDB, true);
            }
            catch (Exception ex)
            {
                sucesso = false;
                erro = ex.Message;
            }
            return Json(new
            {
                success = sucesso,
                html = html,
                message = response,
                error = erro,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Tabelas, Acoes.Editar)]
        public async Task<IActionResult> RedirectToEmail([FromBody] List<string> listaEmails)
        {
            bool sucesso = true;
            try
            {
                if (listaEmails?.Any() == true)
                    TempData["EmailsSelecionadosRedirect"] = listaEmails;
            }
            catch
            {
                sucesso = false;
            }
            return Json(new { success = sucesso, redirect = Url.Action("Index", "Email") });
        }

        [HttpPost]
        public IActionResult GetCidades([FromBody] int estadoId)
        {
            List<SelectListItem> cidades = new List<SelectListItem>();
            if (estadoId > 0 && CacheSystem.Cidades?.Any(c => c.EstadoId == estadoId) == true)
                cidades.AddRange(CacheSystem.Cidades
                    .Where(c => c.EstadoId == estadoId)
                    .OrderBy(c => c.CidadeNome)
                    .Select(x => new SelectListItem() { Text = x.CidadeNome, Value = x.Id.ToString() })
                    .ToList());
            return Json(cidades);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadTabela([FromBody] string[] array, CancellationToken ct)
        {
            try
            {
                if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
                if (array.Length <= 0) array = await _cadastroRepository.GetEmailsPorFiltro(_sessionFiltro, ct);
                var values = await _cadastroRepository.ExportDataTable(array);

                var stream = new MemoryStream();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                if (ct.IsCancellationRequested)
                {
                    HttpContext.Session.SetString("responseServer", "Download cancelado!");
                    ct.ThrowIfCancellationRequested();
                }
                using (var package = new ExcelPackage(stream))
                {
                    var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                    workSheet.Cells.LoadFromCollection(values, true);

                    await package.SaveAsync(ct);
                }
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(499);
            }
        }
    }
}
