using DashBoard.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace DashBoard.Models
{

    [DataContract]
    public abstract class BaseModel
    {
        [DataMember]
        public int Id { get; set; }
    }

    public class Cliente : BaseModel
    {
        public Cliente()
        {
        }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [Key]
        public string Telefone { get; set; } = string.Empty;
        public DateTime? Nascimento { get; set; }
        public int? Estado { get; set; }
        public int? Cidade { get; set; }
        public string? Bairro { get; set; }
        public int Renda { get; set; }
        public bool? FGTS { get; set; }
        public int Filhos { get; set; }
        public int Genero { get; set; }
        public int EstadoCivil { get; set; }
        public DateTime DataCadastro { get; private set; } = DateTime.Now;
        public int QuantidadeEnvios { get; set; }
        [NotMapped]
        public int QuantidadeDeRegistros { get; set; }
        public void UpdateDataCadastro(DateTime cadastro) => DataCadastro = cadastro;
    }

    public class NewsLetter : BaseModel
    {
        public NewsLetter()
        {
        }

        [Key]
        public string Email { get; set; } = string.Empty;
    }

    public class User : BaseModel
    {
        public User()
        {
        }
        [Key]
        public string Matricula { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? Nome { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        [NotMapped]
        public string SenhaConfirmar { get; set; } = string.Empty;
        public string TokenRenovacao { get; set; } = string.Empty;
        public DateTime? ExpiraToken { get; set; }
        public string? Telefone { get; set; }
        public string? Endereco { get; set; }
        public DateTime DataCadastro { get; private set; } = DateTime.Now;
        public string Tema { get; set; } = "light-blue";
        public int Aviso { get; set; }
        public int QuantidadeEnvios { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }

    public class Role : BaseModel
    {
        public Role()
        {
        }

        public string Nome { get; set; } = string.Empty;
        [IgnoreDataMember]
        public List<Permissoes> Permissoes { get; set; } = new List<Permissoes>();
    }

    public class Permissoes : BaseModel
    {
        public Permissoes()
        {
        }

        [DataMember]
        public bool Salvar { get; set; }
        [DataMember]
        public bool Editar { get; set; }
        [DataMember]
        public bool Excluir { get; set; }
        [DataMember]
        public bool Visualizar { get; set; }
        [DataMember]
        public Atividade Atividade { get; set; }
        [DataMember]
        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }

    public class Estado : BaseModel
    {
        public Estado(string uF, string estadoNome)
        {
            UF = uF;
            EstadoNome = estadoNome;
        }

        [DataMember]
        public string UF { get; set; }
        [DataMember]
        public string EstadoNome { get; set; }
        [IgnoreDataMember]
        public List<Cidade>? Cidades { get; set; }
    }

    public class Cidade : BaseModel
    {
        public Cidade(string cidadeNome, int estadoId)
        {
            CidadeNome = cidadeNome;
            EstadoId = estadoId;
        }
        [DataMember]
        public string CidadeNome { get; set; }
        [DataMember]
        public int EstadoId { get; set; }
        [IgnoreDataMember]
        public Estado? Estado { get; set; }
    }

    public class RequestEmail
    {
        public TipoEmail Tipo { get; set; }
        public string Assunto { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public string TemplateHtml { get; set; } = string.Empty;
    }
}
