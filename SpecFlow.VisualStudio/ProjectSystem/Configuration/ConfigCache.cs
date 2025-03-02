﻿using SpecFlow.VisualStudio.Configuration;

namespace SpecFlow.VisualStudio.ProjectSystem.Configuration
{
    internal class ConfigCache
    {
        public DeveroomConfiguration Configuration { get;  }
        public ConfigSource[] ConfigSources { get; }

        public ConfigCache(DeveroomConfiguration configuration, ConfigSource[] configSources)
        {
            Configuration = configuration;
            ConfigSources = configSources;
        }
    }
}