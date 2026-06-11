# STATUS

## Overall
Unity-Port laeuft. Logik-Layer wird verifiziert (`dotnet test`) portiert; UX-Spec
fuer den Vertical Slice ist geschrieben.

## Product Direction
Unity Management-/Progression-Spiel mit Premium 2.5D/3D City Map.
Arcade Cooking ist verworfen (`docs/UNITY_MVP_ARCADE_PLAN.md` = DEPRECATED).

## Claude Code (Planner/Reviewer)
State: implementation complete - RestaurantDetail premium console UI pending review (2026-06-11 17:48)
Done:
- Current run 2026-06-11 17:48: Kaan hat den Start fuer UI gegeben und
  verlangt, dass Codex GitHub-Skills durchsucht und sichtbar besseres UI baut.
  Passender GitHub-Skill recherchiert: `ui-ux-pro-max` (UI/UX Design
  Intelligence). Verwendete Leitlinien: klare Informationshierarchie, groessere
  Touch-Ziele, bessere Status-Zustaende, Management-Konsole statt technischer
  Listen. `RestaurantDetailView` wurde als erster sichtbarer UI-Slice
  ueberarbeitet: Hero-Header mit Cash/Ruf/Team/Tag, linke Management-
  Navigation, deutlichere KPI-Kacheln, Decision-Rows mit Accent-Bar,
  Status-Badges fuer installierte/aktive Optionen und klarere Abschnitts-
  Unterzeilen. Keine neue Wirtschaftsmutation und keine neuen Services; die UI
  feuert weiterhin nur bestehende Controller-Intents. Tests gruen; Review-Item
  "Unity RestaurantDetail premium console UI review" gesetzt.
- Current run 2026-06-11 17:39: Offenes Queue-Item "Unity RestaurantDetail
  shop marketing controller flow review" fuer Commit `7bbb7bf` geprueft.
  Ergebnis: akzeptiert. `GameController.StartShopCampaign(shopId, campaignId)`
  ist die zentrale Shop-Marketing-Mutationsgrenze; bei Fehlern wird nur ein
  Toast publiziert, bei Erfolg Snapshot, RestaurantDetail-Refresh und Toast.
  `RestaurantDetailView` ruft im Marketing-Tab ausschliesslich diesen
  Controller-Intent fuer `MarketingCatalog.ShopCampaigns` auf und mutiert
  `GameState`, `Shop`, Cash, ActiveCampaigns, Save-State, Dateien oder Hotspots
  nicht direkt. `ShopCampaignService` bleibt UnityEngine-frei, validiert State,
  Shop, CampaignId, Scope `Shop`, Duplicate/aktive Kampagne und Cash fuer die
  Katalogkosten. Erfolg zieht genau die Katalogkosten ab und haengt genau eine
  `ActiveCampaign` mit `StartDay = CurrentDay` und `EndDay = CurrentDay +
  DurationDays` an `Shop.ActiveCampaigns`. Keine City-/Global-Kampagnen-
  Mutation, keine Preis-/Equipment-/SizeTier-/Personal-/Day-Sim-Mutation,
  keine Save-/PlayerPrefs-/Filesystem-Logik und keine Arcade-/Realtime-
  Serving-/CustomerSpawner-/manuelle Koch-/First-/Third-Person-Systeme
  eingefuehrt. Tests gruen; Queue auf `Status: empty` gesetzt.
- Current cron run 2026-06-11 17:30: Pflichtdateien gelesen; Worktree enthaelt
  nur erwartete Control-Datei-Aenderungen aus dem 17:03-Review-Queue-Setup.
  `HEAD == origin/main` (`7bbb7bf`). Offenes Claude-Reviewer-Item
  "Unity RestaurantDetail shop marketing controller flow review" wurde gemaess
  Queue-Regel nicht durch Codex implementiert, sondern via n8n an Claude Code
  dispatcht. Dispatch erfolgreich angenommen, runId:
  `ff2b7219-142e-4b07-9b62-d8fb12a49ad0`. Queue bleibt `Status: open`, bis
  das Review-Ergebnis dokumentiert ist.
- Current cron run 2026-06-11 17:03: Pflichtdateien gelesen;
  `REVIEW_QUEUE.md` war `Status: empty`. Letzter Commit ist `7bbb7bf`
  ("Add restaurant detail shop marketing flow"). Entsprechend der
  Agent-Control-Regel wurde kein pauschales Codex-"mach weiter" gestartet,
  sondern ein konkretes Claude-Review-Item formuliert: "Unity RestaurantDetail
  shop marketing controller flow review". Scope: Commit `7bbb7bf` gegen
  Management-Spiel-Richtung pruefen: `GameController.StartShopCampaign(shopId,
  campaignId)` ist die einzige Shop-Marketing-Mutationsgrenze,
  `RestaurantDetailView` feuert im Marketing-Tab nur diesen Intent fuer
  `MarketingCatalog.ShopCampaigns`, `ShopCampaignService` bleibt UnityEngine-
  frei und validiert State, Shop, CampaignId, Scope `Shop`, Duplicate/aktive
  Kampagne und Cash fuer die expliziten Katalogkosten. Erfolg muss genau diese
  Kosten abziehen, genau eine `ActiveCampaign` mit `StartDay = CurrentDay` und
  `EndDay = CurrentDay + DurationDays` an `Shop.ActiveCampaigns` anhaengen und
  Snapshot, RestaurantDetail-Refresh und Toast publizieren. Fehler bleiben ohne
  Mutation und publizieren nur Toasts. Keine City-/Global-Kampagnen-Mutation,
  keine Preis-/Equipment-/SizeTier-/Personal-/Day-Sim-Mutation ausserhalb
  bestehender Pfade, keine Save-/PlayerPrefs-/Filesystem-Logik und keine
  Arcade-/Realtime-Serving-/CustomerSpawner-/manuelle Koch-/First-/Third-
  Person-Systeme freigegeben.
- Current cron run 2026-06-11 17:00: Offenes Codex-Item "Unity
  RestaurantDetail shop marketing controller mutation" umgesetzt.
  `ShopCampaignService` ist UnityEngine-frei und validiert State, Shop,
  CampaignId, Shop-Scope, Duplicate/aktuell aktive Kampagne und Cash fuer
  die expliziten Katalogkosten. Erfolg zieht genau die Kosten der
  Shop-Kampagne ab, haengt genau eine `ActiveCampaign` an
  `Shop.ActiveCampaigns` und setzt `StartDay = CurrentDay`, `EndDay =
  CurrentDay + DurationDays`. `GameController.StartShopCampaign(shopId,
  campaignId)` publiziert bei Erfolg Snapshot, RestaurantDetail-Refresh und
  Toast; Fehler publizieren nur Toasts. `RestaurantDetailView` zeigt
  Shop-Kampagnen im Marketing-Tab und feuert nur diesen Controller-Intent.
  Keine City-/Global-Kampagnen-Mutation, keine Preis-/Equipment-/SizeTier-/
  Personal-/Day-Sim-Mutation, keine Save-/PlayerPrefs-/Filesystem-Logik und
  keine Arcade-/Realtime-Serving-/CustomerSpawner-/manuelle Koch-/First-/
  Third-Person-Systeme eingefuehrt. Tests gruen; Queue auf `Status: empty`
  gesetzt.
- Current cron run 2026-06-11 16:30: Pflichtdateien gelesen;
  `REVIEW_QUEUE.md` war `Status: empty`. Letzter Commit ist `06e381d`
  ("Accept RestaurantDetail staff hiring review"). Entsprechend der
  Agent-Control-Regel wurde kein pauschales Codex-"mach weiter" gestartet,
  sondern ein konkretes Codex-Item formuliert: "Unity RestaurantDetail shop
  marketing controller mutation". Scope: Shop-Marketing fuer bestehende owned
  Shops nur ueber einen `GameController`-Intent, z. B.
  `StartShopCampaign(shopId, campaignId)`. `RestaurantDetailView` darf im
  Marketing-Tab nur diesen Intent ausloesen. Ein UnityEngine-freier Service
  validiert State, Shop, CampaignId, Scope `Shop`, Duplicate/aktive Kampagne
  und ausreichend Cash fuer die expliziten Katalogkosten. Erfolg zieht genau
  diese Kosten ab, fuegt genau eine `ActiveCampaign` mit `StartDay =
  CurrentDay` und `EndDay = CurrentDay + DurationDays` an `Shop.ActiveCampaigns`
  hinzu und publiziert Snapshot, RestaurantDetail-Refresh und Toast. Fehler
  bleiben ohne Mutation und publizieren nur Toasts. Keine City-/Global-
  Kampagnen-Mutation, keine Preis-/Equipment-/SizeTier-/Personal-/Day-Sim-
  Mutation ausserhalb bestehender Pfade, keine Save-/PlayerPrefs-/Filesystem-
  Logik und keine Arcade-/Realtime-Serving-/CustomerSpawner-/manuelle Koch-/
  First-/Third-Person-Systeme freigegeben.
