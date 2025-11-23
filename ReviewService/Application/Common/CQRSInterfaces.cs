using MediatR;

namespace ReviewService.Application.Common;

/// <summary>
///маркерний інтерфейс для команд (операції запису)
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
///маркерний інтерфейс для команд з відповіддю
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
///маркерний інтерфейс для запитів (операції читання)
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}