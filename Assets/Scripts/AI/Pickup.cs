using System;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Behaviours;
using Action = Unity.AI.Behaviours.Action;

[Serializable]
[NodeDescription(name: "Pickup", description: "", story: "Pickup [Chest] / [position]")]
public class Pickup : Action
{
    public GameObject Chest;
    public Vector3 position;
    public override Status OnUpdate()
    {
        if (Chest && Chest.GetComponent<Chest>().m_Opened) {
            return Status.Success;
        }
        
        var bot = Agent.GetComponent<PlayerController>();
        if (bot.GoTo(Chest ? Chest.transform.position : position) == false) {
            return Status.Fail;
        }
        // NOT NEEDED opened is good enough
        if (bot.HasArrived()) {
            return Status.Success;
        }
        return Status.Running;
    }
}
