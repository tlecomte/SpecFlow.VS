using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using FluentAssertions;
using SpecFlow.VisualStudio.Discovery;
using SpecFlow.VisualStudio.ProjectSystem;
using SpecFlow.VisualStudio.SpecFlowConnector.Models;
using SpecFlow.VisualStudio.VsxStubs.ProjectSystem;
using SpecFlow.VisualStudio.VsxStubs.StepDefinitions;
using Xunit;
using Xunit.Abstractions;

namespace SpecFlow.VisualStudio.Tests.Discovery;

[UseReporter/*(typeof(VisualStudioReporter))*/]
[UseApprovalSubdirectory("../ApprovalTestData")]
public class ReprocessStepDefinitionFileTests
{
    private readonly InMemoryStubProjectScope _projectScope;

    public ReprocessStepDefinitionFileTests(ITestOutputHelper testOutputHelper)
    {
        StubIdeScope ideScope = new StubIdeScope(testOutputHelper);
        _projectScope = new InMemoryStubProjectScope(ideScope);
    }

    [Theory]
    [InlineData("IPressAdd.cs")]
    public async Task Approval(string testName)
    {
        //arrange
        NamerFactory.AdditionalInformation = testName;
        var namer = Approvals.GetDefaultNamer();
        var stepDefinitionPath = Path.Combine(namer.SourcePath, namer.Name);
        NamerFactory.AdditionalInformation = testName;
        var content = File.ReadAllText(stepDefinitionPath);
        var stepDefinitionFile = new CSharpStepDefinitionFile(stepDefinitionPath, content);

        _projectScope.AddSpecFlowPackage();
        var discoveryService =
            MockableDiscoveryService.SetupWithInitialStepDefinitions(_projectScope, Array.Empty<StepDefinition>(), TimeSpan.Zero);
        discoveryService.WaitUntilDiscoveryPerformed();
        // _projectScope.AddFile("test.feature", string.Empty);


//        discoveryService.InitializeBindingRegistry();

        //act
        await discoveryService.ProcessAsync(stepDefinitionFile);

        //assert
        ProjectBindingRegistry bindingRegistry = await discoveryService.GetBindingRegistryAsync();
        var dumped = Dump(bindingRegistry);
        Approvals.Verify(dumped);
    }

    public string Dump(ProjectBindingRegistry bindingRegistry)
    {
        var sb = new StringBuilder("ProjectBindingRegistry");
        sb.AppendLine();
        if (bindingRegistry.IsFailed) sb.AppendLine("  Failed");
        int i = 0;
        foreach (ProjectStepDefinitionBinding binding in bindingRegistry.StepDefinitions)
        {
            sb.AppendLine($"  ProjectStepDefinitionBinding-{i}");
            sb.AppendLine(Dump(binding));
        }

        return sb.ToString();
    }
    
    public string Dump(ProjectStepDefinitionBinding binding)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"    {nameof(binding.IsValid)}:`{binding.IsValid}`");
        sb.AppendLine($"    {nameof(binding.Error)}:`{binding.Error}`");
        sb.AppendLine($"    {nameof(binding.StepDefinitionType)}:`{binding.StepDefinitionType}`");
        sb.AppendLine($"    {nameof(binding.SpecifiedExpression)}:`{binding.SpecifiedExpression}`");
        sb.AppendLine($"    {nameof(binding.Regex)}:`{binding.Regex}`");
        sb.AppendLine($"    {nameof(binding.Scope)}:`{binding.Scope}`");
        sb.AppendLine($"    {nameof(binding.Implementation)}:`{binding.Implementation}`");
        sb.AppendLine($"    {nameof(binding.Expression)}:`{binding.Expression}`");
        return sb.ToString();
    }
}
