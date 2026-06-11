using System;
using System.Linq;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using DoenerEmpire.Simulation;
using DoenerEmpire.View3D;

namespace DoenerEmpire.App
{
    public readonly struct LocationSelectedEvent
    {
        public readonly CityMapHotspot Hotspot;

        public LocationSelectedEvent(CityMapHotspot hotspot)
        {
            Hotspot = hotspot;
        }
    }

    public readonly struct BuyDialogRequestedEvent
    {
        public readonly CityMapHotspot Hotspot;

        public BuyDialogRequestedEvent(CityMapHotspot hotspot)
        {
            Hotspot = hotspot;
        }
    }

    public readonly struct RestaurantDetailRequestedEvent
    {
        public readonly string ShopId;

        public RestaurantDetailRequestedEvent(string shopId)
        {
            ShopId = shopId;
        }
    }

    public readonly struct ToastRequestedEvent
    {
        public readonly string Message;

        public ToastRequestedEvent(string message)
        {
            Message = message;
        }
    }

    public readonly struct StateSnapshotChangedEvent
    {
        public readonly GameState State;

        public StateSnapshotChangedEvent(GameState state)
        {
            State = state;
        }
    }

    public readonly struct DayEndedEvent
    {
        public readonly DailyRecord Record;
        public readonly DayResult Result;

        public DayEndedEvent(DailyRecord record, DayResult result)
        {
            Record = record;
            Result = result;
        }
    }

    public sealed class GameController
    {
        private readonly GameState state;
        private readonly ShopOpeningService shopOpeningService = new();
        private readonly ProductPricingService productPricingService = new();
        private readonly ShopExpansionService shopExpansionService = new();
        private readonly EquipmentPurchaseService equipmentPurchaseService = new();
        private readonly EmployeeHiringService employeeHiringService = new();
        private readonly ShopCampaignService shopCampaignService = new();

        public GameController(GameState initialState, EventBus eventBus)
        {
            state = initialState ?? throw new ArgumentNullException(nameof(initialState));
            Events = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public EventBus Events { get; }

        public GameState State => state;

        public void PublishSnapshot()
        {
            Events.Publish(new StateSnapshotChangedEvent(state.Clone()));
        }

        public void SelectLocation(CityMapHotspot hotspot)
        {
            if (hotspot == null)
            {
                return;
            }

            if (hotspot.State == CityMapHotspotState.Locked)
            {
                Events.Publish(new ToastRequestedEvent("Erst durch Umsatz oder Stadtfreischaltung verfuegbar."));
                return;
            }

            Events.Publish(new LocationSelectedEvent(hotspot));
        }

        public void RequestBuyDialog(CityMapHotspot hotspot)
        {
            if (hotspot == null)
            {
                return;
            }

            if (hotspot.State != CityMapHotspotState.Available)
            {
                Events.Publish(new ToastRequestedEvent("Kaufen ist nur auf freien Standorten moeglich."));
                return;
            }

            Events.Publish(new BuyDialogRequestedEvent(hotspot));
        }

        public void OpenShop(CityMapHotspot hotspot)
        {
            if (hotspot == null)
            {
                return;
            }

            if (hotspot.State != CityMapHotspotState.Available)
            {
                Events.Publish(new ToastRequestedEvent("Kaufen ist nur auf freien Standorten moeglich."));
                return;
            }

            ShopOpeningResult result = shopOpeningService.OpenShop(state, new ShopOpeningRequest(
                hotspot.Id,
                state.CompanyName,
                CityIdFor(hotspot),
                hotspot.DisplayName,
                hotspot.FootTraffic,
                hotspot.WeeklyRent,
                hotspot.Deposit,
                hotspot.Personality));

            if (!result.Success)
            {
                Events.Publish(new ToastRequestedEvent(result.ErrorMessage));
                return;
            }

            hotspot.MarkOwned(result.Shop);
            PublishSnapshot();
            Events.Publish(new LocationSelectedEvent(hotspot));
            Events.Publish(new RestaurantDetailRequestedEvent(result.Shop.Id));
            Events.Publish(new ToastRequestedEvent("Filiale eroeffnet."));
        }

        public void RequestRestaurantDetail(CityMapHotspot hotspot)
        {
            if (hotspot == null)
            {
                return;
            }

            if (hotspot.State != CityMapHotspotState.Owned)
            {
                Events.Publish(new ToastRequestedEvent("Optimieren ist nur fuer eigene Filialen moeglich."));
                return;
            }

            Events.Publish(new RestaurantDetailRequestedEvent(hotspot.Id));
        }

        public void SetProductPrice(string shopId, string productId, double price)
        {
            ProductPriceChangeResult result = productPricingService.SetProductPrice(state, shopId, productId, price);
            if (!result.Success)
            {
                Events.Publish(new ToastRequestedEvent(result.ErrorMessage));
                return;
            }

            PublishSnapshot();
            Events.Publish(new RestaurantDetailRequestedEvent(shopId));
            Events.Publish(new ToastRequestedEvent("Preis aktualisiert."));
        }

        public void UpgradeShopSizeTier(string shopId)
        {
            ShopExpansionResult result = shopExpansionService.ExpandToNextTier(state, shopId);
            if (!result.Success)
            {
                Events.Publish(new ToastRequestedEvent(result.ErrorMessage));
                return;
            }

            PublishSnapshot();
            Events.Publish(new RestaurantDetailRequestedEvent(shopId));
            Events.Publish(new ToastRequestedEvent($"Ausbau auf {ShopSizing.Label(result.NewTier)} abgeschlossen."));
        }

        public void BuyEquipment(string shopId, string equipmentId)
        {
            EquipmentPurchaseResult result = equipmentPurchaseService.BuyEquipment(state, shopId, equipmentId);
            if (!result.Success)
            {
                Events.Publish(new ToastRequestedEvent(result.ErrorMessage));
                return;
            }

            PublishSnapshot();
            Events.Publish(new RestaurantDetailRequestedEvent(shopId));
            Events.Publish(new ToastRequestedEvent("Equipment installiert."));
        }

        public void HireEmployee(string shopId, string employeeId)
        {
            EmployeeHiringResult result = employeeHiringService.HireEmployee(state, shopId, employeeId);
            if (!result.Success)
            {
                Events.Publish(new ToastRequestedEvent(result.ErrorMessage));
                return;
            }

            PublishSnapshot();
            Events.Publish(new RestaurantDetailRequestedEvent(shopId));
            Events.Publish(new ToastRequestedEvent("Mitarbeiter eingestellt."));
        }

        public void StartShopCampaign(string shopId, string campaignId)
        {
            ShopCampaignResult result = shopCampaignService.StartShopCampaign(state, shopId, campaignId);
            if (!result.Success)
            {
                Events.Publish(new ToastRequestedEvent(result.ErrorMessage));
                return;
            }

            PublishSnapshot();
            Events.Publish(new RestaurantDetailRequestedEvent(shopId));
            Events.Publish(new ToastRequestedEvent("Kampagne gestartet."));
        }

        public void SimulateDay()
        {
            DayResult result = DayProcessing.ProcessDay(state);
            Events.Publish(new DayEndedEvent(result.Record, result));
            PublishSnapshot();
        }

        private static string CityIdFor(CityMapHotspot hotspot)
        {
            string district = hotspot.District ?? string.Empty;
            CityData city = GameData.AllCities.FirstOrDefault(candidate =>
                district.StartsWith(candidate.Name, StringComparison.OrdinalIgnoreCase));
            return city?.Id ?? "fulda";
        }
    }
}
