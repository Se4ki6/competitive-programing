# このリポジトリでの作業

AtCoderの問題をC#で解き、問題ごとの学習記録を残すリポジトリです。使い方とCLIの引数は [README.md](README.md) を参照してください。

## 構成

- `tools/AtCoderStudy/`: 問題検索、AtCoder Problems APIとの同期、問題フォルダ作成を行う.NET 10のCLI。
- `problems/<contest_id>/<problem_id>/`: 1問分の記録。`Program.cs` はユーザーの解答、`README.md` は問題の要約と考察、`editorial.md` は模範解答との比較に使います。
- `appsettings.local.json`: 個人設定。`AtCoder.UserId` を読み込みます。追跡する設定例は `appsettings.example.json` です。
- `.study/`: APIから取得したローカルキャッシュ。個人設定とキャッシュはGitに含めません。

## 問題を扱うとき

- 問題の検索とフォルダ作成には、可能な限り `dotnet run --project tools/AtCoderStudy -- find` と `start <問題ID>` を使ってください。
- APIに接続できない場合も、問題IDが分かれば `start` でフォルダを作れます。その場合は問題名と難易度が未確認であることを伝えてください。
- ユーザーの `Program.cs` を解説用の実装で上書きしないでください。解説の出典、要点、比較は同じ問題フォルダの `editorial.md` に記録します。
- 問題文と公式解説はAtCoderのページを参照し、問題フォルダにはリンクと自分の言葉による要約を残します。`statement <問題ID>` は公式問題文をローカルの `statement.md` に保存します。このファイルはGitに含めません。
- ヒント、解答レビュー、模範解答との比較、復習を支援するときは [atcoder-learning スキル](.agents/skills/atcoder-learning/SKILL.md) を使ってください。
- AIによる解法支援は過去問練習を対象とします。開催中のABC・ARC・AGCでは[AtCoderの生成AI利用ルール](https://info.atcoder.jp/entry/llm-rules-ja)に従ってください。

## CLIやAPI連携を変更するとき

- `tools/AtCoderStudy/Domain/` には問題のモデルと判定・検索処理を置き、HTTPやファイル操作を持ち込まないでください。
- `Application/` は処理の流れと外部機能のインターフェース、`Infrastructure/` はその実装、`Cli/` は引数と表示を担当します。`Program.cs` はそれらの接続に限定します。
- [AtCoder Problems APIの仕様](https://github.com/kenkoooo/AtCoderProblems/blob/main/doc/api.md)を確認してください。APIへのアクセスは1秒より長く間隔を空け、取得結果をローカルに保存します。
- CLIの変更後は `dotnet build tools/AtCoderStudy/AtCoderStudy.csproj` でコンパイルを確認してください。
