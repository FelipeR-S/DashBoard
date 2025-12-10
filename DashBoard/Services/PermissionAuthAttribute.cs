using DashBoard.Controllers;
using DashBoard.Models;
using DashBoard.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DashBoard.Services
{
    public class PermissionAuthAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public PermissionAuthAttribute(Atividade atividade, Acoes acao)
        {
            _atividade = atividade;
            _acao = acao;
        }

        private Atividade _atividade { get; }
        private Acoes _acao { get; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context != null)
            {
                var resultMessage = "Permissões invalidas ou inexistentes!";
                IActionResult? result = context.Result;

                var permissoesUsuario = context.HttpContext.Session.Get<List<Permissoes>>("PermissoesCache");

                if (permissoesUsuario != null && permissoesUsuario.Any())
                {
                    var permissao = permissoesUsuario.FirstOrDefault(p => p.Atividade == _atividade);
                    if (permissao != null)
                    {
                        bool permite = _acao switch
                        {
                            Acoes.Salvar => permissao.Salvar,
                            Acoes.Editar => permissao.Editar,
                            Acoes.Excluir => permissao.Excluir,
                            Acoes.Visualizar => permissao.Visualizar,
                            _ => false
                        };

                        if (permite)
                            return;

                        resultMessage = $"Usuário não tem permissão de {_acao} em {_atividade}";
                    }
                }
                if (_acao == Acoes.Visualizar)
                    context.Result = new RedirectToActionResult(
                        "PermissaoError",
                        "Erro",
                        new { message = resultMessage }
                    );
                else
                    context.Result = new ContentResult
                    {
                        Content = resultMessage,
                        StatusCode = StatusCodes.Status403Forbidden,
                        ContentType = "text/plain"
                    };
            }
        }

    }
}
