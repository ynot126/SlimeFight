#nullable enable
using Cysharp.Threading.Tasks;

public class MoveEffect : IActionExecution
{
    public UniTask ExecuteAsync(CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId, ActionTarget target)
    {
        return target.HasHex
            ? characterManager.CharacterMoveToHex(activeCharacterRunTimeId, target.Hex)
            : characterManager.CharacterMoveToPosition(activeCharacterRunTimeId, target.Position);
    }
}
