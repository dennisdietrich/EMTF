/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Emtf
{
#if !SILVERLIGHT
    [Serializable]
#endif
    internal class TestAbortedException : TestRunException
    {
        #region Private Fields

        private string _userMessage;

        #endregion Private Fields

        #region Internal Properties

        internal String UserMessage
        {
            get
            {
                return _userMessage;
            }
        }

        #endregion Internal Properties

        #region Constructors

        internal TestAbortedException(String message, String userMessage)
            : base(message)
        {
            _userMessage = userMessage;
        }

#if !SILVERLIGHT
        protected TestAbortedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _userMessage = info.GetString("UserMessage");
        }
#endif

        #endregion Constructors

        #region Public Methods

#if !SILVERLIGHT
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("UserMessage", _userMessage);
        }
#endif

        #endregion Public Methods
    }
}

#endif