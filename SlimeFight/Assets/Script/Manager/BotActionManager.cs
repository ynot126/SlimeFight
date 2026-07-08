#nullable enable

using Cysharp.Threading.Tasks;

public class BotActionManager : BaseCharacterActionManager
{
    readonly BotActionPlanner botActionPlanner = new();

    protected override void BeginTurn(int runTimeId, GameView gameView)
    {
        base.BeginTurn(runTimeId, gameView);

        if (!CharacterManager.TryGetBotData(runTimeId, out var botData))
        {
            RequestEndTurn();
            return;
        }

        botActionPlanner.Initialize(
            botData,
            CharacterManager,
            MapManager,
            runTimeId,
            AvailableActions);
    }

    protected override UniTask<CharacterAction?> GetNextAction()
    {
        return IsEndTurnRequested
            ? UniTask.FromResult<CharacterAction?>(null)
            : botActionPlanner.GetNextAction();
    }
}
