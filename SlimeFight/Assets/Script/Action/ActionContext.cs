#nullable enable

public readonly struct ActionContext
{
    public CharacterManager CharacterManager { get; }
    public MapManager MapManager { get; }
    public int ActiveCharacterRunTimeId { get; }

    public ActionContext(CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId)
    {
        CharacterManager = characterManager;
        MapManager = mapManager;
        ActiveCharacterRunTimeId = activeCharacterRunTimeId;
    }
}
