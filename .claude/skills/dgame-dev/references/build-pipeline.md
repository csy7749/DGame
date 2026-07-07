# 构建打包工作流

> **适用场景**：Unity 整包、AssetBundle 资源包、Jenkins 命令行自动化打包 | **关联文档**：[hotfix-workflow.md](hotfix-workflow.md)、[hotpatch-workflow.md](hotpatch-workflow.md)

DGame 的构建统一收口在 `DGame.ReleaseTools`（`GameUnity/Assets/DGame/Editor/ReleaseTools/ReleaseTools.cs`）。菜单入口用于本地手动打包，`public static` 方法用于命令行/Jenkins 无人值守打包，两者共用同一套 `BuildInternal`（AB）和 `BuildImp`（整包）实现。

## 三类产物

| 产物 | 含义 | 输出目录 |
|------|------|----------|
| AssetBundle 资源包 | YooAsset `DefaultPackage` 资源，用于热更 | `Bundles/{平台}`（`一键打包AB` 走 `Builds/`） |
| Unity 整包 | 可执行安装包（exe/apk/XCode 工程） | `Build/{平台}` |
| StreamingAssets 内置资源 | 首包内置的 AB，随整包发布 | 由 `UpdateSettings` 的打包地址控制 |

每次构建都先执行 `BuildDllCommand.BuildAndCopyDlls()` 编译并复制热更 DLL（AOT metadata + 热更程序集，见 [hotfix-workflow.md](hotfix-workflow.md)），再打 AB，整包构建最后调 `BuildImp`。

## 前置：先转表

> **强制前置**：不管打 AB 还是打整包，**打包前必须先执行配置表转表**，保证包体内的配置表二进制是最新的。转表产物落在 `GameUnity/Assets/BundleAssets/Configs/Binary/`，会被 YooAsset 收集进 AB；漏转表会导致包内配置表停留在旧版本。

转表脚本在 `GameConfig/GenerateTool_Binary/`，客户端**优先使用懒加载转表** `gen_bin_client_lazyload.bat`（`.sh`）；导表规则见 `luban-dev`。Jenkins 流水线里，转表应排在拉取代码之后、调用 `build_*` 脚本之前。

## 前置：HybridCLR GenerateAll

> **强制前置**：在构建首包，或 AOT 程序集/泛型引用发生变化，且当前启用了 HybridCLR 热更新（`ENABLE_HYBRIDCLR` / `UpdateSettings.Enable` 为 true）时，**必须先执行一次 HybridCLR 的 GenerateAll**，再继续 BuildPlayer、`BuildDllCommand.BuildAndCopyDlls()` 和 AB 构建。

执行入口以当前 Unity 菜单为准，通常是 `HybridCLR/Generate/All`（或项目中显示的 `GenerateAll` 等价入口）。这一步会刷新 HybridCLR 生成产物，例如 link、AOTGenericReferences、桥接/反向 PInvoke 等生成文件；漏执行会让首包或 AOT 变动后的裁剪、元数据和热更 DLL 复制链路使用旧生成结果。

不涉及首包、不改 AOT、不启用 HybridCLR 热更时，不需要为了普通资源 AB 构建额外执行 GenerateAll。

## 菜单入口（本地手动）

| 菜单 | 方法 | 行为 |
|------|------|------|
| `DGame Tools/Build/一键打包AB` | `BuildCurrentPlatformAB`，快捷键 `F8` | 当前平台打 AB → `CopyStreamingAssetsFiles` |
| `DGame Tools/Build/AutoBuildWindow` | `AutoBuildWindow` | Windows AB + 整包 `Release_Windows.exe` |
| `DGame Tools/Build/AutoBuildAndroid` | `AutoBuildAndroid` | Android AB + 整包 `{版本}-Android.apk` |
| `DGame Tools/Build/AutoBuildIOS` | `AutoBuildIOS` | iOS AB + 整包 `XCode_Project` |

`AutoBuildXXX` 打完 AB 后会 `SwitchActiveBuildTarget` 切平台再 `BuildPipeline.BuildPlayer`，构建结束用 `RevealInFinder` 打开输出目录。

## 命令行入口（Jenkins 自动化）

`Tools/BuildTools/` 下的 `.bat`（Windows）/`.sh`（macOS）用 `-batchmode -quit -executeMethod` 拉起 Unity 无界面打包。命名规律：`build_{ab_,}{window,android}{_auto,_manual}`——带 `ab_` 只打资源包，不带则 AB+整包；`_auto` 自动版本号，`_manual` 交互输入版本。

| 脚本 | executeMethod | 版本 |
|------|---------------|------|
| `build_ab_window_auto` | `BuildWindowsAB` | 自动 |
| `build_ab_window_manual` | `BuildWindowsABWithVersion` | `-version` |
| `build_ab_android_auto` | `BuildAndroidAB` | 自动 |
| `build_ab_android_manual` | `BuildAndroidABWithVersion` | `-version` |
| `build_window` | `AutoBuildWindow` | 自动 |
| `build_window_manual` | `BuildWindowWithVersion` | `-version` |
| `build_android` | `AutoBuildAndroid` | 自动 |
| `build_android_manual` | `BuildAndroidWithVersion` | `-version` |

