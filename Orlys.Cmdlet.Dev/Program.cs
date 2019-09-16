
namespace Orlys.Cmdlet.Dev
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Orlys.Cmdlet;
    static class Program
    {
        static void Main()
        {
            var m = Cmdlet.Create<MyClass>();
            var re = m.Execute("test -v 80 AAAA -o \"x-\\\"x\" -b on");
            if(re)
            {
                Console.WriteLine(re.Value);
            }
            else
            {
                Console.WriteLine(re.Error);
            }
        }
    }
    class MyClass
    {
        [Mark(Name = "test")]
        public static void TestMethod(string value1,
            [Optional(Name = "o")]string value2 = "val-2",
            [Optional(Name = "v")]int value3 = 0,
            [Optional(Name = "b")]bool value4 = false)
        {
            Console.WriteLine("value1: " + value1);
            Console.WriteLine("value2: " + value2);
            Console.WriteLine("value3: " + value3);
            Console.WriteLine("value4: " + value4);
            Console.WriteLine("-----");
        }
    }
}
