using DashBoard.Data;
using DashBoard.Models;
using DashBoard.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DashBoard.Repositories;

public interface IPermissoesRepository
{
    /// <summary>
    /// Inicializa permissões adicionando de acordo com as roles e atividades existentes
    /// </summary>
    /// <returns></returns>
    Task InitPermissoes();

    /// <summary>
    /// Retorna permissão pelo id
    /// </summary>
    /// <param name="id"><see cref="int"/> id da permissão</param>
    /// <returns><see cref="Permissoes?"/> ou <see cref="null"/></returns>
    Task<Permissoes?> GetPermissaoById(int id);

    /// <summary>
    /// Retorna permissão por cargo e atividade
    /// </summary>
    /// <param name="cargo"><see cref="Role"/> cargo</param>
    /// <param name="atividadeId"><see cref="int"/> atividade id</param>
    /// <returns><see cref="Permissoes?"/> ou <see cref="null"/></returns>
    Task<Permissoes?> GetPermissaoByRoleEAtividade(int cargo, Atividade atividadeId);

    /// <summary>
    /// Retorna todas as permissões para o cargo informado
    /// </summary>
    /// <param name="roleId"><see cref="int"/> ID da <see cref="Role"/></param>
    /// <returns><see cref="List{T}"/> de <see cref="Permissoes"/></returns>
    Task<List<Permissoes>> GetPermissoesByRole(int roleId);

    /// <summary>
    /// Excluí todas as permissões de uma role
    /// </summary>
    /// <param name="roleId"><see cref="int"/> ID da <see cref="Role"/></param>
    /// <returns></returns>
    Task ExcluiPermissoesByRoleId(int roleId);

    /// <summary>
    /// Altera permissão caso exista
    /// </summary>
    /// <param name="permissao"><see cref="Permissoes"/> permissão existente</param>
    /// <returns><see cref="bool"/></returns>
    Task<bool> AlteraPermissao(Permissoes permissao);

    /// <summary>
    /// Adciona permissão caso
    /// </summary>
    /// <param name="permissao"><see cref="Permissoes"/> permissão para adicionr</param>
    /// <returns><see cref="bool"/> para validação do sucesso da ação</returns>
    Task<bool> AdicionaPermissao(Permissoes permissao);

    /// <summary>
    /// Retorna todas as permissões
    /// </summary>
    /// <returns><see cref="List{Permissoes}"/> de <see cref="Permissoes"/></returns>
    Task<List<Permissoes>> GetAllPermissoes();

    /// <summary>
    /// Remove permissões desatualizadas e adiciona novas permissões
    /// </summary>
    /// <returns></returns>
    Task AtualizaPermissoes();
}
public class PermissoesRepository : BaseRepository<Permissoes>, IPermissoesRepository
{
    private readonly IRoleRepository _roleRepository;

    public PermissoesRepository(ApplicationDbContext contex, IRoleRepository roleRepository) : base(contex)
    {
        _roleRepository = roleRepository;
    }

    public async Task InitPermissoes()
    {
        var permissaoDb = await _dbSet.FirstOrDefaultAsync();
        if (permissaoDb is null)
        {
            var roles = await _roleRepository.BuscaTodasRoles();
            if (roles.Any())
            {
                var listaPermissoes = new List<Permissoes>();
                foreach (var role in roles)
                {
                    foreach (var atv in Enum.GetValues(typeof(Atividade)))
                    {
                        bool permite = role.Id != 1 ? false : true;

                        var permissao = new Permissoes()
                        {
                            RoleId = role.Id,
                            Salvar = permite,
                            Editar = permite,
                            Excluir = permite,
                            Visualizar = true,
                            Atividade = (Atividade)atv
                        };
                        listaPermissoes.Add(permissao);
                    }
                }
                if (listaPermissoes.Any())
                {
                    await _dbSet.AddRangeAsync(listaPermissoes);
                    await _context.SaveChangesAsync();
                }
            }
        }
        await AtualizaPermissoes();
    }

    public async Task<bool> AdicionaPermissao(Permissoes permissao)
    {
        var permissaoDB = await GetPermissaoByRoleEAtividade(permissao.RoleId, permissao.Atividade);
        if (permissaoDB != null) return false;
        await _dbSet.AddAsync(permissao);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Permissoes?> GetPermissaoById(int id) 
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Permissoes?> GetPermissaoByRoleEAtividade(int role, Atividade atividade) 
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(p => p.RoleId == role && p.Atividade == atividade);

    public async Task<List<Permissoes>> GetPermissoesByRole(int roleId)
        => await _dbSet.AsNoTracking().Where(p => p.RoleId == roleId).ToListAsync();

    public async Task<bool> AlteraPermissao(Permissoes permissao)
    {
        var permissaoDB = await _dbSet.FirstOrDefaultAsync(p => p.Id == permissao.Id);
        if (permissaoDB == null) return false;
        permissaoDB.Salvar = permissao.Salvar;
        permissaoDB.Editar = permissao.Editar;
        permissaoDB.Excluir = permissao.Excluir;
        permissaoDB.Visualizar = permissao.Visualizar;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Permissoes>> GetAllPermissoes() 
        => await _dbSet.AsNoTracking().ToListAsync();

    public async Task AtualizaPermissoes()
    {
        await RemovePermissaoIncorreta();
        await AdicionaPermissaoFaltante();
    }

    public async Task ExcluiPermissoesByRoleId(int roleId)
    {
        if (roleId > 2)
        {
            var permissoes = await _dbSet.Where(x => x.RoleId == roleId).ToListAsync();
            if (permissoes != null && permissoes.Any())
            {
                _dbSet.RemoveRange(permissoes);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task RemovePermissaoIncorreta()
    {
        var permissoesRemover = new List<Permissoes>();
        var listaAtividades = Enum.GetValues(typeof(Atividade)).Cast<Atividade>();
        var listaRoles = await _roleRepository.BuscaTodasRoles();

        var permissaoIncorretaRole = await _dbSet.Where(p => !listaRoles.Select(r => r.Id).Contains(p.RoleId)).ToListAsync();
        if (permissaoIncorretaRole.Any()) permissoesRemover.AddRange(permissaoIncorretaRole);

        var permissaoIncorretaAtividade = await _dbSet.Where(p => !listaAtividades.ToList().Contains(p.Atividade)).ToListAsync();
        if (permissaoIncorretaAtividade.Any()) permissoesRemover.AddRange(permissaoIncorretaAtividade);

        if (permissoesRemover.Any()) 
        {
            _dbSet.RemoveRange(permissoesRemover);
            await _context.SaveChangesAsync();
        }
    }

    private async Task AdicionaPermissaoFaltante()
    {
        var listaAtividades = Enum.GetValues(typeof(Atividade)).Cast<Atividade>();
        var listaRoles = await _roleRepository.BuscaTodasRoles();
        var listaPermissoes = new List<Permissoes>();

        foreach (var role in listaRoles)
        {
            foreach (var atividade in listaAtividades)
            {
                var permissao = await GetPermissaoByRoleEAtividade(role.Id, atividade);
                if (permissao is not null) continue;

                bool permite = role.Id != 1 ? false : true;
                permissao = new Permissoes()
                {
                    RoleId = role.Id,
                    Salvar = permite,
                    Editar = permite,
                    Excluir = permite,
                    Visualizar = permite,
                    Atividade = atividade
                };
                listaPermissoes.Add(permissao);
            }
        }

        if (listaPermissoes.Any())
        {
            await _dbSet.AddRangeAsync(listaPermissoes);
            await _context.SaveChangesAsync();
        }
    }
}
