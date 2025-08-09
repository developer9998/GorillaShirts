<div align="center">
 <img src="https://raw.githubusercontent.com/developer9998/GorillaShirts-Legacy/refs/heads/main/Marketing/Banner.png" width=70% height=auto</img><br>
</div><br>

<div align="center">
  <a href="https://discord.gg/dev9998"><img src="https://img.shields.io/discord/989239017511989258?label=Discord&style=flat-square&color=blue"></a>
	<a href="https://github.com/developer9998/GorillaShirts/releases/latest"><img src="https://img.shields.io/github/v/release/developer9998/GorillaShirts?label=Version&style=flat-square&color=red"></a>
	<a href="https://github.com/developer9998/GorillaShirts/releases"><img src="https://img.shields.io/github/downloads/developer9998/GorillaShirts/total?label=Downloads&style=flat-square&color=green"></a>
  <a href="https://github.com/developer9998/GorillaShirts/blob/main/LICENSE/"><img src="https://img.shields.io/github/license/developer9998/GorillaShirts?label=License&style=flat-square&color=yellow"</img></a>
</div><br>

*GorillaShirts* is the leading custom cosmetic mod for [Gorilla Tag](https://gorillatagvr.com). The mod has been universally acclaimed by viewers, modders, and content creators alike since it's early access release and public launch during 2023.

## Usage

*GorillaShirts* implements a structure known as the Shirt Stand, a user accessible stump present in most maps used to control the mod, including the shirts you wear, the releases you look up, and the settings you configure.

## Features

*GorillaShirts* contains a wide array of features to make having the mod stand out in addition to improving user experience. 

This section contains the more significant features included within *GorillaShirts*, the rest is up to you to discover!

### Packs

*GorillaShirts* refers to a collection of shirts as a pack. 

Packs are useful in terms of categorization and organization of shirts, and can be managed in the form of releaes using the Pack Browser.

### Configuration

*GorillaShirts* currently implements a few configurable options that can be set using the Shirt Stand, and specifically on the sidebar.

- **Character Identity** (``string``)<br>
  The Shirt Stand possesses a gorilla you can configure based on identity, including Masculine (Steady) and Feminine (Silly).

- **Tag Offset** (``int``)<br>
  The player is able to offset their name tag based on this configuration. This is useful for presenting your name clearly, rather then having it clip into your shirts.

### Anchors

*GorillaShirts*, although an extention to existing game cosmetics, can modify those cosmetics worn by a player in terms of their anchors.

Anchors that *GorillaShirts* can apply include name tag, badge, and chest/slingshot anchors. 

Anchors are used by a custom and base game shirt in the same manner, mostly for clarity, so the name tag, badge, and slingshot of a player can be presented all while wearing shirts.

### Networking

*GorillaShirts* uses Photon's PUN (**P**hoton **U**nity **N**etworking) backend to network various data, which includes:

- **Version**<br>
  What version of GorillaShirts the player is using.

- **Shirts**<br>
  An array of shirts the player has worn on them.

- **Tag Offset**<br>
  The configured tag offset the player has set.

You may view the shirts of another player if you have the shirt they're using installed. If you don't have it, a fallback is used when possible.

A fallback refers to a shirt found within the "Default" shirt pack to use on a player when their intended shirt cannot be used, in this case, when the local player doesn't have the shirt installed.

This fallback might not be assigned, but when it is, it's done by the shirt, and cannot be configured at runtime (in Gorilla Tag).

*GorillaShirts* is a mod for Gorilla Tag. The mod is able to import community-made shirts into the game, which then can be worn and seen by others with the mod. You can create your own shirts using the [GorillaShirtsUnityProject](https://github.com/developer9998/GorillaShirtsUnityProject).

## Instructions

Listed below are instructions regarding a handful of circumstances, but mostly surround downloading and installation.

### Downloading GorillaShirts (Method 1)

Method 1 refers to the automatic process of a program downloading and installing ``GorillaShirts`` before you. Though with most applications you download, please be cautious of malware and RATs, those unfortunately circulate more than you think.

A program capable of this method is **MonkeModManager**. There are two instances of the application you can use:

- **Recommended by Dev:** @arielthemonke Version
  - [Windows Download](https://github.com/arielthemonke/MonkeModManager/releases/latest/download/MonkeModManager.exe)
  - [Linux Download](https://github.com/arielthemonke/MonkeModManager/releases/latest/download/MonkeModManager.Linux)

- @The-Graze Version
  - [Windows Download](https://github.com/The-Graze/MonkeModManager/releases/latest/download/MonkeModManager.exe)
 
Using either version, under the "Cosmetic" section, "GorillaShirts" can be located. You can proceed to select the mod and start the installation/update process.

### Downloading GorillaShirts (Method 2)

Method 2 refers to a more manual way of downloading and installing ``GorillaShirts``. Though the process isn't difficult at all, I'd recommend the first if you don't know what you're doing.

To download *GorillaShirts*, navigate to the "Releases" tab to the right of the page, or click on the "Version" or "Downloads" button at the start of the page to view the latest release.

At a release, a ``GorillaShirts.dll`` is present. This is the file you download, and consists of the entirely of the mod.

If a ``GorillaShirts.pdb`` is included, that file may be downloaded as well. A ``.pdb`` (Program Database) defines information surronding GorillaShirts that can be helpful for debugging.

### Installing GorillaShirts (Method 2)

To install *GorillaShirts*, place your downloaded ``GorillaShirts.dll`` file directly into your plugins folder.

That pretty much covers it. ``GorillaShirts`` doesn't require any other dependencies, with no requirements needed other than Gorilla Tag.

If you did download the accompanied ``GorillaShirts.pdb`` file, simply place it alongside ``GorillaShirts.dll``. 

## Credits

*GorillaShirts* is a product contributed to by many during its continued development since 2022.

Supporters of Dev's Patreon and Ko-fi services are listed within the info section of the mod, in addition to everyone listed below.

### Lead Contributors

- **dev9998**: Creator (programmer and designer)

- **gizmogoat**: Tester

- **crescentmondo**: Artist (2D sprites and 3D models)
    
### Special Thanks

- **sushii64**: Testing

- **ch7ken**: Turtleneck idea and assistience

- **ojsauce**: Tag Offset and Photo Capture ideas and assistience

## Disclaimer

<h4 align="center">
This product is not affiliated with Gorilla Tag or Another Axiom LLC and is not endorsed or otherwise sponsored by Another Axiom LLC. Portions of the materials contained herein are property of Another Axiom LLC. © 2021 Another Axiom LLC.
</h4>
