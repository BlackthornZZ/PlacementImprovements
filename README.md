<p align="center">
<img width="750" height="360" alt="PlacementImprovementsV1 1IconNewTitle" src="https://github.com/user-attachments/assets/ecf7590f-184d-4caf-8a02-b205b1a04a17" />
</p>

**Placement Improvements is unofficial Fan Content permitted under the [Fan Content Policy](https://www.slimerancher.com/fan-content-policy/). Not approved/endorsed by Monomi Park. Portions of the materials used are property of Monomi Park, LLC. ©Monomi Park LLC**

Also available on [Nexus Mods](https://www.nexusmods.com/slimerancher2/mods/179)

## Installation instructions
- (Install MelonLoader v0.7.3 to Slime Rancher 2)
- (Install the mod Starlight 4.0.3)
-  Download the `BuildImprovements.dll` file from the Releases page on the right
   - For modders using UnityExplorer: download the `BuildImprovements-UnityExplorerCompat.dll` file.
-  Place it in Slime Rancher 2\Mods.
-  Launch the game!
## Main features
- Custom Placement Colors
- Individually toggleable build checks
  - Disable the "Must stand on ground" requirement
  - Allow clipping with other gadgets (clipping is clearly shown by the placement turning orange)
  - Allow placement of gadgets on slopes
  - <details><summary><b>Game Spoilers!</b></summary>
    Allow building in the Prismacore, when it is harmonized.
   </details>
   
  - Allow placement of floating gadgets
  - Allow infinite range on linked gadgets
- Placement locking and nudging / Advanced Movement.
  - Allows the player to walk around a gadget and view it from all sides. Also allows vertical and horizontal movement to clip gadgets into things more easily or place floating gadgets.
- Tutorials & UI integration
- Gadget eyedropper / Copy
  - Allows the player to look at a gadget in the world and start building it immediately (given they have it in storage).
   
## Requirements & Compatibility
| Mod / Software Name | Minimum Version | Links | Required? | Compatible? |
|---------------------|-----------------|-------|:---------:|:-----------:|
MelonLoader | 0.7.3 | [GitHub](https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.3) / [Website](https://melonwiki.xyz/) | &check; | &check;
Starlight / SR2E | 4.0.3 | [GitHub](https://github.com/ThatFinn/Starlight/releases) / [Nexus](https://www.nexusmods.com/slimerancher2/mods/60) / [Website](https://starlight.sr2.dev/downloads) | &check; | &check;
Ranching Together | beta-0.3.5 | [GitHub](https://github.com/pyeight/SlimeRancher2Multiplayer/releases/tag/Beta_0.3.5) / [Nexus](https://www.nexusmods.com/slimerancher2/mods/118) | &cross; | &check;
Build Anywhere | Any | Deprecated | **&cross;** | **&cross;**
## Known Issues
- If the game is paused while in gadget mode with a gadget selected and the gadget is unlocked, the Locked Input Legend will disappear.
	- Workaround: Reload your save.
- The first time any gadget is selected in a session the input legend will not have the mod's input hints shown. All selections afterwards do not suffer from this issue.
- When a gadget is eyedropped, the previously selected gadget is shown in the hotbar as the selected gadget, rather than the eyedropped gadget. The eyedropped gadget can still be placed.
- When switching between variants, gadgets do not remain locked (because the variant is an entirely seperate gadget internally)
- The mod crashes the game when ran together with UnityExplorer. 
	- Workaround: Download the UnityExplorerCompat dll version from the release to remove the UI patch that causes the crash.
(If you find any more issues you can create an issue on the github repository.)
