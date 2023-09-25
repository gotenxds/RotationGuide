using System.IO;
using Dalamud.Plugin;
using ImGuiScene;

namespace RotationMaster.Utils;

public static class Images
{
    public static TextureWrap PullBarImage = null!;
    public static TextureWrap ActionIconBorderImage = null!;
    public static TextureWrap OGCDBarImage = null!;

    public static void Init(DalamudPluginInterface pluginInterface)
    {
        var directoryFullName = pluginInterface.AssemblyLocation.Directory?.FullName!;
        
        var pullBarImagePath = Path.Combine(directoryFullName, "pullBar.png");
        PullBarImage = pluginInterface.UiBuilder.LoadImage(pullBarImagePath);
        
        var actionIconBorderImagePath = Path.Combine(directoryFullName, "actionIconBorder.png");
        ActionIconBorderImage = pluginInterface.UiBuilder.LoadImage(actionIconBorderImagePath);
        
        var ogcdBarImage = Path.Combine(directoryFullName, "ogcdBar.png");
        OGCDBarImage = pluginInterface.UiBuilder.LoadImage(ogcdBarImage);
    }
}
