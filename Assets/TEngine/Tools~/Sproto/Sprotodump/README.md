# sproto dump
parse sproto file `.sproto` to binary file `.spb` , c# code `.cs` or go code `.go`.

usage is as follows:
```
usage: lua sprotodump.lua <option> <sproto_file1 sproto_file2 ...> [[<out_option> <outfile>] ...] [namespace_option]

    option: 
        -cs              dump to cSharp code file
        -spb             dump to binary spb  file
        -go              dump to go code file
        -md              dump to markdown file
        
    out_option:
        -d <dircetory>               dump to speciffic dircetory
        -o <file>                    dump to speciffic file
        -p <package name>            set package name(only cSharp code use)

    namespace_option:
        -namespace       add namespace to type and protocol
```
