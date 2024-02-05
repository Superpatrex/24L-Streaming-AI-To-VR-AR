The Rewired_GamePad_U201910f2 package contains a prefab with a pre-configured setup for gamepads (e.g. Xbox One, Sony Dual-shock 4 etc.).

NOTE: THIS DOES NOT WORK IF YOU DON'T OWN REWIRED*.

1. Import the entire asset (e.g. Sci-Fi Ship Controller) into a project
2. Import Rewired 1.1.25.2 or newer into the project (install it when prompted)
3. From the Unity Editor double-click** on the Rewired_GamePad_U201910f2 package within this folder
4. Drag the SCSM\Test Assets\Prefabs\Rewired Input Manager into the scene
5. In the scene, right-click on Rewired Input Manager, and select "Unpack prefab completely"
6. Add a ship to the scene and configure the Player Input Module to use an Input Mode of "Rewired"
7. In the Player Input Module, assign the Input Axis and Primary/Secondary Fire buttons
8. In the Player Input Module, don't forget to set the Player Number to a non-zero value (typically 1).

* Rewired is an excellent 3rd party asset available from the Unity Asset Store which supports many different kinds of controllers.

** If double-click does not work, from the Unity Editor menu, Assets/Import Package/Custom Package... navigate to the folder within your project where the package is located to import the package.