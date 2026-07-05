#nullable enable

using System;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace HackedDesign
{
    [Serializable]
    public class Outfit
    {
        [SerializeField] private string name = "";
        [SerializeField] private SpriteLibraryAsset? library = null;

        public string Name => name;
        public SpriteLibraryAsset? Library => library;
    }
}
