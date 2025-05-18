# Unity Resource Constants Generation

A Unity editor tool that generates constants for assets in Resources folders.

## Installation

1. Open Unity Package Manager (`Window > Package Manager`).
2. Click `+` > `Add package from git URL`.
3. Enter: 

```
https://github.com/RemoteMS/Unity-Resources-Constants-Generator.git?path=/src/Unity-Resources-Constants-Generator#1.0.3
```

4. Click `Add`.

Alternatively, open Packages/manifest.json and add the following to the dependencies block:

```json
{
    "dependencies": {
      "com.rms.unityresourcegeneration": "https://github.com/RemoteMS/Unity-Resources-Constants-Generator.git?path=/src/Unity-Resources-Constants-Generator#1.0.3"
    }
}
```

## Usage

1. Go to `Tools > Generate Resources Keys` in the Unity menu.
2. The tool will generate a `ResourcesKeys.cs` file in `Assets/Generated/`.
3. Use the constants in your code to reference Resources assets safely.

## License

MIT License (see LICENSE file).