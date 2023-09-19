using ImGuiNET;
using RotationGuide.Utils;

namespace RotationGuide.Windows;

public abstract class Renderer
{
    protected float BaseCursorHeight;
    public abstract void Render(Transition transition = Transition.None, float time = 0);

    protected static float CalculateCursorHeight(Transition transition, float time = 0, float startPosOffset = 0)
    {
        switch (transition)
        {
            case Transition.Out:
            {
                var dest = ImGui.GetWindowPos().Y;
                return dest * (time) * -1;
            }
            case Transition.In:
            {
                var startPos = ImGui.GetWindowHeight() - startPosOffset;

                return startPos * (1 - time);
            }
            case Transition.None:
            default:
                return 0;
        }
    }
    
    protected void StyleTransitionEnd(Transition transition)
    {
        if (transition is Transition.Out or Transition.In)
        {
            ImGui.PopStyleVar();
        }
    }

    protected void StyleTransitionBegin(Transition transition, float time)
    {
        if (transition == Transition.None)
        {
            BaseCursorHeight = 0;
            return;
        }
        
        BaseCursorHeight = CalculateCursorHeight(transition, time);

        var timeEasing = AnimationEasings.EaseInQuart(time); 
        
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, transition == Transition.In ? timeEasing : 1 - timeEasing);
    }
}
