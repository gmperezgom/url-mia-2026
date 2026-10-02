using MIA_AzureBlob;

// =======================================================
// MIA - AZURE BLOB STORAGE
// =======================================================
// SEGURIDAD: la Connection String NUNCA se escribe en el
// codigo fuente. Se obtiene desde una variable de entorno,
// cargada automaticamente desde un archivo .env si existe
// en la raiz del proyecto (ver README para mas detalles).
// =======================================================

CargarVariablesDesdeEnv();

const string NombreContenedor = "archivos";
const string VariableEntornoConnString = "AZURE_STORAGE_CONNECTION_STRING";

string? connectionString = Environment.GetEnvironmentVariable(VariableEntornoConnString);

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("ERROR: No se encontro la variable de entorno " +
        $"'{VariableEntornoConnString}'.");
    Console.WriteLine("Configurala como variable de entorno o en un archivo .env en la raiz del proyecto.");
    return;
}

BlobStorageService blobService;
try
{
    blobService = new BlobStorageService(connectionString, NombreContenedor);
}
catch (Exception ex)
{
    Console.WriteLine($"No fue posible conectar con Azure Blob Storage: {ex.Message}");
    return;
}

bool salir = false;

while (!salir)
{
    MostrarMenu();
    string? opcion = Console.ReadLine();

    switch (opcion)
    {
        case "1":
            SubirArchivo(blobService);
            break;
        case "2":
            ListarArchivos(blobService);
            break;
        case "3":
            DescargarArchivo(blobService);
            break;
        case "4":
            EliminarArchivo(blobService);
            break;
        case "5":
            salir = true;
            Console.WriteLine("Hasta luego");
            break;
        default:
            Console.WriteLine("Opcion invalida. Intenta de nuevo.");
            break;
    }

    if (!salir)
    {
        Console.WriteLine("\nPresiona ENTER para continuar...");
        Console.ReadLine();
    }
}

// ================== FUNCIONES DEL MENU ==================

static void MostrarMenu()
{
    Console.Clear();
    Console.WriteLine("=================================");
    Console.WriteLine("   MIA - AZURE BLOB STORAGE");
    Console.WriteLine("=================================");
    Console.WriteLine("1. Subir archivo");
    Console.WriteLine("2. Listar archivos");
    Console.WriteLine("3. Descargar archivo");
    Console.WriteLine("4. Eliminar archivo");
    Console.WriteLine("5. Salir");
    Console.WriteLine("=================================");
    Console.Write("Selecciona una opcion: ");
}

static void SubirArchivo(BlobStorageService servicio)
{
    Console.Write("\nRuta del archivo local a subir: ");
    string? ruta = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(ruta))
    {
        Console.WriteLine("Debes ingresar una ruta valida.");
        return;
    }

    bool exito = servicio.SubirArchivo(ruta, out string mensaje);
    Console.WriteLine(exito ? $"[OK] {mensaje}" : $"[ERROR] {mensaje}");
}

static void ListarArchivos(BlobStorageService servicio)
{
    var archivos = servicio.ListarArchivos();

    Console.WriteLine();
    if (archivos.Count == 0)
    {
        Console.WriteLine("El contenedor no tiene archivos.");
        return;
    }

    Console.WriteLine($"{"Nombre",-30} {"Tamaño",15}");
    Console.WriteLine(new string('-', 46));
    foreach (var (nombre, tamano) in archivos)
    {
        Console.WriteLine($"{nombre,-30} {tamano + " bytes",15}");
    }
}

static void DescargarArchivo(BlobStorageService servicio)
{
    Console.Write("\nNombre del blob a descargar: ");
    string? nombreBlob = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(nombreBlob))
    {
        Console.WriteLine("Debes ingresar un nombre de archivo valido.");
        return;
    }

    if (!servicio.ExisteArchivo(nombreBlob))
    {
        Console.WriteLine($"El archivo '{nombreBlob}' no existe en el contenedor.");
        return;
    }

    Console.Write("Carpeta de destino: ");
    string? carpetaDestino = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(carpetaDestino))
    {
        Console.WriteLine("Debes ingresar una carpeta de destino valida.");
        return;
    }

    bool exito = servicio.DescargarArchivo(nombreBlob, carpetaDestino, out string mensaje);
    Console.WriteLine(exito ? $"[OK] {mensaje}" : $"[ERROR] {mensaje}");
}

static void EliminarArchivo(BlobStorageService servicio)
{
    Console.Write("\nNombre del blob a eliminar: ");
    string? nombreBlob = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(nombreBlob))
    {
        Console.WriteLine("Debes ingresar un nombre de archivo valido.");
        return;
    }

    if (!servicio.ExisteArchivo(nombreBlob))
    {
        Console.WriteLine($"El archivo '{nombreBlob}' no existe en el contenedor.");
        return;
    }

    Console.Write($"¿Confirmas que deseas eliminar '{nombreBlob}'? (S/N): ");
    string? confirmacion = Console.ReadLine();

    if (!string.Equals(confirmacion, "S", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Operacion cancelada.");
        return;
    }

    bool exito = servicio.EliminarArchivo(nombreBlob, out string mensaje);
    Console.WriteLine(exito ? $"[OK] {mensaje}" : $"[ERROR] {mensaje}");
}

// ================== CARGA DE VARIABLES DESDE .env ==================

/// <summary>
/// Lee un archivo .env en la raíz del proyecto (si existe) y carga sus
/// variables en el entorno del proceso actual, sin sobrescribir variables
/// que ya estén definidas externamente (por ejemplo, en PowerShell).
/// </summary>
static void CargarVariablesDesdeEnv()
{
    string rutaEnv = Path.Combine(Directory.GetCurrentDirectory(), ".env");

    if (!File.Exists(rutaEnv))
        return;

    foreach (string lineaCruda in File.ReadAllLines(rutaEnv))
    {
        string linea = lineaCruda.Trim();

        // Ignorar líneas vacías o comentarios
        if (string.IsNullOrWhiteSpace(linea) || linea.StartsWith('#'))
            continue;

        int indiceIgual = linea.IndexOf('=');
        if (indiceIgual <= 0)
            continue;

        string clave = linea[..indiceIgual].Trim();
        string valor = linea[(indiceIgual + 1)..].Trim().Trim('"');

        // No sobrescribir si ya existe en el entorno del proceso/sistema
        if (Environment.GetEnvironmentVariable(clave) is null)
        {
            Environment.SetEnvironmentVariable(clave, valor);
        }
    }
}