using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace ReactiveAnimations;

public static class Animator
{
    public static IAnimator CreateFromClock<T>(IObservable<T> clock)
    {
        return new AnonymousAnimator(duration =>
        {
            return Observable.Defer(() => 
            {
                var stopWatch = Stopwatch.StartNew();
                TimeSpan elapsed = TimeSpan.Zero;
                const double from = 0;
                const double to = 1;
                var speed = 1 / duration.TotalMilliseconds;

                AnimationFrame<double> GetAnimationFrame(double previousValue)
                {
                    var previousElapsed = elapsed;
                    elapsed = stopWatch.Elapsed;
                    var deltaTime = elapsed - previousElapsed;
                    var newValue = previousValue + speed * deltaTime.TotalMilliseconds;
                    return new(newValue, deltaTime);
                }

                stopWatch.Start();
                return clock.Scan(new AnimationFrame<double>(from, TimeSpan.Zero), (x, _) => GetAnimationFrame(x.Value))
                            .TakeUntil(x => x.Value <= from || x.Value >= to)
                            .Finally(stopWatch.Stop);
            });
        });
    }

    class AnonymousAnimator(Func<TimeSpan, IObservable<AnimationFrame<double>>> createAnimation) : IAnimator
    {
        public IObservable<AnimationFrame<double>> CreateAnimation(TimeSpan duration)
        {
            return createAnimation(duration);
        }
    }
}
