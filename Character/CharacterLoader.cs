using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class CharacterLoader : MonoBehaviour
{
    private List<CharacterVisual> _characters;
    private Dictionary<Divine.Moniker, SkeletonGraphic> _charactersUISpine;

    //Base Methods
    void Start()
    {
        _charactersUISpine = new Dictionary<Divine.Moniker, SkeletonGraphic>();
    }

    public CharacterVisual GetCharacterVisual(Divine.Moniker moniker)
    {
        CharacterVisual result = null;

        for (int i = 0; i < _characters.Count; i++)
            if (_characters[i].moniker == moniker)
            {
                result = _characters[i];
                _characters.Remove(result);
                break;
            }

        if (result == null)
            result = LoadNewCharacter(moniker);

        return result;
    }

    public void AddCharacterVisualToLoad(List<Divine.Moniker> monikers)
    {
        if (_characters == null)
            _characters = new List<CharacterVisual>();

        foreach (Divine.Moniker mon in monikers)
            _characters.Add(LoadNewCharacter(mon));
    }

    public SkeletonGraphic GetCharacterUISpine(Divine.Moniker moniker)
    {
        SkeletonGraphic sklGraph = null;
        
        sklGraph = Instantiate(PrefabProvider_Menu.instance.GetPrefab_CharacterUI(
                                    moniker.ToString())
                                    ).GetComponent<SkeletonGraphic>();

        CustomizeManager.instance.DoCustomize(sklGraph, moniker);

        return sklGraph;
    }


    //Private Method
    private CharacterVisual LoadNewCharacter(Divine.Moniker moniker)
    {
        GameObject gObj = Instantiate(PrefabProvider_Battle.instance.GetPrefab_CharVisual(moniker.ToString()));
        gObj.transform.position = new Vector3(1000, 1000, 1000);

        return gObj.GetComponent<CharacterVisual>();
    }

    //Instance
    private static CharacterLoader _instance;
    public static CharacterLoader instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<CharacterLoader>();

            return _instance;
        }
    }
}
