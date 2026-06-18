import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../models/city_map_model.dart';
import '../../models/city_model.dart';
import '../../models/competitor_model.dart';
import '../../models/shop_model.dart';
import '../../services/corporate_engine.dart';
import '../../services/location_engine.dart';
import 'building_styles.dart';

final _fmt = NumberFormat('#,##0', 'de_DE');

/// Detail-Panel im 2.5D-Straßenzug. Zeigt je nach angeklicktem Gebäude:
/// eigene Filiale, Konkurrent (mit Übernahme), freier Bauplatz oder
/// Nachbargebäude.
class DetailPanelStreet extends StatelessWidget {
  final CityData city;
  final double cash;
  final Shop? playerShop;
  final Competitor? competitor;
  final CityMapLocation? freeLocation;
  final bool isFiller;
  final VoidCallback onClose;
  final void Function(Shop) onManage;
  final void Function(Shop) onCustomize;
  final void Function(CityMapLocation) onOpenFree;
  final void Function(Competitor) onAcquire;

  const DetailPanelStreet({
    super.key,
    required this.city,
    required this.cash,
    required this.onClose,
    required this.onManage,
    required this.onCustomize,
    required this.onOpenFree,
    required this.onAcquire,
    this.playerShop,
    this.competitor,
    this.freeLocation,
    this.isFiller = false,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: const BoxDecoration(
        color: MapPalette.bgPanel,
        borderRadius: BorderRadius.vertical(top: Radius.circular(18)),
        border: Border(top: BorderSide(color: MapPalette.border)),
      ),
      child: SafeArea(
        top: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 12, 16, 16),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [_body()],
          ),
        ),
      ),
    );
  }

  Widget _body() {
    if (playerShop != null) return _PlayerBody(this);
    if (competitor != null) return _CompetitorBody(this);
    if (freeLocation != null) return _FreeBody(this);
    return _FillerBody(onClose: onClose);
  }
}

class _Header extends StatelessWidget {
  final String emoji;
  final String title;
  final String subtitle;
  final Color titleColor;
  final VoidCallback onClose;

  const _Header({
    required this.emoji,
    required this.title,
    required this.subtitle,
    required this.titleColor,
    required this.onClose,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Text(emoji, style: const TextStyle(fontSize: 22)),
        const SizedBox(width: 10),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: TextStyle(
                  fontFamily: 'Baloo2',
                  fontSize: 18,
                  fontWeight: FontWeight.w700,
                  color: titleColor,
                ),
              ),
              Text(
                subtitle,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                    fontSize: 12, color: MapPalette.textMuted),
              ),
            ],
          ),
        ),
        IconButton(
          onPressed: onClose,
          icon: const Icon(Icons.close, color: MapPalette.textDim, size: 20),
          padding: EdgeInsets.zero,
          constraints: const BoxConstraints(),
        ),
      ],
    );
  }
}

class _PlayerBody extends StatelessWidget {
  final DetailPanelStreet p;
  const _PlayerBody(this.p);

  @override
  Widget build(BuildContext context) {
    final shop = p.playerShop!;
    final stars = (shop.reputation * 2).round();
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _Header(
          emoji: '🥙',
          title: shop.displayName,
          subtitle: shop.locationName,
          titleColor: Color(shop.accentColor),
          onClose: p.onClose,
        ),
        const SizedBox(height: 12),
        Row(
          children: [
            _StatCell(
                label: 'Miete/Tag',
                value: '${_fmt.format(shop.dailyRent.round())} €'),
            _StatCell(
              label: 'Ruf',
              value: '★ ${shop.reputation.toStringAsFixed(1)} '
                  '(${stars ~/ 2}${stars.isOdd ? '½' : ''})',
            ),
            _StatCell(label: 'Personal', value: '${shop.employees.length}'),
          ],
        ),
        const SizedBox(height: 14),
        Row(
          children: [
            Expanded(
              child: OutlinedButton.icon(
                onPressed: () => p.onCustomize(shop),
                icon: const Icon(Icons.palette_outlined, size: 16),
                label: const Text('Farbe & Stil'),
                style: OutlinedButton.styleFrom(
                  foregroundColor: Color(shop.accentColor),
                  side: BorderSide(color: Color(shop.accentColor)),
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(10)),
                ),
              ),
            ),
            const SizedBox(width: 10),
            Expanded(
              child: ElevatedButton.icon(
                onPressed: () => p.onManage(shop),
                icon: const Icon(Icons.tune_rounded, size: 16),
                label: const Text('Verwalten'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: MapPalette.accent,
                  foregroundColor: Colors.black87,
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(10)),
                ),
              ),
            ),
          ],
        ),
      ],
    );
  }
}

class _CompetitorBody extends StatelessWidget {
  final DetailPanelStreet p;
  const _CompetitorBody(this.p);

