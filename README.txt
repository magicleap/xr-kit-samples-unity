MagicVerse 101

Created on and tested with :
• Unity 2019.3.3f1
• Lumin SDK 0.24
• Lumin OS 0.98.10
• Xcode Version 11.3.1 (11C504)
• iOS Version 13.3.1

This example shows how to create a MagicVerse enabled Unity project that anchors content to MagicLeap created PCFs and transmits that content across a peer to peer local area network, which enables other devices running either an XRKit enabled app on iOS or Android OS, or another MagicLeap devices with the same app to see the same content. The inverse is also shown, where an XRKit enabled iOS or Android app can place a cube attached to an Anchor and it will appear on peer MagicLeap, iOS, and Android devices as well. 

Note: This code uses a modified version of the MLTK Transmission API. A new overloaded version of Transmission.Spawn API has been added which takes a PCFID/XRAnchor string as a parameter. If this parameter exists, the passed position and rotation will be used as a local offset to the given PCF (rather than to the current user’s headpose). It is used to spawn a resource from the resource folder on all peers and set the newly spawned object as a child of a PCF.

The Unity project contains a cross platform codebase and can be compiled and ran on Lumin, iOS, and Android. 

Note, in order to make the XR Lib (in Assets) compile on Lumin, Assembly definition (.asmdef) files needed to be added to the Lib folder and to subsequent Editor folders, and specified these should only be compiled on iOS or Android (or editor accordingly). Additionally, the Lib .asmdef file needed a reference to the .asmdef file located in the packages/XRKit packages. 

The code includes an example main MVPlacementExample which manages the Input and calls our modified Transmission,Spawn to spawn an Anchor/PCF attached game object across all peers on the network. 

PCFSystem handles management of both the PCFs (on Lumin) and Anchors (on iOS or Android). 

PCFAnchorVisual maintains the text strings seen in space. These will only be visible if the checkbox is checked on PCFSystem and if PCFSystem has a prefab with PCFAnchorVisual attached to it. 

There are two scenes, 1 for Lumin and 1 for iOS or Android. Select the correct scene for the correct build target, set up credentials and away you go! Experience the MagicVerse… 

