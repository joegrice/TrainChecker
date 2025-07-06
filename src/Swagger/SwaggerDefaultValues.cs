using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TrainChecker.Swagger;

public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        operation.Deprecated |= apiDescription.IsDeprecated();

        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            var response = new OpenApiResponse();
            response.Content.Add("application/json", new OpenApiMediaType());
            operation.Responses.TryAdd(responseKey, response);
        }

        if (operation.Parameters == null)
        {
            return;
        }

        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

            if (parameter.Description == null)
            {
                parameter.Description = description.ModelMetadata?.Description;
            }

            if (parameter.Schema.Default == null && description.DefaultValue != null)
            {
                parameter.Schema.Default = new OpenApiString(description.DefaultValue.ToString());
            }

            parameter.Required |= description.IsRequired;

            if (parameter.Name.Equals("origin", StringComparison.OrdinalIgnoreCase))
            {
                parameter.Example = new OpenApiString("VIC");
            }
            else if (parameter.Name.Equals("destination", StringComparison.OrdinalIgnoreCase))
            {
                parameter.Example = new OpenApiString("DKG");
            }
        }
    }
}
