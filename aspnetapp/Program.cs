using aspnetapp.Endpoints;
using aspnetapp.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(4999, listenOptions =>
        listenOptions.Protocols = HttpProtocols.Http1);

    options.ListenAnyIP(5000, listenOptions =>
        listenOptions.Protocols = HttpProtocols.Http2);
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services
    .AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddOpenApi("v1", options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Info = new OpenApiInfo
    {
        Version = "v1",
        Title = "Sample API",
        Description = "A simple hybrid REST and gRPC service using ASP.NET Core (.NET 10)",
        Contact = new OpenApiContact
        {
            Name = "Ben Chen",
            Url = new Uri("https://www.linkedin.com/in/ben-jamin-chen/")
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://github.com/bchen04/aspnetcore-grpc-rest/blob/master/LICENSE")
        }
    };

    return Task.CompletedTask;
}));

builder.Services.AddGrpc();
builder.Services.AddScoped<IGreeterService, GreeterService>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapHello();
app.MapGrpcService<GreeterService>();

app.Run();
