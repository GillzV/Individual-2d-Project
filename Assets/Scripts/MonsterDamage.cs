using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDamage : MonoBehaviour
{
    public int damage = 10;
    public float damageInterval = .5f;   // 1.0s (use 0.5f for half-second)

    private float nextDamageTime = 0f;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (Time.time >= nextDamageTime)
        {
            var player = collision.gameObject.GetComponent<PlayerController2D>();
            if (player != null) player.TakeDamage(damage);

            nextDamageTime = Time.time + damageInterval;
        }
    }
}