- Current run 2026-06-11 16:08: Offenes Queue-Item "Unity RestaurantDetail
  staff hiring controller flow review" fuer Commit `cddadac` geprueft.
  Ergebnis: akzeptiert. `GameController.HireEmployee(shopId, employeeId)` ist
  die zentrale Personal-Einstellungs-Mutationsgrenze; bei Fehlern wird nur ein
  Toast publiziert, bei Erfolg Snapshot, RestaurantDetail-Refresh und Toast.
  `RestaurantDetailView` ruft im Personal-Tab ausschliesslich diesen
  Controller-Intent auf und mutiert `GameState`, `Shop`, Cash, Employees,
  `EmployeePool`, Save-State, Dateien oder Hotspots nicht direkt.
  `EmployeeHiringService` bleibt UnityEngine-frei, validiert Shop, Bewerber-ID,
  Rollen-Katalog, Duplicate, effektives Stadt-/SizeTier-Personal-Cap und Cash
  fuer die explizite Hiring Fee. Erfolg fuegt genau einen Mitarbeiter zur
  Ziel-Filiale hinzu, entfernt genau diesen Kandidaten aus `EmployeePool` und
  zieht nur die Hiring Fee ab. Keine Preis-/Equipment-/SizeTier-/Marketing-/
  Day-Sim-Mutation, keine Save-/PlayerPrefs-/Filesystem-Logik und keine
  Arcade-/Realtime-Serving-/CustomerSpawner-/manuelle Koch-/First-/Third-
  Person-Systeme eingefuehrt. Tests gruen; Queue auf `Status: empty` gesetzt.
- Current cron run 2026-06-11 16:00: Pflichtdateien gelesen; Worktree enthaelt
  nur erwartete Control-Datei-Aenderungen aus dem 15:33-Queue-Setup.
  `HEAD == origin/main` (`cddadac`). Offenes Claude-Reviewer-Item
  "Unity RestaurantDetail staff hiring controller flow review" wurde gemaess
  Queue-Regel nicht durch Codex implementiert, sondern via n8n an Claude Code
  dispatcht. Dispatch erfolgreich angenommen, runId:
  `0789f97b-066c-4bcf-8b1b-7bd4afd37e18`. Queue bleibt `Status: open`, bis
  das Review-Ergebnis dokumentiert ist.
- Current cron run 2026-06-11 15:33: Pflichtdateien gelesen;
  `REVIEW_QUEUE.md` war nach Codex' Staff-Hiring-Umsetzung wieder
  `Status: empty`. Worktree war vor diesem Lauf sauber. Letzter Commit ist
  `cddadac` ("Add restaurant detail staff hiring flow"). Entsprechend der
  Agent-Control-Regel wurde kein pauschales Codex-"mach weiter" gestartet,
  sondern ein konkretes Claude-Review-Item formuliert: "Unity RestaurantDetail
  staff hiring controller flow review". Scope: Commit `cddadac` gegen
  Management-Spiel-Richtung pruefen: `GameController.HireEmployee(shopId,
  employeeId)` ist die einzige Personal-Einstellungs-Mutationsgrenze,
  `RestaurantDetailView` feuert im Personal-Tab nur den Intent,
  `EmployeeHiringService` bleibt UnityEngine-frei und validiert Shop,
  Candidate/EmployeeId, Rollen-Katalog, Duplicate, effektives Stadt-/
  SizeTier-Personal-Cap und Cash fuer die explizite Hiring Fee. Erfolg muss
  genau einen Mitarbeiter zur Ziel-Filiale hinzufuegen, genau diesen Kandidaten
  aus `EmployeePool` entfernen, nur die explizite Fee abziehen und Snapshot,
  RestaurantDetail-Refresh und Toast publizieren. Fehler bleiben ohne Mutation
  und publizieren nur Toasts. Keine Preis-/Equipment-/SizeTier-/Marketing-/
  Day-Sim-Mutation ausserhalb bestehender Pfade, keine Save-/PlayerPrefs-/
  Filesystem-Logik und keine Arcade-/Realtime-Serving-/CustomerSpawner-/
  manuelle Koch-/First-/Third-Person-Systeme freigegeben.
- Current cron run 2026-06-11 15:30: Offenes Codex-Item "Unity
  RestaurantDetail staff hiring controller mutation" umgesetzt.
  `EmployeeHiringService` ist UnityEngine-frei und validiert Shop,
  Bewerber-ID, Rollen-Katalog, Duplicate, effektiven Stadt-/SizeTier-Personal-
  Cap und Cash fuer die explizite Hiring Fee (`SalaryPerDay * 1.25`). Erfolg
  entfernt genau einen Bewerber aus `EmployeePool`, fuegt genau einen
  Mitarbeiter zur Ziel-Filiale hinzu und zieht nur diese Fee ab. Fehlerpfade
  mutieren den State nicht. `GameController.HireEmployee(shopId, employeeId)`
  publiziert bei Erfolg Snapshot, RestaurantDetail-Refresh und Toast; bei
  Fehlern nur Toast. `RestaurantDetailView` zeigt im Personal-Tab Bewerber und
  feuert nur diesen Controller-Intent. Tests gruen; Queue wieder `Status:
  empty`.
- Current cron run 2026-06-11 15:00: Pflichtdateien gelesen;
  `REVIEW_QUEUE.md` war `Status: empty`. Letzter Commit ist `82cbe55`
  ("Add restaurant detail equipment purchase flow"). Worktree enthielt bereits
  Control-Datei-Aenderungen in `STATUS.md` und `HANDOFF_LOG.md`; diese wurden
  nicht zurueckgesetzt. Entsprechend der Agent-Control-Regel wurde kein
  pauschales Codex-"mach weiter" gestartet, sondern ein konkretes Codex-Item
  formuliert: "Unity RestaurantDetail staff hiring controller mutation".
  Scope: Personal-Einstellung fuer bestehende owned Shops nur ueber einen
  `GameController`-Intent. `RestaurantDetailView` darf im Personal-Tab nur den
  Intent ausloesen. Ein UnityEngine-freier Service validiert Shop, Employee/
  CandidateId, Duplicate, Employee-Cap und ausreichend Cash, falls ein
  Hiring-Cost-Modell existiert. Erfolg fuegt genau einen Mitarbeiter hinzu,
  entfernt den Kandidaten aus dem Pool falls vorhanden, zieht nur explizite
  Hiring Costs ab und publiziert Snapshot, Detail-Refresh und Toast. Fehler
  bleiben ohne Mutation und publizieren nur Toasts. Keine Preis-/Equipment-/
  SizeTier-/Marketing-/Day-Sim-Mutation ausserhalb bestehender Pfade, keine
  Save-/PlayerPrefs-/Filesystem-Logik und keine Arcade-/Realtime-Serving-/
  CustomerSpawner-/manuelle Koch-/First-/Third-Person-Systeme freigegeben.
- Current cron run 2026-06-11 14:30: Pflichtdateien gelesen und offenes
  Queue-Item "Unity RestaurantDetail equipment purchase controller flow review"
  fuer Commit `82cbe55` geprueft. Ergebnis: akzeptiert.
  `GameController.BuyEquipment(shopId, equipmentId)` ist die zentrale
  Equipment-Kauf-Mutationsgrenze; bei Fehlern wird nur ein Toast publiziert,
  bei Erfolg Snapshot, RestaurantDetail-Refresh und Toast. `RestaurantDetailView`
  ruft im Equipment-Tab ausschliesslich diesen Controller-Intent auf und
  mutiert `GameState`, `Shop`, Cash, Equipment-Liste, Save-State, Dateien oder
  Hotspots nicht direkt. `EquipmentPurchaseService` bleibt UnityEngine-frei,
  validiert Shop, EquipmentId, Duplicate und ausreichend Cash; ungueltige
  Faelle bleiben ohne State-Mutation. Erfolg zieht genau die Katalogkosten ab
  und fuegt genau ein `ShopEquipment` hinzu. Keine Personal-/Marketing-Mutation,
  keine neue SizeTier-/Preis-Mutation, keine Save-/PlayerPrefs-/Filesystem-Logik
  und keine Arcade-/Realtime-Serving-/CustomerSpawner-/manuelle Koch-/First-/
  Third-Person-Systeme eingefuehrt. Tests gruen; Queue auf `Status: empty`
  gesetzt.
