﻿using Codexzier.Wpf.ApplicationFramework.Components.Ui.EventBus;
using Codexzier.Wpf.ApplicationFramework.Views.Base;
using WpfAppTemplateForNuget.Views.DialogContent;

namespace WpfAppTemplateForNuget.Views.Dialog
{
    internal class DoCloseDialogView : BaseCommand
    {
        public override void Execute(object parameter)
        {
            EventBusManager.CloseView<DialogContentView>(2);
            EventBusManager.CloseView<DialogView>(10);
        }
    }
}