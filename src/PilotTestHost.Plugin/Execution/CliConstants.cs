namespace PilotTestHost.Plugin.Execution
{
    // Протокол внешнего запуска. Имена дублируются в PilotTestHost.Cli
    // (net8-консоль не может ссылаться на net48-сборку) - менять синхронно
    public static class CliConstants
    {
        public const string CliModeVariable = "PILOTTESTHOST_CLI";
        public const string FilterVariable = "PILOTTESTHOST_FILTER";
        public const string OutputVariable = "PILOTTESTHOST_OUTPUT";
    }
}