- Current cron run 2026-06-11 13:32: Pflichtdateien gelesen;
  `REVIEW_QUEUE.md` war nach Codex' Equipment-Kauf-Umsetzung wieder
  `Status: empty`. Letzter Commit ist `82cbe55` ("Add restaurant detail
  equipment purchase flow"). Entsprechend der Agent-Control-Regel wurde kein
  pauschales Codex-"mach weiter" gestartet, sondern ein konkretes Claude-
  Review-Item formuliert: "Unity RestaurantDetail equipment purchase controller
  flow review". Scope: Commit `82cbe55` gegen Management-Spiel-Richtung
  pruefen: `GameController.BuyEquipment(shopId, equipmentId)` ist die einzige
  Equipment-Kauf-Mutationsgrenze, `RestaurantDetailView` feuert im Equipment-
  Tab nur den Intent, `EquipmentPurchaseService` bleibt UnityEngine-frei und
  validiert Shop, EquipmentId, Duplicate und Cash ohne Mutation bei Fehlern.
  Erfolg muss genau die Equipment-Kosten abziehen, genau ein `ShopEquipment`
  anfuegen und Snapshot, RestaurantDetail-Refresh und Toast publizieren. Keine
  Personal-/Marketing-Mutation, keine SizeTier-/Preis-Mutation ausserhalb
  bestehender Pfade, keine Save-/PlayerPrefs-/Filesystem-Logik und keine
  Arcade-/Realtime-Serving-/CustomerSpawner-/manuelle Koch-/First-/Third-
  Person-Systeme freigegeben.
- Current cron run 2026-06-11 13:00: Pflichtdateien gelesen;
  `REVIEW_QUEUE.md` war `Status: empty`. Letzter Commit ist `2ef9ca7`
  ("Add restaurant detail size upgrade flow"). Entsprechend der Agent-Control-
  Regel wurde kein pauschales Codex-"mach weiter" gestartet, sondern ein
  konkretes Codex-Item formuliert: "Unity RestaurantDetail equipment purchase
  controller mutation". Scope: Equipment-Kauf fuer bestehende owned Shops nur
  ueber einen `GameController`-Intent; `RestaurantDetailView` darf im
  Equipment-Tab nur diesen Intent ausloesen. Service bleibt UnityEngine-frei
  und validiert Shop, EquipmentId, Duplicate und Cash; Erfolg zieht Kosten ab,
  fuegt genau ein `ShopEquipment` hinzu und publiziert Snapshot, Detail-Refresh
  und Toast. Fehler bleiben ohne Mutation und publizieren nur Toasts. Keine
  Personal-/Marketing-Mutation, keine SizeTier-/Preis-Mutation ausserhalb
  bestehender Pfade, keine Save-/PlayerPrefs-/Filesystem-Logik und keine
  Arcade-/Realtime-Serving-/CustomerSpawner-/manuelle Koch-/First-/Third-
  Person-Systeme freigegeben.
- Current cron run 2026-06-11 12:30: Pflichtdateien gelesen und offenes
  Queue-Item "Unity RestaurantDetail size-tier controller flow review" fuer
  Commit `2ef9ca7` geprueft. Ergebnis: akzeptiert.
  `GameController.UpgradeShopSizeTier(shopId)` ist die zentrale SizeTier-
  Mutationsgrenze; Erfolg publiziert Snapshot, RestaurantDetail-Refresh und
  Toast, Fehler publizieren nur Toast. `RestaurantDetailView` ruft im Ausbau-
  Tab ausschliesslich diesen Controller-Intent auf und mutiert `GameState`,
  `Shop`, Cash, Save-State, Dateien oder Hotspots nicht direkt.
  `ShopExpansionService` bleibt UnityEngine-frei, validiert Shop, naechste
  Stufe, Stadt-/Personal-Cap, Max-Tier und ausreichend Cash; ungueltige Faelle
  bleiben ohne State-Mutation. Keine Equipment-/Personal-/Marketing-Mutation,
  keine Save-/PlayerPrefs-/Filesystem-Logik und keine Arcade-/Realtime-Serving-/
  CustomerSpawner-/manuelle Koch-/First-/Third-Person-Systeme eingefuehrt.
  Tests gruen; Queue auf `Status: empty` gesetzt.
- Current cron run 2026-06-11 09:33: Pflichtdateien gelesen;
  `REVIEW_QUEUE.md` war nach Codex' Size-Tier-Umsetzung wieder `Status:
  empty`. Worktree sauber; `HEAD == origin/main` (`2ef9ca7`, "Add restaurant
  detail size upgrade flow"). Entsprechend der Agent-Control-Regel wurde kein
  pauschales Codex-"mach weiter" gestartet, sondern ein konkretes Review-Item
  formuliert: "Unity RestaurantDetail size-tier controller flow review".
  Scope: Commit `2ef9ca7` gegen Management-Spiel-Richtung pruefen:
  `GameController.UpgradeShopSizeTier(shopId)` ist die einzige
  SizeTier-Mutationsgrenze, `RestaurantDetailView` feuert nur den Intent,
  `ShopExpansionService` bleibt UnityEngine-frei und validiert Shop, naechste
  Stufe, Stadt-/Personal-Cap, Max-Tier und Cash ohne Mutation bei Fehlern.
  Keine Equipment-/Personal-/Marketing-Mutation, keine Save-/PlayerPrefs-/
  Filesystem-Logik und keine Arcade-/Realtime-Serving-/CustomerSpawner-/
  manuelle Koch-/First-/Third-Person-Systeme freigegeben.
- Current cron run 2026-06-11 09:08: Pflichtdateien gelesen;
  `REVIEW_QUEUE.md` war `Status: empty`. Letzter Commit ist `80689f5`
  ("Add city map opening forecast"). Entsprechend der Agent-Control-Regel wurde
  kein pauschales Codex-"mach weiter" gestartet, sondern ein konkretes Codex-
  Item formuliert: "Unity RestaurantDetail size-tier controller mutation".
  Scope: Nur Ausbau eines bestehenden owned Shops ueber einen
  `GameController`-Intent; `RestaurantDetailView` darf nur den Intent
  ausloesen. Erfolg zieht Upgrade-Kosten ab, aktualisiert `Shop.SizeTier` und
  erwartete Rent/Cap-Wirkung, publiziert Snapshot/Detail-Refresh/Toast.
  Ungueltige Eingaben, Max-Tier oder zu wenig Cash muessen ohne Mutation bleiben
  und Toasts publizieren. Keine Equipment-/Personal-/Marketing-Mutation, keine
  Save-/PlayerPrefs-/Filesystem-Logik und keine Arcade-/Realtime-Serving-/
  CustomerSpawner-/manuelle Koch-/First-/Third-Person-Systeme freigegeben.
- Current cron run 2026-06-11 07:38: Pflichtdateien gelesen und offenes
  Queue-Item "Unity RestaurantDetail price controller flow review" geprueft.
  Ergebnis: akzeptiert. `GameController.SetProductPrice(shopId, productId,
  price)` ist die zentrale Preis-Mutationsgrenze; Erfolg publiziert Snapshot,
  `RestaurantDetailRequestedEvent` und Toast, Fehler nur Toast. `RestaurantDetailView`
  ruft im Sortiment nur diesen Controller-Intent auf; Ausbau, Equipment,
  Personal und Marketing bleiben read-only. `ProductPricingService` ist
  UnityEngine-frei, validiert Shop, Produkt und Preisbereich 1.00 bis 25.00 EUR
  und laesst State bei ungueltigen Eingaben unveraendert. Tests gruen; Queue auf
  `Status: empty` gesetzt.
- Current cron run 2026-06-11 07:06: Pflichtdateien gelesen; Worktree clean
  und `HEAD == origin/main` (`bb9e7fa`). `REVIEW_QUEUE.md` enthaelt weiterhin
  das offene Claude-Reviewer-Item "Unity RestaurantDetail price controller flow
  review" fuer Commit `f51589c`. Review-Dispatch an
  `http://127.0.0.1:5678/webhook/doener-unity-dispatch` wurde versucht, ist
  aber fehlgeschlagen: lokale n8n-Verbindung konnte nicht hergestellt werden.
  Keine Code-Aenderung gestartet; Queue bleibt offen.
- Current Claude run 2026-06-06 08:33: Pflichtdateien gelesen;
  `REVIEW_QUEUE.md` war nach Codex' RestaurantDetail-Preis-Umsetzung wieder
  `Status: empty`. Letzter Commit ist `f51589c` ("Add restaurant detail price
  controller flow"). Entsprechend der Agent-Control-Regel wurde kein
  pauschales Codex-"mach weiter" gestartet, sondern ein konkretes Review-Item
  formuliert: "Unity RestaurantDetail price controller flow review". Scope:
  Preis-Aenderungen nur ueber `GameController.SetProductPrice`, UI nur als
  Intent-Ausloeser, `ProductPricingService` UnityEngine-frei mit Shop-/Produkt-/
  Preisvalidierung und keine Mutation bei Fehlern. Keine Upgrades, kein
  Equipment-/Personal-/Marketing-Write, keine Cash- oder Save-Mutation und
  keine Arcade-/Realtime-Serving-/CustomerSpawner-/manuelle Koch-/First-/
  Third-Person-Systeme freigegeben.
- Current Claude run 2026-06-06 08:00: Pflichtdateien gelesen und offenes
  Queue-Item "Unity BuyDialog open-shop controller flow review" fuer Commit
  `2fabaeb` geprueft. Ergebnis: akzeptiert. Open-Shop-Mutation ist ueber
  `GameController.OpenShop(CityMapHotspot)` begrenzt; `BuyDialogView` ruft beim
  Confirm nur diesen Controller-Intent auf und mutiert `GameState`, `Shop`,
  Cash, Hotspot-Ownership, Save-State oder Dateien nicht direkt.
  `ShopOpeningService` bleibt UnityEngine-frei, validiert unvollstaendige/
  doppelte/ungueltige Cash-Faelle, zieht nur `Deposit + WeeklyRent` ab und
  erzeugt eine Default-Filiale mit Default-Menue und Basis-Equipment.
  Erfolgreicher Kauf publiziert Snapshot/Location/RestaurantDetail/Toast und
  markiert den Hotspot owned. Keine RestaurantDetail-Funktionsmutation, keine
  Upgrades, kein Price-Editing, keine Save-/PlayerPrefs-/Filesystem-Logik und
  keine Arcade-/Realtime-Serving-/CustomerSpawner-/manuelle Koch-/First-/
  Third-Person-Systeme eingefuehrt. Danach neues kleines Codex-Item gesetzt:
  "Unity RestaurantDetail price controller mutation" fuer Sortiment-Preis-
  Aenderung nur ueber `GameController`; keine Upgrades, kein Equipment/
  Personal/Marketing, keine Cash- oder Save-Mutation.
- Current Claude run 2026-06-06 07:46: Pflichtdateien gelesen; `REVIEW_QUEUE.md`
  war nach Codex' BuyDialog/OpenShop-Umsetzung wieder `Status: empty`. Letzter
  Commit ist `2fabaeb` ("Add Unity open shop controller flow"). Entsprechend der
  Agent-Control-Regel wurde kein pauschales Codex-"mach weiter" gestartet,
  sondern ein konkretes Review-Item formuliert: Unity BuyDialog open-shop
  controller flow review. Scope: Mutation nur ueber
  `GameController.OpenShop(CityMapHotspot)`, `BuyDialogView` nur als Intent-
  Ausloeser, `ShopOpeningService` UnityEngine-frei und auf erste
  Filialeroeffnung begrenzt, erfolgreiche Events/Hotspot-Owned-Status
  pruefen. Keine RestaurantDetail-Funktionsmutation, keine Upgrades, kein
  Price-Editing, keine Save-/PlayerPrefs-/Filesystem-Logik und keine Arcade-/
  Realtime-Serving-/CustomerSpawner-/manuelle Koch-/First-/Third-Person-
  Systeme freigegeben.
- Current Claude run 2026-06-06 07:40: Pflichtdateien gelesen und offenes
  Queue-Item "Unity DayReport controller flow review" fuer Commit `c94bbdc`
  geprueft. Ergebnis: akzeptiert. `GameController.SimulateDay()` ruft nur den
  bestehenden `GameEngine.SimulateDay(GameState)`-Managementpfad auf, publiziert
  danach `DayEndedEvent` mit `DailyRecord`/`DaySimulationResult` und erst danach
  den `StateSnapshotChangedEvent`. `DayReportView` zeigt Werte aus
  `DailyRecord` und schliesst per `ZURUECK` ohne `GameState`-Mutation.
  `LocationSheetView`/`DayReportView`/`CityMapBootstrap` bleiben
  Presentation-/Controller-Wiring; keine UI-Wirtschaftsformel, keine
  Save-/Filesystem-/PlayerPrefs-Logik und keine Arcade-/Realtime-Serving-/
  CustomerSpawner-/manuelle Koch-/First-/Third-Person-Systeme. Danach wurde als
  naechstes konkretes Codex-Item "Unity BuyDialog open-shop controller mutation"
  gesetzt: echte Filialeroeffnung nur ueber `GameController`, UI mutiert State
  nicht direkt.
- Current Claude run 2026-06-05 21:04: Pflichtdateien gelesen;
  `REVIEW_QUEUE.md` war nach Codex' DayReport-Umsetzung wieder `Status:
  empty`. Letzter Commit ist `c94bbdc` ("Add Unity day report controller
  flow"). Entsprechend der Agent-Control-Regel wurde kein pauschales "mach
  weiter" gestartet, sondern ein konkretes Review-Item formuliert:
  Unity DayReport controller flow review. Scope: Day-Sim-Intent ueber
  `GameController.SimulateDay()`, Event-Reihenfolge `DayEndedEvent` vor
  `StateSnapshotChangedEvent`, DayReport-Anzeige aus `DailyRecord`/
  `DaySimulationResult`, keine UI-Wirtschaftslogik und keine Arcade-/Realtime-
  Serving-/BuyDialog-/RestaurantDetail-Funktionsmutation.
- Current Claude run 2026-06-05 17:34: Pflichtdateien gelesen;
  `REVIEW_QUEUE.md` war nach Codex' GameEngine-Foundation wieder `Status:
  empty`. Entsprechend der Agent-Control-Regel wurde kein pauschales "mach
  weiter" gestartet, sondern ein konkretes Codex-Review-Item formuliert:
  Unity Day Sim controller intent + DayReport shell. Scope bleibt
  Management-/Progression: vorhandenen `GameEngine.SimulateDay(GameState)` nur
  ueber `GameController`/`EventBus` ansteuerbar machen, DayReport-Shell anzeigen
  und keine neue Wirtschafts-, Buy-/RestaurantDetail-Mutations-, Arcade- oder
  Realtime-Serving-Logik einfuehren.
- Current Claude run 2026-06-05 16:09: reviewed commit `2421d41`
  ("Harden Unity save service API"). Result: accepted. `SaveService` remains a
  UnityEngine-free instance API with `Serialize(GameState)` /
  `Deserialize(string)`, uses lower-camelCase JSON, preserves Dart-compatible
  enum strings, covers a broader MVP roundtrip, and deserializes missing/null
  optional collections into usable defaults. No PlayerPrefs, filesystem
  persistence, UI mutation, Buy/Upgrade, GameEngine/Day-Sim, Arcade-Cooking or
  realtime Serving logic was added. Since `REVIEW_QUEUE.md` was `Status:
  empty`, a new concrete Codex review item was queued: Unity GameEngine day
  simulation foundation.
- Current Claude run 2026-06-05 11:04: reviewed commit `1ca92f3`
  ("Add Unity save service foundation"). Result: accepted as foundation.
  The commit adds `docs/UNITY_SAVE_COMPAT.md`, a UnityEngine-free
  `SaveService` JSON string roundtrip for the current MVP `GameState`, links the
  `Save` folder into the logic test project, and adds focused tests for
  non-trivial state roundtrip plus Dart-compatible enum strings. No PlayerPrefs,
  filesystem persistence, UI mutation, Buy/Upgrade, GameEngine/Day-Sim,
  Arcade-Cooking or realtime Serving logic was added. Current worktree already
  contains uncommitted SaveService polish; it was not reverted. Since
  `REVIEW_QUEUE.md` was `Status: empty`, a new concrete Codex review item was
  queued: SaveService API hardening + fixture-shaped JSON.
- Current Claude run 2026-06-05 10:59: reviewed commit `f61dc4d`
  ("Add Unity restaurant detail shell"). Result: accepted.
  `RestaurantDetailView` is presentation-only, opens from
  `RestaurantDetailRequestedEvent`, looks up the owned shop from `GameState.Shops`
  via the hotspot/shop id, and is wired through `CityMapBootstrap` using the
  existing `GameController`/`EventBus`. It shows shop/location identity and
  read-only `Sortiment`, `Ausbau`, `Equipment`, `Personal`, `Marketing`
  sections; close and a new location selection only close the detail shell.
  No price, Shop, SizeTier, Cash, Buy, SaveService, GameEngine/Day-Sim,
  Arcade-Cooking or realtime Serving logic was added. Since `REVIEW_QUEUE.md`
  was `Status: empty`, a new concrete Codex review item was queued:
  Unity SaveService compatibility foundation.
- Current Claude run 2026-06-05 10:55: reviewed commit `1591e9d`
  ("Add Unity city map buy dialog shell"). Result: accepted. `BuyDialogView`
  is presentation-only, opens from `BuyDialogRequestedEvent`, is wired through
  `CityMapBootstrap` using the existing `GameController`/`EventBus`, and keeps
  the confirm action disabled as `NOCH NICHT AKTIV`. It displays Standort,
  Lage/Stadtteil, Kaution, Wochenmiete and Kapital nach Kaution from existing
  hotspot/state data. Abbrechen and a new non-locked Location selection close
  only the dialog. No Buy-/Cash-/Shop-mutation, RestaurantDetail implementation,
  Upgrade, SaveService, GameEngine/Day-Sim, Arcade-Cooking or realtime Serving
  logic was added. Since `REVIEW_QUEUE.md` was `Status: empty`, a new concrete
  Codex review item was queued: CityMap RestaurantDetail shell via
  GameController.
- Current Claude run 2026-06-05 10:50: reviewed commit `e7a1b48`
  ("Add Unity city map focus tween"). Result: accepted. Camera focus tween is
  presentation-only, is driven by `LocationSelectedEvent`, clamps through the
  existing x/z camera bounds, and manual pan cancels the tween. Locked hotspots
  remain Toast-only through `GameController.SelectLocation`; no Buy-State-
  mutation, RestaurantDetail, Upgrade, SaveService, GameEngine/Day-Sim,
  Arcade-Cooking or realtime Serving logic was added. Since `REVIEW_QUEUE.md`
  was `Status: empty`, a new concrete Codex review item was queued: CityMap
  BuyDialog shell via GameController.
- Current Claude run 2026-06-05 10:46: reviewed commit `1af4408`
  ("Polish Unity available KPI order"). Result: accepted. Available KPI order
  now matches `UNITY_CITY_MAP_UX.md` section 3.2 (`TRAFFIC`, `MIETE`,
  `KAUTION`, `KONKURRENZ`); Owned order remains `MARKTANTEIL`, `TRAFFIC`,
  `MIETE`, `PROGNOSE`; locked selection remains Toast-only through
  `GameController`; no Arcade/Serving/Buy/Upgrade/SaveService/Day-Sim logic was
  added. Since `REVIEW_QUEUE.md` was `Status: empty`, a new concrete Codex
  review item was queued: CityMap selection focus tween.
- Current Claude run 2026-06-05 06:52: reviewed commit `333ca76`
  ("Fix Unity city map presentation queue item"). Result: previous item accepted:
  owned KPI labels are `MARKTANTEIL`/`PROGNOSE`, available KPI 4 is
  `KONKURRENZ`, locked selection remains Toast-only through `GameController`,
  and no Arcade/Serving/Buy/Upgrade/Day-Sim logic was added. One small UX polish
  remains: available KPI tile order should match `UNITY_CITY_MAP_UX.md` section
  3.2 (`TRAFFIC`, `MIETE`, `KAUTION`, `KONKURRENZ`), so `REVIEW_QUEUE.md` was
  opened with that concrete item for Codex.
- Current Claude run 2026-06-05: Pflichtdateien gelesen; `REVIEW_QUEUE.md`
  war `Status: empty`, daher ein konkretes Codex-Review-Item formuliert:
  CityMap pre-step-4 presentation fixes (KPI-Labels Owned/Available und
  Locked-Tap nur Toast). Keine Code-Implementierung gestartet.
- Scope-Review: Richtung = Management-Spiel bestaetigt; Arcade-Plan deprecated.
- `docs/UNITY_CITY_MAP_UX.md` erstellt.
- Verifizierter C#-Logik-Port auf 86 gruene Tests.
- Review Codex CityMap Vertical Slice Schritte 1-3: BESTANDEN (HANDOFF_LOG #5, #11, #12).
  Alle vorherigen Flags (Kamerawinkel, xBounds) behoben; verbleibende Non-Blocker
  fuer Schritt 4: KPI-Labels (RUF/DRUCK statt MARKTANTEIL/PROGNOSE), Locked-Toast
  fehlt, kein Fokus-Tween, IMGUI statt UI Toolkit (erwartet Schritt 7).
- Bereit fuer Unity-Editor-Test: Bootstrap auto-fires, keine manuellen Scene-Objekte.
Next:
- Claude Code: Pushed SHA reviewen und danach das naechste kleine kohaerente
  Management-/Progression-Queue-Item waehlen.
- Upgrades, Equipment-/Personal-/Marketing-Aktionen, Cash-Mutation ausserhalb
  eigener Items und Save-/Persistenzlogik bleiben gesperrt.
- UI-Toolkit-Migration (IMGUI ersetzen) geplant fuer Schritt 7 (Premium-Polish).
- Vor Schritt 4: KPI-Labels korrigieren (Owned: MARKTANTEIL/PROGNOSE;
  Available: KONKURRENZ); Locked-Tap auf Toast-only umstellen.

## Codex (Implementation)
State: complete - Unity RestaurantDetail equipment purchase controller mutation implemented, validation green (2026-06-11)
Done:
- Current Codex run 2026-06-11 13:30: Offenes Review-Item
  "Unity RestaurantDetail equipment purchase controller mutation" umgesetzt:
  - `EquipmentPurchaseService` als UnityEngine-freier Kaufpfad hinzugefuegt;
    validiert Shop, EquipmentId, Duplicate und ausreichend Cash.
  - `GameController.BuyEquipment(shopId, equipmentId)` ist der einzige Intent
    fuer Equipment-Kauf; Fehler publizieren nur Toasts, Erfolg zieht Kosten ab,
    fuegt genau ein `ShopEquipment` hinzu, publiziert Snapshot, Detail-Refresh
    und Toast.
  - `RestaurantDetailView` loest im Equipment-Tab nur diesen Controller-Intent
    aus; keine direkte Mutation von `GameState`, `Shop`, Cash, Equipment-Liste,
    Save-State, Dateien oder Hotspots.
  - Fokussierte Tests decken erfolgreichen Kauf sowie ungueltigen Shop,
    ungueltiges Equipment, Duplicate und zu wenig Cash ohne Mutation ab.
  - `REVIEW_QUEUE.md` wieder auf `Status: empty` gesetzt.
- Current Codex run 2026-06-11 09:30: Offenes Review-Item
  "Unity RestaurantDetail size-tier controller mutation" umgesetzt:
  - `ShopExpansionService` als UnityEngine-freier Ausbaupfad hinzugefuegt;
    validiert Shop, naechste Stufe, Stadt-/Personal-Cap und ausreichend Cash.
  - `GameController.UpgradeShopSizeTier(shopId)` ist der einzige Intent fuer
    SizeTier-Ausbau; Fehler publizieren nur Toasts, Erfolg zieht Kosten ab,
    aktualisiert `Shop.SizeTier`, publiziert Snapshot, Detail-Refresh und Toast.
  - `RestaurantDetailView` loest im Ausbau-Tab nur diesen Controller-Intent aus;
    keine direkte Mutation von `GameState`, `Shop`, Cash, Save-State, Dateien
    oder Hotspots.
  - Fokussierte Tests decken erfolgreichen Ausbau sowie zu wenig Cash, Stadt-Cap
    und Max-Tier ohne Mutation ab.
  - Keine Equipment-/Personal-/Marketing-Mutation, keine Save-/PlayerPrefs-/
    Filesystem-Logik und keine Arcade-/Realtime-Serving-/CustomerSpawner-/
    manuelle Koch-/First-/Third-Person-Systeme eingefuehrt.
- Current Codex run 2026-06-06 08:30: Offenes Review-Item
  "Unity RestaurantDetail price controller mutation" umgesetzt:
  - `ProductPricingService` als UnityEngine-freier Preis-Mutationspfad
    hinzugefuegt; validiert Shop, Produkt und expliziten MVP-Preisbereich
    1.00 bis 25.00 EUR.
  - `GameController.SetProductPrice(shopId, productId, price)` ist der
    einzige Intent fuer Sortiment-Preis-Aenderungen; Fehler publizieren Toasts
    und lassen State unveraendert, Erfolg publiziert Snapshot, Detail-Refresh
    und Toast.
  - `RestaurantDetailView` zeigt im Sortiment einfache IMGUI-Stepper und ruft
    nur den Controller-Intent auf; keine direkte Mutation von `GameState`,
    `Shop`, `ShopProduct`, Cash, Save-State, Dateien oder Hotspots.
  - Fokussierte Tests decken erfolgreiche Preis-Aenderung sowie ungueltige
    Shop-/Produkt-/Preis-Faelle ohne Mutation ab.
  - Keine Upgrades, kein Equipment-/Personal-/Marketing-Write, keine Cash-,
    Save-/PlayerPrefs-/Filesystem-Mutation und keine Arcade-/Realtime-Serving-/
    CustomerSpawner-/manuelle Koch-/First-/Third-Person-Systeme eingefuehrt.
- Current Codex run 2026-06-06 07:41: Offenes Review-Item "Unity BuyDialog
  open-shop controller mutation" umgesetzt:
  - `ShopOpeningService` als UnityEngine-freier Open-Shop-Pfad hinzugefuegt:
    validiert doppelte/ungueltige Standortdaten und ausreichend Kapital,
    zieht `Deposit + WeeklyRent` ab und erzeugt eine neue `Shop` mit Default-
    Sortiment und Basis-Equipment.
  - `GameController.OpenShop(CityMapHotspot)` ist der einzige Intent fuer die
    Buy-Mutation; er validiert `Available`, ruft den Service auf, publiziert
    bei Fehlern Toasts und bei Erfolg Snapshot/Location/RestaurantDetail-
    Events.
  - `BuyDialogView` ruft beim Confirm nur `controller.OpenShop(hotspot)` auf;
    Cancel/Close mutieren weiterhin nichts.
  - `CityMapHotspot.MarkOwned(shop)` macht den ausgewaehlten Standort nach
    erfolgreichem Kauf im CityMap-Flow als owned nutzbar.
  - Keine RestaurantDetail-Funktionsmutation, keine Upgrades, kein Price-
    Editing, keine Save-/PlayerPrefs-/Filesystem-Logik und keine Arcade-/
    Realtime-Serving-/CustomerSpawner-/manuelle Koch-/First-/Third-Person-
    Systeme eingefuehrt.
- Current Codex run 2026-06-05 21:00: Offenes Review-Item "Unity Day Sim
  controller intent + DayReport shell" umgesetzt:
  - `GameController.SimulateDay()` als Tagesabschluss-Intent hinzugefuegt; er
    ruft ausschliesslich den vorhandenen `GameEngine.SimulateDay(GameState)`-
    Pfad auf.
  - `DayEndedEvent` publiziert den neuen `DailyRecord` samt
    `DaySimulationResult`; danach wird der sichtbare Zustand ueber
    `StateSnapshotChangedEvent` aktualisiert.
  - `LocationSheetView` bietet im CityMap-HUD `TAG BEENDEN` als Controller-
    Intent an; keine direkte UI-State-Mutation.
  - `DayReportView` als kleine IMGUI-Shell hinzugefuegt und in
    `CityMapBootstrap` verdrahtet.
  - Report zeigt Tag, Umsatz, Kosten, Gewinn und Kunden; `ZURUECK` schliesst
    nur die Shell und kehrt zur CityMap zurueck.
  - Keine BuyDialog-/RestaurantDetail-Funktionsmutation, keine neuen
    Wirtschaftsformeln in der UI, keine Arcade-/Realtime-Serving-Systeme.
- Current Codex run 2026-06-05 17:30: Offenes Review-Item "Unity GameEngine
  day simulation foundation" umgesetzt:
  - `unity/Assets/Scripts/Simulation/GameEngine.cs` als UnityEngine-freier
    Foundation-Pfad fuer `SimulateDay(GameState)` hinzugefuegt.
  - Offene Filialen berechnen deterministische Tageswerte aus Shop-/Menu-/
    Employee-/Equipment-/Brand-/Difficulty-Daten.
  - `Cash`, `CurrentDay`, `CurrentHour`, `TotalRevenue`, `TotalProfit`,
    `CustomersServedTotal` und `History` werden nur in diesem GameEngine-Pfad
    mutiert.
  - Geschlossene Filialen liefern 0 Umsatz und 0 Kunden.
  - Tests fuer deterministische Totals, geschlossene Filialen und sichtbare
    Difficulty-/Shop-Wirkung ergaenzt.
  - Keine Arcade-Cooking-, Echtzeit-Serving-, CustomerSpawner-, manuelle
    Koch-, BuyDialog-, RestaurantDetail- oder UI-Systeme eingefuehrt.
- Current Codex run 2026-06-05 16:06: Offenes Review-Item "Unity SaveService
  API hardening + fixture-shaped JSON" umgesetzt:
  - `SaveService` bleibt eine UnityEngine-freie Instanz-API mit
    `Serialize(GameState)` / `Deserialize(string)` und string-in/string-out.
  - `docs/UNITY_SAVE_COMPAT.md` dokumentiert die Instanz-API zusaetzlich zum
    aktuellen MVP-Feldvertrag und Dart-kompatiblen Enum-Strings.
  - `SaveServiceTests` pruefen jetzt explizit lower-camelCase JSON und keine
    PascalCase-Modellnamen fuer zentrale Felder.
  - Nicht-triviale Roundtrip-Abdeckung umfasst Shops, Menu, Equipment,
    Employees, EmployeePool, Competitors, Loans, Brand, ids/upgrades sowie
    Tutorial-/Event-Felder.
  - Missing/null optionale Collections werden ohne Exception zu nutzbaren
    leeren Collections/defaults deserialisiert.
  - Guardrail-Scan fuer SaveService ist frei von UnityEngine, PlayerPrefs,
    Dateisystem, GameEngine/Day-Sim, BuyDialog, Arcade und Serving.
- Current Codex run 2026-06-05 11:00: Offenes Review-Item "Unity SaveService
  compatibility foundation" umgesetzt:
  - `docs/UNITY_SAVE_COMPAT.md` mit Mapping fuer den aktuell portierten
    MVP-`GameState` und Untermodelle angelegt.
  - `unity/Assets/Scripts/Save/SaveService.cs` als UnityEngine-freier
    String-JSON-Roundtrip fuer `GameState` hinzugefuegt.
  - JSON-Feldnamen sind lower camelCase; Enum-Werte werden ueber vorhandene
    `EnumNames`/`EmployeeEnumNames` als Dart-kompatible Strings gemappt.
  - Save-Code in das Unity-Logik-Testprojekt eingebunden.
  - Fokussierte Roundtrip-/Enum-String-Tests fuer Shop/Menu/Equipment/
    Employees/SizeTier, Competitors, Loans, Difficulty und BrandStats ergaenzt.
  - Keine PlayerPrefs-/Dateisystem-Persistenz, keine Buy-/Cash-/Shop-Mutation,
    keine Preis-/Ausbau-Aktion, keine GameEngine-/Day-Sim- und keine
    Arcade-Cooking-/Realtime-Serving-Logik.
- Current Codex run 2026-06-05: Offenes Review-Item "CityMap RestaurantDetail
  shell via GameController" umgesetzt:
  - `RestaurantDetailView` als presentation-only IMGUI-Shell fuer
    `RestaurantDetailRequestedEvent` hinzugefuegt.
  - `CityMapBootstrap` verdrahtet die Detail-UI ueber den bestehenden
    `GameController`/`EventBus`.
  - Owned-CTA `OPTIMIEREN` bleibt ueber
    `GameController.RequestRestaurantDetail(selected)`.
  - Shell oeffnet nur fuer einen gueltigen Shop aus `GameState.Shops` und zeigt
    Shop-/Location-Identitaet.
  - Tabs/Sektionen fuer `Sortiment`, `Ausbau`, `Equipment`, `Personal` und
    `Marketing`; Stubs sind read-only markiert.
  - Close/Zurueck und neue Location-Auswahl schliessen nur die Detail-Shell.
  - Keine Preis-, Shop-, SizeTier-, Cash-, Buy-, SaveService-, Day-Sim/
    GameEngine- oder Arcade-Cooking-Logik.
- Current Codex run 2026-06-05: Offenes Review-Item "CityMap BuyDialog shell
  via GameController" umgesetzt:
  - `BuyDialogView` als IMGUI-Shell fuer `BuyDialogRequestedEvent`
    hinzugefuegt.
  - `CityMapBootstrap` verdrahtet die BuyDialog-UI ueber den bestehenden
    `GameController`/`EventBus`.
  - Available-Hotspots behalten das LocationSheet; `FILIALE EROEFFNEN`
    oeffnet den Dialog ueber den Controller-Intent.
  - Dialog zeigt Standortname, Lage/Stadt, Kaution, Wochenmiete und Kapital
    nach Kaution aus vorhandenen Hotspot-/State-Daten.
  - Abbrechen oder neue Location-Auswahl schliesst nur den Dialog.
  - Confirm ist sichtbar, aber deaktiviert/als "NOCH NICHT AKTIV" markiert.
  - Keine Buy-/Cash-/Shop-Mutation, kein RestaurantDetail, kein Upgrade, kein
    SaveService, keine Day-Sim/GameEngine- oder Arcade-Cooking-Logik.
- Current Codex run 2026-06-05: Offenes Review-Item "CityMap selection focus
  tween" umgesetzt:
  - `CityMapCameraController` kann per `FocusOn(Vector3)` weich auf einen
    Hotspot fokussieren und nutzt die bestehenden x/z-Bounds.
  - `CityMapBootstrap` verbindet `LocationSelectedEvent` mit dem
    Camera-Controller, damit nur nicht-locked Selections fokussieren.
  - Locked-Hotspots bleiben Toast-only ueber `GameController.SelectLocation`;
    kein `LocationSelectedEvent`, kein LocationSheet-Wechsel.
  - Keine BuyDialog-, RestaurantDetail-Mutation-, Upgrade-, SaveService-,
    Day-Sim/GameEngine- oder Arcade-Cooking-Logik hinzugefuegt.
- Current Codex run 2026-06-05: Offenes Review-Item "CityMap available KPI
  order polish" umgesetzt:
  - `LocationSheetView` rendert KPI-Kacheln jetzt zentral ueber
    `MetricLabel(index)` / `MetricValue(index)`.
  - Available-Reihenfolge ist `TRAFFIC`, `MIETE`, `KAUTION`, `KONKURRENZ`.
  - Owned-Reihenfolge bleibt `MARKTANTEIL`, `TRAFFIC`, `MIETE`, `PROGNOSE`.
  - Competitor/locked Output bleibt erhalten; Locked-Auswahl bleibt Toast-only.
  - Keine BuyDialog-, RestaurantDetail-Mutation-, Upgrade-, SaveService-,
    Day-Sim/GameEngine- oder Arcade-Cooking-Logik hinzugefuegt.
- Current Codex run 2026-06-05: Offenes Review-Item "CityMap pre-step-4
  presentation fixes" umgesetzt:
  - `LocationSheetView` zeigt fuer Owned KPI 1 `MARKTANTEIL` und KPI 4
    `PROGNOSE`.
  - `LocationSheetView` zeigt fuer Available KPI 4 `KONKURRENZ`.
  - Locked-Hotspots bleiben Toast-only ueber `GameController.SelectLocation`;
    kein `LocationSelectedEvent`, kein Sheet-Wechsel auf locked.
  - Keine BuyDialog-, RestaurantDetail-Mutation-, Upgrade-, SaveService-,
    Day-Sim/GameEngine- oder Arcade-Cooking-Logik hinzugefuegt.
- Current Codex run #1327b5f0: Pflichtdateien gelesen; `REVIEW_QUEUE.md`
  ist weiterhin `Status: empty`, daher keine Code-Aenderungen vorgenommen.
  Management-Spiel-Richtung bestaetigt; keine Arcade-Cooking- oder
  Echtzeit-Serving-Systeme hinzugefuegt.
- Current Codex run #61: GameController/EventBus-Vertrag fuer den Unity
  Vertical Slice festgelegt:
  - `Core/EventBus.cs` als UnityEngine-freier Publish/Subscribe-Bus.
  - `App/GameController.cs` als zentrale Intent-Grenze fuer Location-Auswahl,
    BuyDialog-Anfrage, RestaurantDetail-Anfrage und Toasts.
  - `CityMapBootstrap` verdrahtet Selection -> GameController -> Events.
  - `LocationSheetView` hoert auf Controller-Events und feuert nur Intents;
    keine direkte State-Mutation in der UI.
  - `EventBusTests.cs` ergaenzt.
  - Keine Buy-/Upgrade-/Day-Sim-Wirtschaftslogik implementiert; keine
    Arcade-Cooking- oder Echtzeit-Serving-Systeme hinzugefuegt.
- Current Codex run #60: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #59: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #58: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #57: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #56: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #55: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #54: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #53: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #52: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #51: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #50: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #49: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #48: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #47: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #46: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #45: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #44: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #43: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #42: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #41: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #40: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #39: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #38: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #37: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #36: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #35: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #34: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #33: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #32: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #31: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #30: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #29: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #28: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #27: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #26: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #25: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #24: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #23: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #22: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #21: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #20: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #19: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #18: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #17: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #16: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run #15: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run: `REVIEW_QUEUE.md` is `Status: empty`; no open Claude
  review items were present, so no review fixes were implemented.
- Current Codex run: `REVIEW_QUEUE.md` was already `Status: empty`; no open
  Claude review items were present to implement.
- Restored the missing `LocationPersonality` extension import in
  `lib/ui/screens/open_shop_screen.dart` so the requested validation passes.
- `docs/UNITY_CITY_MAP_UX.md` Section 10 Schritte 1-3 umgesetzt:
  `unity/Assets/Scenes/CityMap.unity`, Runtime-Bootstrap, isometrische Kamera
  mit Pan/Zoom, statische Hotspots mit `owned`, `available`, `locked`,
  `competitor`, und read-only `LocationSheet` gegen Dummy/current State.
- Nachpruefung: `CityMapView` nutzt den Bootstrap-`GameState` fuer eigene
  Filiale/Konkurrenzdaten; Hotspot-Auswahl erhaelt die urspruengliche Marker-Skalierung.
- Mini-Fix 2026-06-02: `LocationSheetView.Initialize` nimmt den aktuellen
  `GameState` entgegen; HUD zeigt Company/Cash/Tag aus dem Dummy/current State.
- Kamerawinkel und Bounds verifiziert: Bootstrap nutzt Euler(30, 45, 0),
  `CityMapCameraController.xBounds` ist auf [-6, 6] erweitert.
- `lib/ui/screens/open_shop_screen.dart` importiert wieder die bestehende
  `LocationPersonality`-Extension, damit `flutter analyze` gruen ist.
- Keine Buy-/Upgrade-/Simulate-Day-Aktionen implementiert; Buttons sind bewusst
  nicht interaktiv und verweisen auf GameController/Intent-Folgearbeit.
- Keine Arcade-Cooking- oder Echtzeit-Serving-Systeme hinzugefuegt.
- Revalidation 2026-06-02: Scope 1-3 gegen aktuelle lokale Dateien geprueft;
  keine zusaetzlichen Code-Aenderungen noetig.
- Verification 2026-06-02 (current Codex run): vorhandene CityMap-Implementierung
  deckt den angeforderten Scope weiter ab; keine zusaetzlichen CityMap-Code-Aenderungen
  noetig.
Next:
- Review der Map-/Sheet-Praesentation in Unity.
- Danach erst Section 10 Schritt 4+ (BuyDialog/echte State-Mutation), wenn
  GameController/EventBus-Anbindung abgestimmt ist.

## Last Validation
- Validation 2026-06-06 08:30 (Codex RestaurantDetail price controller mutation):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 103 bestanden, 0 Fehler.
  - Scope scan in App/UI/Simulation/Models fuer CustomerSpawner/Arcade/
    Serving/manual/PlayerPrefs/System.IO/first-person/third-person/
    ShopSizeTier/Cash/Equipment/Personal/Marketing/Upgrade:
    -> keine neuen gesperrten Implementierungen; Treffer sind bestehende
       GameEngine-/ShopOpening-Cash-Pfade, read-only Detail-Tabs,
       Modell-/Katalogfelder und die erlaubte Price-Mutation.
- Validation 2026-06-06 07:41 (Codex BuyDialog open-shop controller mutation):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 98 bestanden, 0 Fehler.
  - Scope scan in App/UI/Simulation/View3D plus `ShopOpeningServiceTests` fuer
    CustomerSpawner/Arcade/Serving/manual/PlayerPrefs/System.IO/File/Directory/
    first-person/third-person/RestaurantDetail/Upgrade/price editing -> nur
    erwartete bestehende RestaurantDetail-Event-/Shell-Treffer und der neue
    OpenShop-Erfolgsevent.
  - `git diff --check` -> clean, nur bestehende Git-LF/CRLF-Warnungen.
- Validation 2026-06-05 21:00 (Codex Day Sim controller intent + DayReport shell):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 95 bestanden, 0 Fehler.
  - Scope scan in `unity\Assets\Scripts\App`, `unity\Assets\Scripts\UI`,
    `unity\Assets\Scripts\Simulation`, and `DailyRecord.cs` for
    CustomerSpawner/Arcade/Serving/manual/PlayerPrefs/System.IO/
    first-person/third-person -> no matches.
  - Focused controller tests were not added because the current logic harness
    intentionally excludes UnityEngine-dependent `App`/`View3D` scripts.
- Validation 2026-06-05 17:30 (Codex GameEngine day simulation foundation):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 95 bestanden, 0 Fehler.
  - Scope scan in `unity\Assets\Scripts\Simulation` and `GameEngineTests.cs`
    for UnityEngine/PlayerPrefs/System.IO/CustomerSpawner/Arcade/Serving/
    manual/BuyDialog/RestaurantDetail -> no matches.
- Validation 2026-06-05 16:06 (Codex SaveService API/fixture polish):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 92 bestanden, 0 Fehler.
  - `rg "UnityEngine|PlayerPrefs|File\.|Directory\." unity/Assets/Scripts/Save -n`
    -> keine Treffer.
  - `rg "GameEngine|EndDay|Simulate|BuyDialog|Arcade|Serving|CustomerSpawner|ServeInteraction" unity/Assets/Scripts/Save unity-logic-tests/DoenerEmpire.Logic.Tests/SaveServiceTests.cs -n`
    -> keine Treffer.
- Validation 2026-06-05 11:06 (Claude review of commit 1ca92f3):
  - `dotnet clean unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj; dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 90 bestanden, 0 Fehler on the current worktree.
  - Scope scan in `unity/Assets/Scripts` / `docs/UNITY_SAVE_COMPAT.md` for
    Arcade/Serving/Buy/GameEngine/Day-Sim/filesystem terms -> no forbidden
    Save implementation; expected existing BuyDialog presentation hits outside
    Save and boundary wording in docs.
- Validation 2026-06-05 11:00 (Codex Unity SaveService compatibility foundation):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 90 bestanden, 0 Fehler.
- Validation 2026-06-05 10:59 (Claude review of commit f61dc4d):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/
    RestaurantDetail/GameEngine/SaveService/Day-Sim terms -> no forbidden
    implementation; only expected controller intent/presentation shell hits and
    existing model fields/comments.
- Validation 2026-06-05 (Codex CityMap RestaurantDetail shell):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/
    RestaurantDetail/GameEngine/SaveService/Day-Sim terms -> only expected
    controller intent/presentation shell hits and existing model fields/comments.
- Validation 2026-06-05 10:55 (Claude review of commit 1591e9d):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/GameEngine/
    SaveService/Day-Sim terms -> no forbidden implementation; only expected
    BuyDialog/controller intent hits and existing model fields/comments.
- Validation 2026-06-05 (Codex CityMap BuyDialog shell):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/GameEngine/
    SaveService/Day-Sim terms -> only expected controller intent/BuyDialog shell
    hits and existing model fields/comments.
- Validation 2026-06-05 10:50 (Claude review of commit e7a1b48):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/GameEngine/
    SaveService/Day-Sim terms -> no new forbidden implementation; only existing
    controller intent/model names and unrelated model fields/comments found.
- Validation 2026-06-05 (Codex CityMap selection focus tween):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
- Validation 2026-06-05 10:46 (Claude review of commit 1af4408):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/GameEngine/
    SaveService/Day-Sim terms -> no new forbidden implementation; only existing
    controller intent/model names and unrelated model fields found.
- Validation 2026-06-05 (Codex available KPI order polish):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
- Validation 2026-06-05 06:52 (Claude review of commit 333ca76):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
  - Scope scan in `unity/Assets/Scripts` for Arcade/Serving/BuyDialog/GameEngine/
    SaveService/Day-Sim terms -> no new forbidden implementation; only existing
    controller intent/event names and unrelated model comments found.
- Validation 2026-06-05 (Codex CityMap presentation fixes):
  - `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
- Validation 2026-06-04 (Codex run #1327b5f0):
  - `git status --short`
    -> clean before control-file updates.
  - `dotnet test unity-logic-tests/DoenerEmpire.Logic.Tests/DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
- Validation 2026-06-04 (Codex run #61):
  - `git status --short`
    -> existing worktree now contains the GameController/EventBus contract edits.
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 88 bestanden, 0 Fehler.
- Validation 2026-06-04 (Codex run #60):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean before control-file updates.
- Validation 2026-06-04 (Codex run #59):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean before control-file updates.
- Validation 2026-06-04 (Codex run #58):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean before control-file updates.
- Validation 2026-06-04 (Codex run #57):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-04 (Codex run #56):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG after
       control-file updates.
- Validation 2026-06-04 (Codex run #55):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean after control-file updates.
- Validation 2026-06-04 (Codex run #54):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-04 (Codex run #53):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-04 (Codex run #52):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-04 (Codex run #51):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean after control-file updates.
- Validation 2026-06-04 (Codex run #50):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean after control-file updates.
- Validation 2026-06-04 (Codex run #49):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-04 (Codex run #48):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #47):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #46):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #45):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #44):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean after control-file updates.
- Validation 2026-06-03 (Codex run #43):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-03 (Codex run #42):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #41):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #40):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-03 (Codex run #39):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-03 (Codex run #38):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #37):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #36):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #35):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #34):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #33):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #32):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #31):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #30):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #29):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #28):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #27):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-03 (Codex run #26):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #25):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #24):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #23):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #22):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-02 (Codex run #21):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #20):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #19):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #18):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #17):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG.
- Validation 2026-06-02 (Codex run #16):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- Validation 2026-06-02 (Codex run #15):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
- `cd unity-logic-tests; dotnet test .\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
  -> 86 bestanden, 0 Fehler (net8.0).
- `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
  -> No issues found.
- `git diff --check` clean (nur Git-LF/CRLF-Warnungen fuer STATUS/HANDOFF_LOG,
  `open_shop_screen.dart` und Flutter-Windows-Generated-Dateien).
- Scope-Scan fuer SaveService/GameEngine/Buy/Upgrade/Simulate-Day sowie
  Arcade-Cooking/Echtzeit-Serving in CityMap-Praesentationsdateien clean.
- Flutter-Spiellogik unveraendert; nur fehlender UI-Import fuer Analyzer repariert.
- Revalidation 2026-06-02:
  - `cd unity-logic-tests; dotnet test .\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen.
  - Scope-Scan fuer SaveService/GameEngine/Buy/Upgrade/Simulate-Day sowie
    Arcade-Cooking/Echtzeit-Serving in CityMap-Praesentationsdateien
    -> keine Treffer.
- Revalidation 2026-06-02 (current Codex run):
  - `cd unity-logic-tests; dotnet test .\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen.
- Revalidation 2026-06-02 (latest Codex run):
  - `cd unity-logic-tests; dotnet test .\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen.
  - Scope-Scan fuer SaveService/GameEngine/Buy/Upgrade/Simulate-Day sowie
    Arcade-Cooking/Echtzeit-Serving in CityMap-Praesentationsdateien
    -> keine Treffer.
- Verification 2026-06-02 (this Codex run):
  - `cd unity-logic-tests; dotnet test .\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen.
  - Scope-Scan fuer SaveService/GameEngine/Buy/Upgrade/Simulate-Day sowie
    Arcade-Cooking/Echtzeit-Serving in CityMap-Praesentationsdateien
    -> keine Treffer.
- Validation 2026-06-02 (review queue empty run):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean, nur Git-LF/CRLF-Warnungen fuer Flutter-Windows-Generated-Dateien.
- Validation 2026-06-02 (current review queue empty run):
  - `dotnet test .\unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
    -> 86 bestanden, 0 Fehler.
  - `$env:FLUTTER_SUPPRESS_ANALYTICS='true'; flutter analyze`
    -> No issues found.
  - `git diff --check`
    -> clean.
