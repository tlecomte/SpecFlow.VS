﻿namespace SpecFlow.VisualStudio.SpecFlowConnector.Discovery.V31;

public class SpecFlowV31Discoverer : SpecFlowV3BaseDiscoverer
{
    public SpecFlowV31Discoverer(AssemblyLoadContext loadContext) : base(loadContext)
    {
    }

    protected override DefaultDependencyProvider CreateDefaultDependencyProvider()
    {
        return new SpecFlowV31DependencyProvider(_loadContext);
    }

    private class SpecFlowV31DependencyProvider : NoInvokeDependencyProvider
    {
        private readonly AssemblyLoadContext _loadContext;

        public SpecFlowV31DependencyProvider(AssemblyLoadContext loadContext)
        {
            _loadContext = loadContext;
        }

        public override void RegisterGlobalContainerDefaults(ObjectContainer globalContainer)
        {
            base.RegisterGlobalContainerDefaults(globalContainer);
            globalContainer.RegisterInstanceAs(_loadContext);

            var pluginLoaderType = new DynamicRuntimePluginLoaderFactory().Create();
            globalContainer.ReflectionRegisterTypeAs(pluginLoaderType, typeof(IRuntimePluginLoader));
        }
    }
}
