using System;

namespace SanctionsApi.Exceptions;

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