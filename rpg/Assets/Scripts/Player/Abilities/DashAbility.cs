using UnityEngine;

[CreateAssetMenu]
public class DashAbility : IAbility
{
    public float movementSpeedMultiplier;
    public float attackSpeedMultiplier;

    public override void Activate(GameObject parent)
    {
        base.Activate(parent);

        Player player =  parent.GetComponent<Player>();
        player.Vol.weight = 1f;
        player.MoveSpeed *= movementSpeedMultiplier;
        player.AttackSpeed *= attackSpeedMultiplier;

        Debug.Log(name + " activated.");
    }

    public override void BeginCooldown(GameObject parent)
    {
        base.BeginCooldown(parent);

        Player player =  parent.GetComponent<Player>();
        player.Vol.weight = 0f;
        player.MoveSpeed = player.MaxSpeed;
        player.AttackSpeed = player.MaxAttackSpeed;
    
        // ability finished
        Debug.Log(name + " on cooldown.");
    }
}

