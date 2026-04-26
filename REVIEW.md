# Mofucat.TcpService Review

実施日: 2026-04-26

## 対象

- ライブラリ本体: `Mofucat.TcpService/`
- サンプル実装: `Example/`
- ビルド確認: `dotnet build`

現在のワークスペースは `Mofucat.TcpService` という NuGet パッケージ用ライブラリと、利用例を示す `Example` の 2 プロジェクト構成です。

## 現状サマリー

- `AddTcpService(Action<TcpServiceOptions>)` は複数回呼び出しても 1 つの hosted service に設定が累積される
- `TcpServiceOptions` から `Listen<T>()` / `ListenLocalhost<T>()` / `ListenAnyIP<T>()` を使って `ConnectionHandler` をバインドできる
- 各 `Listen*` に `Action<ListenOptions>` overload があり、endpoint ごとの追加構成ができる
- `SocketTransportOptions` は `TransportOptions` 経由で直接調整できる
- README は `Mofucat.TcpService` 名義に統一され、英語で最新仕様を説明する構成へ更新された

## Findings

### 1. [Low] graceful shutdown の意図が既定値だけでは伝わりにくい

- 対象: `Mofucat.TcpService/TcpService.cs:15`, `Mofucat.TcpService/TcpService.cs:40-51`, `Mofucat.TcpService/TcpServiceOptions.cs:15`
- `GracefulShutdown` が `false` の場合、停止時にはキャンセル済みトークンを使って Kestrel 側の停止を進める実装です。
- 挙動自体は API として表現されていますが、公開ドキュメントが薄いと利用者は既定の停止動作を理解しづらくなります。
- ドキュメント上で「既定値」「用途」「有効化する場面」を明記しておくと誤用を減らせます。

## 機能強化ポイント

### 1. API の拡張余地

- endpoint 未登録時の起動失敗を明示的な例外として補足できると診断性が上がる
- `ListenUnixSocket` など Kestrel の他 endpoint 形態への対応余地がある

### 2. テスト整備

- 現状はテストプロジェクトが見当たらないため、少なくとも `TcpServiceOptions` の endpoint 登録と `GracefulShutdown` 挙動差分をカバーする回帰テストが欲しい
- サンプルの `SampleHandler` についても、`get` / `set` / `exit` のプロトコル動作を最小限テストできると README の信頼性が上がる

## 補足

- `Example` は非常に小さく、ライブラリの意図を把握しやすい構成です
- 旧 README は別機能を含む過去の内容だったため、現行の `Mofucat.TcpService` に合わせた公開ドキュメントへの更新が優先度高です
