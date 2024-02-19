namespace UlReg.Model.Xml;

public class RegisterEntry
{
    public required string Register { get; set; }
    public required Ul Ul { get; set; }
    public required string Symbol { get; set; }
    public string? DefiningDocument { get; set; }
}
