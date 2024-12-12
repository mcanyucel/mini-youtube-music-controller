using MYMC.Services.Interface;
using Serilog;

namespace MYMC.Windows;

public partial class LyricsWindow
{
    // ReSharper disable once NotAccessedField.Local - Required for DI
    private readonly IPlayerCommandBus _commandBus;

    public LyricsWindow(IPlayerCommandBus commandBus, ILogger logger)
    {
        _commandBus = commandBus;
        InitializeComponent();
        logger.Debug("Lyrics window initialized");
    }
}