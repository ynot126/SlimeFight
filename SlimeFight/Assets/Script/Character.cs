#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] Transform statusCanvasContainer = null!;
    [SerializeField] CharacterStatusCanvas characterStatusCanvasPrefab = null!;
    [SerializeField] SpriteRenderer spriteRenderer = null!;
    // character data
    CharacterType type;
    int runTimeId;
    int currentHealth;
    int maxHealth;
    int currentMana;
    int maxMana;
    int speed;
    int attackPower;
    List<CharacterActionType> actions = new();
    
    // public field data
    public int RunTimeId => runTimeId;
    public int Speed => speed;
    public CharacterType Type => type;
    public int AttackPower => attackPower;
    public Vector2 Position => transform.position;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentMana => currentMana;
    public int MaxMana => maxMana;
    public IReadOnlyList<CharacterActionType> Actions => actions;
    
    // event function
    public event Action? OnDeath;
    
    // UI
    CharacterStatusCanvas characterStatusCanvas = null!;
    
    public void Initialize(CharacterData characterData, int aRunTimeID)
    {
        maxHealth = characterData.maxHealth;
        currentHealth = maxHealth;
        maxMana = characterData.maxMana;
        currentMana = maxMana;
        speed = characterData.speed;
        attackPower = characterData.attackPower;
        runTimeId = aRunTimeID;
        type = characterData.type;
        actions = new List<CharacterActionType>(characterData.actions);
        
        characterStatusCanvas = Instantiate(characterStatusCanvasPrefab , statusCanvasContainer);
        characterStatusCanvas.Initialize(this);
        characterStatusCanvas.SetVisible(false);
    }

    public void RefillMana() => currentMana = maxMana;

    public bool CanAffordMana(int cost) => currentMana >= cost;

    public bool TrySpendMana(int cost)
    {
        if (!CanAffordMana(cost)) return false;
        currentMana -= cost;
        return true;
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

    public void SetStatusCanvasVisible(bool val)
    {
        characterStatusCanvas.SetVisible(val);
        if (val) characterStatusCanvas.UpdateStatus();
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
    public int attackPower = 1;
    public int maxMana = -1;
    public List<CharacterActionType> actions = new();
}