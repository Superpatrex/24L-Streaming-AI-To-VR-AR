If you have the (new) Unity Input System (UIS) installed in your project, follow these instructions to reconfigure the Ship Input Module to work with UIS.

1. Open the SCSM\SciFiShipController\Demos\TechDemo\scenes techdemo2scene1
2. In the scene Hierarchy panel, select "EventSystem" gameobject and click "Replace with InputSystemUIInputModule" on the "Standalone Input Module" component.
3. In the Hierarchy panel select Ships\Friendly\SSCHawk (Arcade)
4. On Player Input Module, change InputMode from "Direct Keyboard" to "Unity Input System"
5. On the Ship Control Module, click "Help" to bring up the SSC manual.
6. In the Table of Contents, locate "Player Input System", "Unity Input System"
7. Follow the instructions to configure the Unity Input System. If you already have a UIS Input Action Asset (UIS configuration file), you can add that rather than creating a new one.
8. Open the UIS Input Action Asset by selecting it in the Project panel and clicking "Edit asset" in the inspector.
9. Select your Action Map (e.g. ShipActions)
10. If the "Camera" action does not exist and it and set the Action Type to Button.
11. In the "Camera" action create a binding for a keyboard key. e.g. C [Keyboard]
12. In the "Camera" action create a binding for Gamepad e.g. Select [Gamepad]
13. If the "Menu" action does not exist and it and set the Action Type to Button.
14. In the "Menu" action create a binding for a keyboard key. e.g. Escape [Keyboard]
15. In the "Menu" action create a binding for Gamepad e.g. Start [Gamepad]
16. Click "Save Asset", then close the UIS Input Action Asset
17. In the Hierarchy panel select Ships\Friendly\SSCHawk (Arcade)
18. On the Player Input Module, expand "Custom Player Input 01"
19. Click "Is Enabled" and select "Camera" for the "Action"
20. On the Player Input Module, expand "Custom Player Input 02"
21. Click "Is Enabled" and select "Menu" for the "Action"
22. Save the scene

You new controls on a Gamepad or XBox One controller are:
Thrusters = Right Trigger, Left Trigger
Pitch+Yaw = Left Stick
Roll = Right Stick
(un)pause = Start
Change Camera = Select
Fire = X button
Fire Guided Missile = B button
There are also keyboard controls.