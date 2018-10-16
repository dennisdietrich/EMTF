/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Threading;

namespace Emtf
{
    /// <summary>
    /// Allows unsynchronized read-only access to the IAsyncResult properties of an object.
    /// </summary>
    public class ReadOnlyAsyncResultWrapper : IAsyncResult
    {
        #region Private Fields

        private IAsyncResult _asyncResult;

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ReadOnlyAsyncResultWrapper"/> class.
        /// </summary>
        /// <param name="asyncResult">
        /// The object to wrap.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="asyncResult"/> is null.
        /// </exception>
        public ReadOnlyAsyncResultWrapper(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            _asyncResult = asyncResult;
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Gets a user-defined object providing context for the asynchronous operation.
        /// </summary>
        public object AsyncState
        {
            get
            {
                return _asyncResult.AsyncState;
            }
        }

        /// <summary>
        /// Gets a <see cref="WaitHandle"/> that is signaled when the asynchronous operation completes.
        /// </summary>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return _asyncResult.AsyncWaitHandle;
            }
        }

        /// <summary>
        /// Gets a flag indicating if the operation completed synchronously.
        /// </summary>
        public bool CompletedSynchronously
        {
            get
            {
                return _asyncResult.CompletedSynchronously;
            }
        }

        /// <summary>
        /// Gets a flag indicating if the operation has been completed.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return _asyncResult.IsCompleted;
            }
        }

        #endregion Public Properties
    }
}

#endif