Objetivo de la aplicación

Esta aplicación de consola en C# / .NET tiene como objetivo administrar archivos en Azure Blob Storage mediante las cuatro operaciones básicas de gestión de archivos: subir, listar, descargar y eliminar. Simula un caso de uso común en aplicaciones empresariales donde se requiere almacenar y recuperar archivos no estructurados (documentos, imágenes, backups, etc.) de forma escalable y sin depender del sistema de archivos local del servidor.


Tecnologías utilizadas
Tecnología	Uso
C# / .NET 8	Lenguaje y runtime de la aplicación de consola
Azure.Storage.Blobs (SDK oficial)	Paquete NuGet para interactuar con Azure Blob Storage
Azure Storage Account	Servicio en la nube donde se almacenan los blobs
PowerShell	Terminal utilizada para configurar variables de entorno y ejecutar la aplicación


Configuración de Azure
Se creó un Storage Account en el Portal de Azure (storagemialab72145545 en este caso).
Dentro del Storage Account, la aplicación crea automáticamente un contenedor de blobs llamado archivos la primera vez que se ejecuta (con acceso privado, PublicAccessType.None), en caso de que no exista.
La autenticación contra el Storage Account se realiza mediante una Connection String, obtenida desde: Storage Account → Security + networking → Access keys → Connection string.
Esa Connection String no se guarda en el código ni en el repositorio; se carga desde una variable de entorno en tiempo de ejecución (ver sección de protección de credenciales más abajo).
Arquitectura de la solución

La solución sigue una separación simple de responsabilidades en dos capas:

MIA_AzureBlob/
├── Program.cs                 → Capa de presentación (menú de consola)
├── BlobStorageService.cs      → Capa de acceso a datos (lógica de Azure Blob Storage)
├── MIA_AzureBlob.csproj       → Definición del proyecto y dependencias
└── README.md                  → Documentación
Program.cs: contiene el punto de entrada, el menú interactivo y el manejo de la entrada/salida del usuario. No conoce detalles internos de Azure; solo llama a métodos de BlobStorageService.
BlobStorageService.cs: encapsula toda la interacción con el SDK de Azure, siguiendo la jerarquía de clases del propio SDK:
BlobServiceClient → representa la Storage Account completa.
BlobContainerClient → representa el contenedor archivos.
BlobClient → representa un blob individual dentro del contenedor.

Esta separación permite que, si en el futuro se cambia la forma de exponer la aplicación (por ejemplo, a una API web en vez de consola), la capa de acceso a Azure (BlobStorageService) se pueda reutilizar sin modificaciones.

Descripción de las cuatro operaciones
1. Subir archivo

Solicita al usuario la ruta local de un archivo, valida que exista con File.Exists, obtiene el nombre del archivo con Path.GetFileName, y lo sube al contenedor mediante BlobClient.Upload(), sobrescribiendo si ya existe uno con el mismo nombre. Informa al usuario si la operación fue exitosa.

2. Listar archivos

Recorre todos los blobs del contenedor con BlobContainerClient.GetBlobs() y muestra una tabla con el nombre y el tamaño (en bytes) de cada uno. Si el contenedor está vacío, informa al usuario explícitamente.

3. Descargar archivo

Solicita el nombre del blob y valida su existencia con BlobClient.Exists(). Luego solicita una carpeta de destino local (creándola si no existe) y descarga el archivo con BlobClient.DownloadTo(), mostrando al final la ruta completa donde quedó guardado.

4. Eliminar archivo

Solicita el nombre del blob, valida que exista, y pide confirmación explícita al usuario (S/N) antes de proceder. Si se confirma, elimina el blob con BlobClient.Delete() e informa el resultado.

Manejo de errores

La aplicación maneja los errores en distintos niveles:

Validaciones previas: antes de llamar a Azure, se valida que las rutas, nombres de archivo y confirmaciones ingresadas por el usuario no estén vacías, y que el blob exista (ExisteArchivo) antes de intentar descargarlo o eliminarlo, evitando llamadas innecesarias a la API.
Excepciones específicas de Azure: cada operación captura RequestFailedException (excepción propia del SDK de Azure), que se lanza cuando el problema es de origen remoto (credenciales inválidas, permisos, problemas de red con el servicio, contenedor no encontrado, etc.), mostrando un mensaje claro con el detalle devuelto por Azure.
Excepciones genéricas: un bloque catch (Exception ex) adicional cubre cualquier error inesperado (por ejemplo, problemas de E/S al leer o escribir archivos locales), evitando que la aplicación se cierre abruptamente.
Falta de configuración: si la variable de entorno con la Connection String no está definida, la aplicación no intenta conectarse a Azure; se detiene de inmediato con un mensaje explicando qué falta configurar.

En todos los casos, cada método de BlobStorageService devuelve un bool (éxito/fracaso) junto con un mensaje de salida (out string mensaje), que Program.cs utiliza para informar al usuario de forma consistente.

Mecanismo utilizado para proteger la Connection String

La Connection String contiene el AccountName y el AccountKey, que otorgan acceso completo de lectura/escritura a la Storage Account. Por eso:

Nunca se escribe en el código fuente. La aplicación la obtiene exclusivamente en tiempo de ejecución mediante Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING").
Se define como variable de entorno en la sesión de PowerShell antes de ejecutar la aplicación:
powershell
   $env:AZURE_STORAGE_CONNECTION_STRING="<connection_string>"

Esta variable vive únicamente en memoria durante la sesión de la terminal y no queda persistida en ningún archivo del proyecto. 3. El .gitignore del proyecto excluye cualquier archivo de configuración que pudiera contener secretos (appsettings.local.json, .env, carpetas bin/ y obj/), previniendo que se suban accidentalmente al control de versiones. 4. Si la variable no está definida, la aplicación se detiene con un mensaje de error en lugar de solicitar la clave por consola o usar un valor por defecto, evitando que la credencial quede expuesta en el historial de comandos o en logs. 5. Recomendación para producción: reemplazar este mecanismo por Managed Identity o autenticación con Azure AD (DefaultAzureCredential), que elimina por completo la necesidad de manejar una clave secreta.

Instrucciones para ejecutar el proyecto
Requisitos previos
.NET 8 SDK instalado
Una Storage Account de Azure con Blob Storage habilitado
Pasos
Clonar o descargar el proyecto y ubicarse en la carpeta raíz:
powershell
   cd MIA_AzureBlobGD
Restaurar las dependencias del proyecto:
powershell
   dotnet restore
Obtener la Connection String desde el Portal de Azure: Storage Account → Security + networking → Access keys → Connection string.
Configurar la variable de entorno en la sesión actual de PowerShell:
powershell
   $env:AZURE_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
Verificar que quedó correctamente definida:
powershell
   echo $env:AZURE_STORAGE_CONNECTION_STRING
Ejecutar la aplicación en la misma ventana donde se definió la variable:
powershell
   dotnet run
Usar el menú interactivo para subir, listar, descargar y eliminar archivos del contenedor archivos.

Nota: la variable de entorno solo persiste durante la sesión de la terminal. Si se cierra la ventana, debe volver a definirse antes de correr dotnet run nuevamente.