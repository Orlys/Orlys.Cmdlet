
namespace Orlys.Cmdlet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;
       
    public static class Cmdlet<T> where T : new()
    {
        public static Cmdlet Singleton { get; } = Cmdlet.Create<T>();
    }

    public sealed class Cmdlet : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Cmdlet Create<T>() where T : new()
        {
            return Create(new T());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Cmdlet Create<T>(T inst)
        {
            return new Cmdlet(inst);
        }

        private readonly List<Tuple<MarkAttribute, MethodInfo>> _marked;

        public new Type GetType()
        {
            return this._type;
        }

        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        private readonly object _inst;

        private readonly Type _type;
        private Cmdlet(object instance)
        {
            this._inst = instance;
            this._type = instance.GetType();

            this._marked = new List<Tuple<MarkAttribute, MethodInfo>>(); 
             
            var index = 0;
            foreach (var method in this._type.GetMethods(Flags))
            {
                var mark = method.GetCustomAttribute<MarkAttribute>();
                if (mark == null)
                    continue;
                // if (method.ReturnType != typeof(void)) continue;
                if (string.IsNullOrWhiteSpace(mark.Name))
                    mark.Name = method.Name;
                mark.Index = index++;
                this._marked.Add(Tuple.Create(mark, method));
            }
        }
        
        private Dictionary<object, string> ParseArguments(Queue<string> cmds, out string methodName)
        {
            // preprocess command           
            methodName = cmds.Dequeue();
            var args = new Dictionary<object, string>();
            var holder = default(string);
            var index = 0;
            while (cmds.Count > 0)
            {
                var current = cmds.Dequeue();
                if (holder != null)
                {
                    args.Add(holder, current);
                    holder = null;
                }
                else if (current.StartsWith("-"))
                {
                    holder = current.TrimStart('-');
                }
                else
                {
                    args.Add(index++, current);
                }
            }

            return args;
        }

        public ExecutedResult Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return new ExecutedResult(new ArgumentNullException(nameof(command)));

            var cs = this.Split(command);
            if (cs.Count == 0)
                return new ExecutedResult(new FormatException(nameof(command)));
            var cmds = new Queue<string>(cs);
            var args = this.ParseArguments(cmds, out var methodName);

            var list = new List<object>();
            var signatureNotMatched = false;
            foreach (var tuple in this._marked)
            {
                if (tuple.Item1.Name.Equals(methodName, StringComparison.CurrentCultureIgnoreCase))
                {
                    var parameters = tuple.Item2.GetParameters();

                    // if (parameters.Length != args.Count)
                    //    continue;
                    for (int index = 0; index < parameters.Length; index++)
                    {
                        var p = parameters[index];
                        if (p.ParameterType.IsByRef)
                        {
                            goto MoveNext;
                        }
                        if (p.HasDefaultValue)
                        {
                            var key = p.GetCustomAttribute<OptionalAttribute>() is OptionalAttribute optional
                                ? optional.Name
                                : p.Name;

                            if (args.TryGetValue(key, out var str) && this.TryParseType(p.ParameterType, str, out var value))
                                list.Add(value);
                            else
                                list.Add(p.DefaultValue);
                        }
                        else if (args.TryGetValue(index, out var str) && this.TryParseType(p.ParameterType, str, out var value))
                            list.Add(value);

                    }

                    try
                    {
                        var r = tuple.Item2.Invoke(this._inst, list.ToArray());
                        return new ExecutedResult(r);
                    }
                    catch (TargetParameterCountException)
                    {
                        signatureNotMatched = true;
                    }
                    catch (Exception e)
                    {
                        return new ExecutedResult(e);
                    }

                }
                MoveNext: { }
            }

            if (signatureNotMatched)
                return new ExecutedResult(new ArgumentException("The corresponding method and its parameters could not be found."));

            return new ExecutedResult(new MissingMethodException(this._type.FullName, methodName));
        }


        private List<string> Split(string command)
        {
            const char QUOTE = '"';
            var list = new List<string>();
            var sb = new StringBuilder(command.Length);
            var inQuote = false;
            for (int i = 0; i < command.Length; i++)
            {
                var c = command[i];
                if (inQuote)
                {
                    if (c == '\\' && command[i + 1] == QUOTE)
                    {
                        sb.Append(QUOTE);
                        i += 2;
                        c = command[i];
                    }

                    if (c == QUOTE)
                    {
                        var value = sb.ToString();
                        sb.Clear();
                        list.Add(value);
                        inQuote = false;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else if (c == QUOTE)
                {
                    inQuote = true;
                }
                else if (c == ' ')
                {
                    if (sb.Length == 0)
                        continue;
                    var value = sb.ToString();
                    sb.Clear();
                    list.Add(value);
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length > 0)
                list.Add(sb.ToString());

            return list;
        }


        private bool TryParseType(Type type, string str, out object value)
        {
            var flag = true;
            if (type == typeof(string))
            {
                value = str;
                return flag;
            }
            if (type == typeof(bool) && TryParseBoolean(str, out var v01)) value = v01;
            else if (type == typeof(char) && char.TryParse(str, out var v02)) value = v02;
            else if (type == typeof(byte) && byte.TryParse(str, out var v03)) value = v03;
            else if (type == typeof(sbyte) && sbyte.TryParse(str, out var v04)) value = v04;
            else if (type == typeof(ushort) && ushort.TryParse(str, out var v05)) value = v05;
            else if (type == typeof(short) && short.TryParse(str, out var v06)) value = v06;
            else if (type == typeof(uint) && uint.TryParse(str, out var v07)) value = v07;
            else if (type == typeof(int) && int.TryParse(str, out var v08)) value = v08;
            else if (type == typeof(ulong) && ulong.TryParse(str, out var v09)) value = v09;
            else if (type == typeof(long) && long.TryParse(str, out var v0a)) value = v0a;
            else if (type == typeof(float) && float.TryParse(str, out var v0b)) value = v0b;
            else if (type == typeof(double) && double.TryParse(str, out var v0c)) value = v0c;
            else if (type == typeof(decimal) && decimal.TryParse(str, out var v0d)) value = v0d;
            else if (type == typeof(Guid) && Guid.TryParse(str, out var v0e)) value = v0e;
            else if (type == typeof(DateTimeOffset) && DateTimeOffset.TryParse(str, out var v0f)) value = v0f;
            else if (type == typeof(DateTime) && DateTime.TryParse(str, out var v10)) value = v10;
            else if (type == typeof(TimeSpan) && TimeSpan.TryParse(str, out var v11)) value = v11;
            else if (type == typeof(Type) && Type.GetType(str) is Type v12) value = v12;
            else
            {
                value = null;
                flag = false;
            }
            return flag;
        }


        private static readonly string[] BOOL_POSITIVE = { "yes", "on", "enabled", "true" };
        private static readonly string[] BOOL_NEGATIVE = { "no", "off", "disabled", "false" };

        private bool TryParseBoolean(string str, out bool b)
        {
            var f = true;
            if (BOOL_POSITIVE.Contains(str, StringComparer.CurrentCultureIgnoreCase))
            {
                b = true;
            }
            else if (BOOL_NEGATIVE.Contains(str, StringComparer.CurrentCultureIgnoreCase))
            {
                b = false;
            }
            else
            {
                f = false;
                b = false;
            }
            return f;
        }

        void IDisposable.Dispose()
        {
            if(this._inst is IDisposable d)
            {
                d.Dispose();
            }
        }
    }

    
}
