using System.Text.Json;
using Api.Contracts;
using Api.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.Tests;

public class ApiContractExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WithInvalidPaginationException_WritesBadRequestProblemDetailsInSpanish()
    {
        var handler = new ApiContractExceptionHandler(NullLogger<ApiContractExceptionHandler>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new InvalidPaginationException("El parámetro 'page' debe ser mayor o igual a 1.");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem!.Status);
        Assert.Equal("Paginación inválida", problem.Title);
        Assert.Equal(exception.Message, problem.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_WithUnmappedException_ReturnsFalse()
    {
        var handler = new ApiContractExceptionHandler(NullLogger<ApiContractExceptionHandler>.Instance);
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        var handled = await handler.TryHandleAsync(context, new InvalidOperationException(), CancellationToken.None);

        Assert.False(handled);
    }
}
