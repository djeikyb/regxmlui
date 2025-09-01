using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using ObservableCollections;
using R3;
using RegXml;
using UlReg;

namespace SearchBench;

class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<SearchBenchJobs>();
    }
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
// [SimpleJob(RuntimeMoniker.NativeAot90)]
// [EventPipeProfiler(EventPipeProfile.CpuSampling)]
public class SearchBenchJobs
{
    private static readonly MultiRegisterRegXmlService _register;

    static SearchBenchJobs()
    {
        var registers = Registers.FromEmbedded();
        _register = new MultiRegisterRegXmlService(
            registers.Elements,
            registers.Essence,
            registers.Groups,
            registers.Labels,
            registers.Types
        );
    }

    [Benchmark(Baseline = true)]
    public NotifyCollectionChangedSynchronizedViewList<RegisterEntry> Search()
    {
        var vm = new MvmClearAdd(_register);
        return Paces(vm);
    }

    [Benchmark]
    public NotifyCollectionChangedSynchronizedViewList<RegisterEntry> Search2()
    {
        var vm = new MvmClearAddSearch2(_register);
        return Paces(vm);
    }

    [Benchmark]
    public IReadOnlyCollection<RegisterEntry> Filter()
    {
        var vm = new FilterExpressions(_register);
        return Paces(vm);
    }

    private NotifyCollectionChangedSynchronizedViewList<RegisterEntry> Paces(IMvm vm)
    {
        vm.SearchTerm.Value = "descri";
        vm.RefreshTable();

        vm.SearchTerm.Value = "descriptor";
        vm.RefreshTable();

        vm.SearchTerm.Value = "jpeg";
        vm.RefreshTable();

        vm.SearchTerm.Value = "jpeg2000";
        vm.RefreshTable();

        vm.SearchTerm.Value = "descriptor";
        vm.SearchUl4.Value = "02";
        vm.RefreshTable();

        vm.SearchTerm.Value = "aux";
        vm.RefreshTable();

        vm.SearchUl4.Value = null;
        vm.RefreshTable();

        return vm.Entries;
    }
}

public interface IMvm
{
    void RefreshTable();
    BindableReactiveProperty<string?> SearchTerm { get; }
    BindableReactiveProperty<string?> SearchUl0 { get; }
    BindableReactiveProperty<string?> SearchUl4 { get; }
    BindableReactiveProperty<string?> SearchUl8 { get; }
    BindableReactiveProperty<string?> SearchUl12 { get; }
    NotifyCollectionChangedSynchronizedViewList<RegisterEntry> Entries { get; set; }
}
