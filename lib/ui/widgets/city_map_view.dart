import 'package:flutter/material.dart';

import '../../core/theme.dart';
import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/shop_model.dart';

/// Premium Card-basierte Standortliste. Öffentliche API unverändert
/// (city/locations/shops/selected/onSelect) — nur die Render-Schicht
/// wurde durch Cards ersetzt.
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
    return Column(
      children: [
        // City Badge Header
        _CityBadge(city: city),
        const SizedBox(height: 12),
        // Location Cards Liste
        Expanded(
          child: ListView.separated(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 10),
            itemCount: locations.length,
            separatorBuilder: (_, __) => const SizedBox(height: 10),
            itemBuilder: (context, index) {
              final location = locations[index];
              return _LocationCard(
                location: location,
                city: city,
                shops: shops,
                isSelected: selected?.id == location.id,
                onTap: () => onSelect(location),
              );
            },
          ),
        ),
      ],
    );
  }
}

/// City Badge im Header
class _CityBadge extends StatelessWidget {
  final CityData city;

  const _CityBadge({required this.city});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      margin: const EdgeInsets.symmetric(horizontal: 8),
      decoration: BoxDecoration(
        color: AppColors.bgCard,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: const Color(0xFF3A2C20)),
      ),
      child: Row(
        children: [
          Text(city.emoji, style: const TextStyle(fontSize: 28)),
          const SizedBox(width: 12),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                city.name,
                style: AppText.display(
                  size: 18,
                  weight: FontWeight.w800,
                ),
              ),
              Text(
                city.tier.label,
                style: const TextStyle(
                  color: AppColors.textSecondary,
                  fontSize: 12,
                ),
              ),
            ],
          ),
          const Spacer(),
          // City Stats Kurzübersicht
          Row(
            children: [
              _StatChip(label: '📍', value: '${city.footTrafficBase ~/ 100}k'),
              const SizedBox(width: 8),
              _StatChip(label: '💰', value: '€${city.rentBase ~/ 100}0'),
            ],
          ),
        ],
      ),
    );
  }
}

class _StatChip extends StatelessWidget {
  final String label;
  final String value;

