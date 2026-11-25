using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject deathEffect;

    public float health = 2f; 

    public static int EnemiesAlive = 0;

    private void Start()
    {
        EnemiesAlive++;
    }

    private void OnCollisionEnter2D(Collision2D colInfo)
    {
        if (colInfo.relativeVelocity.magnitude > health)
        {
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (deathEffect != null)
        {
            var vfx = Instantiate(deathEffect, transform.position, Quaternion.identity);
            vfx.SetActive(true);
        }

        EnemiesAlive--;
        if (EnemiesAlive <= 0)
        {
            Debug.Log("Level won!");
        }

        Destroy(gameObject);
    }
}
