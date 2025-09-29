using Naninovel;
using UnityEngine;

public static class CustomFunctions
{
    [ExpressionFunction("getWealth")]
    public static float getWealth()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        return character.wealth;
    }
    // [ExpressionFunction("getRelationship")]
    // [Doc("获取指定角色的好感度数值", examples: "getRelationship(\"雅琼\")")]
    // public static float GetRelationship(string characterName)
    // {
    //     var character = GameSystem.Instance.GetCurrentCustomer();
    //     // 假设您有一个全局的CustomerManager来管理好感度
    //     if (CustomerManager.Instance != null && 
    //         CustomerManager.Instance.Customers.ContainsKey(characterName))
    //     {
    //         return CustomerManager.Instance.Customers[characterName].relationship;
    //     }
    //     return 0f; // 默认值
    // }
    //
    // [ExpressionFunction("setRelationship")]
    // [Doc("设置指定角色的好感度数值", examples: "setRelationship(\"雅琼\", 50)")]
    // public static void SetRelationship(string characterName, float value)
    // {
    //     if (CustomerManager.Instance != null && 
    //         CustomerManager.Instance.Customers.ContainsKey(characterName))
    //     {
    //         CustomerManager.Instance.Customers[characterName].relationship = value;
    //     }
    // }
    //
    // [ExpressionFunction("addRelationship")]
    // [Doc("增加指定角色的好感度数值", examples: "addRelationship(\"雅琼\", 10)")]
    // public static void AddRelationship(string characterName, float value)
    // {
    //     if (CustomerManager.Instance != null && 
    //         CustomerManager.Instance.Customers.ContainsKey(characterName))
    //     {
    //         CustomerManager.Instance.Customers[characterName].relationship += value;
    //     }
    // }
}