#nullable enable

public readonly struct ActionContext
{
    readonly CharacterManager characterManager;
    readonly MapManager mapManager;
    readonly int activeCharacterRunTimeId;

    public CharacterManager CharacterManager => characterManager;
    public MapManager MapManager => mapManager;
    public int ActiveCharacterRunTimeId => activeCharacterRunTimeId;

    public ActionContext(CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId)
    {
        this.characterManager = characterManager;
        this.mapManager = mapManager;
        this.activeCharacterRunTimeId = activeCharacterRunTimeId;
    }
}
