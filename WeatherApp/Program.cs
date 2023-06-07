using MongoDB.Driver;
using WeatherApp;
using WeatherApp.DataAccess;
using WeatherApp.DataAccess.Interfaces;
using WeatherApp.Factories;

var builder = WebApplication.CreateBuilder(args);

// Add configuration to the app for envrioment variables & appsettings.json
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: true);

// Add services
builder.Services.AddSingleton<IDataAccessFactory, DataAccessFactory>();
builder.Services.AddSingleton<IMongoCollection<WeatherForecast>>(x =>
{
    IDataAccessFactory factory = x.GetRequiredService<IDataAccessFactory>();
    return factory.GetMongoCollection();
});
builder.Services.AddSingleton<IMongoDbCollectionDataAccess, MongoDbCollectionDataAccess>();
builder.Services.AddSingleton<IOpenMeteoDataAccess, OpenMeteoDataAccess>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
