HelloWorld
==========


Shared Project
--------------

The game code of this project resides in a _shared project_. To be able to
use shared projects in Visual Studio you must first download the extension:
https://visualstudiogallery.msdn.microsoft.com/315c13a7-2787-4f57-bdf7-adae6ed54450


Setup New Project
-----------------

###### Mac OSX or Linux:
```base
$ git clone git@github.com:ludomotion/helloworld.git newproject
$ cd newproject/Tools/
$ go run rename-project.go ../ New Project
```

###### Windows:
 1. Clone this project into a new directory
 2. Open the `./Tools/` directory in Explorer
 3. Execute (double-click) the **rename-project.exe** file
 4. Enter a new project name


Assets
------

 * http://opengameart.org/content/attack-animation-for-16x18-rpg-base-sprites
 * http://kenney.nl/assets/roguelike-rpg-pack
