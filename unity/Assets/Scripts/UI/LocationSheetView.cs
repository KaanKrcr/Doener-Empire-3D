using DoenerEmpire.App;
using DoenerEmpire.View3D;
using DoenerEmpire.Models;
using UnityEngine;

namespace DoenerEmpire.UI
{
    public sealed class LocationSheetView : MonoBehaviour
    {
        private static readonly Color Background = new(0.078f, 0.063f, 0.055f, 0.92f);
        private static readonly Color Surface = new(0.142f, 0.106f, 0.083f, 0.98f);
        private static readonly Color Cream = new(0.98f, 0.96f, 0.91f, 1f);
        private static readonly Color Sand = new(0.77f, 0.71f, 0.63f, 1f);
        private static readonly Color Orange = new(0.91f, 0.36f, 0.18f, 1f);

        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;
        private CityMapHotspot selected;
        private GameState gameState;
        private GameController controller;
        private string toastText;
        private float toastUntil;

        public void Initialize(GameController gameController)
        {
            controller = gameController;
            gameState = controller.State;
            controller.Events.Subscribe<StateSnapshotChangedEvent>(e => gameState = e.State);
            controller.Events.Subscribe<LocationSelectedEvent>(e => selected = e.Hotspot);
            controller.Events.Subscribe<ToastRequestedEvent>(ShowToast);
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawHud();

            if (selected != null)
            {
                DrawLocationSheet();
            }

            DrawToast();
        }

        private void DrawHud()
        {
            Rect topRect = new(24, 22, Screen.width - 48, 92);
            DrawPanel(topRect, Background);
            GUI.Label(new Rect(topRect.x + 22, topRect.y + 14, topRect.width * 0.55f, 32), CompanyLabel(), titleStyle);
            GUI.Label(new Rect(topRect.x + 22, topRect.y + 52, 360, 26), $"Fulda City Map - Tag {DayLabel()}", bodyStyle);
            if (GUI.Button(new Rect(topRect.x + topRect.width - 390, topRect.y + 28, 150, 34), "TAG BEENDEN", buttonStyle))
            {
                controller.SimulateDay();
            }

            GUI.Label(new Rect(topRect.x + topRect.width - 220, topRect.y + 28, 200, 34), CashLabel(), titleStyle);
        }

        private void DrawLocationSheet()
        {
            float width = Screen.width - 48;
            float height = Mathf.Min(330, Screen.height * 0.36f);
            Rect sheet = new(24, Screen.height - height - 24, width, height);
            DrawPanel(sheet, Surface);

            GUI.Label(new Rect(sheet.x + 24, sheet.y + 20, sheet.width * 0.62f, 34), selected.DisplayName, titleStyle);
            GUI.Label(new Rect(sheet.x + 24, sheet.y + 58, sheet.width * 0.70f, 26), $"{selected.District} - {StatusLabel(selected.State)}", bodyStyle);
            GUI.Label(new Rect(sheet.x + sheet.width - 190, sheet.y + 22, 160, 30), StatusLabel(selected.State).ToUpperInvariant(), labelStyle);

            float tileY = sheet.y + 100;
            float tileWidth = (sheet.width - 60) / 4f;
            for (int index = 0; index < 4; index++)
            {
                DrawMetric(
                    new Rect(sheet.x + 24 + tileWidth * index, tileY, tileWidth, 74),
                    MetricLabel(index),
                    MetricValue(index));
            }

            GUI.Label(new Rect(sheet.x + 24, sheet.y + 192, sheet.width - 48, 48), RecommendationText(), bodyStyle);

            if (GUI.Button(new Rect(sheet.x + 24, sheet.y + sheet.height - 62, sheet.width - 48, 42), ActionText(), buttonStyle))
            {
                RequestPrimaryAction();
            }
        }

        private void DrawMetric(Rect rect, string label, string value)
        {
            DrawPanel(rect, Background);
            GUI.Label(new Rect(rect.x + 10, rect.y + 8, rect.width - 20, 22), label, labelStyle);
            GUI.Label(new Rect(rect.x + 10, rect.y + 36, rect.width - 20, 28), value, bodyStyle);
        }

