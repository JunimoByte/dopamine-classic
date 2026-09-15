![Dopamine](Dopamine.full.png)

# Dopamine

**A modern continuation of Dopamine 2.0.10.**

Dopamine is an audio player which tries to make organizing and listening to music as simple and pretty as possible. Originally created by Digimezzo, this repository serves as a modernized continuation of the classic Dopamine 2.0.10 release, tailored specifically for stability, performance, and modern Windows ecosystems.

![Dopamine Screenshot](Dopamine.screenshot.png)

## Overview of Changes

This version includes deep architectural fixes and a leaner codebase. Legacy support for Windows 7 and 8 has been explicitly dropped to deliver a native, highly performant experience on Windows 10 and Windows 11.

### Stability & Performance Metrics
* **Crashes & Freezes Fixed:** Resolved 20+ potential hard crashes, unmanaged memory faults, and UI hangs (including critical undocumented API crashes on Windows 11).
* **Memory Leaks Sealed:** Completely rewrote the image caching and virtualization systems, eliminating severe RAM leaks that occurred when scrolling through massive libraries.
* **Codebase Debloat:** Stripped out obsolete dependencies, legacy auto-updaters, and deprecated OS polling to ensure faster cold boots and a minimal memory footprint.

### Key Modernizations
* **Windows 11 Native Stability:** Purged deprecated Windows 10 Acrylic composition APIs (`SetWindowCompositionAttribute`) that caused graphical glitches and desktop window manager crashes on modern OS builds.
* **Modern Media Controls:** Replaced brittle, low-level keyboard hooks with native Windows System Media Transport Controls (SMTC) integration.
* **Gapless Playback:** Re-architected track transition logic to run asynchronously, removing database-write blocking and unlocking perfectly gapless playback.
* **High-Res Artwork:** Migrated from deprecated Last.fm APIs to the iTunes Search API for downloading missing artwork, utilizing automatic lightweight 80% JPEG compression.

## Compilation Instructions

Dopamine is written in C# (WPF) and powered by the [CSCore sound library](https://github.com/filoe/cscore). We recommend using **Visual Studio 2022**.

**Prerequisites:**
The Dopamine source code relies on the Windows 10 SDK for modern system notifications (`Windows.winmd`).
1. Install the Windows 10 SDK.
2. Locate `Windows.winmd` (e.g., `C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.17134.0\Windows.winmd`).
3. Copy the file up to its parent folder so it sits at: `C:\Program Files (x86)\Windows Kits\10\UnionMetadata\Windows.winmd`.
4. The project also relies on `System.Runtime.WindowsRuntime.dll`. Ensure it is available at `C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll`.

Once dependencies are met, open `Dopamine.sln` and build.

## Credits & License

* Dopamine was originally developed by [Digimezzo](https://github.com/digimezzo).
* Powered by the [CSCore sound library](https://github.com/filoe/cscore).
* This software uses code of [FFmpeg](http://ffmpeg.org) licensed under the [LGPLv2.1](http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html) and its source can be downloaded [here](https://github.com/FFmpeg/FFmpeg).
