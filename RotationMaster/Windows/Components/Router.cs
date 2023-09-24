using System;
using System.Collections.Generic;

namespace RotationMaster.Windows;

public struct OnScreenChangeEventArgs<T> where T : Enum
{
    public T From;
    public T To;
}

public class Router<T> where T : Enum
{
    private T currentScreen;
    public event Action<OnScreenChangeEventArgs<T>> OnScreenChange;
    
    private Queue<T> History { get; } = new();

    public T CurrentScreen
    {
        get => currentScreen;
        private set
        {
            var oldScreen = currentScreen;
            currentScreen = value;
            OnScreenChange?.Invoke(new OnScreenChangeEventArgs<T> { From = oldScreen, To = currentScreen });
        }
    }

    public bool HasHistory => History.Count > 0;

    public Router(T currentScreen)
    {
        CurrentScreen = currentScreen;
    }

    public void GoTo(T screen)
    {
        History.Enqueue(CurrentScreen);

        CurrentScreen = screen;
    }

    public void GoBack()
    {
        if (History.Count == 0)
        {
            return;
        }
        
        CurrentScreen = History.Dequeue();
    }
}
