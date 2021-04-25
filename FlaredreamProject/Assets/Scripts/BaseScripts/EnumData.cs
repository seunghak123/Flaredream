using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eThrowEventType
{
    E_THROW_EVENT_TYPE_SET_TARGET,
    E_THROW_EVENT_TYPE_CHANGE_ANI_STATE,
    E_THROW_EVENT_TYPE_CHANGE_SKILL_STATE,
    E_THROW_EVENT_TYPE_CHANGE_SOCIAL_STATE,
    E_THROW_EVENT_TYPE_SELECT_SKILL_INDEX,
    E_THROW_EVENT_TYPE_SELECT_SKILL_TAG,
    E_THROW_EVENT_TYPE_SELECT_SKILL_ID,

    E_THROW_EVENT_TYPE_USE_ACTIVE_RUSH,

    E_THROW_EVENT_TYPE_USE_SKILL,
    E_THROW_EVENT_TYPE_USE_RESERVE_SKILL,
    E_THROW_EVENT_TYPE_USE_SOCIAL_SKILL,

    E_THROW_EVENT_TYPE_USE_ITEM,
    E_THROW_EVENT_TYPE_USE_LIST_ITEM,
    E_THROW_EVENT_TYPE_PREVIEW_ITEM,

    E_THROW_EVENT_TYPE_DETACH_EQUIP_ITEM_BY_SUBTYPE,
    E_THROW_EVENT_TYPE_DETACH_EQUIP_ITEM_BY_INSTANCE,

    E_THROW_EVENT_TYPE_CLEAR_EQUIP_ITEM,

    E_THROW_EVENT_TYPE_REFRESH_DEFAULT_PART_BY_SUBTYPE,
    E_THROW_EVENT_TYPE_REFRESH_DEFAULT_PARTS,

    E_THROW_EVENT_TYPE_REFRESH_COSTUME_PARTS,

    E_THROW_EVENT_TYPE_REFRESH_REINFORCE,

    E_THROW_EVENT_TYPE_SET_HP,
    E_THROW_EVENT_TYPE_INIT_HP,
    E_THROW_EVENT_TYPE_AUTO_AI,
    E_THROW_EVENT_TYPE_MOVE_AI,     //move만 on/off
    E_THROW_EVENT_TYPE_INVINCIBLE,
    E_THROW_EVENT_TYPE_FORCE_CROWD_CONTROL_SET,
    E_THROW_EVENT_TYPE_CROWD_CONTROL_SET,
    E_THROW_EVENT_TYPE_CROWD_CONTROL_INIT,
    E_THROW_EVENT_TYPE_SPECIAL_ATTACK,

    E_THROW_EVENT_TYPE_CHARACTER_REVIVAL,

    E_THROW_EVENT_TYPE_SET_SWAP_OBJECT,

    E_THROW_EVENT_TYPE_RUN_CONDITION_SKILL,
    E_THROW_EVENT_TYPE_RUN_CONDITION_SOCIALSKILL,

    E_THROW_EVENT_TYPE_RUSH_START,
    E_THROW_EVENT_TYPE_RUSH_END,

    E_THROW_EVENT_TYPE_ENABLE,
    E_THROW_EVENT_TYPE_DISABLE,

    E_THROW_EVENT_TYPE_ENABLE_MAIN_SOCIAL_SKILL,
    E_THROW_EVENT_TYPE_ENABLE_SUB_SOCIAL_SKILL,

    E_THROW_EVENT_TYPE_DISABLE_MAIN_SOCIAL,
    E_THROW_EVENT_TYPE_DISABLE_SUB_SOCIAL,

    E_THROW_EVENT_TYPE_DISABLE_TARGET_OBJECT,

    E_THROW_EVENT_TYPE_GAME_MODE,
    E_THROW_EVENT_TYPE_CLEAR_AI,

    E_THROW_EVENT_TYPE_RENDER_ENABLE,
    E_THROW_EVENT_TYPE_RENDER_DISABLE,

    E_THROW_EVENT_TYPE_ORIGINAL_TRANSFORM_SET,
    E_THROW_EVENT_TYPE_CHANGE_FACTOR,
}
public enum eCustomDataType
{
    //int
    E_CUSTOM_DATA_TYPE_CURRENT_HP,
    E_CUSTOM_DATA_TYPE_MAX_HP,
    E_CUSTOM_DATA_TYPE_LEVEL,
    E_CUSTOM_DATA_TYPE_MONSTER_SPAWN_ANI,



