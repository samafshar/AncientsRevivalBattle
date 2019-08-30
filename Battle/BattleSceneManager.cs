using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BattleSceneType
{
    Jungle,
    Castle,
    Hell,
    Tutorial_Castle,
    Tutorial_Hell
}

public class BattleSceneManager : MonoBehaviour
{
    [SerializeField]
    private BattleScene[]   _battleScenes;

    private bool _isTablet;
    private BattleScene _currentBattleScene;
    private Dictionary<BattleSceneType, BattleScene> _battleSceneDic = new Dictionary<BattleSceneType, BattleScene>();

    public BattleScene currentBattleScene { get { return _currentBattleScene; } }

    private void Awake()
    {
        FitAspectRatio();

        AddBattleScenes();
    }
    
    private void FitAspectRatio()
    {
        // determine aspect ratio
        _isTablet = ((float)Screen.width / (float)Screen.height) < 1.5f;

        // activate appropriate scene elements

        MainCamera.instance.SetAspectRatio(_isTablet);
    }

    private void AddBattleScenes()
    {
        for (int i = 0; i < _battleScenes.Length; i++)
        {
            _battleSceneDic.Add(_battleScenes[i].type, _battleScenes[i]);

            if (_battleScenes[i].gameObject.activeSelf)
                _currentBattleScene = _battleScenes[i];
        }
    }

    public BattleScene SelectBattleScene(BattleSceneType battleType)
    {
        if (_currentBattleScene != null)
            _currentBattleScene.FinishIt();

        if (_battleSceneDic.Count == 0)
            AddBattleScenes();

        _currentBattleScene = _battleSceneDic[battleType];
        _currentBattleScene.StartIt(_isTablet);

        return _currentBattleScene;
    }

    public void DeselectBattleScene()
    {
        if (_currentBattleScene != null)
            _currentBattleScene.FinishIt();
    }

    #region instance
    private static BattleSceneManager _instance;
    public static BattleSceneManager instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<BattleSceneManager>();

            return _instance;
        }
    } 
    #endregion
}
