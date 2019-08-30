using System;
using System.Collections.Generic;
using Divine;

public class BotLogic
{    
    private int _AIPartyIndex = 1;
    private int _playerPartyIndex = 0;

    private float _botPower;
    private Party[] _parties;
    private Character _currentSelectedCharacter;
    private List<Character> _orderList;
    private Action<long, int> _sendAction;
    private BotSpellInfoHelper _spellHelper;

    public BotLogic(List<Character> orderList, float botPower)
    {
        _spellHelper = BotSpellInfoHelper.instance;

        _orderList = orderList;
        _botPower = botPower;
    }

    public void RunLogic(Character currentSelectedCharacter, Party[] parties, Action<long, int> sendAction)
    {
        _parties = parties;
        _sendAction = sendAction;
        _currentSelectedCharacter = currentSelectedCharacter;

        float rnd = UnityEngine.Random.Range(0f, 1f);
        if (rnd >= _botPower)
        {
            RunSpell_NormalDamage();
            return;
        }

        switch (_currentSelectedCharacter.moniker)
        {
            case Moniker.Archer:
                RunSpell_Archer();
                break;

            case Moniker.Healer:
                RunSpell_Healer();
                break;

            case Moniker.HealerAll:
                RunSpell_HealerAll();
                break;

            case Moniker.Feri:
                RunSpell_NormalDamage();
                break;

            case Moniker.Sagittarius:
                RunSpell_NormalDamage();
                break;

            case Moniker.Hanzo:
                RunSpell_Hanzo();
                break;

            case Moniker.JellyMage:
                RunSpell_JellyMage();
                break;

            case Moniker.Tiny:
                RunSpell_Tiny();
                break;

            case Moniker.SkyGirl:
                RunSpell_SkyGirl();
                break;

            case Moniker.FireSpirit:
                RunSpell_NormalDamage();
                break;

            case Moniker.Orc:
                RunSpell_Orc();
                break;

            case Moniker.Warrior:
                RunSpell_Warrior();
                break;
                
            case Moniker.Cleric:
                RunSpell_Cleric();
                break;

            case Moniker.Wizard:
                RunSpell_Wizard();
                break;

            case Moniker.WarriorChakra:
                RunSpell_WarriorChakra();
                break;

            case Moniker.ClericChakra:
                RunSpell_ClericChakra();
                break;

            case Moniker.WizardChakra:
                RunSpell_WizardChakra();
                break;

            default:
                RunSpell_NormalDamage();
                break;
        }
    }


    //Private Methods
    private void RunSpell_NormalDamage()
    {
        RunSpell_NormalDamage(0);
    }
    private void RunSpell_NormalDamage(int spellIndex)
    {
        Character TargetEnemy = null;

        TargetEnemy = GetTauntedEnemy(_parties[_playerPartyIndex]);

        if (TargetEnemy == null)
            TargetEnemy = GetWeakestCharacter(_parties[_playerPartyIndex]);

        _sendAction(TargetEnemy.id, spellIndex);
    }

    private void RunSpell_SplashDamage(int spellIndex)
    {
        Character enemy = _parties[_playerPartyIndex].GetRandomCharacter();

        _sendAction(enemy.id, spellIndex);
    }
    
    private void RunSpell_TrueDamage(int spellIndex)
    {
        Character target = null;

        target = GetTauntedEnemy(_parties[_playerPartyIndex]);

        if (target == null && _parties[_playerPartyIndex].hero.isActive && !_parties[_playerPartyIndex].hero.isDead)
            target = _parties[_playerPartyIndex].hero;

        if (target == null)
            target = GetWeakestCharacter(_parties[_playerPartyIndex]);

        _sendAction(target.id, spellIndex);
    }

    private bool RunSpell_BecameChakra(int chakraSpellIndex)
    {
        if (CanCastSpell(chakraSpellIndex))
        {
            _sendAction(_currentSelectedCharacter.id, chakraSpellIndex);

            return true;
        }

        return false;
    }
    

