using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DuBot.Modules
{
    public class JogoDaVelha : ModuleBase<SocketCommandContext>
    {
        [Command("velha")]
        public async Task JogoDaVelhaAsync(IUser user)
        {
            //await ReplyAsync(IPAddress.Parse(new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim()).ToString());
            var componentBuilder = new ComponentBuilder()
                .WithButton("", "0", ButtonStyle.Success, new Emoji("\u2705"))
                .WithButton("", "cancelar-id", ButtonStyle.Danger, new Emoji("\uD83D\uDEAA"));
            await ReplyAsync("", components: componentBuilder.Build());
        }
    }
}
