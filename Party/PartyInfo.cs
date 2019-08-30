using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyInfo
{
    private int         _actionPoint;
    private string      _partyName;
    private HeroInfo    _heroInfo;
    private CharInfo[]  _charInfoes;
    private List<Divine.Secret> _availableSecrets;

    public int actionPoint { get { return _actionPoint; } }
    public string Name { get { return _partyName; } }
    public HeroInfo heroInfo        { get { return _heroInfo; } }
    public CharInfo[] charInfoes    { get { return _charInfoes; } }
    public List<Divine.Secret> availableSecrets { get { return _availableSecrets; } }


    //Base Methods
    public PartyInfo(HeroInfo heroInfo, CharInfo[] charsInfo, List<Divine.Secret> secrets, int actionPoint , string partyName="")
    {
        DivineDebug.Log("Party info created with " + charsInfo.Length + " characters.");

        _charInfoes = new CharInfo[charsInfo.Length];

        _heroInfo = heroInfo;

        for (int i = 0; i < _charInfoes.Length; i++)
        {
            _charInfoes[i] = charsInfo[i];
        }

        _availableSecrets = secrets;

        _actionPoint = actionPoint;

        _partyName = partyName;
    }
}
