#nullable enable
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CharacterVisual : MonoBehaviour
{
    const float attackAnimationScale = 1.2f;
    const float attackAnimationDuration = 0.3f;
    const float damageAnimationDuration = 0.3f;

    [SerializeField] CharacterTypeIcon characterTypeIcon = null!;
    [SerializeField] SpriteRenderer spriteRenderer = null!;
    [SerializeField] SpriteRenderer characterTypeSpriteRenderer = null!;

    public void Initialize(CharacterType characterType)
        => characterTypeSpriteRenderer.sprite = characterTypeIcon.GetIcon(characterType);

    public async UniTask AttackAnimation()
    {
        var spriteTransform = spriteRenderer.transform;
        var originalScale = spriteTransform.localScale;
        var targetScale = originalScale * attackAnimationScale;
        await spriteTransform.DOScale(targetScale, attackAnimationDuration * 0.5f)
            .SetLoops(2, LoopType.Yoyo)
            .ToUniTask();
        spriteTransform.localScale = originalScale;
    }

    public async UniTask DamageAnimation()
    {
        var originalColor = spriteRenderer.color;
        await spriteRenderer.DOColor(Color.red, damageAnimationDuration * 0.5f)
            .SetLoops(2, LoopType.Yoyo)
            .ToUniTask();
        spriteRenderer.color = originalColor;
    }
}
