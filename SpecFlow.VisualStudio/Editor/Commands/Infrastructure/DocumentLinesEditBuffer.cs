﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace SpecFlow.VisualStudio.Editor.Commands.Infrastructure
{
    public class DocumentLinesEditBuffer
    {
        private readonly ITextSnapshot _textSnapshot;
        private readonly int _startLine;
        private readonly int _endLine;
        private readonly string[] _lines;

        public bool IsEmpty => _lines.Length == 0;

        public DocumentLinesEditBuffer(ITextSnapshot textSnapshot, int startLine, int endLine)
            : this(textSnapshot, startLine, endLine, DeveroomEditorCommandBase.GetSpanFullLines(textSnapshot, startLine, endLine)
                .Select(l => l.GetText()).ToArray())
        {
        }

        internal DocumentLinesEditBuffer(ITextSnapshot textSnapshot, int startLine, int endLine, string[] lines)
        {
            _textSnapshot = textSnapshot;
            _startLine = startLine;
            _endLine = endLine;
            _lines = lines;
        }

        private string GetLine(int zeroBasedLineNumber)
        {
            if (zeroBasedLineNumber < _startLine || zeroBasedLineNumber > _endLine)
                return string.Empty;
            return _lines[zeroBasedLineNumber - _startLine];
        }

        public string GetLineOneBased(int oneBasedLineNumber) => GetLine(oneBasedLineNumber - 1);

        private void SetLine(int zeroBasedLineNumber, string line)
        {
            if (zeroBasedLineNumber < _startLine || zeroBasedLineNumber > _endLine)
                return;
            _lines[zeroBasedLineNumber - _startLine] = line;
        }

        public void SetLineOneBased(int oneBasedLineNumber, string line) => SetLine(oneBasedLineNumber - 1, line);

        public string GetModifiedText(string newLine)
        {
            return string.Join(newLine, _lines);
        }

        public SnapshotSpan GetSnapshotSpan()
        {
            return new SnapshotSpan(
                _textSnapshot.GetLineFromLineNumber(_startLine).Start,
                _textSnapshot.GetLineFromLineNumber(_endLine).End);
        }
    }
}
