namespace UlReg.Model.Services;

public interface IApplicationService
{
    Task SetClipboardText(string text);
    void Exit();
}
