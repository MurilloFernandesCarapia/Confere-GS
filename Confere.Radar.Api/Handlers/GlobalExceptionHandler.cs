using Confere.Radar.Domain.Constants;
using Confere.Radar.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Confere.Radar.Api.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var erro = Mapear(exception);

            if (erro.Status >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Erro inesperado em {Metodo} {Caminho}", httpContext.Request.Method, httpContext.Request.Path);
            }
            else
            {
                _logger.LogWarning("Requisição {Metodo} {Caminho} recusada com {Status} ({Codigo}): {Mensagem}",
                    httpContext.Request.Method, httpContext.Request.Path, erro.Status, erro.Codigo, exception.Message);
            }

            var problema = new ProblemDetails
            {
                Status = erro.Status,
                Title = erro.Titulo,
                Detail = erro.Detalhe,
                Instance = httpContext.Request.Path
            };

            problema.Extensions["codigo"] = erro.Codigo;

            if (exception is ValidacaoException validacao)
            {
                problema.Extensions["erros"] = validacao.Erros;
            }

            httpContext.Response.StatusCode = erro.Status;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problema
            });
        }

        private static ErroMapeado Mapear(Exception exception)
        {
            return exception switch
            {
                ValidacaoException e => new ErroMapeado(StatusCodes.Status400BadRequest, e.Codigo, "Dados inválidos", e.Message),
                CredenciaisInvalidasException e => new ErroMapeado(StatusCodes.Status401Unauthorized, e.Codigo, "Não autenticado", e.Message),
                SemPermissaoException e => new ErroMapeado(StatusCodes.Status403Forbidden, e.Codigo, "Sem permissão", e.Message),
                ConsentimentoPendenteException e => new ErroMapeado(StatusCodes.Status403Forbidden, e.Codigo, "Consentimento pendente", e.Message),
                NaoEncontradoException e => new ErroMapeado(StatusCodes.Status404NotFound, e.Codigo, "Não encontrado", e.Message),
                ConflitoException e => new ErroMapeado(StatusCodes.Status409Conflict, e.Codigo, "Conflito", e.Message),
                IdempotenciaCorpoDiferenteException e => new ErroMapeado(StatusCodes.Status422UnprocessableEntity, e.Codigo, "Requisição repetida com corpo diferente", e.Message),
                BadHttpRequestException => new ErroMapeado(StatusCodes.Status400BadRequest, CodigoErro.Validacao, "Dados inválidos", "O corpo ou os parâmetros da requisição estão em um formato inválido."),
                DbUpdateException => new ErroMapeado(StatusCodes.Status409Conflict, CodigoErro.Conflito, "Conflito", "A operação viola uma regra do banco de dados, como um registro duplicado."),
                _ => new ErroMapeado(StatusCodes.Status500InternalServerError, CodigoErro.ErroInterno, "Erro interno", "Ocorreu um erro inesperado. Tente novamente em instantes.")
            };
        }

        private record ErroMapeado(int Status, string Codigo, string Titulo, string Detalhe);
    }
}
