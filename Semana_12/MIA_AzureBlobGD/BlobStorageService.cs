using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace MIA_AzureBlob;

/// <summary>
/// Encapsula toda la interaccion con Azure Blob Storage.
/// Se apoya en las tres clases principales del SDK:
///   - BlobServiceClient   -> representa la Storage Account completa.
///   - BlobContainerClient -> representa un contenedor especifico.
///   - BlobClient          -> representa un blob especifico dentro del contenedor.
/// </summary>
public class BlobStorageService
{
    private readonly BlobServiceClient _serviceClient;
    private readonly BlobContainerClient _containerClient;

    public string ContainerName { get; }

    public BlobStorageService(string connectionString, string containerName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("La connection string no puede estar vacía.");

        ContainerName = containerName;

        // Nivel Storage Account
        _serviceClient = new BlobServiceClient(connectionString);

        // Nivel Contenedor (se crea si no existe)
        _containerClient = _serviceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists(PublicAccessType.None);
    }

    /// <summary>
    /// Sube un archivo local al contenedor. Devuelve true si la operación fue exitosa.
    /// </summary>
    public bool SubirArchivo(string rutaLocal, out string mensaje)
    {
        try
        {
            if (!File.Exists(rutaLocal))
            {
                mensaje = $"El archivo '{rutaLocal}' no existe.";
                return false;
            }

            string nombreArchivo = Path.GetFileName(rutaLocal);

            // Nivel Blob individual
            BlobClient blobClient = _containerClient.GetBlobClient(nombreArchivo);

            using FileStream fs = File.OpenRead(rutaLocal);
            blobClient.Upload(fs, overwrite: true);

            mensaje = $"Archivo '{nombreArchivo}' subido correctamente al contenedor '{ContainerName}'.";
            return true;
        }
        catch (RequestFailedException ex)
        {
            mensaje = $"Error de Azure al subir el archivo: {ex.Message}";
            return false;
        }
        catch (Exception ex)
        {
            mensaje = $"Error inesperado al subir el archivo: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Lista los blobs existentes en el contenedor con nombre y tamaño.
    /// </summary>
    public List<(string Nombre, long Tamano)> ListarArchivos()
    {
        var resultado = new List<(string, long)>();

        foreach (BlobItem blobItem in _containerClient.GetBlobs())
        {
            long tamano = blobItem.Properties.ContentLength ?? 0;
            resultado.Add((blobItem.Name, tamano));
        }

        return resultado;
    }

    /// <summary>
    /// Verifica si un blob existe en el contenedor.
    /// </summary>
    public bool ExisteArchivo(string nombreBlob)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(nombreBlob);
        return blobClient.Exists();
    }

    /// <summary>
    /// Descarga un blob a una carpeta de destino local.
    /// </summary>
    public bool DescargarArchivo(string nombreBlob, string carpetaDestino, out string mensaje)
    {
        try
        {
            BlobClient blobClient = _containerClient.GetBlobClient(nombreBlob);

            if (!blobClient.Exists())
            {
                mensaje = $"El blob '{nombreBlob}' no existe en el contenedor.";
                return false;
            }

            if (!Directory.Exists(carpetaDestino))
                Directory.CreateDirectory(carpetaDestino);

            string rutaDestino = Path.Combine(carpetaDestino, nombreBlob);

            blobClient.DownloadTo(rutaDestino);

            mensaje = $"Archivo descargado correctamente en: {rutaDestino}";
            return true;
        }
        catch (RequestFailedException ex)
        {
            mensaje = $"Error de Azure al descargar el archivo: {ex.Message}";
            return false;
        }
        catch (Exception ex)
        {
            mensaje = $"Error inesperado al descargar el archivo: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Elimina un blob del contenedor.
    /// </summary>
    public bool EliminarArchivo(string nombreBlob, out string mensaje)
    {
        try
        {
            BlobClient blobClient = _containerClient.GetBlobClient(nombreBlob);

            if (!blobClient.Exists())
            {
                mensaje = $"El blob '{nombreBlob}' no existe en el contenedor.";
                return false;
            }

            blobClient.Delete();

            mensaje = $"Archivo '{nombreBlob}' eliminado correctamente.";
            return true;
        }
        catch (RequestFailedException ex)
        {
            mensaje = $"Error de Azure al eliminar el archivo: {ex.Message}";
            return false;
        }
        catch (Exception ex)
        {
            mensaje = $"Error inesperado al eliminar el archivo: {ex.Message}";
            return false;
        }
    }
}