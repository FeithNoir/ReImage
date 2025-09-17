
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageRenamer
{
    public class HistoryEntry
    {
        public DateTime Timestamp { get; set; }
        public string BaseName { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
    }

    class Program
    {
        private static readonly string historyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.json");

        static async Task Main(string[] args)
        {
            while (true)
            {
                ShowMenu();
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await RenameImagesProcess();
                        break;
                    case "2":
                        await ShowHistory();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Opción no válida. Por favor, intente de nuevo.");
                        break;
                }
            }
        }

        static void ShowMenu()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException)
            {
                // Ignorar el error si no se puede limpiar la consola (p. ej., en un entorno no interactivo)
            }
            Console.WriteLine("=====================================");
            Console.WriteLine("        RENOMBRADOR DE IMÁGENES      ");
            Console.WriteLine("=====================================");
            Console.WriteLine("1. Renombrar imágenes de una carpeta");
            Console.WriteLine("2. Ver historial de renombrados");
            Console.WriteLine("3. Salir");
            Console.WriteLine("=====================================");
            Console.Write("Seleccione una opción: ");
        }

        static async Task RenameImagesProcess()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException) { }

            Console.WriteLine("--- Renombrar Imágenes ---");

            Console.WriteLine("Ingrese la palabra base para el nombre de las imágenes:");
            string baseName = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Ingrese la ruta de la carpeta que contiene las imágenes:");
            string folderPath = Console.ReadLine() ?? string.Empty;

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("La ruta de la carpeta no es válida o está vacía.");
                return;
            }

            string[] supportedFormats = { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp", "*.tiff" };
            var files = supportedFormats
                .SelectMany(format => Directory.GetFiles(folderPath, format))
                .ToArray();

            if (files.Length == 0)
            {
                Console.WriteLine("No se encontraron imágenes en la carpeta.");
                return;
            }

            Array.Sort(files, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));

            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    string? directory = Path.GetDirectoryName(files[i]);
                    if (string.IsNullOrEmpty(directory)) continue;

                    string extension = Path.GetExtension(files[i]);
                    string newName = $"{baseName}_{(i + 1):000}{extension}";
                    string newPath = Path.Combine(directory, newName);
                    File.Move(files[i], newPath);
                    Console.WriteLine($"Renombrado: {Path.GetFileName(files[i])} => {newName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al renombrar {files[i]}: {ex.Message}");
                }
            }

            Console.WriteLine("\nProceso de renombrado completado.");

            Console.Write("¿Desea guardar esta operación en el historial? (S/N): ");
            string? saveChoice = Console.ReadLine();
            if (saveChoice?.Equals("S", StringComparison.OrdinalIgnoreCase) == true)
            {
                var entry = new HistoryEntry
                {
                    Timestamp = DateTime.Now,
                    BaseName = baseName,
                    FolderPath = folderPath
                };
                await SaveHistory(entry);
            }
        }

        static async Task SaveHistory(HistoryEntry entry)
        {
            List<HistoryEntry> history = new List<HistoryEntry>();
            if (File.Exists(historyFilePath))
            {
                string json = await File.ReadAllTextAsync(historyFilePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var deserializedHistory = JsonSerializer.Deserialize<List<HistoryEntry>>(json);
                    if (deserializedHistory != null)
                    {
                        history = deserializedHistory;
                    }
                }
            }
            
            history.Add(entry);
            
            string newJson = JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(historyFilePath, newJson);
            
            Console.WriteLine("La operación ha sido guardada en el historial.");
        }

        static async Task ShowHistory()
        {
            try
            {
                Console.Clear();
            }
            catch (IOException) { }

            Console.WriteLine("--- Historial de Operaciones ---");

            if (!File.Exists(historyFilePath))
            {
                Console.WriteLine("El historial está vacío.");
                return;
            }

            string json = await File.ReadAllTextAsync(historyFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("El historial está vacío.");
                return;
            }

            var history = JsonSerializer.Deserialize<List<HistoryEntry>>(json);

            if (history == null || history.Count == 0)
            {
                Console.WriteLine("El historial está vacío.");
                return;
            }

            foreach (var entry in history.OrderByDescending(e => e.Timestamp))
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"Fecha:      {entry.Timestamp:g}");
                Console.WriteLine($"Nombre Base: {entry.BaseName}");
                Console.WriteLine($"Carpeta:    {entry.FolderPath}");
            }
            Console.WriteLine("----------------------------------------");
        }
    }
}
