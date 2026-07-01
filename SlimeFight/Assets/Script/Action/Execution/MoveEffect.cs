#nullable enable
using Cysharp.Threading.Tasks;

public class MoveEffect : IActionExecution
{
    public UniTask ExecuteAsync(ActionContext ctx, ActionTarget target)
        => ctx.CharacterManager.CharacterMoveToPosition(ctx.ActiveCharacterRunTimeId, target.Position);
}
