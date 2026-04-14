# Unity Figma Asset Importer

[![Unity 6000.0+](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)
[![Version](https://img.shields.io/badge/Version-1.0.0-orange.svg)](CHANGELOG.md)

> **Required companion** to the [**Figma2UnityUIToolkit**](https://github.com/BizSim-Game-Studios/Figma2UnityUIToolkit) Figma plugin. This Unity Editor tool reads the Asset Manifest emitted by that plugin and downloads the actual image, icon, and font files from Figma via the REST API — without it, the generated UXML/USS has no assets to reference.

**Package:** `com.bizsim.unity.figma.importer`
**Namespace:** `BizSim.Unity.Figma.Importer.Editor`
**Author:** BizSim Game Studios
**License:** MIT

## How It Fits Together

```
┌────────────────────────────┐       ┌──────────────────────────────────┐
│ Figma2UnityUIToolkit       │       │ com.bizsim.unity.figma.importer  │
│ (Figma Dev Mode plugin)    │       │ (this Unity Editor package)      │
├────────────────────────────┤       ├──────────────────────────────────┤
│ Generates:                 │       │ Reads the Asset Manifest         │
│  • UXML                    │  ──▶  │ Fetches images/icons/fonts from  │
│  • USS                     │       │ Figma via REST API               │
│  • Asset Manifest (JSON)   │       │ Writes them into the project     │
└────────────────────────────┘       └──────────────────────────────────┘
```

The Figma plugin is a **codegen** plugin — it can only emit text (UXML/USS/JSON). It cannot download binary assets. This Unity package handles that second half of the pipeline. Using one without the other gives you either unreferenced files or missing assets.

- **Figma plugin repo:** https://github.com/BizSim-Game-Studios/Figma2UnityUIToolkit
- **Local path (monorepo users):** `D:\Projects\_tools\Figma2UnityUIToolkit`

## Installation

**Window → Package Manager → + → Add package from git URL...**

```
https://github.com/BizSim-Game-Studios/com.bizsim.unity.figma.importer.git
```

Or pin a version:

```
https://github.com/BizSim-Game-Studios/com.bizsim.unity.figma.importer.git#v1.0.0
```

Or add directly to `Packages/manifest.json`:

```json
"com.bizsim.unity.figma.importer": "https://github.com/BizSim-Game-Studios/com.bizsim.unity.figma.importer.git"
```

## Quick Start

1. **Install the Figma plugin.** In Figma, enable [Figma2UnityUIToolkit](https://github.com/BizSim-Game-Studios/Figma2UnityUIToolkit) in Dev Mode.
2. **Generate a manifest.** Select a frame, open the Inspect panel, choose **Asset Manifest** as the codegen output, copy the JSON into a file inside your Unity project.
3. **Open the importer.** In Unity: **Window → Figma Asset Importer**.
4. **Enter your Figma Personal Access Token** (stored in `EditorPrefs`, not checked into version control) and your Figma **File Key**.
5. **Select the manifest file** and click **Import**. Assets are fetched over the Figma REST API and placed into the paths configured in your importer settings.

## Requirements

- Unity **6000.0** or newer
- A **Figma Personal Access Token** with read access to the target file
- The [Figma2UnityUIToolkit](https://github.com/BizSim-Game-Studios/Figma2UnityUIToolkit) plugin installed in Figma to produce the Asset Manifest this importer consumes

## License

MIT — see [LICENSE.md](LICENSE.md).
