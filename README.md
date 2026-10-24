# AtCoder Study

AtCoder Problems のデータを使って、複数コンテストから学習する問題を探すC#のリポジトリです。問題ごとに自分の解答、考察、模範解答との比較を保存します。

## 必要なもの

- .NET 10 SDK
- AtCoderのユーザーID

まず `appsettings.example.json` を `appsettings.local.json` にコピーし、`AtCoder.UserId` に自分のAtCoderユーザーIDを書きます。`appsettings.local.json` はGitに含まれません。

```bash
cp appsettings.example.json appsettings.local.json
```

```json
{
  "AtCoder": {
    "UserId": "YOUR_ATCODER_ID"
  }
}
```

以下のコマンドはリポジトリのルートから実行します。

```bash
dotnet run --project tools/AtCoderStudy -- help
dotnet run --project tools/AtCoderStudy -- sync
```

設定ファイルを手で編集する代わりに、`dotnet run --project tools/AtCoderStudy -- config set user YOUR_ATCODER_ID` でも作成できます。`config show` で読み込まれたIDを確認できます。以前の `.study/config.json` がある場合は、`appsettings.local.json` がないときだけ読み込みます。

`sync` は[AtCoder Problems API](https://github.com/kenkoooo/AtCoderProblems/blob/main/doc/api.md)から問題一覧、コンテストと問題の対応、推定難易度、自分の提出履歴を取得します。APIの注意書きに従ってアクセス間隔を1秒より長く取り、データを `.study/cache/` に保存します。`.study/` はGitの管理対象外です。提出履歴はユーザーIDごとに保存します。データを更新したいときだけ `sync` を再実行してください。

実行環境が外部接続を制限していると、`kenkoooo.com:443` への接続が拒否されます。その場合はネットワーク接続を許可した環境で `sync` を実行してください。問題IDが分かっていれば、同期前でも下記の `start` でフォルダを作れます。

## 問題を探す

```bash
# ABCのA・Bから、未着手の問題を新しいコンテスト順に20件表示
dotnet run --project tools/AtCoderStudy -- find

# 推定難易度400以下、難易度順
dotnet run --project tools/AtCoderStudy -- find --series abc --letters A,B --status new --difficulty-max 400 --sort difficulty

# 一度提出して未ACの問題も含める
dotnet run --project tools/AtCoderStudy -- find --status unaccepted --limit 30

# ARCのAを検索
dotnet run --project tools/AtCoderStudy -- find --series arc --letters A
```

| オプション | 内容 | 既定値 |
| --- | --- | --- |
| `--series abc\|arc\|all` | コンテスト系列 | `abc` |
| `--letters A,B` | 問題番号 | `A,B` |
| `--status new\|unaccepted\|all` | 問題の状態 | `new` |
| `--difficulty-min N` | 推定難易度の下限 | なし |
| `--difficulty-max N` | 推定難易度の上限 | なし |
| `--sort newest\|difficulty` | 並び順 | `newest` |
| `--limit N` | 表示件数（1～500） | `20` |

`new` は提出履歴がなく、このリポジトリにも問題フォルダがない状態です。`unaccepted` は未ACで、未着手・フォルダ作成済み・WAなどの提出済みを含みます。難易度の条件を指定した場合、推定難易度がない問題は結果から除外します。推定難易度はAtCoder Problemsの推定値で、A・Bなどの問題番号とは別の条件です。

## 問題を始める

```bash
dotnet run --project tools/AtCoderStudy -- start abc300_b
```

作成されるフォルダは次の形です。既存のフォルダやファイルは上書きしません。

同期済みの問題情報がない場合も、`abc480_a` のような問題IDからコンテストIDを判定してフォルダを作成します。この場合の問題名と難易度は未確認として記録するため、公式の問題ページで確認してください。

```text
problems/abc300/abc300_b/
├── README.md        # 問題リンク、要約、自分の方針と振り返り
├── Program.cs       # 自分のC#解答
├── Solution.csproj  # ローカル実行用
└── editorial.md     # 模範解答の要点、参考リンク、自分の解答との比較
```

解答は `dotnet run --project problems/abc300/abc300_b/Solution.csproj` で実行できます。AtCoderへ提出するときは `Program.cs` のコードを提出します。`README.md` に問題文の要約を自分の言葉で記録し、問題本文と模範解答の参照先には公式リンクを使います。

## 公式問題文を保存する

AtCoder Problems APIは問題文を提供していません。`statement` コマンドはAtCoderの公式問題ページから日本語の問題文だけを取得し、数式のLaTeX記法を残して問題フォルダの `statement.md` に保存します。フォルダがまだなければ作成します。

```bash
dotnet run --project tools/AtCoderStudy -- statement abc480_a
```

`statement.md` はMarkdownプレビューで読めるローカルの控えで、再実行すると更新されます。問題文の権利は[AtCoderの利用規約](https://atcoder.jp/tos)に記載されているため、このファイルはGitの管理対象外です。共有する記録には公式URLと自分の言葉による要約を残します。

## AIで学ぶ

リポジトリ内の [`atcoder-learning` スキル](.agents/skills/atcoder-learning/SKILL.md)は、解く前の段階的なヒント、提出後のC#コードレビュー、解説を読んだ後の振り返りを支援します。Codexでは `$atcoder-learning` を指定して呼び出せます。例えば「`$atcoder-learning abc300_b の方針についてヒントを一つください`」と依頼します。

AIによる解法支援は過去問の練習で使います。開催中のABC・ARC・AGCには[AtCoderの生成AI利用ルール](https://info.atcoder.jp/entry/llm-rules-ja)が適用されます。

## CLIの構成

`tools/AtCoderStudy` は1つの.NETプロジェクトで、役割ごとにファイルを分けています。

| 場所 | 役割 |
| --- | --- |
| `Domain/` | 問題・提出のモデル、未着手判定、検索と並び替え |
| `Application/` | `sync`・`find`・`start` の処理と外部機能のインターフェース |
| `Infrastructure/` | AtCoder Problems API、JSONキャッシュ、問題フォルダの作成 |
| `Cli/` | 引数の解釈、結果とヘルプの表示 |
| `Program.cs` | 各層の接続とエラー表示 |

問題の選び方は `Domain/ProblemCatalog.cs`、APIや保存先の変更は `Infrastructure/`、コマンドの追加は `Cli/StudyCli.cs` から確認できます。
