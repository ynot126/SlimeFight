using System;
using System.Collections.Generic;

[Serializable]
public class CharacterData
{
    public CharacterType type;
    public int maxHealth = -1;
    public int speed = -1;
    public int attackPower = 1;
    public int maxMana = -1;
    public List<string> actionIds = new();
}