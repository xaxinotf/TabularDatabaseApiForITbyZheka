using TabularDatabaseApiForITbyZheka.Services;

var builder = WebApplication.CreateBuilder(args);

// ������ ������ �� ����������
builder.Services.AddSingleton(new DatabaseService(Path.Combine(builder.Environment.ContentRootPath, "Data", "database.json")));

builder.Services.AddControllers();

// ������ �������� Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ������������ ������� HTTP ������
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
