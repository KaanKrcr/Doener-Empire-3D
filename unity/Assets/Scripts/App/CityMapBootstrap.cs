using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using DoenerEmpire.UI;
using DoenerEmpire.View3D;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DoenerEmpire.App
{
    public sealed class CityMapBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapCityMapScene()
        {
            if (SceneManager.GetActiveScene().name != "CityMap" || FindObjectOfType<CityMapBootstrap>() != null)
            {
                return;
            }

            new GameObject("CityMap Bootstrap").AddComponent<CityMapBootstrap>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;

            GameState state = CreateDummyState();
            CityMapSelection selection = new();
            Camera camera = CreateCamera();
            CreateLights();

            CityMapView mapView = new GameObject("CityMap View").AddComponent<CityMapView>();
            mapView.Initialize(state, selection, camera);

            LocationSheetView locationSheet = new GameObject("LocationSheet UI").AddComponent<LocationSheetView>();
            locationSheet.Initialize(selection, state);

            if (mapView.Hotspots.Count > 0)
            {
                selection.Select(mapView.Hotspots[0]);
            }
        }

        private static GameState CreateDummyState()
        {
            GameState state = GameState.Initial("Doener Empire", "Kaan", GameData.StartingCash);
            state.Shops.Add(new Shop
            {
                Id = "shop_fulda_hauptstrasse",
                Name = "Doener Empire",
                CustomName = "Hauptstrasse 12",
                CityId = "fulda",
                LocationName = "Hauptstrasse",
                FootTraffic = 4600,
                WeeklyRent = 1200,
                Reputation = 4.4,
                DayOpened = 1,
                Personality = LocationPersonality.Business,
                SizeTier = ShopSizeTier.Klein,
            });
            state.Competitors.Add(new Competitor
            {
                Id = "competitor_fulda_01",
                Name = "King Doener",
                CityId = "fulda",
                Personality = CompetitorPersonality.Aggressive,
                ShopCount = 1,
                Reputation = 3.8,
                PriceLevel = 0.95,
                MarketShare = 0.22,
            });
            return state;
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new("CityMap Isometric Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 6.8f;
            camera.backgroundColor = new Color(0.078f, 0.063f, 0.055f);
            camera.transform.position = new Vector3(-7f, 8f, -7f);
            camera.transform.rotation = Quaternion.Euler(30f, 45f, 0f);
            cameraObject.AddComponent<CityMapCameraController>();
            return camera;
        }

        private static void CreateLights()
        {
            GameObject keyObject = new("Warm Map Key Light");
            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, 0.72f, 0.45f);
            key.intensity = 1.2f;
            key.transform.rotation = Quaternion.Euler(48f, -32f, 12f);

            GameObject fillObject = new("Soft City Fill Light");
            Light fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.color = new Color(0.95f, 0.35f, 0.15f);
            fill.intensity = 1.0f;
            fill.range = 16f;
            fill.transform.position = new Vector3(0f, 4.5f, -1f);
        }
    }
}
