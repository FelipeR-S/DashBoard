using DashBoard.Models;
using DashBoard.Models.Enums;
using DashBoard.Models.Stats;
using DashBoard.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace DashBoard.Services
{
    public static class CacheSystem
    {
        #region .: Variaveis DashBoard :.
        public static List<SelectListItem>? RendaDashboard { get; private set; }
        public static List<SelectListItem>? GeneroDashboard { get; private set; }
        public static List<SelectListItem>? EstadoCivilDashboard { get; private set; }
        public static List<SelectListItem>? FilhosDashboard { get; private set; }
        public static StatsModelo? StatsGerais { get; private set; }
        public static int QuantidadeCadastros { get; private set; }

        #endregion
        #region .: Variaveis LandingPage :.
        public static List<SelectListItem>? RendaLandingpage { get; private set; }
        public static List<SelectListItem>? GeneroLandingpage { get; private set; }
        public static List<SelectListItem>? EstadoCivilLandingpage { get; private set; }
        public static List<SelectListItem>? FilhosLandingpage { get; private set; }
        #endregion
        public static bool GerarCadastrosTeste { get; private set; }
        public static List<Estado>? Estados { get; private set; }
        public static List<Cidade>? Cidades { get; private set; }
        public static void SetGerarCadastrosTeste(bool cadastrar) => GerarCadastrosTeste = cadastrar;

        public static async Task CarregaListaEstadosECidades(IEstadoRepository estadoRepository, ICidadeRepository cidadeRepository)
        {
            if (Estados == null)
                Estados = await estadoRepository.BuscarTodos();

            if (Cidades == null)
                Cidades = await cidadeRepository.BuscarTodos();
        }

        public static List<SelectListItem> GeraSelectEstado()
        {
            var listItems = new List<SelectListItem>();
            listItems.Add(new SelectListItem { Value = "", Text = "UF" });
            if (Estados != null && Estados.Any())
                listItems.AddRange(Estados!.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.UF }));
            return listItems;
        }

        public static void CarregaListasEnums()
        {
            if (RendaDashboard == null)
                RendaDashboard = EnumExtension.GetSelectList<Renda>(true);
            if (RendaLandingpage == null)
                RendaLandingpage = EnumExtension.GetSelectList<Renda>();
            if (GeneroDashboard == null)
                GeneroDashboard = EnumExtension.GetSelectList<Genero>(true);
            if (GeneroLandingpage == null)
                GeneroLandingpage = EnumExtension.GetSelectList<Genero>();
            if (EstadoCivilDashboard == null)
                EstadoCivilDashboard = EnumExtension.GetSelectList<EstadoCivil>(true);
            if (EstadoCivilLandingpage == null)
                EstadoCivilLandingpage = EnumExtension.GetSelectList<EstadoCivil>();
            if (FilhosDashboard == null)
                FilhosDashboard = EnumExtension.GetSelectList<Filhos>(true);
            if (FilhosLandingpage == null)
                FilhosLandingpage = EnumExtension.GetSelectList<Filhos>();
        }
        public static async Task AtualizaStatsInicio(ICadastroRepository cadastroRepository)
        {
            int quantidadeCadastros = await cadastroRepository.GetQuantidadeCadastro(TipoFiltro.Total, false);
            if (StatsGerais == null || QuantidadeCadastros != quantidadeCadastros)
            {
                QuantidadeCadastros = quantidadeCadastros;

                StatsGerais = new StatsModelo(QuantidadeCadastros, cadastroRepository);
                if (Estados != null && Estados.Any())
                    StatsGerais.StatsEstadosCidades.UpdateEstados(Estados);
                if (Cidades != null && Cidades.Any())
                    StatsGerais.StatsEstadosCidades.UpdateCidades(Cidades);
            }
        }
    }
}
