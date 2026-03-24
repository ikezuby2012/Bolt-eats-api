using Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Infrastructure.Services;
internal sealed class RazorViewToStringService(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider) : IRazorViewToString
{
    public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
    {
        ActionContext actionContext = GetActionContext();
        IView view = FindView(actionContext, viewName);

        await using var output = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            new ViewDataDictionary<TModel>(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary())
            {
                Model = model
            },
            new TempDataDictionary(
                actionContext.HttpContext,
                tempDataProvider),
            output,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);

        return output.ToString();

    }

    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        return new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor());
    }

    private IView FindView(ActionContext actionContext, string viewName)
    {
        ViewEngineResult getViewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        ViewEngineResult findViewResult = viewEngine.FindView(actionContext, viewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        IEnumerable<string> searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        string errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations));

        throw new InvalidOperationException(errorMessage);
    }
}
