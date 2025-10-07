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
        var character = GameSystem.Instance.GetSanity();
        return character;
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
        15,20,25,30,35,40,45,50,100
    };
    static List<int> talkedTime = new List<int>()
    {
        1,2,3,4,5,6,7,8,9,10,100
    };

    public static string NextStoryRequest()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        var res = "Next Event: ";
        var story = character.lastStory+1;
        if (character.mainAttribute < mainAttributes[story])
        {
            res+= $"{mainAttributes[story]-character.mainAttribute} more {character.info.target} ";
        }
        if (character.talkedTime < talkedTime[story])
        {
            res+= $"{talkedTime[story]-character.talkedTime} more talk";
        }

        return res;
    }
    [ExpressionFunction("story")]
    public static int Story()
    {
        var character = GameSystem.Instance.GetCurrentCustomer();
        var result = -1;
        int i = character.lastStory  + 1;
        //for (int i = 0; i < 10; i++)
        {
            if (mainAttributes.Count>i && talkedTime.Count>i && character.mainAttribute >= mainAttributes[i] && character.talkedTime >= talkedTime[i])
            {
                result= i;
                character.lastStory=i;
            }
        }

        if (result != -1)
        {
            ToastManager.Instance.ShowToast("~~New Story For Customer Revealed~~");
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