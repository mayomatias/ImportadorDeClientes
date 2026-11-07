using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImportadorDeClientes.Core.Almacenamiento;
using ImportadorDeClientes.Core.Modelos;
using ImportadorDeClientes.Core.Validacion;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Fechas de nacimiento como "yyyy-MM-dd" en las respuestas (sin hora).
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new FechaJsonConverter()));

builder.Services.AddSingleton<IClienteRepository>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var ruta = config["Storage:Path"] ?? "data/clientes.json";
    return new JsonFileClienteRepository(Path.Combine(env.ContentRootPath, ruta));
});
builder.Services.AddSingleton<ClienteValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Clientes API"));
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

var clientes = app.MapGroup("/clientes").WithTags("Clientes");

clientes.MapGet("/", (IClienteRepository repo) =>
        TypedResults.Ok(repo.GetAll()))
    .WithName("ListarClientes");

clientes.MapGet("/{dni}", Results<Ok<Cliente>, NotFound> (string dni, IClienteRepository repo) =>
        repo.GetByDni(dni) is { } cliente ? TypedResults.Ok(cliente) : TypedResults.NotFound())
    .WithName("ObtenerCliente");

clientes.MapPost("/", Results<Created<Cliente>, ValidationProblem> (
        Cliente cliente, IClienteRepository repo, ClienteValidator validator) =>
    {
        var validacion = validator.Validar(cliente);
        if (!validacion.EsValido)
            return TypedResults.ValidationProblem(AErrores(validacion));

        if (!repo.Add(cliente))
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["dni"] = [$"Ya existe un cliente con DNI {cliente.Dni}."]
            });

        return TypedResults.Created($"/clientes/{cliente.Dni}", cliente);
    })
    .WithName("CrearCliente");

clientes.MapPut("/{dni}", Results<Ok<Cliente>, NotFound, ValidationProblem> (
        string dni, Cliente cliente, IClienteRepository repo, ClienteValidator validator) =>
    {
        // Normalizamos igual que el modelo para comparar.
        if (!string.Equals(dni.Trim(), cliente.Dni, StringComparison.OrdinalIgnoreCase))
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["dni"] = ["El DNI del cuerpo no coincide con el de la ruta."]
            });

        var validacion = validator.Validar(cliente);
        if (!validacion.EsValido)
            return TypedResults.ValidationProblem(AErrores(validacion));

        return repo.Update(cliente) ? TypedResults.Ok(cliente) : TypedResults.NotFound();
    })
    .WithName("ActualizarCliente");

clientes.MapDelete("/{dni}", Results<NoContent, NotFound> (string dni, IClienteRepository repo) =>
        repo.Delete(dni) ? TypedResults.NoContent() : TypedResults.NotFound())
    .WithName("EliminarCliente");

app.Run();

static Dictionary<string, string[]> AErrores(ResultadoValidacion validacion) =>
    validacion.Errores
        .GroupBy(e => JsonNamingPolicy.CamelCase.ConvertName(e.Campo))
        .ToDictionary(g => g.Key, g => g.Select(e => e.Mensaje).ToArray());

/// <summary>Escribe las fechas como yyyy-MM-dd y acepta también dd/MM/yyyy al leer, igual que la importación.</summary>
internal sealed class FechaJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var texto = reader.GetString();
        if (ImportadorDeClientes.Core.Importacion.FechasCliente.TryParse(texto, out var fecha))
            return fecha;
        throw new JsonException($"Fecha '{texto}' no válida (formatos admitidos: yyyy-MM-dd, dd/MM/yyyy).");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}

// Necesario para WebApplicationFactory<Program> en los tests de integración.
public partial class Program;