    //Characters-----------------------------------------
    private void RunSpell_JellyMage()
    {
        if (CanCastSpell(_spellHelper.jellyMage_SpellB))
        {
            if (_currentSelectedCharacter.GetHP() >=
                    GetPercentOfHp(_currentSelectedCharacter, _spellHelper.jellyMage_coefMinHpForSpellB))
            {
                _sendAction(_currentSelectedCharacter.id, _spellHelper.jellyMage_SpellB);
                return;
            }
        }

        RunSpell_NormalDamage();
    }

    private void RunSpell_SkyGirl()
    {
        if (_parties[_AIPartyIndex].actionPoint >= _spellHelper.skyGirl_minNumOfAPForSpellB)
            RunSpell_NormalDamage(_spellHelper.skyGirl_SpellB);
        else
            RunSpell_NormalDamage();
    }
    
    private void RunSpell_Hanzo()
    {
        if (CanCastSpell(_spellHelper.hanzo_SpellB) &&
               CurrentApCoefToNeededAp(_spellHelper.hanzo_SpellB) >= _spellHelper.hanzo_coefMinAPForSpellB)
            RunSpell_TrueDamage(_spellHelper.hanzo_SpellB);
        else
            RunSpell_TrueDamage(_spellHelper.hanzo_SpellA);
    }
    
    private void RunSpell_Archer()
    {
        List<Character> targets = _parties[_playerPartyIndex].GetAllAvailableCharacters();

        if (CanCastSpell(_spellHelper.archer_SpellB)
                    && targets.Count >= _spellHelper.archer_minNumOfEnemyForSpalsh)
            RunSpell_SplashDamage(_spellHelper.archer_SpellB);
        else
            RunSpell_NormalDamage();
    }

