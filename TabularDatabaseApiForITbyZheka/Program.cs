using TabularDatabaseApiForITbyZheka.Services;

var builder = WebApplication.CreateBuilder(args);

// Додати сервіси до контейнера
builder.Services.AddSingleton(new DatabaseService(Path.Combine(builder.Environment.ContentRootPath, "Data", "database.json")));

builder.Services.AddControllers();

// Додати підтримку Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Налаштування конвеєра HTTP запитів
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
