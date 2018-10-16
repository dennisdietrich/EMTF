/*******************************************************
 * Copyright (C) Dennis Dietrich                       *
 * Released under the Microsoft Public License (Ms-PL) *
 * http://www.opensource.org/licenses/ms-pl.html       *
 *******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrimaryTestSuite.Support;
using System;
using System.Threading;

using EmtfReadOnlyAsyncResultWrapper = Emtf.ReadOnlyAsyncResultWrapper;

namespace PrimaryTestSuite
{
    [TestClass]
    public class ReadOnlyAsyncResultWrapperTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Verifies that the constructor .ctor(IAsyncResult) of the ReadOnlyAsyncResultWrapper throws an exception if the parameter is null")]
        public void ctor_IAsyncResult_ParamNull()
        {
            new EmtfReadOnlyAsyncResultWrapper(null);
        }

        [TestMethod]
        [Description("Tests the ReadOnlyAsyncResultWrapper class")]
        public void ReadOnlyAsyncResultWrapper()
        {
            MockAsyncResult mock = new MockAsyncResult { AsyncState             = null,
                                                         AsyncWaitHandle        = null,
                                                         CompletedSynchronously = false,
                                                         IsCompleted            = false };
            EmtfReadOnlyAsyncResultWrapper wrapper = new EmtfReadOnlyAsyncResultWrapper(mock);

            Assert.IsNull(wrapper.AsyncState);
            Assert.IsNull(wrapper.AsyncWaitHandle);
            Assert.IsFalse(wrapper.CompletedSynchronously);
            Assert.IsFalse(wrapper.IsCompleted);

            Object asyncState = new Object();
            mock = new MockAsyncResult { AsyncState             = asyncState,
                                         AsyncWaitHandle        = null,
                                         CompletedSynchronously = false,
                                         IsCompleted            = true };
            wrapper = new EmtfReadOnlyAsyncResultWrapper(mock);

            Assert.AreSame(asyncState, wrapper.AsyncState);
            Assert.IsNull(wrapper.AsyncWaitHandle);
            Assert.IsFalse(wrapper.CompletedSynchronously);
            Assert.IsTrue(wrapper.IsCompleted);

            using (WaitHandle asyncWaitHandle = new ManualResetEvent(false))
            {
                mock = new MockAsyncResult { AsyncState             = null,
                                             AsyncWaitHandle        = asyncWaitHandle,
                                             CompletedSynchronously = true,
                                             IsCompleted            = false };
                wrapper = new EmtfReadOnlyAsyncResultWrapper(mock);

                Assert.IsNull(wrapper.AsyncState);
                Assert.AreSame(asyncWaitHandle, wrapper.AsyncWaitHandle);
                Assert.IsTrue(wrapper.CompletedSynchronously);
                Assert.IsFalse(wrapper.IsCompleted);

                mock = new MockAsyncResult { AsyncState             = asyncState,
                                             AsyncWaitHandle        = asyncWaitHandle,
                                             CompletedSynchronously = true,
                                             IsCompleted            = true };
                wrapper = new EmtfReadOnlyAsyncResultWrapper(mock);

                Assert.AreSame(asyncState, wrapper.AsyncState);
                Assert.AreSame(asyncWaitHandle, wrapper.AsyncWaitHandle);
                Assert.IsTrue(wrapper.CompletedSynchronously);
                Assert.IsTrue(wrapper.IsCompleted);
            }
        }
    }
}