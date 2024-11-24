using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace ReactiveAnimations;

// ReSharper disable once InconsistentNaming
public static class IAnimatableExtensions
{
    public static IObservable<AnimationFrame<double>> Transform(this IObservable<AnimationFrame<double>> @this, Func<double, double> easingFunction)
    {
        return @this.Select(x => x with { Value = easingFunction(x.Value) });
    }

    public static IObservable<AnimationFrame<double>> WithQuadraticEasing(this IObservable<AnimationFrame<double>> @this)
    {
        return @this.Transform(static x => x * x);
    }

    public static IObservable<AnimationFrame<double>> WithCubicEasing(this IObservable<AnimationFrame<double>> @this)
    {
        return @this.Transform(static x => x * x * x);
    }

    public static IObservable<AnimationFrame<double>> WithEaseOutBounce(this IObservable<AnimationFrame<double>> @this)
    {
        return @this.Transform(static x =>
        {
            const double n1 = 7.5625;
            const double d1 = 2.75;

            return x switch
            {
                < 1 / d1 => n1 * x * x,
                < 2 / d1 => n1 * (x -= 1.5 / d1) * x + 0.75,
                < 2.5 / d1 => n1 * (x -= 2.25 / d1) * x + 0.9375,
                _ => n1 * (x -= 2.625 / d1) * x + 0.984375
            };
        });
    }

    public static IObservable<AnimationFrame<double>> GoBackwardsWhenFinished(this IObservable<AnimationFrame<double>> @this)
    {
        return @this.Concat(@this.Backwards());
    }

    public static IObservable<AnimationFrame<double>> Backwards(this IObservable<AnimationFrame<double>> @this)
    {
        return @this.Transform(static x => 1 - x);
    }

    public static IObservable<AnimationFrame<double>> Animate(this IObservable<AnimationFrame<double>> @this,
                                                              Action<AnimationFrame<double>> action,
                                                              AnimationCancellationOptions animationCancellationOptions = AnimationCancellationOptions.KeepLastValue, double from = 0, double to = 1)
    {
        return Observable.Create<AnimationFrame<double>>(observer =>
        {
            Stopwatch? stopWatch = animationCancellationOptions == AnimationCancellationOptions.KeepLastValue
                ? null 
                : Stopwatch.StartNew();

            double lastValue = 0;

            var singleAssignmentDisposable = new SingleAssignmentDisposable();
            var disposable = @this.Do(x =>
                                  {
                                      var value = Math.Clamp(from + x.Value * (to - from), from, to);
                                      lastValue = value;
                                      stopWatch?.Reset();
                                      action(x with { Value = value });
                                  })
                                  .SubscribeSafe(observer);

            singleAssignmentDisposable.Disposable = disposable;

            return () =>
            {
                try
                {

                    if (animationCancellationOptions is not AnimationCancellationOptions.KeepLastValue && lastValue != to)
                    {
                        var elapsed = stopWatch!.Elapsed;
                        stopWatch.Stop();

                        var value = animationCancellationOptions is AnimationCancellationOptions.SnapToStart ? from : to;
                        action(new AnimationFrame<double>(value, elapsed));
                    }
                }
                finally
                {
                    singleAssignmentDisposable.Dispose();
                }
            };
        });

    }
}