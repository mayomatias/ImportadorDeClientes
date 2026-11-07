using System.Collections.Generic;
using ImportadorDeClientes.Core.Modelos;

namespace ImportadorDeClientes.Core.Almacenamiento;

public interface IClienteRepository
{
    IReadOnlyList<Cliente> GetAll();

    /// <summary>Devuelve null si no existe. El DNI se normaliza (trim + mayúsculas).</summary>
    Cliente GetByDni(string dni);

    /// <summary>Devuelve false si ya existe un cliente con ese DNI.</summary>
    bool Add(Cliente cliente);

    /// <summary>Devuelve false si no existe un cliente con ese DNI.</summary>
    bool Update(Cliente cliente);

    /// <summary>Devuelve false si no existe un cliente con ese DNI.</summary>
    bool Delete(string dni);

    /// <summary>Sustituye todo el contenido almacenado por la lista indicada.</summary>
    void SaveAll(IEnumerable<Cliente> clientes);
}
