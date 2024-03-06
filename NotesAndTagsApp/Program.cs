using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotesAndTagsApp.Helpers;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
                c => // Od google zemeno isto kako podole JWT Tokenot (:
                {
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                          Enter 'Bearer' [space] and then your token in the text input below.
                          \r\n\r\nExample: 'Bearer 12345abcdef'",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                      {
                        {
                          new OpenApiSecurityScheme
                          {
                            Reference = new OpenApiReference
                              {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                              },
                              Scheme = "oauth2",
                              Name = "Bearer",
                              In = ParameterLocation.Header,

                            },
                            new List<string>()
                          }
                        });
                });

// 1 Nacin na logiranje
//builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.File("logs.txt"));

// 2 Nacin na logiranje
Log.Logger = new LoggerConfiguration()
           .Enrich.FromLogContext()
           .MinimumLevel.Information()
           .WriteTo.File(
               $@"{AppDomain.CurrentDomain.BaseDirectory}Logs\UserNotes_LOG_{DateTime.Now.Date:dd-MM-yyyy}.txt",
               LogEventLevel.Information,
               "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {Message}{NewLine}{Exception}")
           .CreateLogger();

// Connect with Database

var appSettings = builder.Configuration.GetSection("DatabaseSettings");
builder.Services.Configure<DatabaseSettings>(appSettings);

DatabaseSettings dbSettingsObject = appSettings.Get<DatabaseSettings>();

DependencyInjectionHelper.InjectDbContext(builder.Services, dbSettingsObject.ConnectionString);

DependencyInjectionHelper.InjectRepositories(builder.Services);
DependencyInjectionHelper.InjectServices(builder.Services);

//Configure JWT - kopirano od google :)
builder.Services.AddAuthentication(x =>
{
    //we will use JWT authentication
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    //token configuration

    x.RequireHttpsMetadata = false;
    //we expect the token into the HttpContext
    x.SaveToken = true;
    //how to validate token
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateIssuerSigningKey = true,
        //the secret key
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("My secret secret secret secret secret secret secret key must be a long key "))
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
