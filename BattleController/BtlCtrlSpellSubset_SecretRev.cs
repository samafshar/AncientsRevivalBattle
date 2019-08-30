using System.Collections.Generic;

public class BtlCtrlSpellSubset_SecretRev : BtlCtrlSpellSubset
{
    BtlCtrlCharacter.ChangeStatInfo changeStatInfo;

    public override void StartIt(BtlCtrlSpell main, List<BtlCtrlCharacter> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].SetEnable(true);

            main.MakeAndAddSpellEffect(targets[i], SpellEffectOnChar.Appear, changeStatInfo);
        }
    }
}
