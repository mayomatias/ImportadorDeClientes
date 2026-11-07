using System;
using Newtonsoft.Json;

namespace ImportadorDeClientes.Core.Modelos;

public class Cliente
{
    private string _dni;

    [JsonProperty("dni")]
    public string Dni
    {
        get => _dni;
        set => _dni = value?.Trim().ToUpperInvariant();
    }

    [JsonProperty("nombre")]
    public string Nombre { get; set; }

    [JsonProperty("apellidos")]
    public string Apellidos { get; set; }

    [JsonProperty("fechaNacimiento")]
    public DateTime FechaNacimiento { get; set; }

    [JsonProperty("telefono")]
    public string Telefono { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }
}