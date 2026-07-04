using System;
using System.Collections.Generic;

[Serializable]
public class CharacterData
{
    public CharacterType type;

    // character stat
    public int vitality = 5;
    public int might = 5;
    public int focus = 5;
    public int armour = 0;
    public int speed = 5;
    public int spirit = 5;
    
    // action
    public List<string> actionIds = new();
}