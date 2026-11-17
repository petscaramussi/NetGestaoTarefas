using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GestaoTarefas.Application.DTOs;
using GestaoTarefas.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace GestaoTarefas.IntegrationTests;

public class TasksEndpointsTests : IClassFixture<ApiFactory>
{
    private const string BaseUrl = "/api/tasks";
    private readonly HttpClient _client;

    public TasksEndpointsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<TaskResponse> CreateAsync(object body)
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, body, ApiFactory.JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskResponse>(ApiFactory.JsonOptions))!;
    }

    [Fact]
    public async Task Post_WithValidBody_Returns201WithLocationAndId()
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { title = "Nova tarefa", status = "InProgress" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TaskResponse>(ApiFactory.JsonOptions);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(TaskItemStatus.InProgress, created.Status);
        Assert.EndsWith($"{BaseUrl}/{created.Id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Post_WithoutTitle_Returns400WithValidationErrors()
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { description = "sem título" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("Title", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Post_WithUnknownStatus_Returns400WithPortugueseMessage()
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { title = "Tarefa", status = "Arquivada" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal("Um ou mais erros de validação ocorreram.", problem!.Title);
        Assert.Contains("status", problem.Errors.Keys);
    }

    [Fact]
    public async Task Get_UnknownRoute_Returns404WithPortugueseTitle()
    {
        var response = await _client.GetAsync("/api/inexistente");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Recurso não encontrado.", problem!.Title);
    }

    [Fact]
    public async Task Get_ById_WhenExists_Returns200()
    {
        var created = await CreateAsync(new { title = "Buscar" });

        var response = await _client.GetAsync($"{BaseUrl}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(ApiFactory.JsonOptions);
        Assert.Equal("Buscar", task!.Title);
    }

    [Fact]
    public async Task Get_ById_WhenMissing_Returns404ProblemDetails()
    {
        var response = await _client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_WithFilters_ReturnsOnlyMatchingTasks()
    {
        await CreateAsync(new { title = "Filtro-alvo", status = "Completed", dueDate = "10-03-2099" });
        await CreateAsync(new { title = "Filtro-outro-status", status = "Pending", dueDate = "10-03-2099" });
        await CreateAsync(new { title = "Filtro-outra-data", status = "Completed", dueDate = "10-04-2099" });

        var tasks = await _client.GetFromJsonAsync<List<TaskResponse>>(
            $"{BaseUrl}?status=Completed&dueDateFrom=01-03-2099&dueDateTo=31-03-2099", ApiFactory.JsonOptions);

        Assert.Equal("Filtro-alvo", Assert.Single(tasks!).Title);
    }

    [Fact]
    public async Task Get_WithInvalidDateRange_Returns400()
    {
        var response = await _client.GetAsync($"{BaseUrl}?dueDateFrom=01-02-2026&dueDateTo=01-01-2026");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ReturnsDatesInBrazilianFormat()
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { title = "Formato", dueDate = "31-12-2099" });

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("31-12-2099", json.GetProperty("dueDate").GetString());
        Assert.Matches(@"^\d{2}-\d{2}-\d{4} \d{2}:\d{2}:\d{2}$", json.GetProperty("createdAt").GetString());
    }

    [Fact]
    public async Task Post_WithIsoDate_Returns400()
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { title = "Formato", dueDate = "2099-12-31" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal("Data inválida para 'dueDate'. Use o formato dd-MM-yyyy.", problem!.Errors["dueDate"].Single());
    }

    [Fact]
    public async Task Get_WithIsoDateInQuery_Returns400()
    {
        var response = await _client.GetAsync($"{BaseUrl}?dueDate=2099-12-31");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Put_WhenExists_Returns200WithUpdatedTask()
    {
        var created = await CreateAsync(new { title = "Antes" });

        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{created.Id}",
            new { title = "Depois", description = "editada", status = "Completed", dueDate = "01-01-2099" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TaskResponse>(ApiFactory.JsonOptions);
        Assert.Equal("Depois", updated!.Title);
        Assert.Equal(TaskItemStatus.Completed, updated.Status);
        Assert.Equal(new DateOnly(2099, 1, 1), updated.DueDate);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task Put_WhenMissing_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{Guid.NewGuid()}", new { title = "X", status = "Pending" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenExists_Returns204AndTaskIsGone()
    {
        var created = await CreateAsync(new { title = "Remover" });

        var deleteResponse = await _client.DeleteAsync($"{BaseUrl}/{created.Id}");
        var getResponse = await _client.GetAsync($"{BaseUrl}/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenMissing_Returns404()
    {
        var response = await _client.DeleteAsync($"{BaseUrl}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_DocumentIsAvailable()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
