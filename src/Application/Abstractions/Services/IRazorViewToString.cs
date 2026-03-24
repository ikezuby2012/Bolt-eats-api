namespace Application.Abstractions.Services;

public interface IRazorViewToString
{
    Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
}
