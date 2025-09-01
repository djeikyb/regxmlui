namespace RegXml;

public interface IRegister
{
    IReadOnlyCollection<RegisterEntry> All();

    IEnumerable<RegisterEntry> Search(
        string? term,
        string? u0 = null,
        string? u4 = null,
        string? u8 = null,
        string? u12 = null
    );
}
