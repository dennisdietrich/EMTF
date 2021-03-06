﻿/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

#if !DISABLE_EMTF

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Res = Emtf.Resources.Logging.StreamLogger;

namespace Emtf.Logging
{
    /// <summary>
    /// Writes test run information and test execution results as plain text to a stream.
    /// </summary>
    public class StreamLogger : Logger
    {
        #region Private Field

        private Object   _syncRoot = new Object();
        private Stream   _stream;
        private Encoding _encoding;

        #endregion Private Field

        #region Public Properties

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="StreamLogger"/> class using UTF-8 encoding.
        /// </summary>
        /// <param name="executor">
        /// <see cref="TestExecutor"/> instance whose events are logged.
        /// </param>
        /// <param name="stream">
        /// <see cref="Stream"/> to log to.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="executor"/> or <paramref name="stream"/> is null.
        /// </exception>
        public StreamLogger(TestExecutor executor, Stream stream)
            : this(executor, stream, null, false)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="StreamLogger"/> class.
        /// </summary>
        /// <param name="executor">
        /// <see cref="TestExecutor"/> instance whose events are logged.
        /// </param>
        /// <param name="stream">
        /// <see cref="Stream"/> to log to.
        /// </param>
        /// <param name="encoding">
        /// Optional <see cref="Encoding"/> used for the output. The default is
        /// <see cref="UTF8Encoding"/>.
        /// </param>
        /// <param name="writePreamble">
        /// Flag indicating if the preamble (e.g. byte-order mark) is written to the stream before
        /// logging starts.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="executor"/> or <paramref name="stream"/> is null.
        /// </exception>
        /// <remarks>
        /// The preamble is only written if <paramref name="writePreamble"/> is true and the
        /// <see cref="Encoding"/> provides a preamble.
        /// </remarks>
        public StreamLogger(TestExecutor executor, Stream stream, Encoding encoding, Boolean writePreamble)
            : base(executor)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            _stream = stream;

            if (encoding == null)
                _encoding = new UTF8Encoding(true);
            else
                _encoding = encoding;

            Byte[] preamble;

            if (writePreamble && (preamble = _encoding.GetPreamble()) != null)
                _stream.Write(preamble, 0, preamble.Length);
        }

        #endregion Constructors

        #region Protected Methods

        /// <summary>
        /// Logs the start of a test run to the stream.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestRunEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="e"/> is null.
        /// </exception>
        protected override void TestRunStarted(TestRunEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

#if SILVERLIGHT
            Byte[] output = _encoding.GetBytes(String.Format(CultureInfo.CurrentCulture,
                                                             Res.TestRunStarted__Silverlight,
                                                             e.Total,
                                                             e.StartTime,
                                                             Environment.NewLine));
#else
            Byte[] output = _encoding.GetBytes(String.Format(CultureInfo.CurrentCulture,
                                                             Res.TestRunStarted__Desktop,
                                                             e.Total,
                                                             e.StartTime,
                                                             Environment.MachineName,
                                                             Environment.NewLine));
#endif

            _stream.Write(output, 0, output.Length);
            _stream.Flush();
        }

        /// <summary>
        /// Logs the completion of a test run to the stream.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestRunCompletedEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="e"/> is null.
        /// </exception>
        protected override void TestRunCompleted(TestRunCompletedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            Byte[] output;

            if (e.Total == 0)
                output = _encoding.GetBytes(Res.TestRunCompleted_NoTests);
            else
            {
                int testCountPlaceholderSize = new Int32[] { FormatedStringLength(e.PassedTests,   Res.TestRunCompleted_TestCountFormatString),
                                                             FormatedStringLength(e.FailedTests,   Res.TestRunCompleted_TestCountFormatString),
                                                             FormatedStringLength(e.ThrowingTests, Res.TestRunCompleted_TestCountFormatString),
                                                             FormatedStringLength(e.SkippedTests,  Res.TestRunCompleted_TestCountFormatString),
                                                             FormatedStringLength(e.AbortedTests,  Res.TestRunCompleted_TestCountFormatString)
                                                           }.Max();

                string        formatString = String.Format(CultureInfo.InvariantCulture, Res.TestRunCompleted_TestRunResultFormatTemplate, testCountPlaceholderSize);
                StringBuilder builder      = new StringBuilder();

                builder.AppendLine();
                builder.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                                 Res.TestRunCompleted_TestRunCompleted,
                                                 e.Total,
                                                 (e.EndTime - e.StartTime).TotalSeconds));

