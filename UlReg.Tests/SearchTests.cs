using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace UlReg.Tests;

public class SearchTests
{
    [Fact]
    public void TermOnly()
    {
        var vm = new MainViewModel(new FakeAppService(), new FakeTimeProvider());
        vm.SearchTerm.Value = "Descriptor";
        vm.RefreshTable();
        vm.EntriesView.Count.Should().Be(90);
    }

    [Fact]
    public void CombineTermAndFirstOctet()
    {
        var vm = new MainViewModel(new FakeAppService(), new FakeTimeProvider());
        vm.SearchTerm.Value = "_sys";
        vm.SearchUl0.Value = "060a";
        vm.RefreshTable();
        vm.EntriesView.Count.Should().Be(6);
    }

    [Fact]
    public void CombineTermAndSecondOctet()
    {
        var vm = new MainViewModel(new FakeAppService(), new FakeTimeProvider());
        vm.SearchTerm.Value = "Descriptor";
        vm.SearchUl4.Value = "01";
        vm.RefreshTable();
        vm.EntriesView.Count.Should().Be(20);
    }

    [Fact]
    public void CombineTermAndThirdOctet()
    {
        var vm = new MainViewModel(new FakeAppService(), new FakeTimeProvider());
        vm.SearchTerm.Value = "Descriptor";
        vm.SearchUl8.Value = "03";
        vm.RefreshTable();
        vm.EntriesView.Count.Should().Be(7);
    }

    [Fact]
    public void CombineTermAndFourthOctet()
    {
        var vm = new MainViewModel(new FakeAppService(), new FakeTimeProvider());
        vm.SearchTerm.Value = "Descriptor";
        vm.SearchUl12.Value = "01012";
        vm.RefreshTable();
        vm.EntriesView.Count.Should().Be(11);
    }

    [Fact]
    public void CombineFirstAndThirdOctet()
    {
        var vm = new MainViewModel(new FakeAppService(), new FakeTimeProvider());
        vm.SearchUl0.Value = "060a";
        vm.SearchUl8.Value = "010103";
        vm.RefreshTable();
        vm.EntriesView.Count.Should().Be(7);
    }


    public class FakeAppService : IApplicationService
    {
        public async Task SetClipboardText(string text)
        {
            throw new NotImplementedException();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
