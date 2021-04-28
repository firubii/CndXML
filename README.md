# CndXML
A program for converting HAL's CND binaries to XML and back

Contains much better and faster code than what can be found in my older tool [KirbyCND](https://github.com/firubii/KirbyCND), which should be able to be easily ported to other programs if needed.
Also supports the newer CND version introduced in Kirby Fighters 2.

## Usage
To covert a CND binary to XML:

```CndXML.exe -d <path to CND>```

To convert an XML file to CND:

```CndXML.exe -a <path to XML>```

The output file will share the same name as the input file and will be placed in the same directory.
