using DashBoard.Data;
using DashBoard.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DashBoard.Repositories
{
    public interface IEstadoRepository
    {
        /// <summary>
        /// Inicializa estados no banco de dados a partir do arquivo JSON
        /// </summary>
        /// <returns></returns>
        Task InitDbEstado();

        /// <summary>
        /// Retorna a lista de todos os estados.
        /// </summary>
        /// <returns>Uma lista de <see cref="Estado"/> estados. A lista pode ser nula</returns>
        Task<List<Estado>> BuscarTodos();

        /// <summary>
        /// Retorna um estado.
        /// </summary>
        /// <param name="id">Id do estado</param>
        /// <returns>Um <see cref="Estado?"/> estado.</returns>
        Task<Estado?> BuscarPorId(int id);

        /// <summary>
        /// Retorna a lista de estados e todas as suas cidades.
        /// </summary>
        /// <param name="id">Id do estado</param>
        /// <returns>Uma lista de <see cref="Estado"/> e suas cidades. A lista pode ser nula</returns>
        Task<Estado?> BuscarEstadosECidadesPorEstadoId(int id);
    }
    public class EstadoRepository : BaseRepository<Estado>, IEstadoRepository
    {
        private readonly ICidadeRepository _cidadeRepository;
        public EstadoRepository(ApplicationDbContext context, ICidadeRepository cidadeRepository) : base(context)
        {
            _cidadeRepository = cidadeRepository;
        }
        public async Task InitDbEstado()
        {
            var estados = CarregaObjetoFromJson("./wwwroot/js/", "estados.json");
            if (estados != null && estados.Any())
            {
                List<Estado> listaAdicionar = new List<Estado>();
                foreach (Estado estado in estados)
                {
                    if (!_dbSet.Any(x => x.UF == estado.UF))
                        listaAdicionar.Add(estado);
                }
                if (listaAdicionar.Any())
                {
                    await _dbSet.AddRangeAsync(listaAdicionar);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<List<Estado>> BuscarTodos() => await _dbSet.AsNoTracking().ToListAsync();

        public async Task<Estado?> BuscarPorId(int id) => await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        public async Task<Estado?> BuscarEstadosECidadesPorEstadoId(int id)
        {
            Estado? estado = await _dbSet.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (estado != null)
                estado.Cidades = await _cidadeRepository.BuscarCidadesPorEstadoId(estado.Id);
            return estado;
        }
    }
}
