Welcome to the Realistic Weather Simulations VFX package. Listed below are some tips to help you get started using
this package.



Realistic Weather Simulations VFX is a package that consists of different rain, snow, clouds, and storm effects.
Each effect can be used in different ways to acheive different results. 

Rain - Rain effects can be used to follow the player by making it a child of the player/camera or can be used to put
rain in a particular part of a scene. 

	Player Effects - Drag the effect into the hierachy as a child of the player or camera. Set the master height
			 to 10 units on the Y axis. Adjust the ground effects (labeled as "empty") height so 
			 that it is near the floor and you can see all the splashes and fog. This works best for 
			 flat surfaces, as uneven surfaces will hide some of the ground effects. 

	Area Effects -   Drag and drop the effect into the desired area of the scene. Set the master height to a 
			 to a level that is at least 5 units above the highest Y value the player can go. Set the 
			 ground effects (labeled as "empty") height so that it is near the floor and you can see all
			 the splashes and fog. This works best for flat surfaces, as uneven surfaces will hide some 
			 of the ground effects. Lifetime of particles may need to be increased so that rain touches
			 the ground in all areas. 


Snow - Snow effects can be to follow the player by making it a child of the player/camera or can be used to put
snow in a particular part of a scene. 

	*note - The Blizzard effect should not be set as a child of the player and should always be used as an 
		area effect.

	Player Effects - Drag the effect into the hierachy as a child of the player or camera. Set the master height
			 to 10 units on the Y axis. Snow is set with collision so that when it touches the 
			 ground it "sticks" to the surface before it destroys. 

	Area Effects -   Drag and drop the effect into the desired area of the scene. Set the master height to a 
			 to a level that is at least 5 units above the highest Y value the player can go. Particle 
			 lifetime may need to be increased so the snow touches the ground in all areas. 


Fog - Each for effect can be set to follow the player be making it a child of the player/camera or can be use to put 
fog in a particular part of a scene. 

	Player Effects -  Drag the effect into the hierarchy as a child of the player/camera. Set the master height 
			  so that the fog is sitting just above floor level. 

	Area Effects -    Drag and drop the effect into the desired area of the scene. Set the master height so that
			  the fog is sitting just above floor level. 


Clouds - Clouds should be use primarily as an area effect. Additional scripting can be written to lock the Y axis
for if a player follow effect is desired. 

	Area Effects -    Set the clouds 12-15 units above the highest Y value the palyer can go. To increase cloud 
			  coverage, increase X and Z values while proportionally increasing the Emission Rate Over
			  Time value.


Fireflies - Fireflies can be set to follow the player by making it a child of the player/camera or can be used as an
area effect by dragging it into the scene. 

	Player Effect -    Drag the effect into the hierarchy as a child of the player/camera. Set the master height 
			   to 5 units on the Y axis. 
				Note* - Using this effect as a player follow style effect in a game with lots of 
				vertical movement may not bring about a desired asthetic. 

	Area Effect -      Drag and drop the effect into the desired area of the scene and set the Y value to 5-6 
			   units.

Snow Drifts - Snow Drifts should be used as an area effect. 

	Area Effect -      Drag and drop the effect into the desired area of the scene. set the Y value just above 
			   the ground level or the mesh where the drift should be coming from (about 0.5 - 1 units
			   on the Y axis). 

Wind - All Snow and Rain (excluding Blizzard) systems effects are set to work with the Unity WindZone. Placing a WindZone within the scene
will affect the movement of the system. WinZones will not affect Snow Drifts, Fog, Clouds, or Fireflies. In order to 
have a WindZone affect these effects, check the External Forces box in the particle systems Inspector panel. 



Other Tips - 

	The Global Fog setting helps make this package look even better!

	The Skybox color will impact the way the clouds look. Setting the Skybox color to a shade of grey will 
	be optimal. 

	When changing the size and shape of the effects, be sure to proportionally change the emission rate along
	with it. 


Thank you for purchasing this package. For questions regarding this package contact danbaratta@gmail.com .
			