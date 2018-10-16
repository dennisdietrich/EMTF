/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Reflection;

namespace Emtf.Dynamic
{
    public abstract class Snapshot
    {
        #region Protected Properties

        protected FieldInfo SourceField
        {
            get;
            set;
        }

        protected PropertyInfo SourceProperty
        {
            get;
            set;
        }

        protected Boolean IsNull
        {
            get;
            set;
        }

        #endregion Protected Properties

        #region Public Properties

        public Boolean Ignore
        {
            get;
            set;
        }

        #endregion Public Properties

        #region Public Methods

        public abstract void Verify(Object instance, IAssert assert, String parent = null);

        #endregion Public Methods
    }
}

#endif