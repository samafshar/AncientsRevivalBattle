using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpellBuilder
{
    public static Spell Build(SpellInfo spellInfo, SpellCastTransData transData)
    {
        Spell spell = GameObject.Instantiate(spellInfo.GetSpellPrefab()).GetComponent<Spell>();
        spell.transform.position = transData.position;
        spell.transform.localScale = transData.scale;
        spell.Init(spellInfo);
        return spell;
    }

    public static Spell Build(SpellInfo spellInfo, Vector3 pos,Quaternion rot,Vector3 scale)
    {
        Spell spell = GameObject.Instantiate(spellInfo.GetSpellPrefab()).GetComponent<Spell>();
        spell.transform.position = pos;
        spell.transform.rotation = rot;
        spell.transform.localScale = scale;
        spell.Init(spellInfo);
        return spell;
    }
}
