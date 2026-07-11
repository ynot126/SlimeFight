#nullable enable
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Entity : MonoBehaviour
{
    const float attackAnimationScale = 1.2f;
    const float attackAnimationDuration = 0.3f;
    const float damageAnimationDuration = 0.3f;

    [SerializeField] SpriteRenderer spriteRenderer = null!;
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

    public void Initialize(EntityStat stat, EntityType aType, int aRunTimeId)
    {
        maxHealth = stat.vitality *10;
        currentHealth = maxHealth;
        maxMana = stat.spirit;
        currentMana = maxMana;
        speed = stat.speed;
        runTimeId = aRunTimeId;
        type = aType;

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

    public async UniTask AttackAnimation()
    {
        var spriteTransform = spriteRenderer.transform;
        var originalScale = spriteTransform.localScale;
        var targetScale = originalScale * attackAnimationScale;
        await spriteTransform.DOScale(targetScale, attackAnimationDuration * 0.5f).SetLoops(2, LoopType.Yoyo).ToUniTask();
        spriteTransform.localScale = originalScale;
    }

    public async UniTask DamageAnimation()
    {
        var originalColor = spriteRenderer.color;
        await spriteRenderer.DOColor(Color.red, damageAnimationDuration * 0.5f).SetLoops(2, LoopType.Yoyo).ToUniTask();
        spriteRenderer.color = originalColor;
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
}

public enum EntityType
{
    Player =1,
    Enemy =2,
}