    //float
    E_CUSTOM_DATA_TYPE_ATTACK_RANGE,
    E_CUSTOM_DATA_TYPE_NEXT_ATTACK_RANGE,
    E_CUSTOM_DATA_TYPE_RUN_SPEED,

    E_CUSTOM_DATA_TYPE_ATTACK_SPEED,
    E_CUSTOM_DATA_TYPE_SEARCH_RANGE,
    E_CUSTOM_DATA_TYPE_DOT_STATE_LIFE_TIME,
    E_CUSTOM_DATA_TYPE_STATE_LIFE_TIME,
    E_CUSTOM_DATA_TYPE_DEFAULT_SIZE,
    E_CUSTOM_DATA_TYPE_AI_CRAYON_ANGER_REST_TIME,



    //bool
    E_CUSTOM_DATA_TYPE_DOT_STATE,
    E_CUSTOM_DATA_TYPE_SELECT_SKILL_FIRST_CONDITION,
    E_CUSTOM_DATA_TYPE_SELECT_SKILL_LAST_CONDITION,
    E_CUSTOM_DATA_TYPE_USE_SKILL_CHECK,
    E_CUSTOM_DATA_TYPE_NON_TARGET,
    E_CUSTOM_DATA_TYPE_IS_DOT_STATE, // 

    //object
    E_CUSTOM_DATA_TYPE_FACTOR_TABLE,
    E_CUSTOM_DATA_TYPE_TEAM,
    E_CUSTOM_DATA_TYPE_STATE,
    E_CUSTOM_DATA_TYPE_AI,
    E_CUSTOM_DATA_TYPE_BONE,
    E_CUSTOM_DATA_TYPE_SELECT_SKILL,
    E_CUSTOM_DATA_TYPE_SELECT_SKILL_NONTARGET,
    E_CUSTOM_DATA_TYPE_MASTER_SELECT_SKILL,
    E_CUSTOM_DATA_TYPE_TARGET,
    E_CUSTOM_DATA_TYPE_CHARACTER_OBJECT,
    E_CUSTOM_DATA_TYPE_SWAP_SET,
    E_CUSTOM_DATA_TYPE_EQUIPITEM_BY_SUBTYPE,
    E_CUSTOM_DATA_TYPE_MOVABLE,
    E_CUSTOM_DATA_TYPE_COLLIDER,
    E_CUSTOM_DATA_TYPE_ANIMATOR,
    E_CUSTOM_DATA_TYPE_ONCE_TARGET,
    E_CUSTOM_DATA_TYPE_SKILL_TARGET,
    E_CUSTOM_DATA_TYPE_AI_CONTROL_MODE,
    E_CUSTOM_DATA_TYPE_CURRENT_HAVANASTATE,
    E_CUSTOM_DATA_TYPE_FORCED_TARGET,


    // stats

    // select skill







    E_CUSTOM_DATA_TYPE_WALK_SPEED,

    E_CUSTOM_DATA_TYPE_INVALID_RUSH,

    E_CUSTOM_DATA_TYPE_SWAP_CURRENT_SELECT,





    // AI 관련 -- START




    // AI 관련 -- END
    E_CUSTOM_DATA_TYPE_SKILLRUNTIME,

    //
    E_CUSTOM_DATA_TYPE_DAMAGE_INFO_DATA,


}

public enum eObjectType
{
    E_OBJECT_TYPE_NONE,
    E_OBJECT_TYPE_CHARACTER,
    E_OBJECT_TYPE_AI,
    E_OBJECT_TYPE_ITEM,
    E_OBJECT_TYPE_BACK_OBJECT,
    E_OBJECT_TYPE_MISSILE,
    E_OBJECT_TYPE_MANAGER,
    E_OBJECT_TYPE_SWAP,
    E_OBJECT_TYPE_TRIGGER,
}

public enum eBaseObjectState
{
    E_OBJECT_STATE_ACTIVE,
    E_OBJECT_STATE_DEACTIVE,
}

