using System.Linq;
using DoenerEmpire.App;
using DoenerEmpire.Core;
using DoenerEmpire.Data;
using DoenerEmpire.Models;
using UnityEngine;

namespace DoenerEmpire.UI
{
    public sealed class RestaurantDetailView : MonoBehaviour
    {
        private static readonly Color Scrim = new(0.02f, 0.016f, 0.012f, 0.58f);
        private static readonly Color Surface = new(0.142f, 0.106f, 0.083f, 0.98f);
        private static readonly Color Background = new(0.078f, 0.063f, 0.055f, 0.94f);
        private static readonly Color Header = new(0.19f, 0.13f, 0.09f, 0.98f);
        private static readonly Color AccentMuted = new(0.36f, 0.19f, 0.12f, 0.95f);
        private static readonly Color Success = new(0.25f, 0.58f, 0.36f, 1f);
        private static readonly Color Cream = new(0.98f, 0.96f, 0.91f, 1f);
        private static readonly Color Sand = new(0.77f, 0.71f, 0.63f, 1f);
        private static readonly Color Orange = new(0.91f, 0.36f, 0.18f, 1f);

        private static readonly string[] Tabs = { "Sortiment", "Ausbau", "Equipment", "Personal", "Marketing" };

        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle tabStyle;
        private GUIStyle activeTabStyle;
        private GUIStyle valueStyle;
        private GameState gameState;
        private Shop shop;
        private GameController controller;
        private int selectedTab;

        public void Initialize(GameController controller)
        {
            this.controller = controller;
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

            Rect panel = new(28, 74, Screen.width - 56, Screen.height - 112);
            DrawPanel(panel, Surface);

            Rect header = new(panel.x + 18, panel.y + 16, panel.width - 36, 112);
            DrawHeroHeader(header);

            if (GUI.Button(new Rect(panel.x + panel.width - 154, panel.y + 28, 130, 40), "ZURUECK", buttonStyle))
            {
                Close();
            }

            Rect navigation = new(panel.x + 18, panel.y + 144, 196, panel.height - 166);
            DrawNavigation(navigation);
            Rect content = new(panel.x + 232, panel.y + 144, panel.width - 250, panel.height - 166);
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
                    DrawEquipment(content);
                    break;
                case 3:
                    DrawPersonal(content);
                    break;
                default:
                    DrawMarketing(content);
                    break;
            }
        }

        private void DrawSortiment(Rect content)
        {
            DrawSectionHeader(content, "SORTIMENT", "Preise steuern Nachfrage, Marge und Tagesergebnis.");

            if (shop.Menu == null || shop.Menu.Count == 0)
            {
                DrawEmptyState(new Rect(content.x + 18, content.y + 68, content.width - 36, 92), "Noch keine Menue-Daten im Slice.", "Preis- und Sortimentsaktionen bleiben deaktiviert.");
                return;
            }

            float y = content.y + 72;
            foreach (ShopProduct product in shop.Menu.Take(5))
            {
                DrawProductPriceRow(new Rect(content.x + 18, y, content.width - 36, 58), product);
                y += 68;
            }
        }

        private void DrawProductPriceRow(Rect rect, ShopProduct product)
        {
            DrawPanel(rect, Surface);
            DrawAccentBar(rect, Orange);
            GUI.Label(new Rect(rect.x + 18, rect.y + 9, rect.width * 0.36f, 20), product.ProductId.ToUpperInvariant(), labelStyle);
            GUI.Label(new Rect(rect.x + 18, rect.y + 31, rect.width * 0.36f, 22), $"{product.Price:n2} EUR", valueStyle);
            GUI.Label(new Rect(rect.x + rect.width * 0.38f, rect.y + 19, rect.width * 0.20f, 24), "Preis feinjustieren", bodyStyle);

            float buttonY = rect.y + 13;
            float buttonWidth = 52;
            float x = rect.x + rect.width - ((buttonWidth + 8) * 4) - 18;
            if (GUI.Button(new Rect(x, buttonY, buttonWidth, 34), "-1", buttonStyle))
            {
                controller.SetProductPrice(shop.Id, product.ProductId, product.Price - 1.0);
            }

            if (GUI.Button(new Rect(x + 60, buttonY, buttonWidth, 34), "-.5", buttonStyle))
            {
                controller.SetProductPrice(shop.Id, product.ProductId, product.Price - 0.5);
            }

            if (GUI.Button(new Rect(x + 120, buttonY, buttonWidth, 34), "+.5", buttonStyle))
            {
                controller.SetProductPrice(shop.Id, product.ProductId, product.Price + 0.5);
            }

            if (GUI.Button(new Rect(x + 180, buttonY, buttonWidth, 34), "+1", buttonStyle))
            {
                controller.SetProductPrice(shop.Id, product.ProductId, product.Price + 1.0);
            }
        }

