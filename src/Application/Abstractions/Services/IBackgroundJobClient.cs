using System.Linq.Expressions;

namespace Application.Abstractions.Services;

public interface IBackgroundJobClient
{
    string Enqueue(Expression<Action> methodCall);
    string Enqueue<T>(Expression<Action<T>> methodCall);
}
