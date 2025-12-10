using DashBoard.Models.Enums;
using DashBoard.Repositories;
using System.Reflection.Metadata;

namespace DashBoard.Models.Stats
{
    public abstract class StatsPadraoModelo
    {
        public int QuantidadeDeCadastro { get; set; }

        protected Stats StartStats(ICadastroRepository repositorio, TipoFiltro tipoFiltro, string? parametro = null)
        {
            bool naoDeclarado = false;

            if (string.IsNullOrEmpty(parametro))
                naoDeclarado = true;
            var quantidade = 0;
            try
            {
                quantidade = repositorio.GetQuantidadeCadastro(tipoFiltro, naoDeclarado, parametro).GetAwaiter().GetResult();
            }
            catch { }
            var percentual = QuantidadeDeCadastro > 0 ? (quantidade * 100) / QuantidadeDeCadastro : 0;
            return new Stats(quantidade, percentual);
        }

        protected Stats StartStats<T>(ICadastroRepository repositorio, TipoFiltro tipoFiltro, T enumParametro) where T : Enum
        {
            var quantidade = 0;
            try
            {
                quantidade = repositorio.GetQuantidadeCadastro(tipoFiltro, enumParametro).GetAwaiter().GetResult();
            }
            catch { }
            var percentual = QuantidadeDeCadastro > 0 ? (quantidade * 100) / QuantidadeDeCadastro : 0;
            return new Stats(quantidade, percentual);
        }
    }
}
