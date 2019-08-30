using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_APGenerator : Spell
{
    [SerializeField] private float _delayForAPGeneration = 0.3f;

    protected override void StartSpell()
    {
        ApplySpellEffects();

        if (_spellInfo.owner.side == PartySide.Player)
            BattleUI.instance.CreateAPGenEffects(_spellInfo.owner.position_pivot, _spellInfo.generatedActionPoint);

        Invoke("GenerateAP", _delayForAPGeneration);
    }

    private void GenerateAP()
    {
        DivineDebug.Log("Adding Action Point: " + _spellInfo.generatedActionPoint);
        
        _spellInfo.owner.AddActionPoint(_spellInfo.generatedActionPoint);

        DestroyIt();
    }
}