# REVIEW_QUEUE

Status: open

## Open Review Items

### Unity RestaurantDetail premium console UI review

Owner: Claude Code
Target commit: `df11d69` (`Improve RestaurantDetail premium console UI`)

Scope:
- Pruefen, dass `RestaurantDetailView` als hochwertigere Management-Konsole
  funktioniert: Hero-Header, KPI-Zeile, linke Tab-Navigation, klarere
  Decision-Rows und Status-Badges.
- Pruefen, dass die UI weiterhin nur bestehende Controller-Intents feuert:
  `SetProductPrice`, `UpgradeShopSizeTier`, `BuyEquipment`, `HireEmployee` und
  `StartShopCampaign`.
- Pruefen, dass keine neue Wirtschaftsmutation, keine neuen Services, keine
  Save-/PlayerPrefs-/Filesystem-Logik und keine Arcade-/Realtime-Serving-/
  CustomerSpawner-/manuelle Koch-/First-/Third-Person-Systeme eingefuehrt
  wurden.
- Pruefen, dass die neue IMGUI-Struktur ohne Layout-Offensichtlichkeiten
  lesbar bleibt: Touch-Ziele groesser, Preise/KPIs deutlich, aktive Tabs und
  aktive Kampagnen visuell erkennbar.

Akzeptanzkriterien:
- `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
  ist gruen.
- Scope-Scan bestaetigt: UI-only-Slice, keine neuen gesperrten Systeme.
- Bei Akzeptanz Queue wieder auf `Status: empty` setzen und Ergebnis in
  `STATUS.md`/`HANDOFF_LOG.md` dokumentieren.

## Rules
- Claude Code writes concrete review items here and sets `Status: open`.
- Codex implements only open review items, then sets `Status: empty`.
- If no review items exist, keep `Status: empty`.
