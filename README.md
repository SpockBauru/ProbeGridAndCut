# ProbeGridAndCut
Make a grid of Light Probes in Unity, and them Cut unwanted ones.

# Index
 * [How to install](#how-to-install)
 * [Making a grid of Light Probes](#making-a-grid-of-light-probes)
 * [Cut Probes Based on Objects](#cut-probes-based-on-objects)
   * [Cut based on Tags](#cut-based-on-tags)
   * [Cut based on objects around](#cut-based-on-objects-around)
     * [Cut probes inside objects](#cut-probes-inside-objects)
     * [Cut probes far from objects](#cut-probes-far-from-objects)
	 * [Make All Colliders](#make-all-colliders)
 * [Cut based on Baked Light](#cut-based-on-baked-light)
 * [Make Everything](#make-everything)
 * [DANGER ZONE](#danger-zone)
 * [Limitations](#limitations)
 * [Compatibility](#compatibility)
 * [Ending notes](#ending-notes)
 * [Changelog](#changelog)

# New in v2.0
https://github.com/SpockBauru/ProbeGridAndCut/assets/67930504/72e6d08d-bc47-4496-a8f0-22b3c9723211



# How to install
Install ProbeGridAndCut in your Assets folder (just copy the folder). 

Them right click on Hierarchy window -> Light -> Probe Grid And Cut. It will place a grid of 5x5x5 Light Probes in your Scene with the script ProbeGridAndCut in the Inspector. This is where you can make settings to the Light Probe Grid.

An alternative method is to place a light probe group in your scene, and then add the ProbeGridAndCut script in the inspector.

# Making a grid of Light Probes
In the ProbeGridAndCut Inspector, you can set the number of Light Probes on each axis, with the minimum of 2. Press "Generate" to apply the settings and place your grid of Light Probes in the Scene.

Now you can cut unwanted probes with the methods bellow

# Cut Probes Based on Objects
First you must choose if you want to test only Static objects. This is important be-cause it's a bad idea to remove Light Probes on moving objects.

Use the checkbox "Static Objects Only?" to consider only objects with a Static flag enabled. 

## Cut based on Tags
This method is designed to cut probes that are placed beyond the limits of the scene, such as the ground or the walls of a cave. 

**Warning:** Work only with objects containing colliders.

First you must add a Tag to the objects that make those limits. After that, add the desired tag on the field "Cut probes outside tagged boundaries". You can add many tags as desired, as long as they are not named "Untagged".

Now you can click on the button "Cut Probes Outside Tagged Boundaries". It will test each Light Probe from the center to the probe and see if the line intercepts the tagged object.

## Cut based on objects around
This method will cast rays on each axis of the light probe (yellow lines) and make decisions based on what is found.

**Warning:** Work only with objects containing colliders.

### Cut probes inside objects
This method is designed to delete probes that are inside objects. It will test 5 axis of each Light Probes: Up, Left, Right, Forward and Backward. The "down" axis is not tested because it is common objects that don't have one side, such as trees.

If all rays intercept the same object, it means that the probe is inside it and will be removed.

### Cut probes far from objects
This method is designed to delete probes that are far away from any object. Normally these probes don't contain any relevant light information, but use with care in places that have a high usage spotlights.

When you click the button, all 6 axis will be tested on each Light Probe. If none of these rays intercept an object, the probe will be cut.

## Make All Colliders
If you are used with the tool and know how to configure it, this button makes all operations above at once! Just make sure that you know what are you doing.

# Cut based on Baked Light
In order to work, your probes need to be baked already: Window -> Rendering -> Lighting -> Generate Lighting.
This option will test the probe with those around. It will be cut if the color baked in the probe is not different enough.
 
When using this option, a windows pop up will appear asking if you want to bake the lighting before cut the probes:
 
-	Normal Bake: The regular Generate Lighting.
-	Simple Bake: Fast baking with reduced quality.
</br>WARNING: This option will destroy your lightmaps!
-	Just Cut: Cut the probes with the current baking.

# Make Everything
If you are used with the tool and know how to configure it, this button makes all operations above at once! Just make sure that you know what you are doing.

# DANGER ZONE
This option open **The Dangerous Button**. This button will use the option "Make Everything" for all ProbeGridAndCut scripts placed in the scene! You must be really careful on this one!

# Limitations
ProbeGridAndCut is not designed to work with a huge number of Light Probes at once covering a vast area. It is designed to be placed various times in a scene, with relatively small grids (less than 10,000 probes). If you want to place something like 100,000 probes at once, please consider professional tool, such as "Magic Light Probes", "AutoProbe" or "Automatic Light Probe Generator"

Cutting probes relies on raycast, which only works on object that have a collider. If the object doesn’t have a collider, it will not be tested for the cut.

# Compatibility
I made an effort to make this tool compatible with most versions of Unity possi-ble, from Unity 5.6 to the present.
ProbeGridAndCut is independent of the rendering pipeline and platform in use. In the Demo scene, you may have to convert the material to URP or HDRP using Unity's Render Pipieline Converter (or just delete the DemoScene folder).

# Ending notes
This tool was entirely made on my free time. If you want to support me, please make an awesome asset compatible with URP and publish for free to the community!

# Changelog
v2.0:
-	New Feature: Cut probes by Light Difference.
-	New Feature: Disable Yellow Lines.
-	New Feature: Count all probes from opened scenes.
-	New Feature: Added progress bar.
-	UI redesign, now with foldouts.
-	Cut probes now follow the group orientation.
-	Complete code overhaul: performance was improved.
-	Fixed a bug related to cut inside objects when there are other objects nearby.
-	Compatibility with Unity 2023

v1.1:
-	Fixed a bug that happens when you rotate the light probe group.
-	Improved Gizmodo scaling.
-	Improved precision.
-	Improved documentation.

v1.0:
-	First release.
