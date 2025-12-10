using DashBoard.Models.Enums;
using DashBoard.Repositories;

namespace DashBoard.Models.Stats
{
    public class StatsIdade : StatsPadraoModelo
    {
        private Stats _ate_18_Anos { get; set; }
        private Stats _entre_19_25_anos { get; set; }
        private Stats _entre_26_35_anos { get; set; }
        private Stats _entre_36_55_anos { get; set; }
        private Stats _maior_55_anos { get; set; }
        private Stats _naoDeclarado { get; set; }

        public StatsIdade(int quantidadeCadastro, ICadastroRepository repository)
        {
            QuantidadeDeCadastro = quantidadeCadastro;
            _ate_18_Anos = StartStats(repository, TipoFiltro.Idade, "0|18");
            _entre_19_25_anos = StartStats(repository, TipoFiltro.Idade, "19|25");
            _entre_26_35_anos = StartStats(repository, TipoFiltro.Idade, "26|35");
            _entre_36_55_anos = StartStats(repository, TipoFiltro.Idade, "36|55");
            _maior_55_anos = StartStats(repository, TipoFiltro.Idade, "55|1000");
            _naoDeclarado = StartStats(repository, TipoFiltro.Idade);
        }

        public Stats Ate_18_Anos { get { return _ate_18_Anos; } }
        public Stats Entre_19_25_anos { get { return _entre_19_25_anos; } }
        public Stats Entre_26_35_anos { get { return _entre_26_35_anos; } }
        public Stats Entre_36_55_anos { get { return _entre_36_55_anos; } }
        public Stats Maior_55_anos { get { return _maior_55_anos; } }
        public Stats NaoDeclarado { get { return _naoDeclarado; } }
    }
}
