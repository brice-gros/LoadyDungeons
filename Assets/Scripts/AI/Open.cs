using System;
using UnityEngine;
using Unity.AI.Behaviours;
using Action = Unity.AI.Behaviours.Action;

[Serializable]
[NodeDescription(name: "Open", description: "", story: "Open [door]")]
public class Open : Action
{
    public GameObject Door;
    public override Status OnUpdate()
    {
        var player = Agent.GetComponent<PlayerController>();
        if (!player.m_HasKey) {
            Debug.Log("Open fail");
            return Status.Fail;
        }
        if((Agent.transform.position - Door.transform.position).sqrMagnitude < 4.0F) {
            Debug.Log("Open success");
            return Status.Success;
        }
        Debug.Log("Open running");
        return Status.Running;
    }
}
