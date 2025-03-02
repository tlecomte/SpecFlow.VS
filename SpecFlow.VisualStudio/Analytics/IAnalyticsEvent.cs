﻿using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace SpecFlow.VisualStudio.Analytics
{
    public interface IAnalyticsEvent
    {
        string EventName { get; }

        Dictionary<string, object> Properties { get; }
    }

    [Export(typeof(IAnalyticsEvent))]
    public class GenericEvent : IAnalyticsEvent
    {
        public GenericEvent(string eventName, Dictionary<string, object> properties = null)
        {
            EventName = eventName;
            Properties = properties ?? new Dictionary<string, object>();
        }

        public string EventName { get; }
        public Dictionary<string, object> Properties { get; }
    }
}
