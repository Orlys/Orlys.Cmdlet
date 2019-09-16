# Cmdlet
提供執行期對函式(亦稱方法)的引動能力之相關基本功能

## 使用方法
1. 在你想呼叫的函式(動靜態皆可)上打上 ``` [Mark] ``` 標籤。  
2. 函式簽章中的參數，若具有預設值者皆可打上 ``` [Optional] ``` 標籤；若無則表示該參數為必要。  
3. 使用 Cmdlet.Create 系列函式建立 Cmdlet 物件實體，並利用 Execute 方法進行操作。
    
## 使用上的注意事項
- 必要參數的剖析方法為依照索引位置逐一解析。  
- ``` [Mark] ``` 及 ``` [Optional] ``` 標籤中的 Name 屬性可覆寫該方法名稱。
## 範例
以下是一個基本的函式宣告
```csharp
class MyClass
{
    [Mark(Name = "test")]
    public static void TestMethod(
        string value1,                                  // 必要參數 0
        [Optional(Name = "o")]string value2 = "val-2",  // 選項參數 -o
        [Optional(Name = "v")]int value3 = 0,           // 選項參數 -v
        [Optional(Name = "b")]bool value4 = false)      // 選項參數 -b
    {
        Console.WriteLine("value1: " + value1);
        Console.WriteLine("value2: " + value2);
        Console.WriteLine("value3: " + value3);
        Console.WriteLine("value4: " + value4);
        Console.WriteLine("-----");
    }
}
```
以下為呼叫方法
```csharp
// 建立 Cmdlet 物件實體
var m = Cmdlet.Create<MyClass>();
// 透過命令字串解析目標函式及填入參數並引動
var re = m.Execute("test -v 80 AAAA -o \"x-\\\"x\" -b on"); 
// ExecutedResult 物件可作為 bool 使用
if(re)
    Console.WriteLine(re.Value);
else
    Console.WriteLine(re.Error);
```