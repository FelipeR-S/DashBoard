using DashBoard.Data;
using DashBoard.Models;
using DashBoard.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DashBoard.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Verifica se o usuário existe via username, e-mail ou matrícula
        /// </summary>
        /// <param name="user"><see cref="User"/> Usuário</param>
        /// <returns>Retorna <see cref="bool"/></returns>
        Task<bool> UserExiste(User user);

        /// <summary>
        /// metodo publico que verifica se usuário é autenticado
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Retorna <see cref="bool"/></returns>
        Task<bool> AutenticaUser(User user);

        /// <summary>
        /// Cria user Admin caso não exista no banco de dados
        /// </summary>
        /// <returns></returns>
        Task InitUser();

        /// <summary>
        /// Verifica se o usuário existe via email
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Retorna <see cref="bool"/></returns>
        Task<bool> ExisteEmail(string email);

        /// <summary>
        /// Envia email de recuperação de conta
        /// </summary>
        /// <param name="email"></param>
        /// <param name="baseUrl"></param>
        /// <returns>Retorna <see cref="string"/> como resposta de email concluído</returns>
        Task<string> RecuperaDados(string email, string baseUrl);

        /// <summary>
        /// Retorna usuário do banco de dados
        /// </summary>
        /// <param name="dados"><see cref="string"/> username ou matrícula</param>
        /// <returns><see cref="User"/> encontrado</returns>
        Task<User?> GetUserDB(string dados);

        /// <summary>
        /// Atualiza dados de usuário no banco de dados
        /// </summary>
        /// <param name="user"><see cref="User"/> usuário</param>
        /// <returns><see cref="bool"/> sucesso da operação</returns>
        Task<KeyValuePair<bool, string>> UpdateUser(User user);

        /// <summary>
        /// Insere novo usuário no banco de dados
        /// </summary>
        /// <param name="user"><see cref="User"/> usuário</param>
        /// <returns><see cref="bool"/> sucesso da operação</returns>
        Task<bool> NewUser(User user);

        /// <summary>
        /// Atualiza configuações de usuário no banco de dados
        /// </summary>
        /// <param name="matricula"><see cref="string"/> matrícula para encontrar o <see cref="User"/></param>
        /// <param name="tema"><see cref="string"/> Tema de cores</param>
        /// <param name="aviso"><see cref="string"/> quantidade de cadastros</param>
        Task UpdateUserConfig(string matricula, string tema, string aviso);

        /// <summary>
        /// Atualiza quantidade de emails enviados
        /// </summary>
        /// <param name="matricula"><see cref="string"/> matricula do usuario</param>
        /// <param name="quantidade"><see cref="int"/> quantidade a ser adicionada</param>
        /// <returns></returns>
        Task UpdateEnvios(string matricula, int quantidade);

        /// <summary>
        /// Altera senha do usuário <see cref="User"/>
        /// </summary>
        /// <param name="token"><see cref="string"/> token</param>
        /// <param name="email"><see cref="string"/> token</param>
        /// <param name="novaSenha"><see cref="string"/> token</param>
        /// <returns><see cref="KeyValuePair"/> Retorna sucesso <see cref="bool"/> e mensagem <see cref="string"/></returns>
        Task<KeyValuePair<bool, string>> UpdatePassword(string token, string email, string novaSenha);

        /// <summary>
        /// Altera senha do usuário <see cref="User"/>
        /// </summary>
        /// <param name="id"><see cref="int"/> Id do usuário</param>
        /// <param name="novaSenha"><see cref="string"/> token</param>
        /// <returns><see cref="KeyValuePair"/> Retorna sucesso <see cref="bool"/> e mensagem <see cref="string"/></returns>
        Task<KeyValuePair<bool, string>> UpdatePassword(int id, string novaSenha);

        /// <summary>
        /// Retorna lista de usuários do banco de dados
        /// </summary>
        /// <returns><see cref="List{T}"/> onde T é <see cref="User"/></returns>
        Task<List<User>> BuscarTodos();

        /// <summary>
        /// Remove usuário no banco de dados
        /// </summary>
        /// <param name="id"><see cref="int"/> usuário Id</param>
        /// <returns><see cref="bool"/> sucesso da operação</returns>
        Task<bool> DeleteUser(int id);


        /// <summary>
        /// Altera role do usuário no banco de dados
        /// </summary>
        /// <param name="id"><see cref="int"/> usuário Id</param>
        /// <param name="roleId"><see cref="int"/> role Id</param>
        /// <returns><see cref="bool"/> sucesso da operação</returns>
        Task<bool> UpdateUserRole(int id, int roleId);
    }

    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly IEncryptData _encrypt;
        private readonly ISendEmail _sendEmail;
        private readonly IConfiguration _configuration;
        private readonly IRoleRepository _roleRepository;

        public UserRepository(ApplicationDbContext context, IEncryptData encrypt, ISendEmail sendEmail, IConfiguration configuration, IRoleRepository roleRepository) : base(context)
        {
            _encrypt = encrypt;
            _sendEmail = sendEmail;
            _configuration = configuration;
            _roleRepository = roleRepository;
        }

        public async Task InitUser()
        {
            var admin = await _dbSet.FirstOrDefaultAsync(u => u.Matricula == "9999" || u.Usuario == _configuration["DefaultUser:UserName"]);
            if (admin == null)
            {
                var senha = _configuration["DefaultUser:Password"];
                var email = _configuration["DefaultUser:Email"];
                var role = await _roleRepository.BuscaRolePorId(1);

                var userAdmin = new User()
                {
                    Matricula = "9999",
                    Usuario = _configuration["DefaultUser:UserName"],
                    Senha = _encrypt.Encrypt(senha),
                    Email = _encrypt.Encrypt(email),
                    RoleId = role!.Id
                };

                await _dbSet.AddAsync(userAdmin);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserExiste(User user)
        {
            var emailValidar = !string.IsNullOrEmpty(user.Email) ? _encrypt.Encrypt(user.Email) : "";
            return await _dbSet.AsNoTracking().AnyAsync(u => (!string.IsNullOrEmpty(user.Usuario) && u.Usuario == user.Usuario)
                                            || (!string.IsNullOrEmpty(user.Matricula) && u.Matricula == user.Matricula)
                                            || u.Email == emailValidar);
        }

        public async Task<bool> AutenticaUser(User user)
            => await VerificaSenha(user);

        public async Task<bool> ExisteEmail(string email)
        => await VerificaEmail(email);

        public async Task<string> RecuperaDados(string email, string baseUrl)
        {
            var dadoEncriptado = _encrypt.Encrypt(email);
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Email == dadoEncriptado);
            if (user is null) return "Não foi possível enviar o email";
            user.TokenRenovacao = Guid.NewGuid().ToString();
            user.ExpiraToken = DateTime.Now.AddMinutes(120);
            await _context.SaveChangesAsync();
            var html = GeraHTMLRecuperacao(user, baseUrl);
            var envio = await _sendEmail.EnviarEmail(email, html, "Recuperação de dados");
            if (envio) return "E-mail enviado com sucesso!";
            else return "Falha no envio de e-mail!";
        }

        public async Task<User?> GetUserDB(string dados)
        {
            var userDB = await _dbSet.AsNoTracking().Include(x => x.Role).FirstOrDefaultAsync(u => u.Usuario == dados || u.Matricula == dados);
            if (userDB == null) return userDB;
            userDB.Email = _encrypt.Decrypt(userDB.Email);
            return userDB;
        }

        public async Task UpdateUserConfig(string matricula, string tema, string aviso)
        {
            var currentUser = await _dbSet.FirstOrDefaultAsync(u => u.Matricula == matricula);
            if (currentUser is not null)
            {
                int.TryParse(aviso, out int avisoUser);
                currentUser.Tema = tema;
                currentUser.Aviso = avisoUser;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> NewUser(User user)
        {
            user.Email = _encrypt.Encrypt(user.Email);
            var userDB = await _dbSet.FirstOrDefaultAsync(u => u.Matricula == user.Matricula || u.Usuario == user.Usuario || u.Email == user.Email);
            if (userDB != null) return false;

            user.Senha = _encrypt.Encrypt(user.Senha);

            await _dbSet.AddAsync(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUser(int id)
        {
            var userDB = await _dbSet.FirstOrDefaultAsync(u => u.Id == id);
            if (userDB is null) return false;
            _dbSet.Remove(userDB);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserRole(int id, int roleId)
        {
            var userDB = await _dbSet.FirstOrDefaultAsync(u => u.Id == id);
            if (userDB is null) return false;
            userDB.RoleId = roleId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<KeyValuePair<bool, string>> UpdateUser(User user)
        {
            var currentUser = await _dbSet.FirstOrDefaultAsync(u => u.Matricula == user.Matricula);
            if (currentUser is null)
                return new KeyValuePair<bool, string>(false, "Usuário não encontrado");

            var emailEncrypt = _encrypt.Encrypt(user.Email);
            if (await _dbSet.AnyAsync(u => u.Usuario == user.Usuario && u.Matricula != user.Matricula))
                return new KeyValuePair<bool, string>(false, "Nome de usuário já está em uso");

            if (await _dbSet.AnyAsync(u => u.Email == emailEncrypt && u.Matricula != user.Matricula))
                return new KeyValuePair<bool, string>(false, "E-mail de usuário já está em uso");

            if (!string.IsNullOrEmpty(user.Usuario)) currentUser.Usuario = user.Usuario;
            if (!string.IsNullOrEmpty(user.Nome)) currentUser.Nome = user.Nome;
            if (!string.IsNullOrEmpty(user.Email)) currentUser.Email = emailEncrypt;
            if (!string.IsNullOrEmpty(user.Telefone)) currentUser.Telefone = user.Telefone;
            if (!string.IsNullOrEmpty(user.Endereco)) currentUser.Endereco = user.Endereco;

            await _context.SaveChangesAsync();
            return new KeyValuePair<bool, string>(true, "Dados de usuário atualizados");
        }

        public async Task UpdateEnvios(string matricula, int quantidade)
        {
            var currentUser = await _dbSet.FirstOrDefaultAsync(c => c.Matricula == matricula);
            if (currentUser is not null)
            {
                currentUser.QuantidadeEnvios += quantidade;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<KeyValuePair<bool, string>> UpdatePassword(string token, string emailOuUserName, string novaSenha)
        {
            var dadoEncriptado = _encrypt.Encrypt(emailOuUserName);
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Usuario == emailOuUserName || u.Email == dadoEncriptado);
            if (user is null) return new KeyValuePair<bool, string>(false, "Usuário não encontrado");
            if (user.TokenRenovacao != token || !user.ExpiraToken.HasValue || user.ExpiraToken.Value < DateTime.Now)
                return new KeyValuePair<bool, string>(false, "Token inválido ou expirado");
            user.Senha = _encrypt.Encrypt(novaSenha);
            _context.Update(user);
            await _context.SaveChangesAsync();
            return new KeyValuePair<bool, string>(true, "Senha atualizada com sucesso");
        }

        public async Task<KeyValuePair<bool, string>> UpdatePassword(int id, string novaSenha)
        {
            var user = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return new KeyValuePair<bool, string>(false, "Usuário não encontrado");

            user.Senha = _encrypt.Encrypt(novaSenha);
            _context.Update(user);
            await _context.SaveChangesAsync();
            return new KeyValuePair<bool, string>(true, "Senha atualizada com sucesso");
        }

        public async Task<List<User>> BuscarTodos()
        {
            var users = await _dbSet.AsNoTracking().Include(x => x.Role).Select(x => new User()
            {
                Id = x.Id,
                Nome = x.Nome,
                Matricula = x.Matricula,
                Usuario = x.Usuario,
                RoleId = x.RoleId,
                Role = x.Role,
            }).ToListAsync();
            return users;
        }

        private async Task<bool> VerificaSenha(User user)
        {
            var UserDB = await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Usuario == user.Usuario);
            if (UserDB == null) return false;

            var senha = _encrypt.Decrypt(UserDB.Senha);
            if (user.Senha == senha) return true;
            else return false;
        }

        private async Task<bool> VerificaEmail(string email)
        {
            var emailEncrypt = _encrypt.Encrypt(email);
            var UserDB = await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == emailEncrypt);
            if (UserDB == null) return false;

            var emailPrincipal = _encrypt.Decrypt(UserDB.Email);
            if (email == emailPrincipal) return true;
            else return false;
        }

        private string GeraHTMLRecuperacao(User user, string baseUrl)
        {
            string link = $"{baseUrl}/Login/RecuperarSenha?token={user.TokenRenovacao}&emailRecuperacao={Uri.EscapeDataString(user.Email)}";
            string email = $@"
                    <!DOCTYPE html>
                    <html lang=""pt-br"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Recuperação de Conta</title>
                    </head>
                    
                    <body style=""margin:0;padding:0;font-family:Arial, sans-serif;background-color:#f5f5f5;"">
                    
                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f5f5f5;padding:20px 0;"">
                            <tr>
                                <td align=""center"">
                    
                                    <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:8px;overflow:hidden;"">
                    
                                        <!-- HEADER -->
                                        <tr>
                                            <td style=""background-color:#222222;padding:20px;text-align:center;"">
                                                <h1 style=""color:#ffffff;margin:0;font-size:24px;text-transform:uppercase;"">
                                                    Recuperação de Acesso
                                                </h1>
                                            </td>
                                        </tr>
                    
                                        <!-- CONTEÚDO -->
                                        <tr>
                                            <td style=""padding:25px;font-size:16px;color:#333333;"">
                    
                                                <p>Você solicitou a recuperação de dados da sua conta. Confira suas informações abaixo:</p>
                    
                                                <p><strong>Usuário:</strong> {user.Usuario}</p>
                                                <p><strong>Email Principal:</strong> {_encrypt.Decrypt(user.Email)}</p>
                    
                                                <br />
                    
                                                <p>
                                                    Clique no botão abaixo para prosseguir com a recuperação da sua conta.
                                                    O link é válido por <strong>5 minutos</strong>.
                                                </p>
                    
                                                <!-- BOTÃO -->
                                                <table cellpadding=""0"" cellspacing=""0"" style=""margin:20px 0;"">
                                                    <tr>
                                                        <td align=""center"">
                                                            <a href=""{link}"" target=""_blank""
                                                               style=""background-color:#007bff;color:#ffffff;padding:12px 25px;
                                                               text-decoration:none;font-size:16px;border-radius:5px;display:inline-block;"">
                                                                Recuperar Conta
                                                            </a>
                                                        </td>
                                                    </tr>
                                                </table>
                    
                                                <p>Se você não solicitou este procedimento, pode simplesmente ignorar este e-mail.</p>
                    
                                            </td>
                                        </tr>
                    
                                        <!-- FOOTER -->
                                        <tr>
                                            <td style=""background-color:#222222;padding:15px;text-align:center;color:#ffffff;font-size:14px;"">
                                                Mensagem automática — não responda.
                                            </td>
                                        </tr>
                    
                                    </table>
                    
                                </td>
                            </tr>
                        </table>
                    
                    </body>
                    </html>
                    ";
            return email;
        }
    }
}
