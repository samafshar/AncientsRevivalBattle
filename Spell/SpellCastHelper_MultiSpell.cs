using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCastHelper_MultiSpell : MonoBehaviour
{
    public SpellCastTransData FindProperCastingPos(Character owner, Character target)
    {
        return FindProperCastingPos(owner.moniker, target);
    }

    public SpellCastTransData FindProperCastingPos(Divine.Moniker owner, Character target)
    {
        if (owner == Divine.Moniker.Archer)
            return FindProperCastingPos_Archer(target);
        else if (owner == Divine.Moniker.Wizard)
            return FindProperCastingPos_Wizard(target);
        else if (owner == Divine.Moniker.Skeleton_Archer)
            return FindProperCastingPos_Archer(target);
        else if (owner == Divine.Moniker.Kharback)
            return FindProperCastingPos_Archer(target);
        else if (owner == Divine.Moniker.FereydounChakra)
            return FindProperCastingPos_ChakraFereydoun(target);
        else if (owner == Divine.Moniker.Fereydoun)
            return FindProperCastingPos_Wizard(target);

        DivineDebug.Log("Find proper pos doesnt support this character: " + target.moniker, DivineLogType.Error);
        return null;
    }

    //Private Methods
    private SpellCastTransData FindProperCastingPos_Archer(Character target)
    {
        SpellCastTransData stData = new SpellCastTransData();

        stData.position = new Vector3(0f, OverScreen() + 2.5f, 0f);
        stData.rotationAmount = 0f;
        stData.scale = target.scale;

        return stData;
    }

    private SpellCastTransData FindProperCastingPos_Wizard(Character target)
    {
        SpellCastTransData stData = new SpellCastTransData();

        stData.position = new Vector3(target.position_pivot.x, OverScreen() * 1.2f, target.position_pivot.z);
        stData.rotationAmount = 0f;
        stData.scale = target.scale;

        return stData;
    }


    private SpellCastTransData FindProperCastingPos_ChakraFereydoun(Character target)
    {
        SpellCastTransData stData = new SpellCastTransData();

        stData.position = new Vector3(target.position_pivot.x, target.position_pivot.y, target.position_pivot.z);
        stData.rotationAmount = 0f;
        stData.scale = target.scale;

        return stData;
    }

    private float OverScreen()
    {
        return Camera.main.orthographicSize;
    }
}
