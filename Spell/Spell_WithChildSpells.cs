using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_WithChildSpells : Spell
{
    public enum SpellWithChildType
    {
        BaseOnEvent,
        BaseOnSpellInfo,
    }

    [SerializeField]
    private SpellWithChildType  _type;
    [SerializeField]
    private float               _delayBetweenEach = 0;
    [SerializeField]
    private Spell               _spell;

    private int             _numOfCreatedSpells = 0;
    private int             _removeSpells = 0;
    private List<int>       _indexesOfDamagedSpells;
    private List<Spell>     _createdSpells = new List<Spell>();

    protected override void StartSpell()
    {
        if(_indexesOfDamagedSpells == null)
        {
            _indexesOfDamagedSpells = new List<int>();
            _indexesOfDamagedSpells = FindIndexesOfDamagedSpells();
        }

        switch (_type)
        {
            case SpellWithChildType.BaseOnEvent:
                MakeSpell();
                break;

            case SpellWithChildType.BaseOnSpellInfo:
                for (int i = 0; i < _indexesOfDamagedSpells.Count; i++)
                {
                    if (_delayBetweenEach > 0)
                        StartCoroutine(MakeSpellWithDelay(_delayBetweenEach * i));
                    else
                        MakeSpell();
                }
                break;
        }
    }

    public override void HappenedAgain(bool isLastCall = false)
    {
        if (_type == SpellWithChildType.BaseOnEvent)
        {
            StartSpell();
        }
    }

    private IEnumerator MakeSpellWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        MakeSpell();
    }

    private void MakeSpell()
    {
        List<SpellEffectInfo> spEfInf = new List<SpellEffectInfo>();

        int currentIndex = _indexesOfDamagedSpells[_numOfCreatedSpells];

        if (_numOfCreatedSpells + 1 == _indexesOfDamagedSpells.Count)
            for (int i = _numOfCreatedSpells; i < _spellInfo.spellEffectInfos.Count; i++)
                spEfInf.Add(_spellInfo.spellEffectInfos[i]); //Because now all other effects are sequential
        else
            spEfInf.Add(_spellInfo.spellEffectInfos[currentIndex]);

        SpellInfo spIn = new SpellInfo(
                                            _spellInfo.charSpellsIndex,
                                            _spellInfo.isInstant,
                                            _spellInfo.dontShowToUser,
                                            _spellInfo.needTargetToComeNear,
                                            _spellInfo.spellName,
                                            _spellInfo.cost,
                                            _spell.name,
                                            _spellInfo.spellType,
                                            _spellInfo.damageType,
                                            _spellInfo.spellImpact,
                                            spEfInf
                                            );
        spIn.owner = _spellInfo.owner;
        
        SpellCastTransData transData = new SpellCastTransData();

        SpellCastHelper_MultiSpell helper_multiCast = gameObject.GetComponent<SpellCastHelper_MultiSpell>();
        if (helper_multiCast != null)
            transData = helper_multiCast.FindProperCastingPos(spIn.owner,
                                            _spellInfo.spellEffectInfos[currentIndex].targetCharacter);
        else
        {
            transData.position = transform.position;
            transData.rotationAmount = 0;
            transData.scale = transform.localScale;
        }
        
        Spell temp = SpellBuilder.Build(spIn, transData);
        temp.Event_SpellWillDestroy += OnSpellDestroy;
        _createdSpells.Add(temp);

        _numOfCreatedSpells++;
    }

    private List<int> FindIndexesOfDamagedSpells()
    {
        List<int> indexs = new List<int>();

        for (int i = 0; i < _spellInfo.spellEffectInfos.Count; i++)
        {
            SpellEffectOnChar effect = _spellInfo.spellEffectInfos[i].effectOnCharacter;

            if (effect == SpellEffectOnChar.NormalDamage || effect == SpellEffectOnChar.SeriousDamage
                            || effect == SpellEffectOnChar.Dodge || effect == SpellEffectOnChar.Miss)
                indexs.Add(i);            
        }

        return indexs;
    }

    private void OnSpellDestroy(Spell spell)
    {
        _removeSpells++;

        _createdSpells.Remove(spell);

        if (_removeSpells == _indexesOfDamagedSpells.Count)
            DestroyIt();        
    }
}
