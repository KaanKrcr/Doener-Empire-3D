using DoenerEmpire.Core;
using Xunit;

namespace DoenerEmpire.Logic.Tests
{
    public class EventBusTests
    {
        private readonly struct TestEvent
        {
            public readonly int Value;

            public TestEvent(int value)
            {
                Value = value;
            }
        }

        [Fact]
        public void PublishInvokesSubscribers()
        {
            EventBus bus = new();
            int observed = 0;

            bus.Subscribe<TestEvent>(e => observed = e.Value);
            bus.Publish(new TestEvent(42));

            Assert.Equal(42, observed);
        }

        [Fact]
        public void DisposedSubscriptionStopsReceivingEvents()
        {
            EventBus bus = new();
            int calls = 0;

            using (bus.Subscribe<TestEvent>(_ => calls++))
            {
                bus.Publish(new TestEvent(1));
            }

            bus.Publish(new TestEvent(2));

            Assert.Equal(1, calls);
        }
    }
}
