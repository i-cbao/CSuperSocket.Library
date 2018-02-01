using System;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.ChmUI;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUIBuilderExtensions
    {
        private const string EmbeddedFilesNamespace = "Swashbuckle.AspNetCore.ChmUI.bower_components.swagger_ui.dist";

        public static IApplicationBuilder UseSwaggerUI(
            this IApplicationBuilder app,
            Action<SwaggerUIOptions> setupAction)
        {
            var options = new SwaggerUIOptions();
            setupAction?.Invoke(options);

            app.UseMiddleware<SwaggerUIIndexMiddleware>(options);
            app.UseFileServer(new FileServerOptions
            {
                RequestPath = $"/{options.RoutePrefix}",
                FileProvider = new EmbeddedFileProvider(typeof(SwaggerUIBuilderExtensions).GetTypeInfo().Assembly, EmbeddedFilesNamespace),
                EnableDefaultFiles = true //serve index.html at /{ options.RoutePrefix }/
            });

            return app;
        }
    }
}
