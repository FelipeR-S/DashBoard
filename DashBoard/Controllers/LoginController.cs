using DashBoard.Models;
using DashBoard.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DashBoard.Controllers
{
    public class LoginController : GeralController
    {
        private readonly IEncryptData _encrypt;
        private readonly IUserRepository _userRepository;
        public LoginController(IConfiguration configuration, IPermissoesRepository permissoesRepository, IUserRepository userRepository, IEstadoRepository estadoRepository, ICidadeRepository cidadeRepository, IEncryptData encrypt) : base(configuration, permissoesRepository, estadoRepository, cidadeRepository)
        {
            _userRepository = userRepository;
            _encrypt = encrypt;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrar(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (await _userRepository.UserExiste(user))
                    {
                        if (await _userRepository.AutenticaUser(user))
                        {
                            var userDB = await _userRepository.GetUserDB(user.Usuario);
                            if (userDB == null)
                                throw new Exception("Usuário não encontrado");

                            await Autentica(userDB);
                            return RedirectToAction("Index", "Inicio");
                        }
                        else throw new Exception("Senha Incorreta");
                    }
                    else throw new Exception("Usuário não encontrado");
                }
                catch (Exception ex)
                {
                    TempData["ErroLogin"] = ex.Message;
                    TempData["User"] = user.Usuario;
                    return RedirectToAction("Index", "Login");
                }
            }
            return RedirectToAction("Index", "Login");
        }

        public async Task<IActionResult> Sair()
        {
            await RemoveAutenticacao();
            return RedirectToAction("Index", "Login");
        }

        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Inicio");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviaRecuperaEmail(User user)
        {
            var mensagem = "E-mail incorreto.\n Favor verificar!";
            if (ModelState.IsValid)
            {
                var emailExiste = await _userRepository.ExisteEmail(user.Email);

                if (!emailExiste) mensagem = "E-mail não encontrado na base de dados!";
                else mensagem = await _userRepository.RecuperaDados(user.Email, $"{Request.Scheme}://{Request.Host}");
            }

            TempData["RespostaRecupera"] = "";
            TempData["ErroLogin"] = mensagem;
            return RedirectToAction("Index", "Login");
        }

        [HttpGet]
        public async Task<IActionResult> RecuperarSenha(string token, string emailRecuperacao)
        {
            string emailDecript = "";
            try
            {
                emailDecript = _encrypt.Decrypt(emailRecuperacao);
            }
            catch { }
            var model = new User() { Email = emailDecript, TokenRenovacao = token };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recuperar(User user)
        {
            var mensagem = "Senha de confirmação não é igual a senha de alteração!";
            bool sucesso = false;
            if (user.Senha == user.SenhaConfirmar)
            {
                var emailExiste = await _userRepository.ExisteEmail(user.Email);
                if (!emailExiste) mensagem = "E-mail não encontrado na base de dados!";
                var result = await _userRepository.UpdatePassword(user.TokenRenovacao, user.Email, user.Senha);
                mensagem = result.Value;
                sucesso = result.Key;
            }

            if (sucesso)
            {
                TempData["ErroLogin"] = mensagem;
                return RedirectToAction("Index", "Login");
            }
            TempData["RespostaRecupera"] = mensagem;
            var encriptEmail = _encrypt.Encrypt(user.Email);
            return RedirectToAction(
                "RecuperarSenha",
                "Login",
                new { token = user.TokenRenovacao, emailRecuperacao = encriptEmail }
            );
        }
    }
}
