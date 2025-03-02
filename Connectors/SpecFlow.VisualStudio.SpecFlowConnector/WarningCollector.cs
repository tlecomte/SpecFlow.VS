﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpecFlow.VisualStudio.SpecFlowConnector
{
    public class WarningCollector
    {
        private readonly List<string> _warnings = new List<string>();

        public string[] Warnings => _warnings.Any() ? _warnings.Distinct().ToArray() : null;

        public void AddWarning(string warning, Exception exception = null)
        {
            Debug.WriteLine(exception, warning);
            if (exception != null)
                warning = $"{warning}: {exception}";
            _warnings.Add(warning);
        }
    }
}
