﻿using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace SpecFlow.VisualStudio.Editor.Completions.Infrastructure
{
    public class ExtendableCompletionSet : CompletionSet
    {
        public ExtendableCompletionSet(string moniker, string displayName, ITrackingSpan applicableTo, IEnumerable<Completion> completions, IEnumerable<Completion> completionBuilders) : base(moniker, displayName, applicableTo, completions, completionBuilders)
        {
        }

        private string _filterBufferText;
        private int _filterBufferTextVersionNumber = -1;
        private CompletionMatchType _filterMatchType;
        private bool _filterCaseSensitive;

        private FilteredObservableCollection<Completion> FilteredCompletions =>
            (FilteredObservableCollection<Completion>) Completions;

        private FilteredObservableCollection<Completion> FilteredCompletionBuilders =>
            (FilteredObservableCollection<Completion>) CompletionBuilders;

        private string FilterBufferText
        {
            get
            {
                if (ApplicableTo != null)
                {
                    ITextSnapshot currentSnapshot = ApplicableTo.TextBuffer.CurrentSnapshot;
                    if (_filterBufferText == null || _filterBufferTextVersionNumber != currentSnapshot.Version.VersionNumber)
                    {
                        _filterBufferText = ApplicableTo.GetText(currentSnapshot);
                        _filterBufferTextVersionNumber = currentSnapshot.Version.VersionNumber;
                    }
                }
                return _filterBufferText;
            }
        }

        public override void Filter()
        {
            ExtendableFilter(CompletionMatchType.MatchDisplayText, false);
        }

        protected void ExtendableFilter(CompletionMatchType matchType, bool caseSensitive)
        {
            if (string.IsNullOrEmpty(FilterBufferText))
            {
                FilteredCompletions.StopFiltering();
                FilteredCompletionBuilders.StopFiltering();
            }
            else
            {
                _filterMatchType = matchType;
                _filterCaseSensitive = caseSensitive;
                FilteredCompletions.Filter(DoesCompletionMatchApplicabilityText);
                FilteredCompletionBuilders.Filter(DoesCompletionMatchApplicabilityText);
            }
        }

        private bool DoesCompletionMatchApplicabilityText(Completion completion)
        {
            string str = string.Empty;
            if (_filterMatchType == CompletionMatchType.MatchDisplayText)
                str = completion.DisplayText;
            else if (_filterMatchType == CompletionMatchType.MatchInsertionText)
                str = completion.InsertionText;
            return DoesTextMatch(str, FilterBufferText, _filterCaseSensitive);
        }

        protected virtual bool DoesTextMatch(string text, string filterText, bool caseSensitive)
        {
            return text.StartsWith(filterText, !caseSensitive, CultureInfo.CurrentCulture);
        }
    }
}