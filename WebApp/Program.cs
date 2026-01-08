using System.IO;
using System.Linq;
using SharedLib.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var appData = Path.Combine(AppContext.BaseDirectory, "App_Data");

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/files", () =>
{
    if (!Directory.Exists(appData)) return Results.Ok(Array.Empty<string>());
    var files = Directory.GetFiles(appData, "*.json", SearchOption.AllDirectories)
        .Select(f => Path.GetRelativePath(appData, f).Replace("\\", "/"))
        .OrderBy(s => s);
    return Results.Ok(files);
});

app.MapGet("/api/files/{*relativePath}", (string relativePath) =>
{
    var file = Path.Combine(appData, relativePath);
    if (!File.Exists(file)) return Results.NotFound();
    var set = JsonDataService.LoadJsonRecordSet(file);
    if (set == null) return Results.BadRequest(new { error = "unable to parse JSON as record set" });
    return Results.Ok(new { file = relativePath, set.FieldNames, set.Count, Records = set.Records });
});

app.Run();
