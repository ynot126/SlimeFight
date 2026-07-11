#nullable enable
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterTypeIcon", menuName = "Slime Fight/Character Type Icon")]
public class CharacterTypeIcon : ScriptableObject
{
    [SerializeField] EnumDictionary<CharacterType, Sprite> icons = new();

    public Sprite? GetIcon(CharacterType characterType)
        => icons.TryGetValue(characterType, out var icon) ? icon : null;
}
