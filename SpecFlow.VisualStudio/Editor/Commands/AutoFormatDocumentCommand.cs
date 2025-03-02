﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using SpecFlow.VisualStudio.Editor.Commands.Infrastructure;
using SpecFlow.VisualStudio.Editor.Services;
using SpecFlow.VisualStudio.Editor.Services.EditorConfig;
using SpecFlow.VisualStudio.Editor.Services.Formatting;
using SpecFlow.VisualStudio.Editor.Services.Parser;
using SpecFlow.VisualStudio.Monitoring;
using SpecFlow.VisualStudio.ProjectSystem;

namespace SpecFlow.VisualStudio.Editor.Commands
{
    [Export(typeof(IDeveroomFeatureEditorCommand))]
    public class AutoFormatDocumentCommand : DeveroomEditorCommandBase, IDeveroomFeatureEditorCommand
    {
        internal static readonly DeveroomEditorCommandTargetKey FormatDocumentKey =
                    new(VSConstants.VSStd2K, VSConstants.VSStd2KCmdID.FORMATDOCUMENT);
        internal static readonly DeveroomEditorCommandTargetKey FormatSelectionKey =
                    new(VSConstants.VSStd2K, VSConstants.VSStd2KCmdID.FORMATSELECTION);

        public override DeveroomEditorCommandTargetKey[] Targets => new[]
        {
            FormatDocumentKey,
            FormatSelectionKey
        };

        private readonly GherkinDocumentFormatter _gherkinDocumentFormatter;
        private readonly EditorConfigOptionsProvider _editorConfigOptionsProvider;

        [ImportingConstructor]
        public AutoFormatDocumentCommand(IIdeScope ideScope, IBufferTagAggregatorFactoryService aggregatorFactory, IMonitoringService monitoringService, GherkinDocumentFormatter gherkinDocumentFormatter, EditorConfigOptionsProvider editorConfigOptionsProvider = null) : base(ideScope, aggregatorFactory, monitoringService)
        {
            _gherkinDocumentFormatter = gherkinDocumentFormatter;
            _editorConfigOptionsProvider = editorConfigOptionsProvider;
        }

        public override bool PreExec(IWpfTextView textView, DeveroomEditorCommandTargetKey commandKey, IntPtr inArgs = default(IntPtr))
        {
            var documentTag = GetDeveroomTagForCaret(textView, DeveroomTagTypes.Document);
            if (!(documentTag?.Data is DeveroomGherkinDocument gherkinDocument))
                return false;

            var isSelectionFormatting = commandKey.Equals(FormatSelectionKey);
            MonitoringService.MonitorCommandAutoFormatDocument(isSelectionFormatting);

            var textSnapshot = textView.TextSnapshot;
            var caretLineNumber = textView.Caret.Position.BufferPosition.GetContainingLine().LineNumber;
            
            var startLine = 0;
            var endLine = textSnapshot.LineCount - 1;
            
            if (isSelectionFormatting)
            {
                var selectionSpan = GetSelectionSpan(textView);
                startLine = selectionSpan.Start.GetContainingLine().LineNumber;
                endLine = selectionSpan.End.GetContainingLine().LineNumber;
            }

            var formatSettings = GherkinFormatSettings.Load(_editorConfigOptionsProvider, textView, GetConfiguration(textView));

            var lines = new DocumentLinesEditBuffer(textSnapshot, startLine, endLine);
            if (lines.IsEmpty)
                return false;

            _gherkinDocumentFormatter.FormatGherkinDocument(gherkinDocument, lines, formatSettings);
            var changeSpan = lines.GetSnapshotSpan();
            var newLine = GetNewLine(textView);
            var replacementText = lines.GetModifiedText(newLine);

            if (changeSpan.GetText().Equals(replacementText)) // no change
                return false;

            using (IdeScope.CreateUndoContext("Auto format document"))
            using (var textEdit = textSnapshot.TextBuffer.CreateEdit())
            {
                textEdit.Replace(changeSpan, replacementText);
                textEdit.Apply();
            }

            textView.Caret.MoveTo(textView.TextSnapshot.GetLineFromLineNumber(caretLineNumber).End);

            return true;
        }

    }
}