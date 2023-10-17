using System.Numerics;
using System.Text.RegularExpressions;
using log4net;

namespace Core.SDKs.Tools;

public class Math
{
    private static readonly ILog log = LogManager.GetLogger(nameof(Math));

    // 评估一个复杂数学表达式，返回一个BigInteger类型的值
    public static BigInteger Evaluate(string expression)
    {
        //log.Debug($"计算{expression}");
        // 去掉空格
        expression = expression.Replace(" ", "");
        // 处理括号
        while (expression.Contains("("))
        {
            // 找到最内层的括号
            int start = expression.LastIndexOf("(");
            int end = expression.IndexOf(")", start);
            // 计算括号内的子表达式
            BigInteger subResult = Evaluate(expression.Substring(start + 1, end - start - 1));
            // 用子表达式的结果替换括号
            expression = expression.Remove(start, end - start + 1).Insert(start, subResult.ToString());
        }

        // 处理幂运算
        while (expression.Contains("^"))
        {
            // 使用正则表达式匹配第一个幂运算符及其两边的操作数
            Match match = Regex.Match(expression, @"(-?\d+)\^(-?\d+)");
            // 获取操作数和运算符
            BigInteger left = BigInteger.Parse(match.Groups[1].Value);
            BigInteger right = BigInteger.Parse(match.Groups[2].Value);
            // 计算幂运算结果
            BigInteger subResult = BigInteger.Pow(left, (int)right);
            // 用幂运算结果替换原来的子表达式
            expression = expression.Remove(match.Index, match.Length).Insert(match.Index, subResult.ToString());
        }

        // 处理乘法和除法
        while (expression.Contains("*") || expression.Contains("/"))
        {
            // 使用正则表达式匹配第一个乘法或除法运算符及其两边的操作数
            Match match = Regex.Match(expression, @"(-?\d+)(\*|/)(-?\d+)");
            // 获取操作数和运算符
            BigInteger left = BigInteger.Parse(match.Groups[1].Value);
            BigInteger right = BigInteger.Parse(match.Groups[3].Value);
            string op = match.Groups[2].Value;
            // 计算乘法或除法结果
            BigInteger subResult = op == "*" ? left * right : left / right;
            if (match.Groups[1].Value.StartsWith("-"))
            {
                expression = expression.Remove(match.Index, match.Length)
                    .Insert(match.Index, "+" + subResult.ToString());
                continue;
            }

            // 用乘法或除法结果替换原来的子表达式
            expression = expression.Remove(match.Index, match.Length).Insert(match.Index, subResult.ToString());
        }

        // 处理加法和减法
        while (expression.Contains("+") || expression.Contains("-"))
        {
            // 使用正则表达式匹配第一个加法或减法运算符及其两边的操作数
            Match match = Regex.Match(expression, @"(-?\d+)(\+|-)(-?\d+)");
            if (string.IsNullOrWhiteSpace(match.Value))
            {
                break;
            }

            // 获取操作数和运算符
            BigInteger left = BigInteger.Parse(match.Groups[1].Value);
            BigInteger right = BigInteger.Parse(match.Groups[3].Value);
            string op = match.Groups[2].Value;
            // 计算加法或减法结果
            BigInteger subResult = op == "+" ? left + right : left - right;
            // 用加法或减法结果替换原来的子表达式
            expression = expression.Remove(match.Index, match.Length).Insert(match.Index, subResult.ToString());
        }

        // 返回最终结果，转换为BigInteger类型

        var bigInteger = BigInteger.Parse(expression);
        //log.Debug($"{bigInteger}");
        return bigInteger;
    }
}