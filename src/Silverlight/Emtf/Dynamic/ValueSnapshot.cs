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
    public class ValueSnapshot<TValue> : Snapshot
    {
        #region Public Properties

        public TValue Value
        {
            get;
            set;
        }

        #endregion Public Properties

        #region Constructors

        public ValueSnapshot(Object originalObject, FieldInfo field)
        {
            if (originalObject == null)
                throw new ArgumentNullException("originalObject");
            if (field == null)
                throw new ArgumentNullException("field");

            SourceField = field;

            Object rawValue;

            if ((rawValue = field.GetValue(originalObject)) == null)
                IsNull = true;
            else
                Value = (TValue)rawValue;
        }

        public ValueSnapshot(Object originalObject, PropertyInfo property)
        {
            if (originalObject == null)
                throw new ArgumentNullException("originalObject");
            if (property == null)
                throw new ArgumentNullException("property");
            if (!property.CanRead)
                throw new ArgumentException("The property cannot be read.", "property");
            if (property.GetIndexParameters().Length > 0)
                throw new ArgumentException("The property is indexed.", "property");

            SourceProperty = property;

            Object rawValue;

            if ((rawValue = property.GetValue(originalObject, null)) == null)
                IsNull = true;
            else
                Value = (TValue)rawValue;
        }

        #endregion Constructors

        #region Public Methods

        public override void Verify(Object instance, IAssert assert, String parent = null)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (assert == null)
                throw new ArgumentNullException("assert");

            if (SourceField != null)
                assert.AreEqual(Value,
                                (TValue)SourceField.GetValue(instance),
                                String.IsNullOrEmpty(parent) ? SourceField.Name : parent + "." + SourceField.Name);
            else if (SourceProperty != null)
                assert.AreEqual(Value,
                                (TValue)SourceProperty.GetValue(instance, null),
                                String.IsNullOrEmpty(parent) ? SourceProperty.Name : parent + "." + SourceProperty.Name);
            else
                throw new InvalidOperationException("Value snapshot doesn't have a source field or property.");
        }

        #endregion Public Methods
    }
}

#endif