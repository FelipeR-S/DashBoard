using DashBoard.Models.Enums;

namespace DashBoard.Models
{
    public class ClienteFiltroModelo
    {
        public Filhos? Filhos { get; set; }
        public Renda? Renda { get; set; }
        public EstadoCivil? EstadoCivil { get; set; }
        public Genero? Genero { get; set; }
        public bool? FGTS { get; set; }
        public int? EstadoId { get; set; }
        public int? CidadeId { get; set; }
        public int? IdadeDe { get; set; }
        public int? IdadeAte { get; set; }
        public DateTime? DataCadastroDe { get; set; }
        public DateTime? DataCadastroAte { get; set; }
        public int Pagina { get; set; }
        public int QuantidadePorPagina { get; set; } = 10;
    }

    public class PaginadoInfo
    {
        public int PaginaAtual { get; set; }
        public int QuantidadePorPagina { get; set; } = 10;
        public int TotalDePaginas 
        { 
            get => QuantidadePorPagina > 0 
                ? (TotalDeRegistros / QuantidadePorPagina) 
                : 0; 
        }
        public int TotalDeRegistros { get; set; }
    }
}
