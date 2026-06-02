// Döner Empire 3D — Shop-Sortiment-Instanz
// Port aus lib/models/product_model.dart (ShopProduct).
// ProductData liegt in Data/GameData.cs, ProductCategory in Core/Enums.cs.

namespace DoenerEmpire.Models
{
    /// <summary>Eine Menü-Position im Shop; Preis ist spielerseitig anpassbar.</summary>
    public sealed class ShopProduct
    {
        public string ProductId;
        public double Price;
        public bool IsActive = true;

        public ShopProduct Clone() => new()
        {
            ProductId = ProductId,
            Price = Price,
            IsActive = IsActive,
        };
    }
}
