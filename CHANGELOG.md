# Changelog

## Unreleased

### Fixed

- Packaging only: the Hexium/Thunderstore zip now puts the DLL and every data folder under `plugins/`, so Gale and r2modman
  keep the folder structure (they flatten anything outside `plugins/`). A CI guard (`check-gale-layout.py`) fails the build
  if that ever regresses. No code or behaviour change for players.

## 0.2.0
- **Mailbox and Drop Box** (Furniture tab): a post-mounted mailbox and a tabletop box. Anyone can send up to 3 stacks and a note through the front slot (one way, with a warning before sending); the back is a plain chest that wards and locks protect. Hinged flap and door animate, the cavity is black inside, and the send window sits to the right so you can watch the box open. Owners set an allow list, an item filter and an open/closed switch (Shift+E). Costs are in config section `7 - Mailboxes`. If DrakesItemShop is installed it provides these pieces instead, so they are never doubled.
- **Display Mat and Small Display Mat** (Furniture tab): a jute runner in an iron-bound tray you lay on a table; what you put in it lies on show. Config section `6 - Display mats`.
- Messages that tell you why something can't be done are shown through a small shared helper that works on every game version.
- Stable release (no longer a beta).

## 0.1.2-beta.1
- Beta.
- **Weapon rack poses** are per weapon: a configurable style, size, turn and offset (`rack-poses.yaml`), synced from the server. Reskinned weapons are posed as the item they look like.
- Back row of a double-sided stand hangs facing the other way.
- Display cases draw only the renderers that will show, so food on a case isn't sized or centred on its hidden table model.
- Regenerated the big and small weapon stand models.

- **Armor on racks hangs worn**, the way an armor stand shows it: chest pieces and capes from the neck, helmets by the top, legs at the hips (baked once per item, so racks stay cheap).
- **Display Case holds 7 items** (was 6).
- Small items on display cases no longer vanish once shrunk onto the jute (they keep their closest detail level).
- Wall racks can go on tilted surfaces.
- Display cases and racks have their own hammer icons, rendered from the piece instead of the chest they're made from.

## 0.1.0
First release: the best pieces from the DrakeMods suite, on their own (no DrakeModsLibs needed).
- **Display Case** (6 slots) and **Small Display Case** (3 slots), from DrakesItemShop: glass and wood cases that show what's inside lying on jute. Fill them from the door side; the glass door slides open while you do.
- **Armor, weapon and wall racks**, from DrakesItemShop: finewood racks with iron hooks and pegs. Free-standing armor racks and weapon racks (6 or 4 slots) take only armor or only weapons and shields; wall racks (5 or 3 slots) take both. What's inside hangs on the rack; weapons and shields are posed like on an item stand.
- **Public doors and chests**, from LockSmith: an always-open copy of every buildable door and chest (vanilla and other mods) in the hammer's Public tab. Anyone can open them, wards or not.
- **Paper (wall)** and **Paper (flat)**, from DrakesRenameit: blank sheets at RenameIt's size that you write on like a sign.
- Costs: display cases FineWood 10, Red jute 2, Crystal 2 (small: 5, 1, 1); racks FineWood plus one BronzeNails per slot; paper 1 Wood. All configurable.
- Each feature has a server-synced on/off switch that applies live; placed pieces always stay.
- Works next to the full mods: DrakesItemShop provides the cases and racks, LockSmith the public doors and chests (and its config rules them), and with DrakesRenameit Essentials' paper leaves the hammer. Each only takes over its own pieces, so any mix works; piece names match, so worlds move between them.
- ServerSync is bundled and patched for Valheim 1.0 (`ZRoutedRpc.Everybody` is now a constant), so live config changes reach players.
