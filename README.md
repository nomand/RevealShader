<img src='/PREVIEW/01.gif'/>

# Reveal Shader

This is a set of shaders for Unity3D. It maps worldspace position of a gameObject and draws to a `RenderTexture` in relation to world bounds and remaps it back onto the world as a mask, allowing for various shader effects.

### Features:
* Black and White to RGB texture reveal shader
* Transparent to Opaque RGB Texture reveal shader
* Auto and Manual world bounds lookup
* Fading over time
* Adaptive RenderTexture aspect ratio
* Custom inpspector

The RenderTexture mask generation and re-projection happens separately from the surface shaders, so any combination of them is possible and adding new shaders is trivial without affecting the setup.

### Download
Download the `UnityPackage` from [Releases](/Releases) to get the bare bones setup, or clone the repo for an example project.

### License
All code licensed under MIT License. Refer to [License.md](License.md) for full terms.
Pull requests welcome.

### Usage
Effects rely on object materials having a shader property `"_Splat"` to assign the `RenderTexture` and offset parameters. Objects without a compatible shader will be skipped.
Use `Runningtap/Reveal/B&W2RGB` or `.../A2RGB` shaders on paintable objects.

Add [`Painter.cs`](/Assets/Runningtap/Reveal/Scripts/Painter.cs) to the scene.

Pick `Lookup Mode`.
   * Auto will iterate through all the world objects to calculate bounds. Use this for quick preview.
   * Manual will enable a box collider to set bounds manually. Use this for builds and complex scenes. BoxCollider will be turned off on start to disable physics.

Pick `SplatMap Resolution`
   * **32**x - **2048**x, more resolutions can be added in the `Resolution` enum.

`Use Relative` enables non-power of two resolution for the splat map, based on world bounds to avoid overly stretched mask pixels. This will be calculated automatically, keeping the longest edge to selected resolution.

`Fade Over Time` allows for non-permanent mask drawing - stroke fades from oldest pixel. Fade duration will be available if selected.

Add a parent `gameObject` containing all the paintable objects to `World`

Make sure `RevealRTX` is plugged into the Render Texture Shader.

`Brush` is a gameObject that will 'draw' the mask onto `RenderTexture`. Only XZ coordinates are considered.

`Brush Size` will change the size of the area drawn onto the `RenderTexture`.

`Brush Strength` determines stroke opacity.

`Splat Map Preview` will display current `RenderTexture` resolution and texture in play mode.