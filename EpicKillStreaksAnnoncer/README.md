
# Epic KillStreaks Announcer
What's more rewarding than an epic announcer commenting on your best kill streaks? This mod adds (on the client side) the famous Unreal Tournament 2004 Announcer to your game! Will you manage to do a monster kill?

## What's new in this update ?
Add HjUpdaterApi as dependency. So updates will be performed automaticly in the futur.

## Features
Whenever you get a kill, the mod registers it and if you managed to reach a certain amount of kills, each one seperated by a maximum of 3seconds (configurable), it will trigger an epic sound.
Also, if you have this mod installed and you are hosting the game, it will notify everyone using the Chat if one user reached the GodLike or the MonsterKill streaks, regardless of the fact that this user has the mod installed or not (configurable).

## Config
Before installing, please remove the old config file if you have used an other version of this mod before 1.1.0.
You can change the number of kills needed to reach each sound in the config file. Put -1 to remove one sound. You can reorder them but GodLike & Monster Kill have to be the two highest killstreaks. By default the values are:

 - Headshot : -1 (deactivated, replace by 1 if you want to activate it)
 - Double kill : -1 (deactivated, replace by 2 if you want to active it)
 - Triple Kill : 3
 - Multi kill : 5
 - Dominating : 8
 - Rampage : 12
 - Ludicrous Kill : 16
 - God like : 20
 - M-M-Monster kill : 25

You can also change the maximum time before the killing streak is reset. By default it's set to 3sec.
Finally change the Broadcast value to false to deactivate the chat broadcast on high tier kill streaks.

## Multiplayer
This is a client side mod. That mean that only you can hear the announcer, and that it doesnt affect any other player's game.
If you are hosting the game however, you are also tracking other players killstreaks, and if one manage to reach one of the 2 biggest kill streaks, a message will appear in the chat.
## Planned Update
 - On my next mod release, I might have to deploy a compatibility patch on this one (not sure yet).


## Versions
 - 1.0.0 - Initial release
 - 1.0.4 - No more structure file and the thing is hopefully finally working
 - 1.1.0 - A LOT of rework on the mod. Like i almost remade it completly. But now it works in multiplayer !
 - 1.1.1 - Small patch to allow you to set any value on the config file for killstreaks. So now you can reorder or deasctivate any sound. No need to update if you don't change the default values.
 - 1.1.2 - Updating to the lastest BepinEx, R2API and add compatibility with ConfigManager, removed AssetPlus dependency
 - 1.1.3 - Fix : Minion kills were not counted & monster suicides were blocking the port to 99%.
 - 1.2.4 - Add HjUpdaterApi as dependency. So updates will be performed automaticly in the futur.
 - 1.2.5 - Add the dependency directive so that it doesnt raise an error but a warning if you forget the dependency

## Contact
I'm available on the ROR2 Modding discord server (@Hijack Hornet). 
Please send me feedback! This is my second mod so I would really appreciate it.
Even if it's just to say that you like it :)

##Thanks
Thanks to all the devs that helped me do this, and thanks to my beta testers: Ravens Queen, FatansticFungus & Twyla
Thanks to Miss_name for making AssetPlus and helping me with it :) Thanks also to the mod community for helping me so much in the process. Harb, iDeathHD you're the best <3'
And thanks to @sly for correcting my english.