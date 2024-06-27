# BGEModTools
Tools for modifying Beyond Good and Evil (20th Anniversary)

This tool can be used to extract a pak archive into folders containing all the known resources inside (with file extensions where known)

## Currently Working
- Extraction of files from PAK archive.
- Filtering based on size.
- Identification of the following file types 
    - png
    - gao
    - wem
    - fnt (dubious)
    - fntdesc (dubious)

## TODO
- Identification of the remaining formats.
- Filtering based on type.
- Re-injection of modified files.

## Example usage
```
BGEModTools -i Resources.pak
```

## Args
| Arg      | Usage                  | Default |
| -------- |----------------------- | ------- |
| -i       | location of input file |         |
| -minsize | filter for small files | 1024    |

## Reference
- Thanks to bartlomiejduda. I referenced their tools project when looking for clarification on some of the pak variables!