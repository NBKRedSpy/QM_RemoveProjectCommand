[h1]Quasimorph Remove Project Command[/h1]


A command to free up a weapon or armor upgrade project.

[h1]Automatic Backup[/h1]

Before the command modifies a save slot, a backup zip file for the target save files will be created in the game's user folder.

Example:
[i]%AppData%\..\LocalLow\Magnum Scriptum Ltd\Quasimorph\slot_2_20240730_171029_698.zip[/i]

The file name is created with a date based suffix.

To undo the changes, quit the game or go back to the main menu.  Unzip the backup file to the folder, overwriting the [i]slot_<number>_*[/i] files.  Do not extract to a sub folder.

[h1]Usage[/h1]

Example command: [i]remove-project 2 army_knife[/i]

Requires the Developer Console https://steamcommunity.com/sharedfiles/filedetails/?id=3281579458 .

This command is only available at the main menu.

At the main menu, open the command console (`).

Type [i]remove-project[/i], space, the save's slot number (0-2), then the tab key.
The command will list all of the projects in the save that can be removed.
Note that the slot numbers start at zero, so valid values are 0-2.

If there is more than one project, use the up and down arrows to select the line the with target project. If there is only one project, the command line will already have the project entered.  Press enter.

[h1]Effects[/h1]

The command will remove the project from the Magnum and replace every modified version with the base version.
Due to how the game stores an item's durability, items with durability increases will not be rolled back to the base value.

Any affected items can be deleted at inventory screens if desired.

[h1]Support[/h1]

If you enjoy my mods and want to buy me a coffee, check out my [url=https://ko-fi.com/nbkredspy71915]Ko-Fi[/url] page.
Thanks!

[h1]Source Code[/h1]

Source code is available on GitHub at https://github.com/NBKRedSpy/QM_RemoveProjectCommand
