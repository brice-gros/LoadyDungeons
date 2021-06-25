using System;
using UnityEngine;
using Unity.AI.Behaviours;
using Action = Unity.AI.Behaviours.Action;

[Serializable]
[NodeDescription(name: "Open", description: "", story: "Open [Door] / [position]")]
public class Open : Action
{
    public GameObject Door;
    public Vector3 position;
    public override Status OnUpdate()
    {
        var player = Agent.GetComponent<PlayerController>();
        if (!player.m_HasKey) {
            return Status.Fail;
        }

        if (Door && Door.GetComponent<Door>().m_Opened) {
            return Status.Success;
        }

        var bot = Agent.GetComponent<PlayerController>();
        if (bot.GoTo(Door ? Door.transform.position : position) == false) {
            return Status.Fail;
        }
        // NOT NEEDED opened is good enough
        if (bot.HasArrived()) {
            return Status.Success;
        }

        return Status.Running;
    }
}
