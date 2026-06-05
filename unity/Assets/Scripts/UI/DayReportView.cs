using DoenerEmpire.App;
using DoenerEmpire.Models;
using UnityEngine;

namespace DoenerEmpire.UI
{
    public sealed class DayReportView : MonoBehaviour
    {
        private static readonly Color Scrim = new(0.02f, 0.016f, 0.012f, 0.54f);
        private static readonly Color Surface = new(0.142f, 0.106f, 0.083f, 0.98f);
        private static readonly Color Background = new(0.078f, 0.063f, 0.055f, 0.94f);
        private static readonly Color Cream = new(0.98f, 0.96f, 0.91f, 1f);
        private static readonly Color Sand = new(0.77f, 0.71f, 0.63f, 1f);
        private static readonly Color Orange = new(0.91f, 0.36f, 0.18f, 1f);

        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;
        private DailyRecord record;
        private bool visible;

        public void Initialize(GameController controller)
        {
            controller.Events.Subscribe<DayEndedEvent>(ShowReport);
            controller.Events.Subscribe<LocationSelectedEvent>(_ => Close());
        }

        private void OnGUI()
        {
            if (!visible || record == null)
            {
                return;
            }

            EnsureStyles();
            DrawPanel(new Rect(0, 0, Screen.width, Screen.height), Scrim);

            float width = Mathf.Min(560, Screen.width - 48);
            float height = 360;
            Rect panel = new(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - height * 0.5f, width, height);
            DrawPanel(panel, Surface);

            GUI.Label(new Rect(panel.x + 24, panel.y + 22, panel.width - 48, 28), "TAGESBERICHT", labelStyle);
            GUI.Label(new Rect(panel.x + 24, panel.y + 54, panel.width - 48, 34), $"Tag {record.Day}", titleStyle);

            float metricY = panel.y + 104;
            float metricWidth = (panel.width - 72) / 2f;
            DrawMetric(new Rect(panel.x + 24, metricY, metricWidth, 58), "UMSATZ", Money(record.Revenue));
            DrawMetric(new Rect(panel.x + 48 + metricWidth, metricY, metricWidth, 58), "KOSTEN", Money(record.Costs));
            DrawMetric(new Rect(panel.x + 24, metricY + 72, metricWidth, 58), "GEWINN", Money(record.Profit));
            DrawMetric(new Rect(panel.x + 48 + metricWidth, metricY + 72, metricWidth, 58), "KUNDEN", $"{record.Customers:n0}");

            GUI.Label(new Rect(panel.x + 24, panel.y + 252, panel.width - 48, 28), "CityMap wurde ueber Controller-Events aktualisiert.", bodyStyle);

            if (GUI.Button(new Rect(panel.x + panel.width - 174, panel.y + panel.height - 56, 150, 38), "ZURUECK", buttonStyle))
            {
                Close();
            }
        }

        private void ShowReport(DayEndedEvent dayEnded)
        {
            record = dayEnded.Record;
            visible = record != null;
        }

        private void Close()
        {
            visible = false;
        }

        private void DrawMetric(Rect rect, string label, string value)
        {
            DrawPanel(rect, Background);
            GUI.Label(new Rect(rect.x + 10, rect.y + 7, rect.width - 20, 20), label, labelStyle);
            GUI.Label(new Rect(rect.x + 10, rect.y + 30, rect.width - 20, 24), value, bodyStyle);
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
