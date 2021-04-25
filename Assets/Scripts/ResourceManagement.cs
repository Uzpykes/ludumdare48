using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceManager
{
    public static ItemDetails explosives = new ItemDetails(ResourceType.Explosives, 1, true, PurchaseRequirement.explosivesRequirement);
    public static ItemDetails diamond = new ItemDetails(ResourceType.Diamond, 0, false);
    public static ItemDetails gold = new ItemDetails(ResourceType.Gold, 0, false);
    public static ItemDetails silver = new ItemDetails(ResourceType.Silver, 0, false);
    public static ItemDetails blocks = new ItemDetails(ResourceType.Blocks, 0, false);

    public static UnityEvent<ItemDetails> onItemChange = new UnityEvent<ItemDetails>();

    public static void SetUp()
    {
        onItemChange?.RemoveAllListeners();
        explosives = new ItemDetails(ResourceType.Explosives, 1, true);
        diamond = new ItemDetails(ResourceType.Diamond, 0, false);
        gold = new ItemDetails(ResourceType.Gold, 0, false);
        silver = new ItemDetails(ResourceType.Silver, 0, false);
        blocks = new ItemDetails(ResourceType.Blocks, 0, false);
    }

    public static ItemDetails GetResourceOfType(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Diamond:
                return diamond;
            case ResourceType.Explosives:
                return explosives;
            case ResourceType.Gold:
                return gold;
            case ResourceType.Silver:
                return silver;
            case ResourceType.Blocks:
                return blocks;
        }
        return null;
    }

    public static ItemDetails CanBuy(ResourceType target)
    {
        var targetRes = GetResourceOfType(target);
        if (targetRes.CanBuy(blocks))
            return blocks;
        else if (targetRes.CanBuy(silver))
            return silver;
        else if (targetRes.CanBuy(gold))
            return gold;
        else if (targetRes.CanBuy(diamond))
            return diamond;
        return null;

    }

    public static void Buy(ResourceType target)
    {
        var targetRes = GetResourceOfType(target);
        ItemDetails currencyItem = CanBuy(target);

        if (currencyItem != null)
        {
            var req = targetRes.GetRequirement(currencyItem);
            ChangeCount(target, 1);
            ChangeCount(currencyItem.type, -req.count);
        }
    }

    public static int GetCount(ResourceType type)
    {
        var resource = GetResourceOfType(type);
        return resource.count;
    }

    public static void ChangeCount(ResourceType type, int count)
    {
        var resource = GetResourceOfType(type);
        resource.count += count;
        onItemChange?.Invoke(resource);
    }

}

public enum ResourceType : byte
{
    Explosives = 0,
    Diamond = 1,
    Gold = 2,
    Silver = 3,
    Blocks = 4
}

public class ItemDetails
{
    public ResourceType type { get; private set; }
    public int count;
    public bool isBuyable { get; private set; }
    public List<PurchaseRequirement> purchaceRequirements { get; private set; }

    public ItemDetails(ResourceType t, int c, bool isB, List<PurchaseRequirement> requirements = null)
    {
        type = t;
        count = c;
        isBuyable = isB;
        purchaceRequirements = requirements;
    }

    public bool CanBuy(ItemDetails otherItem)
    {
        if (!isBuyable)
            return false;

        if (purchaceRequirements.Exists(x => { return (x.type == otherItem.type && x.count <= otherItem.count);}))
        {
            return true;
        }
        return false;
    }

    public PurchaseRequirement GetRequirement(ItemDetails otherItem)
    {
        PurchaseRequirement req = purchaceRequirements.Find(x => { return x.type == otherItem.type; });
        return req;
    }
}

public struct PurchaseRequirement
{
    public ResourceType type;
    public int count;

    public static List<PurchaseRequirement> explosivesRequirement = new List<PurchaseRequirement>()
    {
        new PurchaseRequirement() { type = ResourceType.Diamond, count = 1 },
        new PurchaseRequirement() { type = ResourceType.Gold, count = 2 },
        new PurchaseRequirement() { type = ResourceType.Silver, count = 4 },
        new PurchaseRequirement() { type = ResourceType.Blocks, count = 100 }
    };
}
