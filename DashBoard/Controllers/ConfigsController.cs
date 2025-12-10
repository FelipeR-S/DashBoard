using DashBoard.Models;
using DashBoard.Models.Enums;
using DashBoard.Repositories;
using DashBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DashBoard.Controllers
{
    [Authorize]
    public class ConfigsController : GeralController
    {
        private readonly IUserRepository _userRepository;
        public ConfigsController(IUserRepository userRepository,
            IConfiguration configuration,
            ICadastroRepository cadastroRepository,
            INewsLetterRepository newsLetterRepository,
            IPermissoesRepository permissoesRepository,
            IEstadoRepository estadoRepository,
            ICidadeRepository cidadeRepository) : base(configuration, permissoesRepository, estadoRepository, cidadeRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        [PermissionAuth(Atividade.Configs, Acoes.Visualizar)]
        public async Task<IActionResult> Index()
        {

            var userMatricula = User.FindFirst("Matricula")?.Value;
            if (userMatricula == null)
                return RedirectToAction("Index", "Login");
            var user = (await _userRepository.GetUserDB(userMatricula)) ?? new User();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Configs, Acoes.Visualizar)]
        public async Task<IActionResult> UpdateUserConfig([FromBody] string[] configs)
        {
            bool sucesso = true;
            string msg = string.Empty;
            try
            {
                var userMatricula = User.FindFirst("Matricula")?.Value;
                if (userMatricula == null)
                    throw new Exception("Erro ao alterar configurações");

                HttpContext.Session.SetString("Tema", configs[0]);
                HttpContext.Session.SetString("Aviso", configs[1]);
                await _userRepository.UpdateUserConfig(userMatricula, configs[0], configs[1]);
                msg = "Configurações atualizadas com sucesso!";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                sucesso = false;
            }
            return Json(new { success = sucesso, msg = msg });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Configs, Acoes.Editar)]
        public async Task<IActionResult> UpdateUser(User user)
        {
            bool sucesso = true;
            string msg = string.Empty;
            try
            {
                var statusOk = await _userRepository.UpdateUser(user);
                if (!statusOk.Key)
                    throw new Exception(statusOk.Value);
                msg = statusOk.Value;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                sucesso = false;
            }
            return Json(new { success = sucesso, msg = msg });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Configs, Acoes.Editar)]
        public async Task<IActionResult> UpdateSenha(User user)
        {
            bool sucesso = true;
            string msg = string.Empty;
            try
            {
                var statusOk = await _userRepository.UpdatePassword(user.Id, user.Senha);
                if (!statusOk.Key)
                    throw new Exception(statusOk.Value);
                msg = statusOk.Value;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                sucesso = false;
            }
            return Json(new { success = sucesso, msg = msg });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Configs, Acoes.Editar)]
        public async Task<string> SalvaImagem()
        {
            try
            {
                string path = "./wwwroot/img/dashboard/user";
                var file = Request.Form.Files[0];
                var fileName = Request.Form["fileName"];
                await SaveFile(file, path, fileName);
                return "Imagem salva!";
            }
            catch (Exception)
            {
                return "Erro ao salvar imagem!";
            }
        }
    }
}
