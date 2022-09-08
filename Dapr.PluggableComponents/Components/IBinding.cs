namespace Dapr.PluggableComponents.Components;

public struct BindingResult
{
    public string contentType { get; set; }
    public byte[] data { get; set; }
    public Dictionary<string, string> metadata { get; set; }
}

public interface IInputBinding
{
    string Name();
    void Init(Dictionary<string, string> properties);
    BindingResult Read();
    void Ping();
}

public struct InvokeResult
{
    public string contentType { get; set; }
    public byte[] data { get; set; }
    public Dictionary<string, string> metadata { get; set; }
}

public interface IOutputBinding
{
    string Name();
    void Init(Dictionary<string, string> properties);
    InvokeResult Invoke(string operation, byte[] req, Dictionary<string, string> metadata);
    void Ping();
}
