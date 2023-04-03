# .NET Framework Tests

Hot Potato's Middleware, Core, and OpenApi target frameworks now include .NET Standard 2.0 for the ability to be referenced by .NET Framework projects. To test the compatibility with .NET Framework, this test project contains a small collection of tests for the aforementioned projects to serve as smoke tests. The Middleware test checks if it calls the proxy correctly, the Core test checks if the proxy calls the processor and client correctly, and the OpenApi test checks if the Validation Strategy can be set up and run correctly, and that it calls Pass or Fail in the right situations.