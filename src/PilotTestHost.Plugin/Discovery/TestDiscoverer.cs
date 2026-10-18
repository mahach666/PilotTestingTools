using PilotTestHost.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PilotTestHost.Plugin.Discovery
{
    public static class TestDiscoverer
    {
        public static List<TestDescriptor> DiscoverAll()
        {
            var descriptors = new List<TestDescriptor>();

            foreach (var assembly in TestAssemblyLoader.LoadAll())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    // Частично загруженная сборка: работаем с тем, что загрузилось
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in types)
                {
                    if (type == null || !type.IsClass || type.IsAbstract)
                        continue;
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var fact = method.GetCustomAttribute<PilotFactAttribute>();
                        if (fact == null)
                            continue;

                        descriptors.Add(Describe(type, method, fact));
                    }
                }
            }

            return descriptors;
        }

        private static TestDescriptor Describe(Type type, MethodInfo method, PilotFactAttribute fact)
        {
            var parameters = method.GetParameters();

            // Неверная сигнатура - ошибка в отчете, а не молчаливый пропуск
            string signatureError = null;
            var takesContext = false;

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(ITestContext))
                takesContext = true;
            else if (parameters.Length != 0)
                signatureError = "метод должен быть без параметров или с одним ITestContext";

            if (method.ReturnType != typeof(void))
                signatureError = "метод должен возвращать void (async-тесты не поддерживаются)";

            return new TestDescriptor(type, method, fact.Skip, takesContext, signatureError);
        }
    }
}
