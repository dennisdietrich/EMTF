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
    /// <summary>
    /// The exception that is optionally thrown when the wrapper generator encounters unsupported
    /// members.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class WrapperGenerationException : Exception
    {
        #region Public Properties

        /// <summary>
        /// Gets the type for which a wrapper was generated.
        /// </summary>
        public Type WrappedType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type of the dynamically generated wrapper.
        /// </summary>
        public Type WrapperType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of all constructors that were skipped because they are not supported.
        /// </summary>
        public ReadOnlyCollection<ConstructorInfo> SkippedConstructors
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of all events that were skipped because they are not supported.
        /// </summary>
        public ReadOnlyCollection<EventInfo> SkippedEvents
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of all fields that were skipped because they are not supported.
        /// </summary>
        public ReadOnlyCollection<FieldInfo> SkippedFields
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of all properties that were skipped because they are not supported.
        /// </summary>
        public ReadOnlyCollection<PropertyInfo> SkippedProperties
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of all methods that were skipped because they are not supported.
        /// </summary>
        public ReadOnlyCollection<MethodInfo> SkippedMethods
        {
            get;
            private set;
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="WrapperGenerationException"/> class using a
        /// default message.
        /// </summary>
        public WrapperGenerationException()
            : base(Res.ctor_DefaultMessage)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WrapperGenerationException"/> class with a
        /// caller provided message.
        /// </summary>
        /// <param name="message">
        /// Error message for this instance.
        /// </param>
        public WrapperGenerationException(String message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WrapperGenerationException"/> class with
        /// caller provided message and inner exception.
        /// </summary>
        /// <param name="message">
        /// Error message for this instance.
        /// </param>
        /// <param name="innerException">
        /// Exception that directly or indirectly led to the wrapper generation exception.
        /// </param>
        public WrapperGenerationException(String message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WrapperGenerationException"/> class. This
        /// constructor is used if the generation of a constructor wrapper fails.
        /// </summary>
        /// <param name="wrappedType">
        /// The type for which a wrapper was generated.
        /// </param>
        /// <param name="wrapperType">
        /// The type of the dynamically generated wrapper.
        /// </param>
        /// <param name="skippedConstructors">
        /// A collection of all constructors that were skipped because they are not supported.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="wrappedType"/>, <paramref name="wrapperType"/> or
        /// <paramref name="skippedConstructors"/> is null.
        /// </exception>
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

        /// <summary>
        /// Creates a new instance of the <see cref="WrapperGenerationException"/> class. This
        /// constructor is used if the generation of a static or instance wrapper fails.
        /// </summary>
        /// <param name="wrappedType">
        /// The type for which a wrapper was generated.
        /// </param>
        /// <param name="wrapperType">
        /// The type of the dynamically generated wrapper.
        /// </param>
        /// <param name="skippedEvents">
        /// A collection of all events that were skipped because they are not supported.
        /// </param>
        /// <param name="skippedFields">
        /// A collection of all fields that were skipped because they are not supported.
        /// </param>
        /// <param name="skippedProperties">
        /// A collection of all properties that were skipped because they are not supported.
        /// </param>
        /// <param name="skippedMethods">
        /// A collection of all methods that were skipped because they are not supported.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="wrappedType"/>, <paramref name="wrapperType"/>,
        /// <paramref name="skippedEvents"/>, <paramref name="skippedFields"/>,
        /// <paramref name="skippedProperties"/> or <paramref name="skippedMethods"/> is null.
        /// </exception>
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
        /// <summary>
        /// Creates a new instance of the <see cref="WrapperGenerationException"/> class and
        /// initializes it with serialized data.
        /// </summary>
        /// <param name="info">
        /// Serialized state of the original <see cref="WrapperGenerationException"/> object.
        /// </param>
        /// <param name="context">
        /// Contextual information for serialization and deserialization.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="info"/> is null.
        /// </exception>
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
        /// <summary>
        /// Serializes the state of the current instance.
        /// </summary>
        /// <param name="info">
        /// <see cref="SerializationInfo"/> object the current state is written to.
        /// </param>
        /// <param name="context">
        /// Contextual information for serialization and deserialization.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="info"/> is null.
        /// </exception>
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