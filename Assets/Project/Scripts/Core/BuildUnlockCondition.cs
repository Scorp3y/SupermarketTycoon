using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.Core
{
    [CreateAssetMenu(menuName = "Retail Empire Tycoon/Unlock Condition", fileName = "UnlockCondition_")]
    [MovedFrom(false, "MyShopGame.Core", null, "BuildUnlockCondition")]
    public class BuildUnlockCondition : ScriptableObject
    {
        [Header("Progress")]
        public int requiredStoreLevel;

        [Header("Quest")]
        public string requiredQuestId;

        [Header("Territory")]
        public string requiredTerritoryId;

        [Header("Dependencies")]
        public bool requiresPreviousItemPurchased;
        public string previousItemId;
    }
}
