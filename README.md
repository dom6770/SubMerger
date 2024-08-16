# SubMerger
SubMerger is a C# programm for SABnzbd, used as post-processing script for merging external subtitles into the mkv container.

It supports at the moment only VobSubs (.idx, .sub).

## Compatibility

- Windows
- Linux

## Requirements

- mkvmerge (https://mkvtoolnix.download/windows/releases/) (tested with v74 and v86)
- Linux: glibc 

## Installation

- Put the binary into the SABnzbd Scripts Folder (found in the SABnzbd settings unter "Folders")
- Modify settings in SABnzbd settings under "Categories" and select "SubMerger.exe" 