using HospitalManagement.Core;
using Microsoft.AspNetCore.Mvc;

namespace HospitalWeb.ViewComponents;

/// <summary>
/// Shared pagination bar view component.
/// Usage in any Razor view:
///   @await Component.InvokeAsync("Pagination", new { result = Model.Patients, pageParam = "PageNumber" })
/// </summary>
public class PaginationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IPaged result, string pageParam = "PageNumber")
    {
        var vm = new PaginationViewModel
        {
            TotalCount  = result.TotalCount,
            PageNumber  = result.PageNumber,
            PageSize    = result.PageSize,
            TotalPages  = result.TotalPages,
            HasPrev     = result.HasPrev,
            HasNext     = result.HasNext,
            From        = result.From,
            To          = result.To,
            PageParam   = pageParam,
            // Preserve all existing query-string params except the page param
            QueryParams = HttpContext.Request.Query
                .Where(q => !q.Key.Equals(pageParam, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(q => q.Key, q => q.Value.ToString())
        };
        return View(vm);
    }
}

public class PaginationViewModel
{
    public int TotalCount  { get; set; }
    public int PageNumber  { get; set; }
    public int PageSize    { get; set; }
    public int TotalPages  { get; set; }
    public bool HasPrev    { get; set; }
    public bool HasNext    { get; set; }
    public int From        { get; set; }
    public int To          { get; set; }
    public string PageParam { get; set; } = "PageNumber";
    public Dictionary<string, string> QueryParams { get; set; } = new();

    public string BuildUrl(int page)
    {
        var qs = new Dictionary<string, string>(QueryParams) { [PageParam] = page.ToString() };
        return "?" + string.Join("&", qs.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
    }
}
