using System.Linq.Expressions;
using Hangfire;

namespace Infrastructure.BackgroundJobs;

public class HangfireBackgroundJobClient(IBackgroundJobClient hangFireClient) : Application.Abstractions.Services.IBackgroundJobClient
{
    public string Enqueue(Expression<Action> methodCall) => hangFireClient.Enqueue(methodCall);

    public string Enqueue<T>(Expression<Action<T>> methodCall) => hangFireClient.Enqueue(methodCall);
}
