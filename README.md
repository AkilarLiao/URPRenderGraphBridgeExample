# URP RenderGraph Bridge Example

This project demonstrates how to implement both traditional URP `ScriptableRenderPass` and the new `RenderGraph` API introduced in Unity 6.0.

## 🧩 Features

- ✅ DistortionFullScreenPass implementation (Legacy & RG)
- ✅ CustomRenderObjectPass equivalent in both systems
- ✅ Partial architecture separation for clean compilation
- ✅ Version-based conditional compilation (Unity 2022.3 vs Unity 6+)
- ✅ Modular & minimal design for educational use

## 🛠️ Requirements

- **Unity 2022.3.x**  
  For legacy URP version. Copy `com.sb.urp-render-graph-bridge-example` into your project's `Packages/` folder.

- **Unity 6000.x** (Unity 6.x Preview)  
  Supports native `RenderGraph` API. Install normally via Package Manager or local package reference.
  
## 📁 Folder Structure

/Runtime/
└── RenderFeatures/URPRenderGraphBridgeRenderFeature.cs # Entry point for RenderFeature
└── RenderPasses/ # Legacy & RenderGraph-based passes

/Shaders/ # Reference shaders
/Tests/SampleScene.unity # Demo scene with comparison output
/Images/Demo.gif # Visual reference

## 🧪 Getting Started

1. Open the project with **Unity 2022.3** or **Unity 6000+**
2. Open the scene at `Tests/SampleScene.unity`
3. Hit **Play** to see both legacy and RenderGraph effects in action

## 💡 Why This Exists

Unity's RenderGraph (introduced in Unity 6.0) offers better performance and clearer GPU scheduling, but many URP features were built on the older `ScriptableRenderPass` system.  
This example shows how to **bridge** the gap between these systems — perfect for:

- Porting old RenderFeatures to new Unity versions
- Learning RenderGraph in a modular, minimal setup
- Comparing architecture and behavior between the two systems
