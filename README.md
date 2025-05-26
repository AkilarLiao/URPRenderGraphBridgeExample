# URP RenderGraph Bridge Example

This project demonstrates how to implement both traditional URP `ScriptableRenderPass` and the new `RenderGraph` API introduced in Unity 6.0.

## 🧩 Features

- ✅ FullScreenPass implementation (Legacy & RG)
- ✅ RenderObjectPass equivalent in both systems
- ✅ Sealed architecture separation for clean compilation
- ✅ Version-based conditional compilation (Unity 2022.3 vs Unity 6+)
- ✅ Modular & minimal design for educational use

## 🛠️ Requirements

- Unity 2022.3.x (for legacy URP)
- Unity 6000.x (for RenderGraph version)

## 📁 Folder Overview

- `/Scripts/LegacyURP`: Traditional URP Passes
- `/Scripts/RenderGraphURP`: Unity 6.0 RenderGraph versions
- `/Scripts/Shared`: Common data & logic
- `/Shaders`: Shared materials/shaders

## 🧪 How to Run

1. Open the project in Unity
2. Open the provided example scene
3. Play in editor to compare outputs