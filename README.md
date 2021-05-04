# BuilderBrawlScripts
Script examples used in my latest mobile multiplayer game.

Demo game test (open in Edge):
https://simmer.io/@Periago/builder-brawl

- Birds: attached to bird prefabs, which makes a bird fly accross the sky every now and then.
- Building: handles its levels, health, upgrades, interaction with player.
- CameraWork: simple script to follow local player
- Handles player interaction. Player can interact with tiles, buildings, boats, using resources as scriptable objects.
- Resource + ResourceAsset: ScriptableObject scripts used throughout the game\
- Tile: attached to tile prefab, checks for neighbour tiles and continuously checks if there are opportunities for building, if so handles visualization
