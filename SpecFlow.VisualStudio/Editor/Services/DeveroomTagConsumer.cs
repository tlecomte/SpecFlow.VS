﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace SpecFlow.VisualStudio.Editor.Services
{
    public abstract class DeveroomTagConsumer : IDisposable
    {
        protected readonly ITextBuffer Buffer;
        protected readonly ITagAggregator<DeveroomTag> DeveroomTagAggregator;

        protected DeveroomTagConsumer(ITextBuffer buffer, ITagAggregator<DeveroomTag> deveroomTagAggregator)
        {
            this.Buffer = buffer;
            this.DeveroomTagAggregator = deveroomTagAggregator;

            this.DeveroomTagAggregator.BatchedTagsChanged += DeveroomTagAggregatorOnBatchedTagsChanged;
        }

        private void DeveroomTagAggregatorOnBatchedTagsChanged(object sender, BatchedTagsChangedEventArgs batchedTagsChangedEventArgs)
        {
            var snapshot = Buffer.CurrentSnapshot;
            var spans = batchedTagsChangedEventArgs.Spans.SelectMany(mappingSpan => mappingSpan.GetSpans(snapshot), (mappingSpan, mappedTagSpan) => mappedTagSpan);

            var start = int.MaxValue;
            var end = 0;
            foreach (var sourceSpan in spans)
            {
                start = Math.Min(start, sourceSpan.Start.Position);
                end = Math.Max(end, sourceSpan.End.Position);
            }

            if (start == int.MaxValue || end <= start)
                return;

            var span = new SnapshotSpan(snapshot, start, end - start);
            RaiseChanged(span);
        }

        protected abstract void RaiseChanged(SnapshotSpan span);

        public void Dispose()
        {
            this.DeveroomTagAggregator.BatchedTagsChanged -= DeveroomTagAggregatorOnBatchedTagsChanged;
        }

        protected IEnumerable<KeyValuePair<SnapshotSpan, DeveroomTag>> GetDeveroomTags(SnapshotSpan span, Predicate<DeveroomTag> filter = null)
        {
            return GetDeveroomTags(new NormalizedSnapshotSpanCollection(span), filter);
        }

        protected IEnumerable<KeyValuePair<SnapshotSpan, DeveroomTag>> GetDeveroomTags(NormalizedSnapshotSpanCollection spans, Predicate<DeveroomTag> filter = null)
        {
            var snapshot = spans[0].Snapshot;
            var gherkinMappingTagSpans = DeveroomTagAggregator.GetTags(spans);
            if (filter != null)
                gherkinMappingTagSpans = gherkinMappingTagSpans.Where(t => filter(t.Tag));

            return gherkinMappingTagSpans.SelectMany(
                mappingTagSpan => mappingTagSpan.Span.GetSpans(snapshot),
                (mappingTagSpan, mappedTagSpan) => new KeyValuePair<SnapshotSpan, DeveroomTag>(mappedTagSpan, mappingTagSpan.Tag));
        }
    }
}
