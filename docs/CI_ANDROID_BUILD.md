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

### Pflicht — Unity-Lizenz (eine Variante)

**Variante A (Personal-Lizenz, kostenlos):**
1. Lizenz einmalig via game-ci erzeugen:
   <https://game.ci/docs/github/activation>
   (Aktivierungs-Workflow ausführen → `.ulf`-Datei herunterladen).
2. Secret `UNITY_LICENSE` = vollständiger Inhalt der `.ulf`-Datei.

**Variante B (Pro/Plus-Seriennummer):**
- `UNITY_SERIAL` = Seriennummer
- `UNITY_EMAIL` = Unity-Account-E-Mail
- `UNITY_PASSWORD` = Unity-Account-Passwort

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

- Unity-Version wird automatisch aus `unity/ProjectSettings/ProjectVersion.txt`
  gelesen (aktuell `6000.4.9f1`).
- Der erste Build ist langsam (IL2CPP + kein Library-Cache); Folge-Builds nutzen
  den `Library`-Cache und sind deutlich schneller.
- Player-Settings (Produktname, Bundle-ID `com.doenerempire.doener_empire`,
  IL2CPP, Build-Szene) sind bereits in `ProjectSettings` hinterlegt.
