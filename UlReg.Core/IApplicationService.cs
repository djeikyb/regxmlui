namespace UlReg;

public interface IApplicationService
{
    Task SetClipboardText(string text);
    void Exit();
}
