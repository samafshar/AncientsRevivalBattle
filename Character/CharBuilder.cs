using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharBuilder
{
    public static Character Build(CharInfo chInfo, Party owner, Vector3 position, Quaternion rotation)
    {
        CharacterVisual charV = CharacterLoader.instance.GetCharacterVisual(chInfo.moniker);
        charV.transform.position = position;
        charV.transform.rotation = Quaternion.identity;

        charV.SetGraphicRootOrientation(rotation);
        
        Character charac = new Character();
        charac.Init(owner, chInfo, charV);

        return charac;
    }
}
