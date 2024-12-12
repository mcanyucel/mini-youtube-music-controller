namespace MYMC.Services.Interface;

public interface IDialogService
{
    void ShowMessageBox(string message, string title);
    bool ShowYesNoMessageBox(string message, string title);
}