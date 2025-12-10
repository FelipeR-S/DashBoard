using DashBoard.Models;
using DashBoard.Repositories;
using DashBoard.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;


namespace DashBoard.Controllers
{
    public class GeralController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IPermissoesRepository _permissoesRepository;
        private readonly IEstadoRepository _estadoRepository;
        private readonly ICidadeRepository _cidadeRepository;
        public int Meta { get; private set; }

        public GeralController(IConfiguration configuration, IPermissoesRepository permissoesRepository, IEstadoRepository estadoRepository, ICidadeRepository cidadeRepository)
        {
            _configuration = configuration;
            _permissoesRepository = permissoesRepository;
            _estadoRepository = estadoRepository;
            _cidadeRepository = cidadeRepository;
        }

        protected async Task Autentica(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Usuario),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role?.Nome ?? "Guest"),
                new Claim("Matricula", user.Matricula),
                new Claim("Aviso", user.Aviso.ToString()),
                new Claim("Tema", user.Tema)
            };
            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("CookieAuth", principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(1),
                    AllowRefresh = true
                });
            await InitPermissionsSession(user.RoleId);
        }

        protected async Task RemoveAutenticacao()
        {
            await HttpContext.SignOutAsync("CookieAuth");

            HttpContext.Session.Clear();
            TempData.Clear();
            EndPermissionsSession();
        }

        protected async Task SaveFile(IFormFile file, string path, string fileName)
        {
            if (System.IO.File.Exists(Path.Combine(path, fileName))) System.IO.File.Delete(Path.Combine(path, fileName));
            await using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                file.CopyTo(stream);
            }
        }

        [HttpGet]
        protected string GetResponseString()
        {
            var Response = HttpContext.Session.GetString("responseServer");
            HttpContext.Session.SetString("responseServer", "");
            if (String.IsNullOrEmpty(Response)) return "Erro ao obter resposta.";
            else return Response;
        }

        private void LimpaCache()
        {
            ViewBag.Renda = null;
            ViewBag.Genero = null;
            ViewBag.EstadoCivil = null;
            ViewBag.Filhos = null;
            ViewBag.Estados = null;
        }

        private void CarregaViewBagListas()
        {
            ViewBag.Renda = CacheSystem.RendaDashboard;
            ViewBag.Genero = CacheSystem.GeneroDashboard;
            ViewBag.EstadoCivil = CacheSystem.EstadoCivilDashboard;
            ViewBag.Filhos = CacheSystem.FilhosDashboard;
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

        private async Task InitPermissionsSession(int roleId)
        {
            var permissoes = await _permissoesRepository.GetPermissoesByRole(roleId);
            if (permissoes is null) throw new Exception("Problemas ao inicializar serviços de permissões para o usuário. Favor tente novamente!");
            HttpContext.Session.Set("PermissoesCache", permissoes);
        }

        private void EndPermissionsSession()
        {
            var permissoes = new List<Permissoes>();
            HttpContext.Session.Set("PermissoesCache", permissoes);
        }
    }
}
