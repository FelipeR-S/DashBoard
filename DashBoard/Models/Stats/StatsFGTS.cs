using DashBoard.Models.Enums;
using DashBoard.Repositories;

namespace DashBoard.Models.Stats
{
    public class StatsFGTS : StatsPadraoModelo
    {
        private Stats _possui { get; set; }
        private Stats _naoPossui { get; set; }
        private Stats _naoDeclarado { get; set; }

        public StatsFGTS(int quantidadeCadastro, ICadastroRepository repositorio)
        {
            QuantidadeDeCadastro = quantidadeCadastro;
            _possui = StartStats(repositorio, TipoFiltro.FGTS, "true");
            _naoPossui = StartStats(repositorio, TipoFiltro.FGTS, "false");
            _naoDeclarado = StartStats(repositorio, TipoFiltro.FGTS);
        }

        public Stats Possui { get { return _possui; } }
        public Stats NaoPossui { get { return _naoPossui; } }
        public Stats NaoDeclarado { get { return _naoDeclarado; } }
    }
}
