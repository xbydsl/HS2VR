# About

A version of https://github.com/OrangeSpork/HS2VR with added functionality for Studio.
The Studio tool has been adapted from KKS_VR https://github.com/IllusionMods/KKS_VR
Almost no code by my own, just merged, adapted and fixed the code from the guys above, so all the credit goes to them: OrangeSpork, Manly Marco, killmar-the-3rd, Eusth, Ooetksh.


# Future development

This plugin is probably not worth more effort, instead I will try to adapt the whole KKS_VR plugin to HS2, it uses a more modern version of the graphic library (VRGIN OpenXR).


# What works

The main game, including the character editor, works as the original non-VR game, same as in OrangeSpork/HS2VR.
StudioNEOV2 can work in VR with an unique tool that allows moving objects and characaters articulations with the VR controllers. In standing mode, move the camera with the grip, pick and move/rotate objects and bones with the trigger.


# What doesn't work (Studio)

Each time Studio opens
Mouse can be used in seated mode to move the camera around, but objects are not selectable. You need to use the controllers.
Some of the graphic interface windows will move way too fast when you reposition them.
When saving a scene the thumbnail will look awful because of the open FOV of the camera.
You can use the controllers in seated mode to manipulate objects and articulations, but cannot move the camera with them, you need to use the mouse or switch to standing mode.

Mirrors do not work properly.
If you change screen resolution, the next time VR will be messed up. You will need to close and start the VR game/studio again.


# Setup
You will need a current version of HS2 BetterRepack. 
Download the Release zip, place the content in your game folder.
If you were using HS2VR this plugin will overwrite the older one.

Have a look at VRSettings.XML, you will find the keyboard shortcuts and some options there.


# Trobleshooting/FAQ

You can see more notes about features and problems in the original Orangespook readme:
https://github.com/OrangeSpork/HS2VR


# Building from source

The mod currently uses a patched `VRGIN.dll` from the AI-Shoujo VR mod by Ooetksh, not the included git submodule. Once that version of VRGIN makes it onto github, the repo can be updated to include it.
