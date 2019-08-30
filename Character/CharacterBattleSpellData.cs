using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBattleSpellData : MonoBehaviour
{
    [SerializeField]
    private Divine.Moniker _moniker;
    [SerializeField]
    private List<BattleSpellData> _spells;

    public Divine.Moniker moniker { get { return _moniker; } }
    public List<BattleSpellData> spells { get { return _spells; } }
}
