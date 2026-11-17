using GestaoTarefas.Application.DTOs;
using GestaoTarefas.Domain.Entities;
using GestaoTarefas.Domain.Enums;
using GestaoTarefas.Infrastructure.Persistence;
using GestaoTarefas.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GestaoTarefas.UnitTests.Repositories;

public class TaskRepositoryTests : IDisposable
{
    private static readonly DateTime CreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly AppDbContext _context;
    private readonly TaskRepository _sut;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _sut = new TaskRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private async Task<TaskItem> SeedAsync(string title, TaskItemStatus status, DateOnly? dueDate)
    {
        var task = new TaskItem(title, null, dueDate, status, CreatedAt);
        await _sut.AddAsync(task);
        return task;
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsPersistedTask()
    {
        var task = await SeedAsync("Tarefa", TaskItemStatus.Pending, new DateOnly(2026, 5, 1));

        var found = await _sut.GetByIdAsync(task.Id);

        Assert.NotNull(found);
        Assert.Equal("Tarefa", found.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        Assert.Null(await _sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListAsync_WithoutFilter_ReturnsAllOrderedByDueDateWithNullsLast()
    {
        await SeedAsync("Sem data", TaskItemStatus.Pending, null);
        await SeedAsync("Março", TaskItemStatus.Pending, new DateOnly(2026, 3, 1));
        await SeedAsync("Janeiro", TaskItemStatus.Completed, new DateOnly(2026, 1, 1));

        var result = await _sut.ListAsync(new TaskFilter());

        Assert.Equal(["Janeiro", "Março", "Sem data"], result.Select(t => t.Title));
    }

    [Fact]
    public async Task ListAsync_FilteringByStatus_ReturnsOnlyMatchingTasks()
    {
        await SeedAsync("A", TaskItemStatus.Pending, null);
        await SeedAsync("B", TaskItemStatus.InProgress, null);
        await SeedAsync("C", TaskItemStatus.InProgress, null);

        var result = await _sut.ListAsync(new TaskFilter { Status = TaskItemStatus.InProgress });

        Assert.Equal(["B", "C"], result.Select(t => t.Title).Order());
    }

    [Fact]
    public async Task ListAsync_FilteringByExactDueDate_ReturnsOnlyMatchingTasks()
    {
        await SeedAsync("A", TaskItemStatus.Pending, new DateOnly(2026, 5, 1));
        await SeedAsync("B", TaskItemStatus.Pending, new DateOnly(2026, 5, 2));
        await SeedAsync("C", TaskItemStatus.Pending, null);

        var result = await _sut.ListAsync(new TaskFilter { DueDate = new DateOnly(2026, 5, 1) });

        Assert.Equal("A", Assert.Single(result).Title);
    }

    [Fact]
    public async Task ListAsync_FilteringByDueDateRange_ReturnsTasksWithinInclusiveRange()
    {
        await SeedAsync("Antes", TaskItemStatus.Pending, new DateOnly(2026, 4, 30));
        await SeedAsync("Inicio", TaskItemStatus.Pending, new DateOnly(2026, 5, 1));
        await SeedAsync("Fim", TaskItemStatus.Pending, new DateOnly(2026, 5, 31));
        await SeedAsync("Depois", TaskItemStatus.Pending, new DateOnly(2026, 6, 1));
        await SeedAsync("Sem data", TaskItemStatus.Pending, null);

        var result = await _sut.ListAsync(new TaskFilter
        {
            DueDateFrom = new DateOnly(2026, 5, 1),
            DueDateTo = new DateOnly(2026, 5, 31)
        });

        Assert.Equal(["Inicio", "Fim"], result.Select(t => t.Title));
    }

    [Fact]
    public async Task ListAsync_CombiningStatusAndDueDate_AppliesBothFilters()
    {
        await SeedAsync("A", TaskItemStatus.Pending, new DateOnly(2026, 5, 1));
        await SeedAsync("B", TaskItemStatus.Completed, new DateOnly(2026, 5, 1));
        await SeedAsync("C", TaskItemStatus.Completed, new DateOnly(2026, 5, 2));

        var result = await _sut.ListAsync(new TaskFilter
        {
            Status = TaskItemStatus.Completed,
            DueDate = new DateOnly(2026, 5, 1)
        });

        Assert.Equal("B", Assert.Single(result).Title);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var task = await SeedAsync("Original", TaskItemStatus.Pending, null);

        task.Update("Alterada", "desc", new DateOnly(2026, 8, 1), TaskItemStatus.Completed, CreatedAt.AddDays(1));
        await _sut.UpdateAsync(task);
        _context.ChangeTracker.Clear();

        var found = await _sut.GetByIdAsync(task.Id);
        Assert.NotNull(found);
        Assert.Equal("Alterada", found.Title);
        Assert.Equal(TaskItemStatus.Completed, found.Status);
        Assert.Equal(new DateOnly(2026, 8, 1), found.DueDate);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTask()
    {
        var task = await SeedAsync("Remover", TaskItemStatus.Pending, null);

        await _sut.DeleteAsync(task);

        Assert.Null(await _sut.GetByIdAsync(task.Id));
    }
}