public enum eFrozenMode
{
    e_FREE,     // 각자 컨트롤
    e_MELT,     // 전부 해제
    e_FROZEN,   // 전부 얼림
}
public class EnumData
{
    static public IList GetList(Dictionary<string, object> dicData, string strDataName)
    {
        if (dicData == null)
            return null;

        if (dicData.ContainsKey(strDataName) == false)
            return null;

        return dicData[strDataName] as IList;
    }

    static public Dictionary<string, object> GetDictionary(Dictionary<string, object> dicData, string strDataName)
    {
        if (dicData == null)
            return null;

        if (dicData.ContainsKey(strDataName) == false)
            return null;

        return dicData[strDataName] as Dictionary<string, object>;
    }

    static public int GetInteger(Dictionary<string, object> dicData, string strDataName)
    {
        if (dicData == null)
            return 0;

        if (dicData.ContainsKey(strDataName) == false)
            return 0;

        return GetInteger(dicData[strDataName]);
    }

    static public long GetLong(Dictionary<string, object> dicData, string strDataName)
    {
        if (dicData == null)
            return 0;

        if (dicData.ContainsKey(strDataName) == false)
            return 0;

        return GetLong(dicData[strDataName]);
    }

    static public string GetString(Dictionary<string, object> dicData, string strDataName)
    {
        if (dicData == null)
            return string.Empty;

        if (dicData.ContainsKey(strDataName) == false)
            return string.Empty;

        return GetString(dicData[strDataName]);
    }

    static public float GetFloat(Dictionary<string, object> dicData, string strDataName)
    {
        if (dicData == null)
            return 0;

        if (dicData.ContainsKey(strDataName) == false)
            return 0;

        return GetFloat(dicData[strDataName]);
    }

    static public double GetDouble(Dictionary<string, object> dicData, string strDataName)
    {
        if (dicData == null)
            return 0;

        if (dicData.ContainsKey(strDataName) == false)
            return 0;

        return GetDouble(dicData[strDataName]);
    }

    static public Vector3 GetVector3(Dictionary<string, object> dicData, string strDataName)
    {
        if (dicData == null)
            return Vector3.zero;

        if (dicData.ContainsKey(strDataName) == false)
            return Vector3.zero;


        return GetVector3(dicData[strDataName]);
    }

    static public bool GetBool(Dictionary<string, object> dicData, string strDataName)
    {
        if (dicData == null)
            return false;

        if (dicData.ContainsKey(strDataName) == false)
            return false;

        return GetBool(dicData[strDataName]);
    }

    static public int GetInteger(object dataValue)
    {
        if (dataValue is SimpleJSON.JSONNode)
        {
            return (dataValue as SimpleJSON.JSONNode);
        }

        if (dataValue is int)
            return (int)dataValue;

        if (dataValue is long)
            return Convert.ToInt32((long)dataValue);

        if (dataValue is string)
        {
            int nValue = 0;
            Int32.TryParse(dataValue as string, out nValue);
            return nValue;
        }

        if (dataValue is float)
            return Convert.ToInt32((float)dataValue);

        if (dataValue is double)
            return Convert.ToInt32((double)dataValue);

        return 0;
    }

    static public long GetLong(object dataValue)
    {
        if (dataValue is SimpleJSON.JSONNode)
            return (dataValue as SimpleJSON.JSONNode);

        if (dataValue is long)
            return (long)dataValue;

        if (dataValue is int)
            return Convert.ToInt64((int)dataValue);

        if (dataValue is string)
        {
            long lValue = 0;
            Int64.TryParse(dataValue as string, out lValue);
            return lValue;
        }

        if (dataValue is float)
            return Convert.ToInt64((float)dataValue);

        if (dataValue is double)
            return Convert.ToInt64((double)dataValue);

        if (dataValue is SimpleJSON.JSONLong)
        {
            SimpleJSON.JSONLong longValue = dataValue as SimpleJSON.JSONLong;
            long asLong = longValue.AsLong;
            return asLong;
        }

        return 0L;
    }

