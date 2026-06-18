import 'dart:math' as math;

import 'dart:ui' as ui;
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../core/constants.dart';
import '../../data/berlin_scene.dart';
import '../../models/city_model.dart';
import '../../providers/game_provider.dart';
import '../../services/haptics_service.dart';
import '../../services/sound_service.dart';
import '../screens/campaign_screen.dart' show CampaignChapterDialog;
import '../widgets/bankruptcy_dialog.dart';
import '../widgets/building_styles.dart';
import '../widgets/day_end_dialog.dart';
import '../widgets/iso_tilemap.dart';
import '../widgets/iso_tilemap_painter.dart';
import '../widgets/map_deutschland.dart';
import '../widgets/mission_banner.dart';
import '../widgets/quarterly_report_dialog.dart';
import '../widgets/weekly_report_dialog.dart';

final _fmt = NumberFormat('#,##0', 'de_DE');

/// ZusammenhÃ¤ngende, zoombare Iso-Stadt. Die Deutschlandkarte dient nur noch
/// als Auswahl-UI fÃ¼r einen Stadtwechsel.
class CityMapScreen extends ConsumerStatefulWidget {
  final String cityId;

  const CityMapScreen({super.key, required this.cityId});

  @override
  ConsumerState<CityMapScreen> createState() => _CityMapScreenState();
}

class _CityMapScreenState extends ConsumerState<CityMapScreen> {
  bool _endingDay = false;
  late String _cityId;

  @override
  void initState() {
    super.initState();
    _cityId = widget.cityId;
  }

