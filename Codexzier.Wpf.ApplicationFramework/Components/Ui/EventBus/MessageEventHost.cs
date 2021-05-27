﻿using System;
using System.Windows;

namespace Codexzier.Wpf.ApplicationFramework.Components.Ui.EventBus
{
    public class MessageEventHost<TView, TMessage> : IMessageEventHost
        where TMessage : IMessageContainer
        where TView : DependencyObject
    {
        public Type ViewType => typeof(TView);
        public Type MessageType => typeof(TMessage);

        public void Send(IMessageContainer message) => this.SendEvent?.Invoke(message);

        public void Subscribe(Action<IMessageContainer> receiverMethod)
        {
            this.SendEvent += (message) => { receiverMethod.Invoke(message); return true; };
        }


        // OK, dass muss ich wohl ändern. Das macht so keinen Sinn
        public void Remove() => this.SendEvent -= (message) => { return true; };

        // OK, dass muss ich wohl ändern. Das macht so keinen Sinn
        private bool MessageEventHost_SendEvent(IMessageContainer message) => throw new NotImplementedException();

        // OK, dass muss ich wohl ändern. Das macht so keinen Sinn
        public void RemoveEventMethod() => this.SendEvent -= (message) => { return true; };

        public delegate bool SendEventHandler(IMessageContainer message);
        public event SendEventHandler SendEvent;
    }
}
