using Microsoft.EntityFrameworkCore;
using SafeShelter.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });
    }
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SafeShelter API - DevOps Tools & Cloud Computing",
        Version = "v1",
        Description = "API de Gestão de Comunidades e Dispositivos IoT para Alertas de Emergência (2º Checkpoint 2º Semestre - FIAP)."
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        if (!context.Comunidades.Any())
        {
            var com1 = new SafeShelter.Api.Models.Comunidade 
            { 
                Nome = "Comunidade Paraisópolis - Setor A", 
                PoligonoGeografico = "-23.5988,-46.7262;-23.5995,-46.7270" 
            };
            var com2 = new SafeShelter.Api.Models.Comunidade 
            { 
                Nome = "Comunidade Heliópolis - Núcleo Central", 
                PoligonoGeografico = "-23.6152,-46.5925;-23.6160,-46.5935" 
            };
            context.Comunidades.AddRange(com1, com2);
            context.SaveChanges();

            var disp1 = new SafeShelter.Api.Models.Dispositivo 
            { 
                MacAddress = "ESP32-A1B2-C3D4", 
                PerfilResponsavel = "Defesa Civil - Setor Sul", 
                ComunidadeId = com1.Id 
            };
            var disp2 = new SafeShelter.Api.Models.Dispositivo 
            { 
                MacAddress = "ESP32-RM561760", 
                PerfilResponsavel = "Equipe Resgate Rápido", 
                ComunidadeId = com2.Id 
            };
            context.Dispositivos.AddRange(disp1, disp2);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao inicializar tabelas no banco de dados.");
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SafeShelter API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
