using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Runtime.Serialization;

namespace DashBoard.Models.Enums
{
    public enum Atividade
    {
        [EnumMember(Value = "Início")]
        Inicio = 0,
        [EnumMember(Value = "Enviar E-mails")]
        Emails = 1,
        [EnumMember(Value = "Configurações")]
        Configs = 2,
        [EnumMember(Value = "Tabelas")]
        Tabelas = 3,
    }

    public enum Acoes
    {
        Salvar = 0,
        Editar = 1,
        Excluir = 2,
        Visualizar = 3,
    }

    public enum TipoFiltro
    {
        Total = 0,
        Idade = 1,
        Periodo = 2,
        Renda = 3,
        Genero = 4,
        FGTS = 5,
        EstadoCivil = 6,
        Estado = 7,
        Cidade = 8
    }

    public enum TipoEmail 
    {
        [EnumMember(Value = "Todos via landingpage")]
        LandingPage = 0,
        [EnumMember(Value = "Todos via newsletter")]
        Newsletter = 1,
        [EnumMember(Value = "Todos selecionados")]
        Selecionados = 2
    }

    public enum Renda
    {
        [EnumMember(Value = "Até R$ 2 mil")]
        AteDoisMil = 1,
        [EnumMember(Value = "Entre R$ 2 e R$ 3 mil")]
        EntreDoisETresMil = 2,
        [EnumMember(Value = "Entre R$ 3 e R$ 4 mil")]
        EntreTresEQuatroMil = 3,
        [EnumMember(Value = "Maior que R$ 4 mil")]
        MaiorQueQuatroMil = 4,
        [EnumMember(Value = "Não Informar")]
        NaoDeclarado = 99,
    }

    public enum Genero
    {
        [EnumMember(Value = "Masculino")]
        Masculino = 0,
        [EnumMember(Value = "Feminino")]
        Feminino = 1,
        [EnumMember(Value = "Outro")]
        Outro = 2,
        [EnumMember(Value = "Não Informar")]
        NaoDeclarado = 99,
    }

    public enum EstadoCivil
    {
        [EnumMember(Value = "Solteiro")]
        Solteiro = 0,
        [EnumMember(Value = "Casado")]
        Casado = 1,
        [EnumMember(Value = "Divorciado")]
        Divorciado = 2,
        [EnumMember(Value = "Viúvo")]
        Viuvo = 3,
        [EnumMember(Value = "Não Informar")]
        NaoDeclarado = 99,
    }

    public enum Filhos
    {
        [EnumMember(Value = "Nenhum")]
        Nenhum = 0,
        [EnumMember(Value = "Um")]
        Um = 1,
        [EnumMember(Value = "Dois")]
        Dois = 2,
        [EnumMember(Value = "Três ou mais")]
        TresOuMais = 3,
        [EnumMember(Value = "Não Informar")]
        NaoDeclarado = 99,
    }

    public static class EnumExtension
    {
        public static List<SelectListItem> GetSelectList<T>(bool getNaoDeclarado = false) where T : Enum
        {
            var listItem = new List<SelectListItem>();
            string value = "";
            if (!getNaoDeclarado)
                value = "99";

            listItem.Add(new SelectListItem { Value = value, Text = "Selecione..." });
            var listaEnum = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            if (listaEnum != null && listaEnum.Any())
            {
                foreach (var item in listaEnum)
                {
                    var intEmum = EnumToInt(item);
                    if (intEmum != 99 || intEmum == 99 && getNaoDeclarado)
                        listItem.Add(new SelectListItem { Value = (intEmum).ToString(), Text = GetEnumMemberValue(item) });
                }
            }
            return listItem;
        }

        public static string? GetEnumMemberValue<T>(this T value) where T : Enum
        {
            return typeof(T)
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;
        }

        public static int EnumToInt<T>(this T value) where T : Enum => (int)(object)value;
    }
}
