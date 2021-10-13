using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SpecFlow.VisualStudio.Editor.Commands;
using SpecFlow.VisualStudio.Editor.Services;
using SpecFlow.VisualStudio.SpecFlowConnector.Models;
using SpecFlow.VisualStudio.UI.ViewModels;
using SpecFlow.VisualStudio.VsxStubs;
using SpecFlow.VisualStudio.VsxStubs.ProjectSystem;
using Xunit;
using Xunit.Abstractions;

namespace SpecFlow.VisualStudio.Tests.Editor.Commands;

public class DefineStepsCommandTests : CommandTestBase<DefineStepsCommand>
{
    public DefineStepsCommandTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper,
        ps => new DefineStepsCommand(ps.IdeScope, null, ps.IdeScope.MonitoringService),
        "???",
        "ShowProblem: User Notification: ")
    {
    }

    private void ArrangePopup()
    {
        (ProjectScope.IdeScope.WindowManager as StubWindowManager)
            .RegisterWindowAction<CreateStepDefinitionsDialogViewModel>(model => model.Result = CreateStepDefinitionsDialogResult.Create);
    }

    [Fact]
    public void Warn_if_steps_have_been_defined_already()
    {
        var stepDefinition = ArrangeStepDefinition();
        var featureFile = ArrangeOneFeatureFile();

        var (_, command) = ArrangeSut(stepDefinition, featureFile);
        var textView = CreateTextView(featureFile);

        Invoke(command, textView);

        var stubLogger = GetStubLogger();
        WithoutWarningHeader(stubLogger.Messages.Last().Message).Should()
            .Be("All steps have been defined in this file already.");
    }

    [Fact]
    public void CreateStepDefinitionsDialog_cancelled()
    {
        var stepDefinition = ArrangeStepDefinition(@"""I choose add""");
        var featureFile = ArrangeOneFeatureFile();
        
        var (_, command) = ArrangeSut(stepDefinition, featureFile);
        var textView = CreateTextView(featureFile);

        Invoke(command, textView);
        
        ThereWereNoWarnings();
    }

    [Theory]
    [InlineData("01", @"@""I press add""")]
    public async Task Step_definition_class_saved(string _, string expression)
    {
        var featureFile = ArrangeOneFeatureFile();
        
        ArrangePopup();
        var (_, command) = ArrangeSut(TestStepDefinition.Void, featureFile);
        var textView = CreateTextView(featureFile);

        Invoke(command, textView);

        ThereWereNoWarnings();
        var createdStepDefinitionContent = ProjectScope.StubIdeScope.CurrentTextView.TextBuffer.CurrentSnapshot.GetText();
        Dump(ProjectScope.StubIdeScope.CurrentTextView, "Created stepDefinition file");
        createdStepDefinitionContent.Should().Contain(expression);
        await BindingRegistryIsModified(expression);
    }
}