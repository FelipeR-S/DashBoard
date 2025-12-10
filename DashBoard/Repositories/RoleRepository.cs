using DashBoard.Data;
using DashBoard.Models;
using DashBoard.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DashBoard.Repositories;

public interface IRoleRepository
{
    /// <summary>
    /// Inicializa roles no banco de dados
    /// </summary>
    /// <returns></returns>
    Task InitRole();

    /// <summary>
    /// Adiciona uma nova role ao sistema
    /// </summary>
    /// <param name="role"><see cref="Role"/> para adicionar</param>
    /// <returns><see cref="bool"/> para validação do sucesso da ação</returns>
    Task<bool> AdicionarRole(Role role);

    /// <summary>
    /// Adiciona uma nova role ao sistema justo de suas permissões
    /// </summary>
    /// <param name="role"><see cref="Role"/> para adicionar</param>
    /// <returns><see cref="bool"/> para validação do sucesso da ação</returns>
    Task<bool> AdicionarRoleComPermissao(Role role);

    /// <summary>
    /// Remove uma role do sistema
    /// </summary>
    /// <param name="id"><see cref="int"/> Id da role para remoção</param>
    /// <returns><see cref="bool"/> para validação do sucesso da ação</returns>
    Task<bool> RemoveRole(int id);

    /// <summary>
    /// Faz a edição de uma role no sistema
    /// </summary>
    /// <param name="role"><see cref="Role"/> para editar</param>
    /// <returns><see cref="bool"/> para validação do sucesso da ação</returns>
    Task<bool> EditaRole(Role role);

    /// <summary>
    /// Busca uma role pelo Id
    /// </summary>
    /// <param name="id"><see cref="int"/> Id da role buscada</param>
    /// <returns><see cref="Role?"/> devolução de objeto ou nulo</returns>
    Task<Role?> BuscaRolePorId(int id);

    /// <summary>
    /// Busca todas as roles do sistema
    /// </summary>
    /// <returns><see cref="List{T}"/> onde T é <see cref="Role"/></returns>
    Task<List<Role>> BuscaTodasRoles();

    /// <summary>
    /// Busca todas as roles do sistema para select
    /// </summary>
    /// <returns><see cref="List{T}"/> onde T é <see cref="SelectListItem"/></returns>
    Task<List<SelectListItem>> BuscaTodasRolesSelect();
}
public class RoleRepository : BaseRepository<Role>, IRoleRepository
{
    private readonly PermissoesRepository _permissoesRepository;
    private List<Atividade> _permissoes;
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
        _permissoesRepository = new PermissoesRepository(context, this);
        _permissoes = Enum.GetValues<Atividade>().ToList();
    }

    public async Task InitRole()
    {
        var roleMaster = await _dbSet.Where(r => r.Id == 1).FirstOrDefaultAsync();
        if (roleMaster is null)
        {
            roleMaster = new Role() { Nome = "Master" };
            await _dbSet.AddAsync(roleMaster);
            await _context.SaveChangesAsync();
        }
        var roleGuest = await _dbSet.Where(r => r.Id == 2).FirstOrDefaultAsync();
        if (roleGuest is null)
        {
            roleGuest = new Role() { Nome = "Guest" };
            await _dbSet.AddAsync(roleGuest);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> AdicionarRole(Role role)
    {
        var roleDb = await _dbSet.FirstOrDefaultAsync(r => r.Id == role.Id || role.Nome.ToLower() == r.Nome.ToLower());
        if (roleDb is not null) return false;

        await _dbSet.AddAsync(role);
        await _context.SaveChangesAsync();
        await _permissoesRepository.InitPermissoes();
        return true;
    }

    public async Task<bool> AdicionarRoleComPermissao(Role role)
    {
        var roleDb = await _dbSet.FirstOrDefaultAsync(r => r.Id == role.Id || role.Nome.ToLower() == r.Nome.ToLower());
        if (roleDb is not null) return false;
        await _dbSet.AddAsync(role);
        await _context.SaveChangesAsync();
        foreach (var permissao in _permissoes)
        {
            var pm = new Permissoes()
            {
                Atividade = permissao,
                RoleId = role.Id,
            };
            if (role.Permissoes != null && role.Permissoes.Any(p => p.Atividade == permissao))
            {
                var rolePermissao = role.Permissoes.First(p => p.Atividade == permissao);
                pm.Salvar = rolePermissao.Salvar;
                pm.Editar = rolePermissao.Editar;
                pm.Excluir = rolePermissao.Excluir;
                pm.Visualizar = rolePermissao.Visualizar;
            }
            await _permissoesRepository.AdicionaPermissao(pm);
        }
        return true;
    }

    public async Task<bool> RemoveRole(int id)
    {
        if (id <= 2) return false;
        var roleDb = await _dbSet.FirstOrDefaultAsync(r => r.Id == id);
        if (roleDb == null) return false;

        await _permissoesRepository.ExcluiPermissoesByRoleId(id);
        _dbSet.Remove(roleDb);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EditaRole(Role role)
    {
        if (role.Id <= 2) return false;
        var roleDb = await _dbSet.FirstOrDefaultAsync(r => r.Id == role.Id);
        if (roleDb is null) return false;
        foreach (var permissao in _permissoes)
        {
            var pm = new Permissoes()
            {
                Atividade = permissao,
                RoleId = role.Id,
            };
            if (role.Permissoes != null && role.Permissoes.Any(p => p.Atividade == permissao))
            {
                var rolePermissao = role.Permissoes.First(p => p.Atividade == permissao);
                pm.Id = rolePermissao.Id;
                pm.Salvar = rolePermissao.Salvar;
                pm.Editar = rolePermissao.Editar;
                pm.Excluir = rolePermissao.Excluir;
                pm.Visualizar = rolePermissao.Visualizar;
                await _permissoesRepository.AlteraPermissao(pm);
            }
            else
                await _permissoesRepository.AdicionaPermissao(pm);
        }
        return true;
    }

    public async Task<Role?> BuscaRolePorId(int id)
    {
        var role = await _dbSet.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        if (role != null)
            role.Permissoes = await _permissoesRepository.GetPermissoesByRole(role.Id);
        return role;
    }

    public async Task<List<Role>> BuscaTodasRoles() => await _dbSet.AsNoTracking().ToListAsync();

    public async Task<List<SelectListItem>> BuscaTodasRolesSelect() => await _dbSet.AsNoTracking().Select(x => new SelectListItem() { Text = x.Nome, Value = x.Id.ToString() }).ToListAsync();
}
