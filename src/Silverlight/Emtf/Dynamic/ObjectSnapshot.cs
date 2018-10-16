/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Collections.Generic;

namespace Emtf.Dynamic
{
    public class ObjectSnapshot : Snapshot
    {
        #region Private Fields

        private VerificationMode _verificationMode;
        private Guid             _identity;
        private Type             _sourceType;

        private Dictionary<String, ObjectSnapshot> _objectSnapshots;
        private Dictionary<String, Snapshot>       _valueSnapshots;

        #endregion Private Fields

        #region Public Properties

        public VerificationMode VerificationMode
        {
            get
            {
                return _verificationMode;
            }
            set
            {
                _verificationMode = value;
            }
        }

        #endregion Public Properties

        #region Constructors

        public ObjectSnapshot(Object instance)
        {
            if (instance == null)
            {
                IsNull = true;
            }
            else
            {

            }
        }

        #endregion Constructors

        #region Public Methods

        public override void Verify(Object instance, IAssert assert, String parent = null)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (assert == null)
                throw new ArgumentNullException("assert");

            assert.AreEqual(_sourceType,
                            instance.GetType(),
                            parent == null ? String.Empty : parent);

            if ((_verificationMode & VerificationMode.Identity) != 0)
                assert.AreEqual(_identity,
                                Guid.Empty,
                                parent == null ? String.Empty : parent);

            if ((_verificationMode & VerificationMode.State) != 0)
            {
                foreach (KeyValuePair<String, Snapshot> valueSnapshot in _valueSnapshots)
                    if (!valueSnapshot.Value.Ignore)
                        valueSnapshot.Value.Verify(instance,
                                                   assert,
                                                   String.IsNullOrEmpty(parent) ? valueSnapshot.Key : parent + "." + valueSnapshot.Key);

                foreach (KeyValuePair<String, ObjectSnapshot> objectSnapshot in _objectSnapshots)
                {
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private Boolean IsPrimitive(Type type)
        {
            if (type == typeof(Boolean) ||
                type == typeof(Byte)    ||
                type == typeof(SByte)   ||
                type == typeof(Int16)   ||
                type == typeof(UInt16)  ||
                type == typeof(Int32)   ||
                type == typeof(UInt32)  ||
                type == typeof(Int64)   ||
                type == typeof(UInt64)  ||
                type == typeof(IntPtr)  ||
                type == typeof(UIntPtr) ||
                type == typeof(Char)    ||
                type == typeof(Double)  ||
                type == typeof(Single)  ||
                type == typeof(String))
                return true;

            return false;
        }

        #endregion Private Methods
    }
}

#endif