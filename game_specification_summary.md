```markdown
# HTO - ゲーム仕様概要

## 🎮 ゲーム概要
ヘックスベースのオンラインストラテジーRPGです。
6人パーティでダンジョンを攻略し、「ヘックスピース」を集めてキャラクターを強化していきます。

## ロビーシステム
- 6人パーティ（プレイヤーまたはBOT）を集める事が目的
- 部屋主は空席にロールを選択して、プレイヤーが入ってくるのを待つ
- Botで空席を埋める事も可能

## ダンジョンシステム
- ヘックス20x20ぐらいの自動生成マップ 
- 視界や遮蔽物の概念あり
- トラップなどのギミックの概念あり
- 基礎難易度が一定を超えると、一人当たり通貨(ヘックスエッセンス)の支払いをゲーム開始に必要とする(ベッド金)
- 同じ総レベル設定で、同一ルーム(部屋主が変わっても良い)で攻略に出るとフロアカウントが上がり、敵と報酬が強化されていく(概念としては同一ダンジョンを降りている)
- 全滅するとフロアカウントが1に戻る(全滅により失うモノはベッド金)

### ダンジョン攻略報酬
- ヘックスピース
- ヘックスエッセンス(通貨)
- 経験値(プレイヤーレベルの向上により、キャンバスが拡張にされる)

## 🧩 パズルゲーム要素 ヘックスピースシステム

- プレイヤーのヘックスキャンバスに配置する事でキャラクターが強化される
- 形状と色が重要な要素
- ピースの組み合わせで戦略が変化
- ヘックスエッセンスを使用する事で改造が可能

## 📈 成長要素

- よりシナジーの高いヘックスピース配置を実現する為のハスクラ成長
- レベルに応じたヘックスキャンバス配置可能数の拡張

## 💰 ヘックスエッセンスとマーケットシステム

- プレイヤー間でのヘックスピースとヘックスエッセンスの取引が可能
- ヘックスピースの改造アイテムが通貨として機能 (Path of Exile経済モデル)
- ダンジョンの難易度等により、手に入りやすいヘックスエッセンスが異なる。(市場原理であるダンジョン難易度の設定が分散する)

## 🎨 スキン要素
- UGCベースのスキンシステム
- AIによる内容の審査
- スキルコインを使用して、プレイヤー間での利用権の取引が可能 (RobroxのRobux経済参照)

## ⚙️ その他の特徴
- 地域制限なし
- pingの影響を受けにくい設計
- 多言語対応
- カジュアルなプレイ時間
```

