// Döner Empire 3D — Corporate: Manager-Zuweisung
// Port aus lib/services/corporate_engine.dart (assignManager, unassignManager,
// _shopHasActiveManager).
//
// Auto-Pricing (applyManagerAutoPricing) und Auto-Hire (applyAutoHire) sind
// als TODO offen — sie hängen an mehreren RNG-Helfern und dem Kandidatenpool.

using System.Linq;
using DoenerEmpire.Models;

namespace DoenerEmpire.Simulation
{
    public static class ManagerService
    {
        /// <summary>Macht einen Mitarbeiter zum Manager (Auto-Pricing seiner Filiale).</summary>
        public static GameState AssignManager(GameState state, string employeeId)
        {
            if (!state.ManagerEmployeeIds.Contains(employeeId))
                state.ManagerEmployeeIds.Add(employeeId);
            return state;
        }

        /// <summary>Entfernt den Manager-Status eines Mitarbeiters.</summary>
        public static GameState UnassignManager(GameState state, string employeeId)
        {
            state.ManagerEmployeeIds.RemoveAll(id => id == employeeId);
            return state;
        }

        /// <summary>Hat die Filiale einen Mitarbeiter mit Manager-Status?</summary>
        public static bool ShopHasActiveManager(Shop shop, GameState state)
            => shop.Employees.Any(e => state.ManagerEmployeeIds.Contains(e.Id));
    }
}
