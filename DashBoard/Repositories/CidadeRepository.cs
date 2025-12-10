using DashBoard.Data;
using DashBoard.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DashBoard.Repositories
{
    public interface ICidadeRepository
    {
        /// <summary>
        /// Inicializa cidades no banco de dados a partir do arquivo JSON
        /// </summary>
        /// <returns></returns>
        Task InitDbCidade();

        /// <summary>
        /// Retorna a lista de todas as cidades.
        /// </summary>
        /// <returns>Uma lista de <see cref="Cidade"/> cidades. A lista pode ser nula</returns>
        Task<List<Cidade>> BuscarTodos();

        /// <summary>
        /// Retorna uma cidade.
        /// </summary>
        /// <param name="id">Id da cidade</param>
        /// <returns>Uma <see cref="Cidade"/> cidade.</returns>
        Task<Cidade?> BuscarPorId(int id);

        /// <summary>
        /// Retorna a lista de todas as cidades de um estado.
        /// </summary>
        /// <param name="id">Id do estado</param>
        /// <returns>Uma lista de <see cref="Cidade"/> cidades. A lista pode ser nula</returns>
        Task<List<Cidade>?> BuscarCidadesPorEstadoId(int id);
    }
    public class CidadeRepository : BaseRepository<Cidade>, ICidadeRepository
    {
        public CidadeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task InitDbCidade()
        {
            if (_dbSet.Any())
                return;

            var cidades = CarregaObjetoFromJson("./wwwroot/js/", "cidades.json");
            if (cidades != null && cidades.Any())
            {
                List<Cidade> listaAdicionar = new List<Cidade>();
                foreach (Cidade cidade in cidades)
                {
                    listaAdicionar.Add(cidade);
                }
                if (listaAdicionar.Any())
                {
                    await _dbSet.AddRangeAsync(listaAdicionar);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<List<Cidade>> BuscarTodos() => await _dbSet.AsNoTracking().ToListAsync();

        public async Task<Cidade?> BuscarPorId(int id) => await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        public async Task<List<Cidade>?> BuscarCidadesPorEstadoId(int id) => await _dbSet.AsNoTracking().Where(x => x.EstadoId == id).ToListAsync();
    }
}
