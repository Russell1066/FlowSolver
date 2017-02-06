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

            string text = File.ReadAllText(loadDialog.FileName);
            return JsonConvert.DeserializeObject<T>(text);
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

            string json = JsonConvert.SerializeObject(obj);
            File.WriteAllText(saveDialog.FileName, json);
        }
    }
}
