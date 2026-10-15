# SpearLanyard

*Client-side, vanilla-compatible: spears you throw stay reserved for you.*

Normally anyone nearby with autopickup on can grab a spear you threw. With SpearLanyard installed,
spears you throw can only be picked up by you, until they are far enough away or enough time has passed.
Nobody else needs the mod.

## How it works

Autopickup is decided by each player's own client, so a flag on the spear can't stop other players. What
the thrower's client does control is ownership of the dropped item: a player can only pick up an item
they own, and they get ownership by sending `RPC_RequestOwn` to the current owner, who normally always
agrees. The thrower owns a thrown spear from the moment it lands, so SpearLanyard simply ignores
`RPC_RequestOwn` from other players while the spear is reserved.

 * A transpiler on `Projectile.SpawnOnHit` redirects the single `ItemDrop.DropItem` call there (the one that
   respawns a thrown weapon where it lands) to `LanyardManager.DropSpearItem`, which makes the same drop and
   then records it as reserved if it is a spear thrown by you. Nothing else about dropping items is patched:
   inventory drops, other players' projectiles and every other item drop behave exactly as in vanilla.
 * An `ItemDrop.RPC_RequestOwn` prefix refuses other players' requests while the reservation holds, and lets
   the first request through once it has expired (checked lazily, so there is no `Update` loop).

## Config

 * `isModEnabled` — global toggle.
 * `reservationDistance` (0-100, meters) — reservation ends when the spear is farther than this from you.
 * `reservationSeconds` (0-60) — reservation ends this many seconds after the throw.

A value of 0 turns that limit off. If both are 0, the mod does nothing. If both are set, the reservation
ends as soon as either limit is passed.

## Known behavior

 * Manual pickup (`E`) by other players is blocked too; it uses the same request as autopickup.
 * A blocked player's client keeps retrying: autopickup backs off up to 30 seconds between retries, so
   right after a reservation expires another player may wait up to 30 seconds for autopickup. A blocked
   manual pickup keeps retrying every 0.05 seconds and can complete later, once it is allowed.
 * If you log out or leave the area, vanilla releases ownership of the spear and it becomes a normal item.
 * Anything that reassigns ZDO ownership on its own (a server-side mod, say) bypasses the reservation.

## Test checklist

 1. Two clients A and B near each other, A has SpearLanyard, B autopickup on.
 2. A throws a spear (distance 30, seconds 0). It should land normally. B walks over it: B should not pick
    it up, A should.
 3. A throws again and walks past 30m (or set seconds to 10 and wait): B should then be able to pick it up.
 4. A drops a spear and other items from the inventory: no errors, normal vanilla behavior, B can pick them up.
 5. Set both sliders to 0: B can pick up a thrown spear immediately.
 6. Check the BepInEx log for the "Reserved thrown spear" / "Released thrown spear" lines, and for the
    absence of FieldAccessException.
