using System.Linq.Expressions;
using System.Text.RegularExpressions;
using ObservableCollections;
using R3;
using RegXml;
using UlReg;

namespace SearchBench;

public class FilterExpressions : IMvm
{
    private readonly MultiRegisterRegXmlService _register;
    private readonly ObservableList<RegisterEntry> _entries;
    private readonly ISynchronizedView<RegisterEntry, RegisterEntry> _entriesView;

    public FilterExpressions(MultiRegisterRegXmlService register)
    {
        _register = register;
        _entries = new ObservableList<RegisterEntry>(register.All());

        _entriesView = _entries.CreateView(x => x);
        Entries = _entriesView.ToNotifyCollectionChanged();

        SearchTerm = new BindableReactiveProperty<string?>(string.Empty);
        SearchUl0 = new();
        SearchUl4 = new();
        SearchUl8 = new();
        SearchUl12 = new();
    }

    public void RefreshTable()
    {
        if (SearchUl0.Value is not { Length: > 0 }
            && SearchUl4.Value is not { Length: > 0 }
            && SearchUl8.Value is not { Length: > 0 }
            && SearchUl12.Value is not { Length: > 0 }
            && SearchTerm.Value is not { Length: > 2 })
        {
            _entriesView.ResetFilter();
        }
        else
        {
            var filter = new ExpressionFilter(
                term: SearchTerm.Value,
                u0: SearchUl0.Value,
                u4: SearchUl4.Value,
                u8: SearchUl8.Value,
                u12: SearchUl12.Value
            );
            _entriesView.AttachFilter(re => filter.Match(re));
        }
    }

    public BindableReactiveProperty<string?> SearchTerm { get; }
    public BindableReactiveProperty<string?> SearchUl0 { get; }
    public BindableReactiveProperty<string?> SearchUl4 { get; }
    public BindableReactiveProperty<string?> SearchUl8 { get; }
    public BindableReactiveProperty<string?> SearchUl12 { get; }
    public NotifyCollectionChangedSynchronizedViewList<RegisterEntry> Entries { get; set; }
}

public class ExpressionFilter
{
    private readonly Func<RegisterEntry, bool> _compexp;

    public ExpressionFilter(
        string? term,
        string? u0 = null,
        string? u4 = null,
        string? u8 = null,
        string? u12 = null)
    {
        _compexp = Express(term, u0, u4, u8, u12).Compile();
    }

    public bool Match(RegisterEntry re) => _compexp(re);

    public Expression<Func<RegisterEntry, bool>> Express(
        string? term,
        string? u0 = null,
        string? u4 = null,
        string? u8 = null,
        string? u12 = null
    )
    {
        Expression<Func<RegisterEntry, bool>> q = PredicateBuilder.True<RegisterEntry>();
        if (u0 is { Length: > 0 }) q = q.And(re => re.Ul._s_oct0.StartsWith(u0, StringComparison.OrdinalIgnoreCase));
        if (u4 is { Length: > 0 }) q = q.And(re => re.Ul._s_oct4.StartsWith(u4, StringComparison.OrdinalIgnoreCase));
        if (u8 is { Length: > 0 }) q = q.And(re => re.Ul._s_oct8.StartsWith(u8, StringComparison.OrdinalIgnoreCase));
        if (u12 is { Length: > 0 })
            q = q.And(re => re.Ul._s_oct12.StartsWith(u12, StringComparison.OrdinalIgnoreCase));

        if (term is { Length: > 2 })
        {
            var t = Regex.Replace(term, "-|:| ", ".");
            Expression<Func<RegisterEntry, bool>> chain = re =>
                re.Symbol.StartsWith(term, StringComparison.InvariantCultureIgnoreCase);
            if (term is { Length: <= 8 })
            {
                Expression<Func<RegisterEntry, bool>> p0 = re => re.Ul._s_oct0.StartsWith(t);
                Expression<Func<RegisterEntry, bool>> p4 = re => re.Ul._s_oct4.StartsWith(t);
                Expression<Func<RegisterEntry, bool>> p8 = re => re.Ul._s_oct8.StartsWith(t);
                Expression<Func<RegisterEntry, bool>> p12 = re => re.Ul._s_oct12.StartsWith(t);
                chain = chain.Or(p0).Or(p4).Or(p8).Or(p12);
            }

            if (term is { Length: > 4 and <= 35 })
            {
                chain = chain.Or(re => re.Ul.ToOctets().Contains(t, StringComparison.InvariantCultureIgnoreCase));
            }

            if (term is { Length: >= 3 })
            {
                chain = chain.Or(re => re.Symbol.Contains(term, StringComparison.InvariantCultureIgnoreCase));
            }

            if (term is { Length: >= 3 })
            {
                chain = chain.Or(re =>
                    re.DefiningDocument != null &&
                    re.DefiningDocument.Contains(term, StringComparison.InvariantCultureIgnoreCase));
            }

            q = q.And(chain);
        }

        return q;
    }
}
