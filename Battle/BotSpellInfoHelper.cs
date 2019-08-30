using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSpellInfoHelper : MonoBehaviour
{
    [Header("Warrior")]
    public int warrior_spellA = 0;
    public int warrior_spellB = 1;
    //public int warrior_spellC = 0;
    public int warrior_spellD = 2;
    public int warrior_spellChakra = 3;
    public float warrior_coefToBecomeChakra = .2f;
    public float warrior_coefTargetHpToKill = .3f;
    public float warrior_coefMinAPForSpellB = 2f;

    [Header("Cleric")]
    public int cleric_spellA = 0;
    public int cleric_spellB = 1;
    //public int cleric_spellC = 0;
    public int cleric_spellD = 2;
    public int cleric_spellChakra = 3;
    public float cleric_coefMinHeal = .4f;
    public float cleric_coefMinAPForSpellB = 2f;
    public float cleric_coefToBecomeChakra = .2f;
    public float cleric_coefDontNeedLifeSteal = .7f;

    [Header("Wizard")]
    public int wizard_spellA = 0;
    public int wizard_spellB = 1;
    //public int cleric_spellC = 0;
    public int wizard_spellD = 2;
    public int wizard_spellChakra = 3;
    public int wizard_minNumOfEnemyForSplash = 3;
    public float wizard_coefCriticalHP = .2f;
    public float wizard_coefToBecomeChakra = .2f;
    public float wizard_coefMinHealForTaunt = .6f;
    public float wizard_coefMinHealForNotTaunt = .4f;

    [Header("WarriorChakra")]
    public int warriorChakra_spellA = 0;
    public int warriorChakra_spellB = 1;
    public int warriorChakra_spellC = 2;
    public int warriorChakra_minNumOfEnemyForSplash = 2;
    public float warriorChakra_coefTargetHpToKill = .3f;

    [Header("ClericChakra")]
    public int clericChakra_spellA = 0;
    public int clericChakra_spellB = 1;
    public int clericChakra_spellC = 2;
    public int clericChakra_minNumOfEnemyForSplash = 2;
    public float clericChakra_coefSelfHpToHeal = .6f;
    public float clericChakra_coefSelfCriticalHp = .25f;

    [Header("WizardChakra")]
    public int wizardChakra_spellA = 0;
    public int wizardChakra_spellC = 1;
    public int wizardChakra_minNumOfEnemyForSplash = 2;

    [Header("Hanzo")]
    public int hanzo_SpellA = 0;
    public int hanzo_SpellB = 1;
    public float hanzo_coefMinAPForSpellB = 2f;

    [Header("Archer")]
    public int archer_SpellA = 0;
    public int archer_SpellB = 1;
    public int archer_minNumOfEnemyForSpalsh = 2;

    [Header("SkyGirl")]
    public int skyGirl_SpellA = 0;
    public int skyGirl_SpellB = 1;
    public int skyGirl_minNumOfAPForSpellB = 5;

    [Header("JellyMage")]
    public int jellyMage_SpellA = 0;
    public int jellyMage_SpellB = 1;
    public float jellyMage_coefMinHpForSpellB = .5f;

    [Header("HealerAll")]
    public int healerAll_SpellA = 0;
    public int healerAll_SpellB = 1;
    public int healerAll_minNumOfDamagedAlliedToHeal = 2;
    public float healerAll_coefMinHPToCountAzDmg = .7f;
    public float healerAll_coefMinCriticalHP = .2f;

    [Header("Tiny")]
    public int tiny_SpellA = 0;
    public int tiny_SpellB = 1;
    public float tiny_coefMinSelfHpForTaunt = .5f;
    public float tiny_coefMinAllyHpForTaunt = .6f;

    [Header("Healer")]
    public int healer_SpellA = 0;
    public int healer_SpellB = 1;
    public float healer_coefMinCriticalHP = .35f;
    public float healer_coefMinAPForSpellBDmg = 3f;

    [Header("Orc")]
    public int orc_SpellA = 0;
    public int orc_SpellB = 1;
    public float orc_coefMinHPUseSpellB = .3f;

    [Header("HeadRock")]
    public int headRock_SpellA = 0;
    public int headRock_SpellB = 1;
    public float headRock_coefMinHpToUseSpellB = .5f;
    public float headRock_coefChanceToHitNextChar = .7f;
    public float headRock_coefCriticalHPToForceAttack = .1f;

    //Instance---------------------------------------------
    private static BotSpellInfoHelper _instance;

    public static BotSpellInfoHelper instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<BotSpellInfoHelper>();

            return _instance;
        }
    }
}
