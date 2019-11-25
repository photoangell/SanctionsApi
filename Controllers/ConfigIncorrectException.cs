using System;
namespace SanctionsApi.Exceptions{
    public class ConfigIncorrectException : Exception {
        //move this to controller class?
        public ConfigIncorrectException(string message, System.Exception innerException)
    : base(message, innerException) { }
        
    }
}