using Confere.Radar.Domain.Exceptions;
using FluentValidation;

namespace Confere.Radar.Api.Filters
{
    public class ValidacaoFilter<T> : IEndpointFilter where T : class
    {
        private readonly IValidator<T> _validator;

        public ValidacaoFilter(IValidator<T> validator)
        {
            _validator = validator;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var requisicao = context.Arguments.OfType<T>().FirstOrDefault();
            if (requisicao is null)
            {
                throw new ValidacaoException("corpo", "O corpo da requisição é obrigatório.");
            }

            var resultado = await _validator.ValidateAsync(requisicao);
            if (!resultado.IsValid)
            {
                var erros = resultado.Errors
                    .Select(e => new ErroCampo(ParaCamelCase(e.PropertyName), e.ErrorMessage))
                    .ToList();

                throw new ValidacaoException(erros);
            }

            return await next(context);
        }

        private static string ParaCamelCase(string nome)
        {
            if (string.IsNullOrEmpty(nome))
            {
                return nome;
            }

            return char.ToLowerInvariant(nome[0]) + nome[1..];
        }
    }
}
