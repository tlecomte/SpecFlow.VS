﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using SpecFlow.VisualStudio.UI.ViewModels.WizardDialogs;

namespace SpecFlow.VisualStudio.UI.ViewModels
{
    public class WelcomeDialogViewModel : WizardViewModel
    {
        public const string WELCOME_TEXT = @"
# Welcome to SpecFlow

SpecFlow for Visual Studio includes a number of features that make it easier to edit Gherkin files and navigate to and from bindings in Visual Studio. You can also generate skeleton code including step definition methods from feature files and execute tests from Visual Studio’s Test Explorer. For more information please check [our website](https://docs.specflow.org/).

Here are the most important features supported currently:

* **Support for all SpecFlow versions and all project types**, including SpecFlow v3 on .NET Core and SDK-style projects
* **Gherkin syntax coloring and keyword completion** with new colors, context-sensitive keyword completion and highlighting syntax errors
* **Step definition matching** with *Navigate to step definition*, *Find step definition usages* and *Define Step wizard* (you need to build the project)
* **Hassle-free 'Add new feature file'** that skips design-time code generation setting when unnecessary and works with .NET Core projects as well
";

        public const string HOWTOUSE_TEXT = @"
# How to Use the new Visual Studio Extension

Using the extension is easy. Just open your feature files and start editing. There are a few things 
to note especially if you used the **SpecFlow for Visual Studio 2017 or 2019** extension before.

* **Build your project** to get up-to-date step definition matching, *Define Step wizard* and navigation.
* **Use 'Define Steps...'** from the context menu of the feature file to get started with automating undefined 
steps (colored as purple). You can use the *Ctrl+B,D* keyboard shortcut as well.
";

        public const string TROUBLESHOOTING_TEXT = @"
# Troubleshooting Tips

SpecFlow for Visual Studio 2022 is still new, so there might be some glitches, but **we are eager to hear about your feedback**.

* If you are in trouble, you should first check the **SpecFlow pane of the Output Window**. 
You can open it by choosing *View / Output* from the Visual Studio menu and switch 
the *Show output* from dropdown to *SpecFlow*.

* You can find even more trace information in the **log file** in the [%LOCALAPPDATA%\SpecFlow](file://%LOCALAPPDATA%\SpecFlow) folder.

* Please **submit your suggestions and issues** in our [issue tracker on GitHub](https://github.com/SpecFlowOSS/SpecFlow.VS/issues) or request a new feature [in our Community Forum](https://support.specflow.org/hc/en-us/community/topics/360000519178-Feature-Requests).
";

        public WelcomeDialogViewModel() : base("Close", "Welcome to SpecFlow",
            new MarkDownWizardPageViewModel("Welcome") { Text = WELCOME_TEXT },
            new MarkDownWizardPageViewModel("How to use") { Text = HOWTOUSE_TEXT },
            new MarkDownWizardPageViewModel("Troubleshooting") { Text = TROUBLESHOOTING_TEXT })
        {
        }

#if DEBUG
        public static WelcomeDialogViewModel DesignData = new WelcomeDialogViewModel();
#endif
    }
}
