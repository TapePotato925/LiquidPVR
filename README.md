# LiquidPVR (Liquid PhoneVR)

ARKit のワールドトラッキングを 6DoF の姿勢ソースにして、スマートフォンを Cardboard ビューアで使うための Unity 6 プロトタイプです。初期フィールドは仮想の `Plane` だけです。

> Cardboard 自体は 3DoF（回転）のみです。本プロジェクトの並進を含む 6DoF は、対応 iPhone の ARKit ワールドトラッキングから得ます。カメラ映像は単眼背景、仮想コンテンツは左右分割のステレオで重ねます。

## Quick start

1. Unity Hub で **Unity 6000.6.0f1** を入れ、このフォルダーを開きます。iOS ビルドを GameCI だけで行うなら、ローカルの **iOS Build Support は不要**です。
2. Package Manager が AR Foundation / ARKit / XR Plug-in Management を解決するまで待ちます。
3. `Edit > Project Settings > XR Plug-in Management > iOS` で **ARKit** を有効にします。Project Validation の修正候補はすべて適用してください。
4. メニューの `LiquidPVR > Create Initial AR/Cardboard Scene` を一度実行します。`Assets/Scenes/LiquidPVRField.unity` が作成され、Build Settings にも追加されます。
5. `Project Settings > Player > iOS` で Bundle Identifier を自分の一意な ID（例: `com.example.liquidpvr`）に変更します。Camera Usage Description はすでに設定済みです。
6. GitHub Actions の `Build unsigned iOS IPA` を実行し、生成された IPA をサイドロードツールで再署名して実機へ導入します。対応する Cardboard ビューアへ挿入します。

詳細、CI、検証項目は [docs/SETUP.md](docs/SETUP.md) を参照してください。

## Repository policy

Unity の生成物、署名証明書、`.ulf`、`.ipa` はコミットしません。GitHub Actions にはライセンスの本文を `UNITY_LICENSE` Secret として登録します。
