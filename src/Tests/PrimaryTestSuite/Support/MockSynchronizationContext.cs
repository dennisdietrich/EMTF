/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace PrimaryTestSuite.Support
{
    public class MockSynchronizationContext : SynchronizationContext
    {
        private Collection<MessageData> _messages = new Collection<MessageData>();

        public Collection<MessageData> Messages
        {
            get
            {
                return _messages;
            }
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _messages.Add(new MessageData { DispatchMode = DispatchMode.Post, Callback = d, State = state });
            d(state);
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            _messages.Add(new MessageData { DispatchMode = DispatchMode.Send, Callback = d, State = state });
            d(state);
        }
    }
}