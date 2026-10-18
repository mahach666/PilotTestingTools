using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PilotTestHost.Plugin.Discovery
{
    // Тестовые сборки: папка Tests рядом с плагином, маска *Tests.dll.
    // LoadFrom в домен клиента: dll блокируется до перезапуска клиента -
    // после пересборки тестов клиент нужно перезапустить
    public static class TestAssemblyLoader
    {
        public const string TestsFolderName = "Tests";

        public static IEnumerable<Assembly> LoadAll()
        {
            var pluginDir = Path.GetDirectoryName(typeof(TestAssemblyLoader).Assembly.Location);
            if (pluginDir == null)
                yield break;

            var testsDir = Path.Combine(pluginDir, TestsFolderName);
            if (!Directory.Exists(testsDir))
                yield break;

            foreach (var dll in Directory.GetFiles(testsDir, "*Tests.dll", SearchOption.TopDirectoryOnly))
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.LoadFrom(dll);
                }
                catch
                {
                    // Незагружаемая сборка пропускается
                }

                if (assembly != null)
                    yield return assembly;
            }
        }

        public static string GetTestsDirectory()
        {
            var pluginDir = Path.GetDirectoryName(typeof(TestAssemblyLoader).Assembly.Location);
            return pluginDir == null ? TestsFolderName : Path.Combine(pluginDir, TestsFolderName);
        }
    }
}
