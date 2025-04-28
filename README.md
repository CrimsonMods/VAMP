# NOTICE - PREVIEW BUILD
This is a preview build of VAMP for 1.1. Use this mod at your own risk on live servers until it has been marked as stable (the update that removes this notice). 
If you want to help testing out VAMP (and/or other CrimsonMods) you're more than welcome to do so, but you accept the risk of bugs. 

DO NOT USE BEPINEX ON THUNDERSTORE. [GET IT FROM HERE INSTEAD](https://github.com/decaprime/VRising-Modding/releases).

You can report any encountered bugs or feedback to the [Modding Discord](https://discord.gg/xzd5U5cNyD)

Known Issues:
- Spawn System currently doesn't work as they removed the "OLD" buffs. A fix is in the works. 

![vamp-banner](https://i.imgur.com/R5xL2Eg.png)

# V Rising API Modding Platform

VAMP is a comprehensive modding framework for V Rising, providing developers with powerful tools to create and modify game content. VAMP offers a structured API to interact with game systems, entities, prefabs, and more, enabling the creation of complex mods that enhance gameplay experiences.

Features
- Entity Management: Easily create, modify, and manipulate game entities
- Player Interaction: Access player data, inventory, equipment, and status
- World Manipulation: Work with world zones, territories, and positions
- Prefab System: Utilize the extensive collection of game prefabs for NPCs, abilities, and items
- Castle Management: Interface with castle hearts and territories
- Unit Customization: Create and modify units like horses, NPCs, and monsters
- [EventScheduler](https://vrising.wiki/docs/event-scheduler.html): A global manager for running event mods
- And more...

The original intention for this mod was to stop copy-pasting a large amount of code between projects, especially files like ECSExtensions and Core. Then it expanded from there to include systems that were used in a majority of my CrimsonMods. Now it has been standardized and documented for use by other modders to make modding V Rising easier and more accessible. 

*The API is very much still a work in progress and will likely change a bit over time as I continue to use it and add more features.*

## Getting Started

If you're simply a player:
- Install the version of BepInEx that is defined in the Dependencies.
- Extract *VAMP.dll* into *(Game/Server Folder)/BepInEx/plugins*

If you're a developer:
VAMP is available as a NuGet package for easy integration into your projects
```
dotnet add package VRising.VAMP
```

Or add it directly to your project filee:
```
<PackageReference Include="VRising.VAMP" Version="x.x.x" />
```

## Resources
[API Documentation](https://vrising.wiki/)

[Modding Discord & Support](https://discord.gg/xzd5U5cNyD)

[GitHub Repository](https://github.com/CrimsonMods/VAMP/)

[NuGet Package](https://www.nuget.org/packages/VRising.VAMP)

## Contributing

We welcome contributions from the community! If you'd like to contribute to VAMP, please join the [discord server](https://discord.gg/xzd5U5cNyD) and join the conversation in the VAMP channel.

## Support

Want to support my modding development? 

**Buy / Play My Games!** 

[Train Your Minibot](https://store.steampowered.com/app/713740/Train_Your_Minibot/) 

[Boring Movies](https://store.steampowered.com/app/1792500/Boring_Movies/) **FREE TO PLAY**

[git gud](https://store.steampowered.com/app/1490570/git_gud/) **WISHLIST COMING SOON**

Donations Accepted

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/skytech6)

**This mod was partially a paid creation. If you are looking to hire someone to make a mod for any Unity game reach out to me on Discord! (skytech6)**