public static class RegionLookup
{
    public static SORegion Get(int nodeID)
    {
        if (RegionDatabase.ByID != null &&
            RegionDatabase.ByID.TryGetValue(nodeID, out var r))
            return r;

        return null;
    }
}