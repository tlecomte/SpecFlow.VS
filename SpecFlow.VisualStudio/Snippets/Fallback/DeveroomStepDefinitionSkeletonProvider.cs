﻿using System;
using System.Globalization;
using System.Linq;
using SpecFlow.VisualStudio.Discovery;

namespace SpecFlow.VisualStudio.Snippets.Fallback
{
    public abstract class DeveroomStepDefinitionSkeletonProvider
    {
        public string GetStepDefinitionSkeletonSnippet(UndefinedStepDescriptor undefinedStep,
            string indent, string newLine, string bindingCultureName)
        {
            var bindingCulture = CultureInfo.GetCultureInfo(bindingCultureName);

            var analyzedStepText = Analyze(undefinedStep, bindingCulture);

            var regex = GetExpression(analyzedStepText);
            var methodName = GetMethodName(undefinedStep, analyzedStepText);
            var parameters = string.Join(", ", analyzedStepText.Parameters.Select(ToDeclaration));
            var stringPrefix = UseVerbatimStringForExpression ? "@" : "";

            var method = $"[{undefinedStep.ScenarioBlock}({stringPrefix}\"{regex}\")]" + newLine +
                         $"public void {methodName}({parameters})" + newLine +
                         $"{{" + newLine +
                         $"{indent}throw new PendingStepException();" + newLine +
                         $"}}" + newLine;

            return method;
        }

        protected virtual string GetMethodName(UndefinedStepDescriptor stepInstance, AnalyzedStepText analyzedStepText)
        {
            var keyword = stepInstance.ScenarioBlock.ToString(); //TODO: get lang specific keyword
            return keyword.ToIdentifier() + String.Concat(analyzedStepText.TextParts.ToArray()).ToIdentifier();
        }

        private string ToDeclaration(AnalyzedStepParameter parameter)
        {
            return String.Format("{1} {0}", Keywords.EscapeCSharpKeyword(parameter.Name), GetCSharpTypeName(parameter.Type));
        }

        private string GetCSharpTypeName(string type)
        {
            switch (type)
            {
                case "String":
                    return "string";
                case "Int32":
                    return "int";
                default:
                    return type;
            }
        }


        protected virtual AnalyzedStepText Analyze(UndefinedStepDescriptor stepInstance, CultureInfo bindingCulture)
        {
            var stepTextAnalyzer = CreateStepTextAnalyzer();
            var result = stepTextAnalyzer.Analyze(stepInstance.StepText, bindingCulture);
            if (stepInstance.HasDocString)
                result.Parameters.Add(new AnalyzedStepParameter("String", "multilineText"));
            if (stepInstance.HasDataTable)
                result.Parameters.Add(new AnalyzedStepParameter("Table", "table"));
            return result;
        }

        protected abstract IStepTextAnalyzer CreateStepTextAnalyzer();
        protected abstract bool UseVerbatimStringForExpression { get; }
        protected abstract string GetExpression(AnalyzedStepText stepText);
    }
}
