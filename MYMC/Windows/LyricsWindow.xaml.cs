using MYMC.Services.Interface;

namespace MYMC.Windows;

public partial class LyricsWindow
{
    // ReSharper disable once NotAccessedField.Local - Required for DI
    private readonly IPlayerCommandBus _commandBus;

    public LyricsWindow(IPlayerCommandBus commandBus)
    {
        _commandBus = commandBus;
        InitializeComponent();
    }
}