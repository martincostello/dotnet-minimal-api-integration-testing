// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp
{
    [Collection(HttpServerCollection.Name)]
    public abstract class IntegrationTest : IDisposable
    {
        private bool _disposed;

        protected IntegrationTest(HttpServerFixture fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            OutputHelper = outputHelper;
            Fixture.SetOutputHelper(OutputHelper);
        }

        ~IntegrationTest()
        {
            Dispose(false);
        }

        protected HttpServerFixture Fixture { get; }

        protected ITestOutputHelper OutputHelper { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Fixture?.ClearOutputHelper();
                }

                _disposed = true;
            }
        }
    }
}
