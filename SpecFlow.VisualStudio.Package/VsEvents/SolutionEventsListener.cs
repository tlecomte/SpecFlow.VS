﻿using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;

namespace SpecFlow.VisualStudio.VsEvents
{
    [Export]
    [Export(typeof(IVsSolutionEventListener))]
    public sealed class SolutionEventsListener : IVsSolutionEvents, IVsSolutionEvents4, IVsSolutionLoadEvents, IVsSolutionEventListener, IDisposable//, IVsSolutionEvents7
    {
        private readonly IVsSolution _vsSolution;
        private uint _pdwCookie;

        [ImportingConstructor]
        public SolutionEventsListener([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            _vsSolution = serviceProvider.GetService<IVsSolution>(typeof(SVsSolution));
            AdviseSolutionEvents();
        }

        private void OnSolutionOpened()
        {
            if (Opened == null)
                return;
            string solutionFile = _vsSolution.GetSolutionFile();
            if (string.IsNullOrEmpty(solutionFile))
                return;
            Opened(this, new HostOpenedEventArgs(solutionFile));
        }

        private void OnSolutionClosing()
        {
            Closing?.Invoke(this, EventArgs.Empty);
        }

        private void OnSolutionClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        private void OnSolutionLoadComplete()
        {
            TriggerLoadedEvent();
        }

        private void TriggerLoadedEvent()
        {
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
        {
            this.OnSolutionClosed();
            return 0;
        }

        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            AfterLoadProject?.Invoke(this, new LoadProjectEventArgs(pRealHierarchy, pStubHierarchy));
            return 0;
        }

        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            AfterOpenProject?.Invoke(this, new OpenProjectEventArgs(pHierarchy, Convert.ToBoolean(fAdded)));
            return 0;
        }

        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            OnSolutionOpened();
            return 0;
        }

        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            BeforeCloseProject?.Invoke(this, new CloseProjectEventArgs(pHierarchy, Convert.ToBoolean(fRemoved)));
            return 0;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            OnSolutionClosing();
            return 0;
        }

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return 0;
        }

        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return 0;
        }

        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return 0;
        }

        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return 0;
        }

        public event EventHandler<HostOpenedEventArgs> Opened;

        public event EventHandler Closing;

        public event EventHandler Closed;

        public event EventHandler Loaded;

        public event EventHandler<OpenProjectEventArgs> AfterOpenProject;

        public event EventHandler<LoadProjectEventArgs> AfterLoadProject;

        public event EventHandler<CloseProjectEventArgs> BeforeCloseProject;

        public event EventHandler<HierarchyEventArgs> ProjectRenamed;

        public int OnAfterBackgroundSolutionLoadComplete()
        {
            OnSolutionLoadComplete();
            return 0;
        }

        public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return 0;
        }

        public int OnBeforeBackgroundSolutionLoadBegins()
        {
            return 0;
        }

        public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return 0;
        }

        public int OnBeforeOpenSolution(string pszSolutionFilename)
        {
            return 0;
        }

        public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            pfShouldDelayLoadToNextIdle = false;
            return 0;
        }

        private void AdviseSolutionEvents()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                SolutionEventsListener solutionEventsListener = this;
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                ErrorHandler.ThrowOnFailure(solutionEventsListener._vsSolution.AdviseSolutionEvents(solutionEventsListener, out solutionEventsListener._pdwCookie));
            });
        }

        private void UnadviseSolutionEvents()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (_pdwCookie == 0U || _vsSolution == null)
                    return;
                _vsSolution.UnadviseSolutionEvents(_pdwCookie);
                _pdwCookie = 0U;
            });
        }

        public int OnAfterRenameProject(IVsHierarchy hierarchy)
        {
            ProjectRenamed?.Invoke(this, new HierarchyEventArgs(hierarchy));
            return 0;
        }

        public void OnAfterOpenFolder(string folderPath)
        {
            OnSolutionOpened();
            OnSolutionLoadComplete();
        }

        public void OnBeforeCloseFolder(string folderPath)
        {
            OnSolutionClosing();
        }

        public void OnQueryCloseFolder(string folderPath, ref int pfCancel)
        {
        }

        public int OnAfterAsynchOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return 0;
        }

        public int OnAfterChangeProjectParent(IVsHierarchy pHierarchy)
        {
            return 0;
        }

        public int OnQueryChangeProjectParent(IVsHierarchy pHierarchy, IVsHierarchy pNewParentHier, ref int pfCancel)
        {
            return 0;
        }

        public void OnAfterCloseFolder(string folderPath)
        {
            this.OnSolutionClosed();
        }

        public void OnAfterLoadAllDeferredProjects()
        {
        }

        public void Dispose()
        {
            UnadviseSolutionEvents();
        }
    }
}
