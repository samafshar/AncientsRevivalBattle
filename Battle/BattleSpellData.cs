using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BattleSpellBuffNerfType
{
    None,
    Fear,
    Taunt,
    Confuse,
    Burn,
    Poison,
    DamageReduction,
    Protect,
}

public class BattleSpellData : MonoBehaviour
{
    [SerializeField]
    private BattleSpellBuffNerfType _buffNerfType;
    [SerializeField]
    private List<BattleSpellValueDetail> _spellValue;

    //Props
    public BattleSpellBuffNerfType BuffNerfType
    {
        get { return _buffNerfType; }
    }
    public List<BattleSpellValueDetail> SpellValue
    {
        get { return _spellValue; }
    }

    public void ResetDataValue(CharInfo charInfo, int charSpellIndex, int currentPartyAP)
    {
        foreach (var spVal in SpellValue)
            spVal.Reset(charInfo, charSpellIndex, currentPartyAP);
    }
}
    
   
