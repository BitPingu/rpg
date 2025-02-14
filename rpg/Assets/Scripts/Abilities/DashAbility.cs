using UnityEngine;

[CreateAssetMenu]
public class DashAbility : IAbility
{
    public float dashVelocity;

    public override void Activate(GameObject parent)
    {
        base.Activate(parent);
        
        Player player =  parent.GetComponent<Player>();
        player.MoveSpeed = dashVelocity;

        Debug.Log(name + " activated.");
    }

    public override void BeginCooldown(GameObject parent)
    {
        base.BeginCooldown(parent);

        Player player =  parent.GetComponent<Player>();
        player.MoveSpeed = player.MaxSpeed;
        // ability finished
        Debug.Log(name + " on cooldown.");
    }
}

