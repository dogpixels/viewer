using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace dogpixels_viewer
{
    internal class ConfigurationFactory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string GetDirectoryPath()
        {
            string path = Path.Combine
            (
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "dogpixels",
                "viewer",
                "config"
            );

            Directory.CreateDirectory(path);

            return path;
        }
        public static string GetFilePath(string profileName)
        {
            string path = Path.Combine
            (
                GetDirectoryPath(),
                profileName + ".json"
            );
            
            return path;
        }

        public static string? GetLastProfileName()
        {
            string path = Path.Combine
            (
                GetDirectoryPath(),
                ".lastprofile"
            );

            if (File.Exists(path))
            {
                try
                {
                    return File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            }

            return null;
        }

        public static void SetLastProfileName(string profileName)
        {
            string path = Path.Combine
            (
                GetDirectoryPath(),
                ".lastprofile"
            );

            try
            {
                File.WriteAllText(path, profileName);
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }

        public static Configuration? Read(string name)
        {
            FileStream? fs = null;
            Configuration? ret = null;

            string path = GetFilePath(name);

            log.Info($"Initializing configuration profile '{name}'...");
            log.Debug($"path: {path}");

            if (!File.Exists(path))
            {
                log.Warn($"Configuration file does not exist: '{path}'");
                //ret = new Configuration(name);
                log.Info($"Created new configuration profile name: '{name}'");
                log.Debug($"designated path: {path}");

                bool? result = false;

                while (result != true) {
                    ConfigurationWindow cw = new ConfigurationWindow(new Configuration(name));
                    ret = cw.Configuration;
                    result = cw.ShowDialog();
                }

                if (!Write(ret))
                {
                    return null;
                }
                return ret;
            }

            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                ret = JsonSerializer.Deserialize<Configuration>(fs);
            }
            catch (Exception ex)
            {
                log.Fatal($"Failed to deserialize configuration from file '{path}':");
                log.Fatal(ex);
            }
            finally
            {
                fs?.Close();
            }

            return ret;
        }

        public static bool Write(Configuration config)
        {
            FileStream? fs = null;

            string path = GetFilePath(config.ProfileName);

            try
            {
                fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                JsonSerializer.Serialize(fs, config);
                fs.Close();
            }
            catch(Exception ex)
            {
                fs?.Close();
                log.Fatal($"Failed to write configuration file. Attempted path: '{path}'");
                log.Fatal(ex);
                return false;
            }
            log.Info($"Saved configuration profile '{config.ProfileName}'");
            log.Debug($"path: {path}");
            return true;
        }
    }
}
