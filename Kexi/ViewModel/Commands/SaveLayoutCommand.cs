﻿using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Kexi.Interfaces;
using Kexi.ViewModel.Dock;
using Kexi.ViewModel.Popup;
using Xceed.Wpf.AvalonDock.Layout;

namespace Kexi.ViewModel.Commands
{
    [Export]
    [Export(typeof(IKexiCommand))]
    public class SaveLayoutCommand : IKexiCommand
    {
        [ImportingConstructor]
        public SaveLayoutCommand(Workspace workspace, SaveKeybindingsCommand saveKeybindings, TextInputPopupViewmodel textInputPopup)
        {
            _workspace       = workspace;
            _saveKeybindings = saveKeybindings;
            _textInputPopup  = textInputPopup;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _workspace.PopupViewModel = _textInputPopup;
            _textInputPopup.Open("Layout File Name", NameSelected);
        }

        public event EventHandler                CanExecuteChanged;
        private readonly SaveKeybindingsCommand  _saveKeybindings;
        private readonly TextInputPopupViewmodel _textInputPopup;
        private readonly Workspace               _workspace;

        private void NameSelected(string name)
        {
            var favoriteLocation = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
            var targetName       = Path.Combine(favoriteLocation, $"{name}.ktc");
            var documents        = _workspace.Manager.Layout.Descendents().OfType<LayoutDocument>();
            foreach (var d in documents)
            {
                if (d.Content is DocumentViewModel view)
                {
                    d.ContentId = view.Content.Path;
                }
            }

            _workspace.DockingMananger.SerializeLayout(targetName);
            _saveKeybindings.Execute(null);
        }
    }
}