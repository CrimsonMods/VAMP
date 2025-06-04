![vamp-banner](https://i.imgur.com/R5xL2Eg.png)

# V Rising API Modding Platform

VAMP is a comprehensive modding framework for V Rising, providing developers with powerful tools to create and modify game content. VAMP offers a structured API to interact with game systems, entities, prefabs, and more, enabling the creation of complex mods that enhance gameplay experiences.

Features
- Entity Management: Easily create, modify, and manipulate game entities
- [Player Interaction](https://vrising.wiki/docs/player-service.html): Access player data, inventory, equipment, and status
- World Manipulation: Work with [world zones](https://vrising.wiki/docs/worldregion-data.html), territories, and positions
- Prefab System: Utilize the extensive collection of game prefabs for [NPCs](https://vrising.wiki/docs/vbloods-data.html), abilities, and items
- [Castle Management](https://vrising.wiki/docs/castle-service.html): Interface with castle hearts and [territories](https://vrising.wiki/docs/territory-service.html)
- [Unit Customization](https://vrising.wiki/docs/spawn-service.html): Create and modify units like horses, NPCs, and monsters
- [EventScheduler](https://vrising.wiki/docs/event-scheduler.html): A global manager for running event mods
- [FileReload](https://vrising.wiki/docs/file-reload.html): A system for automatically reloading files without server restart
- [WipeData](https://vrising.wiki/docs/wipe-data.html): A system for resetting mods automatically for server wipe
- [ModTalk](https://vrising.wiki/docs/mod-talk.html): A simple low-friction system for inter-mod communication
- [ModSystem](https://vrising.wiki/docs/mod-system.html): Automatic notification of mod updates on Thunderstore
- [ChatUtil](https://vrising.wiki/docs/chat-util.html): An extension of the System Chat System
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