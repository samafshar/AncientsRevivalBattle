using System;
using System.Collections.Generic;

public class BtlCtrlSecretController
{
    public delegate void Deleg_ActionGenerated(ActionData actionData);

    public event Deleg_ActionGenerated Event_ActionGenerated;

    //Public Methods
    public void CastSecret(BtlCtrlCharacter owner, Divine.Secret type,
                           List<BtlCtrlCharacter> targets, Action<BtlCtrlSpell> onCastFinish)
    {
        BtlCtrlSpell_SecretRevive revive = new BtlCtrlSpell_SecretRevive();
        revive.Init(SpellEffectOnChar.Appear, 0, 0, "");

        revive.SetOwnerAndIndex(owner, -1);
        
        revive.Event_ActionGenerated += OnActionGenerated;

        revive.StartSpell(targets, onCastFinish);
    }

    private void OnActionGenerated(ActionData actionData)
    {
        if (Event_ActionGenerated != null)
            Event_ActionGenerated(actionData);
    }
}