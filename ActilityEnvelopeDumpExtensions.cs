using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;

namespace IoTHub
{
    public static class ActilityEnvelopeDumpExtensions
    {
        public static IApplicationBuilder UseActilityEnvelopeDump(
            this IApplicationBuilder app,
            bool enabled)
        {
            if (!enabled)
                return app;

            return app.Use(async (context, next) =>
            {
                if (context.Request.Path.Equals(
                    "/Actility/persist",
                    StringComparison.OrdinalIgnoreCase))
                {
                    context.Request.EnableBuffering();

                    using StreamReader reader = new(
                        context.Request.Body,
                        Encoding.UTF8,
                        leaveOpen: true);

                    context.Items["ActilityEnvelope"] = await reader.ReadToEndAsync();

                    context.Request.Body.Position = 0;
                }

                await next();
            });
        }
    }
}