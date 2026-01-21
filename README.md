# ORIGINAL REPO → [ToaHartor/GI-cutscenes](https://github.com/ToaHartor/GI-cutscenes)

## THIS REPOSITORY IS A FORK!

A fork of GI-cutscenes focused on performance and the latest .NET features, and makes it more suitable for use as a library rather than a command-line tool. The main library has been pruned, removing the WAV transcoding and MKV muxing functions after decoding. The command-line tools have been streamlined; you can adjust them yourself by referring to the original repository if needed.

In addition, asynchronous processing mode and `Microsoft.Extensions.Logging` support have been added.

If you're not a developer who wants to use GI-cutscenes as a project dependency, I recommend using the original repository. You'll find better community support there.

> [!NOTE]
> 
> This project is a derivative work of the [ToaHartor/GI-cutscenes](https://github.com/ToaHartor/GI-cutscenes) project. While the original source did not contain per-file headers, the project as a whole was licensed under GPLv3, and this fork continues to adhere to those terms.

---

# GI-cutscenes

A command line program playing with the cutscenes files (USM) from Genshin Impact.

Able to extract the USM files and decrypt the tracks.

#### Cutscenes from version 1.0 to 6.3 can be decrypted.

*Also includes CBT3, which has the same files than the live version*

If you want to extract newer cutscenes but the `versions.json` in the released zip is outdated, simply download the updated file in the project tree ([here](https://raw.githubusercontent.com/ToaHartor/GI-cutscenes/main/versions.json)) and replace the file.
This file will be updated with the version key every time a new version drops.