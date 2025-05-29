`latest`
- Hotfix for CleanItemName (CrimsonLog has a crash from it)

`1.1.0`
- Added [ModTalk System](https://vrising.wiki/docs/mod-talk.html)
- Added ChatUtil.MESSAGE_LIMIT readonly int
- JSONUtil.PrettyPrintOptions now supports '

`1.0.0`
- Updated for Stable
- Added FileReload System
- Expanded ClanService with Membership Functions
- Added Documentation for ClanService
- Added Lucile to VBloods
- Fixed Stavros's Default Name to correct value
- Added TimeOnly & TimeOnlyHourMinute JSON Converters (JSONUtil)
- Added DayOfWeek JSON Converter (JSONUtil)
- Added ChatUtil.SystemSendUsers(User[], string)
- Added Multiple Documentation Pages
- Added JSON Comment Header Builder (JSONUtil)

`0.3.4`
- Update for Hotfix 6
- Added ClanService.GetByNetworkId(networkId)

`0.3.3`
- PlayerService critical bug fix

`0.3.1`
- Update for Hotfix 5

`0.3.0`
- Added utility functions to Data.WorldRegions
- Updated Data.VBloods for 1.1 Bosses
- Formatted WorldRegions and VBloods for both Full and Short names
- Added user customizable JSONs for both WorldRegions and VBloods
- Added JsonUtil
- Added optional bool to get short region name with Player.GetWorldZoneString(bool = false)
- Added Documentation for all new functionality
- Updated the Table of Contents for Docs for ease of use

`0.2.3`
- Updated for 1.1 Hotfix 3
- Added ChatUtil.SystemSendAdmins
- PlayerService now uses Player model
- Old PlayerService methods marked deprecated
- Added static Events class with OnCoreLoaded (more coming next update)
- Added CleanItemName to ItemUtil
- Updated Documentation for PlayerService and ChatUtil

`0.2.2`
- Added ChatUtil.SystemSendAllExcept
- Added IsAdminCapable to Player
- Added new ways to convert to Player model from Character Entity and User
- New Vampires now add to PlayerService cache
- Oakveil Woodlands added to World Regions
- 1.0 VBlood Display Names now in Data.VBloods (1.1 VBloods will be added in a later update)

`0.2.0`
- Initial release