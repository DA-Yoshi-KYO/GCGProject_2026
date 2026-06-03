/*
+=====================================
 ファイル名 : CS_EffectPlayAttribute.cs
 概要     : Effect管理システムから再生時に呼び出すメソッドを指定する属性
 作者     : ヨシモト リョウ
 履歴     : 2026/06/03 新規作成
=====================================+
*/

using System;

/// <summary>
/// Effect管理システムから再生時に呼び出すメソッドを示す属性です。
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CS_EffectPlayAttribute : Attribute
{
}
