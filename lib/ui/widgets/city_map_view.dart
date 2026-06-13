import 'package:flutter/material.dart';

import '../../core/theme.dart';
import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/shop_model.dart';

/// Premium-Location-Cards statt isometrischer 2.5D-Karte.
/// Diese Version vermeidet Expensive Layouts (Spacer, Expanded, etc.)
/// und Scroll Conflict mit dem Eltern-ListView.
class CityMapView extends StatelessWidget {
  final CityData city;
  final List<CityMapLocation> locations;
  final List<Shop> shops;
  final CityMapLocation? selected;
  final ValueChanged<CityMapLocation> onSelect;

  const CityMapView({
    super.key,
    required this.city,
    required this.locations,
    required this.shops,
    required this.selected,
    required this.onSelect,
  });

  @override
  Widget build(BuildContext context) {
    // Kein Expanded/ListView — scrollConflict vermeiden (Eltern scrollt bereits)
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      mainAxisSize: MainAxisSize.min,
      children: [
        _CityBadge(city: city),
        const SizedBox(height: 12),
        for (int i = 0; i < locations.length; i++) ...[
          if (i > 0) const SizedBox(height: 10),
          _LocationCard(
            location: locations[i],
            city: city,
            shops: shops,
            isSelected: selected?.id == locations[i].id,
            onTap: () => onSelect(locations[i]),
          ),
        ],
      ],
    );
  }
}

// ── City Badge ────────────────────────────────────────────────────────────
class _CityBadge extends StatelessWidget {
  final CityData city;
  const _CityBadge({required this.city});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: AppColors.bgCard,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFF3A2C20)),
      ),
      child: Row(
        children: [
          Text(city.emoji, style: const TextStyle(fontSize: 28)),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(city.name,
                    style: AppText.display(size: 18, weight: FontWeight.w800)),
                Text(city.tier.label,
                    style: const TextStyle(
                        color: AppColors.textSecondary, fontSize: 12)),
              ],
            ),
          ),
          const SizedBox(width: 8),
          // City Stats
          _StatChip('📍 ${city.footTrafficBase ~/ 100}k'),
          const SizedBox(width: 6),
          _StatChip('💰 €${city.rentBase ~/ 100}0'),
        ],
      ),
    );
  }
}

class _StatChip extends StatelessWidget {
  final String text;
  const _StatChip(this.text);

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: AppColors.bg.withAlpha(180),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Text(text,
          style: const TextStyle(
              color: AppColors.textPrimary,
              fontSize: 12,
              fontWeight: FontWeight.w600)),
    );
  }
}

// ── Location Card ─────────────────────────────────────────────────────────
class _LocationCard extends StatelessWidget {
  final CityMapLocation location;
  final CityData city;
  final List<Shop> shops;
  final bool isSelected;
  final VoidCallback onTap;

  const _LocationCard({
    required this.location,
    required this.city,
    required this.shops,
    required this.isSelected,
    required this.onTap,
  });

  bool get hasOwnedShop =>
      shops.any((s) => s.cityId == city.id && s.locationName == location.template.name);

  int get ownedCount =>
      shops.where((s) => s.cityId == city.id && s.locationName == location.template.name).length;

  @override
  Widget build(BuildContext context) {
    final traffic = location.footTrafficFor(city);
    final rent = location.weeklyRentFor(city);
    final deposit = location.depositFor(city);
    final score = location.attractivenessScore(city).round();

    return GestureDetector(
      onTap: onTap,
      child: Container(
        decoration: BoxDecoration(
          color: const Color(0xFF3D2E22),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: isSelected ? AppColors.primary : const Color(0xFF3A2C20),
            width: isSelected ? 2 : 1,
          ),
        ),
        padding: const EdgeInsets.only(left: 3),
        child: Container(
          decoration: BoxDecoration(
            border: Border(
              left: BorderSide(color: AppColors.primary, width: 3),
            ),
          ),
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisSize: MainAxisSize.min,
            children: [
              // Header: Name + Score
              _headerRow(score),
              const SizedBox(height: 12),
              // Info: Miete | Konkurrenz
              _infoRow('Miete', '€${rent.toStringAsFixed(0)}/Wo', 'Konkurrenz', location.risk),
              const SizedBox(height: 8),
              // Info: Kaution | Nachfrage
              _infoRow('Kaution', '€${deposit.toStringAsFixed(0)}', 'Nachfrage', _trafficLabel(traffic)),
              const SizedBox(height: 12),
              // CTA
              _ctaButton(),
            ],
          ),
        ),
      ),
    );
  }

  Widget _headerRow(int score) {
    return Row(
      children: [
        Expanded(
          child: Text(location.template.name,
              style: AppText.display(size: 16, weight: FontWeight.w800, color: AppColors.primary)),
        ),
        if (hasOwnedShop)
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
            decoration: BoxDecoration(
              color: AppColors.success.withAlpha(40),
              borderRadius: BorderRadius.circular(6),
            ),
            child: Text('EIGEN',
                style: TextStyle(color: AppColors.success, fontSize: 10, fontWeight: FontWeight.w800)),
          )
        else
          Text('★ $score',
              style: const TextStyle(color: AppColors.gold, fontSize: 14, fontWeight: FontWeight.w600)),
      ],
    );
  }

  Widget _infoRow(String l1, String v1, String l2, String v2) {
    return Row(
      children: [
        Expanded(child: _InfoCell(label: l1, value: v1)),
        const SizedBox(width: 16),
        Expanded(child: _InfoCell(label: l2, value: v2)),
      ],
    );
  }

  Widget _ctaButton() {
    return SizedBox(
      height: 48,
      child: ElevatedButton(
        onPressed: hasOwnedShop ? onTap : null,
        style: ElevatedButton.styleFrom(
          backgroundColor: AppColors.primary,
          foregroundColor: Colors.white,
          disabledBackgroundColor: AppColors.textMuted.withAlpha(80),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        ),
        child: Text(
          hasOwnedShop ? 'Verwalten' : 'Filiale eröffnen',
          style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w700),
        ),
      ),
    );
  }

  String _trafficLabel(int traffic) {
    if (traffic >= 2000) return 'Hoch';
    if (traffic >= 1000) return 'Mittel';
    return 'Niedrig';
  }
}

// ── Info Cell ─────────────────────────────────────────────────────────────
class _InfoCell extends StatelessWidget {
  final String label;
  final String value;
  const _InfoCell({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text('$label: ',
            style: const TextStyle(color: Color(0xFFC4B5A0), fontSize: 13)),
        Text(value,
            style: const TextStyle(
                color: Color(0xFFFFFAE6),
                fontSize: 13,
                fontWeight: FontWeight.w600)),
      ],
    );
  }
}
