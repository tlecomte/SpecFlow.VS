﻿using System;
using System.Diagnostics;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SpecFlow.VisualStudio.ProjectSystem
{
    public class VsDeveroomOutputPaneServices : VsServiceBase, IDeveroomOutputPaneServices
    {
        private static Guid _outputPaneGuid = Guid.NewGuid();
        private const string PaneName = "SpecFlow";

        private readonly Lazy<IVsOutputWindowPane> _outputWindowPane;

        public VsDeveroomOutputPaneServices(IVsIdeScope vsIdeScope) : base(vsIdeScope)
        {
            _outputWindowPane = new Lazy<IVsOutputWindowPane>(CreateOutputWindowPane);
        }

        private IVsOutputWindowPane CreateOutputWindowPane()
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var vsOutputWindow = _vsIdeScope.ServiceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                if (vsOutputWindow == null)
                    return null;
                if (ErrorHandler.Failed(vsOutputWindow.CreatePane(ref _outputPaneGuid, PaneName, 1, 1)))
                    return null;
                if (ErrorHandler.Failed(vsOutputWindow.GetPane(ref _outputPaneGuid, out var pane)))
                    return null;
                return pane;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, "Create SpecFlow Pane Error");
                return null;
            }
        }

        public void WriteLine(string text)
        {
            if (!text.EndsWith(Environment.NewLine))
                text += Environment.NewLine;

            SafeWrite(text);
        }

        public void SendWriteLine(string text)
        {
            RunOnUiThread(() => WriteLine(text));
        }

        public void Activate()
        {
            RunOnUiThread(ActivateInternal);
        }

        private void ActivateInternal()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var pane = _outputWindowPane.Value;
            if (pane != null)
            {
                try
                {
                    pane.Activate();
                    ((DTE2) _vsIdeScope.Dte).ToolWindows.OutputWindow.Parent.Activate();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        private void SafeWrite(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var pane = _outputWindowPane.Value;
            if (pane != null)
            {
                try
                {
                    pane.OutputStringThreadSafe(text);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
    }
}
