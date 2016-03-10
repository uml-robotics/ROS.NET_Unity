# A wrapper to precompile ROS.NET for use as a Unity plugin

## ROS,NET?
https://github.com/uml-robotics/ROS.NET
A series of C# projects and one C++ project (a p/invoke wrapper around XMLRPC++) that allow a MANAGED .NET application co communicate with any other ROS nodes.

## How do I use this?
- Check out this repository and init/update its submodule
```
git checkout https://github.com/uml-robotics/ROS.NET
git submodule init
git submodule update
buildme.bat
```
- Copy/Paste all of the resulting DLL files in the top level of this repository (and optionally their pdb files)
  INTO:
```<your Unity project dir>/Assets/Plugins```

Enjoy!

-Eric McCann (a.k.a. nuclearmistake) @ the University of Massachusetts Lowell Robotics Lab