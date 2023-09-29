using System;
using System.Runtime.InteropServices;
using System.Timers;
using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Logging;

namespace RotationMaster.Services;

public class CountDownService : IDisposable
{
    private const string CountDownSignature = "48 89 5C 24 ?? 57 48 83 EC 40 8B 41";

    private delegate void onCountDownChangeDelegate(ulong value);

    private readonly Hook<onCountDownChangeDelegate>? onCountDownChangeDelegateHook;

    private Timer countDownNoLongerActiveTimer;
    public static CountDownService Instance { get; private set; }
    public bool IsCountingDown { get; private set; }
    public float CountDownValue { get; private set; }

    public event Action OnCountDownStopped;
    private CountDownService(ISigScanner scanner)
    {
        var actionUsedFuncPtr = scanner.ScanText(CountDownSignature);

        onCountDownChangeDelegateHook = Hook<onCountDownChangeDelegate>.FromAddress(actionUsedFuncPtr, HandleCountDownChange);
        onCountDownChangeDelegateHook.Enable();

        countDownNoLongerActiveTimer = new Timer(50);
        countDownNoLongerActiveTimer.Elapsed += (_, _) => CountDownStopped();
    }

    public static void Init(ISigScanner scanner)
    {
        if (Instance == null)
        {
            Instance = new CountDownService(scanner);
        }
    }

    public void Dispose()
    {
        onCountDownChangeDelegateHook?.Disable();
        onCountDownChangeDelegateHook?.Dispose();
    }

    private void HandleCountDownChange(ulong value)
    {
        onCountDownChangeDelegateHook.Original(value);

        CountDownValue = Marshal.PtrToStructure<float>((IntPtr)value + 0x2c);

        // This is because the countdown continues well into the negatives 
        if (CountDownValue <= 0)
        {
            if (IsCountingDown)
            {
                CountDownStopped();   
            }

            return;
        }
        
        if (countDownNoLongerActiveTimer.Enabled)
        {
            countDownNoLongerActiveTimer.Stop();
            countDownNoLongerActiveTimer.Start();
        }
        else
        {
            IsCountingDown = true;
            countDownNoLongerActiveTimer.Start();
        }
    }
    
    // TODO: replace with a hook ffs
    private void CountDownStopped()
    {
        countDownNoLongerActiveTimer.Stop();
        IsCountingDown = false;
        
        OnCountDownStopped?.Invoke();
        PluginLog.Debug("countdown stopped");
    }
}
