using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BookingService.Swagger
{
    /// <summary>
    /// Swagger operation filter to handle file uploads with IFormFile
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the action has IFormFile parameters
            var formFileParameters = context.ApiDescription.ParameterDescriptions
                .Where(p => p.ModelMetadata?.ModelType == typeof(IFormFile) ||
                           p.ModelMetadata?.ModelType == typeof(List<IFormFile>))
                .ToList();

            if (!formFileParameters.Any())
                return;

            // Check if the action has [Consumes("multipart/form-data")]
            var consumesAttribute = context.MethodInfo.GetCustomAttributes(true)
                .OfType<Microsoft.AspNetCore.Mvc.ConsumesAttribute>()
                .FirstOrDefault();

            if (consumesAttribute == null || !consumesAttribute.ContentTypes.Contains("multipart/form-data"))
                return;

            // Remove only form parameters from the parameter list (preserve route, query, header params)
            var formParameters = context.ApiDescription.ParameterDescriptions
                .Where(p => p.Source.Id == "Form")
                .ToList();

            // Remove form parameters from operation.Parameters (they'll go in RequestBody instead)
            if (operation.Parameters != null)
            {
                var nonFormParams = operation.Parameters
                    .Where(p => !formParameters.Any(fp => fp.Name == p.Name))
                    .ToList();
                operation.Parameters.Clear();
                foreach (var param in nonFormParams)
                {
                    operation.Parameters.Add(param);
                }
            }

            // Create multipart/form-data request body
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>(),
                            Required = new HashSet<string>()
                        }
                    }
                }
            };

            var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

            // Add only [FromForm] parameters to the request body
            foreach (var parameter in formParameters)
            {
                if (parameter.ModelMetadata?.ModelType == typeof(IFormFile))
                {
                    // IFormFile parameter
                    schema.Properties[parameter.Name] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = parameter.ModelMetadata.Description ?? "Upload file"
                    };

                    // Mark as required if not nullable
                    if (!parameter.ModelMetadata.IsNullableValueType)
                    {
                        schema.Required.Add(parameter.Name);
                    }
                }
                else if (parameter.ModelMetadata?.ModelType == typeof(List<IFormFile>))
                {
                    // List<IFormFile> parameter
                    schema.Properties[parameter.Name] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        },
                        Description = parameter.ModelMetadata.Description ?? "Upload multiple files"
                    };

                    if (!parameter.ModelMetadata.IsNullableValueType)
                    {
                        schema.Required.Add(parameter.Name);
                    }
                }
                else
                {
                    // Regular [FromForm] parameter (string, int, etc.)
                    var propertyType = parameter.ModelMetadata?.ModelType;

                    if (propertyType == typeof(string))
                    {
                        schema.Properties[parameter.Name] = new OpenApiSchema
                        {
                            Type = "string",
                            Description = parameter.ModelMetadata.Description
                        };
                    }
                    else if (propertyType == typeof(int) || propertyType == typeof(int?))
                    {
                        schema.Properties[parameter.Name] = new OpenApiSchema
                        {
                            Type = "integer",
                            Format = "int32",
                            Description = parameter.ModelMetadata.Description
                        };
                    }
                    else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                    {
                        schema.Properties[parameter.Name] = new OpenApiSchema
                        {
                            Type = "boolean",
                            Description = parameter.ModelMetadata.Description
                        };
                    }
                    else
                    {
                        // Default to string for other types
                        schema.Properties[parameter.Name] = new OpenApiSchema
                        {
                            Type = "string",
                            Description = parameter.ModelMetadata?.Description
                        };
                    }

                    // Mark as required if not nullable
                    if (parameter.ModelMetadata != null && !parameter.ModelMetadata.IsNullableValueType && propertyType?.IsValueType == true)
                    {
                        schema.Required.Add(parameter.Name);
                    }
                }
            }
        }
    }
}
