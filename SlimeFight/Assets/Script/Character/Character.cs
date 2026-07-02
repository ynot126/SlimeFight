#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer = null!;
    [SerializeField] SpriteRenderer selectedSpriteRenderer = null!;
    [SerializeField] CharacterActionRangeIndicator actionRangeIndicator = null!;
    // character data
    CharacterType type;
    int runTimeId;
    int currentHealth;
    int maxHealth;
    int currentMana;
    int maxMana;
    int speed;
    int attackPower;
    List<string> actions = new();
    
    // public field data
    public int RunTimeId => runTimeId;
    public int Speed => speed;
    public CharacterType Type => type;
    public int AttackPower => attackPower;
    public Vector3 Position => transform.position;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentMana => currentMana;
    public int MaxMana => maxMana;
    public IReadOnlyList<string> Actions => actions;
    
    // event function
    public event Action? OnDeath;

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
        actions = new List<string>(characterData.actionIds);

        SetCharacterReadyAction(false);
        actionRangeIndicator.SetVisible(false);
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
        selectedSpriteRenderer.gameObject.SetActive(val);
    }

    public void SetActionRangeIndicator(float range, bool visible)
    {
        if (!visible)
        {
            actionRangeIndicator.SetVisible(false);
            return;
        }

        actionRangeIndicator.SetRange(range);
        actionRangeIndicator.SetVisible(true);
    }

}

public enum CharacterType
{
    Player =1,
    Enemy =2,
}