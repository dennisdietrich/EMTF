/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;
using System.Threading;

namespace PrimaryTestSuite.Support
{
    public class MessageData
    {
        public DispatchMode DispatchMode
        {
            get;
            set;
        }

        public SendOrPostCallback Callback
        {
            get;
            set;
        }

        public Object State
        {
            get;
            set;
        }
    }
}