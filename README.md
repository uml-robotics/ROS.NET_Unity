# A wrapper to precompile ROS.NET for use as a Unity plugin

## ROS,NET?
https://github.com/uml-robotics/ROS.NET

## How do I use this?
- Check out this repository and init/update its submodule
```
git clone --recursive https://github.com/uml-robotics/ROS.NET_Unity
cd ROS.NET_Unity
```

THEN:

To use ROS.NET as a Unity script, run:
```
buildUnityScriptSource.bat
```

All of the things needed by Unity to allow you to call ROS.Init(...) and make NodeHandles and all of that ROS-Y stuff now exists in the COPY_TO_UNITY_PROJECT\ directory
	
Copy the contents of that foler into 
INTO: ```<your Unity project dir>/Assets/Plugins```( or a subdirectory there-of)

Now your projects Unity scripts have access to the core ROS.NET capabilities.

For some additional assets and tutorials check out https://github.com/uml-robotics/ROS.NET_Unity_Assets
A set of such Unity scripts are included in the Scripts directory.
The included scene SHOULD be a loose approximation of how RVIZ displays transforms on a Map... but the camera is deeeeerpy. GLHF

Enjoy!

-Eric McCann (a.k.a. nuclearmistake) @ the University of Massachusetts Lowell Robotics Lab