        private void DrawAusbau(Rect content)
        {
            ShopSizeTier currentTier = shop.SizeTier;
            ShopSizeTier? nextTier = ShopSizing.NextTier(currentTier);
            string nextLabel = nextTier == null ? "MAX" : ShopSizing.Label(nextTier.Value);
            string cost = nextTier == null ? "-" : $"{ShopSizing.ExpansionCost(currentTier):n0} EUR";
            ShopSizeTierConfig config = ShopSizing.ConfigFor(currentTier);
            bool canUpgrade = nextTier != null;

            DrawSectionHeader(content, "AUSBAU", "Groessere Flaechen bringen mehr Kapazitaet und hoehere Fixkosten.");
            DrawMetric(new Rect(content.x + 18, content.y + 72, content.width * 0.5f - 24, 64), "AKTUELLE STUFE", ShopSizing.Label(currentTier));
            DrawMetric(new Rect(content.x + content.width * 0.5f + 6, content.y + 72, content.width * 0.5f - 24, 64), "NAECHSTE STUFE", nextLabel);
            DrawMetric(new Rect(content.x + 18, content.y + 148, content.width * 0.5f - 24, 64), "AUSBAUKOSTEN", cost);
            DrawMetric(new Rect(content.x + content.width * 0.5f + 6, content.y + 148, content.width * 0.5f - 24, 64), "PERSONAL-CAP", $"{config.EmployeeCap}");
            GUI.enabled = canUpgrade;
            if (GUI.Button(new Rect(content.x + 18, content.y + 236, 240, 42), "AUSBAUEN", buttonStyle))
            {
                controller.UpgradeShopSizeTier(shop.Id);
            }

            GUI.enabled = true;
            GUI.Label(new Rect(content.x + 278, content.y + 240, content.width - 296, 48), "Controller-Intent: keine direkte UI-Mutation.", bodyStyle);
        }

        private void DrawEquipment(Rect content)
        {
            DrawSectionHeader(content, "EQUIPMENT", "Investitionen machen die Filiale schneller, stabiler und profitabler.");

            float y = content.y + 72;
            foreach (EquipmentData equipment in GameCatalog.AllEquipment.Take(6))
            {
                bool owned = shop.HasEquipment(equipment.Id);
                DrawEquipmentRow(new Rect(content.x + 18, y, content.width - 36, 58), equipment, owned);
                y += 68;
            }
        }

        private void DrawEquipmentRow(Rect rect, EquipmentData equipment, bool owned)
        {
            DrawPanel(rect, Surface);
            DrawAccentBar(rect, owned ? Success : Orange);
            GUI.Label(new Rect(rect.x + 18, rect.y + 9, rect.width * 0.40f, 20), equipment.Name.ToUpperInvariant(), labelStyle);
            GUI.Label(new Rect(rect.x + 18, rect.y + 31, rect.width * 0.40f, 22), $"{equipment.Price:n0} EUR", valueStyle);
            DrawBadge(new Rect(rect.x + rect.width * 0.50f, rect.y + 14, 132, 30), owned ? "INSTALLIERT" : equipment.Category.ToString().ToUpperInvariant(), owned ? Success : AccentMuted);

            GUI.enabled = !owned;
            if (GUI.Button(new Rect(rect.x + rect.width - 144, rect.y + 12, 124, 36), "KAUFEN", buttonStyle))
            {
                controller.BuyEquipment(shop.Id, equipment.Id);
            }

            GUI.enabled = true;
        }

        private void DrawPersonal(Rect content)
        {
            DrawSectionHeader(content, "PERSONAL", "Team-Staerke entscheidet ueber Kapazitaet, Ruf und Wachstum.");
            DrawMetric(new Rect(content.x + 18, content.y + 72, content.width * 0.5f - 24, 64), "TEAM", $"{shop.Employees.Count}/{EmployeeCapFor(shop)}");
            DrawMetric(new Rect(content.x + content.width * 0.5f + 6, content.y + 72, content.width * 0.5f - 24, 64), "BEWERBERPOOL", $"{gameState.EmployeePool.Count}");

            if (gameState.EmployeePool.Count == 0)
            {
                DrawEmptyState(new Rect(content.x + 18, content.y + 154, content.width - 36, 92), "Aktuell keine Bewerber im Pool.", "Nach ein paar Tagen rotiert der Arbeitsmarkt weiter.");
                return;
            }

            float y = content.y + 154;
            foreach (Employee candidate in gameState.EmployeePool.Take(5).ToList())
            {
                DrawCandidateRow(new Rect(content.x + 18, y, content.width - 36, 58), candidate);
                y += 68;
            }
        }

