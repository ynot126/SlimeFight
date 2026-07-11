#nullable enable
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] CharacterVisual characterVisual = null!;
    [SerializeField] SpriteRenderer selectedSpriteRenderer = null!;
    
    // character data
    EntityType type;
    int runTimeId;
    int currentHealth;
    int maxHealth;
    int currentMana;
    int maxMana;
    int speed;
    
    // public field data
    public int RunTimeId => runTimeId;
    public int Speed => speed;
    public EntityType Type => type;
    public Vector3 Position => transform.position;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentMana => currentMana;
    public int MaxMana => maxMana;
    
    // event function
    public event Action? OnDeath;

    public void Initialize(EntityStat stat, EntityType aType, CharacterType characterType, int aRunTimeId)
    {
        maxHealth = stat.vitality * 10;
        currentHealth = maxHealth;
        maxMana = stat.spirit;
        currentMana = maxMana;
        speed = stat.speed;
        runTimeId = aRunTimeId;
        type = aType;

        characterVisual.Initialize(characterType);
        SetCharacterReadyAction(false);
    }

    public void RefillMana() => currentMana = maxMana;

    public bool CanAffordMana(int cost) => currentMana >= cost;

    public bool TrySpendMana(int cost)
    {
        if (!CanAffordMana(cost)) return false;
        currentMana -= cost;
        return true;
    }

    public UniTask AttackAnimation() => characterVisual.AttackAnimation();

    public UniTask DamageAnimation() => characterVisual.DamageAnimation();

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
        => selectedSpriteRenderer.gameObject.SetActive(val);
}

public enum EntityType
{
    Player =1,
    Enemy =2,
}