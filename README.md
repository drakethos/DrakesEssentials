# DrakesEssentials

The best pieces from the DrakeMods suite, on their own: no DrakeModsLibs, no shop menus, no key systems. Just the
pieces you'd want, in one small mod.

## Pieces

| Piece | Hammer tab | From | What it does |
|-------|-----------|------|--------------|
| **Display Case** / **Small Display Case** | Furniture | DrakesItemShop | Glass and wood cases (7 or 3 slots). What's inside lies on the jute, one spot per slot, flat and sized to fit. Fill it from the door side: the glass door slides open while you do. From the glass side it's just looked at. |
| **Armor racks** / **Weapon racks** / **Wall racks** | Furniture | DrakesItemShop | Finewood racks with iron hooks and pegs. Free-standing armor and weapon racks (6 or 4 slots) take only armor or only weapons and shields; wall racks (5 or 3) take both. What's inside hangs on the rack: weapons and shields like on an item stand, armor upright on its hook. |
| **Public doors and chests** | Public | LockSmith | An always-open copy of every buildable door and chest, vanilla and other mods (`*_public`). Anyone can open them, wards or not. |
| **Paper (wall)** / **Paper (flat)** | Furniture | DrakesRenameit | A blank sheet on a wall or lying flat. Write on it like a sign. |

## Config (server-synced)

`BepInEx/config/com.drakesworkshop.essentials.cfg`. With `LockConfiguration` on (default), the server's values apply
to everyone and only admins can change them.

| Section | Setting | Default | |
|---------|---------|---------|---|
| 2 - Display cases | `Enabled` | `true` | Show the cases in the hammer. |
| | `DisplayCaseCost` | `FineWood:10,JuteRed:2,Crystal:2` | `Item:amount` pairs, comma separated (prefab names). |
| | `SmallDisplayCaseCost` | `FineWood:5,JuteRed:1,Crystal:1` | |
| 3 - Public doors and chests | `Enabled` | `true` | Show the public copies in the hammer. |
| 4 - Paper | `Enabled` | `true` | Show the paper in the hammer. |
| | `PaperCost` | `Wood:1` | |
| 5 - Armor and weapon racks | `Enabled` | `true` | Show the racks in the hammer. |
| | `ArmorRackCost` / `SmallArmorRackCost` | `FineWood:12,BronzeNails:6` / `FineWood:8,BronzeNails:4` | |
| | `WallRackCost` / `SmallWallRackCost` | `FineWood:8,BronzeNails:5` / `FineWood:5,BronzeNails:3` | Wall racks (armor and weapons). |
| | `WeaponRackCost` / `SmallWeaponRackCost` | `FineWood:10,BronzeNails:6` / `FineWood:7,BronzeNails:4` | |

Switches apply live and only show or hide pieces in the hammer: anything already built stays, and public pieces stay
public.

## With the full mods

Each full mod only turns off its own part of Essentials; the rest keeps working. So you can run, say, DrakesRenameit
for its paper and still get the display cases and public doors from Essentials.

| Installed | Essentials does |
|-----------|-----------------|
| **DrakesItemShop** | Leaves the display cases and racks to ItemShop (same piece names, so placed ones keep working). |
| **LockSmith** | Leaves the public doors and chests to LockSmith, which then controls them (its own config and menus). Same `_public` piece names, so placed ones stay. |
| **DrakesRenameit** | Hides its paper from the hammer (RenameIt has its own). Sheets already placed stay and can still be written on. |

When a full mod takes over a piece, **its** config rules it (costs, on/off and the rest); Essentials' settings for that
piece are ignored.

## For developers

- Looks come from Drakes Asset Forge packs in `Packs/` (display case models, paper art); the code in `Generated/` is
  made from them, see `Packs/README.md`.
- Shared code is copied in, not referenced: `Forge/` from DrakeModsLibs, `DisplayCases/` from DrakesItemShop,
  `PublicPieces/` from LockSmith. Each file says where it came from.
- ServerSync is merged into the DLL with ILRepack (`ILRepack.targets`, `EmbedServerSync`).
- Build: `dotnet msbuild DrakesEssentials.csproj` (deploys to the Gale and r2modman `drakeTest` profiles).
- Release: Hexium only. Push a tag matching `<Version>` in `mod.package.props` (`v0.1.0`, or `v0.1.0-beta.1`);
  `.github/workflows/release.yml` builds, makes the GitHub release from that version's CHANGELOG section and publishes
  to https://valheim.hexium.gg/mods/DrakeMods/DrakesEssentials.
