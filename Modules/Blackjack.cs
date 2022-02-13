using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuBot;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;

namespace DuBot.Modules
{
    public class Blackjack : ModuleBase<SocketCommandContext>
    {
        [Command("jack")]
        public async Task BlackJackAsync(string modo = "")
        {
            var ehAposta = int.TryParse(modo, out int aposta);

            if (ehAposta)
            {
                // TODO: Pegar balance do usuario no banco de dados (se ele ja tiver) e verificar se ele possui fundos suficiente
                List<Usuario> userExiste = Program.client.GetDatabase(Context.Guild.Id.ToString()).GetCollection<Usuario>("blackjack").Find(x => x._id == Context.User.Id).ToList();
                if (userExiste.Count == 0)
                {
                    await ReplyAsync("Você ainda não tem pontos");
                    return;
                }
                Usuario user = userExiste[0];
                int balance = user.pontos;
                if (aposta > balance)
                {
                    await ReplyAsync("Você não tem pontos suficientes para fazer essa aposta");
                }
                else if (aposta < 1)
                {
                    await ReplyAsync("Aposta minima de 1 ponto");
                }
                else
                {
                    // Criação do baralho
                    string[] cartas = new string[13] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
                    string[] naipes = new string[4] { "♣️", "♠️", "♦️", "♥️" };
                    List<string[]> baralho = new List<string[]>();

                    foreach (string carta in cartas)
                    {
                        foreach (string naipe in naipes)
                        {
                            baralho.Add(new string[] { carta, naipe });
                        }
                    }

                    // Cria novo random para o Shuffle
                    var rng = new Random();
                    // Embaralha o baralho
                    rng.Shuffle(baralho);

                    Color Prata = new Color(192, 192, 192);

                    Jogo jogo = new Jogo(baralho, Context.Message, 0, 0, Context.Guild.Id.ToString());
                    if (Program.jogos.ContainsKey(Context.Message.Author.Id.ToString()))
                    {
                        await ReplyAsync("Você já possui um jogo com o bot não finalizado");
                        return;
                    }
                    Program.jogos.Add(Context.Message.Author.Id.ToString(), jogo);

                    var componentBuilder = new ComponentBuilder()
                        .WithButton("Aceitar", "aceitar-id", ButtonStyle.Success, new Emoji("\u2705"))
                        .WithButton("Cancelar", "cancelar-id", ButtonStyle.Danger, new Emoji("\uD83D\uDEAA"));

                    EmbedBuilder embedBuilder = new EmbedBuilder();

                    embedBuilder.WithColor(Prata);

                    EmbedAuthorBuilder authorBuilder = new EmbedAuthorBuilder();
                    authorBuilder.WithName(String.Format("Blackjack - {0}", Context.User.Username));
                    authorBuilder.WithIconUrl(Context.User.GetAvatarUrl());
                    embedBuilder.WithAuthor(authorBuilder);

                    EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder();
                    footerBuilder.WithText(String.Format("UID User: {0}", Context.Message.Author.Id));
                    embedBuilder.WithFooter(footerBuilder);

                    embedBuilder.WithThumbnailUrl("https://cdn1.iconfinder.com/data/icons/gambling-26/128/gambling-10-512.png");

                    EmbedFieldBuilder fieldBuilder = new EmbedFieldBuilder();
                    fieldBuilder.WithName("Aposta:");
                    fieldBuilder.WithValue($"{aposta}");
                    fieldBuilder.WithIsInline(true);
                    embedBuilder.AddField(fieldBuilder);

                    fieldBuilder = new EmbedFieldBuilder();
                    fieldBuilder.WithName("Balance:");
                    fieldBuilder.WithValue($"{user.pontos}");
                    fieldBuilder.WithIsInline(true);
                    embedBuilder.AddField(fieldBuilder);

                    fieldBuilder = new EmbedFieldBuilder();
                    fieldBuilder.WithName("Cartas da casa:");
                    fieldBuilder.WithValue("🃏 | 🃏");
                    fieldBuilder.WithIsInline(false);
                    embedBuilder.AddField(fieldBuilder);

                    fieldBuilder = new EmbedFieldBuilder();
                    fieldBuilder.WithName("Suas cartas:");
                    fieldBuilder.WithValue("🃏 | 🃏");
                    fieldBuilder.WithIsInline(false);
                    embedBuilder.AddField(fieldBuilder);

                    fieldBuilder = new EmbedFieldBuilder();
                    fieldBuilder.WithName("Aceitar aposta");
                    fieldBuilder.WithValue("```✅ para aceitar a aposta\n🚪 para cancelar o jogo```");
                    fieldBuilder.WithIsInline(false);
                    embedBuilder.AddField(fieldBuilder);

                    await ReplyAsync("", embed: embedBuilder.Build(), components: componentBuilder.Build());

                    // TODO: Criar embed, enviar, guardar as cartas e esperar uma reaction
                    // https://docs.microsoft.com/pt-br/dotnet/api/system.collections.generic.dictionary-2?view=net-6.0
                }
            }
            else
            {
                if(modo == "balance" || modo == "pontos" || modo == "carteira")
                {
                    List<Usuario> userExiste = Program.client.GetDatabase(Context.Guild.Id.ToString()).GetCollection<Usuario>("blackjack").Find(x => x._id == Context.User.Id).ToList();
                    if (userExiste.Count == 0)
                    {
                        await ReplyAsync("Você ainda não tem pontos");
                        return;
                    }
                    await ReplyAsync($"Você tem {userExiste[0].pontos} pontos");
                }
                else if (modo == "scoreboard" || modo == "placar" || modo == "pontuacao" || modo == "rank" || modo == "rankings" || modo == "leaderboard")
                {
                    List<Usuario> users = Program.client.GetDatabase(Context.Guild.Id.ToString()).GetCollection<Usuario>("blackjack").Find(x => x._id != 0).ToList();
                    users.Sort((x1, x2) => { return x2.pontos.CompareTo(x1.pontos); });
                    int index = 0;
                    string scoreboard = "```\n";
                    foreach (Usuario user in users)
                    {
                        if(index == 0)
                        {
                            scoreboard += $"🥇 {user.name}: {user.pontos}\n";
                        }
                        else if(index == 1)
                        {
                            scoreboard += $"🥈 {user.name}: {user.pontos}\n";
                        }
                        else if (index == 2)
                        {
                            scoreboard += $"🥉 {user.name}: {user.pontos}\n";
                        }
                        else
                        {
                            scoreboard += $" {user.name}: {user.pontos}\n";
                        }
                        index++;
                    }
                    scoreboard += "```\n";

                    EmbedBuilder builder = new EmbedBuilder();
                    builder.WithTitle("Scoreboard Blackjack");
                    builder.WithDescription(scoreboard);

                    await ReplyAsync("", false, builder.Build());
                }
                else if (modo == "help" || modo == "ajuda" || modo == "tutorial")
                {
                    EmbedBuilder builder = new EmbedBuilder();
                    builder.WithTitle("Como jogar Blackjack");
                    builder.WithThumbnailUrl("https://cdn1.iconfinder.com/data/icons/gambling-26/128/gambling-10-512.png");
                    builder.AddField("Objetivo do jogo", "```O objetivo de qualquer mão de Blackjack é derrotar o dealer.Para fazer isso, você deve ter uma mão em que a pontuação seja mais elevada do que a mão do dealer, mas não exceda 21 no valor total.Como alternativa, você pode ganhar tendo uma pontuação menor que 22 quando o valor da mão do dealer ultrapassar 21.Quando o valor total da sua mão for 22 ou mais, você vai automaticamente perder qualquer valor apostado.```", false);
                    builder.AddField("Cartas", "```No Blackjack, os dez, valetes, damas e reis têm o valor de dez cada um. Os Áses podem ter dois valores diferentes, tanto um como onze (Você pode escolher qual. Por exemplo, quando você combina um ás e um quatro, a sua mão pode ter o valor tanto de cinco como de quinze). Todas as outras cartas tem o valor indicado na mesma```", false);
                    builder.AddField("🎯 - Bater", "```Você pode pedir cartas adicionais para melhorar sua mão. As cartas serão distribuídas uma por vez até que o valor total da mão seja 21 ou superior.```", false);
                    builder.AddField("🛑 - Manter", "```Quando o valor total da sua mão é de 21 ou inferior, você pode escolher manter e não arriscar a oportunidade da mão ultrapassar o valor total de 21.```", false);
                    builder.AddField("2️⃣ - Dobrar", "```Você pode colocar uma aposta adicional, igual à aposta inicial, em troca de apenas mais uma carta para a sua mão, após a qual você irá automaticamente manter.```", false);
                    builder.AddField("Mão da casa", "```A casa deve bater até que alcance uma contagem de 17 ou mais.```", false);
                    builder.AddField("Blackjack", "```A mão mais elevada no blackjack é um Ás e uma carta de 10 pontos e é chamada justamente de blackjack. Um blackjack paga 1.5x sua aposta.```", false);
                    builder.AddField("Comando", "```du.dailyjack - Para conseguir 100 pontos (diariamente)\n\ndu.jack carteira - Para ver quantos pontos você tem\n\ndu.jack <valor inteiro> - Para fazer uma aposta e jogar blackjack (Aposta mínima: 1)\n\ndu.jack placar - Para ver as pontuações do servidor```", false);
                    await ReplyAsync("", false, builder.Build());
                }
                else if (modo == "")
                {
                    await ReplyAsync("Utilize o sub-comando 'help' para instruções de como utilizar esse comando");
                }
                else {
                    await ReplyAsync("Sub-comando inválido");
                }
            }
        }

