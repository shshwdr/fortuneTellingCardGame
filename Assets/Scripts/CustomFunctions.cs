using System.Collections.Generic;
using Naninovel;
using UnityEngine;

public static class CustomFunctions
{
    [ExpressionFunction("getPower")]
    public static float getPower()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        return character.power;
    }
    
    [ExpressionFunction("getEmotion")]
    public static float getEmotion()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        return character.emotion;
    }
    [ExpressionFunction("getEmotion")]
    public static float getWisdom()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        return character.wisdom;
    }
    [ExpressionFunction("getSanity")]
    public static float getSanity()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        return character.sanity;
    }
    [ExpressionFunction("getMain")]
    public static float getMain()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        return character.mainAttribute;
    }
    
    

    [ExpressionFunction("talked")]
    public static bool HasTalked()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        return character.talkedTime > 0;
    }

    [ExpressionFunction("talkedTime")]
    public static int TalkedTime()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        return character.talkedTime;
    }
    static List<int> mainAttributes = new List<int>()
    {
        55,60,65,70,75,80,85,90,90,90,90
    };
    static List<int> talkedTime = new List<int>()
    {
        0,1,2,3,4,5,6,7,8,9,10
    };
    [ExpressionFunction("story")]
    public static int Story()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        var result = -1;
        for (int i = 0; i < 10; i++)
        {
            if (character.lastStory < i && character.mainAttribute >= mainAttributes[i] && character.talkedTime >= talkedTime[i])
            {
                result= i;
                character.lastStory=i;
                break;
            }
        }
        

        return result;
    }

    [ExpressionFunction("lastStory")]
    public static int lastStory()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        return character.lastStory;
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