using System;

namespace SanctionsApi.Controllers;

public class ConfigIncorrectException : Exception
{
    public ConfigIncorrectException()
    {
    }

    public ConfigIncorrectException(string message) : base(message)
    {
    }

    public ConfigIncorrectException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}