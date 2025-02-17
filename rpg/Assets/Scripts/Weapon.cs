using UnityEngine;

public class Weapon : MonoBehaviour
{
    private Player _player;
    public float Power = 5.0f;

    private void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
    }
 
    private void OnTriggerEnter(Collider hitInfo)
    {
        if (_player != null && _player.BattleState.IsAttacking)
        {
            // hit opponent
            Enemy opponent = hitInfo.GetComponent<Enemy>();
            if (opponent != null)
            {
                // deal damage
                float damage = _player.Strength + Power - opponent.Defence;
                damage = Mathf.Floor(damage);
                opponent.Damage(damage);
                Debug.Log("hit " + opponent.name + " for " + damage + " damage.");
            }
        }
    }
}
