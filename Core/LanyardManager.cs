namespace SpearLanyard;

using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using UnityEngine;

using static PluginConfig;

public static class LanyardManager {
  static readonly AccessTools.FieldRef<Projectile, Character> _projectileOwner =
      AccessTools.FieldRefAccess<Projectile, Character>("m_owner");

  static readonly Dictionary<ItemDrop, float> _reservedDrops = [];

  // Replaces the ItemDrop.DropItem call inside Projectile.SpawnOnHit (see ProjectilePatch), so the only drops
  // that ever pass through here are those spawned by a projectile hitting something.
  public static ItemDrop DropSpearItem(
      ItemDrop.ItemData item, int amount, Vector3 position, Quaternion rotation, Projectile projectile) {
    ItemDrop itemDrop = ItemDrop.DropItem(item, amount, position, rotation);

    if (IsModEnabled.Value && IsLanyardActive() && IsLocalSpearThrow(item, projectile)) {
      TrackDrop(itemDrop);
    }

    return itemDrop;
  }

  public static bool ShouldDenyOwnership(ItemDrop itemDrop, long requesterUid) {
    if (_reservedDrops.Count == 0 || !IsLanyardActive() || requesterUid == ZDOMan.GetSessionID()) {
      return false;
    }

    if (!_reservedDrops.TryGetValue(itemDrop, out float throwTime)) {
      return false;
    }

    if (IsReservationExpired(itemDrop, throwTime)) {
      _reservedDrops.Remove(itemDrop);
      SpearLanyard.LogInfo($"Released thrown spear: {itemDrop.m_itemData.m_shared.m_name}");

      return false;
    }

    return true;
  }

  public static void ClearReservations() {
    _reservedDrops.Clear();
  }

  static bool IsLanyardActive() {
    return ReservationDistance.Value > 0 || ReservationSeconds.Value > 0;
  }

  static bool IsLocalSpearThrow(ItemDrop.ItemData item, Projectile projectile) {
    return Player.m_localPlayer
        && item != null
        && item.m_shared.m_skillType == Skills.SkillType.Spears
        && _projectileOwner(projectile) == Player.m_localPlayer;
  }

  static void TrackDrop(ItemDrop itemDrop) {
    if (!itemDrop) {
      return;
    }

    PruneDestroyedDrops();
    _reservedDrops[itemDrop] = Time.time;

    SpearLanyard.LogInfo($"Reserved thrown spear: {itemDrop.m_itemData.m_shared.m_name}");
  }

  static bool IsReservationExpired(ItemDrop itemDrop, float throwTime) {
    if (!Player.m_localPlayer) {
      return true;
    }

    if (ReservationSeconds.Value > 0 && Time.time - throwTime > ReservationSeconds.Value) {
      return true;
    }

    return ReservationDistance.Value > 0
        && Vector3.Distance(Player.m_localPlayer.transform.position, itemDrop.transform.position)
            > ReservationDistance.Value;
  }

  static void PruneDestroyedDrops() {
    foreach (ItemDrop destroyedDrop in _reservedDrops.Keys.Where((ItemDrop drop) => !drop).ToList()) {
      _reservedDrops.Remove(destroyedDrop);
    }
  }
}
