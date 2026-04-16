# BizSim Figma Importer

Last reviewed: 2026-04-16

## Overview

BizSim Figma Importer is an editor-only Unity package that downloads assets from Figma designs via the Figma REST API. It is the required companion to the Figma UI Toolkit Generator plugin (Figma2UnityUIToolkit), which generates UXML/USS files from Figma frames. This package completes the pipeline by importing the referenced images, icons, and fonts into the Unity project.

The import workflow reads a JSON Asset Manifest produced by the Figma plugin, resolves asset paths based on configurable rules, and downloads missing assets from Figma. Assets are organized into folders by category (backgrounds, icons, logos, illustrations) with configurable naming conventions and optional Addressables integration.

## Table of Contents

| File | Description |
|------|-------------|
| [getting-started.md](getting-started.md) | Installation, Figma token setup, and first import |
| [api-reference.md](api-reference.md) | Public types: orchestrator, settings, manifest, path rules |
| [configuration.md](configuration.md) | FigmaImporterSettings and FigmaUserSettings walkthrough |
| [architecture.md](architecture.md) | Editor assembly structure, import pipeline, extensibility |
| [troubleshooting.md](troubleshooting.md) | Common errors with root causes and fixes |
| [DATA_SAFETY.md](DATA_SAFETY.md) | Data flow and privacy information |

## Links

- [README](../README.md) -- quick-start experience
- [CHANGELOG](../CHANGELOG.md) -- version history
- [GitHub Repository](https://github.com/BizSim-Game-Studios/com.bizsim.unity.figma.importer)