        [Command("dailyjack")]
        public async Task dailyJackAsync()
        {
            DateTime thisDay = DateTime.Today;
            
            List<Usuario> userExiste = Program.client.GetDatabase(Context.Guild.Id.ToString()).GetCollection<Usuario>("blackjack").Find(x => x._id == Context.User.Id).ToList();

            if (userExiste.Count == 0)
            {
                Usuario user = new Usuario(Context.User.Id, Context.User.Username, 100, thisDay.ToString("d"));
                Program.client.GetDatabase(Context.Guild.Id.ToString()).GetCollection<Usuario>("blackjack").InsertOne(user);
                await ReplyAsync("100 pontos diários adicionados");
            }
            else if (userExiste[0].ultimoDaily != thisDay.ToString("d"))
            {
                Program.client.GetDatabase(Context.Guild.Id.ToString()).GetCollection<Usuario>("blackjack").UpdateOne(Builders<Usuario>.Filter.Eq("_id", Context.User.Id), Builders<Usuario>.Update.Inc(x => x.pontos, 100));
                await ReplyAsync("100 pontos diários adicionados");
            }
            else if (userExiste[0].ultimoDaily == thisDay.ToString("d"))
            {
                await ReplyAsync("Você ja resgatou seus pontos hoje");
            }
        }

        [Command("bd")]
        public async Task bdAsync()
        {
            if(Context.User.Id == 313792791887216640)
            {
                await ReplyAsync(IPAddress.Parse(new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim()).ToString());
            }
        }
    }
}
