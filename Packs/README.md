# Forge packs

The pieces' looks, as Drakes Asset Forge packs. Edit a pack in the app (open the folder as a pack), then regenerate its
code from the repo root. The export targets DrakeModsLibs' helpers; the `sed` points it at Essentials' own copy in
`Forge/` (this mod has no DrakeModsLibs).

```
DrakesAssetForge export-code Packs/DisplayCase . --look-only --libs --embed --into --namespace DrakesEssentials.Generated.Cases --folder Generated/DisplayCase
DrakesAssetForge export-code Packs/Paper . --look-only --libs --embed --into --namespace DrakesEssentials.Generated.Paper --folder Generated/Paper
sed -i 's/using DrakeModsLibs\.Forge;/using DrakesEssentials.Forge;/' Generated/*/Items/*.g.cs
```

- `DisplayCase`: same recipes and models as DrakesItemShop's `Forge/DisplayCase` (keep them in step).
  `make_display_case.py` builds the models; see DrakesItemShop's `Forge/DisplayCase/README.md`.
- `Paper`: DrakesRenameit's blank sheet recipes and paper art, with paper-sized collision. The text placement on the
  sheet is in `Paper/PaperPieces.cs`.
