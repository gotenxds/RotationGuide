using System;
using System.Linq;
using Dalamud.Logging;
using RotationMaster.Data;

namespace RotationMaster.Services;

public class RotationRecorder
{
    private Rotation rotation;

    public void RecordInto(Rotation rotation)
    {
        this.rotation = rotation;
        PlayerListenerService.Instance!.OnActionUsed += OnActionUsed;
        CountDownService.Instance.OnCountDownStopped += OnOnCountDownStopped;
    }

    private void OnOnCountDownStopped()
    {
        if (!rotation.HasPullIndicator)
        {
            rotation.AddPullIndicator();
        }
    }

    public void Stop()
    {
        rotation = null!;
        PlayerListenerService.Instance!.OnActionUsed -= OnActionUsed;
        CountDownService.Instance.OnCountDownStopped -= OnOnCountDownStopped;
    }

    private void OnActionUsed(OnActionUsedEventParams eventParams)
    {
        var action = eventParams.Action;

        if (action == null)
        {
            return;
        }

        var type = eventParams.Action!.GetActionType();

        if (type is RotationMasterActionType.NA or RotationMasterActionType.AutoAttack)
        {
            return;
        }

        if (!rotation.HasPullIndicator && CountDownService.Instance!.IsCountingDown)
        {
            rotation.AddAction(new PrePullActionNode() { Id = action.RowId, Time = (int)Math.Round(CountDownService.Instance.CountDownValue)});
            return;
        }

        var rotationNodes = rotation.Nodes;

        switch (type)
        {
            case RotationMasterActionType.GCD:
                rotation.AddAction(new GCDActionNode { Id = action.RowId });
                break;
            case RotationMasterActionType.OGCD:
            {
                var isEmpty = rotationNodes.Length == 0;

                if (isEmpty)
                {
                    var ogcdActionsNode = new OGCDActionsNode();
                    ogcdActionsNode.Ids[0] = action.RowId;

                    rotation.InsertNode(ogcdActionsNode, rotationNodes.Length);
                }
                else if (rotationNodes.Last() is GCDActionNode)
                {
                    var ogcdActionsNode = new OGCDActionsNode();
                    ogcdActionsNode.Ids[0] = action.RowId;

                    rotation.InsertNode(ogcdActionsNode, rotationNodes.Length);
                }
                else if (rotationNodes.Last() is OGCDActionsNode ogcdActionsNode)
                {
                    var emptyId = ogcdActionsNode.Ids.ToList().FindIndex(i => !FFAction.Exists(i));

                    if (emptyId != -1)
                    {
                        ogcdActionsNode.Ids[emptyId] = action.RowId;
                    }
                    else
                    {
                        rotation.AddAction(new GCDActionNode { Id = action.RowId });
                    }
                }

                break;
            }
        }
    }
}
