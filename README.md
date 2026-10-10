# DrakesEssentials

The best pieces from the DrakeMods suite, on their own: no DrakeModsLibs, no shop menus, no key systems. Just the
pieces you'd want, in one small mod.

## Pieces

| Piece | Hammer tab | From | What it does |
|-------|-----------|------|--------------|
| **Display Case** / **Small Display Case** | Furniture | DrakesItemShop | Glass and wood cases (6 or 3 slots). What's inside lies on the jute, one spot per slot, flat and sized to fit. Fill it from the door side: the glass door slides open while you do. From the glass side it's just looked at. |
| **Public doors and chests** | Public | LockSmith | An always-open copy of every buildable door and chest, vanilla and other mods (`*_public`). Anyone can open them, wards or not. |
| **Paper (wall)** / **Paper (flat)** | Furniture | DrakesRenameit | A blank sheet on a wall or lying flat. Write on it like a sign. |

## Config (server-synced)

`BepInEx/config/com.drakesworkshop.essentials.cfg`. With `LockConfiguration` on (default), the server's values apply
to everyone and only admins can change them.

| Section | Setting | Default | |
|---------|---------|---------|---|
| 2 - Display cases | `Enabled` | `true` | Show the cases in the hammer. |
| | `DisplayCaseCost` | `Wood:10` | `Item:amount` pairs, comma separated (prefab names). |
| | `SmallDisplayCaseCost` | `Wood:5` | |
| 3 - Public doors and chests | `Enabled` | `true` | Show the public copies in the hammer. |
| 4 - Paper | `Enabled` | `true` | Show the paper in the hammer. |
| | `PaperCost` | `Wood:1` | |

Switches apply live and only show or hide pieces in the hammer: anything already built stays, and public pieces stay
public.

## With the full mods

If **DrakesItemShop** or **LockSmith** is installed, it provides its own version of that piece and Essentials steps
aside for it. The piece names are the same, so a world built with one keeps working with the other.

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
