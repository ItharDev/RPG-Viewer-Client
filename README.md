# RPG Viewer VTT

Official cliend-side application for RPG Viewer VTT

By reading this guide, you will understand everything you need to know about this application and its features

![Downloads](https://img.shields.io/github/downloads/ItharDev/RPG-Viewer-Client/total)

## Disclaimer

This guide is highly outdated, and will be updated in the next major update.

## Table of contents

<!--ts-->
* [Installation](#installation)
* [User guide](#user-guide)
  * [Connecting](#connecting)
  * [Accounts](#accounts)
  * [Sessions](#sessions)
  * [Game view](#game-view)
    * [Side buttons](#side-buttons)
    * [Side panel](#side-panel)
  * [Journal pages](#journal-pages)
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

# Installation

You can find the latest release on the right hand panel

![GitHub release (latest by date)](https://img.shields.io/github/v/release/ItharDev/RPG-Viewer-Client)

1. Download `RPG-Viewer.zip`
2. Extract all of the zip file's contents
3. Run the applicaton by starting `RPG Viewer.exe`
   
   Windows may ask for permission to operate on private networks. Please accept this to allow the application to work normally
4. See [user guide](#user-guide) below 

# User guide

## Connecting

![image](https://user-images.githubusercontent.com/70658690/214797669-9742276a-cf76-4076-a262-48b130c24b5a.png)

* When the application is started, an IP Address must be specified to which the application will try to connect
* This can be done from the top right corner of the main menu
* Syntax: `<IP Address>:<Port>` For example: `127.0.0.1:3000` will connect to localhost, on port 3000

*  Once connected for the first time, the application will automatically connect to this IP address when opened.
* * The IP address can be changed at any time by entering a new address in the top right hand corner of the main menu using the syntax above.

## Accounts

![image](https://user-images.githubusercontent.com/70658690/214797780-dfde14bd-7dbe-4f9c-b9e8-bc377efab93d.png)

Account data is synchronised across all devices, including
* Sessions
* Blueprints
* Scenes
* Notes

## Sessions

![image](https://user-images.githubusercontent.com/70658690/214797913-3c4b2ded-23a3-40da-917d-9cd1634a69ee.png)

* There is no limit to the number of sessions you can create.
* Landing page will be visible if there are no Scenes loaded or the session is not synced with players (more on this [later](#side-buttons)).
* Click the button next to the Join panel drop down menu to copy the session ID to your clipboard.
* Invite someone to this session by giving them this ID
* Accept invitations by pasting a key you have received into the right hand panel
* You can remove your access to any session you have been invited to by clicking the 'Remove Licences' button.

## Game view

![image](https://user-images.githubusercontent.com/70658690/214657068-9f40dfe2-5517-4722-a815-629aa27b3fd2.png)

* When you first join, you should see your landing page as the background.
* As a player, you won't be able to see the side panel on the right. It is only visible to Game Masters.

### Side buttons
  
  All buttons are only visible when a Scene is active.
  
  * ![image](https://user-images.githubusercontent.com/70658690/214668643-62a6a8a0-b93e-42c8-86e3-ec4aa5bfb505.png) 
    Pan Tool
    * Quick select this Tool by pressing `1` on your keyboard 
    * Left-click to move the camera
  * ![image](https://user-images.githubusercontent.com/70658690/214668692-54e15ca9-6ead-4a3c-a8b8-70481f1c7e29.png)
    Measure Tool
    * Quick select this Tool by pressing `3` on your keyboard
    * ![image](https://user-images.githubusercontent.com/70658690/214667672-31709621-4d3c-45b0-9f8a-29c6b1ae0eb5.png)
      Precise. Measures every foot
    * ![image](https://user-images.githubusercontent.com/70658690/214667880-22d375dc-1d22-4eb9-8caa-bf18dfff73a0.png)
      Cell-based. Snaps measurement points to cells in a grid. Distances are calculated using D&D 5e's optional movement.
      Each cell counts as 5 feet, and every other diagonal counts as 10 feet.
    * Right-click while measuring to add waypoints
    * Quick select this Tool by pressing `2` on your keyboard
  * ![image](https://user-images.githubusercontent.com/70658690/214668832-1b29f490-d61a-4cbc-854d-37bffc5473e0.png)
    Ping Tool
    * The markers remain on the map for 10 seconds
    * Left-click and hold to move each players' camera to that location
  * ![image](https://user-images.githubusercontent.com/70658690/215039279-0a064a12-49ce-4842-855a-cdb7544d29e1.png)
    Note Tools
    * Quick select this Tool by pressing `4` on your keyboard
    * Add Notes by clicking left-clicking
    * See [Notes](#notes)

  * ![image](https://user-images.githubusercontent.com/70658690/214669095-5fcb56cc-6761-445a-b79b-724a66db672e.png)
    Fog Tool (Game Master only)
    * Quick select this Tool by pressing `5` on your keyboard
    * ![image](https://user-images.githubusercontent.com/70658690/214665922-02bbf0fb-7a02-4ef5-996c-f86ea6ebdf0a.png)
      Player view. Clicking on a Token with this enabled will simulate that player's view of the game.
    * ![image](https://user-images.githubusercontent.com/70658690/214666376-61d7809b-5965-4a11-8620-f1ab6350c443.png)
       Vision only. Clicking a Token with this enabled will show what that player can see. Everything else is shown opaque
    * ![image](https://user-images.githubusercontent.com/70658690/214666211-3869adb5-cef2-45bf-b5fa-4a1bc8ab94b4.png)
      No Fog. This will remove all Fog and allow you to see the entire map and Tokens.
  * ![image](https://user-images.githubusercontent.com/70658690/214669164-d0685566-3deb-4e2f-8311-75b8eeb3f662.png)
    Light Tool (Game Master only)
    * Quick select this Tool by pressing `6` on your keyboard
    * Left-clicking on a map will add a light source on that location
    * More on lights can be found [here](#lights)
  * ![image](https://user-images.githubusercontent.com/70658690/214669185-852c161f-72f9-4e4c-92fa-4785320d9334.png)
    Sync (Game master only)
    * Sync session to players
    * While locked, players will only see the landing page.
    * While syncing, players will see the active Scene if there is one.
  * ![image](https://user-images.githubusercontent.com/70658690/214670372-ded947c8-111f-4d22-b477-d798084983f8.png)
    Centre camera
    * Move camera back to centre and reset zoom
  * ![image](https://user-images.githubusercontent.com/70658690/214670519-3ac5c9d6-7dae-497e-bd53-13da8f378945.png)
    Leave session
    * Leave session and return to main menu
    * Clicking this as a Game Master will set the sync state to locked, so that players will only see the landing page.

### Side panel
* Create and modify Scenes and Blueprints (Game Master only)
* Create and modify Journal pages. More information [here](#journal-pages)
* Scene configuration can be seen [here](#scenes)
* Creating and managing Blueprints can be seen [here](#blueprints)

## Journal pages

* Create a new Journal pages from the right panel.
* Right-clicking on a Journal page in the right panel allows you to delete, modify, or share it with others
* Journal pages shared with you are located in the `Shared` folder
* You can drag Journal pages and folders from one folder to another
* Rename folders by double clicking them

## Scenes

* Create and modify Scenes by opening the tab on the right.
* Scene configuration can be seen [here](#creating-and-modifying-scenes)
* While modifying or creating a Scene, players will not be able to affect the current Scene and will only see the landing page.

### Creating and modifying scenes

* ![image](https://user-images.githubusercontent.com/70658690/214675341-8c7a686f-066c-4b0a-9f14-f18b092f62b9.png)
  Wall Tools
  * Restrict players' vision and lighting. Create toggleable doors that can be hidden from players.
  * ![image](https://user-images.githubusercontent.com/70658690/214675899-47515036-173d-4898-96a2-0c3f6f4ff50a.png)
    Regular walls. Block player movement, light and vision
  * ![image](https://user-images.githubusercontent.com/70658690/214676076-45314635-07c2-4cd8-bc87-5ac83f88a17e.png)
    Doors. Works like normal walls. Can be turned on and off (door icons visible to players)
  * ![image](https://user-images.githubusercontent.com/70658690/214676358-0033633d-dc2f-4047-b1f4-08cd886ff615.png)
    Hidden doors. Works like a normal door, but the door-icons are not visible to players
  * ![image](https://user-images.githubusercontent.com/70658690/214676578-7f57c534-bdde-42c7-97a4-b97ebd2c9a4e.png)
    Invisible walls. Blocks player movement, but not light or vision
  * Click and drag to create walls
  * Hold `Ctrl` and drag from wall endpoint to continue wall
  * Click on a wall endpoint to select it. Press `Backspace` or `Delete` to delete that wall point.
  * Snap wall endpoints together by dragging one endpoint over another
* ![image](https://user-images.githubusercontent.com/70658690/214675430-c027dc43-bb5f-448b-a735-1153f5692736.png)
  Grid configuration
    * Configure the grid for this Scene. Activate grid snap and measure Tools
    * ![image](https://user-images.githubusercontent.com/70658690/214678419-9204d23c-5d3d-46a0-ad91-b1024b82a583.png)
      Configure grid settings
      
      ![image](https://user-images.githubusercontent.com/70658690/214679711-45a6f21d-d819-4b42-aa9f-07338ee8c5bc.png)
      
      * `Enable grid`: Enables / disables the grid for this Scene
      * `Snap to grid`: Snaps Tokens to grid cells. Does nothing if grid is off.
      * `Grid dimensions`: Grid size. Default size when creating the Scene is `10x10`.
      * `Grid colour`: Opens a colour picker where you can choose the colour for your grid.
      
        There is currently a known bug where you have to click this button at least twice to update the colour of the colour picker.
      * `Grid opacity` Opacity of the grid. Set to `0` to hide the grid
      * To save changes, click `Save`
      * To discard changes, click `Close`
    * ![image](https://user-images.githubusercontent.com/70658690/214679223-86133691-198f-48aa-ad0f-6190263023ce.png)
      Enable & disable grid visibility. Applies to Modification View only
    * Scale grid by dragging one of its corners. Move grid around using `Arrow keys` or `W A S D`
* ![image](https://user-images.githubusercontent.com/70658690/214675490-301ee42f-5a21-480c-bf64-7c95b8a17ff7.png)
  Fog configuration
  * Restrict players' view. Add dynamic lighting and shadows to the Scene
  
  ![image](https://user-images.githubusercontent.com/70658690/214681958-d89da85f-4b01-494e-b42c-af74ff1b5025.png)
  
  * `Enable Fog of War`: Enables / disables Fog of War for this Scene.
  * `Global lighting`: Light up the entire Scene. Lights have no effect when this is enabled.
  * `Translucency`: Change global lighting translucency.
  * `Fog colour`: Opens a colour picker where you can choose the colour for your Fog.
      
      There is currently a known bug where you have to click this button at least twice to update the colour in the colour picker.
  * To save changes, click the `Save` button.
  * To discard changes, click `Close`
* ![image](https://user-images.githubusercontent.com/70658690/214675517-3c7d468f-e4d9-4d97-bd9b-8f692b15110e.png)
  Night Filter
    * Apply the Night filter to this Scene to create the illusion of night lighting.

  ![image](https://user-images.githubusercontent.com/70658690/214682559-c3151e92-1a7c-4150-af83-7e16d75bae12.png)
  
* `Night effect strength` controls how much of the night filter is applied. 0 is no effect and 100 is the maximum effect.
* Rename Scene from the top left corner
* ![image](https://user-images.githubusercontent.com/70658690/214683961-78a3a849-83c5-4122-9d99-76a5a3bfebf5.png)
  Save and exit 
  
### Tokens

While you are the owner of the Token, you can do the following
  * Switch between the Tokens you own by clicking `Tab`.
  * Move Tokens using the left mouse button (with the Pan Tool enabled)
  * Click on a Token to select it
    * Press `Delete` or `Backspace` to delete the Token 
    * Move the Token with the arrow keys
  * Movement is blocked by walls (normal walls, doors, hidden doors, invisible walls)
  * As the Game Master, you can move the Token through walls. You will be notified if you collide with a wall.
  * When dragging, right-click to add a waypoint. 
  
    Releasing the left mouse button will move the Token along its waypoints.
    
    If snap to grid is enabled. The Token will snap to the nearest cell. To prevent this, press `Alt` while releasing the left mouse button.
  * Move distance and path is displayed when dragging.
  * Copy and paste Tokens using `Ctrl + V` and `Ctrl + V`.
  
    Distance is calculated in the same way as the cell based measure Tool (see [Tools guide](#side buttons))
  * Double-click on a Token to open its configuration panel
  
    ![image](https://user-images.githubusercontent.com/70658690/214686628-7673f0ba-f872-4e47-a28e-fce981703a55.png)
    * `Display name`: The name of the Token
    * `Token type`: Character type can have vision
    
      Mount types can have vision and will grab other Tokens on top of them when they move.
      
      Item types cannot have vision
    * `Token size`: The dimensions of the Token. Scales the Token's image to fit (does not stretch).
    * `Image`: Token's sprite
    
    ![image](https://user-images.githubusercontent.com/70658690/214688869-dc7e69ef-bcc6-4a11-b4be-34cd9f5dea34.png)
    * `Has vision`: Does this Token have vision. Does nothing if the Token type is set to `Item`.
    * `Night vision`: A special light source that only you can see. Does nothing if the Token does not have vision enabled
    * `Token highlighted`: Small light source to show this Token even if it's in total darkness.
    * `Light Source`: Select the light source of the Token. There are a few presets that will automatically change the light depending on the source.
    
      By selecting `Flickering` or `Pulsing` you can fully configure the effect speed, strength etc.
      
      Larger light sources have reduced shadow resolution and precision.
    * Select the light colour. Selecting one of the light presets will automatically update this to match that preset's settings.
    
      There is currently a known bug where you need to click this button at least twice to update the colour of the colour picker.
    
      By selecting `Flickering` or `Pulsing` you will be able to fully configure effect speed, strength, etc.
      
      Bigger light sources have decreased shadow resolution and precision
    * `Light color` Select light color. Selecting one of the light presets automatically updates this to match that preset settings
    
      There is currently a known bug, where you have to click this button at least twice to update the color picker's color
    
    ![image](https://user-images.githubusercontent.com/70658690/214690354-349a14ad-f42e-4e89-bff9-0e1d26532054.png)
    * Select possible conditions for this Token. Conditions will be visible as small icons at the top of the Token
  
  * To save changes, click `Save`
  * To discard changes, click `Close`
  
### Lights
  * Move lights with the left mouse button. New position will not be updated for players until you stop dragging.
  * Copy and paste lights with `Ctrl + V` and `Ctrl + V`.
  * Click on a light to open its configuration panel
  
  ![image](https://user-images.githubusercontent.com/70658690/214691876-e2c51761-3ec6-4b88-a66c-6dccea93aead.png)
  * `Enabled`: to enable/disable a light source
  * `Light Radius`: Configure the size of the light. Larger light sources have reduced shadow resolution and precision.
  * `Light Intensity`: configure light intensity (how bright the emitted light is)
  * `Light source`: Select light source. There are a few presets that will automatically change the light depending on the source.
    
      By selecting `Flickering` or `Pulsing` you can fully configure the effect speed, strength etc.
      
    * Select the light colour. Selecting one of the light presets will automatically update this to match that preset's settings.
    
      There is currently a known bug where you need to click this button at least twice to update the colour of the colour picker.
  * To save changes, click `Save`.
  * Discard changes will be added in the near future.
  
  
### Notes

* Create Notes by selecting the `Notes` Tool and clicking on a Scene
* By default, Notes are set to be visible only for you
* Double clicking on a Note icon will open it
* Modify Note text by pressin `Ctrl + M`, and save edits by pressin `Ctrl + S`
* Move Notes around by dragging, and minimise by double clicking. Both actions can be done from the top left corner
* Scale Notes from bottom right corner

## Blueprints

* Create a new Blueprint from the right panel. This will open a configuration panel. (see [Token configuration](#tokens))
* Add Tokens to the Scene by dragging Blueprints out from the side panel
* Right-clicking on a Blueprint in the right panel allows you to delete, modify, or set permissions for it
* Modifying Blueprints doesn't update any Tokens already on the Scene. 
* You can drag Blueprints and folders from one folder to another
* Rename folders by double clicking them

### Permissions

* Open Permission panel by right-clicking on a Blueprint, and selecting `Permissions`
* Configure permissions for each user you have invited to this game session
* Clicking on `Refresh` will add any missing users, who have been invited after this Blueprint was created

#### Owner

* This player can see through the eyes of the Token, but cannot move or delete it.
* This option is useful for general NPCs.

#### Observer

* This player can see through the Token's eyes, but have no permissions to move, or delete it
* This option is useful for general NPCs

#### None

* This player does not have permissions to move, edit, delete or see through the Token's eyes.
* This option is useful for enemies and Tokens that you don't want the player to interact with.

# Keyboard shortcuts

* Here are some useful keayboard shortcuts that everyone should know
* More will be added in future updates

## Game View

### Tokens

* `Tab`: Switch between Tokens you own
* `Arrow keys`: Moves selected Token on the map, one cell at a time
* `Alt`: Prevents Token from snapping to grid when moved

### Side panel

* `Num 1`: Pan Tool
* `Num 2`: Measure Tools
* `Num 3`: Ping Tool
* `Num 4`: Note Tool
* `Num 5`: Fog Tools (Game Master only)
* `Num 6`: Light Tools (Game Master only)
