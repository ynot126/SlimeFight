#nullable enable
using Cysharp.Threading.Tasks;

public class AttackEffect : IActionExecution
{
    readonly int damage;

    public AttackEffect(int damage)
    {
        this.damage = damage;
    }

    public async UniTask ExecuteAsync(CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId, ActionTarget target)
    {
        if (target.TargetCharacterRunTimeId <= 0) return;
        characterManager.DealDamage(target.TargetCharacterRunTimeId, damage);
        await UniTask.Yield();
    }
}
