# Unity Save Compatibility

Unity `SaveService` serialisiert den aktuell portierten MVP-`GameState` als
UnityEngine-freien JSON-String. Die JSON-Feldnamen bleiben lower camelCase wie
im Flutter/Dart-Modell; Enums werden als Dart-`enum.name`-Strings geschrieben.

## Mapping

- `GameState`
  - `companyName`, `founderName`, `cash`, `currentDay`, `currentHour`
  - `shops`, `unlockedCityIds`, `competitors`, `loans`
  - `totalRevenue`, `totalProfit`, `customersServedTotal`
  - `difficulty` via `EnumNames.ToDart(GameDifficulty)`
  - `brand`, `achievementIds`, `employeePool`, `lastEmployeePoolDay`
  - `managerEmployeeIds`, `globalUpgradeIds`, `activeThemeId`, `prestigePoints`
  - `tutorialDone`, `tutorialEnabled`, `tutorialStep`, `seenEventIds`
- `Shop`
  - `id`, `name`, `customName`, `cityId`, `locationName`, `footTraffic`
  - `weeklyRent`, `isOpen`, `menu`, `equipment`, `employees`
  - `reputation`, `dayOpened`, `activeCampaigns`
  - `personality` via `EnumNames.ToDart(LocationPersonality)`
  - `upgradeIds`, `autoHire`, `originalCompetitorName`, `wasAcquired`
  - `morale`, `regulars`, `sizeTier` via `EnumNames.ToDart(ShopSizeTier)`
- `ShopProduct`
  - `productId`, `price`, `isActive`
- `ShopEquipment`
  - `equipmentId`
- `Employee`
  - `id`, `typeId`, `name`, `speed`, `friendliness`, `reliability`
  - `experience`, `salaryPerDay`, `daysEmployed`, `growthPotential`
  - `origin`, `shift`, `traits` via `EmployeeEnumNames`
- `Competitor`
  - `id`, `name`, `cityId`, `personality`, `shopCount`, `reputation`
  - `priceLevel`, `marketShare`, `daysSinceLastAction`
  - `personality` via `EnumNames.ToDart(CompetitorPersonality)`
- `Loan`
  - `id`, `amount`, `interestRate`, `durationDays`, `dayTaken`, `amountPaid`
- Marketing/BrandStats
  - `BrandStats`: `brandAwareness`, `cityReputation`
  - Active shop campaigns: `campaignId`, `startDay`, `endDay`

## Boundaries

`SaveService` reads and writes strings only. It does not use `UnityEngine`,
`PlayerPrefs`, filesystem persistence, UI state, buy flows, day simulation, or
cash/shop mutation logic.
