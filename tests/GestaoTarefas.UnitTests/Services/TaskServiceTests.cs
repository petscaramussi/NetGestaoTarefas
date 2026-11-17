using FluentValidation;
using GestaoTarefas.Application.Abstractions;
using GestaoTarefas.Application.DTOs;
using GestaoTarefas.Application.Exceptions;
using GestaoTarefas.Application.Services;
using GestaoTarefas.Application.Validators;
using GestaoTarefas.Domain.Entities;
using GestaoTarefas.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace GestaoTarefas.UnitTests.Services;

public class TaskServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly FakeTimeProvider _timeProvider = new(Now);
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _sut = new TaskService(
            _repository,
            new CreateTaskRequestValidator(_timeProvider),
            new UpdateTaskRequestValidator(),
            new TaskFilterValidator(),
            _timeProvider,
            NullLogger<TaskService>.Instance);
    }

    private static TaskItem NewTask(string title = "Tarefa") =>
        new(title, null, null, TaskItemStatus.Pending, Now.UtcDateTime.AddDays(-1));

    [Fact]
    public async Task CreateAsync_WithValidRequest_PersistsAndReturnsTaskWithUniqueId()
    {
        var request = new CreateTaskRequest
        {
            Title = "  Estudar EF Core  ",
            Description = "InMemory provider",
            DueDate = new DateOnly(2026, 6, 20),
            Status = TaskItemStatus.InProgress
        };

        var result = await _sut.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Estudar EF Core", result.Title);
        Assert.Equal("InMemory provider", result.Description);
        Assert.Equal(new DateOnly(2026, 6, 20), result.DueDate);
        Assert.Equal(TaskItemStatus.InProgress, result.Status);
        Assert.Equal(Now.UtcDateTime, result.CreatedAt);
        Assert.Null(result.UpdatedAt);
        await _repository.Received(1).AddAsync(Arg.Is<TaskItem>(t => t.Id == result.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_CalledTwice_GeneratesDifferentIds()
    {
        var request = new CreateTaskRequest { Title = "Tarefa" };

        var first = await _sut.CreateAsync(request);
        var second = await _sut.CreateAsync(request);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyTitle_ThrowsValidationExceptionAndDoesNotPersist()
    {
        var request = new CreateTaskRequest { Title = "" };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(request));

        Assert.Contains(exception.Errors, e => e.PropertyName == nameof(CreateTaskRequest.Title));
        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task CreateAsync_WithPastDueDate_ThrowsValidationException()
    {
        var request = new CreateTaskRequest { Title = "Tarefa", DueDate = new DateOnly(2026, 6, 14) };

        var exception = await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(request));

        Assert.Contains(exception.Errors, e => e.PropertyName == nameof(CreateTaskRequest.DueDate));
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskExists_ReturnsTask()
    {
        var task = NewTask();
        _repository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);

        var result = await _sut.GetByIdAsync(task.Id);

        Assert.Equal(task.Id, result.Id);
        Assert.Equal(task.Title, result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskDoesNotExist_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListAsync_ForwardsFilterToRepositoryAndMapsResult()
    {
        var filter = new TaskFilter { Status = TaskItemStatus.Pending, DueDateFrom = new DateOnly(2026, 1, 1) };
        var tasks = new List<TaskItem> { NewTask("A"), NewTask("B") };
        _repository.ListAsync(filter, Arg.Any<CancellationToken>()).Returns(tasks);

        var result = await _sut.ListAsync(filter);

        Assert.Equal(["A", "B"], result.Select(t => t.Title));
    }

    [Fact]
    public async Task ListAsync_WithInvalidDateRange_ThrowsValidationException()
    {
        var filter = new TaskFilter { DueDateFrom = new DateOnly(2026, 2, 1), DueDateTo = new DateOnly(2026, 1, 1) };

        await Assert.ThrowsAsync<ValidationException>(() => _sut.ListAsync(filter));
        await _repository.DidNotReceiveWithAnyArgs().ListAsync(default!, default);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskExists_UpdatesAllFieldsAndPersists()
    {
        var task = NewTask();
        _repository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        var request = new UpdateTaskRequest
        {
            Title = "Novo título",
            Description = "Nova descrição",
            DueDate = new DateOnly(2026, 7, 1),
            Status = TaskItemStatus.Completed
        };

        var result = await _sut.UpdateAsync(task.Id, request);

        Assert.Equal(task.Id, result.Id);
        Assert.Equal("Novo título", result.Title);
        Assert.Equal("Nova descrição", result.Description);
        Assert.Equal(new DateOnly(2026, 7, 1), result.DueDate);
        Assert.Equal(TaskItemStatus.Completed, result.Status);
        Assert.Equal(Now.UtcDateTime, result.UpdatedAt);
        await _repository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskDoesNotExist_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TaskItem?)null);
        var request = new UpdateTaskRequest { Title = "Título", Status = TaskItemStatus.Pending };

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(Guid.NewGuid(), request));
        await _repository.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidRequest_ThrowsValidationExceptionBeforeQueryingRepository()
    {
        var request = new UpdateTaskRequest { Title = "", Status = TaskItemStatus.Pending };

        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(Guid.NewGuid(), request));
        await _repository.DidNotReceiveWithAnyArgs().GetByIdAsync(default, default);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskExists_RemovesTask()
    {
        var task = NewTask();
        _repository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);

        await _sut.DeleteAsync(task.Id);

        await _repository.Received(1).DeleteAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskDoesNotExist_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((TaskItem?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid()));
        await _repository.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
    }
}