    private void RunSpell_HealerAll()
    {
        if (!CanCastSpell(_spellHelper.healerAll_SpellB))
        {
            RunSpell_NormalDamage();
            return;
        }

        List<Character> allies = _parties[_AIPartyIndex].GetAllAvailableCharacters();

        Character target = null;

        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].GetHP() < allies[i].charInfo.baseStats.maxHp * _spellHelper.healerAll_coefMinCriticalHP)
            {
                target = allies[i];
                break;
            }
        }

        if (target == null)
        {
            int damagedCharacters = 0;
            for (int i = 0; i < allies.Count; i++)
            {
                if (allies[i].GetHP() < allies[i].charInfo.baseStats.maxHp * _spellHelper.healerAll_coefMinHPToCountAzDmg)
                    damagedCharacters++;
            }

            if (damagedCharacters >= _spellHelper.healerAll_minNumOfDamagedAlliedToHeal)
                target = _parties[_AIPartyIndex].GetRandomCharacter();
        }

        if (target == null)
            RunSpell_NormalDamage();
        else
            _sendAction(target.id, _spellHelper.healerAll_SpellB);        
    }

    private void RunSpell_Tiny()
    {
        if (_currentSelectedCharacter.GetHP() <
                GetPercentOfHp(_currentSelectedCharacter, _spellHelper.tiny_coefMinSelfHpForTaunt))
        {
            RunSpell_NormalDamage();
            return;
        }

        if (CanCastSpell(_spellHelper.tiny_SpellB))
        {
            List<Character> allies = _parties[_AIPartyIndex].GetAllAvailableCharacters();

            for (int i = 0; i < allies.Count; i++)
            {
                if (allies[i].GetHP() < GetPercentOfHp(allies[i], _spellHelper.tiny_coefMinAllyHpForTaunt))
                {
                    _sendAction(_currentSelectedCharacter.id, _spellHelper.tiny_SpellB);
                    return;
                }
            }
        }

        RunSpell_NormalDamage();
    }

    private void RunSpell_Healer()
    {
        if (!CanCastSpell(_spellHelper.healerAll_SpellB))
        {
            RunSpell_NormalDamage();
            return;
        }

        List<Character> allies = _parties[_AIPartyIndex].GetAllAvailableCharacters();

        Character target = null;

        Character hero = _parties[_AIPartyIndex].hero;
        Character chakra = _parties[_AIPartyIndex].chakra;

        if (hero.GetHP() < GetPercentOfHp(hero, _spellHelper.healer_coefMinCriticalHP)
                                                 && hero.isActive)
            target = hero;
        else if (chakra.GetHP() < GetPercentOfHp(chakra, _spellHelper.healer_coefMinCriticalHP)
                                                 && chakra.isActive)
            target = chakra;

        if (target == null)
        {
            Character weakest = GetWeakestCharacter(_parties[_AIPartyIndex]);

            if (weakest.GetHP() < GetPercentOfHp(weakest, _spellHelper.healer_coefMinCriticalHP))            
                target = weakest;
        }

        //If target null means no one is proper for healling
        if (target == null &&
                CurrentApCoefToNeededAp(_spellHelper.healer_SpellB) > _spellHelper.healer_coefMinAPForSpellBDmg)
        {
            Character tauntedChar = GetTauntedEnemy(_parties[_playerPartyIndex]);

            if (tauntedChar != null)
                target = tauntedChar;
            else
            {
                if (_parties[_playerPartyIndex].hero.isActive)
                    target = _parties[_playerPartyIndex].hero;
                else                                  
                    target = GetWeakestCharacter(_parties[_playerPartyIndex]);                                 
            }
        }

        if (target != null)
            _sendAction(target.id, _spellHelper.healer_SpellB);
        else
            RunSpell_NormalDamage();
    }

    private void RunSpell_Orc()
    {
        Character target = null;

        target = GetTauntedEnemy(_parties[_playerPartyIndex]);

        if (target == null)
            target = GetWeakestCharacter(_parties[_playerPartyIndex]);

        if (target.GetHP() > GetPercentOfHp(target, _spellHelper.orc_coefMinHPUseSpellB) &&
                CanCastSpell(_spellHelper.orc_SpellB))
        {
            _sendAction(target.id, _spellHelper.orc_SpellB);
            return;
        }

        RunSpell_NormalDamage();
    }

    private void RunSpell_HeadRock()
    {
        Character target = GetTauntedEnemy(_parties[_playerPartyIndex]);

        if (target == null)
            target = GetWeakestCharacter(_parties[_playerPartyIndex]);

        if (target.GetHP() < GetPercentOfHp(target, _spellHelper.headRock_coefCriticalHPToForceAttack))
        {
            RunSpell_NormalDamage();
            return;
        }

        if (CanCastSpell(_spellHelper.headRock_SpellB))
        {
            List<Character> chars = _parties[_AIPartyIndex].GetAllAvailableCharacters();
            Character charToAddDmg = null;

            for (int i = 0; i < chars.Count; i++)
            {
                if ((chars[i].isHero || chars[i].isChakra) &&
                      chars[i].GetHP() > GetPercentOfHp(chars[i], _spellHelper.headRock_coefMinHpToUseSpellB))
                    charToAddDmg = chars[i];
                else if ((chars[i].moniker == Moniker.Goolakh || chars[i].moniker == Moniker.Orc) &&
                         chars[i].GetHP() > GetPercentOfHp(chars[i], _spellHelper.headRock_coefMinHpToUseSpellB))
                    charToAddDmg = chars[i];

                if (charToAddDmg != null)
                {
                    _sendAction(charToAddDmg.id, _spellHelper.headRock_SpellB);
                    return;
                }
            }
        }

        int rnd = UnityEngine.Random.Range(0, 100);
        if (rnd < _spellHelper.headRock_coefChanceToHitNextChar)
        {
            int currentCharIndex = _orderList.FindIndex(x => x.id == _currentSelectedCharacter.id);

            int i = currentCharIndex;
            int counter = 0;
            while (true)
            {
                counter++;
                i++;
                if (i >= _orderList.Count)
                    i = i % _orderList.Count;

                if (_currentSelectedCharacter.side != _orderList[i].side)
                {
                    target = _orderList[i];
                    break;
                }

                if (counter > _orderList.Count)
                {
                    DivineDebug.Log("Bot Head rock couldnt find proper target in row", DivineLogType.Error);
                    break;
                }
            }

            if (target != null)
            {
                _sendAction(target.id, _spellHelper.headRock_SpellA);
                return;
            }
        }
        else
        {
            RunSpell_NormalDamage();
            return;
        }
    }

    private void RunSpell_Warrior()
    {
        int curHP = _currentSelectedCharacter.GetHP();
        int maxHP = _currentSelectedCharacter.charInfo.baseStats.maxHp;
        Character target = null;

        if (curHP < maxHP * _spellHelper.warrior_coefToBecomeChakra)
        {
            if (RunSpell_BecameChakra(_spellHelper.warrior_spellChakra))
                return;
        }

        if (CanCastSpell(_spellHelper.warrior_spellD))
        {
            Character tauntedChar = GetTauntedEnemy(_parties[_playerPartyIndex]);

            if (tauntedChar != null)            
                target = tauntedChar.GetHP() < GetPercentOfHp(tauntedChar, _spellHelper.warrior_coefTargetHpToKill)
                            ? tauntedChar : null;            
            else
            { 
                List<Character> targets = _parties[_playerPartyIndex].GetAllAvailableCharacters();

                target = targets.Find(x =>
                           x.GetHP() < GetPercentOfHp(x, _spellHelper.warrior_coefTargetHpToKill));
            }

            if (target != null)
            {
                _sendAction(target.id, _spellHelper.warrior_spellD);
                return;
            }
        }

        //We dont have spell C yet

        if(CurrentApCoefToNeededAp(_spellHelper.warrior_spellB) >= _spellHelper.warrior_coefMinAPForSpellB)
        {
            target = GetTauntedEnemy(_parties[_playerPartyIndex]);

            if (target == null)
                target = GetWeakestCharacter(_parties[_playerPartyIndex]);

            _sendAction(target.id, _spellHelper.warrior_spellB);
            return;
        }

        //NormalDamage
        RunSpell_NormalDamage();
    }

    private void RunSpell_Cleric()
    {
        int curHP = _currentSelectedCharacter.GetHP();
        int maxHP = _currentSelectedCharacter.charInfo.baseStats.maxHp;
        Character target = null;

        if (curHP < maxHP * _spellHelper.cleric_coefToBecomeChakra)
        {
            if (RunSpell_BecameChakra(_spellHelper.cleric_spellChakra))
                return;
        }

        if (CanCastSpell(_spellHelper.cleric_spellD))
        {
            if (curHP > maxHP * _spellHelper.cleric_coefMinHeal)
            {
                _sendAction(_currentSelectedCharacter.id, _spellHelper.cleric_spellD);
                return;
            }
        }

        if (CurrentApCoefToNeededAp(_spellHelper.cleric_spellB) >= _spellHelper.cleric_coefMinAPForSpellB
              && curHP > maxHP * _spellHelper.cleric_coefDontNeedLifeSteal)
        {
            target = GetTauntedEnemy(_parties[_playerPartyIndex]);

            if (target == null)
                target = GetWeakestCharacter(_parties[_playerPartyIndex]);

            _sendAction(target.id, _spellHelper.cleric_spellB);
            return;
        }

        //NormalDamage
        RunSpell_NormalDamage();
    }

    private void RunSpell_Wizard()
    {
        int curHP = _currentSelectedCharacter.GetHP();
        int maxHP = _currentSelectedCharacter.charInfo.baseStats.maxHp;
        
        if (curHP < maxHP * _spellHelper.wizard_coefToBecomeChakra)
        {
            if (RunSpell_BecameChakra(_spellHelper.wizard_spellChakra))
                return;
        }

        if (CanCastSpell(_spellHelper.wizard_spellB))
        {
            List<Character> allies = _parties[_AIPartyIndex].GetAllAvailableCharacters();

            Character ally = null;

            for (int i = 0; i < allies.Count; i++)
            {
                if ((allies[i].moniker == Moniker.Goolakh || allies[i].moniker == Moniker.FireSpirit)
                        && allies[i].GetHP() > GetPercentOfHp(allies[i], _spellHelper.wizard_coefMinHealForTaunt))
                {
                    ally = allies[i];
                    break;
                }
            }

            if (ally == null)
            {
                Character WeakChar = GetWeakestCharacter(_parties[_AIPartyIndex]);
                if (WeakChar.GetHP() < GetPercentOfHp(WeakChar, _spellHelper.wizard_coefCriticalHP))
                    ally = GetHealthiestCharacter(_parties[_AIPartyIndex], false);
            }

            if (ally != null)
            {
                _sendAction(ally.id, _spellHelper.wizard_spellB);
                return;
            }
        }

        if (CanCastSpell(_spellHelper.wizard_spellD))
        {
            List<Character> targets = _parties[_playerPartyIndex].GetAllAvailableCharacters();

            if (targets.Count >= _spellHelper.wizard_minNumOfEnemyForSplash)
            {
                RunSpell_SplashDamage(_spellHelper.wizard_spellD);
                return;
            }
        }

        RunSpell_NormalDamage();
    }

    private void RunSpell_WarriorChakra()
    {
        Character target = null;
        
        if (CanCastSpell(_spellHelper.warriorChakra_spellB))
        {
            Character tauntedChar = GetTauntedEnemy(_parties[_playerPartyIndex]);

            if (tauntedChar != null)
                target = tauntedChar.GetHP() < GetPercentOfHp(tauntedChar, _spellHelper.warriorChakra_coefTargetHpToKill)
                            ? tauntedChar : null;
            else
            {
                List<Character> targets = _parties[_playerPartyIndex].GetAllAvailableCharacters();

                target = targets.Find(x =>
                           x.GetHP() < GetPercentOfHp(x, _spellHelper.warriorChakra_coefTargetHpToKill));
            }

            if (target != null)
            {
                _sendAction(target.id, _spellHelper.warriorChakra_spellB);
                return;
            }
        }

        if (CanCastSpell(_spellHelper.warriorChakra_spellC))
        {
            List<Character> targets = _parties[_playerPartyIndex].GetAllAvailableCharacters();

            if (targets.Count >= _spellHelper.warriorChakra_minNumOfEnemyForSplash)
            {
                RunSpell_SplashDamage(_spellHelper.warriorChakra_spellC);
                return;
            }
        }

        RunSpell_NormalDamage();
    }

    private void RunSpell_ClericChakra()
    {
        if(CanCastSpell(_spellHelper.clericChakra_spellB))
        {
            if (_currentSelectedCharacter.GetHP() <
                    GetPercentOfHp(_currentSelectedCharacter, _spellHelper.clericChakra_coefSelfCriticalHp))
            {
                Character target = null;

                target = GetTauntedEnemy(_parties[_playerPartyIndex]);

                if (target == null)
                    target = _parties[_playerPartyIndex].hero.isActive
                                            ? _parties[_playerPartyIndex].hero
                                            : _parties[_playerPartyIndex].chakra;

                _sendAction(target.id, _spellHelper.clericChakra_spellB);
                return;
            }
        }

        if(CanCastSpell(_spellHelper.clericChakra_spellC))
        {
            List<Character> targets = _parties[_playerPartyIndex].GetAllAvailableCharacters();

            if (targets.Count >= _spellHelper.clericChakra_minNumOfEnemyForSplash)
            {
                RunSpell_SplashDamage(_spellHelper.clericChakra_spellC);
                return;
            }
        }

        if (CanCastSpell(_spellHelper.clericChakra_spellB))
        {
            if (_currentSelectedCharacter.GetHP() <
                    GetPercentOfHp(_currentSelectedCharacter, _spellHelper.clericChakra_coefSelfHpToHeal))
            {
                Character target = _parties[_playerPartyIndex].hero.isActive
                                       ? _parties[_playerPartyIndex].hero
                                       : _parties[_playerPartyIndex].chakra;

                _sendAction(target.id, _spellHelper.clericChakra_spellB);
                return;
            }
        }

        RunSpell_NormalDamage();
    }

    private void RunSpell_WizardChakra()
    {
        if (CanCastSpell(_spellHelper.wizardChakra_spellC))
        {
            List<Character> targets = _parties[_playerPartyIndex].GetAllAvailableCharacters();

            if (targets.Count >= _spellHelper.wizardChakra_minNumOfEnemyForSplash)
            {
                RunSpell_SplashDamage(_spellHelper.wizardChakra_spellC);
                return;
            }
        }

        RunSpell_NormalDamage();
    }

    //Helper Methods
    private AppearanceConfigData.MagicAppearanceData GetSpellData(Moniker moniker, int index)
    {
        return AppearanceConfigData.instance.GetMagicWithIndex(moniker, index);
    }
    private Character GetWeakestCharacter(Party relatedParty)
    {
        List<Character> chars = relatedParty.GetAllAvailableCharacters();

        int weakestEnemyIndex = 0;
        for (int i = 0; i < chars.Count; i++)
        {
            int thisCharHP = chars[i].GetHP();
            if (thisCharHP > 0 && thisCharHP < chars[weakestEnemyIndex].GetHP())
                weakestEnemyIndex = i;
        }

        return chars[weakestEnemyIndex];
    }
    private Character GetHealthiestCharacter(Party relatedParty, bool canHero = true)
    {
        List<Character> chars = relatedParty.GetAllAvailableCharacters();

        int healthiestEnemyIndex = 0;
        bool found = false;

        for (int i = 0; i < chars.Count; i++)
        {
            if (!canHero && (chars[i].isHero || chars[i].isChakra))
                continue;

            int thisCharHP = chars[i].GetHP();
            if (thisCharHP > 0 && thisCharHP > chars[healthiestEnemyIndex].GetHP())
            {
                found = true;
                healthiestEnemyIndex = i;
            }
        }

        if (found)
            return chars[healthiestEnemyIndex];
        else
            return null;
    }
    private Character GetTauntedEnemy(Party relatedParty)
    {
        List<Character> chars = relatedParty.GetAllAvailableCharacters();
        
        int tauntedEnemyIndex = -1;
        for (int i = 0; i < chars.Count; i++)
        {
            if (chars[i].isTaunt)
                tauntedEnemyIndex = i;
        }

        return tauntedEnemyIndex == -1 ? null : chars[tauntedEnemyIndex];
    }
    private bool CanCastSpell(int spellIndex)
    {
        AppearanceConfigData.MagicAppearanceData spellData = new AppearanceConfigData.MagicAppearanceData();

        spellData = GetSpellData(_currentSelectedCharacter.moniker, spellIndex);

        return _parties[_AIPartyIndex].actionPoint >= spellData._cost;
    }
    private float CurrentApCoefToNeededAp(int spellIndex)
    {
        AppearanceConfigData.MagicAppearanceData spellData = new AppearanceConfigData.MagicAppearanceData();

        spellData = GetSpellData(_currentSelectedCharacter.moniker, spellIndex);

        return _parties[_AIPartyIndex].actionPoint / spellData._cost;
    }
    private float GetPercentOfHp(Character character,float coef)
    {
        return character.charInfo.baseStats.maxHp * coef;
    }
}
