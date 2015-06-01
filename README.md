HelloWorld
==========

This is a template project for the C# game engine [Phantom][github-phantom],
used at Ludomotion as skeleton for new projects.

#### It features:
 * A basic Phantom setup;
   - the Konsoul
   - one [GameState](Game/PlayState.cs) with an entity layer
   - a tiled [world](Game/World.cs) and a [player](Game/Mobs/Player.cs) with basic collision
 * Textures, phonts and audio.
 * Fullscreen toggle.
 * Build-tools to publish for Windows or OSX.
 * A project-rename tool.
 * Support for Xamarin (with some fiddling).


Shared Project
--------------

The game code of this project resides in a _shared project_. To be able to
use shared projects in Visual Studio you must first download the extension:
https://visualstudiogallery.msdn.microsoft.com/315c13a7-2787-4f57-bdf7-adae6ed54450
(Xamarin already supports this)


Setup New Project
-----------------

###### Windows:
 1. Clone this project into a new directory
 2. Open the `./Tools/` directory in Explorer
 3. Execute (double-click) the **rename-project.exe** file
 4. Enter a new project name (space seperated words)

###### Mac OSX or Linux:
```bash
$ git clone git@github.com:ludomotion/helloworld.git newproject
$ cd newproject/Tools/
$ go run rename-project.go ../ New Project
# where 'New Project' is the new project name (space seperated words)
```


Publish
-------

###### Windows Build:
 * Install NSIS (on the default location): http://nsis.sourceforge.net/Main_Page
 * Execute (double-click) the ./Tools/**build-windows.exe**


###### Mac OSX Build:
 * Execute (double-click) the ./Tools/**build-osx.exe**

(running Mac OSX, install golang and `go run build-osx.go`)


Assets
------

The game itself is just a placeholder using opensource assets:

 * http://kenney.nl/assets/roguelike-rpg-pack
 * http://opengameart.org/content/attack-animation-for-16x18-rpg-base-sprites


[github-phantom]: https://github.com/ludomotion/phantom  "Phantom Code"