        private string MetricLabel(int index)
        {
            return selected.State switch
            {
                CityMapHotspotState.Owned => index switch
                {
                    0 => "MARKTANTEIL",
                    1 => "TRAFFIC",
                    2 => "MIETE",
                    _ => "PROGNOSE",
                },
                CityMapHotspotState.Available => index switch
                {
                    0 => "TRAFFIC",
                    1 => "MIETE",
                    2 => "KAUTION",
                    _ => "KONKURRENZ",
                },
                CityMapHotspotState.Competitor => index switch
                {
                    0 => "KAUTION",
                    1 => "TRAFFIC",
                    2 => "MIETE",
                    _ => "MARKTANTEIL",
                },
                _ => index switch
                {
                    0 => "KAUTION",
                    1 => "TRAFFIC",
                    2 => "MIETE",
                    _ => "SPERRE",
                },
            };
        }

        private string MetricValue(int index)
        {
            return MetricLabel(index) switch
            {
                "MARKTANTEIL" => $"{selected.MarketShare:P0}",
                "TRAFFIC" => $"{selected.FootTraffic:n0}/Tag",
                "MIETE" => selected.WeeklyRent <= 0 ? "-" : $"{selected.WeeklyRent:n0} EUR/Wo",
                "KAUTION" => selected.Deposit <= 0 ? "-" : $"{selected.Deposit:n0} EUR",
                "PROGNOSE" => "stabil",
                "KONKURRENZ" => CompetitionValue(),
                _ => selected.State == CityMapHotspotState.Locked ? "hoch" : "mittel",
            };
        }

        private string CompetitionValue()
        {
            if (selected.State == CityMapHotspotState.Competitor)
            {
                return $"{selected.MarketShare:P0}";
            }

            return "mittel";
        }

        private string RecommendationText()
        {
            return selected.State switch
            {
                CityMapHotspotState.Owned => "Aktive Filiale: Reputation und Lage beobachten. Optimieren ist spaeter an GameController gebunden.",
                CityMapHotspotState.Available => "Freier Standort: Traffic, Miete und Kaution pruefen. Kaufen laeuft als Controller-Intent.",
                CityMapHotspotState.Locked => "Gesperrte Lage: Erst durch Umsatz oder Stadtfreischaltung verfuegbar.",
                CityMapHotspotState.Competitor => "Konkurrenzstandort: Preisniveau, Ruf und Marktanteil nur lesen.",
                _ => string.Empty,
            };
        }

        private string ActionText()
        {
            return selected.State switch
            {
                CityMapHotspotState.Owned => "OPTIMIEREN",
                CityMapHotspotState.Available => "FILIALE EROEFFNEN",
                CityMapHotspotState.Locked => "GESPERRT",
                CityMapHotspotState.Competitor => "KONKURRENZ INFO",
                _ => "AUSWAHL",
            };
        }

        private void RequestPrimaryAction()
        {
            if (selected == null)
            {
                return;
            }

            switch (selected.State)
            {
                case CityMapHotspotState.Owned:
                    controller.RequestRestaurantDetail(selected);
                    break;
                case CityMapHotspotState.Available:
                    controller.RequestBuyDialog(selected);
                    break;
                case CityMapHotspotState.Locked:
                    controller.SelectLocation(selected);
                    break;
                default:
                    ShowToast(new ToastRequestedEvent("Dieser Standort ist im Vertical Slice nur lesbar."));
                    break;
            }
        }

        private void ShowToast(ToastRequestedEvent toast)
        {
            toastText = toast.Message;
            toastUntil = Time.realtimeSinceStartup + 2.2f;
        }

        private void DrawToast()
        {
            if (string.IsNullOrWhiteSpace(toastText) || Time.realtimeSinceStartup > toastUntil)
            {
                return;
            }

            Rect rect = new(Screen.width * 0.5f - 210, 126, 420, 44);
            DrawPanel(rect, Background);
            GUI.Label(new Rect(rect.x + 14, rect.y + 10, rect.width - 28, 24), toastText, bodyStyle);
        }

        private string CompanyLabel()
        {
            return string.IsNullOrWhiteSpace(gameState?.CompanyName)
                ? "DOENER EMPIRE"
                : gameState.CompanyName.ToUpperInvariant();
        }

        private int DayLabel()
        {
            return gameState?.CurrentDay ?? 1;
        }

        private string CashLabel()
        {
            return $"{(gameState?.Cash ?? 0):n0} EUR";
        }

        private static string StatusLabel(CityMapHotspotState state)
        {
            return state switch
            {
                CityMapHotspotState.Owned => "owned",
                CityMapHotspotState.Available => "available",
                CityMapHotspotState.Locked => "locked",
                CityMapHotspotState.Competitor => "competitor",
                _ => "unknown",
            };
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
            labelStyle.alignment = TextAnchor.MiddleRight;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 16;
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