  const _StatChip({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: AppColors.bg.withAlpha(180),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(label, style: const TextStyle(fontSize: 12)),
          const SizedBox(width: 4),
          Text(
            value,
            style: const TextStyle(
              color: AppColors.textPrimary,
              fontSize: 12,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
}

/// Einzelne Location Card mit Premium Design
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

  /// Prüft ob eigene Filiale an diesem Standort
  bool get hasOwnedShop => shops.any((s) =>
      s.cityId == city.id && s.locationName == location.template.name);

  /// Anzahl eigener Filialen an diesem Standort
  int get ownedCount => shops
      .where((s) =>
          s.cityId == city.id && s.locationName == location.template.name)
      .length;

  /// Standort ist verfügbar (keine eigene Filiale)
  bool get isAvailable => !hasOwnedShop;

  /// Standort ist gesperrt (durch Tutorial oder andere Mechanik)
  // TODO: Implementiere echte Sperr-Logik falls benötigt
  bool get isLocked => false;

  @override
  Widget build(BuildContext context) {
    // Metriken berechnen
    final traffic = location.footTrafficFor(city);
    final rent = location.weeklyRentFor(city);
    final deposit = location.depositFor(city);
    final score = location.attractivenessScore(city).round();

    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        curve: Curves.easeOut,
        decoration: BoxDecoration(
          color: const Color(0xFF3D2E22),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: isSelected
                ? AppColors.primary
                : const Color(0xFF3A2C20),
            width: isSelected ? 2 : 1,
          ),
          boxShadow: isSelected
              ? [
                  BoxShadow(
                    color: AppColors.primary.withAlpha(60),
                    blurRadius: 16,
                    spreadRadius: 0,
                  ),
                ]
              : null,
        ),
        child: Container(
          decoration: BoxDecoration(
            border: Border(
              left: BorderSide(
                color: AppColors.primary,
                width: 3,
              ),
            ),
          ),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Header: Name + Traffic
                Row(
                  children: [
                    Expanded(
                      child: Text(
                        location.template.name,
                        style: AppText.display(
                          size: 16,
                          weight: FontWeight.w800,
                          color: AppColors.primary,
                        ),
                      ),
                    ),
                    if (hasOwnedShop)
                      Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 8,
                          vertical: 2,
                        ),
                        decoration: BoxDecoration(
                          color: AppColors.success.withAlpha(40),
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: Text(
                          'EIGEN',
                          style: TextStyle(
                            color: AppColors.success,
                            fontSize: 10,
                            fontWeight: FontWeight.w800,
                          ),
                        ),
                      )
                    else
                      Row(
                        children: [
                          Text(
                            '★',
                            style: TextStyle(
                              color: AppColors.gold,
                              fontSize: 14,
                            ),
                          ),
                          const SizedBox(width: 4),
                          Text(
                            '$score',
                            style: const TextStyle(
                              color: Color(0xFFFFFAE6),
                              fontSize: 14,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ],
                      ),
                  ],
                ),
                const SizedBox(height: 12),

                // Info Zeilen: Zweispaltig
                Row(
                  children: [
                    Expanded(
                      child: _InfoRow(
                        label: 'Miete',
                        value: '€${rent.toStringAsFixed(0)}/Wo',
                      ),
                    ),
                    Expanded(
                      child: _InfoRow(
                        label: 'Konkurrenz',
                        value: location.risk,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                Row(
                  children: [
                    Expanded(
                      child: _InfoRow(
                        label: 'Kaution',
                        value: '€${deposit.toStringAsFixed(0)}',
                      ),
                    ),
                    Expanded(
                      child: _InfoRow(
                        label: 'Nachfrage',
                        value: _trafficLabel(traffic),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),

                // Icon + Standort-Typ
                Row(
                  children: [
                    Text(
                      location.icon,
                      style: const TextStyle(fontSize: 24),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        location.template.name,
                        style: const TextStyle(
                          color: Color(0xFFC4B5A0),
                          fontSize: 12,
                        ),
                      ),
                    ),
                    if (hasOwnedShop)
                      Text(
                        '×$ownedCount',
                        style: AppText.display(
                          size: 18,
                          color: AppColors.primary,
                        ),
                      ),
                  ],
                ),
                const SizedBox(height: 12),

                // CTA Button
                _buildCTA(),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildCTA() {
    if (isLocked) {
      return Container(
        width: double.infinity,
        padding: const EdgeInsets.symmetric(vertical: 14),
        decoration: BoxDecoration(
          color: AppColors.textMuted.withAlpha(40),
          borderRadius: BorderRadius.circular(12),
        ),
        child: const Center(
          child: Text(
            'GESPERRT',
            style: TextStyle(
              color: AppColors.textMuted,
              fontSize: 14,
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
      );
    }

    if (hasOwnedShop) {
      return Container(
        width: double.infinity,
        padding: const EdgeInsets.symmetric(vertical: 14),
        decoration: BoxDecoration(
          color: Colors.transparent,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: AppColors.primary, width: 2),
        ),
        child: Center(
          child: Text(
            'VERWALTEN',
            style: TextStyle(
              color: AppColors.primary,
              fontSize: 14,
              fontWeight: FontWeight.w600,
            ),
          ),
        ),
      );
    }

    // Freier Standort
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(vertical: 14),
      decoration: BoxDecoration(
        color: AppColors.primary,
        borderRadius: BorderRadius.circular(12),
      ),
      child: const Center(
        child: Text(
          'FILIALE ERÖFFNEN',
          style: TextStyle(
            color: Colors.white,
            fontSize: 14,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),
    );
  }

  String _trafficLabel(int traffic) {
    if (traffic > city.footTrafficBase * 1.5) return 'Sehr hoch';
    if (traffic > city.footTrafficBase * 1.1) return 'Hoch';
    if (traffic > city.footTrafficBase * 0.8) return 'Mittel';
    return 'Niedrig';
  }
}

/// Info-Zeile mit Label und Value
class _InfoRow extends StatelessWidget {
  final String label;
  final String value;

  const _InfoRow({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Text(
          '$label: ',
          style: const TextStyle(
            color: Color(0xFFC4B5A0),
            fontSize: 13,
          ),
        ),
        Text(
          value,
          style: const TextStyle(
            color: Color(0xFFFFFAE6),
            fontSize: 13,
            fontWeight: FontWeight.w600,
          ),
        ),
      ],
    );
  }
}
