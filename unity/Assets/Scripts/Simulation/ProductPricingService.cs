using System;
using System.Linq;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public readonly struct ProductPriceChangeResult
    {
        public readonly bool Success;
        public readonly string ErrorMessage;

        private ProductPriceChangeResult(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }

        public static ProductPriceChangeResult Changed() => new(true, null);

        public static ProductPriceChangeResult Failed(string message) => new(false, message);
    }

    public sealed class ProductPricingService
    {
        public const double MinPrice = 1.0;
        public const double MaxPrice = 25.0;

        public ProductPriceChangeResult SetProductPrice(
            GameState state,
            string shopId,
            string productId,
            double price)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (string.IsNullOrWhiteSpace(shopId))
            {
                return ProductPriceChangeResult.Failed("Filiale nicht gefunden.");
            }

            Shop shop = state.Shops.FirstOrDefault(candidate => candidate.Id == shopId);
            if (shop == null)
            {
                return ProductPriceChangeResult.Failed("Filiale nicht gefunden.");
            }

            if (string.IsNullOrWhiteSpace(productId))
            {
                return ProductPriceChangeResult.Failed("Produkt nicht gefunden.");
            }

            ShopProduct product = shop.Menu.FirstOrDefault(candidate => candidate.ProductId == productId);
            if (product == null)
            {
                return ProductPriceChangeResult.Failed("Produkt nicht gefunden.");
            }

            if (double.IsNaN(price) || double.IsInfinity(price) || price < MinPrice || price > MaxPrice)
            {
                return ProductPriceChangeResult.Failed($"Preis muss zwischen {MinPrice:n2} EUR und {MaxPrice:n2} EUR liegen.");
            }

            product.Price = Math.Round(price, 2);
            return ProductPriceChangeResult.Changed();
        }
    }
}