  @override
  Widget build(BuildContext context) {
    final c = p.competitor!;
    final price = CorporateEngine.acquisitionPrice(c);
    final canAfford = p.cash >= price;
    final stars = (c.reputation * 2).round();
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _Header(
          emoji: c.personality.emoji,
          title: c.name,
          subtitle: c.personality.tagline,
          titleColor: MapPalette.danger,
          onClose: p.onClose,
        ),
        const SizedBox(height: 12),
        Row(
          children: [
            _StatCell(
              label: 'Ruf',
              value: '★ ${c.reputation.toStringAsFixed(1)} '
                  '(${stars ~/ 2}${stars.isOdd ? '½' : ''})',
            ),
            _StatCell(label: 'Filialen', value: '${c.shopCount}'),
            _StatCell(
                label: 'Marktanteil',
                value: '${(c.marketShare * 100).round()} %'),
          ],
        ),
        const SizedBox(height: 14),
        SizedBox(
          width: double.infinity,
          child: ElevatedButton.icon(
            onPressed: canAfford ? () => p.onAcquire(c) : null,
            icon: const Icon(Icons.handshake_outlined, size: 16),
            label: Text(
              canAfford
                  ? 'Übernehmen · ${_fmt.format(price.round())} €'
                  : 'Zu wenig Kapital · ${_fmt.format(price.round())} €',
            ),
            style: ElevatedButton.styleFrom(
              backgroundColor: MapPalette.danger,
              disabledBackgroundColor: MapPalette.bgCard,
              foregroundColor: Colors.white,
              disabledForegroundColor: MapPalette.textDim,
              padding: const EdgeInsets.symmetric(vertical: 12),
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10)),
            ),
          ),
        ),
      ],
    );
  }
}

class _FreeBody extends StatelessWidget {
  final DetailPanelStreet p;
  const _FreeBody(this.p);

  @override
  Widget build(BuildContext context) {
    final loc = p.freeLocation!;
    final deposit = loc.depositFor(p.city);
    final canAfford = p.cash >= deposit;
    final forecast = LocationEngine.forecastOpening(p.city, loc);
    final breakEven = forecast.breakEvenDays == null
        ? 'kritisch'
        : '${forecast.breakEvenDays} Tage';
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _Header(
          emoji: loc.icon,
          title: loc.label,
          subtitle: loc.audience,
          titleColor: MapPalette.success,
          onClose: p.onClose,
        ),
        const SizedBox(height: 12),
        Row(
          children: [
            _StatCell(
                label: 'Miete/Woche',
                value: '${_fmt.format(loc.weeklyRentFor(p.city).round())} €'),
            _StatCell(
                label: 'Kunden/Tag',
                value: '${forecast.estimatedCustomersPerDay}'),
            _StatCell(label: 'Break-even', value: breakEven),
          ],
        ),
        const SizedBox(height: 14),
        SizedBox(
          width: double.infinity,
          child: ElevatedButton.icon(
            onPressed: canAfford ? () => p.onOpenFree(loc) : null,
            icon: const Icon(Icons.storefront_rounded, size: 16),
            label: Text(
              canAfford
                  ? 'Filiale eröffnen · ${_fmt.format(deposit.round())} €'
                  : 'Zu wenig Kapital · ${_fmt.format(deposit.round())} €',
            ),
            style: ElevatedButton.styleFrom(
              backgroundColor: MapPalette.accent,
              disabledBackgroundColor: MapPalette.bgCard,
              foregroundColor: Colors.black87,
              disabledForegroundColor: MapPalette.textDim,
              padding: const EdgeInsets.symmetric(vertical: 12),
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10)),
            ),
          ),
        ),
      ],
    );
  }
}

class _FillerBody extends StatelessWidget {
  final VoidCallback onClose;
  const _FillerBody({required this.onClose});

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _Header(
          emoji: '🏚️',
          title: 'Nachbargebäude',
          subtitle: 'Kein Imbiss — gehört nicht zum Markt.',
          titleColor: MapPalette.textMuted,
          onClose: onClose,
        ),
      ],
    );
  }
}

class _StatCell extends StatelessWidget {
  final String label;
  final String value;
  const _StatCell({required this.label, required this.value});

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Container(
        margin: const EdgeInsets.only(right: 6),
        padding: const EdgeInsets.all(8),
        decoration: BoxDecoration(
          color: MapPalette.bgCard,
          borderRadius: BorderRadius.circular(10),
          border: Border.all(color: MapPalette.border),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              value,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: const TextStyle(
                color: MapPalette.textMain,
                fontWeight: FontWeight.w700,
                fontSize: 12,
              ),
            ),
            const SizedBox(height: 1),
            Text(label,
                style: const TextStyle(
                    color: MapPalette.textDim, fontSize: 9)),
          ],
        ),
      ),
    );
  }
}
