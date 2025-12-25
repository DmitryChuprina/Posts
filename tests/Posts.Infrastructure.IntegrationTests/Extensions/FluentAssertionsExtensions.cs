using FluentAssertions;
using FluentAssertions.Equivalency;

namespace Posts.Infrastructure.IntegrationTests.Extensions
{
    public static class FluentAssertionsExtensions
    {
        public static EquivalencyOptions<T> WithTimeTolerance<T>(
            this EquivalencyOptions<T> options,
            int seconds = 3)
        {
            var tolerance = TimeSpan.FromSeconds(seconds);

            return options
                .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, tolerance))
                .WhenTypeIs<DateTime>()
                .Using<DateTime?>(ctx =>
                {
                    if (ctx.Subject is null && ctx.Expectation is null)
                    {
                        return;
                    }

                    ctx.Subject.Should().NotBeNull();
                    ctx.Expectation.Should().NotBeNull();

                    ctx.Subject!.Value.Should().BeCloseTo(ctx.Expectation!.Value, tolerance);
                })
                .WhenTypeIs<DateTime?>();
        }
    }
}
