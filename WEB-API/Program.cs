using WEB_API.Middlewares;
using WEB_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//Adicionando o Serilog
builder.AddSerilog();

// Confirura��o DbContext
builder.Services.AddDbContextConfiguration(builder.Configuration);

// Servi�o de log
builder.Services.AddScoped<ILogErroService, LogErroService>();

// Configura��o do CORS
builder.Services.AddCorsConfiguration();

// Configura��o do JWT
builder.Services.AddJwtContextConfiguration(builder.Configuration);

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Configura��o do DbContext (Middleware ainda pode ser usado para outros fins)
app.UseDbContextConfiguration();

// Configura��o do Servi�o de Log (Middleware ainda pode ser usado para outros fins)
app.UseLogServiceConfiguration();

// Configura��o Coors (Middleware ainda pode ser usado para outros fins)
app.UseCorsConfiguration();

// Configura��o do JWT
app.UseJwtConfiguration();

// Tratamento de erro global
app.UseErrorHandler();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();