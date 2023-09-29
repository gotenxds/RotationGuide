using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace RotationMaster.Data;

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

    public void InsertNode(IRotationNode node, int index)
    {
        var pullIndicatorIndex = HasPullIndicator ? nodes.FindIndex(n => n is PullIndicatorNode) : -1;

        switch (node)
        {
            case PrePullActionNode when HasPullIndicator && index > pullIndicatorIndex:
                throw new ArgumentException("PrePull nodes needs to be before pull indicators");
            case PrePullActionNode:
                nodes.Insert(index, node);
                break;
            case PullIndicatorNode when HasPullIndicator:
                throw new ArgumentException("Already have pull indicator");
            case PullIndicatorNode when nodes.FindLastIndex(n => n is PrePullActionNode) > index:
                throw new ArgumentException("Cant put pull indicator after pre pull action");
            case PullIndicatorNode:
                nodes.Insert(index, node);
                HasPullIndicator = true;
                break;
            case OGCDActionsNode:
                nodes.Insert(index, node);
                break;
            case GCDActionNode actionNode:
                if (index == nodes.Count)
                {
                    AddAction(actionNode);
                }
                else if (nodes[index] is GCDActionNode)
                {
                    nodes.InsertRange(index, new[] { node, new OGCDActionsNode() });
                }
                else if (index != 0 && nodes[index - 1] is GCDActionNode)
                {
                    nodes.InsertRange(index, new[] { new OGCDActionsNode(), node });
                }
                else
                {
                    nodes.Insert(index, node);
                }

                break;
        }

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

    public void RemoveNode(int index)
    {
        var node = nodes[index];
        switch (node)
        {
            case OGCDActionsNode:
                throw new ArgumentException("Cant remove OGCDActionsNode");
            case GCDActionNode when index != (nodes.Count - 1) && nodes[index + 1] is OGCDActionsNode:
                throw new ArgumentException("Cant remove GCDActionNode that has an OGCDActionsNode after it");
            case GCDActionNode:
            {
                nodes.RemoveAt(index);

                if (index != 0 && nodes[index - 1] is OGCDActionsNode)
                {
                    nodes.RemoveAt(index - 1);
                }

                break;
            }
            case PullIndicatorNode:
                if (index != 0 && nodes[index - 1] is GCDActionNode && index != nodes.Count - 1 &&
                    nodes[index + 1] is GCDActionNode)
                {
                    nodes.RemoveAt(index);
                    nodes.Insert(index, new OGCDActionsNode());
                }
                else
                {
                    nodes.RemoveAt(index);
                }

                HasPullIndicator = false;
                break;
            default:
            {
                nodes.RemoveAt(index);
                break;
            }
        }

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
    public uint[] Ids { get; set; } = { uint.MaxValue, uint.MaxValue, uint.MaxValue };

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
        set => time = Math.Clamp(Math.Abs(value) * -1, -120, 0);
    }
}

public struct PullIndicatorNode : IRotationNode { }
