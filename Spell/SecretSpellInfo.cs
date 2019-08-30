using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SecretSpellType
{
    SecretSpells_0,
    SecretSpells_1,
    SecretSpells_2,
    SecretSpells_3,
    SecretSpells_4,
    SecretSpells_5,
    SecretSpells_6,
    SecretSpells_7,
    SecretSpells_8,
    SecretSpells_9,
}

public class SecretSpellInfo {

    public SecretSpellType secretType;
    public string title;
    public string info;

    public SecretSpellInfo(SecretSpellType secretType, string title, string info)
    {
        this.secretType = secretType;
        this.title = title;
        this.info = info;
    }
    public SecretSpellInfo() { }
}
