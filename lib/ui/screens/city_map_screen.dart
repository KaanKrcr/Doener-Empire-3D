import 'package:flutter/material.dart';
import 'package:flutter_animate/flutter_animate.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../core/constants.dart';
import '../../core/theme.dart';
import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../providers/game_provider.dart';
import '../../services/game_engine.dart';
import '../../services/location_engine.dart';
import '../widgets/city_map_view.dart';

final _fmt = NumberFormat('#,##0', 'de_DE');

class CityMapScreen extends ConsumerStatefulWidget {
  final String cityId;
  const CityMapScreen({super.key, required this.cityId});

  @override
  ConsumerState<CityMapScreen> createState() => _CityMapScreenState();
}

class _CityMapScreenState extends ConsumerState<CityMapScreen> {
  CityMapLocation? _selected;

  CityData get city => kAllCities.firstWhere((c) => c.id == widget.cityId);

  @override
  Widget build(BuildContext context) {
    final game = ref.watch(gameProvider)!;
    final locations = LocationEngine.locationsFor(city);
    final selected =
        _selected ?? (locations.isNotEmpty ? locations.first : null);
    final cityShops =
        game.shops.where((shop) => shop.cityId == city.id).toList();
    final summary = LocationEngine.summarize(city, game.shops);
    final competition = LocationEngine.competitionBrief(game, city.id);

    return Scaffold(
      backgroundColor: AppColors.bg,
      appBar: AppBar(
        title: Text('${city.name} City-Map'),
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(16, 10, 16, 24),
        children: [
          _SummaryStrip(summary: summary, competition: competition),
          const SizedBox(height: 14),
          CityMapView(
            city: city,
            locations: locations,
            shops: cityShops,
            selected: selected,
            onSelect: (location) => setState(() => _selected = location),
          ).animate().fadeIn(duration: 260.ms).slideY(begin: 0.04, end: 0),
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
            Text('Deine Filialen',
                style: AppText.label(color: AppColors.secondary)),
            const SizedBox(height: 8),
            for (final shop in cityShops)
              _ShopMapCard(
                title: shop.displayName,
                subtitle:
                    '${shop.locationName} · ${shop.reputation.toStringAsFixed(1)} ★',
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
    );
  }
}

class _SummaryStrip extends StatelessWidget {
  final CityMapSummary summary;
  final CityCompetitionBrief competition;

  const _SummaryStrip({
    required this.summary,
    required this.competition,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.bgCard,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: AppColors.border),
      ),
      child: Row(
        children: [
          _Metric(
              label: 'Filialen',
              value: '${summary.shopCount}',
              color: AppColors.primary),
          _Metric(
              label: 'Laufkundschaft',
              value: _fmt.format(summary.totalFootTraffic),
              color: AppColors.accent),
          _Metric(
              label: 'Miete/Woche',
              value: '${_fmt.format(summary.weeklyRent)} €',
              color: AppColors.warning),
          _Metric(
            label: 'Konkurrenz',
            value: competition.pressureLabel,
            color: competition.hasRivals ? AppColors.danger : AppColors.gold,
          ),
        ],
      ),
    );
  }
}

class _Metric extends StatelessWidget {
  final String label;
  final String value;
  final Color color;
  const _Metric(
      {required this.label, required this.value, required this.color});

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(value, style: AppText.display(size: 15, color: color)),
          const SizedBox(height: 2),
          Text(label,
              style: const TextStyle(fontSize: 10, color: AppColors.textMuted)),
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
        color: AppColors.bgCard,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: AppColors.primary.withAlpha(90)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Text(location.icon, style: const TextStyle(fontSize: 28)),
              const SizedBox(width: 10),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(location.label, style: AppText.display(size: 20)),
                    Text(location.audience,
                        style: const TextStyle(
                            color: AppColors.textSecondary, fontSize: 12)),
                  ],
                ),
              ),
              if (shopCount > 0)
                Chip(
                  label: Text('$shopCount Filiale${shopCount > 1 ? 'n' : ''}'),
                  backgroundColor: AppColors.accent.withAlpha(35),
                  side: const BorderSide(color: AppColors.accent),
                ),
            ],
          ),
          const SizedBox(height: 14),
          Row(
            children: [
              _PanelStat(
                  label: 'Score',
                  value: '${location.attractivenessScore(city).round()}/100'),
              _PanelStat(
                  label: 'Traffic',
                  value: _fmt.format(location.footTrafficFor(city))),
              _PanelStat(
                  label: 'Miete',
                  value: '${_fmt.format(location.weeklyRentFor(city))} €'),
            ],
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              _PanelStat(
                label: 'Prognose',
                value:
                    '${_fmt.format(forecast.estimatedProfitPerDay.round())} EUR/Tag',
              ),
              _PanelStat(label: 'Break-even', value: breakEven),
              _PanelStat(
                label: 'Cash danach',
                value: '${_fmt.format(cashAfterDeposit.round())} EUR',
              ),
            ],
          ),
          const SizedBox(height: 14),
          _Insight(
              icon: Icons.warning_amber_rounded,
              text: location.risk,
              color: AppColors.warning),
          const SizedBox(height: 8),
          _Insight(
              icon: Icons.lightbulb_outline_rounded,
              text: location.recommendation,
              color: AppColors.accent),
          const SizedBox(height: 8),
          _Insight(
            icon: Icons.shield_outlined,
            text: competition.recommendation,
            color: competition.hasRivals ? AppColors.danger : AppColors.gold,
          ),
          const SizedBox(height: 16),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: canAfford ? onOpenShop : null,
              icon: const Icon(Icons.storefront_rounded),
              label: Text(
                canAfford
                    ? 'Filiale hier eröffnen · Kaution ${_fmt.format(deposit)} €'
                    : 'Zu wenig Kapital · ${_fmt.format(deposit)} € Kaution',
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
  const _PanelStat({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Container(
        margin: const EdgeInsets.only(right: 8),
        padding: const EdgeInsets.all(10),
        decoration: BoxDecoration(
          color: AppColors.bgSurface,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: AppColors.border),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(value,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                    color: AppColors.textPrimary, fontWeight: FontWeight.w800)),
            const SizedBox(height: 2),
            Text(label,
                style:
                    const TextStyle(color: AppColors.textMuted, fontSize: 10)),
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
  const _Insight({required this.icon, required this.text, required this.color});

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, color: color, size: 18),
        const SizedBox(width: 8),
        Expanded(
          child: Text(text,
              style: const TextStyle(
                  color: AppColors.textSecondary, fontSize: 12, height: 1.35)),
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
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        onTap: onTap,
        leading: const CircleAvatar(
          backgroundColor: AppColors.primary,
          child: Text('🥙'),
        ),
        title: Text(title,
            style: const TextStyle(
                color: AppColors.textPrimary, fontWeight: FontWeight.w700)),
        subtitle:
            Text(subtitle, style: const TextStyle(color: AppColors.textMuted)),
        trailing: Text('${_fmt.format(revenue)} €',
            style: const TextStyle(
                color: AppColors.accent, fontWeight: FontWeight.w800)),
      ),
    );
  }
}
