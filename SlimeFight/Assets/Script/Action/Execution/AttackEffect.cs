#nullable enable
using Cysharp.Threading.Tasks;

public class AttackEffect : IActionExecution
{
    readonly int damage;

    public AttackEffect(int damage)
    {
        this.damage = damage;
    }

    public async UniTask ExecuteAsync(ActionContext ctx, ActionTarget target)
    {
        if (target.TargetCharacterRunTimeId == 0) return;
        ctx.CharacterManager.DealDamage(target.TargetCharacterRunTimeId, damage);
        await UniTask.Yield();
    }
}