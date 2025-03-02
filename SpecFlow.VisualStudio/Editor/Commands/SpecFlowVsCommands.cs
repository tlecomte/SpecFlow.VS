﻿using System;

namespace SpecFlow.VisualStudio.Editor.Commands
{
    public static class SpecFlowVsCommands
    {
        public static readonly Guid DefaultCommandSet = new("7b9f385f-5db1-4fc3-9202-064b4a3fa987");

        public const int DefineStepsCommandId = 0x0100;
        public const int FindStepDefinitionUsagesCommandId = 0x0101;
        public const int RegenerateAllFeatureFileCodeBehindCommandId = 0x0102;
        public const int RenameStepCommandId = 0x0103;
    }
}