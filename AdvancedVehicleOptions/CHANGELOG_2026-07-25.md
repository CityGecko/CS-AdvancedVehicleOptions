# Advanced Vehicle Options – Änderungen vom 24.–25.07.2026

> Hinweis: Die Änderungen dieser Session wurden mit Unterstützung eines KI-Coding-Assistenten (Claude Code von Anthropic) erarbeitet und vom Maintainer im Spiel geprüft und getestet.

Zusammenfassung aller Fixes aus der heutigen Session, inklusive Root-Cause-Analyse. Betroffene Dateien sind jeweils verlinkt.

## 1. Übersetzungs-Framework komplett ersetzt

**Ursprüngliches Problem:** "Detect Game Language" fiel bei Simplified Chinese immer auf Englisch zurück, obwohl manuelle Auswahl aus dem Dropdown funktionierte.

**Root Cause:** Der alte Parser (`Translation/TranslationFramework.cs`, jetzt entfernt) matchte die Spielsprache exakt (`ContainsKey`) gegen den Code aus der CSV-`CODE`-Zeile. Das Spiel meldet für Chinesisch aber `zh` (bestätigt im Log), während die CSV `zh_CN` enthielt – exakter String-Vergleich schlug fehl, kein Fallback vorhanden. Gleiches Problem bei Tschechisch (`cz` statt korrektem ISO-Code `cs`).

**Fix:** Das komplette Framework wurde durch eine robustere Version ersetzt (Basis: von dir bereitgestellte `Language.cs`/`Translations.cs`/`Translator.cs`, angepasst an dieses Projekt):
- [Translation/Language.cs](Translation/Language.cs) (neu)
- [Translation/Translations.cs](Translation/Translations.cs) (neu)
- [Translation/Translator.cs](Translation/Translator.cs) (neu, ersetzt `TranslationFramework.cs`)

Wichtigste Änderung: Sprachcode kommt jetzt aus dem **Dateinamen** (`zh-CN.csv` → Code `zh-CN`) statt aus der `CODE`-Zeile, kombiniert mit einem Zwei-Buchstaben-Präfix-Fallback (`FindLanguage()`), der `zh` automatisch auf `zh-CN` matcht. Angepasst an dieses Projekt: Namespace `AdvancedVehicleOptionsUID` (statt `AlgernonCommons.Translation`, um alle ~10 Aufrufer-Dateien unverändert zu lassen), `NAME`/`TRN_SYS`-Keys beibehalten (matcht bestehende CSVs), Default-Sprache `"en"` (Bug im Originalcode: `"en-EN"`), `ModUtils.GetAssemblyPath()` statt fehlendem `AssemblyUtils`.

Live im Log bestätigt:
```
Advanced Vehicle Options: game language is zh.
Advanced Vehicle Options: using translation file zh-CN for language zh.
```

## 2. Kaputte CSV-Zeile in cs-CZ.csv repariert

`AVO_MOD_OP03` enthielt eine defekte mehrzeilige Zelle mit nicht escapten doppelten Anführungszeichen (`""`) und Debug-Restmüll ("KONTEXTOVÁ ŽÁDOST"), die der alte Parser nicht korrekt verarbeiten konnte. Auf den korrekten Einzeiler zurückgesetzt (analog zu allen anderen 13 Sprachen).

## 3. en.csv: unquotiertes Komma repariert

`AVO_MOD_OP49` hatte ein rohes Komma im Wert (`If enabled, the engine properties...`) ohne Anführungszeichen. Der alte Parser tolerierte das (naiver Split am ersten Komma), der neue, RFC4180-konforme Parser hätte den Text danach abgeschnitten. Zeile jetzt korrekt gequotet.

## 4. Fehlender Übersetzungs-Key `AVO_MOD_VO16` ergänzt

**Problem:** Bank-Vans (`BankVanAI` in [VehicleOptions.cs:838](VehicleOptions.cs)) zeigten den rohen Key `AVO_MOD_VO16` statt eines Textes – der Key fehlte in **allen 14** Sprachdateien, auch in der Default-Fallback-Sprache Englisch.

**Fix:** `AVO_MOD_VO16` ("Cash Capacity: ") in allen 14 `Translation/Translations/*.csv` ergänzt, jeweils passend zum Sprachmuster der Nachbar-Keys (`VO12`–`VO15`) übersetzt.

## 5. Ungenutzte Übersetzungs-Keys entfernt

Per Abgleich aller `Translate("...")`-Aufrufe im Code (inkl. dynamisch gebauter Kategorie-Keys `AVO_MOD_CAT00`–`CAT45`) gegen alle CSVs identifiziert und aus allen 14 Dateien entfernt:
- `AVO_MOD_BGUIDESC`
- `AVO_MOD_OP47`
- `AVO_OPT_COMP_TRL` / `AVO_OPT_COMP_TRL_TT` (mehrzeilig, vollständig entfernt)
- `CODE` (Überbleibsel aus dem alten Framework, seit Punkt 1 ungenutzt)

Ergebnis: Alle 14 Dateien haben jetzt exakt 172 Keys, 1:1 deckungsgleich mit dem tatsächlichen Code-Bedarf.

## 6. Options-Panel: kein Live-Refresh bei Sprachwechsel

**Problem:** Sprache im AVO-Dropdown wechseln aktualisierte das Options-Panel selbst nicht, nur ein Wechsel der Spiel-UI-Sprache tat das.

