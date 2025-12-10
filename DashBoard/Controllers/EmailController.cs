using Microsoft.AspNetCore.Mvc;
using DashBoard.Repositories;
using DashBoard.Services;
using DashBoard.Models;
using DashBoard.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
namespace DashBoard.Controllers
{
    [Authorize]
    public class EmailController : GeralController
    {
        private readonly ICadastroRepository _cadastroRepository;
        private readonly INewsLetterRepository _newsLetterRepository;
        private readonly IUserRepository _userRepository;
        const string _pathTemplates = "./Templates/";
        private List<string> _emails
        {
            get => HttpContext.Session.Get<List<string>>("responseServer")!;
            set => HttpContext.Session.Set("responseServer", value);
        }

        public EmailController(IConfiguration configuration,
            ICadastroRepository cadastroRepository,
            INewsLetterRepository newsLetterRepository,
            IUserRepository userRepository,
            IPermissoesRepository permissoesRepository,
            IEstadoRepository estadoRepository,
            ICidadeRepository cidadeRepository) : base(configuration, permissoesRepository, estadoRepository, cidadeRepository)
        {
            _cadastroRepository = cadastroRepository;
            _newsLetterRepository = newsLetterRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        [PermissionAuth(Atividade.Emails, Acoes.Visualizar)]
        public IActionResult Index()
        {
            var emails = (string[]?)TempData["EmailsSelecionadosRedirect"];
            if (emails?.Any() == true)
                _emails = emails.ToList();
            TempData["EmailsSelecionados"] = _emails;
            ViewData["Templates"] = GetTemplates();
            return View(_emails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Emails, Acoes.Salvar)]
        public async Task<ActionResult> SalvaTemplate(string html, string fileName, CancellationToken ct = default)
        {
            string msg = string.Empty;
            bool success = true;
            Dictionary<string, string> templates = new Dictionary<string, string>();
            try
            {
                if (string.IsNullOrEmpty(html))
                    throw new Exception("Conteúdo do template vazio!");

                if (string.IsNullOrEmpty(fileName))
                {
                    var serverFiles = Directory.GetFiles(_pathTemplates, "*.html", SearchOption.TopDirectoryOnly).ToList();
                    var filesContent = serverFiles.Select(x => NormalizeHtml(System.IO.File.ReadAllText(x))).ToList();
                    var htmlValidar = NormalizeHtml(html);
                    if (filesContent.Any(x => x == htmlValidar))
                        throw new Exception("Já existe um template igual!");
                    fileName = Guid.NewGuid().ToString();
                    await System.IO.File.WriteAllTextAsync(Path.Combine(_pathTemplates, fileName + ".html"), html, ct);
                    msg = "Template salvo com sucesso!";
                }
                else
                {
                    var currentUser = User.FindFirst("Matricula")?.Value;
                    if (fileName.Contains("_"))
                    {
                        var userPart = fileName.Split("_")[1];
                        if (currentUser != userPart)
                            throw new Exception("Você não tem permissão para editar este template!");

                        await System.IO.File.WriteAllTextAsync(Path.Combine(_pathTemplates, fileName + ".html"), html, ct);
                        msg = "Template alterado com sucesso!";
                    }
                    else
                    {
                        fileName = $"{fileName}_{currentUser}";
                        await System.IO.File.WriteAllTextAsync(Path.Combine(_pathTemplates, fileName + ".html"), html, ct);
                        msg = "Template salvo com sucesso!";
                    }
                }
                templates = GetTemplates();
            }
            catch (Exception ex)
            {
                msg = "Erro ao salvar novo template! " + ex.Message;
                success = false;
            }
            return Json(new
            {
                success = success,
                msg = msg,
                templates = templates,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Emails, Acoes.Editar)]
        public async Task<string> PostEmails(RequestEmail request, CancellationToken ct)
        {
            var response = string.Empty;
            try
            {
                if (request is null)
                    throw new Exception("Request invalida!");

                var currentUser = User.FindFirst("Matricula")?.Value;
                int quantidade = 0;
                var fileName = $"{request.TemplateId}.html";
                if (!System.IO.File.Exists(Path.Combine(_pathTemplates, fileName)))
                    throw new Exception("Template não encontrado!");
                request.TemplateHtml = System.IO.File.ReadAllText(Path.Combine(_pathTemplates, fileName));

                switch (request.Tipo)
                {
                    case TipoEmail.LandingPage:
                        var clientes = await _cadastroRepository.GetDataTable(new ClienteFiltroModelo(), ct);
                        if (!(clientes?.Any() == true))
                            throw new Exception("Nenhum e-mail encontrado para o filtro selecionado!");
                        (quantidade, response) = await _cadastroRepository.EnviaEmailMarketing(clientes, request, ct);
                        break;
                    case TipoEmail.Newsletter:
                        var emailsNewsLetter = await _newsLetterRepository.GetAllData();
                        if (!(emailsNewsLetter?.Any() == true))
                            throw new Exception("Nenhum e-mail encontrado para o filtro selecionado!");
                        (quantidade, response) = await _newsLetterRepository.EnviaEmailMarketing(emailsNewsLetter, request, ct);
                        break;
                    case TipoEmail.Selecionados:
                        var listCliente = new List<Cliente>();
                        foreach (var email in _emails)
                        {
                            var cliente = await _cadastroRepository.GetCliente(email);
                            if (cliente is not null) listCliente.Add(cliente);
                        }
                        if (!(listCliente?.Any() == true))
                            throw new Exception("Nenhum e-mail encontrado para o filtro selecionado!");
                        (quantidade, response) = await _cadastroRepository.EnviaEmailMarketing(listCliente, request, ct);
                        break;
                    default:
                        quantidade = 0;
                        response = "Não foi possível enviar e-mails!";
                        break;
                }
                if (quantidade > 0) await _userRepository.UpdateEnvios(currentUser!, quantidade);
                HttpContext.Session.SetString("responseServer", response);
            }
            catch (Exception ex)
            {
                response = $"Não foi possível enviar e-mails! {ex.Message}";
            }
            return response;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Emails, Acoes.Visualizar)]
        public ActionResult LoadSelectedTemplate([FromBody]string templateId)
        {
            string templateHtml = string.Empty;
            string errorMsg = string.Empty;
            bool success = true;
            try
            {
                var fileName = $"{templateId}.html";
                if (!System.IO.File.Exists(Path.Combine(_pathTemplates, fileName)))
                    throw new Exception("Template não encontrado!");

                templateHtml = System.IO.File.ReadAllText(Path.Combine(_pathTemplates, fileName));
            }
            catch (Exception ex)
            {
                success = false;
                errorMsg = ex.Message;
            }
            return Json(new
            {
                success = success,
                html = templateHtml,
                error = errorMsg
            });
        }

        private Dictionary<string, string> GetTemplates()
        {
            var currentUser = User.FindFirst("Matricula")?.Value;
            var templates = new Dictionary<string, string>();
            var serverFiles = Directory.GetFiles(_pathTemplates, "*.html", SearchOption.TopDirectoryOnly).ToList();
            if (serverFiles.Count() == 0) return templates;
            serverFiles = serverFiles.OrderByDescending(f => f).ToList();
            int count = 1;
            foreach (var file in serverFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName.Contains("_"))
                {
                    var userPart = fileName.Split("_")[1];
                    if (userPart == currentUser)
                        templates.Add(fileName, $"Template {count} (personalizado)");
                }
                else
                    templates.Add(fileName, $"Template {count} (sistema)");

                if (serverFiles.Count(x => x.Contains(fileName)) < 2) count++;
            }
            return templates;
        }

        private string NormalizeHtml(string html)
        {
            return string.IsNullOrEmpty(html) 
                ? html 
                : html
                .Replace("\r\n", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("\t", "")
                .Trim()
                .ToLowerInvariant();
        }
    }
}
