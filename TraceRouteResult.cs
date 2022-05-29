public class TraceRouteResult
{
    public TraceRouteResult(string message, bool isComplete)
    {
        Message = message;
        IsComplete = isComplete;
    }

    public string Message
    {
        get; private set;
    }

    public bool IsComplete
    {
        get;private set;
    }
}