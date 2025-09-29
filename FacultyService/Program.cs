using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var faculties = new List<Faculty>();

app.MapGet("/faculties", () => faculties);

app.MapGet("/faculties/{id:guid}", (Guid id) =>
	faculties.FirstOrDefault(f => f.Uuid == id) is { } f
		? Results.Ok(f)
		: Results.NotFound());

app.MapPost("/faculties", (FacultyCreate dto) =>
{
	var f = new Faculty
	{
		Uuid = Guid.NewGuid(),
		Name = dto.Name,
		Dean = dto.Dean
	};
	faculties.Add(f);
	return Results.Ok(f);
});

app.MapPut("/faculties/{id:guid}", (Guid id, FacultyCreate dto) =>
{
	var f = faculties.FirstOrDefault(f => f.Uuid == id);
	if (f is null) return Results.NotFound();
	f.Name = dto.Name;
	f.Dean = dto.Dean;
	return Results.Ok(f);
});

app.MapDelete("/faculties/{id:guid}", (Guid id) =>
{
	var f = faculties.FirstOrDefault(f => f.Uuid == id);
	if (f is null) return Results.NotFound();
	faculties.Remove(f);
	return Results.Ok();
});

app.Run();

record Faculty
{
	[JsonPropertyName("uuid")] public Guid Uuid { get; set; }
	[JsonPropertyName("name")] public string Name { get; set; } = "";
	[JsonPropertyName("dean")] public string Dean { get; set; } = "";
}

record FacultyCreate(string Name, string Dean);
