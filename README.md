# Magicverse 101

Copyright 2019-2020 Magic Leap, Inc. All rights reserved.

Created on and tested with :

* **Unity 2019.3.5f1**
* **Lumin SDK 0.24**
* **Lumin OS 0.98.10**
* **Xcode Version 11.3.1 (11C504)**
* **iOS Version 13.3.1**
* **Android 10**

## Overview

This example shows how to create a Magicverse enabled Unity project that anchors content to Magic Leap created PCFs and transmits that content across a peer to peer local area network. This enables other devices running either an [**XR Kit**](https://developer.magicleap.com/downloads/magicversesdk) enabled version of the app on iOS or Android, or other Magic Leap devices with the same app to see the same content. The opposite is also shown, where an **XR Kit** enabled iOS or Android app can place a cube attached to an Anchor and it will appear on peer Magic Leap, iOS, and Android devices as well. 

## Getting started

This project contains one file that is stored in [LFS](https://git-lfs.github.com/) since it is too large: https://github.com/magicleap/xr-kit-samples-unity/blob/master/Magicverse101/Packages/MagicLeapXRKit/Runtime/Native/iOS/libmlxr_runtime_s.a

So to get started on this project, `git clone` the repo to a folder in your hard drive, then `cd` into that folder and run the `git lfs pull` command to pull down the full file.

Caveat: if you use the "Download Zip" button above, the zip will only contain the lfs link and not the full file.

## Implementation notes
**Note:** This code uses a modified version of the [**MLTK**](https://developer.magicleap.com/learn/guides/magic-leap-toolkit-overview) Transmission API. A new overloaded version of Transmission.Spawn API has been added which takes a PCFID/XRAnchor string as a parameter. If this parameter exists, the passed position and rotation will be used as a local offset to the given PCF (rather than to the current user’s headpose). It is used to spawn a resource from the resource folder on all peers and set the newly spawned object as a child of a PCF.

The Unity project contains a cross platform codebase and can be compiled and ran on Lumin, iOS, and Android. 

**Note:** For iOS and Android, this sample project uses [**XR Kit for Unity**](https://developer.magicleap.com/downloads/magicversesdk), and will require the developer to obtain OAuth credentials and populate them in the project as described in the documentation there.

The code includes an example main MVPlacementExample which manages the Input and calls our modified **Transmission.Spawn** to spawn an Anchor/PCF attached game object across all peers on the network. 

PCFSystem handles management of both the PCFs (on Lumin) and Anchors (on iOS or Android). 

PCFAnchorVisual maintains the text strings seen in space. These will only be visible if the checkbox is checked on PCFSystem and if PCFSystem has a prefab with PCFAnchorVisual attached to it. 

There are two scenes, one for Lumin and another for iOS and Android. Select the correct scene for the correct build target, set up OAuth credentials and away you go! **Experience the Magicverse…**

## License

See details in [license.md](license.md)
