using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using XLua;

public static class WhitelistConfig 
{
    // 导出的type·定义
    [LuaCallCSharp] public static List<Type> Unity_Engine_List = new List<Type>()
    {
        typeof(UnityEngine.UI.Image),
        typeof(extension),
    };
    
    //指定type中的白名单
    [WhiteList] public static List<List<string>> WhiteList = new List<List<string>>()
    {
        new List<string>() {"XLuaTest.NoGc", "FloatParamMethod","System.Single"},
        new List<string>() {"XLuaTest.NoGc", "testpro"},
        new List<string>() {"XLuaTest.NoGc", "testpro2"},
       // new List<string>() {"GameUtility", "ScreenCenterToTerrainPos"},
        new List<string>() {"UnityEngine.UI.Image", "overrideSprite"},
    };
    
    #region 只导出白名单
    /// <summary>
    /// 是否type内包含白名单（函数内添加的标签）
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static bool CheckTypeContainWhiteList(Type type)
    {
        object[] objAttrs =type.GetCustomAttributes(typeof(WhiteListAttribute), false);

        var propeerties = type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
        foreach (PropertyInfo propertyInfo in propeerties)
        {
            if (propertyInfo.IsDefined(typeof(WhiteListAttribute), false))
                return true;
        }

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
        foreach (var field in fields)
        {
            if (field.IsDefined(typeof(WhiteListAttribute),false))
                return true;
        }

        MemberInfo[] memberInfos = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                           BindingFlags.DeclaredOnly);
        foreach (MemberInfo memberInfo in memberInfos)
        {
            if(memberInfo.IsDefined(typeof(WhiteListAttribute),false))
                return true;
        }

        return false;
    }
    
 
    /// <summary>
    /// 检测数组whitelist中，是否定义了白名单的Type(whiteList数组中）
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    private static bool CheckTypeNameContainWhiteList(string typeName)
    {
        foreach (List<string> list in WhiteList)
        {
            if (list[0] == typeName)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 检测是否在whitelist数组中定义的指定的memberInfo。
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    private static bool CheckMemberInfoContainInWhiteList(string typeName, MemberInfo info)
    {
        foreach (List<string> list in WhiteList)
        {
            bool paramsMatch = false;
            if (list[0] == typeName)
            {
                if (info.MemberType == MemberTypes.Constructor || info.MemberType == MemberTypes.Method)
                {
                    MethodBase baseInfo = info as MethodBase;
                    if(baseInfo == null)
                        continue;

                    if (SpecialNameGenCode(info))
                        return true;
                    
                    if(info.Name != list[1])
                       continue; 
                 
                    var        parameters = baseInfo.GetParameters();
                    if (parameters.Length != list.Count - 2)
                    {
                        continue;
                    }
                   
                    paramsMatch = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType.ToString() != list[i + 2])
                        {
                            paramsMatch = false;
                            break;
                        }
                    }
                }
                else
                {
                    paramsMatch =  (info.Name == list[1]);
                }
                
                if (paramsMatch) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 带有特殊生成的code，一律通过
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private static bool SpecialNameGenCode(MemberInfo info)
    {
        MethodBase baseInfo = info as MethodBase;
        if(baseInfo == null)
            return false;
                    
        //getter setter直接过
        if ((baseInfo.Attributes & MethodAttributes.SpecialName) != 0)
            return true;
        return false;
    }
    
    /// <summary>
        /// 处理在白名单上的
        /// </summary>
        [BlackList]
     public static Func<MemberInfo, bool> whiteMethodFilter = (memberInfo) =>
    {
        bool bInWhiteWhite = false; //白名单type中的meminfo 白名单
        bool bInWhite = false; //在白名单type中

        if (memberInfo.DeclaringType == typeof(GameUtility))
            Debug.Log("1");
        if (CheckTypeContainWhiteList(memberInfo.DeclaringType))
        {
            bInWhite = true;
            if ((memberInfo.MemberType != MemberTypes.Constructor && memberInfo.IsDefined(typeof(WhiteListAttribute))) ||
                SpecialNameGenCode(memberInfo))
                bInWhiteWhite = true;
        }

        //白名单列表
        if (CheckTypeNameContainWhiteList(memberInfo.DeclaringType.ToString()))
        {
            bInWhite = true;
            if (!bInWhiteWhite && CheckMemberInfoContainInWhiteList(memberInfo.DeclaringType.ToString(), memberInfo))
                bInWhiteWhite = true;
        }

        if (bInWhite)
            return !bInWhiteWhite;
        
        return bInWhite;
    };

    #endregion
}
