using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;

public static class StringUtils
{

    static private System.Text.StringBuilder s_builder = new System.Text.StringBuilder();

    //-------------------------------------------------------------------------
    //
    static public string Append(params string[] strs)
    {
        s_builder.Remove(0, s_builder.Length);

        for (int i = 0; i < strs.Length; ++i)
        {
            if (string.IsNullOrEmpty(strs[i]))
                continue;

            s_builder.Append(strs[i]);
        }

        return s_builder.ToString();
    }

    //-------------------------------------------------------------------------
    //
    public static string AsCurrency(this int value)
    {
        //return value.AsCurrency(CultureInfo.CurrentCulture);
        return value.ToString("#,##0");
    }

    /*
    //-------------------------------------------------------------------------
    //
    public static string AsCurrency(this int value, CultureInfo culture)
    {
        decimal result = value / 100m;
        //return result.ToString("c", culture);
        return result.ToString("N0", culture);
    }*/

    //-------------------------------------------------------------------------
    //
    public static string AsLapTime(long tick)
    {
        DateTime elapsedTime = new DateTime(tick);

        return elapsedTime.ToString("HH:mm:ss");
    }

    public static string Replace(string source, char specifier, string args)
    {
        string[] paramArr = new string[1];
        paramArr[0] = args;

        return Replace(source, specifier, paramArr);
    }

    public static string Replace(string source, char specifier, params string[] args)
    {
        string _message = source;

        int nStart = 0;
        int nEnd = 0;

        List<string> listParse = new List<string>();


        for (int i = 0; i < _message.Length - 1; ++i)
        {
            if (_message[i] == specifier && _message[i + 1] == specifier)
            {
                nStart = i;
                ++i;

                for (; i < _message.Length - 1; ++i)
                {
                    if (_message[i] == specifier && _message[i + 1] == specifier)
                    {
                        nEnd = i + 1;
                        i++;
                        string subString = _message.Substring(nStart, nEnd - nStart + 1);
                        listParse.Add(subString);

                        nStart = 0;
                        nEnd = 0;

                        break;
                    }
                }
            }
        }

        for (int i = 0; i < listParse.Count; ++i)
        {
            if (args.Length < i + 1)
                break;

            try
            {
                string replaceString = "";// HavanaString.AutoAddKoreanPostPosition(TrimSpecifier(listParse[i]), args[i]);
                _message = _message.Replace(listParse[i], replaceString);
            }
            catch (Exception e)
            {
                Debug.Log("Replacer Error(" + listParse[i] + ") - " + e.ToString());
            }
        }


        return _message;
    }

    static string TrimSpecifier(string source)  // 시작과 끝의 두글자씩 잘라낸다
    {
        if (source == null)
        {
            Debug.LogError("str source is null");
            return source;
        }

        if (source.Length < 4)
        {
            Debug.LogError("invalid str source");
            return source;
        }

        return source.Substring(2, source.Length - 4);
    }


    public const string StrTagEnder = "[-]";
    public static string ColorToTagStart(Color srcCol)
    {
        int iR = (int)(255.0f * srcCol.r);
        int iG = (int)(255.0f * srcCol.g);
        int iB = (int)(255.0f * srcCol.b);

        return string.Format("[{0}{1}{2}]", iR.ToString("X2"), iG.ToString("X2"), iB.ToString("X2"));
    }

    public static string ColorCoating(string srcStr, Color coatCol)
    {
        return Append(ColorToTagStart(coatCol), srcStr, StrTagEnder);
    }
}
