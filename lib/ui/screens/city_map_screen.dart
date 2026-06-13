import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../core/constants.dart';
import '../../core/theme.dart';
import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../providers/game_provider.dart';
import '../../services/game_engine.dart';
import '../../services/haptics_service.dart';
import '../../services/location_engine.dart';
import '../../services/sound_service.dart';
import '../widgets/city_map_view.dart';
import '../widgets/day_end_dialog.dart';
import '../widgets/mission_banner.dart';
import '../widgets/quarterly_report_dialog.dart';
import '../widgets/weekly_report_dialog.dart';
import '../widgets/bankruptcy_dialog.dart';
import '../screens/campaign_screen.dart';

final _fmt = NumberFormat('#,##0', 'de_DE');

class CityMapScreen extends ConsumerStatefulWidget {
  final String cityId;
  const CityMapScreen({super.key, required this.cityId});

  @override
  ConsumerState<CityMapScreen> createState() => _CityMapScreenState();
}

class _CityMapScreenState extends ConsumerState<CityMapScreen> {
  bool _endingDay = false;
  CityMapLocation? _selected;

  CityData get city => kAllCities.firstWhere((c) => c.id == widget.cityId);

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
  
    }
    if (mounted) setState(() => _endingDay = false);
  }

  @override
  Widget build(BuildContext context) {
    final game = ref.watch(gameProvider)!;
    final locations = LocationEngine.locationsFor(city);
    final selected =
        _selected ?? (locations.isNotEmpty ? locations.first : null);
    final cityShops =
        game.shops.where((shop) => shop.cityId == city.id).toList();
    final competition = LocationEngine.competitionBrief(game, city.id);

    // Berechne Gesamtumsatz für alle eigenen Filialen in dieser Stadt
    double totalRevenue = 0;
    for (final shop in cityShops) {
      totalRevenue += GameEngine.calculateDailyRevenue(
        shop,
        day: game.currentDay,
        state: game,
      );
    }

    // ── Insolvenz-Listener ─────────────────────────────────────────
    ref.listen(gameProvider, (prev, next) {
      if (next == null) return;
      final wasOk = prev == null || prev.cash >= 0;
      final isNowBad = next.cash < 0;
      if (wasOk && isNowBad && mounted) {
        BankruptcyDialog.show(context);
      }
    });

    return Scaffold(
      backgroundColor: AppColors.bg,
      body: SafeArea(
        bottom: false,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Premium Header
            _PremiumHeader(
              cityName: city.name,
              cash: game.cash,
            ),
            // Summary Strip
            _SummaryStrip(
              totalRevenue: totalRevenue,
              shopCount: cityShops.length,
            ),
            // Gold-CTA Button unter dem Summary Strip
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
            // City Map und restlicher Content
            Expanded(
              child: ListView(
                padding: const EdgeInsets.fromLTRB(16, 10, 16, 24),
                children: [
                  CityMapView(
                    city: city,
                    locations: locations,
                    shops: cityShops,
                    selected: selected,
                    onSelect: (location) => setState(() => _selected = location),
                  ),
                  const SizedBox(height: 16),
                  if (selected != null)
                    _LocationPanel(
                      city: city,
                      location: selected,
                      shopCount: cityShops
                          .where((shop) => shop.locationName == selected.template.name)
                          .length,
                      cash: game.cash,
                      competition: competition,
                      onOpenShop: () => context.push(
                        '/open-shop/${city.id}?location=${Uri.encodeComponent(selected.template.name)}',
                      ),
                    ),
                  const SizedBox(height: 14),
                  if (cityShops.isNotEmpty) ...[
                    Padding(
                      padding: const EdgeInsets.only(bottom: 8),
                      child: Text(
                        'Deine Filialen',
                        style: AppText.label(color: AppColors.secondary),
                      ),
                    ),
                    for (final shop in cityShops)
                      _ShopMapCard(
                        title: shop.displayName,
                        subtitle: '${shop.locationName} · ${shop.reputation.toStringAsFixed(1)} ★',
                        revenue: GameEngine.calculateDailyRevenue(
                          shop,
                          day: game.currentDay,
                          state: game,
                        ),
                        onTap: () => context.push('/shop/${shop.id}'),
                      ),
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _PremiumHeader extends StatelessWidget {
  final String cityName;
  final double cash;

  const _PremiumHeader({
    required this.cityName,
    required this.cash,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.fromLTRB(16, 12, 16, 12),
      decoration: const BoxDecoration(
        color: Color(0xFF141010),
        border: Border(
          bottom: BorderSide(color: AppColors.border, width: 1),
        ),
      ),
      child: Row(
        children: [
          Expanded(
            child: Text(
              cityName,
              style: const TextStyle(
                fontFamily: 'Baloo2',
                fontSize: 24,
                fontWeight: FontWeight.w700,
                color: Color(0xFFD46816), // Gold/Amber
              ),
            ),
          ),
          Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(
                Icons.account_balance_wallet_rounded,
                color: Color(0xFFD46816),
                size: 20,
              ),
              const SizedBox(width: 6),
              Text(
                '${_fmt.format(cash)} €',
                style: const TextStyle(
                  fontFamily: 'Baloo2',
                  fontSize: 18,
                  fontWeight: FontWeight.w700,
                  color: Color(0xFFD46816),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _SummaryStrip extends StatelessWidget {
  final double totalRevenue;
  final int shopCount;

  const _SummaryStrip({
    required this.totalRevenue,
    required this.shopCount,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: const Color(0xFF3D2E22),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFF3A2C20)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(40),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        children: [
          Expanded(
            child: _SummaryCard(
              icon: Icons.trending_up_rounded,
              label: 'Umsatz',
              value: '${_fmt.format(totalRevenue)} €',
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: _SummaryCard(
              icon: Icons.storefront_rounded,
              label: 'Filialen',
              value: '$shopCount',
            ),
          ),
        ],
      ),
    );
  }
}

class _SummaryCard extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;

  const _SummaryCard({
    required this.icon,
    required this.label,
    required this.value,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: const Color(0xFF3D2E22),
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          Icon(
            icon,
            color: const Color(0xFFD46816),
            size: 24,
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: const TextStyle(
                    fontSize: 11,
                    color: Color(0xFFC4B5A0), // secondary text
                  ),
                ),
                Text(
                  value,
                  style: const TextStyle(
                    fontFamily: 'Baloo2',
                    fontSize: 16,
                    fontWeight: FontWeight.w700,
                    color: Color(0xFFFFFAE6), // warm white
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _LocationPanel extends StatelessWidget {
  final CityData city;
  final CityMapLocation location;
  final int shopCount;
  final double cash;
  final CityCompetitionBrief competition;
  final VoidCallback onOpenShop;

  const _LocationPanel({
    required this.city,
    required this.location,
    required this.shopCount,
    required this.cash,
    required this.competition,
    required this.onOpenShop,
  });

  @override
  Widget build(BuildContext context) {
    final deposit = location.depositFor(city);
    final canAfford = cash >= deposit;
    final forecast = LocationEngine.forecastOpening(city, location);
    final breakEven = forecast.breakEvenDays == null
        ? 'kritisch'
        : '${forecast.breakEvenDays} Tage';
    final cashAfterDeposit = cash - deposit;
    
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: const Color(0xFF3D2E22),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFFD46816).withAlpha(90)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(40),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header mit Standortname
          Row(
            children: [
              Text(location.icon, style: const TextStyle(fontSize: 28)),
              const SizedBox(width: 10),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      location.label,
                      style: const TextStyle(
                        fontFamily: 'Baloo2',
                        fontSize: 20,
                        fontWeight: FontWeight.w700,
                        color: Color(0xFFD46816), // Gold
                      ),
                    ),
                    Text(
                      location.audience,
                      style: const TextStyle(
                        color: Color(0xFFC4B5A0),
                        fontSize: 12,
                      ),
                    ),
                  ],
                ),
              ),
              if (shopCount > 0)
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: const Color(0xFFD46816).withAlpha(35),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: const Color(0xFFD46816)),
                  ),
                  child: Text(
                    '$shopCount Filiale${shopCount > 1 ? 'n' : ''}',
                    style: const TextStyle(
                      color: Color(0xFFD46816),
                      fontWeight: FontWeight.w600,
                      fontSize: 12,
                    ),
                  ),
                ),
            ],
          ),
          const SizedBox(height: 14),
          // Detail-Zeilen
          Row(
            children: [
              _PanelStat(
                label: 'Score',
                value: '${location.attractivenessScore(city).round()}/100',
              ),
              _PanelStat(
                label: 'Traffic',
                value: _fmt.format(location.footTrafficFor(city)),
              ),
              _PanelStat(
                label: 'Miete',
                value: '${_fmt.format(location.weeklyRentFor(city))} €',
              ),
            ],
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              _PanelStat(
                label: 'Prognose',
                value: '${_fmt.format(forecast.estimatedProfitPerDay.round())} EUR/Tag',
              ),
              _PanelStat(label: 'Break-even', value: breakEven),
              _PanelStat(
                label: 'Cash danach',
                value: '${_fmt.format(cashAfterDeposit.round())} EUR',
              ),
            ],
          ),
          const SizedBox(height: 12),
          // Insights
          _Insight(
            icon: Icons.warning_amber_rounded,
            text: location.risk,
            color: const Color(0xFFF59E0B),
          ),
          const SizedBox(height: 6),
          _Insight(
            icon: Icons.lightbulb_outline_rounded,
            text: location.recommendation,
            color: const Color(0xFFD46816),
          ),
          const SizedBox(height: 6),
          _Insight(
            icon: Icons.shield_outlined,
            text: competition.recommendation,
            color: competition.hasRivals 
                ? AppColors.danger 
                : const Color(0xFFD46816),
          ),
          const SizedBox(height: 16),
          // Gold CTA Button
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: canAfford ? onOpenShop : null,
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFFD46816),
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(vertical: 14),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
              icon: const Icon(Icons.storefront_rounded),
              label: Text(
                canAfford
                    ? 'Filiale hier eröffnen · ${_fmt.format(deposit)} €'
                    : 'Zu wenig Kapital · ${_fmt.format(deposit)} €',
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _PanelStat extends StatelessWidget {
  final String label;
  final String value;

  const _PanelStat({
    required this.label,
    required this.value,
  });

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Container(
        margin: const EdgeInsets.only(right: 6),
        padding: const EdgeInsets.all(10),
        decoration: BoxDecoration(
          color: const Color(0xFF231F19),
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: const Color(0xFF3A2C20)),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              value,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(
                color: Color(0xFFFFFAE6),
                fontWeight: FontWeight.w800,
                fontSize: 13,
              ),
            ),
            const SizedBox(height: 2),
            Text(
              label,
              style: const TextStyle(
                color: Color(0xFFC4B5A0),
                fontSize: 10,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _Insight extends StatelessWidget {
  final IconData icon;
  final String text;
  final Color color;

  const _Insight({
    required this.icon,
    required this.text,
    required this.color,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, color: color, size: 18),
        const SizedBox(width: 8),
        Expanded(
          child: Text(
            text,
            style: const TextStyle(
              color: Color(0xFFC4B5A0),
              fontSize: 12,
              height: 1.35,
            ),
          ),
        ),
      ],
    );
  }
}

class _ShopMapCard extends StatelessWidget {
  final String title;
  final String subtitle;
  final double revenue;
  final VoidCallback onTap;

  const _ShopMapCard({
    required this.title,
    required this.subtitle,
    required this.revenue,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      decoration: BoxDecoration(
        color: const Color(0xFF3D2E22),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFF3A2C20)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withAlpha(30),
            blurRadius: 6,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          onTap: onTap,
          borderRadius: BorderRadius.circular(16),
          child: Container(
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(16),
              border: const Border(
                left: BorderSide(
                  color: Color(0xFFD46816), // Gold-Akzent links
                  width: 3,
                ),
              ),
            ),
            child: Padding(
              padding: const EdgeInsets.all(14),
              child: Row(
                children: [
                  const CircleAvatar(
                    backgroundColor: Color(0xFFD46816),
                    child: Text('🥙', style: TextStyle(fontSize: 18)),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          title,
                          style: const TextStyle(
                            fontFamily: 'Baloo2',
                            fontSize: 16,
                            fontWeight: FontWeight.w700,
                            color: Color(0xFFFFFAE6),
                          ),
                        ),
                        Text(
                          subtitle,
                          style: const TextStyle(
                            color: Color(0xFFC4B5A0),
                            fontSize: 12,
                          ),
                        ),
                      ],
                    ),
                  ),
                  Column(
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: [
                      Text(
                        '${_fmt.format(revenue)} €',
                        style: const TextStyle(
                          fontFamily: 'Baloo2',
                          fontSize: 16,
                          fontWeight: FontWeight.w700,
                          color: Color(0xFFD46816),
                        ),
                      ),
                      const Text(
                        '/Tag',
                        style: TextStyle(
                          color: Color(0xFFC4B5A0),
                          fontSize: 10,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