    static public string GetString(object dataValue)
    {
        if (dataValue is string)
            return dataValue as string;

        if (dataValue is long)
            return ((long)dataValue).ToString();

        if (dataValue is int)
            return ((int)dataValue).ToString();

        if (dataValue is float)
            return ((float)dataValue).ToString();

        if (dataValue is double)
            return ((double)dataValue).ToString();

        return string.Empty;
    }

    static public float GetFloat(object dataValue)
    {
        if (dataValue is SimpleJSON.JSONNode)
            return (dataValue as SimpleJSON.JSONNode);

        if (dataValue is float)
            return (float)dataValue;

        if (dataValue is double)
            return Convert.ToSingle((double)dataValue);

        if (dataValue is long)
            return Convert.ToSingle((long)dataValue);

        if (dataValue is int)
            return Convert.ToSingle((int)dataValue);

        if (dataValue is string)
        {
            float fValue = 0;
            float.TryParse(dataValue as string, out fValue);
            return fValue;
        }

        return 0L;
    }

    static public double GetDouble(object dataValue)
    {
        if (dataValue is SimpleJSON.JSONNode)
            return (dataValue as SimpleJSON.JSONNode);

        if (dataValue is double)
            return (double)dataValue;

        if (dataValue is float)
            return Convert.ToDouble((float)dataValue);

        if (dataValue is int)
            return Convert.ToDouble((int)dataValue);

        if (dataValue is string)
        {
            double dValue = 0;
            double.TryParse(dataValue as string, out dValue);
            return dValue;
        }

        if (dataValue is long)
            return Convert.ToDouble((long)dataValue);

        return 0L;
    }

    static public Vector3 GetVector3(object dataValue)
    {
        if (dataValue is string == false)
            return Vector3.zero;

        string strData = dataValue as string;

        string[] tmp = strData.Substring(1, strData.Length - 2).Split(',');
        float x = 0;
        float y = 0;
        float z = 0;

        float.TryParse(tmp[0], out x);
        float.TryParse(tmp[1], out y);
        float.TryParse(tmp[2], out z);

        return new Vector3(x, y, z);
    }

    static public bool GetBool(object dataValue)
    {
        if (dataValue is bool)
        {
            return (bool)dataValue;
        }

        return false;
    }

    static public string JsonToString(SimpleJSON.JSONNode jnode)
    {
        if (null == jnode || !(jnode is SimpleJSON.JSONObject))
            return string.Empty;
        return JsonToString((jnode as SimpleJSON.JSONObject).ToString());
    }

    static public string JsonToString(SimpleJSON.JSONObject jClass)
    {
        if (null == jClass)
            return string.Empty;
        return JsonToString(jClass.ToString());
    }

    static public string JsonToString(string strData)
    {
        return JsonToString(MiniJSON.Json.Deserialize(strData) as Dictionary<string, object>);
    }

    static public string JsonToString(Dictionary<string, object> jsonObject, System.Text.StringBuilder sb = null, int depth = 0)
    {
        if (null == jsonObject)
            return string.Empty;

        if (null == sb)
            sb = new System.Text.StringBuilder();

        sb.Append(' ', depth * 4);
        sb.AppendLine("{");
        depth += 1;
        foreach (KeyValuePair<string, object> pair in jsonObject)
        {
            sb.Append(' ', depth * 4);
            sb.Append(pair.Key + " : ");
            if (pair.Value is Dictionary<string, object>)
            {
                sb.AppendLine();
                JsonToString(pair.Value as Dictionary<string, object>, sb, depth);
            }
            else if (pair.Value is List<object>)
            {
                sb.AppendLine("[");
                foreach (var obj in (pair.Value as List<object>))
                {
                    if (obj is Dictionary<string, object>)
                    {
                        JsonToString(obj as Dictionary<string, object>, sb, depth + 1);
                    }
                    else
                    {
                        sb.Append(' ', (depth + 1) * 4);
                        sb.AppendLine(obj.ToString() + ", ");
                    }
                }
                sb.Append(' ', depth * 4);
                sb.AppendLine("]");
            }
            else
                sb.AppendLine(pair.Value.ToString());
        }
        depth -= 1;
        sb.Append(' ', depth * 4);
        sb.AppendLine("}");

        if (0 == depth)
            return sb.ToString();
        else
            return null;
    }
}
