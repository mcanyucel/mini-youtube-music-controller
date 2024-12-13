using System.Windows;
using MYMC.Services.Interface;

namespace MYMC.Services.Implementation;

public class SystemService : ISystemService
{
    public void CopyToClipboard(string text) => Clipboard.SetText(text);
}