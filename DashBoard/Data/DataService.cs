using DashBoard.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DashBoard.Data
{
    public class DataService : IDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissoesRepository _permissoesRepository;
        private readonly IEstadoRepository _estadoRepository;
        private readonly ICidadeRepository _cidadeRepository;

        public DataService(ApplicationDbContext context, 
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPermissoesRepository permissoesRepository,
            IEstadoRepository estadoRepository,
            ICidadeRepository cidadeRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _permissoesRepository = permissoesRepository;
            _estadoRepository = estadoRepository;
            _cidadeRepository = cidadeRepository;
        }

        public async Task InitDB()
        {
            await _context.Database.MigrateAsync();
            await _roleRepository.InitRole();
            await _userRepository.InitUser();
            await _permissoesRepository.InitPermissoes();
            await _estadoRepository.InitDbEstado();
            await _cidadeRepository.InitDbCidade();
        }
    }
}
