using UnityEngine;

/// <summary>
/// Bullet — destroys itself on collision and deals damage to zombies.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float damage = 25f;

    void OnCollisionEnter(Collision col)
    {
        col.gameObject.GetComponent<ZombieAI>()?.TakeDamage(damage);
        Destroy(gameObject);
    }
}
