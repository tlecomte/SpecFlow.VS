﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SpecFlow.VisualStudio.Editor.Services.StepDefinitions
{
    public class AnalyzedStepDefinitionExpression
    {
        public AnalyzedStepDefinitionExpression(ImmutableArray<AnalyzedStepDefinitionExpressionPart> parts)
        {
            Parts = parts;
        }

        public ImmutableArray<AnalyzedStepDefinitionExpressionPart> Parts { get; }

        public bool ContainsOnlySimpleText => Parts.OfType<AnalyzedStepDefinitionExpressionSimpleTextPart>().Count() == Parts.Length/2 + 1;
        public IEnumerable<AnalyzedStepDefinitionExpressionParameterPart> ParameterParts => Parts.OfType<AnalyzedStepDefinitionExpressionParameterPart>();
    }
}