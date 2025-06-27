# RPG Viewer Client

![Downloads](https://img.shields.io/github/downloads/ItharDev/RPG-Viewer-Client/total)

Welcome to the RPG Viewer Client! This application works hand-in-hand with the [RPG Viewer Server](https://github.com/ItharDev/RPG-Viewer-Server).

This guide will walk you through everything you need to know, from setup to running full-fledged digital adventures.

## Table of Contents

- [RPG Viewer Client](#rpg-viewer-client)
  - [Table of Contents](#table-of-contents)
  - [Installing the Application](#installing-the-application)
    - [Windows](#windows)
    - [Linux](#linux)
  - [Connecting to a Server](#connecting-to-a-server)
  - [Tuning Your Settings](#tuning-your-settings)
  - [Authentication \& Registration](#authentication--registration)
  - [Running Game Sessions](#running-game-sessions)
    - [Creating a Session](#creating-a-session)
    - [Connecting to an Existing Session](#connecting-to-an-existing-session)
    - [Joining a Session with a Key](#joining-a-session-with-a-key)
  - [Inside the Session](#inside-the-session)
    - [The Side Panel](#the-side-panel)
    - [The Tool Panel](#the-tool-panel)
    - [The GM's Configuration Panel](#the-gms-configuration-panel)
  - [Scenes](#scenes)
    - [Creating a Scene](#creating-a-scene)
    - [Activating a Scene](#activating-a-scene)
    - [Tuning the Grid](#tuning-the-grid)
      - [If You Know the Dimensions](#if-you-know-the-dimensions)
      - [If You Don't Know the Dimensions](#if-you-dont-know-the-dimensions)
    - [Lighting Up the Scene](#lighting-up-the-scene)
    - [Light Sources](#light-sources)
      - [Lighting Presets](#lighting-presets)
    - [Walls: Defining Spaces and Blocking Vision](#walls-defining-spaces-and-blocking-vision)
      - [Placing walls](#placing-walls)
      - [Types of Walls](#types-of-walls)
    - [Portals: Way of Linking Areas](#portals-way-of-linking-areas)
      - [Creating Portals](#creating-portals)
      - [Managing Portals](#managing-portals)
      - [Connecting Portals](#connecting-portals)
  - [Tokens \& Blueprints](#tokens--blueprints)
    - [Permissions \& Visibility](#permissions--visibility)
      - [Permission Levels](#permission-levels)
      - [Visibility](#visibility)
    - [Limiting Token Vision](#limiting-token-vision)
    - [Token Groups](#token-groups)
    - [Token Effects](#token-effects)
  - [Journals \& Notes](#journals--notes)
    - [Scene Notes](#scene-notes)
    - [Global Journals](#global-journals)
  - [Useful Keybinds](#useful-keybinds)
    - [General Keybinds](#general-keybinds)
    - [Grid \& Scene Configuration](#grid--scene-configuration)
    - [Wall Tools](#wall-tools)
    - [Measuring Tools](#measuring-tools)
    - [Ping Tools](#ping-tools)
    - [Notes](#notes)
    - [Token Management](#token-management)
    - [Token Groups](#token-groups-1)

## Installing the Application

Find the latest release on the right-hand panel. Installation is pretty straightforward:

### Windows

- Download and unzip the version for Windows.
- When prompted by Windows Defender about your mysterious new app, don't panic. If you got it from the official GitHub, you can ignore the warning.
- Accept private network permissions if prompted — otherwise your client might not work properly.

### Linux

- Linux builds are available from version `v2.7.0` onward.
- Download and unzip the version for Linux.
- Make the downloaded file executable:
  
    ```
    chmod +x RPG-Viewer.x86_64
    ```
- Without this, your terminal will say Permission denied.

## Connecting to a Server

To connect the Client to a Server, you must have a running instance of the [RPG Viewer Server](https://github.com/ItharDev/RPG-Viewer-Server) and know its IP address and port.

Open the connection field by clicking the **Settings** button in the top left corner of the application. Enter the server address in the format `<ip:port>`, for example:

```
127.0.0.1:4000
```

Replace `127.0.0.1` with the actual IP address of your server and `4000` with the correct port if different. The client will attempt to connect to the specified server for all further interactions.

## Tuning Your Settings

Inside the Settings panel, you'll find an FPS slider. 30 FPS is the sweet spot for most machines — unless you're roleplaying on a toaster, then maybe try 20.

Pressing the **Esc** key anywhere in the application opens a panel where you can manage the application resolution and toggle fullscreen mode.

## Authentication & Registration

Before you storm dungeons and conquer camps, you'll need an account. Head to the top-right corner and open the registration panel. Sign up once, and the client remembers you until you log out manually. 

> [!NOTE]
> Forgot your password? There's no reset button yet. Changing it involves diving into the database directly. Not for the faint of heart — or the technically inexperienced.

## Running Game Sessions

Game Sessions are the heart of the app. Here, you can manage Scenes, Tokens, lighting, and player actions.

### Creating a Session

Click the Sessions panel in the top left, give your Session a name, and select a Landing Page image using the folder icon. Creating a Session without choosing the Landing Page first is not allowed. You can change the Landing Page later inside the Session ([more on that further down](#side-panel)).

### Connecting to an Existing Session

Pick your Session from the dropdown, then click the connect button. You can copy the Session's licence key using the button on the far right — you'll need this to invite others (see below).

Your latest Session is featured front and center in the pre-Session view for quick access.

### Joining a Session with a Key

To join, you need that licence key from the Session's host. Paste it into the validation field between the dropdown and creation panel. If it matches, you're in.

## Inside the Session

When a Session opens, you'll see the Landing Page you picked earlier. Depending on your role (GM or player), you'll have access to different tools and panels.

### The Side Panel

![](https://github.com/user-attachments/assets/171a5797-9811-46b2-bb11-4e0d38b1ce81)


This is the big utility belt on the right. Buttons here do everything from changing Scenes to writing lore.

- **Change Landing Page (GM only)**: Set a new Landing Page.
- **Sync View  (GM only)**: Toggle this off to experiment privately. Toggle on to show players the current state.
- **Token Effects**: For managing visual effects. Details covered in the Token section.
- **Lighting Presets**: Manage reusable lighting setups.
- **Journal**: For Notes, logs, and handouts.
- **Scenes  (GM only)** Create, modify, or switch Scenes.
- **Blueprints** Token templates.

> [!TIP]
> Items can be grouped into directories to keep things tidy. To move an item, click it and choose the **Select** option. This marks the item as selected. Then, click on the target folder and choose **Move here** to place the item inside it. If you want to move the item back to the root directory, click the item again and select **To root**.

### The Tool Panel

When a Scene is open, this floating toolbox appears. Here's what each mode does:

- ![image](https://github.com/user-attachments/assets/8a4475ed-7545-4624-8f72-02db520b08d8) **Move**: Pan and zoom. Some tools disable movement, but you can always drag with the middle mouse.
- ![](https://github.com/user-attachments/assets/e752b0ec-2826-46dc-acb6-b4de4849ed75) **Measure**:
  - ![](https://github.com/user-attachments/assets/cce89eaa-0eb8-4ed7-bf97-db85e1a4240a) **Precice**: Straight-line distances for absolute positioning.
  - ![](https://github.com/user-attachments/assets/fd13e099-c3bd-479d-8733-19a1d8d1fcc4) **Snap to Grid**:  Snaps to grid cells, handy for grid-based movement.
  - While measuring, you can add waypoints by `Right Clicking` during the measurement.
  - All measuring actions require holding down `Left Shift` while clicking.
- ![](https://github.com/user-attachments/assets/343fee34-dea9-4121-8f15-3c0990b692e1) **Ping**:
  - ![](https://github.com/user-attachments/assets/02cf72a0-0a71-4eee-bca1-a691b286c989) **Mark**: Place a marker with `Left Shift + Click`. Hold to focus everyone's view.
  - ![](https://github.com/user-attachments/assets/bbe42143-abe3-4bcf-8a0e-424708099ad5) **Pointer**: Show a live pointer using `Left Shift + Click`. Follows your mouse and helps direct attention.
- ![](https://github.com/user-attachments/assets/204be568-bcb7-421b-a1be-4435a92eb98e) **Notes**:
  - ![](https://github.com/user-attachments/assets/241b2487-2600-4d01-9eed-0c58600ef3f6) **Create**: Hold `Left Shift` and click to drop a Note.
  - ![](https://github.com/user-attachments/assets/fc724226-8719-432e-a257-84f08096b705) **Delete**: Click Notes to remove them.

### The GM's Configuration Panel

For Game Masters only. When a Scene is open, this panel lets you reshape the Scene itself.

- ![image](https://github.com/user-attachments/assets/5cd3c5a9-7728-4d85-88f2-bcd1e7617310) **Change Image**: Swap the Scene background. Dimensions must match.
- ![image](https://github.com/user-attachments/assets/9a5c18fc-2d96-46e3-8874-7a14c98ef038) **Grid Settings**: Modify grid size, cell count, and colors. Read more about grid configuration [here](#tuning-the-grid).
- ![image](https://github.com/user-attachments/assets/433783d9-c9bd-42d4-af63-a7fab88cd739) **Wall Tools**: Open [wall creation tools](#walls-defining-spaces-and-blocking-vision).
- ![image](https://github.com/user-attachments/assets/8dda9d85-8639-4687-889a-cff2865e5165) **Lighting Tools**:  Edit [lights and fog](#lighting-up-the-Scene).
- ![image](https://github.com/user-attachments/assets/443ecbe7-0e5e-4f0d-9f70-0c421d744883) **Portal Tools**:  Add [teleportation](#portals-way-of-linking-areas) points.
- ![image](https://github.com/user-attachments/assets/844a458e-67b9-4f59-a542-f1c2a19d86ca) **Change View**: From this menu, you can change the current view (visible only to you). There are currently three options:
  - ![image](https://github.com/user-attachments/assets/58d002ce-9268-4964-ae41-fd20421b3750) **Player View**:  This simulates what the selected Token would see, including lighting and fog. It's the default player experience.
  - ![image](https://github.com/user-attachments/assets/0ed50a8f-e3a4-4a22-b95a-3f8ec026ad98) **Vision Only**: Shows the visible area based on each Token's vision range, but without applying any lighting effects. Useful for checking vision coverage.
  - ![image](https://github.com/user-attachments/assets/e9560fa7-07ef-4cf0-8c0a-99b76b6bfdc6) **No Fog**: Completely removes fog and lighting for the GM, allowing a clear view of the entire Scene. Ideal for setup and configuration.
- ![image](https://github.com/user-attachments/assets/d23e36ba-cd1e-4ad4-9382-eefa210bf290) **Token Groups**: Create and manage Token groups.<TODO>

## Scenes

Scenes are your adventuring environments. Each one has its own lighting, Tokens, and configuration.
### Creating a Scene

From the Side Panel, upload an image to start a new Scene. It will then be added to your Scene list.

### Activating a Scene

Click the Scene, then hit **Play**. You can also rename or delete from this menu.

> [!TIP] 
> Keep Sync View off if you don't want players seeing a half-finished dungeon.

### Tuning the Grid

A well-aligned grid is the core element of any grid-based system. It ensures that Tokens line up and measurements are accurate.

> [!IMPORTANT]
> After tweaking the grid, hit the **Save** button before switching Scenes — otherwise your changes might vanish.

There are many ways to setup the grid, but in my opinion the two best ways to achieve it are the following:

#### If You Know the Dimensions

1. Drag the **bottom-left grid corner** to the correct position. You can nudge it with the `Arrow Keys`; hold `Shift` while doing so for full-cell movement.
2. Open the configuration panel and enter the exact width and height of the grid cells.
3. Drag the **top-right corner** to stretch the grid and lock in the size.

#### If You Don't Know the Dimensions

1. Temporarily change the grid color to something that contrasts well with your map — bright pink is a classic.
2. Drag **one of the corners** to a **known feature** (like a doorway or floor tile) and eyeball the estimated cell size.
3. Stretch the **opposite corner** until the cells appear to align with map features.
4. Adjust the **bottom-left corner** of the grid for final alignment, and input dimensions into the configuration panel if needed.

### Lighting Up the Scene

Lighting is where the mood of your Scene is forged — from eerie crypts to sunlit forests, it's all about ambiance.

There are few fields to modify, each contributing to a different effect:

- **Enabled**: This switch turns the entire lighting and vision system on or off. If you're wondering why everything's **pitch black**, check this first.
- **Darkness Color**: This sets the color for areas your Tokens can't see. It doesn't affect lighting — it just fills in the unknown. Think of it as the color of the void.
- **Global Lighting Color**: Applies a global light tint across the entire Scene. This is perfect for simulating daylight, torchlight, moonlight, or the dreadful green glow of goblin caverns. **Common tip:** a soft blue hue makes a convincing nighttime tone.

> [!NOTE]
> Like any good mood lighting, it might take a few tries to get just right. Don't worry if it's not perfect on the first pass — lighting finesse often comes with experimentation.

### Light Sources

Light sources are attached to Tokens or specific parts of the Scene to dynamically illuminate the surroundings based on their settings.

To place a light source, open the [Configuration Panel](#the-gms-configuration-panel), select the **Create** Tool from the Lighting section, choose a point and press `Left Shift + Click` to assign a light. You'll be able to fine-tune how it behaves, from color and intensity to range and effect.

Each light has two light sources, allowing you to create a blend between the colors to create unique effects. Here are the main options for each light source:

- **Enabled**: Toggles the light on or off.
- **Direction**: Controls the direction in degrees where the light source points at. **Positive** values rotate the source **counter-clockwise** and **negative** rotates it **clockwise**.
- **Radius**: Defines how far the light reaches from its origin point.
- **Angle**: Defines the arc of angle for the light source. Use narrow angles for flashlights and larger for bonfires. 
- **Intensity**: Controls how strong the light appears. Higher values make the light appear brighter and more saturated.
- **Color**: The hue of the light. Use warm tones for cozy Scenes, or sickly greens and icy blues for a more unsettling feel.
- **Effect**: Adds a custom effect for the light source. This way you can create flickering torches or pulsating portals, drawing the attention of players.
- **Effect Strength**: Controls the effect strength. This is the **strength** for **flickering** effect and **pulsating amount** for **pulsing** effect.
- **Effect Frequency**: Controls how often the light flickers, or how fast the pulsating happens. For flickering, it's flickers per second, and for pulsing, it's duration of each pulse.


> [!TIP]
> Use multiple overlapping light sources with different ranges and colors to create more complex moods — like a smoky red glow near a forge.

Once you get the hang of it, lighting becomes a powerful storytelling tool. A single torch in an otherwise pitch-black dungeon can do more for tension than a paragraph of exposition.

#### Lighting Presets

> [!WARNING] 
> There is currently a known bug when creating or modifying a preset from light source panel. This causes the panel to open far left. You can still create and modify presets from the [Side Panel](#the-side-panel).

Lighting presets allow you to save and reuse specific lighting configurations across different Tokens or light sources. Instead of adjusting the color, intensity, and other fields manually each time, you can apply a preset to maintain consistency and speed things up.

To apply a preset, select one from the top right corner of the lighting configuration screen. This marks the current light source to use that preset. Any future changes made to the preset will automatically update all light sources using it.

> [!TIP]  
> You've created a "Dungeon Torch" preset with a warm orange color and applied it to every torch in your Scene. All looks good... until your players enters a magical field that causes the torches to glow blue.
> 
> You update the preset to reflect the new look — switching the hue to blue. Instantly, every linked torch updates across the Scene, no need to click each one individually.
> 
> Just remember: if you had tweaked a torch earlier to dim it slightly or change the color for "variety," it's no longer using the preset — and won't be affected by these changes. You'll need to reassign the preset to re-establish the link.

However, keep in mind:

- If you manually change any field of a light source that's currently using a preset, that link will be broken.
- Even if you revert the values to match the preset exactly, the connection is still considered broken.
- To reassign the preset, simply select it again from the dropdown.
  
This system helps maintain consistency across your Scenes while giving you the tools to tweak individual light sources as needed.

> [!WARNING]  
> All light presets are also available to players for **use**, **modification** and **deletion**.

### Walls: Defining Spaces and Blocking Vision

Walls are the unseen guardians of your Scene — defining rooms, blocking sightlines, and keeping overly curious players from wandering into the boss fight early.

#### Placing walls

Before you start clicking, make sure to choose the appropriate wall type from the wall tools. Each type has its own behavior (we'll cover those in the next section).

Once you've selected a wall type, here's how to start building your dungeon:

1. **Draw New Walls**: Hold `Left Shift` and drag your mouse to create a new wall segment.
2. **Extend Existing Walls**: If you want to continue from a previously placed endpoint, hold `Left Control` and drag from that point.
3. **Move Wall Points**: Drag any wall point with the mouse. They'll snap to nearby points automatically.
4. **Split Walls**: Click to select a point and press `Space`. This splits the wall at that point — handy for editing just one section.
5. **Right Click for Options**: Want to lock a door, change wall behavior, or inspect details? Right-click the point or segment.
6. **Delete Wall Points**: Select the point and hit `Delete` or `Backspace`.

> [!TIP]  
> To preview what these walls actually do, switch to Player View in the Configuration Panel and drag a Token with vision around the map. You'll instantly see how your walls affect visibility and movement.
> 
> Note that as a Game Master, you can freely move Tokens through walls. Players won't have this power in their hands, and moving a Token through a wall notifies you about it.

#### Types of Walls

Walls come in different flavors, and each one is designed to block (or not block) something. Here's the lineup:

- ![image](https://github.com/user-attachments/assets/3ed8589a-37a8-4fb1-b07b-9ad634faa2d3) **Regular Walls**: The default wall type. Blocks both movement and vision, like any stone wall should.
- ![image](https://github.com/user-attachments/assets/0dea34e8-5038-4fcf-a20d-e1569f1347b8) **Environmental Walls**: Acts like a regular wall, but also masks out everything inside the boundary. Don't go overboard. Too many of these can seriously impact performance.
- ![image](https://github.com/user-attachments/assets/b9b10e05-af2c-4bf2-9897-b53cc111f11a) **Invisible Walls**: These block movement but let vision and light pass through. Perfect for physical barriers that characters can see through, like windows or fences.
- ![image](https://github.com/user-attachments/assets/dfd684fe-ea46-4019-a3b0-04f80dba4217) **Regular Doors**: Functionally a wall, but with an on-screen icon that players can click to open or close the passage. `Right Click` the icon (or one of its wall points) to lock the door and deny entry.
- ![image](https://github.com/user-attachments/assets/c2e42544-7391-4c3f-9b46-16eee606185b) **Secret Doors**: Like Regular Doors, but sneakier. These don't show an icon to players. Only the GM knows it's there — ideal for ambushes, escape routes, and secret lairs.
- ![image](https://github.com/user-attachments/assets/574665c7-ec4a-4a66-9568-3522310a252c) **Curtains**: These block light but not movement. Useful for illusions or magical veils.
- ![image](https://github.com/user-attachments/assets/dd689624-b6e8-40c3-a60d-fe6b453b9069) **Darkness**:  These blocks both movement and vision, and the global light from entering the enclosed area. Once the loop is closed, the area within becomes shadowed from everything, including global light.
  
> [!NOTE] 
> Darkness Walls automatically seal off the enclosed shape by connecting the last segment back to the first. This makes using them with openings like doors or windows tricky — so use sparingly.

### Portals: Way of Linking Areas

Portals are your go-to method for linking separate parts of the map — upstairs and downstairs, or maybe different wings of a castle.

#### Creating Portals

To create a Portal, select the Portal creation view from the [Configuration Panel](#the-gms-configuration-panel), then hold `Left Shift` and click where you want the Portal to appear. You'll see a detection area pop up.

There are two modes of Portal operation:

- **Proximity**: This mode keeps its ears open at all times. As soon as an active Token enters thePportal's radius, the teleportation triggers instantly. Ideal for seamless transitions between different segments.
- **Stationary**: This mode is a bit more refined. It only activates when a Token finishes its movement inside the Portal's detection radius.

Portals can be used creatively to manage complex Scenes. They're perfect for connecting floors, teleporting between distant areas, or hiding shortcuts your players won't notice.

#### Managing Portals

Once placed, Portals aren't set in stone. You can:

- **Move** a Portal by dragging it.
- **Toggle** a Portal on or off by `Left Clicking` it.
- **Configure** a Portal by `Right Clicking` it to access its settings.
- **Delete** a Portal by selecting the Delete option in the Configuration Panel, then clicking the Portal you want to delete.

> [!TIP]  
> Disabled Portals don't show up for players and don't trigger teleports. Use this to control when shortcuts are revealed.

#### Connecting Portals

A single Portal does nothing on its own — like a bridge with no other side. To make them functional, you must connect two Portals together.

Here's how:

1. Open the Configuration Panel and select the Linking Tool.
2. Click on the source portal (where the journey begins).
3. Then click the destination portal (where it ends).

A directional arrow will appear to indicate the connection.

Each portal can:
- Serve as the **source** of one connection (**must be enabled**).
- Serve as the **destination** for multiple connections (**even when disabled**).

Removing a connected portal will automatically sever any links associated with it.

> [!NOTE]  
> Linking is one-directional. If you want two-way travel, you'll need to connect both portals to each other manually.

## Tokens & Blueprints

Tokens represent characters, creatures, or objects that exist within your Scene — but unlike some systems, you can't just spawn Tokens from nothing. First, you need to create a **Blueprint**, which acts as the template. Once that's done, you can drag the Blueprint into a Scene to instantiate a **Token**.

To create a Blueprint:

1. Open the Blueprints Panel from the Side Panel.
2. Create a new Blueprint and configure it (name, image, vision, lighting, etc.).
3. Once saved, it appears in your list of Blueprints and can be dragged into any Scene.

Tokens placed into the Scene are then fully configurable on their own. But here's the twist: Blueprints can optionally be **marked as Synced** from their context menu — look for the little globe icon on the right side of the Blueprint in the Side Panel.

If you drag a **Synced Blueprint** into a Scene, that Token maintains a live connection with the original. Update the Blueprint? All Tokens using it will update too. Adjust a Token that originated from it? The source Blueprint (and its other synced Tokens) update along with it.

> [!IMPORTANT]  
> Each Blueprint can only sync to Tokens that were created from it *after* it was marked as synced. Existing Tokens won't retroactively sync.

Blueprints can also be created inside the **Public Folder**, a shared space designed for your players. Blueprints in this folder can be freely created, edited, and dragged into Scenes by players. It's a great way to let them manage their own Tokens, companions, or even bring in utility objects like illusionary spell markers, measuring tools, or custom light sources.

> [!TIP]  
> Use the Public Folder for anything players should have access to — it keeps the GM folder tidy and gives players just enough rope to be useful (or to hang themselves with, depending on your game).

> [!CAUTION]  
> If you're not using Synced Blueprints, editing a placed Token won't update the Blueprint it came from — and modifying a Blueprint won't affect existing Tokens already in a Scene.

### Permissions & Visibility

Every Token and Blueprint in your game can be finely tuned for visibility and player control, giving you complete command over what your players see — and what they can do.

> [!NOTE]
> You might need to refresh the visibility and permission panels when opening them to refresh default permissions. If your visibility or permission panels looks empty, this might be the issue.

#### Permission Levels

Each player's interaction with a Token is defined by its Permission Level, which comes in three tiers:

- **None (Default)**: The player has no control or visibility over the Token beyond what the game passively reveals (e.g., lighting, fog). They cannot move, select, or interact with the Token.
- **Observer**: The player can view the world through the Token's vision (great for NPC allies or familiars), but cannot move or edit it in any way.
- **Controller**: The player has full control over the Token. They can move it, edit its properties, delete it, and use it as their own character.

> [!NOTE] 
> The Game Master is considered a **Controller** for **all** Tokens by default, regardless of individual permission settings.

#### Visibility

Visibility settings control who can see the Token at all — regardless of lighting or line-of-sight. This allows for more immersive scenarios:

- Hide an invisible creature from all but one player.
- Let one character see hidden allies, thanks to magical effects.
- Surprise players with hidden enemies that only appear when triggered.

Visibility works per player and can be configured on both Tokens and Blueprints. Tokens retain their visibility settings when placed in a Scene.

> [!TIP]
> A player has the ability to see invisible creatures. You configure a hidden creature to be invisible to everyone except that player. The creature appears only to them — and they now face the moral dilemma of shouting a warning or staying quiet.

These controls help ensure that only the right eyes see what they're supposed to, giving your game that layer of mystery and surprise. It also opens the door to stealth mechanics, hidden allies, and all the chaos that follows.

### Limiting Token Vision

Limiting Token vision is crucial for creating realism in your game. By restricting what each Token can see, you ensure that players only have information their characters would reasonably know.

To limit Token vision:

- **Assign Vision Ranges:** Each Token can be given a specific vision range from its configuration panel. This determines how far the Token can see in any direction.
  
> [!TIP]  
> You can set the global vision range from the [lighting configuration panel](#lighting-up-the-Scene).

- **Configure Vision Types:** Choose between different vision types, such as normal, darkvision, or blindsight, depending on the character's abilities. These options are available in the Token's settings.
- **Use Walls:** Walls will automatically block vision, preventing Tokens from seeing through them.
- **Adjust Lighting:** The amount of light in a Scene affects how much a Token can see. Tokens without darkvision will be limited by darkness, while those with darkvision can see further in low-light areas.

> [!TIP] 
> You can test Token vision by switching to the **Player View** in the Configuration Panel and clicking on a Token. This shows exactly what that Token can see, helping you fine-tune vision settings for your game.

### Token Groups

Token Groups are your secret weapon for timed ambushes, synchronized strikes, or just unleashing chaos with a single click. They let you group Tokens together and control them as a unit — making it easy to reveal, or hide multiple creatures or objects at once.

To group Tokens:

1. Select the Tokens you want to group by clicking on them. Hold `Left Control` to multi-select.
2. Press `Left Control` + `1`, `2`, or `3` to assign the selected Tokens to Group **1**, **2**, or **3** respectively.

Once grouped, you can manage the Token groups from the [Configuration Panel](#the-gms-configuration-panel):
- Enable / Disable Groups: Toggle visibility and interaction for an entire group in one go.
- Clear Group: Remove all Tokens from a group without affecting the Tokens themselves.

Tokens can belong to multiple groups at once, allowing for overlapping control schemes. To remove a Token from a group, simply select it and press `Left Control + [group number]` again — the same key combo unassigns it.

Token Groups are perfect for preparing surprises, managing encounters, or just staying organized in a dungeon crawl.

### Token Effects

Token Effects are a powerful way to visually enhance your Scenes by attaching dynamic overlays **above or below** Tokens. Whether your caster is walking around with a swirling aura of fire, or your summoned spirit exudes radiant energy, effects help communicate what's happening.

To add a Token Effect, Select a Token, and assign an effect. Each effect can be:
- Positioned **above or below** the Token.
- Assigned an image — use a visual representation like fire, smoke, runes, or magical energy.
- Animated with effects such as pulse, rotation, or both — perfect for auras, enchantments, or timers.

Effects behave similarly to Lighting Presets — you can configure them once and apply them consistently across multiple Tokens.

> [!TIP]
> Use Token Effects to represent:
> 
> - Ongoing spell effects (like Spirit Guardians or Darkness)
> - AoE zones that follow the Token
> - Utility indicators (radius indicators, stealth mode, etc.)
> - Light sources (glowing orbs, torches, magical lanterns)

## Journals & Notes

Journals and Notes are your tools for storing, sharing, and organizing written information — whether it's GM secrets, player lore, or in-game puzzles.

While similar, they serve different purposes:

- Notes are Scene-specific. They're placed directly onto the Scene and tied to that location. Perfect for handouts, environmental storytelling, or leaving clues in a dungeon.
- Journals are global, accessible from the Journal Panel at any time, regardless of which Scene is active. They're ideal for campaign logs, player backstories, rules references, or worldbuilding material.

Each Note and Journal entry features two sections: **Text** and **Image**, accessible via the toggle in the bottom right corner. If you've selected an image for the entry, that section will be shown by default when opening the Note or Journal — making this a powerful way to share visual content with your players.

> [!TIP]  
> Want to display a clue, handout, or spooky sketch? Just create a Journal, add your image, and hit the upward arrow icon to push it to your players. Each player can also save a local copy of that image to their Journal Panel for future reference.

> [!TIP]
> The text editor supports some Markdown features, like headings, text decorations and lists.

### Scene Notes

To place a Note:

1. Open the [Tool Panel](#the-tool-panel).
2. Select the Note Tool.
3. Click where you want to place the Note in the Scene while holding `Left Shift`.

By default, Notes are visible only to you. You can change this by clicking the Context Menu (the small icon to the left of the close button on the Note). This menu allows you to:

- Modify the visibility.
- Send the Note to everyone, instantly popping it open on their screens (via the upward arrow icon).
- Save a copy to your Journal Panel, creating a static snapshot of the Note's current state.

> [!NOTE] 
> Saving a local copy creates a separate Journal entry — it won't auto-update if the original Note changes.

### Global Journals

Journals are created and edited from the Journal Panel, which houses all your global entries. They're not tied to any Scene, making them perfect for persistent reference material.

Use Journals to:

- Track party progress.
- Store lore handouts.
- Keep a running GM prep document.
- Share Session summaries with players

> [!TIP]  
> You can turn any Note into a Journal Entry using the save icon in the Note's context menu — great for preserving player-discovered information across Scenes.

You can also share a Journal Entry with another player or Game Master by clicking the Share button on the Journal context menu on the Side Panel. This allows you to create shared, Session-wide Journals where everyone can contribute or refer to shared information.

> [!NOTE] 
> When sharing a Journal, if some player doesn't show up right away, click the refresh button to the left of the close button on the panel to update the list.

## Useful Keybinds

There are many useful keybinds, which are not listed anywhere in the application. Here's a list of all the keybinds inside the Session:

### General Keybinds

| Keybind                  | Use Case / Description                                                                           |
| ------------------------ | ------------------------------------------------------------------------------------------------ |
| **Esc**                  | Opens the settings panel anywhere in the application to manage resolution and toggle fullscreen. |
| **Middle Button + Drag** | Force move the camera when Left Click Dragging isn't available.                                  |

### Grid & Scene Configuration

| Keybind                | Use Case / Description                                      |
| ---------------------- | ----------------------------------------------------------- |
| **Arrow Keys**         | Nudge the grid corner for precise alignment.                |
| **Shift + Arrow Keys** | Move the grid corner by a full cell for faster adjustments. |

### Wall Tools

| Keybind                     | Use Case / Description                                                           |
| --------------------------- | -------------------------------------------------------------------------------- |
| **Left Shift + Drag**       | Draw a new wall segment.                                                         |
| **Left Control + Drag**     | Extend an existing wall from a selected endpoint.                                |
| **Space**                   | Split a wall at the selected point.                                              |
| **Delete** or **Backspace** | Delete a selected wall point.                                                    |
| **Right Click**             | Open options for a wall point or segment (lock, change behavior, inspect, etc.). |

### Measuring Tools

| Keybind                           | Use Case / Description                                              |
| --------------------------------- | ------------------------------------------------------------------- |
| **Left Shift + Click**            | Start measuring (all measuring actions require holding Left Shift). |
| **Right Click (while measuring)** | Add waypoints during measurement.                                   |

### Ping Tools

| Keybind                     | Use Case / Description                                                   |
| --------------------------- | ------------------------------------------------------------------------ |
| **Left Shift + Click**      | Place a marker or show a live pointer (depending on selected ping tool). |
| **Hold Left Shift + Click** | Focus everyone's view on the marker.                                     |

### Notes

| Keybind                | Use Case / Description               |
| ---------------------- | ------------------------------------ |
| **Left Shift + Click** | Drop a Note at the clicked location. |

### Token Management

| Keybind                     | Use Case / Description                                                                |
| --------------------------- | ------------------------------------------------------------------------------------- |
| **Arrow Keys**              | Move a Token one cell at a time.                                                      |
| **Left Alt + Drag**         | Drag Token without the grid snapping.                                                 |
| **Left Control + Drag**     | Drag Token with pathfinding enabled, findign the best route around walls and corners. |
| **Left Shift + Drop**       | Teleport the Token to the cursor position. Useful for cross-map transfers.            |
| **Space**                   | Quickly toggle the Token on and off.                                                  |
| **X**                       | Quickly add the **Dead** condition for the Token.                                     |
| **Delete** or **Backspace** | Quickly delete the Token.                                                             |
| **Q** and **E**             | Rotate the light source arount the Token.                                             |
| **A** and **D**             | Rotate the Token.                                                                     |

### Token Groups

| Keybind                           | Use Case / Description                           |
| --------------------------------- | ------------------------------------------------ |
| **Left Control + Click**          | Multi-select Tokens for grouping.                |
| **Left Control + 1/2/3**          | Assign selected Tokens to Group 1, 2, or 3.      |
| **Left Control + [group number]** | Remove a Token from a group (toggle assignment). |
