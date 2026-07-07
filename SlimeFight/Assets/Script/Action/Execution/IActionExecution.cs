#nullable enable
using Cysharp.Threading.Tasks;

public interface IActionExecution
{
    UniTask ExecuteAsync(CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId, ActionTarget target);
}
