using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSpellDataProvider : MonoBehaviour
{
    [SerializeField]
    private List<CharacterBattleSpellData> _characterSpells;

    private Dictionary<Divine.Moniker, List<BattleSpellData>> _characterSpellsDic;

    void Start()
    {
        _characterSpellsDic = new Dictionary<Divine.Moniker, List<BattleSpellData>>();

        for (int i = 0; i < _characterSpells.Count; i++)
            _characterSpellsDic.Add(_characterSpells[i].moniker, _characterSpells[i].spells);
    }

    public BattleSpellData GetBattleSpellData(CharInfo charInfo, int index, int currentPartyAP)
    {
        BattleSpellData btlSpData = _characterSpellsDic[charInfo.moniker][index];

        btlSpData.ResetDataValue(charInfo, index, currentPartyAP);
        
        return btlSpData;        
    }

    public List<BattleSpellData> GetBattleSpellDatas(CharInfo charInfo, int currentPartyAP)
    {
        List<BattleSpellData> listSpellData = new List<BattleSpellData>();
        
        for (int i = 0; i < charInfo.spells.Count; i++)
            listSpellData.Add(GetBattleSpellData(charInfo, i, currentPartyAP));
        
        return listSpellData;        
    }


    //Instance
    private static BattleSpellDataProvider _instance;
    public static BattleSpellDataProvider instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<BattleSpellDataProvider>();

            return _instance;
        }
    }
}
