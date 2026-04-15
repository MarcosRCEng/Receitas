using Microsoft.OpenApi.Models;
using MyRecipeBook.API.Binders;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MyRecipeBook.API.Filters;

public class IdsFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var encryptedIds = context
            .ApiDescription?
            .ParameterDescriptions?
            .Where(UsesMyRecipeBookIdBinder)
            .ToDictionary(d => d.Name, d => d)
            ?? [];

        foreach (var parameter in operation.Parameters ?? [])
        {
            if (parameter.Schema is not null && encryptedIds.ContainsKey(parameter.Name))
            {
                parameter.Schema.Format = string.Empty;
                parameter.Schema.Type = "string";
            }
        }

        foreach (var schema in context.SchemaRepository.Schemas.Values)
        {
            foreach (var property in schema.Properties ?? Enumerable.Empty<KeyValuePair<string, OpenApiSchema>>())
            {
                if (property.Value is not null && encryptedIds.ContainsKey(property.Key))
                {
                    property.Value.Format = string.Empty;
                    property.Value.Type = "string";
                }
            }
        }
    }

    private static bool UsesMyRecipeBookIdBinder(ApiParameterDescription parameterDescription) =>
        parameterDescription.ModelMetadata?.BinderType == typeof(MyRecipeBookIdBinder);
}
