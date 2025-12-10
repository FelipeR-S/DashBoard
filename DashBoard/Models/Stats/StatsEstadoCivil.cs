using DashBoard.Models.Enums;
using DashBoard.Repositories;

namespace DashBoard.Models.Stats
{
    public class StatsEstadoCivil : StatsPadraoModelo
    {
        private Stats _casado { get; set; }
        private Stats _solteiro { get; set; }
        private Stats _divorciado { get; set; }
        private Stats _viuvo { get; set; }
        private Stats _naoDeclarado { get; set; }

        public StatsEstadoCivil(int quantidadeCadastro, ICadastroRepository repository)
        {
            QuantidadeDeCadastro = quantidadeCadastro;
            _casado = StartStats(repository, TipoFiltro.EstadoCivil, EstadoCivil.Casado);
            _solteiro = StartStats(repository, TipoFiltro.EstadoCivil, EstadoCivil.Solteiro);
            _divorciado = StartStats(repository, TipoFiltro.EstadoCivil, EstadoCivil.Divorciado);
            _viuvo = StartStats(repository, TipoFiltro.EstadoCivil, EstadoCivil.Viuvo);
            _naoDeclarado = StartStats(repository, TipoFiltro.EstadoCivil, EstadoCivil.NaoDeclarado);
        }

        public Stats Casado { get { return _casado; } }
        public Stats Solteiro { get { return _solteiro; } }
        public Stats Divorciado { get { return _divorciado; } }
        public Stats Viuvo { get { return _viuvo; } }
        public Stats NaoDeclarado { get { return _naoDeclarado; } }
    }
}
