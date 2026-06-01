using System;
using System.Collections.Generic;
using UnityEngine;

public class Astronaut : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    public enum OutfitOption
    {
        BLUE_JUMPSUIT,
        SPACESUIT
    }

    [Serializable]
    public struct OutfitData
    {
        public OutfitOption outfit;
        public Sprite sprite;
    }

    [SerializeField]
    private List<OutfitData> outfitList;
    private Dictionary<OutfitOption, Sprite> _outfitDict;

    private OutfitOption _outfit;
    public OutfitOption Outfit
    {
        get { return _outfit; }
        set
        {
            if (value == _outfit)
            {
                return;
            }
            _outfit = value;

            _spriteRenderer.sprite = _outfitDict[value];
        }        
    }

    private bool _isInside;
    public bool IsInside
    {
        get { return _isInside;}
        set
        {
            if (value == _isInside)
            {
                return;
            }
            _isInside = value;
        }
    }

    private uint _insideCount;


    private void Start()
    {
        _insideCount = 0;
        IsInside = true;
        Outfit = OutfitOption.BLUE_JUMPSUIT;
        _outfitDict = new();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        foreach (var entry in outfitList)
        {
            _outfitDict.Add(entry.outfit, entry.sprite);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Inside")
        {
            _insideCount++;
            IsInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "Inside")
        {
            _insideCount--;
            if (_insideCount <= 0)
            {
                IsInside = false;
            }
        }
    }
}
