﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace SpecFlow.VisualStudio.Editor.Completions.Infrastructure
{
    public abstract class DeveroomCompletionSourceBase : ICompletionSource
    {
        protected readonly ITextBuffer _buffer;
        private readonly string _name;

        protected DeveroomCompletionSourceBase(string name, ITextBuffer buffer)
        {
            _name = name;
            _buffer = buffer;
        }

        protected abstract KeyValuePair<SnapshotSpan, List<Completion>> CollectCompletions(SnapshotPoint triggerPoint);

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var snapshotTriggerPoint = session.GetTriggerPoint(_buffer.CurrentSnapshot);
            if (snapshotTriggerPoint == null)
                return;

            var completionResult = CollectCompletions(snapshotTriggerPoint.Value);
            if (completionResult.Value.Count == 0)
                return;

            var applicableTo = GetApplicableTo(completionResult);
            if (applicableTo == null)
                return;

            completionSets.Add(new WordContainsFilteredCompletionSet(
                _name,
                _name,
                applicableTo,
                completionResult.Value,
                null));
        }

        private ITrackingSpan GetApplicableTo(KeyValuePair<SnapshotSpan, List<Completion>> completionResult)
        {
            //TODO: double check if this logic is useful, but if this is enabled, it is impossible to change an existing entry using completion
            //var applicableToText = completionResult.Key.GetText();
            //// if the full insertion text has been typed in already, we skip
            //if (applicableToText.Length > 0 && completionResult.Value.Any(c => applicableToText.StartsWith(c.InsertionText)))
            //    return null;

            return _buffer.CurrentSnapshot.CreateTrackingSpan(completionResult.Key, SpanTrackingMode.EdgeInclusive);
        }

        public void Dispose()
        {
            //nop
        }
    }
}
