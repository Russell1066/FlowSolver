using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace FlowSolver
{
    static class FileUtilities
    {
        public static T LoadFromJson<T>(string description, string fileExtension)  where T : class, new()
        {
            var loadDialog = new OpenFileDialog()
            {
                DefaultExt = fileExtension,
                Filter = $"{description}|*{fileExtension}"
            };

            bool? load = loadDialog.ShowDialog();
            if (!load.HasValue || !load.Value)
            {
                return null;
            }

            return ReadJsonFile<T>(loadDialog.FileName);
        }

        public static T ReadJsonFile<T>(string filename) where T : class, new()
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filename));
        }

        internal static void SaveAsJson<T>(T obj, string description, string fileExtension)
        {
            var saveDialog = new SaveFileDialog()
            {
                DefaultExt = fileExtension,
                Filter = $"{description}|*{fileExtension}"
            };

            bool? save = saveDialog.ShowDialog();
            if (!save.HasValue || !save.Value)
            {
                return;
            }

            obj.WriteJsonFile(saveDialog.FileName);
        }

        public static void WriteJsonFile<T>(this T obj, string filename)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(obj));
        }
    }
}
