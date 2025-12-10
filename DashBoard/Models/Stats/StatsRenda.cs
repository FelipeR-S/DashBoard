using DashBoard.Models.Enums;
using DashBoard.Repositories;

namespace DashBoard.Models.Stats
{
    public class StatsRenda : StatsPadraoModelo
    {
        private Stats _maior_4_mil { get; set; }
        private Stats _maior_3_menor_4_mil { get; set; }
        private Stats _maior_2_menor_3_mil { get; set; }
        private Stats _de_1_menor_2_mil { get; set; }
        private Stats _naoDeclarado { get; set; }

        public StatsRenda(int quantidadeCadastro, ICadastroRepository repository)
        {
            QuantidadeDeCadastro = quantidadeCadastro;
            _maior_4_mil = StartStats(repository, TipoFiltro.Renda, Renda.MaiorQueQuatroMil);
            _maior_3_menor_4_mil = StartStats(repository, TipoFiltro.Renda, Renda.EntreTresEQuatroMil);
            _maior_2_menor_3_mil = StartStats(repository, TipoFiltro.Renda, Renda.EntreDoisETresMil);
            _de_1_menor_2_mil = StartStats(repository, TipoFiltro.Renda, Renda.AteDoisMil);
            _naoDeclarado = StartStats(repository, TipoFiltro.Renda, Renda.NaoDeclarado);
        }

        public Stats Maior_4_mil
        {
            get { return _maior_4_mil; }
        }
        public Stats Maior_3_Menor_4_mil
        {
            get { return _maior_3_menor_4_mil; }
        }
        public Stats Maior_2_Menor_3_mil
        {
            get { return _maior_2_menor_3_mil; }
        }
        public Stats De_1_Menor_2_mil
        {
            get { return _de_1_menor_2_mil; }
        }
        public Stats NaoDeclarado
        {
            get { return _naoDeclarado; }
        }
    }
}
