# Advanced Vehicle Options – Änderungen vom 23.08.2026

Fortsetzung der Session vom 24.–25.07.2026 (siehe [CHANGELOG_2026-07-25.md](CHANGELOG_2026-07-25.md)). Thema heute: ein von mehreren Spielern gemeldeter Bug, bei dem sich District-/Campus-Policies (z. B. an einer Universität) mit aktivem AVO nicht mehr auswählen ließen.

## Gemeldetes Problem

Mehrere Spieler berichteten, dass sich bestimmte Policy-Einstellungen (z. B. Campus-/Varsity-Sports-Policies an Gebäuden wie Universitäten, aber auch District-Policies allgemein) nicht mehr anklicken ließen, sobald AVO aktiv war. Reproduzierbar auch in einem vanilla Spielstand ganz ohne DLC. Mit AVO deaktiviert funktionierten die Policies einwandfrei.

## Untersuchungsweg

### 1. Falsche Spur: `MainToolbar`/Toolbar-Tab-Integration (echter Bug, aber nicht die Ursache)

Erste Analyse (Dekompilierung der Original-Spiel-Assembly per `ilspycmd`) zeigte einen echten, seit der allerersten Version des Mods (2018, Ur-Autor SamSam) bestehenden Fehler: AVO fügt seinen eigenen Toolbar-Button per `toolStrip.AddTab(...)` direkt in dieselbe von `MainToolbar`/`GameMainToolbar` verwaltete Tab-Liste ein wie die eingebauten Werkzeuge (Roads, Districts, **Policies**, ...). Die spieleigene `RefreshPanel()`-Methode baut diese Liste bei jedem Level-Load, Meilenstein und Sprachwechsel anhand einer rein positionsbasierten Zählung (`m_ObjectIndex`) neu auf. Landet AVOs Tab nicht strikt am Ende der Liste, verschieben sich alle nachfolgenden eingebauten Tabs auf falsche Positionen und werden mit der falschen Werkzeug-Identität neu verdrahtet.

**Fix (bleibt drin, war ein echter Bug):** [Patches/MainToolbarPatch.cs](Patches/MainToolbarPatch.cs) – Harmony-Postfix auf `MainToolbar.RefreshPanel()`, verschiebt AVOs Tab per `BringToFront()` (offizielle Colossal-API) nach jedem Aufruf garantiert ans Ende der Liste.

**Ergebnis:** Per Logging verifiziert, dass die Toolbar-Struktur nach diesem Fix sauber ist (26 eingebaute Tabs + AVO korrekt an Position 27). Das Policy-Problem bestand aber **trotzdem weiter** – diese Spur war also ein echter, aber unabhängiger Bug, nicht die gesuchte Ursache.

### 2. Tatsächliche Ursache gefunden: `LocaleManager.ForceReload()`

Direkter Blick ins Live-Log beim Policy-Klick zeigte die eigentliche Exception, tief im Spielcode, ganz ohne AVO-Bezug im Stacktrace:

```
InvalidOperationException: Already in the same thread. Call directly
  at DistrictWorldInfoPanel.OnPoliciesChanged (Policies policy)
  at DistrictManager.UnsetParkPolicy (Policies policy, Byte park)
  at CampusWorldInfoPanel.RefreshPolicies (...)
  at CampusWorldInfoPanel.RefreshPolicyLists ()
  at CampusWorldInfoPanel+<TogglePolicy>...
```

`DistrictWorldInfoPanel.OnPoliciesChanged` ruft intern `ThreadHelper.dispatcher.Dispatch(UpdatePolicies)` auf – eine spieleigene Methode, die Aufrufe sicher auf den Haupt-Thread verlagert und **wirft**, wenn sie von dem Thread aus aufgerufen wird, auf den sie eigentlich verlagern soll (Re-Entranz-Schutz).

**Verdacht:** `LocaleManager.ForceReload()` – eine Session zuvor eingebaute, sehr weitreichende, synchrone Neu-Ladung des kompletten spieleigenen Locale-Systems, ursprünglich als Versuch gedacht, Fahrzeugnamen nach einem Sprachwechsel zu aktualisieren – störte dabei offenbar einen globalen Threading-/Dispatch-Zustand des Spiels.

**Wichtig:** `ForceReload()` war zu diesem Zeitpunkt bereits **funktional überflüssig** – der eigentliche Fahrzeugnamen-Fix läuft längst über `VehicleOptions.localizedName` als live berechnete Property (siehe [CHANGELOG_2026-07-25.md](CHANGELOG_2026-07-25.md), Abschnitt 7), die bei jedem Zugriff frisch aus dem Spiel gelesen wird und dafür keinen Reload braucht.

**Fix:** `LocaleManager.ForceReload()`-Aufruf in [AdvancedVehicleOptions.cs](AdvancedVehicleOptions.cs) (`OnLevelLoaded`) ersatzlos entfernt.

**Bestätigt:** Nach dem Entfernen funktioniert die Policy-Auswahl im Spiel wieder einwandfrei (live getestet).

## Fazit

Zwei unabhängige Themen an einem Tag:
1. Ein 6+ Jahre alter, latenter Bug in der Toolbar-Integration wurde gefunden und behoben (schadet nicht, hat aber die Policy-Sache nicht verursacht).
2. Die eigentliche Ursache war eine Nebenwirkung eines eigenen, mittlerweile überflüssigen Fixes aus der Vorsession – entfernt, Problem gelöst.

**Build-Status:** Erfolgreich gebaut (`dotnet build`, 0 Fehler/Warnungen) und automatisch deployed. Fix live im Spiel bestätigt.
