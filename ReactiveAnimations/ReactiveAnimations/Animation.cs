using System;

namespace ReactiveAnimations;

public interface IAnimator
{
    IObservable<AnimationFrame<double>> CreateAnimation(TimeSpan duration);
}

public enum AnimationCancellationOptions
{
    KeepLastValue,
    SnapToStart,
    SnapToEnd
}

public record struct AnimationFrame<T>(T Value, TimeSpan DeltaTime);