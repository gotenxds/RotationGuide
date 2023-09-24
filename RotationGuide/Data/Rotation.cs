using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace RotationGuide.Data;

[Serializable]
public class Rotation
{
    private string id;
    private uint jobId;
    private string name;
    public string Patch;

    private List<IRotationNode> nodes;

    [JsonInclude]
    public bool HasPullIndicator { get; private set; }

    [JsonInclude]
    public IRotationNode[] Nodes
    {
        get => nodes.ToArray();
        private set => nodes = new List<IRotationNode>(value);
    }

    public string Id => id;
    public uint JobId => jobId;

    public string Name
    {
        get => name;
        set
        {
            name = value;
            OnRotationChanged?.Invoke(this);
        }
    }

    public event Action<Rotation> OnRotationChanged;

    public Rotation(uint jobId)
    {
        this.jobId = jobId;
        id = Guid.NewGuid().ToString();
        Name = "";
        Patch = "";
        nodes = new List<IRotationNode>();
        HasPullIndicator = false;
    }

    public void AddPullIndicator()
    {
        if (HasPullIndicator)
        {
            throw new ArgumentException("Cant add another pull indicator");
        }

        nodes.Add(new PullIndicatorNode());
        HasPullIndicator = true;

        OnRotationChanged(this);
    }

    public void AddAction(IActionNode actionNode)
    {
        if (actionNode is PrePullActionNode && HasPullIndicator)
        {
            throw new ArgumentException("Cant add prepull after pull indicator");
        }

        if (nodes.Count > 0 && nodes.Last() is GCDActionNode && actionNode is GCDActionNode)
        {
            nodes.Add(new OGCDActionsNode());
        }

        nodes.Add(actionNode);
        OnRotationChanged(this);
    }

    public void ReplaceActionNode(int index, IActionNode actionNode)
    {
        if (nodes[index].GetType() != actionNode.GetType())
        {
            throw new AggregateException("Action must replace an action of the same type");
        }

        nodes[index] = actionNode;
        OnRotationChanged(this);
    }

    public void UpdateOgcdNode(int index, int innerIndex, uint id)
    {
        var rotationNode = Nodes[index];

        if (rotationNode is OGCDActionsNode actionsNode)
        {
            actionsNode.Ids[innerIndex] = id;
            OnRotationChanged(this);
        }
        else
        {
            throw new ArgumentException("Trying to update a non ogcd node");
        }
    }

    public void RemoveActionNode(int index)
    {
        nodes.RemoveAt(index);
        OnRotationChanged(this);
    }
}

[JsonDerivedType(typeof(GCDActionNode), typeDiscriminator: "GCD")]
[JsonDerivedType(typeof(OGCDActionsNode), typeDiscriminator: "OGCD")]
[JsonDerivedType(typeof(PrePullActionNode), typeDiscriminator: "PrePull")]
[JsonDerivedType(typeof(PullIndicatorNode), typeDiscriminator: "PullIndicator")]
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

public struct OGCDActionsNode : IRotationNode
{
    private uint[] ids = { uint.MaxValue, uint.MaxValue, uint.MaxValue };

    [JsonInclude]
    public uint[] Ids
    {
        get => ids;
        private set => ids = value;
    }

    public OGCDActionsNode() { }
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
