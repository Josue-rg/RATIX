using UnityEngine;

namespace RATIX
{
    /// <summary>
    /// Warehouse panel: equipped weapon slots and the owned item grid.
    /// Actions only log for now; the inventory backend is not implemented yet.
    /// </summary>
    public class WarehousePanel : MonoBehaviour
    {
        public void Equip(int index) => Debug.Log($"[RATIX] Warehouse equip item {index}");
        public void RemoveItem(int index) => Debug.Log($"[RATIX] Warehouse remove item {index}");
        public void RemoveEquipped(int index) => Debug.Log($"[RATIX] Warehouse unequip slot {index}");
        public void RepairAll() => Debug.Log("[RATIX] Repair all weapons");
        public void Clear() => Debug.Log("[RATIX] Clear warehouse filter");
    }
}
