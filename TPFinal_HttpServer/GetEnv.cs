using System;
using System.IO;
using System.Text.Json;

namespace GetServerEnv
{
    internal sealed class ServerEnv
    {
        private static ServerEnv? _instance;
        private const string envFileName = "serverConfig.json";
        private static ConfigJSONParams? config;


        private ServerEnv()
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), envFileName);


                if (File.Exists(filePath))
                {
                    string fileContent = File.ReadAllText(filePath);
                    Console.WriteLine("Cargando archivo de configuración...");
                    config = JsonSerializer.Deserialize<ConfigJSONParams>(fileContent);
                    if (config == null)
                    {
                        throw new Exception("Erorr en JsonSerializer");
                    }
                    Console.WriteLine("Configuración cargada:\n\tHOST: {0}\n\tPORT: {1}\n\tSERVE_FOLDER: {2}\n", config.host, config.port, config.serve_path);
                }
                else
                {
                    Console.WriteLine($"Error: Error en carga de archivo de configuración: '{envFileName} en {filePath} no encontrado.'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        
        public static ServerEnv ServerEnvInstance
        {
            get
            {
                //Crea la instancia solo si no existe
                if (_instance == null)
                {
                    _instance = new ServerEnv();
                }
                return _instance;
            }
        }

        public int GetPort()
        {
            if (config != null)
            {
                return config.port;
            }
            else
            {
                return -1;
            }
        }
        public string GetHost()
        {
            if (config != null)
            {
                return config.host;
            }
            else
            {
                return "";
            }
        }
        public string GetServePath()
        {
            if(config != null)
            {
                return config.serve_path;
            }
            else
            {
                return "";
            }
        }

    }
    
    internal class ConfigJSONParams
    {
        public string host { get; set; }
        public int port { get; set; }
        public string serve_path { get; set; }
    }
}