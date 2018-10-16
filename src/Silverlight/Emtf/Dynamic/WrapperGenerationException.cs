/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;

using Res = Emtf.Resources.Dynamic.WrapperGenerationException;

namespace Emtf.Dynamic
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class WrapperGenerationException : Exception
    {
        #region Public Properties

        public Type WrappedType
        {
            get;
            private set;
        }

        public Type WrapperType
        {
            get;
            private set;
        }

        public ReadOnlyCollection<ConstructorInfo> SkippedConstructors
        {
            get;
            private set;
        }

        public ReadOnlyCollection<EventInfo> SkippedEvents
        {
            get;
            private set;
        }

        public ReadOnlyCollection<FieldInfo> SkippedFields
        {
            get;
            private set;
        }

        public ReadOnlyCollection<PropertyInfo> SkippedProperties
        {
            get;
            private set;
        }

        public ReadOnlyCollection<MethodInfo> SkippedMethods
        {
            get;
            private set;
        }

        #endregion Public Properties

        #region Constructors

        public WrapperGenerationException()
            : base(Res.ctor_DefaultMessage)
        {
        }

        public WrapperGenerationException(String message) : base(message)
        {
        }

        public WrapperGenerationException(String message, Exception innerException) : base(message, innerException)
        {
        }

        public WrapperGenerationException(Type wrappedType, Type wrapperType, Collection<ConstructorInfo> skippedConstructors)
            : base(Res.ctor_UnsupportedConstructorMessage, null)
        {
            if (wrappedType == null)
                throw new ArgumentNullException("wrappedType");
            if (wrapperType == null)
                throw new ArgumentNullException("wrapperType");
            if (skippedConstructors == null)
                throw new ArgumentNullException("skippedConstructors");

            SkippedEvents     = new ReadOnlyCollection<EventInfo>(new EventInfo[0]);
            SkippedFields     = new ReadOnlyCollection<FieldInfo>(new FieldInfo[0]);
            SkippedProperties = new ReadOnlyCollection<PropertyInfo>(new PropertyInfo[0]);
            SkippedMethods    = new ReadOnlyCollection<MethodInfo>(new MethodInfo[0]);

            WrappedType = wrappedType;
            WrapperType = wrapperType;

            SkippedConstructors = new ReadOnlyCollection<ConstructorInfo>(skippedConstructors);
        }

        public WrapperGenerationException(Type wrappedType,
                                          Type wrapperType,
                                          Collection<EventInfo>    skippedEvents,
                                          Collection<FieldInfo>    skippedFields,
                                          Collection<PropertyInfo> skippedProperties,
                                          Collection<MethodInfo>   skippedMethods)
            : this(Res.ctor_UnsupportedMemberMessage, null)
        {
            if (wrappedType == null)
                throw new ArgumentNullException("wrappedType");
            if (wrapperType == null)
                throw new ArgumentNullException("wrapperType");
            if (skippedEvents == null)
                throw new ArgumentNullException("skippedEvents");
            if (skippedFields == null)
                throw new ArgumentNullException("skippedFields");
            if (skippedProperties == null)
                throw new ArgumentNullException("skippedProperties");
            if (skippedMethods == null)
                throw new ArgumentNullException("skippedMethods");

            SkippedConstructors = new ReadOnlyCollection<ConstructorInfo>(new ConstructorInfo[0]);

            WrappedType = wrappedType;
            WrapperType = wrapperType;

            SkippedEvents     = new ReadOnlyCollection<EventInfo>(skippedEvents);
            SkippedFields     = new ReadOnlyCollection<FieldInfo>(skippedFields);
            SkippedProperties = new ReadOnlyCollection<PropertyInfo>(skippedProperties);
            SkippedMethods    = new ReadOnlyCollection<MethodInfo>(skippedMethods);
        }

#if !SILVERLIGHT
        protected WrapperGenerationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            WrappedType = (Type)info.GetValue("WrappedType", typeof(Type));
            WrapperType = (Type)info.GetValue("WrapperType", typeof(Type));

            SkippedConstructors = (ReadOnlyCollection<ConstructorInfo>) info.GetValue("SkippedConstructors", typeof(ReadOnlyCollection<ConstructorInfo>));
            SkippedEvents       = (ReadOnlyCollection<EventInfo>)       info.GetValue("SkippedEvents",       typeof(ReadOnlyCollection<EventInfo>));
            SkippedFields       = (ReadOnlyCollection<FieldInfo>)       info.GetValue("SkippedFields",       typeof(ReadOnlyCollection<FieldInfo>));
            SkippedMethods      = (ReadOnlyCollection<MethodInfo>)      info.GetValue("SkippedMethods",      typeof(ReadOnlyCollection<MethodInfo>));
            SkippedProperties   = (ReadOnlyCollection<PropertyInfo>)    info.GetValue("SkippedProperties",   typeof(ReadOnlyCollection<PropertyInfo>));
        }
#endif

        #endregion Constructors

        #region Public Methods

#if !SILVERLIGHT
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("WrappedType", WrappedType);
            info.AddValue("WrapperType", WrapperType);

            info.AddValue("SkippedConstructors", SkippedConstructors);
            info.AddValue("SkippedEvents",       SkippedEvents);
            info.AddValue("SkippedFields",       SkippedFields);
            info.AddValue("SkippedMethods",      SkippedMethods);
            info.AddValue("SkippedProperties",   SkippedProperties);
        }
#endif

        #endregion Public Methods
    }
}

#endif