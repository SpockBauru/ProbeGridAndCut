# ProbeGridAndCut
Make a grid of Light Probes in Unity, and them Cut unwanted ones using raycast tools

# How to install
Install ProbeGridAndCut in your Assets folder (just copy the folder). 

Them right click on Hierarchy window -> Light -> Probe Grid And Cut. It will place a grid of 5x5x5 Light Probes in your Scene with the script ProbeGridAndCut in the Inspector. This is where you can make settings to the Light Probe Grid.

An alternative method is to place a light probe group in your scene, and then add the ProbeGridAndCut script in the inspector.

# Making a grid of Light Probes
In the ProbeGridAndCut Inspector, you can set the number of Light Probes on each axis, with the minimum of 2. Press "Generate" to apply the settings and place your grid of Light Probes in the Scene.

Now you can cut unwanted probes with the methods bellow

# Cutting Light Probes
There are a few methods to cut unwanted Light Probes based on the objects in the scene. But first you must choose if you want to test only Static objects, or all the objects.

This is important because it's not a good idea to remove Light Probes on moving objects.

Use the checkbox **"Static Objects Only?"** to make the tests only on objects with a Static flag enabled.

## Cut based on Tags
This method is designed to cut probes that are placed beyond the limits of the scene, such as the ground or the walls of a cave. 

**Warning:** Work only with objects containing colliders.

First you must add a Tag to the objects that make those limits. After that, add the desired tag on the field "Cut probes outside tagged boundaries". You can add many tags as desired, as long as they are not named "Untagged".

Now you can click on the button "Cut Probes Outside Tagged Boundaries". It will test each Light Probe from the center to the probe and see if the line intercepts the tagged object.

## Cut based on objects around
This method will cast rays on each axis of the light probe (yellow lines) and make decisions based on what is found.

**Warning:** Work only with objects containing colliders.

There are two methods here: "Cut inside objects" and "Cut far from objects.

For both methods, you can set the size of the ray (yellow line) on the field "ray test size".

### Cut probes inside objects
This method is designed to delete probes that are inside objects. It will test 5 axis of each Light Probes: Up, Left, Right, Forward and Backward

If all rays intercept the same object, it means that the probe is inside it and will be removed.

Note: The "down" axis is not tested because it is common practice to have objects that have no bottom, such as trees.

### Cut probes far from objects
This method is designed to delete probes that are far away from any object. Normally these probes don't contain any relevant light information, but use with care in places that have a high usage spotlights.

When you click the button, all 6 axis will be tested on each Light Probe: Up, Down, Left, Right, Forward and Backward

If any ray intercepts an object, the probe will be cut.

# Make Everything
If you are used with the tool and know how to configure it, this button makes all operations above at once! Just make sure that you know what are you doing.

# The Dangerous Button
This option open **The Dangerous Button**. This button will use the option "Make Everything" for all ProbeGridAndCut scripts placed in the scene! You must be really careful on this one!

# Limitations
ProbeGridAndCut is not designed to work with a huge number of Light Probes at once covering a vast area. It is designed to be placed various times in a scene, with relatively small grids (less than 10,000 probes). If you want to place something like 100,000 probes at once, please consider professional tool, such as "Magic Light Probes", "AutoProbe" or "Automatic Light Probe Generator"

Cutting probes relies on raycast, which only works on object that have a collider. If the object doesn’t have a collider, it will not be tested for the cut.
