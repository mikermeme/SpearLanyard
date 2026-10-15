## Changelog

### 0.1.1

  * Fixed FieldAccessException at runtime: the game's real assemblies are not publicized, so private fields
    (`ItemDrop.m_nview`, `Projectile.m_owner`) can't be touched directly. The mod no longer reads them
    directly (the projectile owner is read through Harmony's `AccessTools.FieldRefAccess`).
  * Removed the global `ItemDrop.DropItem` patch. Inventory drops are no longer touched at all: the mod now
    only redirects the single `ItemDrop.DropItem` call inside `Projectile.SpawnOnHit` (a transpiler).

### 0.1.0

  * Initial draft.
