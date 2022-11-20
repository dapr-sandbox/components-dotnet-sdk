namespace Dapr.PluggableComponents;

public sealed class DaprPluggableComponentApplication
{
    public static DaprPluggableComponentApplication Create(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.UseDaprPluggableComponents();
        return new DaprPluggableComponentApplication(builder);
    }

    private DaprPluggableComponentApplication(string[] args)
    {
    } 

    public void Run()
    {
    }

    public Task RunAsync()
    {
    }
}