namespace SpearLanyard;

using HarmonyLib;

using static PluginConfig;

[HarmonyPatch(typeof(ItemDrop))]
static class ItemDropPatch {
  [HarmonyPrefix]
  [HarmonyPatch(nameof(ItemDrop.RPC_RequestOwn))]
  static bool RPC_RequestOwnPrefix(ItemDrop __instance, long uid) {
    if (!IsModEnabled.Value) {
      return true;
    }

    return !LanyardManager.ShouldDenyOwnership(__instance, uid);
  }
}
