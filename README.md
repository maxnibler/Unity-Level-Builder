# Unity Level Builder

## How to use:
Access the Level Builder tool with the Level Builder Window. _Window >> Level Builder_<br/>
You can set the Height and Width of the grid you want to use. This is currently capped at 50x50 for the sake of performance.<br/>
If the grid does not appear at first you can fix this by adjusting the grid dimentions. This is currently a minor issue. <br/>
The "Brush" can be selected by pressing any of the labeled buttons. <br/>
If you draw the wall and floor layout you want on the grid, the program will select the correct wall tiles for each point. <br/>
When you press "Generate Scene" these tiles will be spawned in your currently active Scene.<br/>
These tiles will all be childed to an empty game object at the world's orgin.<br/>
You can use this parent object to rotate and position the level within your game world, or do easily delete the level as you make adjustments to your design. <br/>
You can create tilesets with your own assets by going to _Create >> Level Builder >> Tileset_. <br/>

## Tileset
This Tool assumes a set of 7 tiles, which must all be included in the tileset for it to function correctly.<br/>

I call the assets tiles, but this tool is in no way related to the Unity Tilemap system.<br/>



1. Floor
2. Column
3. Edge Wall
4. Line Wall
5. Corner Wall
6. T Shaped Corner Wall
7. X Shaped Corner Wall<br/>

## How to Install:
1. Open the Unity Package Manager
2. Click the "+" button in the top left of the package manager window
3. Select "Add package from git URL"
4. Paste the following url: https://github.com/maxnibler/Unity-Level-Builder.git
5. Click "Add"