**Fix:** In [AdvancedVehicleOptions.cs](AdvancedVehicleOptions.cs), Dropdown-Callback (`Language_DropDown`, `OnSettingsUI`): Ruft nach Sprachwechsel per Reflection `OptionsMainPanel.OnLocaleChanged()` (privat, aber via `MethodInfo` aufrufbar) auf `UIView.library.Get<OptionsMainPanel>("OptionsPanel")` auf – dieselbe Methode, die das Spiel selbst bei einem Sprachwechsel aufruft. Panel baut sich sofort neu auf, kein Neustart nötig.

## 7. Fahrzeugnamen folgten der Sprache nicht

**Problem:** Nach Sprachwechsel + neue Partie blieben Fahrzeugnamen (z. B. "Schulbus") in der vorherigen Sprache hängen, obwohl das Spiel selbst (z. B. Namen in Buslinien-Panels) korrekt umschaltete.

**Erster Versuch (verworfen):** `LocaleManager.ForceReload()` beim Laden aufzurufen – lief fehlerfrei, hatte aber nachweislich keine Wirkung auf die Fahrzeugtitel.

**Zweiter Versuch (per Coroutine verzögert) – verursachte Regression:** Führte zu einem Crash (`KeyNotFoundException` in `DefaultOptions.CheckForConflicts()`), weil Unitys `Start()`-Callback synchron VOR dem verzögerten Dictionary-Aufbau lief. Sofort zurückgerollt.

**Finaler, funktionierender Fix:** In [VehicleOptions.cs](VehicleOptions.cs), Property `localizedName`: Wert wird nicht mehr einmalig beim Laden gecacht (`m_localizedName`-Feld entfernt), sondern **live berechnet** bei jedem Zugriff (`Locale.GetUnchecked("VEHICLE_TITLE", ...)` direkt im Getter). Da die Fahrzeugliste im Panel ohnehin erst nach vollständigem Laden geöffnet wird, ist die Spiel-eigene Locale zu dem Zeitpunkt garantiert aktuell – kein Timing-Risiko, keine Unity-Lifecycle-Änderung nötig. `LocaleManager.ForceReload()` beim Laden wurde als zusätzliche Absicherung beibehalten.

## 8. Leerer Tutorial-Adviser-Popup beim Öffnen von AVO

**Problem:** Beim Fokussieren des AVO-Toolbar-Buttons zeigte das Spiel automatisch ein leeres (zuvor: mit rohen Locale-Keys wie `TUTORIAL_ADVISER_TITLE[Advanced Vehicle Options]:0` beschriftetes) Popup-Fenster.

**Root Cause:** Jeder fokussierte Toolbar-Tab löst automatisch das spieleigene `TutorialAdvisorPanel`-Singleton aus – unabhängig von der zunächst vermuteten `TutorialUITag`-Komponente (deren Entfernung keine Wirkung zeigte).

**Fix:** In [GUI/UIMainPanel.cs](GUI/UIMainPanel.cs), `eventButtonStateChanged`-Handler: Ruft bei Fokus zusätzlich `TutorialAdvisorPanel.instance.Hide()` auf (öffentliche API, kein Reflection nötig) – schließt das Popup, bevor es sichtbar wird.

## 9. Dezimaltrennzeichen-Bug (`.` bzw. `,` teils nicht eingebbar)

**Gemeldetes Problem:** Manche Nutzer konnten in Zahlenfeldern (Max Speed, Acceleration, Braking, Turning, Springs, Dampers, Lean/Nod Multiplier) je nach System keine Dezimalstellen eingeben – lokal nie reproduzierbar.

**Root Cause (zweistufig):**
1. `float.Parse(text)` ohne explizite Kultur nutzte die System-Kultur des Spielers für die Dezimaltrennzeichen-Erkennung beim Parsen des abgeschickten Texts.
2. **Eigentliche Ursache:** ColossalFrameworks `UITextField` mit `numericalOnly = true` / `allowFloats = true` filtert schon **beim Tippen** auf Zeichenebene und lässt nur **ein einziges, intern festgelegtes** Trennzeichen zu (bei betroffenen Nutzern z. B. nur `,`, während "2.5" komplett verworfen wurde). Das Komma erschien zwar im Feld, ging aber vor dem Absenden verloren bzw. wurde beim Tippen des jeweils anderen Zeichens gar nicht erst angenommen – Ergebnis: "2,5" wurde zu "25".

**Fix:** In [GUI/UIOptionPanel.cs](GUI/UIOptionPanel.cs):
- Neue Hilfsmethode `TryParseUserFloat()`: ersetzt `,` durch `.` und parst mit `CultureInfo.InvariantCulture`; bei ungültiger Eingabe wird der Wert einfach nicht übernommen (kein unbehandelter Crash mehr wie vorher bei `float.Parse`).
- `numericalOnly`/`allowFloats`-Zeichenfilter für alle sieben Dezimalfelder (Acceleration, Braking, Turning, Springs, Dampers, Lean/Nod Multiplier) deaktiviert – Validierung passiert jetzt ausschließlich beim Abschicken über `TryParseUserFloat()`.
- **Ausnahme:** `Max Speed` bleibt bewusst `numericalOnly = true` ohne `allowFloats` (rein ganzzahlig, wie ursprünglich) – kein Dezimaltrennzeichen nötig, daher kein Locale-Problem möglich.
- **Unverändert:** `Capacity`/`Special Capacity` (echte Ganzzahlfelder) – weiterhin strikt `numericalOnly = true`.

---

**Build-Status:** Alle Änderungen wurden erfolgreich gebaut (`dotnet build`, 0 Fehler/Warnungen) und automatisch ins lokale Mod-Verzeichnis deployed. Punkte 1, 6, 7, 8 und 9 wurden im laufenden Spiel live getestet und bestätigt.
