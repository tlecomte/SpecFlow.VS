﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using SpecFlow.VisualStudio.ProjectSystem;

namespace SpecFlow.VisualStudio.Editor.Commands
{
    internal abstract class RenameStepAction : IRenameStepAction
    {
        public abstract Task PerformRenameStep(RenameStepCommandContext ctx);

        protected static Task EditTextBuffer<T>(
            ITextBuffer textBuffer,
            IIdeScope ideScope,
            IEnumerable<T> expressionsToReplace,
            Func<T, Span> calculateReplaceSpan,
            Func<T, string> calculateReplacementText)
        {
            return ideScope.RunOnUiThread(() =>
            {
                using var textEdit = textBuffer.CreateEdit();

                foreach (var token in expressionsToReplace)
                {
                    var replaceSpan = calculateReplaceSpan(token);
                    textEdit.Replace(replaceSpan, calculateReplacementText(token));
                }

                textEdit.Apply();
            });
        }
    }
}
