// Döner Empire 3D — Shop-Equipment-Instanz
// Port aus lib/models/equipment_model.dart (ShopEquipment).
// EquipmentData liegt in Data/GameCatalog.cs, EquipmentCategory in Core/Enums.cs.

namespace DoenerEmpire.Models
{
    /// <summary>Im Shop installiertes Equipment (Verweis auf EquipmentData-Id).</summary>
    public sealed class ShopEquipment
    {
        public string EquipmentId;

        public ShopEquipment Clone() => new() { EquipmentId = EquipmentId };
    }
}
