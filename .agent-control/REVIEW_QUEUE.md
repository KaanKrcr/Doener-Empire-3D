# REVIEW_QUEUE

Status: open

## Open Review Items

### Unity RestaurantDetail price controller flow review

Reviewer: Claude Code
Target commit: `f51589c Add restaurant detail price controller flow`

Review scope:
- Verify that Sortiment price changes are only possible through
  `GameController.SetProductPrice(shopId, productId, price)`.
- Verify that `RestaurantDetailView` only triggers the controller intent and
  does not directly mutate `GameState`, `Shop`, `ShopProduct`, cash, save state,
  files, or hotspot ownership.
- Verify that `ProductPricingService` stays UnityEngine-free, validates shop id,
  product id, and the MVP price range 1.00 to 25.00 EUR, and leaves state
  unchanged on invalid input.
- Verify that a successful price change publishes a state snapshot, refreshes
  the restaurant detail, and emits an appropriate toast without introducing
  extra management actions.

Acceptance criteria:
- Focused tests for valid price update and invalid shop/product/price cases are
  present and pass.
- `dotnet test unity-logic-tests\DoenerEmpire.Logic.Tests\DoenerEmpire.Logic.Tests.csproj`
  is green.
- Scope scan finds no new Upgrade, Equipment, Personal, Marketing, Cash, Save,
  PlayerPrefs, filesystem, Arcade Cooking, realtime serving, CustomerSpawner,
  manual cooking, first-person, or third-person implementation.
- Management/progression direction from `CURRENT_DECISION.md`,
  `UNITY_PRODUCT_VISION.md`, and `UNITY_CITY_MAP_UX.md` remains intact.

## Rules
- Claude Code writes concrete review items here and sets `Status: open`.
- Codex implements only open review items, then sets `Status: empty`.
- If no review items exist, keep `Status: empty`.
