using System;

namespace PilotTestHost.Sdk
{
    // Тест, выполняющийся внутри клиента Pilot.
    // Метод: public, экземплярный, void, без параметров или с одним ITestContext
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class PilotFactAttribute : Attribute
    {
        // Непустая причина - тест пропускается
        public string Skip { get; set; }
    }
}
