using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] protected int health;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int attack;
    [SerializeField] protected int defence;
    [SerializeField] int maxStats = 20;

    [SerializeField] protected bool isDead;

    private int rand;

    public virtual void CheckHealth()
    {
        if(health <= 0)
        {
            health = 0;
            Die();
        }
        if (health >= maxHealth)
        {
            health = maxHealth;
        }
    }

    public virtual void Die()
    {
        isDead = true;
        Destroy(gameObject);
    }

    private void SetHealthTo(int healthToSetTo)
    {
        health = healthToSetTo;
        CheckHealth();
    }

    public void TakeDamage(int damage)
    {
        int healthAfterDamage = health - damage;
        SetHealthTo(healthAfterDamage);

    }

    public void Heal(int heal)
    {
        int healthAfterHeal = health + heal;
        SetHealthTo(healthAfterHeal);
    }

    public void InitVariables()
    {
        rand = Random.Range(0, 3);
        for (int x = 0; x < maxStats; x++)
        {
            rand = Random.Range(0, 3);

            if (rand == 0)
            {
                maxHealth += 1;
            }
            else if (rand == 1)
            {
                attack += 1;
            }
            else
            {
                defence += 1;
            }

        }
        SetHealthTo(maxHealth);
        isDead = false;
    }
}