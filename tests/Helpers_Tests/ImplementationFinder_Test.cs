using webapi.Attributes;
using webapi.Helpers;

namespace tests.Helpers_Tests
{
    public class ImplementationFinder_Test
    {
        public class ImplementationFinderTests
        {
            private interface IService { }

            [ImplementationKey("key1")]
            private class Service1 : IService { }

            [ImplementationKey("key2")]
            private class Service2 : IService { }

            [ImplementationKey("key3")]
            private class Service3 : IService { }

            [Fact]
            public void GetImplementationByKey_ValidKey_ReturnsImplementation()
            {
                var finder = new ImplementationFinder();
                var implementations = new List<IService>
                {
                    new Service1(),
                    new Service2(),
                    new Service3()
                };

                var result = finder.GetImplementationByKey(implementations, "key2");

                Assert.IsType<Service2>(result);
            }

            [Fact]
            public void GetImplementationByKey_InvalidKey_ThrowsNotImplementedException()
            {
                var finder = new ImplementationFinder();
                var implementations = new List<IService>
                {
                    new Service1(),
                    new Service2(),
                    new Service3()
                };

                Assert.Throws<NotImplementedException>(() => finder.GetImplementationByKey(implementations, "invalidKey"));
            }
        }
    }
}
