using UnityEngine;

[CreateAssetMenu]
public class LungeAbility : IAbility
{
    public float Power = 5f;
    public float movementSpeedMultiplier;
    public Vector3 TargetPos;
    public bool IsActive;

    public override void Activate(GameObject parent)
    {
        base.Activate(parent);

        Slime slime =  parent.GetComponent<Slime>();
        slime.MoveSpeed *= movementSpeedMultiplier;
        TargetPos = slime.BattleState.CurrentOpponent.transform.position - slime.transform.position;
        slime.LungeAttack();

        // Debug.Log(name + " activated.");
    }

    public override void BeginCooldown(GameObject parent)
    {
        base.BeginCooldown(parent);

        Slime slime =  parent.GetComponent<Slime>();
        slime.MoveSpeed = slime.MaxSpeed;
    
        // ability finished
        // Debug.Log(name + " on cooldown.");
    }

    public bool Condition(GameObject parent)
    {
        Enemy enemy = parent.GetComponent<Enemy>();

        return !enemy.BattleState.OpponentInRange();
    }
}

