/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;

using Res = Emtf.Resources.AsyncTestRun;

namespace Emtf
{
    internal sealed class AsyncTestRun : IAsyncResult, IDisposable
    {
        #region Private Fields

        private volatile Boolean   _isCompleted;
        private volatile Exception _exception;

        private readonly TestExecutor            _executor;
        private readonly IEnumerable<Assembly>   _assemblies;
        private readonly IEnumerable<MethodInfo> _methods;
        private readonly IList<String>           _groups;
        private readonly ParameterType           _parameterType;

        private readonly IAsyncResult  _readOnlyAsyncResult;
        private readonly Object        _syncRoot;
        private readonly AsyncCallback _callback;
        private readonly Object        _state;
        private readonly Thread        _thread;

        private ManualResetEvent _event;

        #endregion Private Fields

        #region Public Properties

        public object AsyncState
        {
            get
            {
                return _state;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_event == null)
                        _event = new ManualResetEvent(_isCompleted);
                }

                return _event;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return _isCompleted;
            }
        }

        #endregion Public Properties

        #region Internal Properties

        internal IAsyncResult ReadOnlyAsyncResult
        {
            get
            {
                return _readOnlyAsyncResult;
            }
        }

        internal Exception Exception
        {
            get
            {
                return _exception;
            }
        }

        #endregion Internal Properties

        #region Constructors

        private AsyncTestRun(BaseParameters parameters)
        {
            _executor  = parameters.Executor;
            _groups    = parameters.Groups;

            _syncRoot = new Object();
            _callback = parameters.Callback;
            _state    = parameters.State;

            _readOnlyAsyncResult = new ReadOnlyAsyncResultWrapper(this);

            _thread = new Thread(StartTestRun);
        }

#if !SILVERLIGHT
        internal AsyncTestRun(TestExecutor executor, IList<String> groups, AsyncCallback callback, Object state)
            : this(new BaseParameters(executor, groups, callback, state))
        {
            _parameterType = ParameterType.None;

            _thread.Start();
        }
#endif

        internal AsyncTestRun(TestExecutor executor, IEnumerable<Assembly> assemblies, IList<String> groups, AsyncCallback callback, Object state)
            : this(new BaseParameters(executor, groups, callback, state))
        {
            _assemblies    = assemblies;
            _parameterType = ParameterType.IEnumerableOfAssembly;

            _thread.Start();
        }

        internal AsyncTestRun(TestExecutor executor, IEnumerable<MethodInfo> methods, IList<String> groups, AsyncCallback callback, Object state)
            : this(new BaseParameters(executor, groups, callback, state))
        {
            _methods       = methods;
            _parameterType = ParameterType.IEnumerableOfMethodInfo;

            _thread.Start();
        }

        #endregion Constructors

        #region Public Methods

        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (_event != null)
                    ((IDisposable)_event).Dispose();
            }
        }

        #endregion Public Methods

        #region Private Methods

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Begin/End pattern requires exception to be thrown from End method")]
        private void StartTestRun()
        {
            try
            {
                switch (_parameterType)
                {
#if !SILVERLIGHT
                    case ParameterType.None:
                        _executor.Execute(_groups);
                        break;
#endif
                    case ParameterType.IEnumerableOfMethodInfo:
                        _executor.Execute(_methods, _groups);
                        break;
                    case ParameterType.IEnumerableOfAssembly:
                        _executor.Execute(_assemblies, _groups);
                        break;
                    default:
                        throw new InvalidOperationException(Res.StartTestRun_InvalidParameterType);
                }
            }
            catch (Exception e)
            {
                _exception = e;
            }
            finally
            {
                lock (_syncRoot)
                {
                    _isCompleted = true;

                    if (_event != null)
                        _event.Set();
                }

                if (_callback != null)
                    _callback(_readOnlyAsyncResult);
            }
        }

        #endregion Private Methods

        #region Nested Types

        private enum ParameterType
        {
            None                    = 1,
            IEnumerableOfAssembly   = 2,
            IEnumerableOfMethodInfo = 3
        }

        private class BaseParameters
        {
            internal BaseParameters(TestExecutor executor, IList<String> groups, AsyncCallback callback, Object state)
            {
                Executor = executor;
                Groups   = groups;
                Callback = callback;
                State    = state;
            }

            internal TestExecutor Executor
            {
                get;
                set;
            }

            internal IList<String> Groups
            {
                get;
                set;
            }

            internal AsyncCallback Callback
            {
                get;
                set;
            }

            internal Object State
            {
                get;
                set;
            }
        }

        #endregion Nested Types
    }
}

#endif