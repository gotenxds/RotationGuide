using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using RotationGuide.Utils;

namespace RotationGuide.Windows;

public class RotationWindow : Window, IDisposable
{
    private int max = 500;
    private int min = 0;
    private int current = 0;
    private int speed = 1;
    private bool goingUp = true;

    public RotationWindow(Plugin plugin) : base("Rotation runner", ImGuiWindowFlags.NoTitleBar)
    {
        Size = new Vector2(600, 100);
        SizeCondition = ImGuiCond.FirstUseEver;

        Position = new Vector2(1500, 295);
        PositionCondition = ImGuiCond.FirstUseEver;

        // ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0 ,0.6f));
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text($"The random config bool is");

        Animate();

        var imDrawListPtr = ImGui.GetWindowDrawList();

        // DrawUtils.DrawRectFilled(imDrawListPtr, new Vector2(100, 100), new Vector2(100, 25),
        //                          new Vector4(0.165f, 0.173f, 0.176f, 1f),
        //                          new Vector4(0.357f, 0.365f, 0.388f, 1f), 4);
        // DrawUtils.DrawLine(imDrawListPtr, new Vector2(150, 102), new Vector2(150, 126 - 3),
        //                    new Vector4(0.271f, 0.278f, 0.294f, 1), 3);
    }

    private void Animate()
    {
        var imDrawListPtr = ImGui.GetWindowDrawList();

        if (goingUp)
        {
            current += speed;

            if (current > max)
            {
                goingUp = false;
            }
        }
        else
        {
            current -= speed;

            if (current <= min)
            {
                goingUp = true;
            }
        }

        DrawUtils.DrawRectFilled(imDrawListPtr, new Vector2(100 + current, 100), new Vector2(100, 25),
                                 new Vector4(0.165f, 0.173f, 0.176f, 1f),
                                 new Vector4(0.357f, 0.365f, 0.388f, 1f), 4);
        DrawUtils.DrawLine(imDrawListPtr, new Vector2(current + 150, 102), new Vector2(current + 150, 126 - 3),
                           new Vector4(0.271f, 0.278f, 0.294f, 1), 3);
    }
}

