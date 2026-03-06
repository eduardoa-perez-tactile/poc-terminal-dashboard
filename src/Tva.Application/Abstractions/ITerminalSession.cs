namespace Tva.Application;

public interface ITerminalSession
{
    void SendInput(string input);
    IAsyncEnumerable<string> Output { get; }
    Task CloseAsync();
}
