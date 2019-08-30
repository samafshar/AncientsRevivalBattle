using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStartingPoint : MonoBehaviour
{
    [SerializeField]
    private int _formationIndex;

    public int formationIndex { get { return _formationIndex; } }
}