﻿using System;
using System.ComponentModel.Composition;
using System.Windows;
using Kexi.Common;
using Kexi.Interfaces;

namespace Kexi.ViewModel.Commands
{
    [Export]
    [Export(typeof(IKexiCommand))]
    public class WindowDockLeftCommand : IKexiCommand
    {
        [ImportingConstructor]
        public WindowDockLeftCommand(Workspace workspace)
        {
            _workspace = workspace;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var s = UIHelper.GetCurrentScreen();
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Top    = s.WorkingArea.Y;
                Application.Current.MainWindow.Height = s.WorkingArea.Height;
                Application.Current.MainWindow.Width  = s.WorkingArea.Width / 2;
                Application.Current.MainWindow.Left   = s.WorkingArea.X;
            }
        }

        public event EventHandler  CanExecuteChanged;
        private readonly Workspace _workspace;
    }
}