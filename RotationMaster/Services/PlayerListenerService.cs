using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using RotationMaster.Data;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RotationMaster.Services;

public struct OnActionUsedEventParams
{
    public uint ActionId;

    public Action? Action => FFAction.TryById(ActionId, out var action) ? action : null;
}

public class PlayerListenerService : IDisposable
{
    private const string ActionUsedSignature =
        "40 55 53 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 70";

    private delegate void OnActionUsedDelegate(
        uint sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail);

    private readonly Hook<OnActionUsedDelegate>? onActionUsedHook;
    private IClientState clientState;

    public event Action<OnActionUsedEventParams> OnActionUsed = null!;

    public static PlayerListenerService? Instance { get; private set; }

    private PlayerListenerService(ISigScanner scanner, IClientState clientState)
    {
        this.clientState = clientState;

        var actionUsedFuncPtr = scanner.ScanText(
            ActionUsedSignature);

        onActionUsedHook = Hook<OnActionUsedDelegate>.FromAddress(actionUsedFuncPtr, TryFireOnActionUsed);
        onActionUsedHook.Enable();
    }

    public static void Init(ISigScanner scanner, IClientState clientState)
    {
        if (Instance == null)
        {
            Instance = new PlayerListenerService(scanner, clientState);
        }
    }

    private void TryFireOnActionUsed(
        uint sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail)
    {
        PropagateActionUsed(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail);

        if (OnActionUsed == null)
        {
            return;
        }

        var player = clientState.LocalPlayer;
        if (player == null || sourceId != player.ObjectId)
        {
            return;
        }

        var actionId = (uint)Marshal.ReadInt32(effectHeader, 0x8);

        OnActionUsed.Invoke(new OnActionUsedEventParams { ActionId = actionId });
    }

    private void PropagateActionUsed(
        uint sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail)
    {
        onActionUsedHook.Original(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail);
    }

    public void Dispose()
    {
        onActionUsedHook?.Dispose();
    }
}
