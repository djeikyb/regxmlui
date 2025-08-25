namespace RegXml;

public class RegisterLoadException : Exception
{
    public RegisterLoadException(string register) : base($"Missing register: {register}")
    {
    }
}
