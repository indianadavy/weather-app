using MongoDB.Driver;
using WeatherApp;
using WeatherApp.DataAccess;
using WeatherApp.DataAccess.Interfaces;
using WeatherApp.Factories;

var builder = WebApplication.CreateBuilder(args);

// Add configuration to the app for enviroment variables > appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: true);
builder.Configuration.AddEnvironmentVariables();

// Add services
builder.Services.AddSingleton<IDataAccessFactory, DataAccessFactory>();
builder.Services.AddSingleton<IMongoCollection<WeatherForecast>>(sp =>
    sp.GetRequiredService<IDataAccessFactory>().GetMongoCollection());
builder.Services.AddSingleton<IMongoDbCollectionDataAccess, MongoDbCollectionDataAccess>();
builder.Services.AddTransient(sp => sp.GetRequiredService<IDataAccessFactory>().GetOpenMeteoDataAccess(sp));
builder.Services.AddHttpClient();
builder.Services.AddLogging();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherApp v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
