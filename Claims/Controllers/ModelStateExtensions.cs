using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Claims.Controllers;

public static class ModelStateExtensions
{
    public static void AddErrors(this ModelStateDictionary modelState, IReadOnlyDictionary<string, string> errors)
    {
        foreach (var (key, value) in errors)
        {
            modelState.AddModelError(key, value);
        }
    }
}