  CityData get _city => kAllCities.firstWhere(
        (city) => city.id == _cityId,
        orElse: () => kAllCities.first,
      );

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
            content: Text(
              'Steuern (30 Tage): -${_fmt.format(result.taxPaid)} â‚¬',
            ),
            duration: const Duration(seconds: 3),
          ),
        );
      }
    }
    if (mounted) setState(() => _endingDay = false);
  }

  void _onSelectCity(String cityId) {
    Navigator.of(context).pop();
    setState(() => _cityId = cityId);
  }

  void _onTileTapped(TileType type, String? label, double? rating, bool isHero) {
    final game = ref.read(gameProvider);
    if (game == null) return;

    switch (type) {
      case TileType.hero:
        final shop = game.shops.firstWhere(
          (s) => s.cityId == _cityId,
          orElse: () => game.shops.first,
        );
        context.push('/shop/${shop.id}');

      case TileType.empty:
        _showLocationInfo(label);

      case TileType.competitor:
        _showCompetitorInfo(label, rating);

      case TileType.building:
      case TileType.grass:
      case TileType.road:
      case TileType.sidewalk:
      case TileType.water:
        // Keine Interaktion fÃ¼r FÃ¼ll-GebÃ¤ude und Boden
        break;
    }
  }

  void _showLocationInfo(String? label) {
    showModalBottomSheet(
      context: context,
      backgroundColor: MapPalette.bgPanel,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
        side: BorderSide(color: MapPalette.border),
      ),
      builder: (ctx) => Padding(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Center(
              child: Container(
                width: 38, height: 5,
                decoration: BoxDecoration(
                  color: MapPalette.border,
                  borderRadius: BorderRadius.circular(3),
                ),
              ),
            ),
            const SizedBox(height: 18),
            Row(
              children: [
                Container(
                  width: 44, height: 44,
                  decoration: BoxDecoration(
                    color: MapPalette.success.withAlpha(30),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(Icons.store_outlined,
                      color: MapPalette.success, size: 22),
                ),
                const SizedBox(width: 14),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        label ?? 'Freier Standort',
                        style: const TextStyle(
                          fontFamily: 'Baloo2',
                          fontSize: 18,
                          fontWeight: FontWeight.w700,
                          color: MapPalette.textMain,
                        ),
                      ),
                      const SizedBox(height: 2),
                      const Text(
                        'VerfÃ¼gbar Â· GewerbeflÃ¤che',
                        style: TextStyle(
                          fontSize: 12,
                          color: MapPalette.textMuted,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 20),
            Row(
              children: [
                const Icon(Icons.euro_outlined,
                    color: MapPalette.accent, size: 16),
                const SizedBox(width: 6),
                const Text('Kaution: ', style: TextStyle(
                    color: MapPalette.textMuted, fontSize: 13)),
                const Text('15.000 â‚¬', style: TextStyle(
                    fontFamily: 'Baloo2', fontSize: 15,
                    fontWeight: FontWeight.w700,
                    color: MapPalette.textMain)),
              ],
            ),
            const SizedBox(height: 20),
            SizedBox(
              width: double.infinity,
              height: 50,
              child: ElevatedButton.icon(
                onPressed: () {
                  Navigator.pop(ctx);
                  context.push('/open-shop/$_cityId');
                },
                icon: const Icon(Icons.add_business_outlined, size: 20),
                label: const Text('Filiale erÃ¶ffnen',
                    style: TextStyle(fontSize: 15, fontWeight: FontWeight.w700)),
                style: ElevatedButton.styleFrom(
                  backgroundColor: MapPalette.accent,
                  foregroundColor: Colors.white,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _showCompetitorInfo(String? label, double? rating) {
    showModalBottomSheet(
      context: context,
      backgroundColor: MapPalette.bgPanel,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
        side: BorderSide(color: MapPalette.border),
      ),
      builder: (ctx) => Padding(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Center(
              child: Container(
                width: 38, height: 5,
                decoration: BoxDecoration(
                  color: MapPalette.border,
                  borderRadius: BorderRadius.circular(3),
                ),
              ),
            ),
            const SizedBox(height: 18),
            Row(
              children: [
                Container(
                  width: 44, height: 44,
                  decoration: BoxDecoration(
                    color: MapPalette.danger.withAlpha(30),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(Icons.store_mall_directory_outlined,
                      color: MapPalette.danger, size: 22),
                ),
                const SizedBox(width: 14),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        label ?? 'Wettbewerber',
                        style: const TextStyle(
                          fontFamily: 'Baloo2',
                          fontSize: 18,
                          fontWeight: FontWeight.w700,
                          color: MapPalette.textMain,
                        ),
                      ),
                      if (rating != null) ...[
                        const SizedBox(height: 2),
                        Row(
                          children: [
                            Icon(Icons.star, color: MapPalette.gold, size: 14),
                            const SizedBox(width: 4),
                            Text(
                              rating.toStringAsFixed(1),
                              style: const TextStyle(
                                fontSize: 14,
                                fontWeight: FontWeight.w600,
                                color: MapPalette.textMuted,
                              ),
                            ),
                          ],
                        ),
                      ],
                    ],
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _showCityPicker() async {
    final game = ref.read(gameProvider);
    if (game == null) return;
    await showModalBottomSheet<void>(
      context: context,
      useSafeArea: true,
      isScrollControlled: true,
      backgroundColor: MapPalette.bgDeep,
      builder: (context) => FractionallySizedBox(
        heightFactor: 0.88,
        child: MapDeutschland(
          unlockedCityIds: game.unlockedCityIds.toSet(),
          cityIdsWithShops: game.shops.map((shop) => shop.cityId).toSet(),
          onSelectCity: _onSelectCity,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final game = ref.watch(gameProvider)!;

    ref.listen(gameProvider, (previous, next) {
      if (next == null) return;
      final wasSolvent = previous == null || previous.cash >= 0;
      if (wasSolvent && next.cash < 0 && mounted) {
        BankruptcyDialog.show(context);
      }
    });

    return Scaffold(
      backgroundColor: MapPalette.bgBase,
      body: Stack(
        children: [
          Column(
            children: [
              SafeArea(
                bottom: false,
                child: _Header(
                  title: '${_city.emoji} ${_city.name}',
                  subtitle: 'Iso-Stadt',
                  cash: game.cash,
                  onBack: context.pop,
                  onChangeCity: _showCityPicker,
                ),
              ),
              Expanded(
                child: _IsoCityMap(
                  key: ValueKey(_cityId),
                  data: buildBerlinScene(),
                  onTileTapped: (type, label, rating, isHero) =>
                      _onTileTapped(type, label, rating, isHero),
                ),
              ),
            ],
          ),
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
}

class _IsoCityMap extends StatefulWidget {
  final TilemapData data;

  /// Wird aufgerufen, wenn ein Tile ausgewÃ¤hlt wird. Liefert Tile-Typ,
  /// Label und ob es das Hero-Tile ist.
  final void Function(TileType type, String? label, double? rating, bool isHero)
      onTileTapped;

  const _IsoCityMap({
    super.key,
    required this.data,
    required this.onTileTapped,
  });

  @override
  State<_IsoCityMap> createState() => _IsoCityMapState();
}

class _IsoCityMapState extends State<_IsoCityMap> {
  Map<String, ui.Image> _sprites = {};
  bool _loading = true;
  final TransformationController _controller = TransformationController();
  math.Point<int>? _selectedTile;
  Size? _initializedViewport;

  @override
  void initState() {
    super.initState();
    _loadSprites();
  }

  Future<void> _loadSprites() async {
    final assetPaths = <String>{
      'assets/iso/building_owned.png',
      'assets/iso/building_empty.png',
      'assets/iso/building_competitor.png',
      'assets/iso/building_filler.png',
    };
    final loaded = <String, ui.Image>{};
    for (final path in assetPaths) {
      try {
        final bundle = DefaultAssetBundle.of(context);
        final data = await bundle.load(path);
        final codec = await ui.instantiateImageCodec(data.buffer.asUint8List());
        final frame = await codec.getNextFrame();
        loaded[path] = frame.image;
      } catch (_) {}
    }
    if (mounted) setState(() { _sprites = loaded; _loading = false; });
  }

  IsoGrid get _grid => const IsoGrid().centeredFor(
        widget.data.width,
        widget.data.height,
      );

  Size get _sceneSize =>
      _grid.sceneSize(widget.data.width, widget.data.height);

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _centerOnHero(Size viewport, {bool force = false}) {
    if (!force && _initializedViewport == viewport) return;
    _initializedViewport = viewport;
    final hero = _grid.tileToScreen(widget.data.heroTile);
    final scene = _sceneSize;
    final fitScale = math.min(
      viewport.width / scene.width,
      viewport.height / scene.height,
    );
    final scale = (fitScale * 1.6).clamp(0.55, 1.0);
    final tx = viewport.width / 2 - hero.dx * scale;
    final ty = viewport.height * 0.56 - hero.dy * scale;

    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      _controller.value = Matrix4.identity()
        ..translateByDouble(tx, ty, 0, 1)
        ..scaleByDouble(scale, scale, 1, 1);
    });
  }

  void _selectTile(TapUpDetails details) {
    final scenePoint = _controller.toScene(details.localPosition);
    final tile = _grid.screenToNearestTile(scenePoint);
    setState(() {
      _selectedTile = widget.data.contains(tile) ? tile : null;
    });
  }

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final viewport = Size(constraints.maxWidth, constraints.maxHeight);
        _centerOnHero(viewport);
        return Stack(
          children: [
            Positioned.fill(
              child: GestureDetector(
                behavior: HitTestBehavior.opaque,
                onTapUp: (details) {
                  _selectTile(details);
                  final tile = _selectedTile;
                  if (tile != null && widget.data.contains(tile)) {
                    final iso = widget.data.tileAt(tile.x, tile.y);
                    widget.onTileTapped(
                      iso.type,
                      iso.label,
                      iso.rating,
                      tile == widget.data.heroTile,
                    );
                  }
                },
                child: InteractiveViewer(
                  transformationController: _controller,
                  constrained: false,
                  minScale: 0.45,
                  maxScale: 2.2,
                  boundaryMargin: const EdgeInsets.all(260),
                  child: CustomPaint(
                    size: _sceneSize,
                    painter: IsoTilemapPainter(
                      data: widget.data,
                      grid: _grid,
                      selectedTile: _selectedTile,
                      sprites: _sprites,
                    ),
                  ),
                ),
              ),
            ),
            Positioned(
              right: 14,
              top: 14,
              child: Column(
                children: [
                  _MapControlButton(
                    icon: Icons.my_location_rounded,
                    tooltip: 'Hero zentrieren',
                    onPressed: () => _centerOnHero(viewport, force: true),
                  ),
                  const SizedBox(height: 8),
                  _MapControlButton(
                    icon: Icons.zoom_out_map_rounded,
                    tooltip: 'Karte einpassen',
                    onPressed: () => _centerOnHero(viewport, force: true),
                  ),
                ],
              ),
            ),
          ],
        );
      },
    );
  }
}

class _MapControlButton extends StatelessWidget {
  final IconData icon;
  final String tooltip;
  final VoidCallback onPressed;

  const _MapControlButton({
    required this.icon,
    required this.tooltip,
    required this.onPressed,
  });

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        color: MapPalette.bgPanel.withAlpha(230),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: MapPalette.border),
      ),
      child: IconButton(
        onPressed: onPressed,
        tooltip: tooltip,
        icon: Icon(icon, color: MapPalette.textMuted, size: 20),
      ),
    );
  }
}

class _Header extends StatelessWidget {
  final String title;
  final String? subtitle;
  final double cash;
  final VoidCallback onBack;
  final VoidCallback onChangeCity;

  const _Header({
    required this.title,
    required this.subtitle,
    required this.cash,
    required this.onBack,
    required this.onChangeCity,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 58,
      padding: const EdgeInsets.symmetric(horizontal: 8),
      decoration: const BoxDecoration(
        color: MapPalette.bgPanel,
        border: Border(bottom: BorderSide(color: MapPalette.border)),
      ),
      child: Row(
        children: [
          IconButton(
            onPressed: onBack,
            icon: const Icon(
              Icons.arrow_back_ios_new_rounded,
              color: MapPalette.textMuted,
              size: 18,
            ),
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
                      fontSize: 11,
                      color: MapPalette.textDim,
                    ),
                  ),
              ],
            ),
          ),
          const Icon(
            Icons.account_balance_wallet_outlined,
            color: MapPalette.accent,
            size: 16,
          ),
          const SizedBox(width: 4),
          Text(
            '${_fmt.format(cash.round())} â‚¬',
            style: const TextStyle(
              fontFamily: 'Baloo2',
              fontSize: 14,
              fontWeight: FontWeight.w700,
              color: MapPalette.accent,
            ),
          ),
          const SizedBox(width: 4),
          TextButton.icon(
            onPressed: onChangeCity,
            icon: const Icon(Icons.public_rounded, size: 15),
            label: const Text('Stadt wechseln'),
            style: TextButton.styleFrom(
              foregroundColor: MapPalette.textMuted,
              visualDensity: VisualDensity.compact,
              padding: const EdgeInsets.symmetric(horizontal: 7),
              textStyle: const TextStyle(
                fontSize: 10,
                fontWeight: FontWeight.w700,
              ),
            ),
          ),
        ],
      ),
    );
  }
}




