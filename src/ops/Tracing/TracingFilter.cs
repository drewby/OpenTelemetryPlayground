namespace Ops.Tracing;

public class TracingFilter
{
    private readonly string[] incomingIgnoreList;
    private readonly string[] outgoingIgnoreList;

    public TracingFilter(string incomingIgnore, string outgoingIgnore)
    {
        incomingIgnoreList = ParseIgnoreList(incomingIgnore);
        outgoingIgnoreList = ParseIgnoreList(outgoingIgnore);
    }

    public bool FilterIncoming(HttpContext context)
    {
        if (!context.Request.Path.HasValue)
        {
            return true;
        }
        return Filter(context.Request.Path.Value, incomingIgnoreList);
    }

    public bool FilterOutgoing(HttpRequestMessage context)
    {
        if (context.RequestUri == null)
        {
            return true;
        }
        return Filter(context.RequestUri.ToString(), outgoingIgnoreList);
    }

    private string[] ParseIgnoreList(string ignoreList) => String.IsNullOrWhiteSpace(ignoreList) ? new string[0] :
            ignoreList.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private bool Filter(string uri, string[] ignoreList)
    {
        foreach (string ignore in ignoreList)
        {
            if (uri.StartsWith(ignore))
            {
                return false;
            }
        }
        return true;
    }
}
