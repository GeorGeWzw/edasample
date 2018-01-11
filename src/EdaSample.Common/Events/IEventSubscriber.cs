﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EdaSample.Common.Events
{
    public interface IEventSubscriber : IDisposable
    {
        void Subscribe<TEvent, TEventHandler>()
            where TEvent : IEvent
            where TEventHandler : IEventHandler<TEvent>;
    }
}
