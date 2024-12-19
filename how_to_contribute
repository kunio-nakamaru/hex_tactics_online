# オープンソースプロジェクトへの貢献フロー

1. プロジェクトのフォーク（Fork）
   - メインリポジトリを自分のアカウントにフォークする
   - これにより自分専用のコピーが作成される

2. ローカル環境のセットアップ
   ```bash
   # フォークしたリポジトリをクローン
   git clone https://github.com/[your-username]/hex_tactics_online.git
   
   # 本家リポジトリを upstream として追加
   git remote add upstream https://github.com/kunio-nakamaru/hex_tactics_online.git
   ```

3. 開発用ブランチの作成
   ```bash
   # 最新の main ブランチに切り替え
   git checkout main
   
   # upstream から最新変更を取得
   git fetch upstream
   git merge upstream/main
   
   # 作業用ブランチを作成
   git checkout -b feature/new-feature
   ```

4. 開発作業
   - コーディング規約に従って開発
   - 適切な粒度でコミット
   ```bash
   git add .
   git commit -m "feat: 機能の追加"
   ```

5. 変更のプッシュ
   ```bash
   git push origin feature/new-feature
   ```

6. プルリクエスト（PR）の作成
   - GitHubやGitLabのWeb画面からPRを作成   

7. レビュー対応
   - レビューコメントに基づいて修正
   - 修正後は同じブランチに push
   ```bash
   git add .
   git commit -m "fix: レビュー指摘の修正"
   git push origin feature/new-feature
   ```

8. マージ後の後処理
   ```bash
   # mainブランチに戻る
   git checkout main
   
   # ローカルの main を更新
   git pull upstream main
   
   # 作業ブランチの削除
   git branch -d feature/new-feature
   ```