        private void DrawCandidateRow(Rect rect, Employee candidate)
        {
            DrawPanel(rect, Surface);
            string name = string.IsNullOrWhiteSpace(candidate.Name) ? candidate.Id : candidate.Name;
            double hiringCost = candidate.SalaryPerDay * 1.25;
            DrawAccentBar(rect, Orange);
            GUI.Label(new Rect(rect.x + 18, rect.y + 9, rect.width * 0.40f, 20), name.ToUpperInvariant(), labelStyle);
            GUI.Label(new Rect(rect.x + 18, rect.y + 31, rect.width * 0.40f, 22), $"{candidate.TypeId} - {candidate.SalaryPerDay:n0} EUR/Tag", bodyStyle);
            GUI.Label(new Rect(rect.x + rect.width * 0.48f, rect.y + 18, rect.width * 0.24f, 26), $"Kosten {hiringCost:n0} EUR", valueStyle);

            if (GUI.Button(new Rect(rect.x + rect.width - 144, rect.y + 12, 124, 36), "EINSTELLEN", buttonStyle))
            {
                controller.HireEmployee(shop.Id, candidate.Id);
            }
        }

        private void DrawMarketing(Rect content)
        {
            DrawSectionHeader(content, "MARKETING", "Kampagnen erzeugen Druck auf Nachfrage, Ruf und lokale Sichtbarkeit.");

            float y = content.y + 72;
            foreach (MarketingCampaign campaign in MarketingCatalog.ShopCampaigns.Take(6))
            {
                bool active = shop.ActiveCampaigns.Any(current =>
                    current.CampaignId == campaign.Id && current.IsActive(gameState.CurrentDay));
                DrawCampaignRow(new Rect(content.x + 18, y, content.width - 36, 58), campaign, active);
                y += 68;
            }
        }

        private void DrawCampaignRow(Rect rect, MarketingCampaign campaign, bool active)
        {
            DrawPanel(rect, Surface);
            DrawAccentBar(rect, active ? Success : Orange);
            GUI.Label(new Rect(rect.x + 18, rect.y + 9, rect.width * 0.40f, 20), campaign.Name.ToUpperInvariant(), labelStyle);
            GUI.Label(new Rect(rect.x + 18, rect.y + 31, rect.width * 0.40f, 22), $"{campaign.Cost:n0} EUR - {campaign.DurationDays} Tage", bodyStyle);
            DrawBadge(new Rect(rect.x + rect.width * 0.48f, rect.y + 14, 132, 30), active ? "AKTIV" : $"+{campaign.CustomerBoost:P0} Kunden", active ? Success : AccentMuted);

            GUI.enabled = !active;
            if (GUI.Button(new Rect(rect.x + rect.width - 144, rect.y + 12, 124, 36), "STARTEN", buttonStyle))
            {
                controller.StartShopCampaign(shop.Id, campaign.Id);
            }

            GUI.enabled = true;
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
            DrawAccentBar(rect, AccentMuted);
            GUI.Label(new Rect(rect.x + 16, rect.y + 9, rect.width - 32, 18), label.ToUpperInvariant(), labelStyle);
            GUI.Label(new Rect(rect.x + 16, rect.y + 31, rect.width - 32, 24), value, valueStyle);
        }

        private void DrawHeroHeader(Rect rect)
        {
            DrawPanel(rect, Header);
            DrawAccentBar(rect, Orange);
            GUI.Label(new Rect(rect.x + 24, rect.y + 16, rect.width - 230, 34), shop.DisplayName, titleStyle);
            GUI.Label(new Rect(rect.x + 24, rect.y + 54, rect.width - 230, 24), $"{shop.CityId} - {shop.LocationName}", bodyStyle);

            float tileWidth = (rect.width - 280) / 4f;
            float y = rect.y + 76;
            DrawCompactMetric(new Rect(rect.x + 24, y, tileWidth, 28), "Cash", $"{gameState.Cash:n0} EUR");
            DrawCompactMetric(new Rect(rect.x + 32 + tileWidth, y, tileWidth, 28), "Ruf", $"{shop.Reputation:n1}");
            DrawCompactMetric(new Rect(rect.x + 40 + tileWidth * 2, y, tileWidth, 28), "Team", $"{shop.Employees.Count}/{EmployeeCapFor(shop)}");
            DrawCompactMetric(new Rect(rect.x + 48 + tileWidth * 3, y, tileWidth, 28), "Tag", $"{gameState.CurrentDay}");
        }

