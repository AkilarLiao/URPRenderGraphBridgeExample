# URP RenderGraph Bridge Example

This project demonstrates how to implement both traditional URP `ScriptableRenderPass` and the new `RenderGraph` API introduced in Unity 6.0.

## 🧩 Features

- ✅ DistortionFullScreenPass implementation (Legacy & RG)
- ✅ CustomRenderObjectPass equivalent in both systems
- ✅ Partial architecture separation for clean compilation
- ✅ Version-based conditional compilation (Unity 2022.3 vs Unity 6+)
- ✅ Modular & minimal design for educational use

## 🛠️ Requirements

- Unity 2022.3.x (for legacy URP, copy com.sb.urp-render-graph-bridge-example to Packages folder)
- Unity 6000.x (for RenderGraph version)

## 📁 Folder & File Overview

- `/Runtime/RenderFeatures/URPRenderGraphBridgeRenderFeature.cs`: Traditional URP RenderFeature
- `/Runtime/RenderPasses`: Reference RenderPass, with _RenderGraph is RenderGraph Implement(Unity 6.0 RenderGraph versions)
- `/Shaders`: Reference shaders
- `/Tests`: Reference demo

## 🧪 How to Run

1. Open the project in Unity
2. Open Tests/SampleScene.unity
3. Play in editor to compare outputs
