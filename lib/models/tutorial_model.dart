enum TutorialStep {
  openFirstShop,
  changeProductPrice,
  hireFirstEmployee,
  endFirstDay,
  readDayReport,
  viewCityMapMetrics,
  finishTutorial,
}

const int kTutorialStepCount = 7;

TutorialStep tutorialStepFromIndex(int index) {
  if (index <= 0) return TutorialStep.openFirstShop;
  if (index >= TutorialStep.values.length) return TutorialStep.finishTutorial;
  return TutorialStep.values[index];
}

extension TutorialStepMeta on TutorialStep {
  String get title {
    return switch (this) {
      TutorialStep.openFirstShop => 'Erste Filiale eröffnen',
      TutorialStep.changeProductPrice => 'Produktpreis anpassen',
      TutorialStep.hireFirstEmployee => 'Mitarbeiter einstellen',
      TutorialStep.endFirstDay => 'Ersten Tag abschließen',
      TutorialStep.readDayReport => 'Tagesbericht lesen',
      TutorialStep.viewCityMapMetrics => 'Stadtkarten-Kennzahlen',
      TutorialStep.finishTutorial => 'Tutorial abschließen',
    };
  }

  String get description {
    return switch (this) {
      TutorialStep.openFirstShop =>
        'Wähle auf der Stadtkarte einen Standort und eröffne deine erste Filiale.',
      TutorialStep.changeProductPrice =>
        'Öffne eine Filiale und passe die Preise an.',
      TutorialStep.hireFirstEmployee =>
        'Stelle in der Filiale einen Mitarbeiter ein.',
      TutorialStep.endFirstDay =>
        'Beende den Tag über den goldenen Button.',
      TutorialStep.readDayReport =>
        'Lies den Tagesbericht und bestätige ihn.',
      TutorialStep.viewCityMapMetrics =>
        'Sieh dir auf der Karte Umsatz und Filialen an.',
      TutorialStep.finishTutorial =>
        'Tutorial abschließen und frei spielen.',
    };
  }

  String get hint {
    return switch (this) {
      TutorialStep.openFirstShop => 'Tipp: Wähle einen Standort auf der Karte',
      TutorialStep.changeProductPrice => 'Preise veränderst du in den Filialdetails.',
      TutorialStep.hireFirstEmployee => 'Mitarbeiter findest du in der Filiale.',
      TutorialStep.endFirstDay => 'Nutze den goldenen Button im Dashboard.',
      TutorialStep.readDayReport => 'Schließe den Tagesbericht nach dem Lesen.',
      TutorialStep.viewCityMapMetrics => 'Achte auf Umsatz und Filialen.',
      TutorialStep.finishTutorial =>
        'Du bist bereit für den Ausbau deines Imperiums.',
    };
  }

  String? get actionLabel {
    return switch (this) {
      TutorialStep.changeProductPrice => 'Preis geändert',
      TutorialStep.viewCityMapMetrics => 'Verstanden',
      TutorialStep.finishTutorial => 'Tutorial beenden',
      _ => null,
    };
  }

  String get whyItMatters {
    return switch (this) {
      TutorialStep.openFirstShop =>
        'Ohne Filiale entstehen keine Einnahmen. Das ist dein Startpunkt für alles Weitere.',
      TutorialStep.changeProductPrice =>
        'Preisentscheidungen beeinflussen Nachfrage und Marge direkt. Das ist einer der wichtigsten Hebel.',
      TutorialStep.hireFirstEmployee =>
        'Personal sorgt für Kapazität und Servicequalität. Ohne Team wächst die Filiale nicht stabil.',
      TutorialStep.endFirstDay =>
        'Der Tagesabschluss zeigt dir, ob dein Setup wirtschaftlich funktioniert.',
      TutorialStep.readDayReport =>
        'Im Bericht erkennst du früh, ob Preise, Personal oder Kosten angepasst werden müssen.',
      TutorialStep.viewCityMapMetrics =>
        'Die Stadtkarte zeigt dir deinen Gesamtfortschritt mit Umsatz und Filialen.',
      TutorialStep.finishTutorial =>
        'Mit dem Abschluss kennst du die Kernmechaniken und kannst eigenständig optimieren.',
    };
  }

  int? get targetTabIndex {
    return switch (this) {
      TutorialStep.openFirstShop => 0,
      TutorialStep.changeProductPrice => 0,
      TutorialStep.hireFirstEmployee => 0,
      TutorialStep.endFirstDay => 0,
      TutorialStep.readDayReport => 0,
      TutorialStep.viewCityMapMetrics => 0,
      TutorialStep.finishTutorial => null,
    };
  }
}
