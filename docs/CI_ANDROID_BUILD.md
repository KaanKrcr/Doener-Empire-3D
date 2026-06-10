# CI: Automatischer Android-Build (GitHub Actions)

Workflow: `.github/workflows/unity-android-build.yml`
Baut die Android-App via [game-ci/unity-builder](https://game.ci/) auf
GitHub-Runnern. Vorgeschaltet laufen die UnityEngine-freien Logic-Tests als Gate.

## Auslösen

- **Manuell:** Actions → „Unity Android Build" → *Run workflow* →
  Ausgabe wählen (`androidPackage` = APK, `androidAppBundle` = AAB).
- **Per Tag:** Git-Tag `android-v*` pushen (z. B. `git tag android-v1.0 && git push --tags`).

Das Ergebnis liegt danach als Artifact `doener-empire-android` am Workflow-Run.

## Benötigte Repository-Secrets

`Settings → Secrets and variables → Actions → New repository secret`

### Pflicht — Unity Personal: ALLE DREI Secrets

game-ci braucht für eine Personal-Lizenz **alle drei** Werte (sonst:
„No valid license activation strategy could be determined"):

| Secret | Inhalt |
|---|---|
| `UNITY_LICENSE` | Inhalt der Lizenzdatei. **Unity 6 (neuer Licensing-Client):** Inhalt von `%LOCALAPPDATA%\Unity\licenses\UnityEntitlementLicense.xml`. Ältere Unity: `C:\ProgramData\Unity\Unity_lic.ulf`. |
| `UNITY_EMAIL` | E-Mail des Unity-Accounts |
| `UNITY_PASSWORD` | Passwort des Unity-Accounts |

> Die Lizenz muss vorher **lokal im Unity Hub aktiviert** sein
> (Preferences → Licenses → Add → Get a free personal license).
>
> ⚠️ Der frühere `.alf`/`.ulf`-Aktivierungs-Workflow
> (`game-ci/unity-request-activation-file`) ist von game-ci **abgeschaltet**
> („This action is no longer supported") und wurde aus dem Repo entfernt.

**Pro/Plus-Alternative:** statt `UNITY_LICENSE` ein `UNITY_SERIAL`
(+ `UNITY_EMAIL` + `UNITY_PASSWORD`).

### Optional — Release-Signing

Ohne diese Secrets baut Unity mit **Debug-Signing** (gut für interne Tests).
Für Play-Store-Builds einen echten Keystore hinterlegen:

| Secret | Inhalt |
|---|---|
| `ANDROID_KEYSTORE_BASE64` | `base64 -w0 doener_empire.keystore` (PowerShell: `[Convert]::ToBase64String([IO.File]::ReadAllBytes('doener_empire.keystore'))`) |
| `ANDROID_KEYSTORE_PASS` | Keystore-Passwort |
| `ANDROID_KEYALIAS_NAME` | Key-Alias-Name |
| `ANDROID_KEYALIAS_PASS` | Key-Alias-Passwort |

> ⚠️ Den Keystore **nie ins Repo committen**. Verlust = keine App-Updates mehr
> möglich. Außerhalb des Repos sichern.

## Hinweise

- CI baut mit **Unity `6000.4.10f1`** (im Workflow gepinnt), weil game-ci kein
  Docker-Image für das lokal installierte `6000.4.9f1` anbietet — eine
  Patch-Version Unterschied, öffnet das Projekt problemlos.
- Der erste Build ist langsam (IL2CPP + kein Library-Cache); Folge-Builds nutzen
  den `Library`-Cache und sind deutlich schneller.
- Player-Settings (Produktname, Bundle-ID `com.doenerempire.doener_empire`,
  IL2CPP, Build-Szene) sind bereits in `ProjectSettings` hinterlegt.

## ⚠️ Bekannte Einschränkung: Personal-Lizenz + Cloud-CI

Unity-**Personal**-Lizenzen aktivieren sich mit dem neuen Licensing-Client
(Unity 2021.3.16+ / alle Unity 6) **nicht zuverlässig** in game-ci/Docker:
game-ci erwartet im `UNITY_LICENSE` ein klassisches `.ulf` mit Serial-Feld
(`<DeveloperData>`); die neue `UnityEntitlementLicense.xml` hat das nicht →
Aktivierung scheitert mit `[Licensing::Client] Code 20110 (serial invalid)`.

**Konsequenz / empfohlene Wege:**
- **Lokaler Build im Editor** (Personal-Lizenz, funktioniert zuverlässig) —
  Projekt ist dafür vollständig vorkonfiguriert (Szene, Player-Settings,
  IL2CPP, Android Build Support). Siehe Build-Checkliste.
- **Cloud-CI** funktioniert sauber mit **Unity Pro/Plus** (`UNITY_SERIAL`).

Das Logic-Test-Gate (`dotnet test`) im Build-Workflow läuft unabhängig davon
und bleibt als Regressionsschutz aktiv.
