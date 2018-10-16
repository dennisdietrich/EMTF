/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Emtf.Dynamic
{
    public class ObjectTracker
    {
        #region Private Fields

        private Object  _syncRoot = new Object();
        private Boolean _cleanupPendingOrRunning;

        private List<WeakReference> _references  = new List<WeakReference>();
        private List<Guid>          _objectGuids = new List<Guid>();

        #endregion Private Fields

        #region Public Methods

        public Guid GetObjectGuid<T>(T instance) where T : class
        {
            if (instance == null)
                return Guid.Empty;

            lock (_syncRoot)
            {
                object obj;

                for (int i = 0; i < _references.Count; i++)
                {
                    if ((obj = _references[i].Target) != null)
                    {
                        if (Object.ReferenceEquals(instance, obj))
                            return _objectGuids[i];
                    }
                    else
                    {
                        if (!_cleanupPendingOrRunning)
                        {
                            _cleanupPendingOrRunning = true;
                            ThreadPool.QueueUserWorkItem(Cleanup);
                        }
                    }
                }

                Guid newGuid = Guid.NewGuid();

                _references.Add(new WeakReference(instance));
                _objectGuids.Add(newGuid);

                return newGuid;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void Cleanup(Object state)
        {
            lock (_syncRoot)
            {
                try
                {
                    for (int i = _references.Count - 1; i > -1; i--)
                        if (!_references[i].IsAlive)
                        {
                            _references.RemoveAt(i);
                            _objectGuids.RemoveAt(i);
                        }
                }
                finally
                {
                    _cleanupPendingOrRunning = false;
                }
            }
        }

        #endregion Private Methods
    }
}

#endif