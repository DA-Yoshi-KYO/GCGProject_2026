/*
+=====================================
 ファイル名 : CSED_CreateTools_FieldDataSaveData.cs
 概要     : CreateToolsのFieldData復元用保存データ
 作者     : ヨシモト リョウ
 履歴     : 2026/05/20 新規作成
=====================================+
*/

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CreateToolsのFieldDataを保存・復元するためのデータです。
/// </summary>
[Serializable]
public class CSED_CreateTools_FieldDataSaveData
{
    public CSE_CreateTools_FieldType fieldType;
    public string fieldName;
    public CSE_CreateTools_FieldLayoutType fieldLayoutType;
    public string tagName;

    public CSE_CreateTools_FieldType listElementFieldType;
    public bool isListDefaultValueNull;
    public bool isListDefaultCountNull;
    public string listDefaultCountText;

    public bool isPublicSetter;
    public string tooltipText;

    public bool isDefaultValueNull;
    public string defaultValueText;

    public bool isDefaultMinValueNull;
    public string defaultMinValueText;

    public bool isDefaultMaxValueNull;
    public string defaultMaxValueText;

    public bool isSliderMinValueNull;
    public string sliderMinValueText;

    public bool isSliderMaxValueNull;
    public string sliderMaxValueText;

    public MonoScript scriptableObjectTypeScript;
    public MonoScript enumTypeScript;
    public ScriptableObject defaultScriptableObjectValue;
    public MonoScript defaultScriptValue;
    public GameObject defaultGameObjectValue;

    public List<string> listDefaultElementValueTextList = new List<string>();
    public List<string> listDefaultMinValueTextList = new List<string>();
    public List<string> listDefaultMaxValueTextList = new List<string>();
    public List<UnityEngine.Object> listDefaultObjectValueList = new List<UnityEngine.Object>();
}
#endif
