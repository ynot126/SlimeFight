#nullable enable

public abstract class BaseTargetSelectStrategy
{
    protected CharacterManager characterManager = null!;
    protected MapManager mapManager = null!;
    protected InputManager inputManager = null!;
    protected int characterRunTimeId = -1;

    protected CharacterManager CharacterManager => characterManager;
    protected MapManager MapManager => mapManager;
    protected InputManager InputManager => inputManager;
    protected int CharacterRunTimeId => characterRunTimeId;

    public virtual void Initialize(
        CharacterManager aCharacterManager,
        MapManager aMapManager,
        InputManager aInputManager,
        int aCharacterRunTimeId)
    {
        characterManager = aCharacterManager;
        mapManager = aMapManager;
        inputManager = aInputManager;
        characterRunTimeId = aCharacterRunTimeId;
    }

    public virtual void ShowTargetPreview()
    {
        mapManager.ClearRange();
    }
}
