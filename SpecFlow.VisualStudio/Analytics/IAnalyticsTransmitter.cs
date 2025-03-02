﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using SpecFlow.VisualStudio.Common;

namespace SpecFlow.VisualStudio.Analytics
{
    public interface IAnalyticsTransmitter
    {
        void TransmitEvent(IAnalyticsEvent runtimeEvent);
        void TransmitExceptionEvent(Exception exception, Dictionary<string, object> additionalProps = null, bool? isFatal = null, bool anonymize = true);
    }

    [Export(typeof(IAnalyticsTransmitter))]
    public class AnalyticsTransmitter : IAnalyticsTransmitter
    {
        private readonly IAnalyticsTransmitterSink _analyticsTransmitterSink;
        private readonly IEnableAnalyticsChecker _enableAnalyticsChecker;

        [ImportingConstructor]
        public AnalyticsTransmitter(IAnalyticsTransmitterSink analyticsTransmitterSink, IEnableAnalyticsChecker enableAnalyticsChecker)
        {
            _analyticsTransmitterSink = analyticsTransmitterSink;
            _enableAnalyticsChecker = enableAnalyticsChecker;
        }

        public void TransmitEvent(IAnalyticsEvent analyticsEvent)
        {
            try
            {
                if (!_enableAnalyticsChecker.IsEnabled())
                {
                    return;
                }
                
                _analyticsTransmitterSink.TransmitEvent(analyticsEvent);
            }
            catch (Exception ex)
            {
                TransmitExceptionEvent(ex, anonymize: false);
            }
        }

        public void TransmitExceptionEvent(Exception exception, Dictionary<string, object> additionalProps = null, bool? isFatal = null, bool anonymize = true)
        {
            var isNormalError = IsNormalError(exception);
            TransmitException(exception, isFatal ?? !isNormalError, additionalProps, anonymize);
        }
        
        private void TransmitException(Exception exception, bool isFatal, Dictionary<string, object> additionalProps = null, bool anonymize = true)
        {
            try
            {
                additionalProps ??= new Dictionary<string, object>();
                
                additionalProps.Add("IsFatal", isFatal.ToString());

                _analyticsTransmitterSink.TransmitException(exception, additionalProps);
            }
            catch (Exception ex)
            {
                // catch all exceptions since we do not want to break the whole extension simply because data transmission failed
                Debug.WriteLine(ex, "Error during transmitting analytics event.");
            }
        }

        private static bool IsNormalError(Exception exception)
        {
            if (exception is AggregateException aggregateException)
                return aggregateException.InnerExceptions.All(IsNormalError);
            return
                exception is DeveroomConfigurationException ||
                exception is TimeoutException ||
                exception is TaskCanceledException ||
                exception is OperationCanceledException ||
                exception is HttpRequestException;
        }
    }
}
