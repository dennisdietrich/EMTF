/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;

namespace PrimaryTestSuite.Support
{
    public class EventData
    {
        public string Name
        {
            get;
            set;
        }

        public object Sender
        {
            get;
            set;
        }

        public EventArgs EventArgs
        {
            get;
            set;
        }
    }
}