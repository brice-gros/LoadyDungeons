using System;
using UnityEngine;
using Unity.AI.Behaviours;
using Action = Unity.AI.Behaviours.Action;

[Serializable]
[NodeDescription(name: "Pickup", description: "", story: "Pickup [Chest]")]
public class Pickup : Action
{
    public GameObject Chest;
    public override Status OnUpdate()
    {
        if((Agent.transform.position - Chest.transform.position).sqrMagnitude < 4.0F) {
            Debug.Log("Pickup success");
            return Status.Success;
        }
        return Status.Running;
    }
}
