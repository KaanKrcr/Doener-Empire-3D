# Unity Save Compatibility

This document pins the current MVP save shape for the Unity C# port. Field names
use Dart-style camelCase so the save path can stay compatible with the Flutter
model shape while the Unity implementation grows.

## GameState

- `companyName`, `founderName`, `cash`, `currentDay`, `currentHour`
- `shops`, `unlockedCityIds`, `competitors`, `loans`
- `totalRevenue`, `totalProfit`, `customersServedTotal`
- `difficulty` as Dart enum string: `easy`, `normal`, `hard`, `impossible`
- `brand`, `achievementIds`, `employeePool`, `lastEmployeePoolDay`
- `managerEmployeeIds`, `globalUpgradeIds`, `activeThemeId`, `prestigePoints`
- `tutorialDone`, `tutorialEnabled`, `tutorialStep`, `seenEventIds`

## Shop

- Identity/location: `id`, `name`, `customName`, `cityId`, `locationName`
- Metrics/state: `footTraffic`, `weeklyRent`, `isOpen`, `reputation`,
  `dayOpened`, `morale`, `regulars`
- Collections: `menu`, `equipment`, `employees`, `activeCampaigns`,
  `upgradeIds`
- Enums as Dart strings:
  - `personality`: `touristic`, `business`, `transit`, `residential`,
    `university`, `nightlife`
  - `sizeTier`: `klein`, `mittel`, `gross`, `flagship`
- Acquisition/admin: `autoHire`, `originalCompetitorName`, `wasAcquired`

## Shop Submodels

- `ShopProduct`: `productId`, `price`, `isActive`
- `ShopEquipment`: `equipmentId`
- `ActiveCampaign`: `campaignId`, `startDay`, `endDay`

## Employee

- `id`, `typeId`, `name`
- `speed`, `friendliness`, `reliability`, `experience`, `salaryPerDay`
- `traits` as Dart strings such as `charmer`, `workaholic`, `loyal`
- `daysEmployed`, `growthPotential`
- `origin` as Dart string, including multi-word values such as `hiddenGem`,
  `topTalent`, `juniorPotential`, `exCompetitor`, `teamContact`
- `shift` as `ganztags`, `frueh`, `mittag`, `abend`

## Competitor, Loan, Brand

- `Competitor`: `id`, `name`, `cityId`, `personality`, `shopCount`,
  `reputation`, `priceLevel`, `marketShare`, `daysSinceLastAction`
- `personality` uses Dart strings including `cheapMass`, `balanced`,
  `premium`, `aggressive`, `traditional`
- `Loan`: `id`, `amount`, `interestRate`, `durationDays`, `dayTaken`,
  `amountPaid`
- `BrandStats`: `brandAwareness`, `cityReputation`

## Current Boundary

`SaveService` serializes/deserializes strings only. It does not touch
`UnityEngine`, `PlayerPrefs`, files, UI, Buy/Shop/Cash mutation, GameEngine,
Day-Sim, arcade-cooking, or realtime serving logic.
