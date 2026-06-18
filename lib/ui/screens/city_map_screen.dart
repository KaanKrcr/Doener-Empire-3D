import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../core/constants.dart';
import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/competitor_model.dart';
import '../../models/game_state.dart';
import '../../models/shop_model.dart';
import '../../providers/game_provider.dart';
import '../../services/haptics_service.dart';
import '../../services/location_engine.dart';
import '../../services/sound_service.dart';
import '../widgets/bankruptcy_dialog.dart';
import '../widgets/building_styles.dart';
import '../widgets/day_end_dialog.dart';
import '../widgets/facade_customizer.dart';
import '../widgets/map_city_overview.dart';
import '../widgets/map_deutschland.dart';
import '../widgets/map_street_view.dart';
import '../widgets/mission_banner.dart';
import '../widgets/quarterly_report_dialog.dart';
import '../widgets/weekly_report_dialog.dart';
import '../screens/campaign_screen.dart' show CampaignChapterDialog;

final _fmt = NumberFormat('#,##0', 'de_DE');

/// Die drei Ebenen des Karten-Systems.
enum _MapLevel { deutschland, city, street }

/// 3-Ebenen-City-Map: Deutschlandkarte → Vogelperspektive → 2.5D-Straßenzug.
class CityMapScreen extends ConsumerStatefulWidget {
  final String cityId;
  const CityMapScreen({super.key, required this.cityId});

  @override
  ConsumerState<CityMapScreen> createState() => _CityMapScreenState();
}

class _CityMapScreenState extends ConsumerState<CityMapScreen> {
  bool _endingDay = false;
  late _MapLevel _level;
  late String _cityId;
  CityMapLocation? _location;

  @override
  void initState() {
    super.initState();
    _cityId = widget.cityId;
    _level = _MapLevel.city; // Einstieg: Stadtplan der gewählten Stadt
  }

  CityData get _city => kAllCities.firstWhere(
        (c) => c.id == _cityId,
        orElse: () => kAllCities.first,
      );

