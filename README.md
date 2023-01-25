# RPG Viewer Client

Official cliend-side application for RPG Viewer VTT

By reading this guide, you will understand everything you need to know about this application and its features

## Table of contents

<!--ts-->
* [Connecting](#connecting)
* [Accounts](#accounts)
* [Sessions](#sessions)
* [Game view](#game-view)
  * [Side buttons](#side-buttons)
  * [Game master](#game-master)
* [Scenes](#scenes)
  * [Creation & modification](#creating-and-modifying-scenes)
  * [Tokens](#tokens)
  * [Lights](#lights)
  * [Notes](#notes)
* [Blueprints](#blueprints)
  * [Permissions](#permissions)
    * [Owner](#owner)
    * [Observer](#observer)
    * [None](#none)
* [Keyboard shortcuts](#keyboard-shortcuts)
<!--te-->

## Connecting

![image](https://user-images.githubusercontent.com/70658690/214673500-f749f687-2c3d-4cd9-9583-6286c23b1621.png)

* When the application is started, an IP address must be specified to which the application will try to connect
* This can be done from the top right corner of the main menu
* Syntax: `<IP address>:<port>` For example: `127.0.0.1:3000` will connect to localhost, on port 3000

* Once the connection is established for the first time, the application will be automatically connect to that IP address upon opening
* IP address can always be changed by typing a new address in the top right corner of the main menu, following the syntax above

## Accounts

![image](https://user-images.githubusercontent.com/70658690/214652812-8cc895ca-c9bf-44df-b8dd-da1c57a29da0.png)

Account's data will be synchronised accross all devices, which includes the following 
* Sessions
* Blueprints
* Scenes
* Notes

## Sessions

![image](https://user-images.githubusercontent.com/70658690/214652734-9a858f7b-e1bb-48d0-98db-562537aa838f.png)

* There is no limit to how many sessions you can create.
* The landing page is visible when scenes are not loaded or the session is not synchronised with players (more on this [later](#side-buttons)).
* Clicking the button next to the drop down menu in the join panel will copy the id of that session to the clipboard
* Invite someone to that game session by giving them that id
* Apply the invitation by pasting the key you received into the right-most panel.
* You can remove your licenses for all sessions you have been invited to by clicking on the `Remove licenses` button.

## Game view

![image](https://user-images.githubusercontent.com/70658690/214657068-9f40dfe2-5517-4722-a815-629aa27b3fd2.png)

* When you join for the first time, you should see your landing page in the background
* As a player you will not see the side panel on the right. It is only visible to Game master

### Side buttons
  
  All the buttons will be visible only when a scene is active
  
  * ![image](https://user-images.githubusercontent.com/70658690/214668643-62a6a8a0-b93e-42c8-86e3-ec4aa5bfb505.png) 
    Panning tool
    * Move camera using right mouse button
    * Click on doors, tokens and notes using left mouse button
    * Quick select this tool by clicking `1` on your keyboard 
  * ![image](https://user-images.githubusercontent.com/70658690/214668692-54e15ca9-6ead-4a3c-a8b8-70481f1c7e29.png)
    Measurement tool
    * ![image](https://user-images.githubusercontent.com/70658690/214667672-31709621-4d3c-45b0-9f8a-29c6b1ae0eb5.png)
      Precise. Measures every foot
    * ![image](https://user-images.githubusercontent.com/70658690/214667880-22d375dc-1d22-4eb9-8caa-bf18dfff73a0.png)
      Cell-based. Snaps measurement points to cells in a grid. Distances are calculated using D&D 5e's optional movement.
      Each cell counts as 5 feet, and every other diagonal counts as 10 feet.
    * Right clicking during measurement will add waypoints
    * Quick select this tool by clicking `2` on your keyboard
  * ![image](https://user-images.githubusercontent.com/70658690/214668832-1b29f490-d61a-4cbc-854d-37bffc5473e0.png)
    Ping tool
    * The markers remain on the map for 10 seconds
    * Long pressing left click will move every player's camera to that location
    * Quick select this tool by clicking `3` on your keyboard
  * ![image](https://user-images.githubusercontent.com/70658690/214669095-5fcb56cc-6761-445a-b79b-724a66db672e.png)
    Fog tool (Game master only)
    * ![image](https://user-images.githubusercontent.com/70658690/214665922-02bbf0fb-7a02-4ef5-996c-f86ea6ebdf0a.png)
      Player view. Clicking a token with this enabled simulates that player's game view
    * ![image](https://user-images.githubusercontent.com/70658690/214666376-61d7809b-5965-4a11-8620-f1ab6350c443.png)
      Vision only. Clicking a token with this enabled shows where this player can see. Everything else is shown opaque
    * ![image](https://user-images.githubusercontent.com/70658690/214666211-3869adb5-cef2-45bf-b5fa-4a1bc8ab94b4.png)
      No Fog. This clears all fog and allows seeing the whole map and tokens
    * Quick select this tool by clicking `4` on your keyboard
  * ![image](https://user-images.githubusercontent.com/70658690/214669164-d0685566-3deb-4e2f-8311-75b8eeb3f662.png)
    Light tool (Game master only)
    * Quick select this tool by clicking `5` on your keyboard
    * Left clicking on a map will add a light source on that location
    * More on lights can be found [here](#lights)
  * ![image](https://user-images.githubusercontent.com/70658690/214669185-852c161f-72f9-4e4c-92fa-4785320d9334.png)
    Sync (Game master only)
    * Sync session to players
    * While locked, players will only see the landing page.
    * While synced, players will see active scene, if there is one
  * ![image](https://user-images.githubusercontent.com/70658690/214670372-ded947c8-111f-4d22-b477-d798084983f8.png)
    Center camera
    * Move camera back to center and reset zoom
  * ![image](https://user-images.githubusercontent.com/70658690/214670519-3ac5c9d6-7dae-497e-bd53-13da8f378945.png)
    Leave session
    * Leave session and return to main menu
    * Clicking this as a Game master will set sync state to locked, so that the palyers will only see the landing page

### Game master
* Create and modify scenes and blueprints by opening the tab on the right
* Scene configuration can be seen [here](#scenes)
* Creating and managing blueprints can be seen [here](#blueprints)
* While modifying or creating a scene, players will not be able to affect current scene, and will only see the laning page

## Scenes

* Create new scene from the right panel. This will open a new view.
* Set scene active by dragging them out from the side panel
* Right clicking a scene on the right panel allows you to delete, or modify it
* You can drag scenes and folers from one folder to another
* Rename folders by double clicking them

### Creating and modifying scenes

* ![image](https://user-images.githubusercontent.com/70658690/214675341-8c7a686f-066c-4b0a-9f14-f18b092f62b9.png)
  Wall tools
  * Limit player's vision and lights. Create toggleable doors, which can be hidden from players
  * ![image](https://user-images.githubusercontent.com/70658690/214675899-47515036-173d-4898-96a2-0c3f6f4ff50a.png)
    Reguar walls. Block player movement, lights, and vision
  * ![image](https://user-images.githubusercontent.com/70658690/214676076-45314635-07c2-4cd8-bc87-5ac83f88a17e.png)
    Doors. Functions just like regular walls. Can be toggled on and off (door-icons visible to players)
  * ![image](https://user-images.githubusercontent.com/70658690/214676358-0033633d-dc2f-4047-b1f4-08cd886ff615.png)
    Hidden doors. Functions just like a normal door, but the door-icons are not visible to players
  * ![image](https://user-images.githubusercontent.com/70658690/214676578-7f57c534-bdde-42c7-97a4-b97ebd2c9a4e.png)
    Invisible walls. Blocks player movement, but not lights or vision
  * Click and drag to create walls
  * Hold `Ctrl` and drag from wall endpoint to continue the wall
  * Click to a wall edge point to select it. Press `Backspace` or `Delete` to delete that wall point
  * Snap wall endpoints together by dragging an edge point on top of another
* ![image](https://user-images.githubusercontent.com/70658690/214675430-c027dc43-bb5f-448b-a735-1153f5692736.png)
  Grid configuration
    * Configure grid for this scene. Enable grid snapping and measurement tools
    * ![image](https://user-images.githubusercontent.com/70658690/214678419-9204d23c-5d3d-46a0-ad91-b1024b82a583.png)
      Configure grid settings
      
      ![image](https://user-images.githubusercontent.com/70658690/214679711-45a6f21d-d819-4b42-aa9f-07338ee8c5bc.png)
      
      * `Enable grid`: Enables / disbles grid for this scene
      * `Snap to grid`: Snaps tokens to grid cells. Does nothing when grid is disabled
      * `Grid dimensions`: Grid size. Default size when the scene is created is `10x10`
      * `Grid color`: Opens a color picker, where you can pick the color for your grid.
      
        There is currently a known bug, where you have to click this button at least twice to update the color picker's color
      * `Grid opacity` Opacity of the grid. Set to `0` to hide the grid
      * To save changes, click on `Save`
      * To discard changes, click on `Close`
    * ![image](https://user-images.githubusercontent.com/70658690/214679223-86133691-198f-48aa-ad0f-6190263023ce.png)
      Enable & disable grid visibility. Applies only to modification view
* ![image](https://user-images.githubusercontent.com/70658690/214675490-301ee42f-5a21-480c-bf64-7c95b8a17ff7.png)
  Fog configuration
  * Limit players' vision. Enhance the scene with dynamic lighting and shadows
  
  ![image](https://user-images.githubusercontent.com/70658690/214681958-d89da85f-4b01-494e-b42c-af74ff1b5025.png)
  
  * `Enable fog of war`: Enables / disbles fog of war for this scene
  * `Global lighting`: Light up the whole scene. Lights have no effect when this is enabled
  * `Fog color`: Opens a color picker, where you can pick the color for your fog.
      
      There is currently a known bug, where you have to click this button at least twice to update the color picker's color
  * To save changes, click on `Save`
  * To discard changes, click on `Close`
* ![image](https://user-images.githubusercontent.com/70658690/214675517-3c7d468f-e4d9-4d97-bd9b-8f692b15110e.png)
  Night filter
    * Apply night filter to this scene, which will create an illusion of night lighting

  ![image](https://user-images.githubusercontent.com/70658690/214682559-c3151e92-1a7c-4150-af83-7e16d75bae12.png)
  
    * `Night effect strength` control how much night filter is applied. `0` is no effect, and `100` applies the maximum effect
* Rename scene from the upper left corner
* ![image](https://user-images.githubusercontent.com/70658690/214683961-78a3a849-83c5-4122-9d99-76a5a3bfebf5.png)
  Save and exit 
  
### Tokens

While you are the owner of the token, you can do the following
  * Switch between your owned tokens by clicking `Tab`
  * Move tokens around using left mouse button (while panning tool is enabled)
  * Clicking on a token selects it
    * Pressing `Delete` or `Backspace` will delete the token 
    * Move the token using `Arrow keys`
  * Movement is blocked by walls (regular walls, doors, hidden doors, invisible walls)
  * As a Game master, you can move tokens through walls. It will notify when collision with a wall happens
  * While dragging, clicking right mouse button will add a waypoint. 
  
    Releasing left mouse button will move the token along its waypoints.
    
    If snap to grid is enabled. Token will be snapped to nearest cell. To prevent this, press `Alt` while left mouse button is released
  * Movement distace and path is shown when dragging.
  * Copy and paste tokens using `Ctrl + V` and `Ctrl + V`
  
    Distance is calculated the same way as cell based measurement tool (see [tools guide](#side-buttons))
  * Double clicking a token will open its configuration panel
  
    ![image](https://user-images.githubusercontent.com/70658690/214686628-7673f0ba-f872-4e47-a28e-fce981703a55.png)
    * `Display name`: Token's name
    * `Token type`: Character type can have vision
    
      Mount types can have vision and will grab other tokens on top of them when moved
      
      Item types can't have vision
    * `Token size`: Token's dimensions. Scales token's image to fit (does't stretch)
    * `Image`: Token's sprite
    
    ![image](https://user-images.githubusercontent.com/70658690/214688869-dc7e69ef-bcc6-4a11-b4be-34cd9f5dea34.png)
    * `Has vision?`: Does this token have vision. Does nothing when token type is set to `Item`
    * `Night vision`: Special light source which is visible only to you. Does nothing when token has no vision enabled
    * `Token highlighted?`: Small light source to show this token, even when it's in total darkness
    * `Light source`: Select tokens light source. There are few presets, which will automatically modify it's light according to the source
    
      By selecting `Flickering` or `Pulsing` you will be able to fully configure effect speed, strength, etc.
      
      Bigger light sources have decreased shadow resolution and precision
    * `Light color` Select light color. Selecting one of the light presets automatically updates this to match that preset settings
    
      There is currently a known bug, where you have to click this button at least twice to update the color picker's color
    
    ![image](https://user-images.githubusercontent.com/70658690/214690354-349a14ad-f42e-4e89-bff9-0e1d26532054.png)
    * Select possible conditions for this token. Conditions will be visible as small icons on top of the token
  
  * To save changes, click on `Save`
  * To discard changes, click on `Close`
### Lights
  * Move lights around using left mouse button. New position is updated to players' only after dragging has stopped
  * Copy and paste lights using `Ctrl + V` and `Ctrl + V`
  * Clicking on a light source opens its configuration panel
  
  ![image](https://user-images.githubusercontent.com/70658690/214691876-e2c51761-3ec6-4b88-a66c-6dccea93aead.png)
  * `Enabled` Enable / disable light source
  * `Light radius` configure light size. Bigger light sources have decreased shadow resolution and precision
  * `Light intensity` configure light intensity (how bright the emitted light is)
  * `Light source`: Select light source. There are few presets, which will automatically modify it's light according to the source
    
      By selecting `Flickering` or `Pulsing` you will be able to fully configure effect speed, strength, etc.
      
    * `Light color` Select light color. Selecting one of the light presets automatically updates this to match that preset settings
    
      There is currently a known bug, where you have to click this button at least twice to update the color picker's color
  * To save changes, click on `Save`
  * Discarding changes will be added in near future
  
### Notes

* Notes will be added in future updates

## Blueprints

* Create new blueprint from the right panel. This will open a configuration panel. (see [token configuration](#tokens))
* Add tokens to scene by dragging blueprints out from the side panel
* Right clicking a blueprint on the right panel allows you to delete, modify, or set permissions for it
* Modifying blueprints doesn't update any tokens already on the scene. 
* You can drag blueprints and folers from one folder to another
* Rename folders by double clicking them

### Permissions

* Open permission panel from right clicking a blueprint, and then selecting the `Permissions` option
* Configure permissions for each user you have invited to this game session
* Clicking on `Refresh` adds possible missing users, who have been invited after this blueprint was created

#### Owner

* This player has every right to move, select, edit and delete this token
* This option is useful for setting up player tokens

#### Observer

* This player will be able to see through the token's eyes, but have no permissions to move, or delete it
* This option is useful for general NPCs

#### None

* This player has no permissions to move, edit, delete or see through the tokens eyes
* This option is useful for enemies, and tokens, which players' shouldn't be able to interact with

## Keyboard shortcuts

* Here are some useful shortcuts that everyone should know
* More will be added in future updates

### Game View

Tokens
* `Tab`: Switch between tokens you own
* `Arrow keys`: Moves selected token on the map, one cell at a time
* `Alt`: Prevents token from snapping to grid when moved

Side panel
* `Num 1`: Panning tool
* `Num 2`: Measurement tools
* `Num 3`: Ping tool
* `Num 4`: Fog tools (Game master only)
* `Num 5`: Light tools (Game master only)
