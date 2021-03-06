﻿using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using Kexi.Interfaces;

namespace Kexi.ViewModel.Commands
{
    [Export]
    [Export(typeof(IKexiCommand))]
    public class MoveCursorLeftCommand : IKexiCommand
    {
        [ImportingConstructor]
        public MoveCursorLeftCommand()
        {
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var uie = Keyboard.FocusedElement as UIElement;
            uie?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
        }

        public event EventHandler  CanExecuteChanged;
    }
}