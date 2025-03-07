namespace ImageRenamer
{
    class Program
    {
        static void Main(string[] args)
        {

            string rutaCarpeta;

            Console.WriteLine("Ingrese la palabra que desea agregar al nombre de las imágenes:");
            string palabra = Console.ReadLine();

            if (args.Length > 0)
            {
                rutaCarpeta = args[0];
            }
            else
            {
                Console.WriteLine("Ingrese la ruta de la carpeta que contiene las imágenes:");
                rutaCarpeta = Console.ReadLine();
            }

            // Verificar si la carpeta existe
            if (!Directory.Exists(rutaCarpeta))
            {
                Console.WriteLine("La ruta de la carpeta no es válida.");
                return;
            }

            // Obtener la lista de archivos de imagen en la carpeta
            string[] archivos = Directory.GetFiles(rutaCarpeta, "*.jpg");

            if (archivos.Length == 0)
            {
                Console.WriteLine("No se encontraron imágenes en la carpeta.");
                return;
            }

            // Ordenar los archivos por fecha de creación
            Array.Sort(archivos, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));

            // Renombrar los archivos
            for (int i = 0; i < archivos.Length; i++)
            {
                string nuevoNombre = $"{palabra}_{(i + 1).ToString("000")}.jpg";
                string nuevaRuta = Path.Combine(Path.GetDirectoryName(archivos[i]), nuevoNombre);
                File.Move(archivos[i], nuevaRuta);
                Console.WriteLine($"Renombrado: {archivos[i]} => {nuevaRuta}");
            }

            Console.WriteLine("Proceso completado. Presione cualquier tecla para salir...");
            Console.ReadKey();

        }
    }
}
