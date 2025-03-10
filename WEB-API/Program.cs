using WEB_API.Middlewares;
using WEB_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//Adicionando o Serilog
builder.AddSerilog();

// Confiruração DbContext
builder.Services.AddDbContextConfiguration(builder.Configuration);

// Serviço de log
builder.Services.AddScoped<ILogErroService, LogErroService>();

// Configuração do CORS
builder.Services.AddCorsConfiguration();

// Configuração do JWT
builder.Services.AddJwtContextConfiguration(builder.Configuration);

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Configuração do DbContext (Middleware ainda pode ser usado para outros fins)
app.UseDbContextConfiguration();

// Configuração do Serviço de Log (Middleware ainda pode ser usado para outros fins)
app.UseLogServiceConfiguration();

// Configuração Coors (Middleware ainda pode ser usado para outros fins)
app.UseCorsConfiguration();

// Configuração do JWT
app.UseJwtConfiguration();

// Tratamento de erro global
app.UseErrorHandler();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();