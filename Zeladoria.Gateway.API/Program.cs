var builder = WebApplication.CreateBuilder(args);

// Adiciona o YARP lendo as configurações do appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.MapGet("/", () => "Zeladoria API Gateway rodando na porta 9000!");

app.Run();