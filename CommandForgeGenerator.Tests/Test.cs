using System.IO;
using CommandForgeGenerator.Generator.CodeGenerate;
using CommandForgeGenerator.Generator.Json;
using CommandForgeGenerator.Generator.Semantic;
using UnityEngine;
using Xunit;

namespace CommandForgeGenerator.Tests;

public class Test
{
    [Fact]
    public void JsonTokenizerTest()
    {
        var json = """
                   {
                       "hoge": "fuga", 
                       "piyo": [
                           "puyo", 
                           "poyo"
                       ]
                   }
                   """;

        Token[] answer =
        [
            new(TokenType.LBrace, "{"),
            new(TokenType.String, "hoge"),
            new(TokenType.Colon, ":"),
            new(TokenType.String, "fuga"),
            new(TokenType.Comma, ","),
            new(TokenType.String, "piyo"),
            new(TokenType.Colon, ":"),
            new(TokenType.LSquare, "["),
            new(TokenType.String, "puyo"),
            new(TokenType.Comma, ","),
            new(TokenType.String, "poyo"),
            new(TokenType.RSquare, "]"),
            new(TokenType.RBrace, "}")
        ];
        var tokens = JsonTokenizer.GetTokens(json);
        Assert.Equivalent(tokens, answer, true);
    }

    [Fact]
    public void JsonParserTest()
    {
        var json = """
                   {
                       "hoge": "fuga", 
                       "piyo": [
                           "puyo", 
                           "poyo"
                       ]
                   }
                   """;
    }
    
    [Fact]
    public void GenerateTest()
    {
        var yaml = GetSampleYaml();
        var commandsSchema = CommandSemanticsLoader.GetCommandSemantics(yaml);
        var codeFiles = CodeGenerator.Generate(commandsSchema);
        
        Assert.Equal(16, codeFiles.Count);
        
        #region Internal
        
        string GetSampleYaml()
        {
            return """
                   version: 1
                   commands:
                     - id: text
                       label: テキスト
                       description: 台詞を表示
                       commandListLabelFormat: "{character}「{body}」"
                       properties:
                         character:
                           type: enum
                           options: ["キャラA", "キャラB", "キャラC", "キャラD", "先生", "店員"]
                           required: true
                         body:
                           type: string
                           multiline: true
                           required: true
                   
                     - id: emote
                       label: エモート
                       description: 立ち絵・表情切替
                       commandListLabelFormat: "EMOTE: {character}, {emotion}"
                       properties:
                         character:
                           type: enum
                           options: ["キャラA", "キャラB", "キャラC", "キャラD", "先生", "店員"]
                           required: true
                         emotion:
                           type: enum
                           options: ["通常", "笑顔", "驚き", "怒り", "悲しみ", "困惑", "照れ", "恐怖", "喜び", "真剣"]
                           required: true
                   
                     - id: wait
                       label: 待機
                       description: 指定秒数だけウェイト
                       commandListLabelFormat: "WAIT: {seconds}"
                       defaultBackgroundColor: '#57e317'
                       properties:
                         seconds:
                           type: number
                           default: 0.5
                           constraints:
                             min: 0
                   
                     - id: bgm
                       label: BGM
                       description: 背景音楽を変更
                       commandListLabelFormat: "BGM: {track}, volume={volume}"
                       properties:
                         track:
                           type: enum
                           options: ["なし", "日常", "緊張", "悲しい", "楽しい", "神秘的", "アクション", "ロマンティック", "エンディング"]
                           required: true
                         volume:
                           type: number
                           default: 1.0
                           constraints:
                             min: 0
                             max: 1.0
                   
                     - id: sound
                       label: 効果音
                       description: 効果音を再生
                       commandListLabelFormat: "SOUND: {effect}, volume={volume}"
                       properties:
                         effect:
                           type: enum
                           options: ["ドア", "足音", "衝撃", "爆発", "鐘", "拍手", "警報", "雨", "雷", "風"]
                           required: true
                         volume:
                           type: number
                           default: 1.0
                           constraints:
                             min: 0
                             max: 1.0
                   
                     - id: background
                       label: 背景
                       description: 背景画像を変更
                       commandListLabelFormat: "BG: {scene}, effect={transition}"
                       properties:
                         scene:
                           type: enum
                           options: ["教室", "廊下", "体育館", "屋上", "公園", "駅", "カフェ", "自宅", "図書館", "商店街"]
                           required: true
                         transition:
                           type: enum
                           options: ["なし", "フェード", "ワイプ", "クロスフェード", "フラッシュ"]
                           default: "なし"
                   
                     - id: camera
                       label: カメラ
                       description: カメラワークを指定
                       commandListLabelFormat: "CAMERA: {action}, target={target}"
                       properties:
                         action:
                           type: enum
                           options: ["ズームイン", "ズームアウト", "パン左", "パン右", "シェイク", "フォーカス", "リセット"]
                           required: true
                         target:
                           type: enum
                           options: ["全体", "キャラA", "キャラB", "キャラC", "キャラD", "先生", "店員", "背景"]
                           default: "全体"
                   
                     - id: choice
                       label: 選択肢
                       description: 選択肢を表示
                       commandListLabelFormat: "CHOICE: {options}"
                       properties:
                         options:
                           type: string
                           multiline: true
                           description: "選択肢を1行に1つずつ記述"
                           required: true
                         timeout:
                           type: number
                           default: 0
                           description: "自動選択までの秒数（0で無制限）"
                   
                     - id: action
                       label: アクション
                       description: キャラクターのアクションを実行
                       commandListLabelFormat: "ACTION: {character}, {action}"
                       properties:
                         character:
                           type: enum
                           options: ["キャラA", "キャラB", "キャラC", "キャラD", "先生", "店員"]
                           required: true
                         action:
                           type: enum
                           options: ["歩く", "走る", "座る", "立つ", "ジャンプ", "踊る", "倒れる", "手を振る", "指さす", "抱きしめる"]
                           required: true
                         direction:
                           type: enum
                           options: ["左", "右", "上", "下", "中央"]
                           default: "中央"
                   
                     - id: narration
                       label: ナレーション
                       description: ナレーションテキストを表示
                       commandListLabelFormat: "NARRATION: {text}"
                       properties:
                         text:
                           type: string
                           multiline: true
                           required: true
                         style:
                           type: enum
                           options: ["通常", "強調", "小さく", "斜体", "点滅"]
                           default: "通常"
                   
                     - id: branch
                       label: 分岐
                       description: 他のコマンドを参照する分岐
                       commandListLabelFormat: "BRANCH: Target {targetCommand}"
                       defaultBackgroundColor: "#f9f0ff"
                       properties:
                         targetCommand:
                           type: command
                           required: true
                           commandTypes: ["text", "narration"] # Only allow text and narration commands
                         condition:
                           type: string
                           required: true
                           multiline: true
                   """;
        }
        
        #endregion

    }
}
