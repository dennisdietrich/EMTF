# Embeddable Micro Test Framework
## What is EMTF?
1. It's embeddable! One reason why EMTF was created was the need for a simple test framework for educational purposes, allowing for a quick introduction to both the usage and implementation of a test framework like [MSTest](http://msdn.microsoft.com/en-us/library/ms182515.aspx) or [NUnit](http://www.nunit.com/). The other reason is that it can be useful to be able to put a couple of tests into an application under test rather than the other way around, for example in case of testing features that have many and/or complex dependencies that cannot be easily mocked or for directly integrating [smoke tests](https://en.wikipedia.org/wiki/Smoke_testing_(software)) into debug builds of an application.
2. It's micro! EMTF is easy to understand and use by limiting the number of features to a bare minimum and keeping the public API simple.
3. It's a test framework for the .NET Framework and Silverlight! And while one could say that EMTF is yet another wheel, anything that is suitable for making people look at what testing tools (or methodologies for that matter) are already out there and what their strengths and weaknesses are is a good thing.
## Documentation
* Quickstart: [Introducing EMTF](http://blogs.msdn.com/ddietric/archive/2009/02/15/introducing-emtf.aspx)
* Advanced features: [EMTF 2.0 Beta released](http://blogs.msdn.com/ddietric/archive/2009/07/14/emtf-2-0-beta-released.aspx)
* Using the wrapper factory for white box API testing: [D.C. and the Wrapper Factory](http://blogs.msdn.com/ddietric/archive/2010/01/24/d-c-and-the-wrapper-factory.aspx)
## What is required for working on/running/building EMTF?
* Programming language: C# 4.0
* Build tool: MSBuild 4.0
* Target frameworks: .NET Framework 4 and Silverlight 4
* EMTF test suite: Visual Studio 2010 Professional
* API Reference: Sandcastle and Sandcastle Help File Builder
