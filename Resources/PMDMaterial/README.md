MMD Shader for Unity
========

### Preview ###

<img src="http://3dcgarts.github.com/MMD-Shader-for-Unity/images/material_preview.png">

Presented by [3DCGArts](http://www.3dcg-arts.net/)

### Download ###

[Version 1.2](https://github.com/3dcgarts/MMD-Shader-for-Unity/zipball/v1.2)

### 使い方 ###

Unityパッケージである

```
PMDMaterials.unitypackage
```

を、Unityのメニューの

```
Assets->Import Package->Custom Package...
```

からインポートしてください。

シェーダは以下の４つが用意されています。

```
MMD/PMDMaterial
MMD/PMDMaterial-with-Outline
MMD/Transparent/PMDMaterial
MMD/Transparent/PMDMaterial-with-Outline
```

Transparent とついているものは、透明な材質に使ってください。

Outline     とついているものは、輪郭線を表示する材質に使ってください。

### その他 ###

シェーダに割り当てられているプロパティは、PMDEditor の値に準拠しています。

Directional Light (Color: White, Intensity: 0.5) の光源を受けている時に正しく表示されるように設計されています。

「Lat式ミク」のような特殊なメッシュ構造を持ったモデルも正しく表示することができます。

