# LiquidPVR セットアップと CI 運用

最終確認日: 2026-09-17。対象は Unity **6000.6.0f1**、URP、AR Foundation / Apple ARKit XR Plugin **6.5.0-pre.2**、XR Plug-in Management **4.5.3**、GameCI `unity-builder@v5.0.0` です。

## アーキテクチャ

```text
ARKit world tracking (iPhone rear camera)
                  │  6DoF pose
                  ▼
AR Camera / XR Origin ──► mono passthrough background
                  │
                  └────► CardboardStereoPresenter ──► left / right virtual-content cameras
                                                               │
                                                               ▼
                                                        Initial Field (Plane)
```

ARKit が位置と回転を更新し、`CardboardStereoPresenter` が IPD 64 mm の左右カメラを作ります。これにより仮想オブジェクトはステレオ視差を持ち、現実のカメラ背景は 1 回だけ描画されます。ARKit は端末を Cardboard に入れた状態では背面カメラを塞がないよう、開口部のあるビューアを使ってください。

## Unity 側の設定

`README.md` の Quick start を完了後、次を確認します。

ローカルではシーン編集・スクリプト編集だけを行い、iOS の Xcode プロジェクト生成と IPA 化を GameCI に任せる場合、Unity Hub の **iOS Build Support は不要**です。GameCI の iOS ターゲット用 Unity コンテナが必要なビルドモジュールを持ちます。ローカルで iOS への切替・ビルド・XR Plug-in Management の iOS タブを操作する場合にのみ、iOS Build Support を追加してください。

- iOS の XR Plug-in Management で ARKit が有効。
- Player Settings の Minimum iOS Version は **16.0** 以上、Architecture は ARM64、Graphics API は Metal。
- `NSCameraUsageDescription` が設定済み。
- `LiquidPVRField` に `AR Session`、`XR Origin (Mobile AR)`、AR Camera、`CardboardStereoPresenter`、`Initial Field` がある。
- 最初のフィールドは `VirtualContent`（レイヤー 8）であり、左右カメラだけが描画する。

ARKit の有効化は `ProjectSettings.asset` に固定せず Unity Editor の XR Plug-in Management で行います。これは XR 管理設定アセットを Unity が使用中のパッケージ版に合わせて生成するためです。生成された `ProjectSettings/XRPackageSettings.asset` はコミットしてください。

Package Manager の解決後に `Packages/packages-lock.json` が更新された場合も必ずコミットしてください。パッケージの再現可能な CI ビルドに必要です。

## 実機テスト

1. 十分に明るく、模様のある空間で起動し、ARKit の初期化を待ちます。
2. 端末をゆっくり前後左右に動かし、Plane が回転だけではなく位置に対して安定することを確認します。
3. Cardboard に入れ、左右の Plane に適切な視差があることを確認します。
4. カメラ許可拒否・ARKit 非対応端末・暗所では追跡不能になることを確認します。

これは安全配慮を要する視界を覆うアプリです。歩行中、車両内、階段・段差の近くでは使用しないでください。

## GameCI: 署名無し IPA

ワークフロー [ios-unsigned.yml](../.github/workflows/ios-unsigned.yml) は次の二段階です。

1. Ubuntu で GameCI が Unity から iOS Xcode プロジェクトをエクスポートする。
2. macOS で `xcodebuild` に `CODE_SIGNING_ALLOWED=NO` と `CODE_SIGNING_REQUIRED=NO` を渡し、`Payload/LiquidPVR.app` を zip 形式の `LiquidPVR-unsigned.ipa` にする。

### GitHub Secrets

| Secret | 値 |
| --- | --- |
| `UNITY_LICENSE` | 準備済み `.ulf` ファイルの**内容全体** |
| `UNITY_EMAIL` | その Unity ライセンスのアカウントメール |
| `UNITY_PASSWORD` | 同アカウントのパスワード |

`.ulf` をリポジトリーへ置いたりコミットしたりしません。GameCI のアクティベーションでは `.ulf` を Secret として渡します。

### サイドロード時の重要事項

生成 IPA は意図どおり**未署名**です。しかし iOS は未署名アプリを起動できません。AltStore / SideStore / LiveContainer など、利用する導入側がインストール時または実行時にその端末・Apple ID・証明書で署名／再署名できる場合にだけ使えます。CI での未署名化は App Store 用 IPA や「署名不要で直接インストールできる IPA」を作る意味ではありません。

Actions タブから `Build unsigned iOS IPA` を実行し、`LiquidPVR-unsigned-ipa` Artifact を取得します。導入ツール固有の再署名手順とプロビジョニング制限はそのツールの最新ドキュメントに従ってください。

## 参照（2026-09-17 確認）

- [Unity: Apple ARKit XR Plugin](https://docs.unity3d.com/ja/6000.0/Manual/com.unity.xr.arkit.html)
- [Unity: ARKit project configuration](https://docs.unity3d.com/ja/Packages/com.unity.xr.arkit%405.1/manual/project-configuration-arkit.html)
- [Unity: XR Origin](https://docs.unity3d.com/6000.0/Manual/xr-origin.html)
- [GameCI: activation](https://game.ci/docs/github/activation/)
- [GameCI: Builder](https://game.ci/docs/github/builder/)
