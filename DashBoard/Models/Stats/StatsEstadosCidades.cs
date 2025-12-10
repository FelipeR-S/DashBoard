using DashBoard.Models.Enums;
using DashBoard.Repositories;
using System.Threading.Tasks;

namespace DashBoard.Models.Stats
{
    public class StatsEstadosCidades : StatsPadraoModelo
    {
        private List<StatsEstado> _cadastrosPorEstado { get; set; }
        private List<StatsCidade> _cadastrosPorCidade { get; set; }
        private Stats _cidadeNaoDeclarada { get; set; }
        private Stats _estadosNaoDeclarados { get; set; }

        public StatsEstadosCidades(int quantidadeCadastro, ICadastroRepository repository)
        {
            QuantidadeDeCadastro = quantidadeCadastro;
            _cadastrosPorEstado = SetEstados(repository);
            _cadastrosPorCidade = SetCidade(repository);
            _cidadeNaoDeclarada = StartStats(repository, TipoFiltro.Cidade);
            _estadosNaoDeclarados = StartStats(repository, TipoFiltro.Estado);
        }

        public List<StatsEstado> CadastrosPorEstado { get { return _cadastrosPorEstado; } }
        public List<StatsCidade> CadastrosPorCidade { get { return _cadastrosPorCidade; } }
        public Stats CidadeNaoDeclarada { get {  return _cidadeNaoDeclarada; } }
        public Stats EstadosNaoDeclarados { get { return _estadosNaoDeclarados; } }

        private List<StatsEstado> SetEstados(ICadastroRepository repository)
        {
            List<StatsEstado> lista = new List<StatsEstado>();
            try
            {
                lista = repository.GetQuantidadeCadastroEstado(QuantidadeDeCadastro).GetAwaiter().GetResult();
            }
            catch { }
            return lista;
        }
        private List<StatsCidade> SetCidade(ICadastroRepository repository)
        {
            List<StatsCidade> lista = new List<StatsCidade>();
            try
            {
                lista = repository.GetQuantidadeCadastroCidade(QuantidadeDeCadastro).GetAwaiter().GetResult();
            }
            catch { }
            return lista;
        }

        public void UpdateEstados(List<Estado> estados)
        {
            this._cadastrosPorEstado.ForEach(x =>
            {
                x.SetEstadoUF(estados.Where(e => e.Id == x.EstadoId).FirstOrDefault()!.UF);
                x.SetEstadoNome(estados.Where(e => e.Id == x.EstadoId).FirstOrDefault()!.EstadoNome);
            });
        }

        public void UpdateCidades(List<Cidade> cidades)
        {
            this._cadastrosPorCidade.ForEach(x =>
            {
                x.SetCidadeNome(cidades.Where(e => e.Id == x.CidadeId).FirstOrDefault()!.CidadeNome);
            });
        }
    }

    public class StatsEstado
    {
        private int _estadoId { get; set; }
        private string? _estadoUF { get; set; }
        private string? _estadoNome { get; set; }
        private int _quantidade { get; set; }
        public decimal _percentual { get; set; }
        public StatsEstado(int estadoId, int quantidade, int total)
        {
            _estadoId = estadoId;
            _quantidade = quantidade;
            _percentual = total > 0 ? (quantidade * 100) / total : 0;
        }

        public int EstadoId { get { return _estadoId; } }
        public string? EstadoUF { get { return _estadoUF; } }
        public string? EstadoNome { get { return _estadoNome; } }
        public int Quantidade { get { return _quantidade; } }
        public decimal Percentual { get { return _percentual; } }

        public void SetEstadoUF(string UF) => _estadoUF = UF; 
        public void SetEstadoNome(string nome) => _estadoNome = nome;
    }

    public class StatsCidade
    {
        private int _cidadeId { get; set; }
        private string? _cidadeNome { get; set; }
        private int _quantidade { get; set; }
        public decimal _percentual { get; set; }
        public StatsCidade(int cidadeId, int quantidade, int total)
        {
            _cidadeId = cidadeId;
            _quantidade = quantidade;
            _percentual = total > 0 ? (quantidade * 100) / total : 0;
        }

        public int CidadeId { get { return _cidadeId; } }
        public string? CidadeNome { get { return _cidadeNome; } }
        public int Quantidade { get { return _quantidade; } }
        public decimal Percentual { get { return _percentual; } }
        public void SetCidadeNome(string nome) => _cidadeNome = nome;
    }
}
