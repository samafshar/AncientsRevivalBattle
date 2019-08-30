using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleScene : MonoBehaviour
{
    [SerializeField]
    private GameObject                  _16_9_environment;
    [SerializeField]
    private GameObject                  _4_3_environment;
    [SerializeField]
    protected Transform                 _selectedPoint_left;
    [SerializeField]
    protected Transform                 _selectedPoint_right;
    [SerializeField]
    protected Transform[]               _startedPointsForLeftSide;
    [SerializeField]
    protected Transform[]               _startedPointsForRightSide;
    [SerializeField]
    protected Transform[]               _spawnPointsForLeftSide;
    [SerializeField]
    protected Transform[]               _spawnPointsForRightSide;
    [SerializeField]
    protected CharacterStartingPoint[]  _formationPointsForLeftSide;
    [SerializeField]
    protected CharacterStartingPoint[]  _formationPointsForRightSide;
    [SerializeField]
    protected BattleSceneType           _type;
    
    public Transform selectedPoint_left { get { return _selectedPoint_left; } }
    public Transform selectedPoint_right { get { return _selectedPoint_right; } }
    public Transform[] startingPointsForLeftSide { get { return _startedPointsForLeftSide; } }
    public Transform[] startingPointsForRightSide { get { return _startedPointsForRightSide; } }
    public Transform[] spawnPointsForLeftSide { get { return _spawnPointsForLeftSide; } }
    public Transform[] spawnPointsForRightSide { get { return _spawnPointsForRightSide; } }
    public CharacterStartingPoint[] formationPointsForLeftSide { get { return _formationPointsForLeftSide; } }
    public CharacterStartingPoint[] formationPointsForRightSide { get { return _formationPointsForRightSide; } }
    public BattleSceneType type { get { return _type; } }

    //Public Methods
    public virtual void StartIt(bool isTablet)
    {
        gameObject.SetActive(true);

        _4_3_environment.SetActive(isTablet);
        _16_9_environment.SetActive(!isTablet);
    }
    public virtual void FinishIt()
    {
        gameObject.SetActive(false);
    }
}
