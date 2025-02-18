using UnityEngine;
using System.Collections;

public class Slime : Enemy
{
    public void LungeAttack()
    {
        StartCoroutine(LungeRaycast());
    }

    IEnumerator LungeRaycast()
    {
        yield return new WaitForSeconds(.4f);

        // Animate attack
        Anim.SetTrigger("Lunge");

        BattleState.Lunge.IsActive = true;

        yield return new WaitForSeconds(1f);
        
        BattleState.Lunge.IsActive = false;

        // Animate attack
        Anim.SetTrigger("Lunge");

        yield return new WaitForSeconds(.4f);
    }

    private void OnTriggerEnter(Collider hitInfo)
    {
        if (BattleState.Lunge.IsActive)
        {
            // hit opponent
            Player opponent = hitInfo.GetComponent<Player>();
            if (opponent != null)
            {
                // deal damage
                float damage = Strength + BattleState.Lunge.Power - opponent.Defence;
                damage = Mathf.Floor(damage);
                opponent.Damage(damage);
                Debug.Log(BattleState.Lunge.name + "hit " + opponent.name + " for " + damage + " damage.");
            }
        }
    }

}

