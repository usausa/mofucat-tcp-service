# Mofucat.TcpServer Review

実施日: 2026-04-25

## 対象

- ライブラリ本体: `Mofucat.TcpServer/`
- サンプル実装: `Example/`
- ビルド確認: `dotnet build Mofucat.TcpServer.slnx -v minimal`

ビルドは成功し、警告 0 / エラー 0 でした。

## Findings

### 1. [High] 強制停止が既定値になっており、通常停止でも接続を急切断しやすい

- 対象: `Mofucat.TcpServer/TcpServerOptions.cs:15`, `Mofucat.TcpServer/TcpServerService.cs:32`, `Mofucat.TcpServer/TcpServerService.cs:40-50`
- `GracefulShutdown` の既定値は `false` です。そのため `StopAsync()` ではキャンセル済みトークンを `KestrelServer.StopAsync()` に渡す分岐が既定で選ばれます。
- Generic Host 上の `IHostedService` としては、明示的な理由がない限り「停止要求時は graceful に drain する」挙動のほうが自然です。現状だと、利用者が `GracefulShutdown = true` を知らないまま使うと、接続中クライアントが応答途中で切られる可能性があります。
- 優先度の高い改善として、既定値を graceful 側に寄せるか、`ShutdownMode` のような明示的な enum にして意図を表現したほうが安全です。

### 2. [Medium] `AddTcpServer()` を複数回呼んでも設定が合成されず、後勝ちになりやすい

- 対象: `Mofucat.TcpServer/ServiceCollectionExtensions.cs:8-12`
- 登録しているのは単一の `Action<TcpServerOptions>` なので、複数モジュールから `AddTcpServer()` を呼ぶ構成にすると、最終的に解決される delegate は 1 つだけになります。
- この設計だと、アプリ本体と別パッケージの両方がエンドポイント追加を試みたときに、片方の listener 設定が静かに落ちるリスクがあります。
- `IConfigureOptions` 相当の積み上げ方式か、内部で `IEnumerable<Action<TcpServerOptions>>` を順番に適用する形に変えると、モジュール分割しやすくなります。

### 3. [Medium] 現在の公開 API では Kestrel の強みをかなり使い切れない

- 対象: `Mofucat.TcpServer/TcpServerOptions.cs:23-53`
- `Listen*<T>()` が内部で `Protocols = HttpProtocols.None` と `UseConnectionHandler<T>()` を固定しているため、利用者は `ListenOptions` に対して追加設定できません。
- その結果、TLS (`UseHttps()`)、接続ログ、backlog、ソケット単位の詳細設定、接続 middleware の追加など、Kestrel 側の拡張点を公開 API から触れません。
- 「TCP サーバーを最短で立ち上げる」目的には合っていますが、ライブラリとしては伸びしろを自ら狭めています。少なくとも `Action<ListenOptions>` を受け取る overload は欲しいです。

## 機能強化ポイント

### 1. 設定モデルの拡張

- `AddTcpServer(Action<TcpServerOptions>)` に加えて、複数回呼んでも累積される登録方式を用意する
- エンドポイント未登録時は `StartAsync()` 前に明示的な例外を投げる
- `GracefulShutdown` を `ShutdownMode` や `StopTimeout` 付き設定に分解して、意図を API に出す

### 2. Kestrel 機能の露出

- `Listen<T>(..., Action<ListenOptions> configure)` を追加して、TLS や connection middleware を有効化できるようにする
- `ListenUnixSocket` や既存ソケット受け取りなど、HTTP 以外の Kestrel endpoint も検討する
- `SocketTransportOptions` だけでなく、エンドポイント単位の細かな調整ポイントも公開する

### 3. 運用性の強化

- 接続数、切断数、例外数のログやメトリクスを出しやすいフックを用意する
- 起動時に「どの endpoint を listen したか」を構造化ログで出す
- 接続ごとの idle timeout、最大同時接続数、最大フレーム長のような保護機能を helper 側で持てると実運用しやすい

### 4. テストとサンプルの整備

- 現状はテストプロジェクトが見当たらないため、`ConnectionHandler` ベースの疎通テストを追加したい
- `GracefulShutdown` の挙動差分、複数 endpoint 登録、異常系切断を最低限の回帰テストに入れる
- `README.md` はほぼ空で、`README.old.md` には別機能の記述が混ざっているため、公開用ドキュメントは早めに整理したい

## 補足

- サンプルの `Example/Handlers/SampleHandler.cs` は最小構成として分かりやすい一方、プロトコル終端やタイムアウト時の扱いは簡略化されています。
- ライブラリ本体が非常に小さいので、今のうちに API 方向性を固めると後方互換の負債を抑えやすいです。