                if (e.PassedTests > 0)
                    builder.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                                     formatString,
                                                     Res.TestRunCompleted_TestsPassed,
                                                     e.PassedTests,
                                                     (double)e.PassedTests / (double)e.Total));
                if (e.SkippedTests > 0)
                    builder.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                                     formatString,
                                                     Res.TestRunCompleted_TestsSkipped,
                                                     e.SkippedTests,
                                                     (double)e.SkippedTests / (double)e.Total));
                if (e.AbortedTests > 0)
                    builder.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                                     formatString,
                                                     Res.TestRunCompleted_TestsAborted,
                                                     e.AbortedTests,
                                                     (double)e.AbortedTests / (double)e.Total));
                if (e.FailedTests > 0)
                    builder.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                                     formatString,
                                                     Res.TestRunCompleted_TestsFailed,
                                                     e.FailedTests,
                                                     (double)e.FailedTests / (double)e.Total));
                if (e.ThrowingTests > 0)
                    builder.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                                     formatString,
                                                     Res.TestRunCompleted_TestsThrowing,
                                                     e.ThrowingTests,
                                                     (double)e.ThrowingTests / (double)e.Total));

                output = _encoding.GetBytes(builder.ToString());
            }

            _stream.Write(output, 0, output.Length);
            _stream.Flush();
        }

        /// <summary>
        /// Logs the start of a test to the stream.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="e"/> is null.
        /// </exception>
        protected override void TestStarted(TestEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            lock (_syncRoot)
            {
                Byte[] output = _encoding.GetBytes(String.Format(CultureInfo.CurrentCulture,
                                                                 Res.TestStarted,
                                                                 UseFullTestName ? e.FullTestName : e.TestName,
                                                                 e.StartTime,
                                                                 Environment.NewLine));

                _stream.Write(output, 0, output.Length);
                _stream.Flush();
            }
        }

        /// <summary>
        /// Logs the completion of a test to the stream.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestCompletedEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="e"/> is null.
        /// </exception>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if the event arguments contain an unknown <see cref="TestResult"/> value.
        /// </exception>
        protected override void TestCompleted(TestCompletedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            int           executionTime = (int)((e.EndTime.Ticks - e.StartTime.Ticks) / 10000);
            StringBuilder builder       = new StringBuilder();

            switch (e.Result)
            {
                case TestResult.Passed:
                    builder.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                                    Res.TestCompleted_Passed,
                                                    UseFullTestName ? e.FullTestName : e.TestName,
                                                    executionTime));
                    break;
                case TestResult.Failed:
                    builder.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                                    Res.TestCompleted_Failed,
                                                    UseFullTestName ? e.FullTestName : e.TestName,
                                                    executionTime,
                                                    e.Message));

                    if (!String.IsNullOrEmpty(e.UserMessage))
                        builder.AppendLine(String.Format(CultureInfo.CurrentCulture, Res.TestCompleted_AdditionalMessage, e.UserMessage));

                    break;
                case TestResult.Exception:
                    builder.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                                    Res.TestCompleted_Throwing,
                                                    e.Exception.GetType().FullName,
                                                    UseFullTestName ? e.FullTestName : e.TestName,
                                                    executionTime));
                    builder.AppendLine(String.Format(CultureInfo.CurrentCulture, Res.TestCompleted_AdditionalMessage, e.Exception.Message));
                    break;
                case TestResult.Aborted:
                    builder.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                                    Res.TestCompleted_Aborted,
                                                    UseFullTestName ? e.FullTestName : e.TestName,
                                                    executionTime));

                    if (!String.IsNullOrEmpty(e.UserMessage))
                        builder.AppendLine(String.Format(CultureInfo.CurrentCulture, Res.TestCompleted_AdditionalMessage, e.UserMessage));

                    break;
                default:
                    throw new LoggerException(Res.TestCompleted_UnknownTestResult);
            }

            if (e.Log != null)
            {
                foreach (string line in e.Log.Split(new String[] { Environment.NewLine }, StringSplitOptions.None))
                    builder.AppendLine(String.Format(CultureInfo.CurrentCulture,
                                                    Res.TestCompleted_UserLogLine,
                                                    line));
            }

            lock (_syncRoot)
            {
                Byte[] output = _encoding.GetBytes(builder.ToString());
                _stream.Write(output, 0, output.Length);
                _stream.Flush();
            }
        }

        /// <summary>
        /// Logs that a test was skipped to the stream.
        /// </summary>
        /// <param name="e">
        /// The original <see cref="TestSkippedEventArgs"/> instance provided by the
        /// <see cref="TestExecutor"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="e"/> is null.
        /// </exception>
        /// <exception cref="Emtf.Logging.LoggerException">
        /// Thrown if the event arguments contain an unknown <see cref="SkipReason"/> value.
        /// </exception>
        protected override void TestSkipped(TestSkippedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            string message;

            switch (e.Reason)
            {
                case SkipReason.SkipTestAttributeDefined:
                    message = String.Format(CultureInfo.CurrentCulture,
                                            Res.TestSkipped_SkipTestAttribute,
                                            UseFullTestName ? e.FullTestName : e.TestName,
                                            Environment.NewLine);

                    if (!String.IsNullOrEmpty(e.Message))
                        message += String.Format(CultureInfo.CurrentCulture,
                                                 Res.TestSkipped_Message,
                                                 e.Message,
                                                 Environment.NewLine);

                    break;
                case SkipReason.TypeNotSupported:
                    message = String.Format(CultureInfo.CurrentCulture,
                                            Res.TestSkipped_TypeNotSupported,
                                            UseFullTestName ? e.FullTestName : e.TestName,
                                            Environment.NewLine);
                    break;
                case SkipReason.MethodNotSupported:
                    message = String.Format(CultureInfo.CurrentCulture,
                                            Res.TestSkipped_MethodNotSupported,
                                            UseFullTestName ? e.FullTestName : e.TestName,
                                            Environment.NewLine);
                    break;
                case SkipReason.ConstructorThrewException:
                    message = String.Format(CultureInfo.CurrentCulture,
                                            Res.TestSkipped_ConstructorThrewException,
                                            UseFullTestName ? e.FullTestName : e.TestName,
                                            e.Exception.GetType().FullName,
                                            Environment.NewLine);
                    message += String.Format(CultureInfo.CurrentCulture,
                                             Res.TestSkipped_Message,
                                             e.Exception.Message,
                                             Environment.NewLine);
                    break;
                case SkipReason.TestActionAttributeDefined:
                    message = String.Format(CultureInfo.CurrentCulture,
                                            Res.TestSkipped_TestActionAttribute,
                                            UseFullTestName ? e.FullTestName : e.TestName,
                                            Environment.NewLine);
                    break;
                default:
                    throw new LoggerException(Res.TestSkipped_UnknownSkipReason);
            }

            lock (_syncRoot)
            {
                Byte[] output = _encoding.GetBytes(message);
                _stream.Write(output, 0, output.Length);
                _stream.Flush();
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private static int FormatedStringLength(int value, string formatString)
        {
            return String.Format(CultureInfo.CurrentCulture, "{0:" + formatString + "}", value).Length;
        }

        #endregion Private Methods
    }
}

#endif