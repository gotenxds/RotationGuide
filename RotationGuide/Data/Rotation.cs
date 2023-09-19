using System;
using System.Collections.Generic;

namespace RotationGuide.Data;

public class Rotation
{
    public string Name;
    public string Patch;
    public List<IRotationNode> nodes;
    public bool HasPullIndicator { get; private set; }

    public IRotationNode[] Nodes => nodes.ToArray();

    public Rotation()
    {
        Reset();
    }

    public void Reset()
    {
        Name = "";
        Patch = "";
        nodes = new List<IRotationNode>();
    }

    public void AddPullIndicator()
    {
        if (HasPullIndicator)
        {
            throw new ArgumentException("Cant add another pull indicator");
        }

        nodes.Add(new PullIndicatorNode());
        HasPullIndicator = true;
    }

    public void AddAction(IActionNode actionNode)
    {
        if (actionNode is PrePullActionNode && HasPullIndicator)
        {
            throw new ArgumentException("Cant add prepull after pull indicator");
        }

        nodes.Add(actionNode);
    }

    public void ReplaceActionNode(int index, IActionNode actionNode)
    {
        if (nodes[index].GetType() != actionNode.GetType())
        {
            throw new AggregateException("Action must replace an action of the same type");
        }

        nodes[index] = actionNode;
    }

    public void RemoveActionNode(int index)
    {
        nodes.RemoveAt(index);
    }

}

public interface IRotationNode { }

public interface IActionNode : IRotationNode
{
    public uint Id { get; set; }
}

public struct GCDActionNode : IActionNode
{
    private uint id;

    public uint Id
    {
        get => id;
        set => id = value;
    }
}

public struct OGCDActionNode : IActionNode
{
    private uint id;

    public uint Id
    {
        get => id;
        set => id = value;
    }
}

public struct PrePullActionNode : IActionNode
{
    private uint id;
    private int time;

    public uint Id
    {
        get => id;
        set => id = value;
    }

    public int Time
    {
        get => time;
        set => time = value;
    }
}

public struct PullIndicatorNode : IRotationNode { }
