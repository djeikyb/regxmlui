using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using ObservableCollections;
using RegXml;
using UlReg;

namespace SearchBaseline;

class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<SearchBenchJobs>();
    }
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90, baseline: true)]
[SimpleJob(RuntimeMoniker.NativeAot90)]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
public class SearchBenchJobs
{
    private Registers _registers;
    private BenchAppService AppService;
    private MainViewModel _vm;

    // static SearchBenchJobs()
    // {
    //     _registers = Registers.FromSax();
    // }

    [GlobalSetup]
    public void Setup()
    {
        _registers = Registers.FromSax();
        AppService = new BenchAppService();
        _vm = new MainViewModel(AppService, registers: _registers);
    }

    [Benchmark]
    public NotifyCollectionChangedSynchronizedViewList<RegisterEntry> Baseline()
    {
        _vm.SearchTerm.Value = "descri";
        _vm.RefreshTable();

        _vm.SearchTerm.Value = "descriptor";
        _vm.RefreshTable();

        _vm.SearchTerm.Value = "jpeg";
        _vm.RefreshTable();

        _vm.SearchTerm.Value = "jpeg2000";
        _vm.RefreshTable();

        _vm.SearchTerm.Value = "descriptor";
        _vm.SearchUl4.Value = "02";
        _vm.RefreshTable();

        _vm.SearchTerm.Value = "aux";
        _vm.RefreshTable();

        _vm.SearchUl4.Value = null;
        _vm.RefreshTable();

        return _vm.EntriesView;
    }
}

public class BenchAppService : IApplicationService
{
    public Task SetClipboardText(string text)
    {
        return Task.CompletedTask;
    }

    public void Exit()
    {
    }
}