命令行模板（`.bat`）：

```bat
"%UNITYEDITOR_PATH%\Unity.exe" -projectPath "%WORKSPACE%" -batchmode -quit ^
  -logFile "%BUILD_LOGFILE%" -executeMethod DGame.ReleaseTools.BuildWindowsAB ^
  -CustomArgs:Language=en_US;"%WORKSPACE%"
```

手动版本号在方法内通过 `GetCommandLineArg("-version")` 读取，脚本追加 `-version=%VERSION%`。

## 环境变量

所有脚本 `source`/`call` 同目录 `path_define`，Jenkins 节点按机器改这里即可：

| 变量 | 含义 | 示例 |
|------|------|------|
| `WORKSPACE` | Unity 工程目录 | `E:\UnityProject\DGame\GameUnity` |
| `UNITYEDITOR_PATH` | Unity 编辑器目录 | `E:\Editor\2022.3.62f3\Editor` |
| `BUILD_LOGFILE` | 打包日志 | `./Log/build.log` |
| `BUILD_DLL_LOGFILE` | DLL 编译日志 | `./Log/build_dll.log` |

> `path_define_tmp.{bat,sh}` 是占位模板，实际值以 `path_define.bat` 为准；新节点从模板复制后填真实路径。

## 版本号

- 自动：`GetBuildPackageVersion()` → `UpdateSettings.GetBuildPackageVersion()`，格式 `yyyy-MM-dd-分钟段`（每 10 分钟一段）。
- 手动：命令行 `-version` 传入，作为 YooAsset `PackageVersion`。

版本号最终写入 `BuildInternal` 的 `BuildParameters.PackageVersion`，决定 YooAsset 清单版本。

## AssetBundle 构建要点（BuildInternal）

- 管线默认 `ScriptableBuildPipeline`，压缩 `LZ4`，包名固定 `DefaultPackage`。
- `ClearBuildCacheFiles = false` + `UseAssetDependencyDB = true`：启用增量构建，加快打包。
- `EnableSharePackRule = true`：共享资源打包；内置 Shader 单独成包（`GetBuiltinShaderBundleName`）避免重复。
- 加密服务从 `GameEntry.prefab` 的 `ResourceModuleDriver.EncryptionType` 取，与运行时解密一致。
- `UpdateSettings.ForceGenerateAtlas` 开启时先 `EditorSpriteSaveInfo.ForceGenerateAll` 重生成图集。
- 打完 AB 后 `CopyStreamingAssetsFiles` 受 `UpdateSettings.IsAutoAssetCopyToBuildAddress()` 控制，关闭则不复制到首包内置目录。

## Jenkins 落地

1. 节点安装对应 Unity 版本，改 `path_define` 的 `WORKSPACE`/`UNITYEDITOR_PATH`。
2. 拉取代码 → 转表（`GameConfig` 导表到 `BundleAssets/Configs/Binary/`，见 luban-dev）→ 如为首包或 AOT 变动且启用 HybridCLR，先执行 `HybridCLR/Generate/All` → 调用对应 `build_*` 脚本。
3. `-quit` 后用退出码判断成功失败，`BUILD_LOGFILE` 归档；`.sh` 末尾的 `read` 交互仅本地用，CI 节点用 `_auto` 脚本或去掉暂停。
4. 首次或改 AOT 后必须先执行 HybridCLR GenerateAll，再出一次整包，最后打 AB——裁剪后的 AOT DLL 在 `BuildPlayer` 时才生成（见 [hotfix-workflow.md](hotfix-workflow.md) AOT 泛型补充步骤）。

## 常见错误

| 错误 | 正确做法 |
|------|---------|
| 打包前没转表 | 任何 AB/整包构建前先转表，确保 `BundleAssets/Configs/Binary/` 是最新配置 |
| 首包或 AOT 变动后未跑 GenerateAll | 启用 HybridCLR 热更新时先执行 `HybridCLR/Generate/All`，再 BuildPlayer / BuildDll / AB |
| 命令行直接打 AB 未先出整包 | 先 `BuildPlayer` 生成裁剪 AOT DLL，再打 AB |
| Jenkins 用 `_manual` 脚本 | 无人值守用 `_auto`，`_manual` 会阻塞等版本输入 |
| 改了 `path_define_tmp` 期待生效 | 实际读 `path_define.bat`，改这个 |
| AB 打完真机加载不到 | 确认 `IsAutoAssetCopyToBuildAddress` 已开，AB 已复制到首包内置目录 |
| 手动打包漏传版本 | `*WithVersion` 方法缺 `-version` 会直接报错返回 |

## 交叉引用

| 关联主题 | 文档 |
|---------|------|
| 热更 DLL 编译与 AOT metadata | [hotfix-workflow.md](hotfix-workflow.md) |
| YooAsset 包版本、下载器、缓存清理 | [hotpatch-workflow.md](hotpatch-workflow.md) |
| 资源加载与 BundleAssets 落位 | [resource-api.md](resource-api.md) |
