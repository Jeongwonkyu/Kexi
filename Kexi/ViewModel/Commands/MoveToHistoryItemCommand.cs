﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using Kexi.Common;
using Kexi.Interfaces;
using Kexi.ViewModel.Lister;

namespace Kexi.ViewModel.Commands
{
    [Export]
    [Export(typeof(IKexiCommand))]
    public class MoveToHistoryItemCommand : IKexiCommand
    {
        [ImportingConstructor]
        public MoveToHistoryItemCommand(Workspace workspace)
        {
            _workspace = workspace;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var item = parameter as HistoryItem;
            _currentHistoryItem              =  item;
            _workspace.ActiveLister.GotItems += HistoryFocus;
            _workspace.ActiveLister.Path     =  item.FullPath;
            //TODO: Refactor FileLister
            if (!(_workspace.ActiveLister is FileLister))
                _workspace.ActiveLister.Refresh();
        }

        public event EventHandler  CanExecuteChanged;
        private readonly Workspace _workspace;

        private HistoryItem _currentHistoryItem;

        private void HistoryFocus(object sender, EventArgs ea)
        {
            _workspace.ActiveLister.GotItems -= HistoryFocus;
            _workspace.ActiveLister.Filter   =  _currentHistoryItem.Filter;
            _workspace.ActiveLister.GroupBy  =  _currentHistoryItem.GroupBy;

            var selected = _workspace.ActiveLister.ItemsView.SourceCollection.Cast<IItem>().FirstOrDefault(f => f.Path == _currentHistoryItem.SelectedPath);
            _workspace.ActiveLister.View.FocusItem(selected);
        }
    }
}