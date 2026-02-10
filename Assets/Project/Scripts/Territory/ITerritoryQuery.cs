using UnityEngine;

namespace RetailEmpireTycoon.Territory
{
    public interface ITerritoryQuery
    {
        bool IsCellPurchased(Vector3Int cell);
    }
}
