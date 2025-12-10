using DashBoard.Models.Enums;
using DashBoard.Repositories;

namespace DashBoard.Models.Stats
{
    public class StatsGenero : StatsPadraoModelo
    {
        private Stats _masculino { get; set; }
        private Stats _feminino { get; set; }
        private Stats _outros { get; set; }
        private Stats _naoDeclarado { get; set; }
        public StatsGenero(int quantidadeCadastro, ICadastroRepository repository)
        {
            QuantidadeDeCadastro = quantidadeCadastro;
            _masculino = StartStats(repository, TipoFiltro.Genero, Genero.Masculino);
            _feminino = StartStats(repository, TipoFiltro.Genero, Genero.Feminino);
            _outros = StartStats(repository, TipoFiltro.Genero, Genero.Outro);
            _naoDeclarado = StartStats(repository, TipoFiltro.Genero, Genero.NaoDeclarado);
        }
        public Stats Masculino { get { return _masculino;  } }
        public Stats Feminino { get { return _feminino; } }
        public Stats Outros { get { return _outros; } }
        public Stats NaoDeclarado { get { return _naoDeclarado; } }
    }
}
