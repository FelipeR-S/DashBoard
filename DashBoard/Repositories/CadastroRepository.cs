using DashBoard.Data;
using DashBoard.Models;
using DashBoard.Models.Enums;
using DashBoard.Models.Stats;
using DashBoard.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DashBoard.Repositories
{
    public interface ICadastroRepository
    {
        /// <summary>
        /// Retorna quantidade de cadastros filtrados 
        /// </summary>
        /// <param name="tipoFiltro"><see cref="TipoFiltro"/> Tipo do filtro</param>
        /// <param name="naoDeclarado"><see cref="bool"/> declarados ou não</param>
        /// <param name="filtro"><see cref="string?"/> filtro opcional</param>
        /// <returns><see cref="int"/> Total</returns>
        Task<int> GetQuantidadeCadastro(TipoFiltro tipoFiltro, bool naoDeclarado, string? filtro = null);

        /// <summary>
        /// Retorna quantidade de cadastros filtrados por enum
        /// </summary>
        /// <param name="tipoFiltro"><see cref="TipoFiltro"/> Tipo do filtro</param>
        /// <param name="enumParametro"><see cref="Enum"/> parametro</param>
        /// <returns><see cref="int"/> Total</returns>
        Task<int> GetQuantidadeCadastro<T>(TipoFiltro tipoFiltro, T enumParametro) where T : Enum;

        /// <summary>
        /// Retorna quantidade de cadastros por estado 
        /// </summary>
        /// <param name="quantidadeTotal"><see cref="int"/> quantidade total atual</param>
        /// <returns><see cref="List{T}"/> de <see cref="StatsEstado"/></returns>
        Task<List<StatsEstado>> GetQuantidadeCadastroEstado(int quantidadeTotal);

        /// <summary>
        /// Retorna quantidade de cadastros por cidade 
        /// </summary>
        /// <param name="quantidadeTotal"><see cref="int"/> quantidade total atual</param>
        /// <returns><see cref="List{T}"/> de <see cref="StatsCidade"/></returns>
        Task<List<StatsCidade>> GetQuantidadeCadastroCidade(int quantidadeTotal);

        /// <summary>
        /// Gera lista fictícia de cadastros para testes 
        /// </summary>
        /// <returns></returns>
        Task CadastrosTeste();

        /// <summary>
        /// Metodo responsável por cadastrar cliente no banco de dados na tabela de <see cref="Cliente"></see> 
        /// </summary>
        /// <param name="novoCliente"><see cref="Cliente"/> novo cliente</param>
        /// <returns>Retorna <see cref="string"></see> de confirmação de cadasto</returns>
        Task<string> CadastraCliente(Cliente novoCliente);

        /// <summary>
        /// Retorna todos os dados da tabela de clientes
        /// </summary>
        /// <param name="filtro"><see cref="ClienteFiltroModelo"/> filtro de busca</param>
        /// <returns><see cref="List{T}"/> lista de <see cref="Cliente"/> de acordo com filtros requeridos</returns>
        Task<List<Cliente>?> GetDataTable(ClienteFiltroModelo filtro, CancellationToken ct = default);

        /// <summary>
        /// Retorna todos os emails da tabela de clientes
        /// </summary>
        /// <param name="filtro"><see cref="ClienteFiltroModelo"/> filtro de busca</param> 
        /// <returns><see cref="List{T}"/> lista de <see cref="Cliente"/> de acordo com filtros requeridos</returns>
        Task<string[]> GetEmailsPorFiltro(ClienteFiltroModelo filtro, CancellationToken ct = default);

        /// <summary>
        /// Deleta <see cref="Cliente"/> de acordo com os emails contidos no array
        /// </summary>
        /// <param name="emails"><see cref="Array"/> emails</param>
        /// <returns><see cref="string"/> mensagem de confirmação</returns>
        Task<string> DeletaCliente(string[] emails, CancellationToken ct);

        /// <summary>
        /// Exporta arquivo em Excel
        /// </summary>
        /// <param name="emails"><see cref="Array"/> de e-mails</param>
        /// <returns><see cref="List{T}"/> lista de <see cref="Dictionary{TKey, TValue}"/> contendo <see cref="string"/> Nome do atributo <see cref="object"/> atributo </returns>
        Task<List<Dictionary<string, object?>>> ExportDataTable(string[] emails);

        /// <summary>
        /// Retorna um unico <see cref="Cliente"/>
        /// </summary>
        /// <param name="atributo">Email ou telefone</param>
        /// <returns><see cref="Cliente"/> cliente ou null</returns>
        Task<Cliente?> GetCliente(string atributo);

        /// <summary>
        /// Gera loop para envio de emails para lista de clientes
        /// </summary>
        /// <param name="listaClientes"><see cref="List{T}"/> de <see cref="Cliente"/> e-mails</param>
        /// <param name="html"><see cref="string"/> Html da mensagem</param>
        /// <param name="assunto"><see cref="string"/> assunto</param>
        /// <param name="ct"><see cref="CancellationToken"/> token de cancelamento</param>
        /// <returns><see cref="Tuple{T1, T2}"/><see cref="int"/> quantidade de envios, <see cref="string"/> mensagem de confirmação</returns>
        Task<Tuple<int, string>> EnviaEmailMarketing(List<Cliente> listaClientes, RequestEmail request, CancellationToken ct);
    }
    public class CadastroRepository : BaseRepository<Cliente>, ICadastroRepository
    {
        private readonly ISendEmail _sendEmail;
        public CadastroRepository(ApplicationDbContext context, ISendEmail sendEmail) : base(context)
        {
            _sendEmail = sendEmail;
        }

        public async Task CadastrosTeste()
        {
            try
            {
                if (CacheSystem.GerarCadastrosTeste)
                {
                    var quantidade = await _dbSet.CountAsync();
                    if (quantidade > 500) return;
                    Random rnd = new Random();
                    List<Cliente> clienteAdd = new List<Cliente>();
                    #region .: listas :.
                    var listEmails = new List<string>()
                {
                    "@gmail.com",
                    "@outlook.com",
                    "@outlook.com.br",
                    "@live.com",
                };

                    #region .: prefix :.
                    var prefixos = new Dictionary<int, List<int>>();
                    prefixos.Add(1, new List<int>() { 11, 12, 13, 14, 15, 16, 17, 18, 19 });
                    prefixos.Add(2, new List<int>() { 21, 22, 24 });
                    prefixos.Add(3, new List<int>() { 96 });
                    prefixos.Add(4, new List<int>() { 92, 97 });
                    prefixos.Add(5, new List<int>() { 71, 73, 74, 75, 77 });
                    prefixos.Add(6, new List<int>() { 85, 88 });
                    prefixos.Add(7, new List<int>() { 61 });
                    prefixos.Add(8, new List<int>() { 27, 28 });
                    prefixos.Add(9, new List<int>() { 62, 64 });
                    prefixos.Add(10, new List<int>() { 98, 99 });
                    prefixos.Add(11, new List<int>() { 65, 66 });
                    prefixos.Add(12, new List<int>() { 67 });
                    prefixos.Add(13, new List<int>() { 31, 32, 33, 34, 35, 37, 38 });
                    prefixos.Add(14, new List<int>() { 91, 93, 94 });
                    prefixos.Add(15, new List<int>() { 83 });
                    prefixos.Add(16, new List<int>() { 41, 42, 43, 44, 45, 46, });
                    prefixos.Add(17, new List<int>() { 81, 87 });
                    prefixos.Add(18, new List<int>() { 89, 86 });
                    prefixos.Add(19, new List<int>() { 82 });
                    prefixos.Add(20, new List<int>() { 84 });
                    prefixos.Add(21, new List<int>() { 51, 53, 54, 55 });
                    prefixos.Add(22, new List<int>() { 69 });
                    prefixos.Add(23, new List<int>() { 95 });
                    prefixos.Add(24, new List<int>() { 47, 48, 49 });
                    prefixos.Add(25, new List<int>() { 68 });
                    prefixos.Add(26, new List<int>() { 79 });
                    prefixos.Add(27, new List<int>() { 63 });
                    #endregion

                    List<string> listaNomeFem = GetListaStringFromJson("./wwwroot/js/", "nomefem.json");
                    List<string> listaNomeMasc = GetListaStringFromJson("./wwwroot/js/", "nomemasc.json");
                    List<string> listaSobrenome = GetListaStringFromJson("./wwwroot/js/", "sobrenome.json");
                    var listCidades = CacheSystem.Cidades;
                    #endregion

                    while (quantidade <= 1000)
                    {
                        Cliente cliente = new Cliente();

                        #region .: Enumerados :.
                        int genero = rnd.Next(4);
                        genero = genero > 2 ? 99 : genero;

                        int estadoCivil = rnd.Next(5);
                        estadoCivil = estadoCivil > 3 ? 99 : estadoCivil;

                        var filhos = rnd.Next(5);
                        filhos = filhos > 3 ? 99 : filhos;

                        int renda = rnd.Next(6);
                        renda = renda > 4 ? 99 : renda;

                        int _fgts = rnd.Next(3);
                        bool? fgts = _fgts > 1 ? null : _fgts == 0;
                        #endregion

                        #region .: Nome :.
                        string nome;
                        if ((Genero)genero == Genero.Feminino)
                            nome = listaNomeFem[rnd.Next(listaNomeFem.Count - 1)];
                        else if ((Genero)genero == Genero.Masculino)
                            nome = listaNomeMasc[rnd.Next(listaNomeMasc.Count - 1)];
                        else
                        {
                            var numeroPar = rnd.Next(1, 2) % 2 == 0;
                            nome = numeroPar ?
                                listaNomeFem[rnd.Next(listaNomeFem.Count - 1)]
                                : listaNomeMasc[rnd.Next(listaNomeMasc.Count - 1)];
                        }
                        string sobreNomeAdd = string.Empty;
                        string sobreNome = listaSobrenome[rnd.Next(listaSobrenome.Count - 1)];
                        if (rnd.Next(3) == 0) sobreNomeAdd = listaSobrenome[rnd.Next(listaSobrenome.Count - 1)];
                        if (sobreNomeAdd != sobreNome) sobreNome += " " + sobreNomeAdd;
                        string nomeCliente = nome + " " + sobreNome;

                        #region .: Nome Camel Case :.
                        List<string> nomeSeparado = new List<string>();
                        foreach (var palavra in nomeCliente.Split(' ').Where(x => !string.IsNullOrEmpty(x)).ToList())
                        {
                            string corpo = palavra.ToLower();
                            string capital = corpo[0].ToString().ToUpper();
                            nomeSeparado.Add(capital + corpo.Remove(0, 1));
                        }
                        nomeCliente = string.Join(' ', nomeSeparado);
                        #endregion
                        #endregion

                        #region .: Email :.
                        string provedor = listEmails[rnd.Next(listEmails.Count - 1)];
                        string email = (nome + "." + sobreNome + rnd.Next(99).ToString() + provedor).ToLower();
                        #endregion

                        #region .: Endereço :.
                        int? cidadeId = null;
                        int? estadoId = null;
                        string? bairro = null;
                        bool cadastrarEndereco = rnd.Next(3) != 0;
                        if (listCidades != null && listCidades.Any() && cadastrarEndereco)
                        {
                            var cidadeBusca = rnd.Next(listCidades.Count - 1);
                            var cidadeEscolhida = listCidades[cidadeBusca];
                            cidadeId = cidadeEscolhida.Id;
                            estadoId = cidadeEscolhida.EstadoId;
                            bairro = cidadeEscolhida.CidadeNome + $" Bairro nº {cidadeBusca}";
                        }
                        #endregion

                        #region .: Telefone :.
                        int prefixBusca = estadoId.HasValue ?
                            estadoId.GetValueOrDefault() : rnd.Next(1, 27);
                        var prefixEscolhido = prefixos.FirstOrDefault(x => x.Key == prefixBusca).Value;
                        int ddd = prefixEscolhido[rnd.Next(prefixEscolhido.Count - 1)];

                        string telefone = $"{ddd}9";
                        for (int i = 0; i < 8; i++)
                        {
                            int min = 0;
                            if (i == 0) min = 5;
                            telefone += rnd.Next(min, 9).ToString();
                        }
                        #endregion

                        #region .: Nascimento :.
                        DateTime? nascimento = null;
                        bool cadastraNascimento = rnd.Next(2) != 0;
                        if (cadastraNascimento)
                        {
                            DateTime nascStart = DateTime.Now.AddYears(-90);
                            int totalDias = (DateTime.Now.AddYears(-16) - nascStart).Days;
                            nascimento = nascStart.AddDays(rnd.Next(totalDias));
                        }
                        #endregion

                        #region .: Cadastro :.
                        DateTime cadStart = DateTime.Now.AddYears(-3);
                        int totalDiasCad = (DateTime.Now - cadStart).Days;
                        DateTime cadastro = cadStart.AddDays(rnd.Next(totalDiasCad));
                        #endregion

                        cliente.Nome = nomeCliente;
                        cliente.Genero = genero;
                        cliente.Renda = renda;
                        cliente.Filhos = filhos;
                        cliente.EstadoCivil = estadoCivil;
                        cliente.FGTS = fgts;
                        cliente.Cidade = cidadeId;
                        cliente.Estado = estadoId;
                        cliente.Bairro = bairro;
                        cliente.Email = email;
                        cliente.Telefone = telefone;
                        cliente.Nascimento = nascimento;
                        cliente.UpdateDataCadastro(cadastro);

                        if (!clienteAdd.Any(x => x.Email == cliente.Email || x.Telefone == cliente.Telefone))
                        {
                            clienteAdd.Add(cliente);
                            quantidade++;
                        }
                    }
                    var listaEmails = clienteAdd.Select(x => x.Email).ToList();
                    var listaTelefone = clienteAdd.Select(x => x.Telefone).ToList();

                    var listaRemoveEmail = _dbSet.AsNoTracking().AsQueryable().Select(x => x.Email)
                        .Where(x => listaEmails.Any(e => e.Equals(x))).ToList();

                    var listaRemoveTelefone = _dbSet.AsNoTracking().AsQueryable().Select(x => x.Telefone)
                        .Where(x => listaTelefone.Any(e => e.Equals(x))).ToList();

                    clienteAdd = clienteAdd.Where(x =>
                    !listaRemoveEmail.Any(e => e.Equals(x.Email))
                    || !listaRemoveTelefone.Any(t => t.Equals(x.Telefone)))
                        .ToList();

                    await _dbSet.AddRangeAsync(clienteAdd);
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
            }
        }

        public async Task<int> GetQuantidadeCadastro(TipoFiltro tipoFiltro, bool naoDeclarado, string? filtro = null)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();
            switch (tipoFiltro)
            {
                case TipoFiltro.Idade:
                    if (naoDeclarado)
                        query = query.Where(x => !x.Nascimento.HasValue);
                    else
                        query = FiltroIdade(query, filtro);
                    break;
                case TipoFiltro.Periodo:
                    query = FiltroPeriodo(query, filtro);
                    break;
                case TipoFiltro.FGTS:
                    if (naoDeclarado)
                        query = query.Where(x => !x.FGTS.HasValue);
                    else
                        query = FiltroFGTS(query, filtro);
                    break;
                case TipoFiltro.Cidade:
                    query = query.Where(x => !x.Cidade.HasValue);
                    break;
                case TipoFiltro.Estado:
                    query = query.Where(x => !x.Estado.HasValue);
                    break;
                case TipoFiltro.Total:
                default:
                    break;
            }

            return await query.CountAsync();
        }

        public async Task<int> GetQuantidadeCadastro<T>(TipoFiltro tipoFiltro, T enumParametro) where T : Enum
        {
            int quantidade = 0;
            switch (tipoFiltro)
            {
                case TipoFiltro.Renda:
                    quantidade = await _dbSet.AsNoTracking().Where(x => x.Renda == (int)(Renda)EnumExtension.EnumToInt(enumParametro)).CountAsync();
                    break;
                case TipoFiltro.Genero:
                    quantidade = await _dbSet.AsNoTracking().Where(x => x.Genero == (int)(Genero)EnumExtension.EnumToInt(enumParametro)).CountAsync();
                    break;
                case TipoFiltro.EstadoCivil:
                    quantidade = await _dbSet.AsNoTracking().Where(x => x.EstadoCivil == (int)(EstadoCivil)EnumExtension.EnumToInt(enumParametro)).CountAsync();
                    break;
                default:
                    break;
            }

            return quantidade;
        }

        public async Task<List<StatsEstado>> GetQuantidadeCadastroEstado(int quantidadeTotal)
        {
            List<StatsEstado> list = new List<StatsEstado>();
            return (await _dbSet.AsQueryable()
                .Select(x => x.Estado).Where(x => x.HasValue).ToListAsync())
                .GroupBy(x => x)
                .Select(x => new StatsEstado(x.FirstOrDefault().GetValueOrDefault(), x.Count(), quantidadeTotal))
                .ToList();
        }

        public async Task<List<StatsCidade>> GetQuantidadeCadastroCidade(int quantidadeTotal)
        {
            List<StatsCidade> list = new List<StatsCidade>();
            return (await _dbSet.AsQueryable()
                .Select(x => x.Cidade).Where(x => x.HasValue).ToListAsync())
                .GroupBy(x => x)
                .Select(x => new StatsCidade(x.FirstOrDefault().GetValueOrDefault(), x.Count(), quantidadeTotal))
                .ToList();
        }

        public async Task<string> CadastraCliente(Cliente novoCliente)
        {
            var clienteDB = await _dbSet.Where(c => c.Telefone == novoCliente.Telefone || c.Email == novoCliente.Email).SingleOrDefaultAsync();

            if (clienteDB == null)
            {
                await _dbSet.AddAsync(novoCliente);
                await _context.SaveChangesAsync();
                return "Cadastro concluído";
            }
            else
            {
                if (clienteDB.Email == novoCliente.Email && clienteDB.Telefone == novoCliente.Telefone)
                    return "E-mail e telefone já constam nas bases de dados.";
                if (clienteDB.Email == novoCliente.Email)
                    return "E-mail já consta nas bases de dados.";
                else
                    return "Telefone já consta nas bases de dados.";
            }
        }

        public async Task<string> DeletaCliente(string[] emails, CancellationToken ct)
        {
            var nEmails = 0;
            var quantidadeErros = 0;
            var erros = new List<string>();
            try
            {
                foreach (var email in emails)
                {
                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                        break;
                    }
                    var cliente = await _dbSet.Where(cliente => cliente.Email == email).SingleOrDefaultAsync();
                    if (cliente == null)
                    {
                        quantidadeErros++;
                        erros.Add(email);
                        continue;
                    }
                    nEmails++;
                    _dbSet.Remove(cliente);
                }
                await _context.SaveChangesAsync();
                var mensagem = "";
                if (nEmails > 1) mensagem = $"{nEmails} cadastros foram excluídos do banco de dados! \n";
                else mensagem = $"{nEmails} cadastro foi excluído do banco de dados! \n";

                if (quantidadeErros > 0)
                {
                    mensagem += $"{quantidadeErros} cadastros não foram exluídos pois não foram encontrados:\n";
                    foreach (var erro in erros)
                    {
                        mensagem += $"{erro}\n";
                    }
                }
                return mensagem;

            }
            catch (OperationCanceledException)
            {
                return "Cadastros não foram excluídos";
            }

        }

        public async Task<List<Dictionary<string, object?>>> ExportDataTable(string[] emails)
        {
            var values = new List<Dictionary<string, object?>>();
            var clientes = await _dbSet.AsNoTracking().Where(c => emails.Contains(c.Email)).ToListAsync();
            foreach (var cliente in clientes)
            {
                if (cliente != null)
                {
                    string nascimento = "-";
                    string cidade = "-";
                    string estado = "-";
                    string fgts = "-";
                    string bairro = "-";

                    if (cliente.Nascimento is not null) nascimento = cliente.Nascimento.Value.ToString("dd/MM/yyyy");
                    if (cliente.Cidade is not null && CacheSystem.Cidades is not null && CacheSystem.Cidades.FirstOrDefault(x => x.Id == cliente.Cidade) is not null)
                        cidade = CacheSystem.Cidades.FirstOrDefault(x => x.Id == cliente.Cidade)!.CidadeNome;
                    if (cliente.Estado is not null && CacheSystem.Estados is not null && CacheSystem.Estados.FirstOrDefault(x => x.Id == cliente.Estado) is not null)
                        estado = CacheSystem.Estados.FirstOrDefault(x => x.Id == cliente.Estado)!.UF;
                    if (cliente.FGTS is not null) fgts = cliente.FGTS.Value ? "Possuí" : "Não Possuí";
                    if (!string.IsNullOrEmpty(cliente.Bairro)) bairro = cliente.Bairro;

                    values.Add(
                        new Dictionary<string, object?> {
                        { "Nome", cliente.Nome },
                        { "E-mail", cliente.Email },
                        { "Telefone", cliente.Telefone },
                        { "Nascimento", nascimento },
                        { "Bairro", bairro },
                        { "Cidade", cidade },
                        { "UF", estado },
                        { "Renda", EnumExtension.GetEnumMemberValue((Renda)cliente.Renda) },
                        { "FGTS", fgts },
                        { "Gênero", EnumExtension.GetEnumMemberValue((Genero)cliente.Genero) },
                        { "Estado Civil", EnumExtension.GetEnumMemberValue((EstadoCivil)cliente.EstadoCivil) },
                        { "Filhos", EnumExtension.GetEnumMemberValue((Filhos)cliente.Filhos) },
                        { "Cadastro", cliente.DataCadastro.ToString("dd/MM/yyyy")}
                    });
                }
            }
            return values;
        }

        public async Task<List<Cliente>?> GetDataTable(ClienteFiltroModelo filtro, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (filtro.Filhos is not null)
                query = query.Where(x => x.Filhos == (int)filtro.Filhos);

            if (filtro.Renda is not null)
                query = query.Where(x => x.Renda == (int)filtro.Renda);

            if (filtro.EstadoCivil is not null)
                query = query.Where(x => x.EstadoCivil == (int)filtro.EstadoCivil);

            if (filtro.Genero is not null)
                query = query.Where(x => x.Genero == (int)filtro.Genero);

            if (filtro.FGTS is not null)
                query = query.Where(x => x.FGTS == filtro.FGTS);

            if (filtro.EstadoId is not null)
                query = query.Where(x => x.Estado.GetValueOrDefault() == filtro.EstadoId);

            if (filtro.CidadeId is not null)
                query = query.Where(x => x.Cidade.GetValueOrDefault() == filtro.CidadeId);

            if (filtro.IdadeDe is not null)
                query = query.Where(x => x.Nascimento.GetValueOrDefault() <= DateTime.Now.AddYears(filtro.IdadeDe.GetValueOrDefault() * -1));

            if (filtro.IdadeAte is not null)
                query = query.Where(x => x.Nascimento.GetValueOrDefault() >= DateTime.Now.AddYears(filtro.IdadeAte.GetValueOrDefault() * -1));

            if (filtro.DataCadastroDe is not null)
                query = query.Where(x => x.DataCadastro <= filtro.DataCadastroDe);

            if (filtro.DataCadastroAte is not null)
                query = query.Where(x => x.DataCadastro <= filtro.DataCadastroAte);

            var total = await query.CountAsync(ct);
            var lista = await query.Skip(filtro.Pagina * filtro.QuantidadePorPagina)
                                .Take(filtro.QuantidadePorPagina)
                                .ToListAsync(ct);
            if (lista?.Any() == true)
                lista[0].QuantidadeDeRegistros = total;
            return lista;
        }

        public async Task<string[]> GetEmailsPorFiltro(ClienteFiltroModelo filtro, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();
            if (filtro.Filhos is not null)
                query = query.Where(x => x.Filhos == (int)filtro.Filhos);

            if (filtro.Renda is not null)
                query = query.Where(x => x.Renda == (int)filtro.Renda);

            if (filtro.EstadoCivil is not null)
                query = query.Where(x => x.EstadoCivil == (int)filtro.EstadoCivil);

            if (filtro.Genero is not null)
                query = query.Where(x => x.Genero == (int)filtro.Genero);

            if (filtro.FGTS is not null)
                query = query.Where(x => x.FGTS == filtro.FGTS);

            if (filtro.EstadoId is not null)
                query = query.Where(x => x.Estado.GetValueOrDefault() == filtro.EstadoId);

            if (filtro.CidadeId is not null)
                query = query.Where(x => x.Cidade.GetValueOrDefault() == filtro.CidadeId);

            if (filtro.IdadeDe is not null)
                query = query.Where(x => x.Nascimento.GetValueOrDefault() <= DateTime.Now.AddYears(filtro.IdadeDe.GetValueOrDefault() * -1));

            if (filtro.IdadeAte is not null)
                query = query.Where(x => x.Nascimento.GetValueOrDefault() >= DateTime.Now.AddYears(filtro.IdadeAte.GetValueOrDefault() * -1));

            if (filtro.DataCadastroDe is not null)
                query = query.Where(x => x.DataCadastro <= filtro.DataCadastroDe);

            if (filtro.DataCadastroAte is not null)
                query = query.Where(x => x.DataCadastro <= filtro.DataCadastroAte);

            var lista = await query.Select(x => x.Email).ToArrayAsync(ct);
            return lista;
        }

        public async Task<Cliente?> GetCliente(string atributo)
            => await _dbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Email == atributo || c.Telefone == atributo);

        public async Task<Tuple<int, string>> EnviaEmailMarketing(List<Cliente> listaClientes, RequestEmail request, CancellationToken ct)
        {
            var enviados = 0;
            var naoEnviados = listaClientes.Count();
            var mensagem = "";
            try
            {
                var options = new ParallelOptions
                {
                    CancellationToken = ct,
                    MaxDegreeOfParallelism = 10
                };
                var html = request.TemplateId;
                var estados = CacheSystem.Estados;
                var cidades = CacheSystem.Cidades;
                await Parallel.ForEachAsync(listaClientes, options, async (cliente, token) =>
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(cliente.Email))
                        {
                            var sendHtml = request.TemplateHtml.Replace("@nome", cliente.Nome)
                                                .Replace("@email", cliente.Email)
                                                .Replace("@telefone", cliente.Telefone)
                                                .Replace("@bairro", cliente.Bairro is null ? "" : cliente.Bairro);
                            if (cliente.Estado is not null)
                            {
                                var estado = estados?.FirstOrDefault(x => x.Id == cliente.Estado.GetValueOrDefault());
                                string estadoNome = estado is null ? "" : estado.EstadoNome;
                                sendHtml = sendHtml.Replace("@estado", estadoNome);
                            }
                            if (cliente.Cidade is not null)
                            {
                                var cidade = cidades?.FirstOrDefault(x => x.Id == cliente.Cidade.GetValueOrDefault());
                                string cidadeNome = cidade is null ? "" : cidade.CidadeNome;
                                sendHtml = sendHtml.Replace("@cidade", cidadeNome);
                            }

                            var result = await _sendEmail.EnviarEmail(cliente.Email, sendHtml, request.Assunto);
                            if (result)
                            {
                                await UpdateEnvios(cliente);
                                Interlocked.Decrement(ref naoEnviados);
                                Interlocked.Increment(ref enviados);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro: {ex.Message}");
                    }
                });
            }
            catch (OperationCanceledException)
            {
                mensagem = "Operação cancelada\r\n";
            }
            if (naoEnviados > 0) return new Tuple<int, string>(enviados, mensagem += $"{enviados} e-mails foram enviados!\r\n{naoEnviados} não puderam ser enviados.");
            else return new Tuple<int, string>(enviados, mensagem += $"{enviados} e-mails foram enviados!");
        }

        private async Task UpdateEnvios(Cliente cliente)
        {
            var currentCliente = await _dbSet.Where(c => c.Email == cliente.Email).FirstOrDefaultAsync();
            if (currentCliente is not null)
            {
                currentCliente.QuantidadeEnvios++;
                await _context.SaveChangesAsync();
            }
        }

        private IQueryable<Cliente> FiltroIdade(IQueryable<Cliente> query, string? filtro)
        {
            if (string.IsNullOrEmpty(filtro))
                return query;

            string[] filtroDeAte = filtro.Split('|');

            if (filtroDeAte.Length == 2)
            {
                int idadeDe;
                int idadeAte;
                bool existeIdadeDe = int.TryParse(filtroDeAte[0], out idadeDe);
                bool existeIdadeAte = int.TryParse(filtroDeAte[1], out idadeAte);

                var de = DateTime.Now.AddYears(idadeDe * -1);
                var ate = DateTime.Now.AddYears(idadeAte * -1);

                if (existeIdadeDe && existeIdadeAte)
                    query = query.Where(x => x.Nascimento != null && x.Nascimento >= ate && x.Nascimento <= de);
            }

            return query;
        }

        private IQueryable<Cliente> FiltroPeriodo(IQueryable<Cliente> query, string? filtro)
        {
            if (string.IsNullOrEmpty(filtro))
                return query;

            string[] filtroDeAte = filtro.Split('|');

            if (filtroDeAte.Length == 2)
            {
                DateTime periodoDe;
                DateTime periodoAte;
                bool existePeriodoDe = DateTime.TryParse(filtroDeAte[0], out periodoDe);
                bool existePeriodoAte = DateTime.TryParse(filtroDeAte[1], out periodoAte);

                if (existePeriodoDe && existePeriodoAte)
                    query = query.Where(x => x.DataCadastro >= periodoDe && x.DataCadastro <= periodoAte);
            }
            return query;
        }

        private IQueryable<Cliente> FiltroFGTS(IQueryable<Cliente> query, string? filtro)
        {
            bool temFGTS = false;
            var existeFiltro = bool.TryParse(filtro, out temFGTS);

            if (existeFiltro)
                query = query.Where(x => x.FGTS != null && x.FGTS == temFGTS);

            return query;
        }
    }
}
