using System.Linq;
using DoenerEmpire.App;
using DoenerEmpire.Models;
using UnityEngine;

namespace DoenerEmpire.UI
{
    public sealed class RestaurantDetailView : MonoBehaviour
    {
        private static readonly Color Scrim = new(0.02f, 0.016f, 0.012f, 0.58f);
        private static readonly Color Surface = new(0.142f, 0.106f, 0.083f, 0.98f);
        private static readonly Color Background = new(0.078f, 0.063f, 0.055f, 0.94f);
        private static readonly Color Cream = new(0.98f, 0.96f, 0.91f, 1f);
        private static readonly Color Sand = new(0.77f, 0.71f, 0.63f, 1f);
        private static readonly Color Orange = new(0.91f, 0.36f, 0.18f, 1f);

        private static readonly string[] Tabs = { "Sortiment", "Ausbau", "Equipment", "Personal", "Marketing" };

        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;
        private GameState gameState;
        private Shop shop;
        private int selectedTab;

        public void Initialize(GameController controller)
        {
            gameState = controller.State;
            controller.Events.Subscribe<StateSnapshotChangedEvent>(e => gameState = e.State);
            controller.Events.Subscribe<RestaurantDetailRequestedEvent>(ShowDetail);
            controller.Events.Subscribe<LocationSelectedEvent>(_ => Close());
        }

        private void OnGUI()
        {
            if (shop == null)
            {
                return;
            }

            EnsureStyles();
            DrawPanel(new Rect(0, 0, Screen.width, Screen.height), Scrim);

            Rect panel = new(28, 128, Screen.width - 56, Screen.height - 184);
            DrawPanel(panel, Surface);

            GUI.Label(new Rect(panel.x + 24, panel.y + 18, panel.width - 210, 34), shop.DisplayName, titleStyle);
            GUI.Label(new Rect(panel.x + 24, panel.y + 56, panel.width - 220, 24), $"{shop.CityId} - {shop.LocationName}", bodyStyle);

            if (GUI.Button(new Rect(panel.x + panel.width - 154, panel.y + 22, 130, 34), "ZURUECK", buttonStyle))
            {
                Close();
            }

            selectedTab = GUI.Toolbar(new Rect(panel.x + 24, panel.y + 94, panel.width - 48, 34), selectedTab, Tabs);

            Rect content = new(panel.x + 24, panel.y + 138, panel.width - 48, panel.height - 162);
            DrawPanel(content, Background);
            DrawSelectedTab(content);
        }

        private void ShowDetail(RestaurantDetailRequestedEvent request)
        {
            shop = gameState?.Shops?.FirstOrDefault(candidate => candidate.Id == request.ShopId);
            selectedTab = 0;
        }

        private void Close()
        {
            shop = null;
        }

        private void DrawSelectedTab(Rect content)
        {
            switch (selectedTab)
            {
                case 0:
                    DrawSortiment(content);
                    break;
                case 1:
                    DrawAusbau(content);
                    break;
                case 2:
                    DrawReadOnlyStub(content, "EQUIPMENT", "Equipment-Kauf folgt spaeter ueber GameController-Intents.");
                    break;
                case 3:
                    DrawReadOnlyStub(content, "PERSONAL", "Personal einstellen/feuern bleibt in diesem Slice bewusst deaktiviert.");
                    break;
                default:
                    DrawReadOnlyStub(content, "MARKETING", "Kampagnen werden erst nach der Controller-Anbindung aktiv.");
                    break;
            }
        }

        private void DrawSortiment(Rect content)
        {
            GUI.Label(new Rect(content.x + 18, content.y + 14, content.width - 36, 24), "SORTIMENT", labelStyle);

            if (shop.Menu == null || shop.Menu.Count == 0)
            {
                GUI.Label(new Rect(content.x + 18, content.y + 48, content.width - 36, 48), "Noch keine Menue-Daten im Slice. Preis- und Sortimentsaktionen bleiben deaktiviert.", bodyStyle);
                return;
            }

            float y = content.y + 48;
            foreach (ShopProduct product in shop.Menu.Take(5))
            {
                DrawMetric(new Rect(content.x + 18, y, content.width - 36, 44), product.ProductId, $"{product.Price:n2} EUR - read-only");
                y += 52;
            }
        }

        private void DrawAusbau(Rect content)
        {
            ShopSizeTier currentTier = shop.SizeTier;
            ShopSizeTier? nextTier = ShopSizing.NextTier(currentTier);
            string nextLabel = nextTier == null ? "MAX" : ShopSizing.Label(nextTier.Value);
            string cost = nextTier == null ? "-" : $"{ShopSizing.ExpansionCost(currentTier):n0} EUR";
            ShopSizeTierConfig config = ShopSizing.ConfigFor(currentTier);

            GUI.Label(new Rect(content.x + 18, content.y + 14, content.width - 36, 24), "AUSBAU", labelStyle);
            DrawMetric(new Rect(content.x + 18, content.y + 48, content.width * 0.5f - 24, 58), "AKTUELLE STUFE", ShopSizing.Label(currentTier));
            DrawMetric(new Rect(content.x + content.width * 0.5f + 6, content.y + 48, content.width * 0.5f - 24, 58), "NAECHSTE STUFE", nextLabel);
            DrawMetric(new Rect(content.x + 18, content.y + 118, content.width * 0.5f - 24, 58), "AUSBAUKOSTEN", cost);
            DrawMetric(new Rect(content.x + content.width * 0.5f + 6, content.y + 118, content.width * 0.5f - 24, 58), "PERSONAL-CAP", $"{config.EmployeeCap}");
            GUI.Label(new Rect(content.x + 18, content.y + 194, content.width - 36, 48), "Ausbau ist in dieser Shell nur lesbar. Keine SizeTier-, Miete- oder Cash-Mutation.", bodyStyle);
        }

        private void DrawReadOnlyStub(Rect content, string title, string text)
        {
            GUI.Label(new Rect(content.x + 18, content.y + 14, content.width - 36, 24), title, labelStyle);
            GUI.Label(new Rect(content.x + 18, content.y + 48, content.width - 36, 52), text, bodyStyle);
            GUI.Label(new Rect(content.x + 18, content.y + 104, content.width - 36, 30), "READ-ONLY STUB", labelStyle);
        }

        private void DrawMetric(Rect rect, string label, string value)
        {
            DrawPanel(rect, Surface);
            GUI.Label(new Rect(rect.x + 10, rect.y + 7, rect.width - 20, 18), label.ToUpperInvariant(), labelStyle);
            GUI.Label(new Rect(rect.x + 10, rect.y + 26, rect.width - 20, 20), value, bodyStyle);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
            {
                return;
            }

            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 24;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Cream;

            bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.fontSize = 16;
            bodyStyle.normal.textColor = Sand;
            bodyStyle.wordWrap = true;

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 13;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = Orange;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 15;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.normal.textColor = Cream;
        }

        private static void DrawPanel(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