  // ── Tag beenden ────────────────────────────────────────────────────────
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
            content:
                Text('Steuern (30 Tage): -${_fmt.format(result.taxPaid)} €'),
            duration: const Duration(seconds: 3),
          ),
        );
      }
    }
    if (mounted) setState(() => _endingDay = false);
  }

  // ── Navigation zwischen den Ebenen ───────────────────────────────────────
  void _onBack() {
    switch (_level) {
      case _MapLevel.street:
        setState(() => _level = _MapLevel.city);
        break;
      case _MapLevel.city:
        setState(() => _level = _MapLevel.deutschland);
        break;
      case _MapLevel.deutschland:
        context.pop();
        break;
    }
  }

  void _onSelectCity(String cityId) {
    setState(() {
      _cityId = cityId;
      _level = _MapLevel.city;
    });
  }

  void _onEnterLocation(CityMapLocation loc) {
    setState(() {
      _location = loc;
      _level = _MapLevel.street;
    });
  }

  // ── Aktionen im Straßenzug ───────────────────────────────────────────────
  void _onManage(Shop shop) => context.push('/shop/${shop.id}');

  void _onOpenFree(CityMapLocation loc) {
    context.push(
      '/open-shop/$_cityId?location=${Uri.encodeComponent(loc.template.name)}',
    );
  }

  void _onAcquire(Competitor c) {
    ref.read(gameProvider.notifier).acquireCompetitor(c);
    Haptics.medium();
    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('${c.name} übernommen 🤝')),
      );
    }
  }

  Future<void> _onCustomize(Shop shop) async {
    await FacadeCustomizer.show(
      context,
      currentColor: shop.accentColor,
      onPick: (argb) =>
          ref.read(gameProvider.notifier).setShopAccentColor(shop.id, argb),
    );
  }

  // ── Build ─────────────────────────────────────────────────────────────────
  @override
  Widget build(BuildContext context) {
    final game = ref.watch(gameProvider)!;

    ref.listen(gameProvider, (prev, next) {
      if (next == null) return;
      final wasOk = prev == null || prev.cash >= 0;
      if (wasOk && next.cash < 0 && mounted) {
        BankruptcyDialog.show(context);
      }
    });

    final cityShops = game.shops.where((s) => s.cityId == _cityId).toList();

    final (String title, String? subtitle) = switch (_level) {
      _MapLevel.deutschland => ('🇩🇪 Deutschland', null),
      _MapLevel.city => ('${_city.emoji} ${_city.name}', 'Stadtplan'),
      _MapLevel.street => (_city.name, _location?.label),
    };

    final showEndDay = _level != _MapLevel.deutschland;

    return Scaffold(
      backgroundColor: MapPalette.bgBase,
      body: Stack(
        children: [
          Column(
            children: [
              SafeArea(
                bottom: false,
                child: _Header(
                  title: title,
                  subtitle: subtitle,
                  cash: game.cash,
                  onBack: _onBack,
                ),
              ),
              Expanded(child: _buildLevel(game, cityShops)),
            ],
          ),
          // Tag-beenden-Button: unten-rechts, über der Karte
          if (showEndDay)
            Positioned(
              right: 16,
              bottom: 24,
              child: SafeArea(
                top: false,
                child: SizedBox(
                  height: 52,
                  child: ElevatedButton.icon(
                    onPressed: _endingDay ? null : _endDay,
                    icon: Icon(
                      _endingDay ? Icons.hourglass_empty : Icons.nightlight_round,
                      size: 18,
                      color: Colors.white,
                    ),
                    label: Text(
                      _endingDay ? '...' : 'Tag beenden',
                      style: const TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w800,
                      ),
                    ),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: MapPalette.accent,
                      foregroundColor: Colors.white,
                      disabledBackgroundColor: MapPalette.accent.withAlpha(100),
                      padding: const EdgeInsets.symmetric(horizontal: 20),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(26),
                      ),
                      elevation: 8,
                      shadowColor: MapPalette.accent.withAlpha(80),
                    ),
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildLevel(GameState game, List<Shop> cityShops) {
    switch (_level) {
      case _MapLevel.deutschland:
        return MapDeutschland(
          unlockedCityIds: game.unlockedCityIds.toSet(),
          cityIdsWithShops: game.shops.map((s) => s.cityId).toSet(),
          onSelectCity: _onSelectCity,
        );
      case _MapLevel.city:
        return MapCityOverview(
          city: _city,
          locations: LocationEngine.locationsFor(_city),
          cityShops: cityShops,
          competitors: game.competitorsIn(_cityId),
          onEnterLocation: _onEnterLocation,
        );
      case _MapLevel.street:
        final loc = _location ?? LocationEngine.locationsFor(_city).first;
        Shop? playerShop;
        for (final s in cityShops) {
          if (s.locationName == loc.template.name) {
            playerShop = s;
            break;
          }
        }
        return MapStreetView(
          city: _city,
          location: loc,
          playerShop: playerShop,
          competitors: game.competitorsIn(_cityId),
          cash: game.cash,
          onManage: _onManage,
          onCustomize: _onCustomize,
          onOpenFree: _onOpenFree,
          onAcquire: _onAcquire,
        );
    }
  }
}

// ── Header (back + Titel + Cash + Tag-beenden) ─────────────────────────────
class _Header extends StatelessWidget {
  final String title;
  final String? subtitle;
  final double cash;
  
  final VoidCallback onBack;

  const _Header({
    required this.title,
    required this.subtitle,
    required this.cash,
    
    required this.onBack,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 56,
      padding: const EdgeInsets.symmetric(horizontal: 8),
      decoration: const BoxDecoration(
        color: MapPalette.bgPanel,
        border: Border(bottom: BorderSide(color: MapPalette.border)),
      ),
      child: Row(
        children: [
          IconButton(
            onPressed: onBack,
            icon: const Icon(Icons.arrow_back_ios_new_rounded,
                color: MapPalette.textMuted, size: 18),
            padding: const EdgeInsets.symmetric(horizontal: 8),
          ),
          Expanded(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(
                    fontFamily: 'Baloo2',
                    fontSize: 17,
                    fontWeight: FontWeight.w700,
                    color: MapPalette.textMain,
                  ),
                ),
                if (subtitle != null)
                  Text(
                    subtitle!,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                        fontSize: 11, color: MapPalette.textDim),
                  ),
              ],
            ),
          ),
          const SizedBox(width: 6),
          const Icon(Icons.account_balance_wallet_outlined,
              color: MapPalette.accent, size: 16),
          const SizedBox(width: 4),
          Text(
            '${_fmt.format(cash.round())} €',
            style: const TextStyle(
              fontFamily: 'Baloo2',
              fontSize: 14,
              fontWeight: FontWeight.w700,
              color: MapPalette.accent,
            ),
          ),
        ],
      ),
    );
  }
}
