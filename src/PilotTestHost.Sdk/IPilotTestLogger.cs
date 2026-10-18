namespace PilotTestHost.Sdk
{
    public interface IPilotTestLogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
    }
}
