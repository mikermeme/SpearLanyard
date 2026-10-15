namespace SpearLanyard;

using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

[HarmonyPatch(typeof(Projectile))]
static class ProjectilePatch {
  // Inside Projectile.SpawnOnHit, redirect the one `ItemDrop.DropItem(m_spawnItem, 1, pos, rot)` call to
  // LanyardManager.DropSpearItem(m_spawnItem, 1, pos, rot, this). Nothing else about dropping items is touched.
  [HarmonyTranspiler]
  [HarmonyPatch(nameof(Projectile.SpawnOnHit))]
  static IEnumerable<CodeInstruction> SpawnOnHitTranspiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
        .MatchStartForward(
            new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ItemDrop), nameof(ItemDrop.DropItem))))
        .ThrowIfInvalid("Could not find the ItemDrop.DropItem call in Projectile.SpawnOnHit.")
        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
        .SetOperandAndAdvance(AccessTools.Method(typeof(LanyardManager), nameof(LanyardManager.DropSpearItem)))
        .InstructionEnumeration();
  }
}
