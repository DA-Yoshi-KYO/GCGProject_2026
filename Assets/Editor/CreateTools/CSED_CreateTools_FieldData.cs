/*
+=====================================
 ファイル名 : CSED_CreateTools_FieldData.cs
 概要     : CreateToolsで中央エリアに配置する変数データ
 作者     : ヨシモト リョウ
 履歴     : 2026/05/08 新規作成
            2026/05/08 表示レイアウト情報を追加
            2026/05/13 Default設定とSlider設定用データを追加
=====================================+
*/

#if UNITY_EDITOR

/// <summary>
/// CreateToolsで扱う変数1つ分のデータです。
/// </summary>
public class CSED_CreateTools_FieldData
{
    /// <summary>
    /// Sliderの最小値をnull扱いにするかどうかです。
    /// </summary>
    public bool IsSliderMinValueNull { get; set; }

    /// <summary>
    /// Sliderの最小値です。
    /// </summary>
    public string SliderMinValueText { get; set; }

    /// <summary>
    /// 変数の型です。
    /// </summary>
    public CSE_CreateTools_FieldType FieldType { get; set; }

    /// <summary>
    /// 変数名です。
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// 表示レイアウト種別です。
    /// </summary>
    public CSE_CreateTools_FieldLayoutType FieldLayoutType { get; set; }

    /// <summary>
    /// Tag名です。
    /// </summary>
    public string TagName { get; set; }

    /// <summary>
    /// 通常初期値をnull扱いにするかどうかです。
    /// </summary>
    public bool IsDefaultValueNull { get; set; }

    /// <summary>
    /// 通常初期値です。
    /// </summary>
    public string DefaultValueText { get; set; }

    /// <summary>
    /// 最小初期値をnull扱いにするかどうかです。
    /// </summary>
    public bool IsDefaultMinValueNull { get; set; }

    /// <summary>
    /// 最小初期値です。
    /// </summary>
    public string DefaultMinValueText { get; set; }

    /// <summary>
    /// 最大初期値をnull扱いにするかどうかです。
    /// </summary>
    public bool IsDefaultMaxValueNull { get; set; }

    /// <summary>
    /// 最大初期値です。
    /// </summary>
    public string DefaultMaxValueText { get; set; }

    /// <summary>
    /// Sliderの最大値をnull扱いにするかどうかです。
    /// </summary>
    public bool IsSliderMaxValueNull { get; set; }

    /// <summary>
    /// Sliderの最大値です。
    /// </summary>
    public string SliderMaxValueText { get; set; }

    /// <summary>
    /// 変数データを作成します。
    /// </summary>
    /// <param name="f_fieldType">変数の型</param>
    /// <param name="f_fieldName">変数名</param>
    /// <param name="f_fieldLayoutType">表示レイアウト種別</param>
    public CSED_CreateTools_FieldData(
        CSE_CreateTools_FieldType f_fieldType,
        string f_fieldName,
        CSE_CreateTools_FieldLayoutType f_fieldLayoutType)
    {
        FieldType = f_fieldType;
        FieldName = f_fieldName;
        FieldLayoutType = f_fieldLayoutType;
        TagName = f_fieldName;

        IsSliderMinValueNull = true;
        SliderMinValueText = string.Empty;

        IsSliderMaxValueNull = true;
        SliderMaxValueText = string.Empty;

        IsDefaultValueNull = true;
        DefaultValueText = string.Empty;

        IsDefaultMinValueNull = true;
        DefaultMinValueText = string.Empty;

        IsDefaultMaxValueNull = true;
        DefaultMaxValueText = string.Empty;

        IsSliderMaxValueNull = true;
        SliderMaxValueText = string.Empty;
    }
}
#endif
