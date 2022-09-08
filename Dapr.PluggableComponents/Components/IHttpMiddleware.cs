using Microsoft.AspNetCore.Http;

namespace Dapr.PluggableComponents.Components;

public struct MiddlewareCapabilities {
    public bool HandlesHeader { get; set; }
    public bool HandlesBody { get; set; }
}

public struct MiddlewareResult {
    public HttpRequest? request { get; set; }
    public HttpResponseMessage? response { get; set; }
}

public interface IHttpMiddleware {
    public MiddlewareCapabilities Init(Dictionary<string, string> parameters);
    public MiddlewareResult HandleHeader(HttpRequest request);
    public MiddlewareResult HandleBody(HttpRequest request); 
}
