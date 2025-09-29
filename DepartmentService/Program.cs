using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
// Добавляем сервисы Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
var http = new HttpClient();

var departments = new List<Department>();

// список кафедр
app.MapGet("/departments", () => departments);

// подробности кафедры и ее факультета
app.MapGet("/departments/{id:guid}", async (Guid id) =>
{
	var dep = departments.FirstOrDefault(x => x.Uuid == id);
	if (dep is null) return Results.NotFound();

	FacultyDto? faculty = null;
	if (dep.FacultyUuid != Guid.Empty)
	{
		try
		{
			faculty = await http.GetFromJsonAsync<FacultyDto>(
				 $"http://faculty:8080/faculties/{dep.FacultyUuid}"
				);
		}
		catch { }
	}
	var details = new DepartmentDetails(dep, faculty);
	return Results.Ok(details);
});
// создать кафедру
app.MapPost("/departments", (DepartmentCreate dto) =>
{
	var f = new Department
	{
		Uuid = Guid.NewGuid(),
		Name = dto.Name,
		Head = dto.Head,
		FacultyUuid = dto.FacultyUuid,
	};
	departments.Add(f);
	return Results.Ok(f);
});

// обновить кафедру
app.MapPut("/departments/{id:guid}", (Guid id, DepartmentCreate dto) =>
{
	var f = departments.FirstOrDefault(f => f.Uuid == id);
	if (f is null) return Results.NotFound();
	f.Name = dto.Name;
	f.Head = dto.Head;
	f.FacultyUuid = dto.FacultyUuid;
	return Results.Ok(f);
});


// удалить кафедру
app.MapDelete("/departments/{id:guid}", (Guid id) =>
{
	var f = departments.FirstOrDefault(f => f.Uuid == id);
	if (f is null) return Results.NotFound();
	departments.Remove(f);
	return Results.Ok();
});

app.Run();

record Department
{
	[JsonPropertyName("uuid")] public Guid Uuid { get; set; }
	[JsonPropertyName("name")] public string Name { get; set; } = "";
	[JsonPropertyName("dean")] public string Head { get; set; } = "";
	[JsonPropertyName("facultyUuid")] public Guid FacultyUuid { get; set; }

}

record DepartmentCreate(string Name, string Head, Guid FacultyUuid);

record FacultyDto(Guid Uuid, string Name, string Dean);

record DepartmentDetails(Department Department, FacultyDto? FacultyInfo);