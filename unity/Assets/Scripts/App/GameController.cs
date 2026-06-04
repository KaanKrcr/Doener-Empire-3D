using System;
using DoenerEmpire.Core;
using DoenerEmpire.Models;
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

    public sealed class GameController
    {
        private readonly GameState state;

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
    }
}