        private void DrawNavigation(Rect rect)
        {
            DrawPanel(rect, Background);
            GUI.Label(new Rect(rect.x + 16, rect.y + 14, rect.width - 32, 24), "MANAGEMENT", labelStyle);
            float y = rect.y + 50;
            for (int i = 0; i < Tabs.Length; i++)
            {
                if (DrawTabButton(new Rect(rect.x + 14, y, rect.width - 28, 42), i))
                {
                    selectedTab = i;
                }

                y += 50;
            }

            DrawPanel(new Rect(rect.x + 14, rect.y + rect.height - 92, rect.width - 28, 76), Header);
            GUI.Label(new Rect(rect.x + 28, rect.y + rect.height - 78, rect.width - 56, 20), "AKTIVE KAMPAGNEN", labelStyle);
            GUI.Label(new Rect(rect.x + 28, rect.y + rect.height - 52, rect.width - 56, 24), $"{shop.ActiveCampaigns.Count(active => active.IsActive(gameState.CurrentDay))}", valueStyle);
        }

        private bool DrawTabButton(Rect rect, int index)
        {
            bool active = selectedTab == index;
            DrawPanel(rect, active ? AccentMuted : Surface);
            DrawAccentBar(rect, active ? Orange : Background);
            return GUI.Button(rect, Tabs[index].ToUpperInvariant(), active ? activeTabStyle : tabStyle);
        }

        private void DrawSectionHeader(Rect content, string title, string subtitle)
        {
            GUI.Label(new Rect(content.x + 18, content.y + 14, content.width - 36, 24), title, labelStyle);
            GUI.Label(new Rect(content.x + 18, content.y + 40, content.width - 36, 22), subtitle, bodyStyle);
        }

        private void DrawCompactMetric(Rect rect, string label, string value)
        {
            GUI.Label(new Rect(rect.x, rect.y, rect.width * 0.38f, rect.height), label.ToUpperInvariant(), labelStyle);
            GUI.Label(new Rect(rect.x + rect.width * 0.40f, rect.y, rect.width * 0.60f, rect.height), value, bodyStyle);
        }

        private void DrawBadge(Rect rect, string text, Color color)
        {
            DrawPanel(rect, color);
            GUI.Label(new Rect(rect.x + 10, rect.y + 6, rect.width - 20, rect.height - 12), text, labelStyle);
        }

        private void DrawEmptyState(Rect rect, string title, string body)
        {
            DrawPanel(rect, Header);
            DrawAccentBar(rect, AccentMuted);
            GUI.Label(new Rect(rect.x + 18, rect.y + 16, rect.width - 36, 22), title, labelStyle);
            GUI.Label(new Rect(rect.x + 18, rect.y + 44, rect.width - 36, 36), body, bodyStyle);
        }

        private static void DrawAccentBar(Rect rect, Color color)
        {
            DrawPanel(new Rect(rect.x, rect.y, 5, rect.height), color);
        }

        private static int EmployeeCapFor(Shop shop)
        {
            CityData city = GameData.AllCities.FirstOrDefault(candidate => candidate.Id == shop.CityId);
            CityTier cityTier = city?.Tier ?? CityTier.Klein;
            return ShopSizing.EmployeeCap(cityTier, shop.SizeTier);
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

            tabStyle = new GUIStyle(GUI.skin.button);
            tabStyle.fontSize = 14;
            tabStyle.alignment = TextAnchor.MiddleLeft;
            tabStyle.fontStyle = FontStyle.Bold;
            tabStyle.normal.textColor = Sand;

            activeTabStyle = new GUIStyle(tabStyle);
            activeTabStyle.normal.textColor = Cream;

            valueStyle = new GUIStyle(GUI.skin.label);
            valueStyle.fontSize = 18;
            valueStyle.fontStyle = FontStyle.Bold;
            valueStyle.normal.textColor = Cream;
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
