using System.Collections.Generic;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using UnityEngine;

namespace DoenerEmpire.View3D
{
    public sealed class CityMapView : MonoBehaviour
    {
        private readonly List<CityMapHotspot> hotspots = new();
        private GameState gameState;
        private Camera mapCamera;
        private CityMapSelection selection;

        public IReadOnlyList<CityMapHotspot> Hotspots => hotspots;

        public void Initialize(GameState state, CityMapSelection newSelection, Camera camera)
        {
            gameState = state;
            selection = newSelection;
            mapCamera = camera;
            BuildMapBase();
            BuildHotspots();
        }

        private void Update()
        {
            if (mapCamera == null || !Input.GetMouseButtonDown(0))
            {
                return;
            }

            Ray ray = mapCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                CityMapHotspot hotspot = hit.collider.GetComponentInParent<CityMapHotspot>();
                if (hotspot != null)
                {
                    selection.Select(hotspot);
                }
            }
        }

        private void BuildMapBase()
        {
            Material groundMaterial = MaterialFor(new Color(0.09f, 0.07f, 0.055f));
            Material roadMaterial = MaterialFor(new Color(0.18f, 0.14f, 0.11f));
            Material buildingMaterial = MaterialFor(new Color(0.22f, 0.17f, 0.13f));

            CreateCube("City Base", new Vector3(0f, -0.12f, 0f), new Vector3(13f, 0.18f, 11f), groundMaterial);
            CreateCube("North Road", new Vector3(-0.4f, 0.02f, 0f), new Vector3(1.0f, 0.05f, 10f), roadMaterial);
            CreateCube("Market Road", new Vector3(0.5f, 0.03f, -0.6f), new Vector3(11.5f, 0.05f, 0.9f), roadMaterial);

            Vector3[] buildingPositions =
            {
                new(-5.1f, 0.45f, 3.4f), new(-3.4f, 0.55f, -3.5f),
                new(-1.2f, 0.75f, -3.2f), new(2.8f, 0.45f, 3.3f),
                new(4.8f, 0.7f, 3.5f), new(3.7f, 0.9f, -2.0f),
                new(5.1f, 0.5f, -0.8f), new(-4.7f, 0.65f, 0.7f),
            };

            for (int index = 0; index < buildingPositions.Length; index++)
            {
                float height = 0.7f + (index % 3) * 0.35f;
                CreateCube($"Background Building {index + 1}", buildingPositions[index], new Vector3(0.95f, height, 0.95f), buildingMaterial);
            }
        }

        private void BuildHotspots()
        {
            CityData city = GameData.AllCities[0];
            Shop ownedShop = gameState.Shops.Count > 0 ? gameState.Shops[0] : CreateOwnedShop(city);
            Competitor competitor = gameState.Competitors.Count > 0 ? gameState.Competitors[0] : CreateCompetitor(city);

            AddHotspot(ownedShop.Id, DisplayNameFor(ownedShop), $"{city.Name} - {ownedShop.LocationName}", CityMapHotspotState.Owned,
                new Vector3(-2.8f, 0.35f, -1.4f), ownedShop.FootTraffic, ownedShop.WeeklyRent, 0, ownedShop.Personality, ownedShop, null);
            AddHotspot("available_marktplatz", "Marktplatz", "Fulda - Innenstadt", CityMapHotspotState.Available,
                new Vector3(1.2f, 0.35f, -0.5f), 5400, 1560, 4200, LocationPersonality.Touristic, null, null);
            AddHotspot("available_bahnhof", "Bahnhofsnaehe", "Fulda - Transit", CityMapHotspotState.Available,
                new Vector3(-4.4f, 0.35f, 1.5f), 4050, 1080, 3000, LocationPersonality.Transit, null, null);
            AddHotspot("locked_koeln_uni", "Uni-Viertel", "Koeln - Hochschul-Lage", CityMapHotspotState.Locked,
                new Vector3(4.1f, 0.35f, -3.2f), 24000, 5200, 12000, LocationPersonality.University, null, null);
            AddHotspot(competitor.Id, competitor.Name, $"{city.Name} - Konkurrenz", CityMapHotspotState.Competitor,
                new Vector3(4.5f, 0.35f, 2.2f), 5000, 0, 0, LocationPersonality.Touristic, null, competitor);
        }

        private void AddHotspot(
            string id,
            string displayName,
            string district,
            CityMapHotspotState state,
            Vector3 position,
            int footTraffic,
            double weeklyRent,
            double deposit,
            LocationPersonality personality,
            Shop shop,
            Competitor competitor)
        {
            Material material = state switch
            {
                CityMapHotspotState.Owned => MaterialFor(new Color(0.91f, 0.36f, 0.18f)),
                CityMapHotspotState.Available => MaterialFor(new Color(0.92f, 0.88f, 0.78f)),
                CityMapHotspotState.Locked => MaterialFor(new Color(0.20f, 0.15f, 0.12f)),
                CityMapHotspotState.Competitor => MaterialFor(new Color(0.75f, 0.18f, 0.13f)),
                _ => MaterialFor(Color.gray),
            };

            GameObject marker = GameObject.CreatePrimitive(state == CityMapHotspotState.Owned ? PrimitiveType.Cube : PrimitiveType.Cylinder);
            marker.name = $"Hotspot - {displayName}";
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = position;
            marker.transform.localScale = state == CityMapHotspotState.Owned
                ? new Vector3(0.7f, 0.65f, 0.7f)
                : new Vector3(0.48f, 0.42f, 0.48f);

            CityMapHotspot hotspot = marker.AddComponent<CityMapHotspot>();
            hotspot.Initialize(id, displayName, district, state, footTraffic, weeklyRent, deposit, personality, material, shop, competitor);
            hotspots.Add(hotspot);
        }

        private GameObject CreateCube(string objectName, Vector3 position, Vector3 scale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(transform, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private static Material MaterialFor(Color color)
        {
            Material material = new(Shader.Find("Standard"));
            material.color = color;
            return material;
        }

        private static string DisplayNameFor(Shop shop)
        {
            return string.IsNullOrWhiteSpace(shop.CustomName) ? shop.Name : shop.CustomName;
        }

        private static Shop CreateOwnedShop(CityData city)
        {
            return new Shop
            {
                Id = "shop_fulda_hauptstrasse",
                Name = "Doener Empire",
                CustomName = "Hauptstrasse 12",
                CityId = city.Id,
                LocationName = "Hauptstrasse",
                FootTraffic = 4600,
                WeeklyRent = 1200,
                Reputation = 4.4,
                DayOpened = 1,
                Personality = LocationPersonality.Business,
                SizeTier = ShopSizeTier.Klein,
            };
        }

        private static Competitor CreateCompetitor(CityData city)
        {
            return new Competitor
            {
                Id = "competitor_fulda_01",
                Name = "King Doener",
                CityId = city.Id,
                Personality = CompetitorPersonality.Aggressive,
                ShopCount = 1,
                Reputation = 3.8,
                PriceLevel = 0.95,
                MarketShare = 0.22,
            };
        }
    }
}
