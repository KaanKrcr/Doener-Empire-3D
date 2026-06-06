using DoenerEmpire.App;
using DoenerEmpire.Models;
using DoenerEmpire.View3D;
using UnityEngine;

namespace DoenerEmpire.UI
{
    public sealed class BuyDialogView : MonoBehaviour
    {
        private static readonly Color Scrim = new(0.02f, 0.016f, 0.012f, 0.50f);
        private static readonly Color Surface = new(0.142f, 0.106f, 0.083f, 0.98f);
        private static readonly Color Background = new(0.078f, 0.063f, 0.055f, 0.94f);
        private static readonly Color Cream = new(0.98f, 0.96f, 0.91f, 1f);
        private static readonly Color Sand = new(0.77f, 0.71f, 0.63f, 1f);
        private static readonly Color Orange = new(0.91f, 0.36f, 0.18f, 1f);

        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;
        private GUIStyle disabledButtonStyle;
        private CityMapHotspot hotspot;
        private GameState gameState;
        private GameController controller;

        public void Initialize(GameController controller)
        {
            this.controller = controller;
            gameState = controller.State;
            controller.Events.Subscribe<StateSnapshotChangedEvent>(e => gameState = e.State);
            controller.Events.Subscribe<BuyDialogRequestedEvent>(ShowDialog);
            controller.Events.Subscribe<LocationSelectedEvent>(_ => Close());
        }

        private void OnGUI()
        {
            if (hotspot == null)
            {
                return;
            }

            EnsureStyles();
            DrawPanel(new Rect(0, 0, Screen.width, Screen.height), Scrim);

            float width = Mathf.Min(520, Screen.width - 48);
            float height = 320;
            Rect dialog = new(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - height * 0.5f, width, height);
            DrawPanel(dialog, Surface);

            GUI.Label(new Rect(dialog.x + 24, dialog.y + 20, dialog.width - 48, 32), "FILIALE EROEFFNEN", labelStyle);
            GUI.Label(new Rect(dialog.x + 24, dialog.y + 54, dialog.width - 48, 34), hotspot.DisplayName, titleStyle);
            GUI.Label(new Rect(dialog.x + 24, dialog.y + 92, dialog.width - 48, 24), hotspot.District, bodyStyle);

            float metricY = dialog.y + 130;
            float metricWidth = (dialog.width - 60) / 2f;
            DrawMetric(new Rect(dialog.x + 24, metricY, metricWidth, 58), "KAUTION", Money(hotspot.Deposit));
            DrawMetric(new Rect(dialog.x + 36 + metricWidth, metricY, metricWidth, 58), "WOCHENMIETE", Money(hotspot.WeeklyRent));
            DrawMetric(new Rect(dialog.x + 24, metricY + 70, dialog.width - 48, 58), "KAPITAL DANACH", Money(CapitalAfterOpening()));

            if (GUI.Button(new Rect(dialog.x + 24, dialog.y + dialog.height - 56, 150, 38), "ABBRECHEN", buttonStyle))
            {
                Close();
            }

            bool canAfford = CapitalAfterOpening() >= 0;
            GUI.enabled = canAfford;
            if (GUI.Button(new Rect(dialog.x + dialog.width - 214, dialog.y + dialog.height - 56, 190, 38), "EROEFFNEN", canAfford ? buttonStyle : disabledButtonStyle))
            {
                controller.OpenShop(hotspot);
                Close();
            }
            GUI.enabled = true;
        }

        private void ShowDialog(BuyDialogRequestedEvent request)
        {
            hotspot = request.Hotspot?.State == CityMapHotspotState.Available
                ? request.Hotspot
                : null;
        }

        private void Close()
        {
            hotspot = null;
        }

        private void DrawMetric(Rect rect, string label, string value)
        {
            DrawPanel(rect, Background);
            GUI.Label(new Rect(rect.x + 10, rect.y + 7, rect.width - 20, 20), label, labelStyle);
            GUI.Label(new Rect(rect.x + 10, rect.y + 30, rect.width - 20, 24), value, bodyStyle);
        }

        private double CapitalAfterOpening()
        {
            return (gameState?.Cash ?? 0) - hotspot.Deposit - hotspot.WeeklyRent;
        }

        private static string Money(double value)
        {
            return $"{value:n0} EUR";
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

            disabledButtonStyle = new GUIStyle(buttonStyle);
            disabledButtonStyle.normal.textColor = Sand;
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
