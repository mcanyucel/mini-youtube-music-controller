namespace MYMC.AutoUpdate;

public class UpdateErrorEventArgs(string message, Exception? exception) : EventArgs
{
    public string Message { get; } = message;
    public Exception? Exception { get; } = exception;
}