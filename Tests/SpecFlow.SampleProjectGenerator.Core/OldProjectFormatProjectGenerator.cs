﻿using System;
using System.IO;

namespace SpecFlow.SampleProjectGenerator
{
    public class OldProjectFormatProjectGenerator : ProjectGenerator
    {
        public override string GetOutputAssemblyPath(string config = "Debug")
            => Path.Combine("bin", config, AssemblyFileName);

        public OldProjectFormatProjectGenerator(GeneratorOptions options, Action<string> consoleWriteLine) : base(options, consoleWriteLine)
        {
        }

        protected override ProjectChanger CreateProjectChanger(string projectFilePath)
        {
            return new OldProjectFormatProjectChanger(projectFilePath);
        }

        protected override string GetTemplatesFolder()
        {
            return @"Templates\CS-OLD";
        }

        protected override string GetPackagesFolder()
        {
            return Path.Combine(_options.TargetFolder, "packages");
        }

        protected override int ExecBuild()
        {
            return Exec(_options.TargetFolder, ToolLocator.GetToolPath(ExternalTools.MsBuild, _consoleWriteLine));
        }
    }
}
