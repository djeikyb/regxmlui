using Avalonia.Controls;

namespace UlReg;

public class ApplicationService(Window window) : IApplicationService
{
    private readonly TopLevel _topLevel = TopLevel.GetTopLevel(window) ?? throw new Exception("No top level from window.");

    public async Task SetClipboardText(string text)
    {
        if (_topLevel.Clipboard == null) return;
        await _topLevel.Clipboard.SetTextAsync(text);
    }

    public void Exit()
    {
        throw new NotImplementedException();
    }
}
