using RetailEmpireTycoon.Core;

namespace RetailEmpireTycoon.BuildSystem
{
    public interface IPlacementRule
    {
        bool EnabledFor(BuildItemData item);
        PlacementResult Evaluate(PlacementRequest req);
    }
}
