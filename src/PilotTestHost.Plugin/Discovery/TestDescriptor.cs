using System;
using System.Reflection;

namespace PilotTestHost.Plugin.Discovery
{
    public sealed class TestDescriptor
    {
        public TestDescriptor(Type declaringType, MethodInfo method, string skipReason,
            bool takesContext, string signatureError = null)
        {
            DeclaringType = declaringType;
            Method = method;
            SkipReason = skipReason;
            TakesContext = takesContext;
            SignatureError = signatureError;
        }

        public Type DeclaringType { get; }
        public MethodInfo Method { get; }
        public string SkipReason { get; }
        public bool TakesContext { get; }

        // Непустое значение: метод помечен атрибутом, но сигнатура неверна -
        // тест попадет в отчет ошибкой, а не пропадет молча
        public string SignatureError { get; }

        public string ClassFullName => DeclaringType.FullName;
        public string MethodName => Method.Name;
    }
}
