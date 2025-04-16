namespace WSPWeatherService.Api;

public static class DefaultEndpointsExtensions
{
    public static void MapDefaultEndpoint(this IEndpointRouteBuilder routeBuilder, IWebHostEnvironment environment)
    {
        routeBuilder.MapGet("/", () =>
        {
            string? endpoints = null;
            if (environment.IsDevelopment())
            {
                endpoints = """
                            <p>Available endpoints:</p>
                            <ul>
                                <li><a href="/swagger">Swagger UI</a> <span class="tag">API Explorer</span></li>
                                <li><a href="/swagger/v1/swagger.json">OpenAPI JSON</a> <span class="tag">OAS</span></li>
                                <li><a href="/hangfire">Hangfire Dashboard</a> <span class="tag">Jobs</span></li>
                            </ul>
                            """;
            }

            var html = """
                       <!DOCTYPE html>
                       <html lang="en">
                       <head>
                       <meta charset="UTF-8" />
                       <title>WSP Weather Service</title>
                       <style>
                           body {
                               font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                               background-color: #f4f6f8;
                               color: #333;
                               padding: 2rem;
                               max-width: 600px;
                               margin: auto;
                           }
                           
                           .title{
                               display: flex;
                               flex-direction: row;
                               align-items: center;
                               gap: 0.5rem;
                           }
                       
                           h1 {
                               color: #007acc;
                               font-size: 1.8rem;
                               margin:0;
                           }
                       
                           ul {
                               list-style: none;
                               padding: 0;
                           }
                       
                           li {
                               margin-bottom: 0.5rem;
                           }
                       
                           a {
                               text-decoration: none;
                               color: #007acc;
                               font-weight: 500;
                           }
                       
                           a:hover {
                               text-decoration: underline;
                           }
                       
                           .tag {
                               display: inline-block;
                               background: #e0f3ff;
                               color: #007acc;
                               padding: 0.2rem 0.5rem;
                               border-radius: 4px;
                               font-size: 0.7rem;
                               font-weight:bold;
                           }
                       
                           .tag.healthy {
                               background: #a3e7a3;
                               color: #2f7e3e;
                           }
                       
                           footer {
                               margin-top: 2rem;
                               font-size: 0.8rem;
                               color: #888;
                           }
                       </style>
                       </head>
                       <body>
                       <div class="title">
                           <h1>🌤️ WSP Weather Service </h1><span class="tag healthy">Healthy</span>
                       </div>

                       """ +
                       endpoints
                       + """
                             
                             <footer>
                                 &copy; 2025 Michael Isler
                             </footer>
                         </body>
                         </html>
                         """;
            return Results.Content(html, "text/html");
        }).ExcludeFromDescription();
    }
}