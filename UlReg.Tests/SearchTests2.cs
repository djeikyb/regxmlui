using SearchBench;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using RegXml;

namespace UlReg.Tests;

public class SearchTests2
{
    private static readonly MultiRegisterRegXmlService _register;

    static SearchTests2()
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

    [Fact]
    public void TermOnly()
    {
        var vm = new FilterView(_register);
        vm.SearchTerm.Value = "Descriptor";
        vm.RefreshTable();
        vm.Entries.Count.Should().Be(90);
    }

    [Fact]
    public void CombineTermAndFirstOctet()
    {
        var vm = new FilterView(_register);
        vm.SearchTerm.Value = "_sys";
        vm.SearchUl0.Value = "060a";
        vm.RefreshTable();
        vm.Entries.Count.Should().Be(6);
    }

    [Fact]
    public void CombineTermAndSecondOctet()
    {
        var vm = new FilterView(_register);
        vm.SearchTerm.Value = "Descriptor";
        vm.SearchUl4.Value = "01";
        vm.RefreshTable();
        vm.Entries.Count.Should().Be(20);
    }

    [Fact]
    public void CombineTermAndThirdOctet()
    {
        var vm = new FilterView(_register);
        vm.SearchTerm.Value = "Descriptor";
        vm.SearchUl8.Value = "03";
        vm.RefreshTable();
        vm.Entries.Count.Should().Be(7);
    }

    [Fact]
    public void CombineTermAndFourthOctet()
    {
        var vm = new FilterView(_register);
        vm.SearchTerm.Value = "Descriptor";
        vm.SearchUl12.Value = "01012";
        vm.RefreshTable();
        vm.Entries.Count.Should().Be(11);
    }

    [Fact]
    public void CombineFirstAndThirdOctet()
    {
        var vm = new FilterView(_register);
        vm.SearchUl0.Value = "060a";
        vm.SearchUl8.Value = "010103";
        vm.RefreshTable();
        vm.Entries.Count.Should().Be(7);
    }


}
