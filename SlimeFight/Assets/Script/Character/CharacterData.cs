using System;
using System.Collections.Generic;

[Serializable]
public class CharacterData
{
    // character stat
    public EntityStat stat;
    
    // action
    public List<string> actionIds = new();
}

[Serializable]
public class EntityStat
{
    public int vitality;
    public int might ;
    public int focus ;
    public int armour ;
    public int speed ;
    public int spirit;

    public EntityStat(int aVitality, int aMight, int aFocus, int aArmour, int aSpeed, int aSpirit)
    {
        vitality = aVitality;
        might = aMight;
        focus = aFocus;
        armour = aArmour;
        speed = aSpeed;
        spirit = aSpirit;
    }
}