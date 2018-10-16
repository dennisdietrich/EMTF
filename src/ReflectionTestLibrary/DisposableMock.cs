/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using System;

namespace ReflectionTestLibrary
{
    public class DisposableMock : IDisposable
    {
        private bool _hasBeenDisposed;

        public bool HasBeenDisposed
        {
            get
            {
                return _hasBeenDisposed;
            }
        }

        public void NoOp()
        {
        }

        void IDisposable.Dispose()
        {
            _hasBeenDisposed = true;
        }
    }
}