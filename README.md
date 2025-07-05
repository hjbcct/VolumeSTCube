# VolumeSTCube

This is the repo for the paper (https://ieeexplore.ieee.org/document/10858751)

Please kindly find the release package (executable) in this repo.

The source code will be released later.

If you have any questions, feel free to contact jiabaoh20@outlook.com.

---

## 📦 Contents

- `DataTransformationModule/`  
  Scripts for preprocessing, interpolation, and smoothing of raw spatial-temporal data.

- `RenderingModule/`  
  Unity project for volume rendering and interactive exploration.

---

## 🚀 Quick Start

Use the air pollution data provided in `/DataTransformationModule/exampleData` as an example.

### 1. Data Transformation

#### (a) Preprocessing

```bash
python /DataTransformationModule/exampleData/0_exampleDataMerge.py
```

#### (b) Kriging Interpolation

```bash
python /DataTransformationModule/1_KrigingInterpolation.py
```

#### (c) Smoothing

```bash
python /DataTransformationModule/2_Smooth.py
```

After running these scripts, you will obtain:
**8 `.raw` volumetric data files with the corresponding `.ini` configuration files**

These will be located in:

```bash
/DataTransformationModule/UnityRawData
```

---

### 2. Rendering and Visualization in Unity

#### (a) Open the `/RenderingModule` folder as a Unity project.

#### (b) In the Unity Editor:

   - Navigate to the top menu bar and click `Volume Rendering > Load dataset > Load raw dataset`.
   - Import the 8 `.raw` files **one by one**.  
     (This means you will execute `Load dataset > Load raw dataset` **8 times**.)

#### (c) After importing, you will see 8 `VolumeRenderedObjects` in your scene.  

   - Group them under the GameObject named `VolumeController`.

#### ⚠ **Important:**  

   Make sure you import the datasets in the same sequence as they appear (bottom to top), to ensure the volumetric layers align correctly.

#### (d) Locate the script `VolumeControllerObject.cs` at:

```bash
Assets/Scripts/VolumeObject/VolumeControllerObject.cs
```

Bind this script to the `VolumeController` GameObject.  
The script will automatically initialize:

- local scales,
- positions,
- and other volume parameters.

---

💡 Once setup is complete, you can freely explore the example dataset in Unity.