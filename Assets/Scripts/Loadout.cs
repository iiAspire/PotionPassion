using System.Collections.Generic;

public class Loadout
{
    public SOCarryItem carry;
    public List<SOToolItem> tools = new();

    bool handOccupied = false;
    int usedSlots = 0;

    public bool TryAddTool(SOToolItem tool)
    {
        if (carry == null) return false;

        // --- Try using hands ---
        if (tool.requiresHand && carry.handsFree && !handOccupied)
        {
            tools.Add(tool);
            handOccupied = true;
            return true;
        }

        // --- Try storing in slots ---
        if (tool.canBeStored && usedSlots < carry.slotCapacity)
        {
            tools.Add(tool);
            usedSlots++;
            return true;
        }

        return false;
    }

    public void RemoveTool(SOToolItem tool)
    {
        if (!tools.Remove(tool))
            return;

        Recalculate();
    }

    void Recalculate()
    {
        handOccupied = false;
        usedSlots = 0;

        foreach (var t in tools)
        {
            if (t.requiresHand && carry.handsFree && !handOccupied)
                handOccupied = true;
            else
                usedSlots++;
        }
    }

    public int RemainingSlots =>
        carry ? carry.slotCapacity - usedSlots : 0;

    public bool HandAvailable =>
        carry && carry.handsFree && !handOccupied;
}