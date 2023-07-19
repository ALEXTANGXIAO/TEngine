#if TENGINE_NET
using NLog;

namespace TEngine;

public class NLog : ILog
{
    private readonly Logger _logger;

    public NLog(string name)
    {
        _logger = LogManager.GetLogger(name);
    }

    public void Trace(string message)
    {
        _logger.Trace(message);
    }

    public void Warning(string message)
    {
        _logger.Warn(message);
    }

    public void Info(string message)
    {
        _logger.Info(message);
    }

    public void Debug(string message)
    {
        _logger.Debug(message);
    }

    public void Error(string message)
    {
        _logger.Error(message);
    }

    public void Fatal(string message)
    {
        _logger.Fatal(message);
    }

    public void Trace(string message, params object[] args)
    {
        _logger.Trace(message, args);
    }

    public void Warning(string message, params object[] args)
    {
        _logger.Warn(message, args);
    }

    public void Info(string message, params object[] args)
    {
        _logger.Info(message, args);
    }

    public void Debug(string message, params object[] args)
    {
        _logger.Debug(message, args);
    }

    public void Error(string message, params object[] args)
    {
        _logger.Error(message, args);
    }

    public void Fatal(string message, params object[] args)
    {
        _logger.Fatal(message, args);
    }
}
#endif