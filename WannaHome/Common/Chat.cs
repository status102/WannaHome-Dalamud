using Dalamud.Game.Text.SeStringHandling;

namespace WannaHome.Common
{
    public class Chat
    {
        public static void PrintError(string msg)
        {
            // 45 绿 #00CC22
            // 17 红 #DC0000
            // 508 粉红 #FF8080
            var builder = new SeStringBuilder()
                .AddUiForeground($"[{WannaHome.Plugin_Name}]", 45)
                .AddUiForeground(msg, 508);
            Service.ChatGui.PrintError(builder.BuiltString);
        }

        public static void PrintWarning(string msg)
        {
            // 62 黄 #F5EB67
            var builder = new SeStringBuilder()
                .AddUiForeground($"[{WannaHome.Plugin_Name}]", 45)
                .AddUiForeground(msg, 62);
            Service.ChatGui.Print(builder.BuiltString);
        }
        public static void PrintMsg(string msg)
        {
            var builder = new SeStringBuilder()
                .AddUiForeground($"[{WannaHome.Plugin_Name}]", 45)
                .AddUiForeground(msg, 1);
            Service.ChatGui.Print(builder.BuiltString);


        }
        public static void PrintLog(string msg)
        {
            var builder = new SeStringBuilder()
                .AddUiForeground($"[{WannaHome.Plugin_Name}]", 45)
                .AddText(msg);
            Service.ChatGui.Print(builder.BuiltString);


        }
    }
}
