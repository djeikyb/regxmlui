using System.Collections.Generic;

namespace Ul;

public interface IRegister
{
    ICollection<IEntry> Entries { get; }
}

public interface IEntry
{
    public string Register { get; }
    public string Ul { get; }
}
