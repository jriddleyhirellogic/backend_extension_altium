# backend_extension_altium

## extensions
Add extension folder to C:\ProgramData\Altium\Altium Designer*\Extensions and include the following:
* rcs
* ins
* small logo .png (16x16px)
* large logo .png (98x98px)

Register extension in .\ExtensionsRegistry.xml of the same folder. If creating an Altium extension project, folder and register will be automatically updated.

## menus 
Menus are added to rcs file
See Schematic (advsch.rcs) and PCB (advpcb.rcs) in C:\Program Files\Altium\AD20\System\ for guidance on where to add.
For example in rcs,
* RefID0     = 'MNSchematic_PUSCHMENU255' should be the same as the targeted separator in insert after (InsertType = 'After').  In advsch.rcs this is "Separator MNSchematic_PUSCHMENU255 End"
* "Link MNSchematic_PUSCHMENU262", '262' needs to be an index after 255 and before the next separator or item
