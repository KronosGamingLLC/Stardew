#AFK Hosting
Allows the host to be afk and still host the game via an auto sleep. This is designed to have a makeshift dedicated server.
-PLAYER MUST BE 'TOUCHING' BED TO AUTOSLEEP (mostly in the bed but not necessarily enough to trigger the normal sleep dialog)
	-this is to avoid editing too much base game functionality and introducing more potential bugs
-Disabled in single player mode
-Disabled while no other players are online
-Can toggle on and off
-Works for non-host players too if desired (although i would assume most players would just disconnect)
-Configurable
	-Toggle Key
	-On at game start

Do to the way stardew handles multiplayer sleeping, sleeping in general, and new days, automatically triggering sleep when sleep is possible seems to be the safest way to continue to the next day with an afk player

TODO
-Pause time when afk mode is on and no other players are online. Additionally add time scaling and pause functionality since this would be incompatible with other mods that contain these features.
