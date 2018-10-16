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
    /// <summary>
    /// Allow uniquely identifying objects without affecting their lifetime.
    /// </summary>
    public class ObjectTracker
    {
        #region Private Fields

        private Object  _syncRoot = new Object();
        private Boolean _cleanupPendingOrRunning;

        private List<WeakReference> _references  = new List<WeakReference>();
        private List<Guid>          _objectGuids = new List<Guid>();

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Provides a unique GUID for class instances.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the instance.
        /// </typeparam>
        /// <param name="instance">
        /// The instance for which to get a GUID.
        /// </param>
        /// <returns>
        /// A unique GUID for <paramref name="instance"/> or <see cref="Guid.Empty"/> if
        /// <paramref name="instance"/> is null.
        /// </returns>
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