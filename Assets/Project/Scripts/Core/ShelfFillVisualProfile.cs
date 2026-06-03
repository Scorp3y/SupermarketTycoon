using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.Core
{
    [CreateAssetMenu(menuName = "Retail Empire Tycoon/Shelf Fill Visual Profile", fileName = "ShelfFillProfile_")]
    [MovedFrom(false, "MyShopGame.Core", null, "ShelfFillVisualProfile")]
    public class ShelfFillVisualProfile : ScriptableObject
    {
        public ShelfFillStage[] stages = Array.Empty<ShelfFillStage>();

        [Serializable]
        [MovedFrom(false, "MyShopGame.Core", null, "ShelfFillStage")]
        public class ShelfFillStage
        {
            public string stageName = "Empty";
            [Range(0f, 1f)]
            public float minFill01;
            public GameObject[] enableObjects = Array.Empty<GameObject>();
            public GameObject[] disableObjects = Array.Empty<GameObject>();
        }
    }
}
