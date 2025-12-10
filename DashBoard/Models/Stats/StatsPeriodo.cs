using DashBoard.Models.Enums;
using DashBoard.Repositories;

namespace DashBoard.Models.Stats
{
    public class StatsPeriodo : StatsPadraoModelo
    {
        private List<StatsCadastroMesAno> _statsPeriodoMesAno { get; set; } = new List<StatsCadastroMesAno>();

        public StatsPeriodo(int quantidadeCadastro, ICadastroRepository repository)
        {
            QuantidadeDeCadastro = quantidadeCadastro;
            FiltroDe = DateTime.Now.AddMonths(-6);
            _statsPeriodoMesAno = SetPeriodo(repository);
        }
        public DateTime FiltroDe { get; set; }
        public List<StatsCadastroMesAno> StatsPeriodoMesAno { get { return _statsPeriodoMesAno; } }

        private List<StatsCadastroMesAno> SetPeriodo(ICadastroRepository repository)
        {
            var listaPeriodo = new List<StatsCadastroMesAno>();

            var de = new DateTime(FiltroDe.Year, FiltroDe.Month, 1);
            var ate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            while (de <= ate)
            {
                int ano = de.Year;
                int mes = de.Month;
                int dias = DateTime.DaysInMonth(ano, mes);
                var parametro = $"{de.Year}-{de.Month}-{01}|{de.Year}-{de.Month}-{dias}";
                var stats = StartStats(repository, TipoFiltro.Periodo, parametro);

                var statPeriodo = new StatsCadastroMesAno(stats, mes, ano);
                listaPeriodo.Add(statPeriodo);
                de = de.AddMonths(1);
            }

            return listaPeriodo;
        }

        public void AtualizaPeriodo(ICadastroRepository repository)
        {
            _statsPeriodoMesAno = SetPeriodo(repository);
        }
    }
    public class StatsCadastroMesAno 
    {
        private int _mes { get; set; }
        private int _ano { get; set; }
        private Stats _cadastrosPeriodo { get; set; }

        public StatsCadastroMesAno(Stats stats, int mes, int ano)
        {
            _cadastrosPeriodo = stats;
            _mes = mes;
            _ano = ano;
        }

        public int Mes { get { return _mes; } }
        public int Ano { get { return _ano; } }
        public Stats CadastrosPeriodo { get { return _cadastrosPeriodo; } }
    }
}
