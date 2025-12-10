using DashBoard.Models;
using DashBoard.Models.Enums;
using DashBoard.Repositories;
using DashBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace DashBoard.Controllers
{
    [Authorize(Roles = "Master")]
    public class SistemaController : GeralController
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        public SistemaController(IUserRepository userRepository,
            IEncryptData encryptData,
            IConfiguration configuration,
            ICadastroRepository cadastroRepository,
            INewsLetterRepository newsLetterRepository,
            IRoleRepository roleRepository,
            IPermissoesRepository permissoesRepository,
            IEstadoRepository estadoRepository,
            ICidadeRepository cidadeRepository) : base(configuration, permissoesRepository, estadoRepository, cidadeRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        [HttpGet]
        [PermissionAuth(Atividade.Configs, Acoes.Visualizar)]
        public async Task<IActionResult> Index()
        {
            var model = new RolesEUsers
            {
                Roles = await _roleRepository.BuscaTodasRoles(),
                Users = await _userRepository.BuscarTodos()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Configs, Acoes.Salvar)]
        public async Task<IActionResult> BuscarRole(int id)
        {
            bool success = true;
            string html = string.Empty;
            string msg = string.Empty;
            try
            {
                Role? role = null;
                if (id > 0)
                    role = await _roleRepository.BuscaRolePorId(id);
                else
                {
                    role = new Role();
                    role.Permissoes = new List<Permissoes>()
                    {
                        new Permissoes() { Atividade = Atividade.Inicio },
                        new Permissoes() { Atividade = Atividade.Tabelas },
                        new Permissoes() { Atividade = Atividade.Configs },
                        new Permissoes() { Atividade = Atividade.Emails },
                    };
                }
                if (role == null)
                    throw new Exception("Role não encontrada!");

                html = await GetHtmlFormRole(role);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                success = false;
            }
            return Json(new { success = success, msg = msg, html = html });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Configs, Acoes.Salvar)]
        public async Task<IActionResult> CreateRole(Role role)
        {
            bool success = true;
            string message = string.Empty;
            string listaRole = string.Empty;
            string listaUser = string.Empty;
            string formUser = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(role.Nome))
                    throw new Exception("Nome da Role não pode ser vazio!");

                if (role.Id == 0)
                {
                    var result = await _roleRepository.AdicionarRoleComPermissao(role);
                    if (!result)
                        throw new Exception("Role não pode ser criada! Já existe uma igual!");
                    message = "Role criada com sucesso!";
                }
                else
                {
                    if (role.Id < 3)
                        throw new Exception("Role padrão do sistema não pode ser editada!");
                    var result = await _roleRepository.EditaRole(role);
                    if (!result)
                        throw new Exception("Role invalida ou não encontrada!");
                    message = "Role alterada com sucesso!";
                }
                listaRole = await GetHtmlListaRoles();
                listaUser = await GetHtmlListaUsers(await _roleRepository.BuscaTodasRolesSelect());
                formUser = await this.RenderViewAsync("_PartialNewUser", new User(), true);
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, msg = message, listaRole = listaRole, listaUser = listaUser, formUser = formUser });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Configs, Acoes.Excluir)]
        public async Task<IActionResult> DeleteRole(int id)
        {
            bool success = true;
            string message = string.Empty;
            string listaRole = string.Empty;
            string listaUser = string.Empty;
            string formUser = string.Empty;
            try
            {
                var role = await _roleRepository.BuscaRolePorId(id);
                if (role == null)
                    throw new Exception("Role não encontrada!");

                if (role.Id < 3)
                    throw new Exception("Role padrão do sistema não pode ser removida!");

                var roleGuest = await _roleRepository.BuscaRolePorId(2);
                if (roleGuest == null)
                    throw new Exception("Role 'Guest' não encontrada! Impossível reatribuir usuários.");


                var users = await _userRepository.BuscarTodos();
                if (users?.Any(x => x.RoleId == id) == true)
                {
                    var usersToUpdate = users.Where(x => x.RoleId == id).ToList();
                    foreach (var user in usersToUpdate)
                        await _userRepository.UpdateUserRole(user.Id, roleGuest.Id);
                }
                await _roleRepository.RemoveRole(id);
                listaRole = await GetHtmlListaRoles();
                listaUser = await GetHtmlListaUsers(await _roleRepository.BuscaTodasRolesSelect());
                formUser = await this.RenderViewAsync("_PartialNewUser", new User(), true);
                message = "Role removida com sucesso!";
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, msg = message, listaRole = listaRole, listaUser = listaUser, formUser = formUser });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Configs, Acoes.Salvar)]
        public async Task<IActionResult> CreateUser(User user)
        {
            bool success = true;
            string message = string.Empty;
            string lista = string.Empty;
            try
            {
                if (await _userRepository.UserExiste(user))
                    throw new Exception("Username, e-mail ou matrícula já cadastrados!");

                var cadastrado = await _userRepository.NewUser(user);
                if (!cadastrado)
                    throw new Exception("Ocorreu um erro ao tentar cadastrar! Tente novamente!");

                lista = await GetHtmlListaUsers(await _roleRepository.BuscaTodasRolesSelect());
                message = "Usuário cadastrado com sucesso!";
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, msg = message, lista = lista });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Configs, Acoes.Excluir)]
        public async Task<IActionResult> DeleteUser(string matricula)
        {
            bool success = true;
            string message = string.Empty;
            string lista = string.Empty;
            try
            {
                var user = await _userRepository.GetUserDB(matricula);
                if (user == null)
                    throw new Exception("Usuário não encontrado!");

                if (user.Matricula == "9999")
                    throw new Exception("User padrão do sistema não pode ser removido!");

                var result = await _userRepository.DeleteUser(user.Id);
                if (!result)
                    throw new Exception("Usuário não encontrado!");

                lista = await GetHtmlListaUsers(await _roleRepository.BuscaTodasRolesSelect());
                message = "User removido com sucesso!";
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, msg = message, lista = lista });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuth(Atividade.Configs, Acoes.Editar)]
        public async Task<IActionResult> AlteraRoleDoUsuario(string matricula, int roleId)
        {
            bool success = true;
            string message = string.Empty;
            string lista = string.Empty;
            try
            {
                var user = await _userRepository.GetUserDB(matricula);
                if (user == null)
                    throw new Exception("Usuário não encontrado!");

                if (user.Matricula == "9999")
                    throw new Exception("User padrão do sistema não pode ser alterado!");
                var role = await _roleRepository.BuscaRolePorId(roleId);
                if (role == null)
                    throw new Exception("Role não encontrada!");

                await _userRepository.UpdateUserRole(user.Id, roleId);
                lista = await GetHtmlListaUsers(await _roleRepository.BuscaTodasRolesSelect());
                message = "Role de usuário alterada com sucesso!";
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            return Json(new { success = success, msg = message, lista = lista });
        }

        private async Task<string> GetHtmlListaRoles()
        {
            string html = string.Empty;
            var roles = await _roleRepository.BuscaTodasRoles();
            return await this.RenderViewAsync("_PartialRoles", roles, true);
        }

        private async Task<string> GetHtmlListaUsers(List<SelectListItem>? roles)
        {
            string html = string.Empty;
            var users = await _userRepository.BuscarTodos();
            ViewData["RolesSelect"] = roles?.Any() == true ? roles : new List<SelectListItem>();
            return await this.RenderViewAsync("_PartialUsers", users, true);
        }

        private async Task<string> GetHtmlFormRole(Role role)
        {
            string html = string.Empty;
            return await this.RenderViewAsync("_PartialRoleForm", role, true);
        }

        public class RolesEUsers
        {
            public List<Role> Roles { get; set; } = new List<Role>();
            public List<User> Users { get; set; } = new List<User>();
        }
    }
}
