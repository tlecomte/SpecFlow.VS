﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpecFlow.VisualStudio.ProjectSystem.Actions
{
    public static class IdeActionsExtensions
    {
        public static void ShowSyncContextMenu(this IIdeActions ideActions, string header, params ContextMenuItem[] contextMenuItems)
        {
            ideActions.ShowContextMenu(header, false, contextMenuItems);
        }

        public static IAsyncContextMenu ShowAsyncContextMenu(this IIdeActions ideActions, string header, params ContextMenuItem[] contextMenuItems)
        {
            return ideActions.ShowContextMenu(header, true, contextMenuItems);
        }

        public static MessageBoxResult ShowSyncQuestion(this IIdeActions ideActions, string title, string description, bool includeCancel = false, MessageBoxResult defaultButton = MessageBoxResult.Yes)
        {
            MessageBoxResult result = MessageBoxResult.Cancel;
            ideActions.ShowQuestion(
                new QuestionDescription(title, description, _ => result = MessageBoxResult.Yes, _ => result = MessageBoxResult.No, includeCancel, defaultButton == MessageBoxResult.No));
            return result;
        }
    }
}
