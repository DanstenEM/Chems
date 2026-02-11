using System;
using System.Collections.Generic;

[Serializable]
public class SavedItemStack
{
    public string itemId;
    public int count;
}

[Serializable]
public class SavedInventory
{
    public List<SavedItemStack> stacks = new List<SavedItemStack>();
}
