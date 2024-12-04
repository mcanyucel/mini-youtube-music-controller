namespace MYMC.Models;

public class PlayerCommandMessage(PlayerCommandType commandType, object? parameter = null)
{
    public PlayerCommandType CommandType { get; } = commandType;
    public object? Parameter { get;  } = parameter;
}