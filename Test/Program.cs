// See https://aka.ms/new-console-template for more inform

string input = "这是一段包含IP地址的字符串：192.168.0.1，10.0.0.1，172.16.0.1中文";
string pattern = @"([a-zA-Z0-9.]+)|([\u4e00-\u9fa5]+)"; //匹配英文数字或中文
Console.WriteLine(NPinyin.Pinyin.GetInitials("钉钉").ToLower());
