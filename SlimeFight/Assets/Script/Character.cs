#nullable enable
using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    // character data
    int currentHealth;
    int maxHealth;
    public void Initialize(CharacterData characterData)
    {
        maxHealth = characterData.maxHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if(currentHealth <=0) Death();
    }

    void Death()
    {
        Destroy(gameObject);
    }
}
[Serializable]
public class CharacterData
{
    public int maxHealth = -1;
}