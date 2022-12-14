namespace Dapr.PluggableComponents;

public static class Constants
{
    public static class Defaults
    {
        public const string DaprComponentsSocketsExtension = ".sock";
        public const string DaprComponentsSocketsFolder = "/tmp/dapr-components-sockets";
    }

    public static class EnvironmentVariables
    {
        public const string DaprComponentsSocketsExtension = "DAPR_COMPONENTS_SOCKETS_EXTENSION";

        public const string DaprComponentsSocketsFolder = "DAPR_COMPONENTS_SOCKETS_FOLDER";
    }
}