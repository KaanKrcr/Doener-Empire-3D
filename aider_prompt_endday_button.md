# Prompt: "Tag beenden" Button zur City Map hinzufügen

## Änderung in `lib/ui/screens/city_map_screen.dart`

### 1. Importe HINZUFÜGEN (unter den bestehenden imports):
```dart
import '../../services/haptics_service.dart';
import '../../services/sound_service.dart';
import '../widgets/day_end_dialog.dart';
import '../widgets/mission_banner.dart';
import '../widgets/quarterly_report_dialog.dart';
import '../widgets/weekly_report_dialog.dart';
import '../screens/campaign_screen.dart';
```

### 2. State-Variable
Füge `bool _endingDay = false;` im `_CityMapScreenState` hinzu (als erstes Feld, vor `CityMapLocation? _selected;`).

### 3. `_endDay()` Methode
Füge im `_CityMapScreenState` folgende Methode hinzu:
```dart
Future<void> _endDay() async {
  if (_endingDay) return;
  Haptics.medium();
  SoundService.play(Sfx.dayend);
  setState(() => _endingDay = true);
  final notifier = ref.read(gameProvider.notifier);
  notifier.endDay();
  final result = notifier.lastDayResult;
  if (result != null && mounted) {
    await DayEndDialog.show(context, result);
    if (result.missionCompleted != null && mounted) {
      await MissionCompletedDialog.show(context, result.missionCompleted!);
    }
    if (result.quarterlyReport != null && mounted) {
      await QuarterlyReportDialog.show(context, result.quarterlyReport!);
    }
    if (result.chapterCompleted != null && mounted) {
      await CampaignChapterDialog.show(context, result.chapterCompleted!);
    }
    if (result.weeklyReport != null && mounted) {
      await WeeklyReportDialog.show(context, result.weeklyReport!);
    }
    if (result.taxPaid > 0 && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Steuern (30 Tage): -${_fmt.format(result.taxPaid)} €'),
          duration: const Duration(seconds: 3),
        ),
      );
    }
    if (result.bankrupt && mounted) {
      await BankruptcyDialog.show(context);
    }
  }
  if (mounted) setState(() => _endingDay = false);
}
```

### 4. Gold-CTA Button unter dem Summary Strip
Direkt nach dem `_SummaryStrip` widget und vor dem `const SizedBox(height: 14)` und der `CityMapView` einen goldenen "Tag beenden" Button einfügen:
```dart
const SizedBox(height: 4),
Container(
  width: double.infinity,
  margin: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
  child: ElevatedButton.icon(
    onPressed: _endingDay ? null : _endDay,
    icon: Icon(
      _endingDay ? Icons.hourglass_empty : Icons.nightlight_round,
      color: Colors.white,
      size: 18,
    ),
    label: Text(
      _endingDay ? 'Tag läuft...' : 'Tag beenden  ·  Kasse machen',
      style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w700),
    ),
    style: ElevatedButton.styleFrom(
      backgroundColor: AppColors.primary,
      foregroundColor: Colors.white,
      disabledBackgroundColor: AppColors.primary.withAlpha(100),
      padding: const EdgeInsets.symmetric(vertical: 16),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
    ),
  ),
),
```

### Wichtig:
- KEINE bestehende Logik ändern
- KEINE anderen Dateien anfassen
- `flutter analyze` muss sauber sein
- `flutter test` muss 100/100
