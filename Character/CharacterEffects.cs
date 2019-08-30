using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEffects : MonoBehaviour
{
    [SerializeField]
    private VFX         _grave;
    [SerializeField]
    private VFX         _fearEffect;
    [SerializeField]
    private VFX         _deathRattle;
    [SerializeField]
    private VFX         _damageReductionEffect;
    [SerializeField]
    private Transform   _tr_Helper_Fear;

    private VFX _vfx;
    private Divine.Moniker _moniker;

    public void HandleChangingFlag(BattleFlags newFlags,BattleFlags previousFlags, Vector3 scale)
    {
        if (newFlags == previousFlags)
            return;

        _vfx = null;
        Vector3 pos = Vector3.zero;

        List<BattleFlags> newFlagList = new List<BattleFlags>();
        List<BattleFlags> prevFlagList = new List<BattleFlags>();

        foreach (BattleFlags flag in Enum.GetValues(typeof(BattleFlags)))
        {
            if ((newFlags & flag) == flag)
                newFlagList.Add(flag);

            if ((previousFlags & flag) == flag)
                prevFlagList.Add(flag);
        }

        foreach(BattleFlags nFlag in newFlagList)
        {
            if(!prevFlagList.Contains(nFlag))
            {
                switch (nFlag)
                {
                    case BattleFlags.Fear:
                        _vfx = _fearEffect;
                        pos = _tr_Helper_Fear.position;
                        break;

                    case BattleFlags.DamageReduction:
                        _vfx = _damageReductionEffect;
                        pos = _tr_Helper_Fear.position;
                        break;

                    case BattleFlags.DeathRattle:
                        _vfx = _deathRattle;
                        pos = transform.position;
                        break;
                }
            }
        }        

        if (_vfx != null)
            CreateVFX(_vfx, pos, scale);
    }

    public void ShowEffect_Grave()
    {
        CreateVFX(_grave, transform.position, transform.localScale);
    }

    public void Init(Divine.Moniker moniker)
    {
        _moniker = moniker;
    }

    //Private Methods
    private void OnEffectEnd(VFX endedVFX)
    {
        Destroy(endedVFX.gameObject);
    }

    private void CreateVFX(VFX neededVFX, Vector3 pos, Vector3 scale)
    {
        _vfx = Instantiate(neededVFX);
        _vfx.transform.position = pos;

        _vfx.SetScale(scale);
        _vfx.StartVfx(OnEffectEnd);
    }
}
