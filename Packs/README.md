# Forge packs

The pieces' looks, as Drakes Asset Forge packs. Edit a pack in the app (open the folder as a pack), then regenerate its
code from the repo root. The export targets DrakeModsLibs' helpers; the `sed` points it at Essentials' own copy in
`Forge/` (this mod has no DrakeModsLibs).

```
DrakesAssetForge export-code Packs/DisplayCase . --look-only --libs --embed --into --namespace DrakesEssentials.Generated.Cases --folder Generated/DisplayCase
DrakesAssetForge export-code Packs/Paper . --look-only --libs --embed --into --namespace DrakesEssentials.Generated.Paper --folder Generated/Paper
DrakesAssetForge export-code Packs/ItemRack . --look-only --libs --embed --into --namespace DrakesEssentials.Generated.Racks --folder Generated/ItemRack
DrakesAssetForge export-code Packs/Mailbox . --look-only --libs --embed --into --namespace DrakesEssentials.Generated.Mail --folder Generated/Mailbox
sed -i 's/using DrakeModsLibs\.Forge;/using DrakesEssentials.Forge;/' Generated/*/Items/*.g.cs
```

- `DisplayCase`: same recipes and models as DrakesItemShop's `Forge/DisplayCase` (keep them in step).
  `make_display_case.py` builds the models; see DrakesItemShop's `Forge/DisplayCase/README.md`. The display mats
  (`Drake_DisplayMat`, `Drake_DisplayMatSmall`) are in the same pack: `make_display_mat.py` builds their models and writes
  their recipes; their floor is in `DisplayCases/DisplayCasePieces.cs`.
- `Mailbox`: the Mailbox and Drop Box recipes and models from DrakesItemShop's `Forge/Mailbox` (keep them in step; the PO box
  and post wall stay ItemShop only). `make_mailbox.py` builds the models; hinge points are in `Mail/MailboxPieces.cs`.
- `ItemRack`: same recipes and models as DrakesItemShop's `Forge/ItemRack` (keep them in step), armor and weapon
  racks. `make_item_rack.py` builds the models; see DrakesItemShop's `Forge/ItemRack/README.md`. Hang points are in
  `ItemRacks/ItemRackPieces.cs`.
- `Paper`: DrakesRenameit's blank sheet recipes and paper art, with paper-sized collision. The text placement on the
  sheet is in `Paper/PaperPieces.cs`.
