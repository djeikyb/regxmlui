// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using RegXml;
using UlReg;

namespace Benchmark;

internal static class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<Deserialize>();
    }
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
// [SimpleJob(RuntimeMoniker.NativeAot90)]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
public class Deserialize
{
    [Benchmark]
    public IReadOnlyCollection<RegisterEntry> Sax()
    {
        var sax = Registers.FromSax();
        new MultiRegisterRegXmlService(sax.Essence);
        return new MultiRegisterRegXmlService(
            sax.Elements,
            sax.Elements,
            sax.Essence,
            sax.Groups,
            sax.Labels,
            sax.Types
        ).All();
    }

    [Benchmark(Baseline = true)]
    public IReadOnlyCollection<RegisterEntry> XDocument()
    {
        var xdoc = Registers.FromXPath();
        return new MultiRegisterRegXmlService(
            xdoc.Elements,
            xdoc.Essence,
            xdoc.Groups,
            xdoc.Labels,
            xdoc.Types
        ).All();
    }
}
