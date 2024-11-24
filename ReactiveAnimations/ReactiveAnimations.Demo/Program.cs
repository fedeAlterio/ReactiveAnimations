using System.Reactive.Linq;
using ReactiveAnimations;

var animator = Animator.CreateFromClock(Observable.Interval(TimeSpan.FromSeconds(1 / 60.0)));
var subscription = animator
                   .CreateAnimation(TimeSpan.FromSeconds(1))
                   .GoBackwardsWhenFinished()
                   .WithEaseOutBounce()
                   .Repeat()
                   .Animate(x =>
                   {
                       var text = new string('.', 21);
                       Console.Clear();
                       Print(text, x.Value);
                   })
                   .Subscribe();

Console.ReadLine();
subscription.Dispose();
Console.WriteLine("Animation cancelled");
Console.ReadLine();

void Print(string pattern, double percentage)
{
    var diameterLength = pattern.Length;

    for (var row = 0; row < (diameterLength * 2) * percentage; row += 2)
    {
        // 0 => 1
        // 2 => 3
        // 4 => 1
        var totalChars = row < diameterLength ? (row + 1) : diameterLength - ((row + 1) - diameterLength);
        var emptyHalfCount = (diameterLength - totalChars) / 2;

        var spaces = emptyHalfCount > 0 ? new string(' ', emptyHalfCount) : "";
        Console.Write(spaces);
        Console.Write(pattern.Substring(spaces.Length, totalChars));
        Console.Write(spaces);
        Console.Write(Environment.NewLine);
    }
}

Console.ReadLine();
subscription.Dispose();

Console.ReadLine();
