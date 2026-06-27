#nullable enable
using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer = null!;
    // character data
    CharacterType type;
    int runTimeId;
    int currentHealth;
    int maxHealth;
    int speed;
    
    // public field data
    public int RunTimeId => runTimeId;
    public int Speed => speed;
    
    // event function
    public event Action? OnDeath;
    
    public void Initialize(CharacterData characterData, int aRunTimeID)
    {
        maxHealth = characterData.maxHealth;
        currentHealth = maxHealth;
        speed = characterData.speed;
        runTimeId = aRunTimeID;
        
        type = characterData.type;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if(currentHealth <=0) Death();
    }
    
    void Death()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    public void SetCharacterReadyAction(bool val)
    {
        spriteRenderer.color = val ? Color.red : Color.white;
    }
}

public enum CharacterType
{
    Player =1,
    Enemy =2,
}
[Serializable]
public class CharacterData
{
    public CharacterType type;
    public int maxHealth = -1;
    public int speed = -1;
}