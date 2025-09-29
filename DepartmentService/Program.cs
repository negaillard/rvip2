using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var http = new HttpClient();
app.Urls.Add("http://*:5000");
var departments = new List<Department>();

// ������ ������
app.MapGet("/departments", () => departments);

// ����������� ������� � �� ����������
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
				"http://faculty:5000/faculties/{dep.FacultyUuid}"
				);
		}
		catch { }
	}
	var details = new DepartmentDetails(dep, faculty);
	return Results.Ok(details);
});
// ������� �������
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

// �������� �������
app.MapPut("/departments/{id:guid}", (Guid id, DepartmentCreate dto) =>
{
	var f = departments.FirstOrDefault(f => f.Uuid == id);
	if (f is null) return Results.NotFound();
	f.Name = dto.Name;
	f.Head = dto.Head;
	f.FacultyUuid = dto.FacultyUuid;
	return Results.Ok(f);
});


// ������� �������
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