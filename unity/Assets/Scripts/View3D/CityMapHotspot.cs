using DoenerEmpire.Core;
using DoenerEmpire.Models;
using UnityEngine;

namespace DoenerEmpire.View3D
{
    public sealed class CityMapHotspot : MonoBehaviour
    {
        public string Id { get; private set; }
        public string DisplayName { get; private set; }
        public string District { get; private set; }
        public CityMapHotspotState State { get; private set; }
        public int FootTraffic { get; private set; }
        public double WeeklyRent { get; private set; }
        public double Deposit { get; private set; }
        public double Reputation { get; private set; }
        public double MarketShare { get; private set; }
        public LocationPersonality Personality { get; private set; }

        private Vector3 baseScale;

        public void Initialize(
            string id,
            string displayName,
            string district,
            CityMapHotspotState state,
            int footTraffic,
            double weeklyRent,
            double deposit,
            LocationPersonality personality,
            Material material,
            Shop shop = null,
            Competitor competitor = null)
        {
            Id = id;
            DisplayName = displayName;
            District = district;
            State = state;
            FootTraffic = footTraffic;
            WeeklyRent = weeklyRent;
            Deposit = deposit;
            Personality = personality;
            Reputation = shop?.Reputation ?? competitor?.Reputation ?? (state == CityMapHotspotState.Locked ? 0.0 : 3.2);
            MarketShare = competitor?.MarketShare ?? (state == CityMapHotspotState.Owned ? 0.18 : 0.0);
            baseScale = transform.localScale;

            Renderer markerRenderer = GetComponentInChildren<Renderer>();
            if (markerRenderer != null)
            {
                markerRenderer.sharedMaterial = material;
            }
        }

        public void SetSelected(bool selected)
        {
            transform.localScale = selected ? baseScale * 1.16f : baseScale;
        }
    }
}
