/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using Res = Emtf.Resources.TestContext;

namespace Emtf
{
    /// <summary>
    /// Represents a test context which provides additional functionality to test methods and their
    /// pre- and post-test actions.
    /// </summary>
    public class TestContext
    {
        #region Private Fields

        private Collection<LogEntry> _logEntries;

        #endregion Private Fields

        #region Constructor

        internal TestContext(Collection<LogEntry> logEntries)
        {
            if (logEntries == null)
                throw new ArgumentNullException("logEntries");

            _logEntries = logEntries;
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Aborts the current test immediately with the test result set to
        /// <see cref="TestResult.Aborted"/>.
        /// </summary>
        [DebuggerHidden]
        public void AbortTest()
        {
            AbortTest(null);
        }

        /// <summary>
        /// Aborts the current test immediately with the test result set to
        /// <see cref="TestResult.Aborted"/>.
        /// </summary>
        /// <param name="message">
        /// Message describing the reason for aborting the test. It is used in the
        /// <see cref="TestCompletedEventArgs.UserMessage"/> property of the
        /// <see cref="TestCompletedEventArgs"/>.
        /// </param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "May need to access instance members in the future and making this one static would lead to a misleading API design")]
        [DebuggerHidden]
        public void AbortTest(String message)
        {
            throw new TestAbortedException(Res.AbortTest_Message, message);
        }

        /// <summary>
        /// Adds text to the test log.
        /// </summary>
        /// <param name="message">
        /// Text to add.
        /// </param>
        public void Log(String message)
        {
            Log(message, false);
        }

        /// <summary>
        /// Adds text to the test log.
        /// </summary>
        /// <param name="message">
        /// Text to add.
        /// </param>
        /// <param name="failuresOnly">
        /// Flag indicating if the text should only be added if the test result is not <see cref="TestResult.Passed"/>.
        /// </param>
        public void Log(String message, Boolean failuresOnly)
        {
            _logEntries.Add(new LogEntry(message, false, failuresOnly));
        }

        /// <summary>
        /// Adds a line of text to the test log.
        /// </summary>
        /// <param name="message">
        /// Line of text to add.
        /// </param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LogLine")]
        public void LogLine(String message)
        {
            LogLine(message, false);
        }

        /// <summary>
        /// Adds a line of text to the test log.
        /// </summary>
        /// <param name="message">
        /// Line of text to add.
        /// </param>
        /// <param name="failuresOnly">
        /// Flag indicating if the text should only be added if the test result is not <see cref="TestResult.Passed"/>.
        /// </param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LogLine")]
        public void LogLine(String message, Boolean failuresOnly)
        {
            _logEntries.Add(new LogEntry(message, true, failuresOnly));
        }

        #endregion Public Methods

        #region Nested Types

        internal struct LogEntry
        {
            private String  _message;
            private Boolean _newLine;
            private Boolean _failuresOnly;

            internal LogEntry(String message, Boolean newLine, Boolean failuresOnly)
            {
                _message      = message;
                _newLine      = newLine;
                _failuresOnly = failuresOnly;
            }

            internal static String CreateUserLog(Collection<LogEntry> logEntries, Boolean testFailed)
            {
                if (logEntries == null || logEntries.Count == 0)
                    return null;

                Boolean       addNewLine = false;
                StringBuilder output     = new StringBuilder();

                foreach (LogEntry entry in logEntries)
                {
                    if (!entry._failuresOnly || testFailed)
                    {
                        if (addNewLine)
                            output.AppendLine();

                        output.Append(entry._message);
                        addNewLine = entry._newLine;
                    }
                }

                return output.ToString();
            }
        }

        #endregion Nested Types
    }
}

#endif