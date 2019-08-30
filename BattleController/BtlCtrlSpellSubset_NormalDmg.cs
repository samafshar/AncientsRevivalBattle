using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtlCtrlSpellSubset_NormalDmg : BtlCtrlSpellSubset
{
    BtlCtrlCharacter.ChangeStatInfo changeStatInfo;

    public override void StartIt(BtlCtrlSpell main, List<BtlCtrlCharacter> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            changeStatInfo = targets[i].ApplyDamage(main.damage);

            main.MakeAndAddSpellEffect(targets[i], SpellEffectOnChar.NormalDamage, changeStatInfo);
        }
    }    
}
