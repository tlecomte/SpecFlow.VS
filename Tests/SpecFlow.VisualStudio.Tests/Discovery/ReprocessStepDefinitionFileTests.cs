using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using FluentAssertions;
using Microsoft.VisualStudio.Text;
using Newtonsoft.Json;
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
    [InlineData("MultipleStepDefinitions.cs")]
    public async Task Approval(string testName)
    {
        //arrange
        NamerFactory.AdditionalInformation = testName;
        var namer = Approvals.GetDefaultNamer();
        var stepDefinitionPath = Path.GetFullPath(Path.Combine(namer.SourcePath, namer.Name));
       
        NamerFactory.AdditionalInformation = testName;
        var content = File.ReadAllText(stepDefinitionPath);
        var stepDefinitionFile = new CSharpStepDefinitionFile(stepDefinitionPath, content);

        _projectScope.AddSpecFlowPackage();
        var discoveryService =
            MockableDiscoveryService.SetupWithInitialStepDefinitions(_projectScope, Array.Empty<StepDefinition>(), TimeSpan.Zero);
        discoveryService.WaitUntilDiscoveryPerformed();

        //act
        await discoveryService.ProcessAsync(stepDefinitionFile);

        //assert
        ProjectBindingRegistry bindingRegistry = await discoveryService.GetBindingRegistryAsync();
        var dumped = Dump(bindingRegistry);
        Approvals.Verify(dumped);
    }

    private string _indent = string.Empty;

    private void IncreaseIndent()
    {
        _indent += "  ";
    }

    private void DecreaseIndent()
    {
        _indent = _indent.Substring(0, _indent.Length - 2);
    }

    public string Dump(ProjectBindingRegistry bindingRegistry)
    {
        IncreaseIndent();
        var sb = new StringBuilder("ProjectBindingRegistry:");
        sb.AppendLine();
        if (bindingRegistry.IsFailed) sb.AppendLine("{_indent}Failed");
        int i = 0;
        foreach (ProjectStepDefinitionBinding binding in bindingRegistry.StepDefinitions)
        {
            sb.AppendLine($"{_indent}ProjectStepDefinitionBinding-{i}:");
            sb.Append(Dump(binding));
        }

        DecreaseIndent();
        return sb.ToString();
    }

    public string Dump(ProjectStepDefinitionBinding binding)
    {
        IncreaseIndent();
        var sb = new StringBuilder();
        sb.AppendLine($"{_indent}{nameof(binding.IsValid)}:`{binding.IsValid}`");
        sb.AppendLine($"{_indent}{nameof(binding.Error)}:`{binding.Error}`");
        sb.AppendLine($"{_indent}{nameof(binding.StepDefinitionType)}:`{binding.StepDefinitionType}`");
        sb.AppendLine($"{_indent}{nameof(binding.SpecifiedExpression)}:`{binding.SpecifiedExpression}`");
        sb.AppendLine($"{_indent}{nameof(binding.Regex)}:`{binding.Regex}`");
        sb.AppendLine($"{_indent}{nameof(binding.Scope)}:`{binding.Scope}`");
        sb.AppendLine($"{_indent}{nameof(binding.Implementation)}:");
        sb.Append(Dump(binding.Implementation));
        sb.AppendLine($"{_indent}{nameof(binding.Expression)}:`{binding.Expression}`");
        DecreaseIndent();
        return sb.ToString();
    }

    public string Dump(ProjectStepDefinitionImplementation implementation)
    {
        IncreaseIndent();
        var sb = new StringBuilder();
        sb.AppendLine($"{_indent}{nameof(implementation.Method)}:`{implementation.Method}`");
        sb.AppendLine($"{_indent}{nameof(implementation.ParameterTypes)}:`{implementation.ParameterTypes}`");
        IncreaseIndent();
        foreach (string parameterType in implementation.ParameterTypes)
        {
            sb.AppendLine($"{_indent}- `{parameterType}`");
        }
        DecreaseIndent();
        sb.AppendLine($"{_indent}{nameof(implementation.SourceLocation)}:");
        sb.Append(Dump(implementation.SourceLocation));
        DecreaseIndent();
        return sb.ToString();
    }

    public string Dump(SourceLocation sourceLocation)
    {
        if (sourceLocation == null) return string.Empty;
        IncreaseIndent();
        var sb = new StringBuilder();
        sb.AppendLine($"{_indent}{nameof(sourceLocation.SourceFile)}:`{sourceLocation.SourceFile}`");
        sb.AppendLine($"{_indent}{nameof(sourceLocation.SourceFileColumn)}:`{sourceLocation.SourceFileColumn}`");
        sb.AppendLine($"{_indent}{nameof(sourceLocation.SourceFileEndColumn)}:`{sourceLocation.SourceFileEndColumn}`");
        sb.AppendLine($"{_indent}{nameof(sourceLocation.SourceFileEndLine)}:`{sourceLocation.SourceFileEndLine}`");
        sb.AppendLine($"{_indent}{nameof(sourceLocation.SourceLocationSpan)}:`{sourceLocation.SourceLocationSpan}`");
        sb.AppendLine($"{_indent}{nameof(sourceLocation.SourceFileLine)}:`{sourceLocation.SourceFileLine}`");
        DecreaseIndent();
        return sb.ToString();
    }
}
