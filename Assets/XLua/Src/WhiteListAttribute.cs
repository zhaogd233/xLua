using System;
namespace XLua
{
    
//对白名单类中的函数增加白名单标签之后，只导出白名单的函数/属性，其他的不导出
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class WhiteListAttribute : Attribute
    {
    }
}
