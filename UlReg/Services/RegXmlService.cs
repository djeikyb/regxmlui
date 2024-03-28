using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using UlRegBiz.Model.Services;
using UlRegBiz.Model.Xml;

namespace UlRegBiz.Services;

public class RegXmlService : IRegisterService
{
    private readonly Lazy<IReadOnlyCollection<RegisterEntry>> _singleton;
    private readonly XDocument _doc;

    public RegXmlService(XDocument doc)
    {
        _doc = doc;
        _singleton = new(() => DeserializeAll().ToList());
    }

    public IEnumerable<RegisterEntry> All() => _singleton.Value;

    private IEnumerable<RegisterEntry> DeserializeAll()
    {
        var xpath = $"//*[local-name() = 'Entry']/*[local-name() = 'UL']/..";
        var found = _doc.XPathSelectElements(xpath);
        return found.Select(
            el =>
            {
                var urn = el.Descendants().First(x => "UL".Equals(x.Name.LocalName)).Value;
                var ul = Ul.FromUrn(urn);
                var symbol = el.Descendants().First(x => "Symbol".Equals(x.Name.LocalName)).Value;
                var register = el.Descendants().First(x => "Register".Equals(x.Name.LocalName)).Value;
                var defDoc = el.Descendants().FirstOrDefault(x => "DefiningDocument".Equals(x.Name.LocalName))?.Value;
                return new RegisterEntry()
                {
                    Register = register, Ul = ul, Symbol = symbol, DefiningDocument = defDoc,
                };
            }
        );
    }

    public bool EntryPropertyAlwaysExists(string property)
    {
        const string xpathTotal = "count(//*[local-name() = 'Entries']/*[local-name() = 'Entry'])";
        var totalElements = (double)_doc.XPathEvaluate(xpathTotal);
        string xpathForProperty =
            $"count(//*[local-name() = 'Entries']/*[local-name() = 'Entry']/*[local-name() = '{property}'])";
        var elementsWithProperty = (double)_doc.XPathEvaluate(xpathForProperty);
        return Convert.ToInt64(totalElements) == Convert.ToInt64(elementsWithProperty);
    }

    public IEnumerable<RegisterEntry> Search(
        string? term,
        string? u0 = null,
        string? u4 = null,
        string? u8 = null,
        string? u12 = null
    )
    {
        var q = _singleton.Value.AsQueryable();
        if (u0 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(0, re.Ul, u0));
        if (u4 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(4, re.Ul, u4));
        if (u8 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(8, re.Ul, u8));
        if (u12 is { Length: > 0 }) q = q.Where(re => OctetStartsWith(12, re.Ul, u12));
        if (term is { Length: > 2 })
        {
            var t = Regex.Replace(term, "-|:| ", ".");
            Expression<Func<RegisterEntry, bool>> chain = re =>
                re.Symbol.StartsWith(term, StringComparison.InvariantCultureIgnoreCase);
            if (term is { Length: <= 8 })
            {
                Expression<Func<RegisterEntry, bool>> p0 = re => OctetStartsWith(0, re.Ul, t);
                Expression<Func<RegisterEntry, bool>> p4 = re => OctetStartsWith(4, re.Ul, t);
                Expression<Func<RegisterEntry, bool>> p8 = re => OctetStartsWith(8, re.Ul, t);
                Expression<Func<RegisterEntry, bool>> p12 = re => OctetStartsWith(12, re.Ul, t);
                chain = chain.Or(p0).Or(p4).Or(p8).Or(p12);
            }

            if (term is { Length: > 4 and <= 35 })
            {
                chain = chain.Or(re => re.Ul.ToOctets().Contains(t, StringComparison.InvariantCultureIgnoreCase));
            }

            if (term is { Length: > 4 })
            {
                chain = chain.Or(re => re.Symbol.Contains(term, StringComparison.InvariantCultureIgnoreCase));
            }

            q = q.Where(chain);
        }


        return q.ToList();
    }

    public static bool OctetStartsWith(int octet, Ul ul, string search)
    {
        return Convert.ToHexString(ul.Bytes.Span.Slice(octet, 4))
            .StartsWith(search, StringComparison.InvariantCultureIgnoreCase);
    }
}

/// https://stackoverflow.com/a/66336173/659715, Feb 23, 2021 at 15:33 Nick Nijenhuis
// public static class ExpressionExtensions
// {
//     public static Expression<Func<T, bool>> AndAlso<T>(
//         this Expression<Func<T, bool>> leftExpression,
//         Expression<Func<T, bool>> rightExpression
//     ) =>
//         Combine(leftExpression, rightExpression, Expression.AndAlso);
//
//     public static Expression<Func<T, bool>> Or<T>(
//         this Expression<Func<T, bool>> leftExpression,
//         Expression<Func<T, bool>> rightExpression
//     ) =>
//         Combine(leftExpression, rightExpression, Expression.Or);
//
//     public static Expression<Func<T, bool>> Combine<T>(
//         Expression<Func<T, bool>> leftExpression,
//         Expression<Func<T, bool>> rightExpression,
//         Func<Expression, Expression, BinaryExpression> combineOperator
//     )
//     {
//         var leftParameter = leftExpression.Parameters[0];
//         var rightParameter = rightExpression.Parameters[0];
//
//         var visitor = new ReplaceParameterVisitor(rightParameter, leftParameter);
//
//         var leftBody = leftExpression.Body;
//         var rightBody = visitor.Visit(rightExpression.Body);
//
//         return Expression.Lambda<Func<T, bool>>(combineOperator(leftBody, rightBody), leftParameter);
//     }
//
//     private class ReplaceParameterVisitor : ExpressionVisitor
//     {
//         private readonly ParameterExpression _oldParameter;
//         private readonly ParameterExpression _newParameter;
//
//         public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
//         {
//             _oldParameter = oldParameter;
//             _newParameter = newParameter;
//         }
//
//         protected override Expression VisitParameter(ParameterExpression node)
//         {
//             return ReferenceEquals(node, _oldParameter) ? _newParameter : base.VisitParameter(node);
//         }
//     }
// }

/// http://www.albahari.com/nutshell/predicatebuilder.aspx
public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> True<T>() { return f => true; }
    public static Expression<Func<T, bool>> False<T>() { return f => false; }

    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2
    )
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }

    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2
    )
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
    }
}
