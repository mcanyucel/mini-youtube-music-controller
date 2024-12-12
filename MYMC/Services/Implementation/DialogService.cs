using System.Windows;
using MYMC.Services.Interface;

namespace MYMC.Services.Implementation;

public class DialogService : IDialogService
{
    public void ShowMessageBox(string message, string title) => MessageBox.Show(messageBoxText: message, caption: title);

    public bool ShowYesNoMessageBox(string message, string title)
    {
        var result = MessageBox.Show(messageBoxText: message, caption: title, button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }
}