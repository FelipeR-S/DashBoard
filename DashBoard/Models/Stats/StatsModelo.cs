using DashBoard.Repositories;

namespace DashBoard.Models.Stats
{
    public class StatsModelo : StatsPadraoModelo
    {
        public StatsModelo(int quantidadeDeCadastros, ICadastroRepository repositorio)
        {
            QuantidadeDeCadastro = quantidadeDeCadastros;
            StatsEstadoCivil = new StatsEstadoCivil(quantidadeDeCadastros, repositorio);
            StatsFGTS = new StatsFGTS(quantidadeDeCadastros, repositorio);
            StatsGenero = new StatsGenero(quantidadeDeCadastros, repositorio);
            StatsIdade = new StatsIdade(quantidadeDeCadastros, repositorio);
            StatsPeriodo = new StatsPeriodo(quantidadeDeCadastros, repositorio);
            StatsRenda = new StatsRenda(quantidadeDeCadastros, repositorio);
            StatsEstadosCidades = new StatsEstadosCidades(quantidadeDeCadastros, repositorio);
        }

        public StatsEstadoCivil StatsEstadoCivil { get; }
        public StatsFGTS StatsFGTS { get; }
        public StatsGenero StatsGenero { get; }
        public StatsIdade StatsIdade { get; }
        public StatsPeriodo StatsPeriodo { get; }
        public StatsRenda StatsRenda { get; }
        public StatsEstadosCidades StatsEstadosCidades { get; }
    }

    public class Stats
    {
        private int _quantidade { get; set; }
        public decimal _percentual { get; set; }
        public Stats(int quantidade, decimal percentual)
        {
            _quantidade = quantidade;
            _percentual = percentual;
        }
        public int Quantidade { get { return _quantidade; } }
        public decimal Percentual { get { return _percentual; } }
    }
}
