using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using DuBot.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DuBot.Services
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider provider;
        private readonly CommandService service;
        private readonly IConfiguration configuration;

        public CommandHandler(ILogger<CommandHandler> logger, IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration configuration) : base(client, logger)
        {
            this.provider = provider;
            this.service = service;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client.MessageReceived += HandleMessage;
            Client.ButtonExecuted += HandleButton;
            await this.service.AddModulesAsync(Assembly.GetEntryAssembly(), this.provider);
        }

        private async Task HandleMessage(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message) return;
            if (message.Source != MessageSource.User ) return;

            int argPos = 0;
            if (!message.HasStringPrefix(Environment.GetEnvironmentVariable("TOKEN") ?? this.configuration["Token"], ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(Client, message);
            await this.service.ExecuteAsync(context, argPos, this.provider);
        }

        public int[] valorMao(List<string[]> mao)
        {
            int As = 0;
            foreach (string[] carta in mao)
            {
                if(carta[0] == "A")
                {
                    As += 1;
                }
            }
            int valorMao1 = 0, valorMao2 = 0;
            if (As == 0)
            {
                foreach(string[] carta in mao)
                {
                    if (carta[0] == "J" || carta[0] == "Q" || carta[0] == "K")
                    {
                        valorMao1 += 10;
                    }
                    else
                    {
                        valorMao1 += Int32.Parse(carta[0]);
                    }
                }
            }
            else if (As == 1)
            {
                foreach(string[] carta in mao)
                {
                    if(carta[0] == "A")
                    {
                        valorMao1 += 11;
                        valorMao2 += 1;
                    }
                    else if (carta[0] == "J" || carta[0] == "Q" || carta[0] == "K")
                    {
                        valorMao1 += 10;
                        valorMao2 += 10;
                    }
                    else
                    {
                        valorMao1 += Int32.Parse(carta[0]);
                        valorMao2 += Int32.Parse(carta[0]);
                    }
                }
            }
            else
            {
                foreach (string[] carta in mao)
                {
                    if (carta[0] == "A")
                    {
                        if(As == 1)
                        {
                            valorMao1 += 11;
                            valorMao2 += 1;
                        }
                        else
                        {
                            valorMao1 += 1;
                            valorMao2 += 1;
                            As--;
                        }
                        
                    }
                    else if (carta[0] == "J" || carta[0] == "Q" || carta[0] == "K")
                    {
                        valorMao1 += 10;
                        valorMao2 += 10;
                    }
                    else
                    {
                        valorMao1 += Int32.Parse(carta[0]);
                        valorMao2 += Int32.Parse(carta[0]);
                    }
                }
            }
            if (As == 0)
            {
                return new int[] { valorMao1 };
            }
            else if (valorMao1 <= 21)
            {
                return new int[] { valorMao1, valorMao2 };
            }
            else
            {
                return new int[] { valorMao2 };
            }
        }

        public string maoParaString(List<string[]> mao)
        {
            string stringMao = String.Format("{0}.{1}", mao[0][0], mao[0][1]);
            if(mao.Count > 1)
            {
                for (int i = 1; i < mao.Count; i++)
                {
                    stringMao += String.Format(" | {0}.{1}", mao[i][0], mao[i][1]);
                }
                
            }
            return stringMao;
        }

        public List<string[]> stringParaBaralho(string mao)
        {
            string[] cartas = mao.Split('|','.', ' ').Where(e => e != "").ToArray();
            List<string[]> baralho = new List<string[]>();
            for (int i = 0; i < cartas.Length-1; i += 2)
            {
                baralho.Add(new string[] { cartas[i], cartas[i+1] });
            }
            
            return baralho;
        }

        public async void FimJogo(SocketMessageComponent component, List<string[]> maoUser, List<string[]> maoBot, int aposta, int balance)
        {
            Color Roxo = new Color(128, 0, 128);
            Color Vermelho = new Color(255, 0, 0);
            Color Verde = new Color(0, 255, 0);
            Color Amarelo = new Color(255, 255, 0);

            EmbedBuilder embedBuilder = new EmbedBuilder();

            embedBuilder.WithColor(Roxo);

            EmbedAuthorBuilder authorBuilder = new EmbedAuthorBuilder();
            authorBuilder.WithName($"Blackjack - {component.User.Username}");
            authorBuilder.WithIconUrl(component.User.GetAvatarUrl());
            embedBuilder.WithAuthor(authorBuilder);

            EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder();
            footerBuilder.WithText($"UID User: {component.User.Id}");
            embedBuilder.WithFooter(footerBuilder);

            embedBuilder.WithThumbnailUrl("https://cdn1.iconfinder.com/data/icons/gambling-26/128/gambling-10-512.png");

            EmbedFieldBuilder fieldBuilder = new EmbedFieldBuilder();
            fieldBuilder.WithName("Aposta:");
            fieldBuilder.WithValue(String.Format("{0}", aposta));
            fieldBuilder.WithIsInline(true);
            embedBuilder.AddField(fieldBuilder);

            fieldBuilder = new EmbedFieldBuilder();
            fieldBuilder.WithName("Balance:");
            fieldBuilder.WithValue(String.Format("{0}", balance));
            fieldBuilder.WithIsInline(true);
            embedBuilder.AddField(fieldBuilder);

            List<string[]> baralho = Program.jogos[component.User.Id.ToString()].baralho;

            Program.jogos[component.User.Id.ToString()].qntCartasBot++;
            maoBot.Add(baralho[Program.jogos[component.User.Id.ToString()].qntCartasBot+2]);
            Program.jogos[component.User.Id.ToString()].qntCartasBot++;
            string stringMaoBot = maoParaString(maoBot);
            int[] valorMaoBot = valorMao(maoBot);

            string stringMaoUser = maoParaString(maoUser);
            int[] valorMaoUser = valorMao(maoUser);

            fieldBuilder = new EmbedFieldBuilder();
            if (valorMaoBot.Length == 1)
            {
                fieldBuilder.WithName(String.Format("Cartas da casa ({0}):", valorMaoBot[0]));
            }
            else
            {
                fieldBuilder.WithName(String.Format("Cartas da casa ({0} | {1}):", valorMaoBot[0], valorMaoBot[1]));
            }
            fieldBuilder.WithValue(stringMaoBot);
            fieldBuilder.WithIsInline(false);
            embedBuilder.AddField(fieldBuilder);

            fieldBuilder = new EmbedFieldBuilder();
            if (valorMaoUser.Length == 1)
            {
                fieldBuilder.WithName(String.Format("Suas cartas ({0}):", valorMaoUser[0]));
            }
            else
            {
                fieldBuilder.WithName(String.Format("Suas cartas ({0} | {1}):", valorMaoUser[0], valorMaoUser[1]));
            }
            fieldBuilder.WithValue(stringMaoUser);
            fieldBuilder.WithIsInline(false);
            embedBuilder.AddField(fieldBuilder);

            await component.ModifyOriginalResponseAsync(x => { x.Embed = embedBuilder.Build(); x.Components = new ComponentBuilder().Build(); });
            
            while (valorMaoBot[0] < 17 && valorMaoUser[0] <= 21)
            {
                await Task.Delay(500);
                maoBot.Add(baralho[Program.jogos[component.User.Id.ToString()].qntCartasBot + Program.jogos[component.User.Id.ToString()].qntCartasUser]);
                Program.jogos[component.User.Id.ToString()].qntCartasBot++;
                stringMaoBot = maoParaString(maoBot);
                valorMaoBot = valorMao(maoBot);
                if (valorMaoBot.Length == 1)
                {
                    embedBuilder.Fields[2].Name = String.Format("Cartas da casa ({0}):", valorMaoBot[0]);
                }
                else
                {
                    embedBuilder.Fields[2].Name = String.Format("Cartas da casa ({0} | {1}):", valorMaoBot[0], valorMaoBot[1]);
                }
                embedBuilder.Fields[2].Value = stringMaoBot;
                await component.ModifyOriginalResponseAsync(x => { x.Embed = embedBuilder.Build(); x.Components = new ComponentBuilder().Build(); });
            }

            Usuario user = Program.client.GetDatabase(Program.jogos[component.User.Id.ToString()].idServidor).GetCollection<Usuario>("blackjack").Find(x => x._id == component.User.Id).ToList()[0];

            if (valorMaoUser[0] > 21)
            {
                embedBuilder.WithTitle("Bust!");
                embedBuilder.WithColor(Vermelho);
                embedBuilder.WithDescription($"Você perdeu {aposta} pontos\nBalance: `{balance - aposta} pontos`");
                Program.client.GetDatabase(Program.jogos[component.User.Id.ToString()].idServidor).GetCollection<Usuario>("blackjack").UpdateOne(Builders<Usuario>.Filter.Eq("_id", component.User.Id), Builders<Usuario>.Update.Inc(x => x.pontos, -aposta));
            }
            else if (valorMaoUser[0] == 21 && valorMaoBot[0] != 21)
            {
                if(maoUser.Count == 2)
                {
                    aposta = Convert.ToInt32(Math.Round(aposta * 1.5, 0));
                    embedBuilder.WithTitle("Blackjack!");
                }
                else
                {
                    embedBuilder.WithTitle("21!");
                }
                embedBuilder.WithDescription($"Você ganhou {aposta} pontos\nBalance: `{balance + aposta} pontos`");
                embedBuilder.WithColor(Verde);
                Program.client.GetDatabase(Program.jogos[component.User.Id.ToString()].idServidor).GetCollection<Usuario>("blackjack").UpdateOne(Builders<Usuario>.Filter.Eq("_id", component.User.Id), Builders<Usuario>.Update.Inc(x => x.pontos, aposta));
            }
            else if (valorMaoUser[0] > valorMaoBot[0] || valorMaoBot[0] > 21)
            {
                embedBuilder.WithTitle("Winner!");
                embedBuilder.WithColor(Verde);
                embedBuilder.WithDescription($"Você ganhou {aposta} pontos\nBalance: `{balance + aposta} pontos`");
                Program.client.GetDatabase(Program.jogos[component.User.Id.ToString()].idServidor).GetCollection<Usuario>("blackjack").UpdateOne(Builders<Usuario>.Filter.Eq("_id", component.User.Id), Builders<Usuario>.Update.Inc(x => x.pontos, aposta));
            }
            else if (valorMaoUser[0] < valorMaoBot[0])
            {
                embedBuilder.WithTitle("Loser!");
                embedBuilder.WithColor(Vermelho);
                embedBuilder.WithDescription($"Você perdeu {aposta} pontos\nBalance: `{balance - aposta} pontos`");
                Program.client.GetDatabase(Program.jogos[component.User.Id.ToString()].idServidor).GetCollection<Usuario>("blackjack").UpdateOne(Builders<Usuario>.Filter.Eq("_id", component.User.Id), Builders<Usuario>.Update.Inc(x => x.pontos, -aposta));
            }
            else if (valorMaoUser[0] == valorMaoBot[0])
            {
                embedBuilder.WithTitle("Push!");
                embedBuilder.WithColor(Amarelo);
                embedBuilder.WithDescription($"Você não perdeu nem ganhou pontos\nBalance: `{balance} pontos`");
            }

            await Task.Delay(750);
            await component.ModifyOriginalResponseAsync(x => { x.Embed = embedBuilder.Build(); x.Components = new ComponentBuilder().Build(); });

            Program.jogos.Remove(component.User.Id.ToString());
        }

        public async void MostraJogo(SocketMessageComponent component, string id)
        {
            Color Roxo = new Color(128, 0, 128);
            int aposta = Int32.Parse(component.Message.Embeds.First().Fields.ElementAt(0).Value);
            int balance = Int32.Parse(component.Message.Embeds.First().Fields.ElementAt(1).Value);
            List<string[]> baralho = Program.jogos[component.User.Id.ToString()].baralho;

            EmbedBuilder embedBuilder = new EmbedBuilder();

            embedBuilder.WithColor(Roxo);

            EmbedAuthorBuilder authorBuilder = new EmbedAuthorBuilder();
            authorBuilder.WithName($"Blackjack - {component.User.Username}");
            authorBuilder.WithIconUrl(component.User.GetAvatarUrl());
            embedBuilder.WithAuthor(authorBuilder);

            EmbedFooterBuilder footerBuilder = new EmbedFooterBuilder();
            footerBuilder.WithText($"UID User: {component.User.Id}");
            embedBuilder.WithFooter(footerBuilder);

            embedBuilder.WithThumbnailUrl("https://cdn1.iconfinder.com/data/icons/gambling-26/128/gambling-10-512.png");

            EmbedFieldBuilder fieldBuilder = new EmbedFieldBuilder();
            fieldBuilder.WithName("Aposta:");
            fieldBuilder.WithValue(String.Format("{0}", aposta));
            fieldBuilder.WithIsInline(true);
            embedBuilder.AddField(fieldBuilder);

            fieldBuilder = new EmbedFieldBuilder();
            fieldBuilder.WithName("Balance:");
            fieldBuilder.WithValue(String.Format("{0}", balance));
            fieldBuilder.WithIsInline(true);
            embedBuilder.AddField(fieldBuilder);

            List<string[]> maoUser = new List<string[]>();
            if (id == "aceitar-id")
            {
                Program.jogos[component.User.Id.ToString()].qntCartasUser += 2;
            } 
            else if (id == "bater-id" || id == "dobrar-id")
            {
                Program.jogos[component.User.Id.ToString()].qntCartasUser++;
            }
            
            for (int i = 0; i < Program.jogos[component.User.Id.ToString()].qntCartasUser; i++)
            {
                if(i > 1)
                {
                    maoUser.Add(baralho[i+2]);
                }
                else
                {
                    maoUser.Add(baralho[i]);
                }          
            }

            string stringMaoUser = maoParaString(maoUser);
            int[] valorMaoUser = valorMao(maoUser);

            List<string[]> maoBot = new List<string[]>();
            maoBot.Add(baralho[2]);
            string stringMaoBot = maoParaString(maoBot);
            int[] valorMaoBot = valorMao(maoBot);

            fieldBuilder = new EmbedFieldBuilder();
            if (valorMaoBot.Length == 1)
            {
                fieldBuilder.WithName(String.Format("Cartas da casa ({0}):", valorMaoBot[0]));
            }
            else
            {
                fieldBuilder.WithName(String.Format("Cartas da casa ({0} | {1}):", valorMaoBot[0], valorMaoBot[1]));
            }
            fieldBuilder.WithValue(String.Format("{0}.{1} | 🃏", maoBot[0][0], maoBot[0][1]));
            fieldBuilder.WithIsInline(false);
            embedBuilder.AddField(fieldBuilder);

            fieldBuilder = new EmbedFieldBuilder();
            if (valorMaoUser.Length == 1)
            {
                fieldBuilder.WithName(String.Format("Suas cartas ({0}):", valorMaoUser[0]));
            }
            else
            {
                fieldBuilder.WithName(String.Format("Suas cartas ({0} | {1}):", valorMaoUser[0], valorMaoUser[1]));
            }
            fieldBuilder.WithValue(stringMaoUser);
            fieldBuilder.WithIsInline(false);
            embedBuilder.AddField(fieldBuilder);

            if (valorMaoUser[0] >= 21)
            {
                await component.UpdateAsync(x => { x.Embed = embedBuilder.Build(); x.Components = new ComponentBuilder().Build(); });
                FimJogo(component, maoUser, maoBot, aposta, balance);
            }
            else if (id == "dobrar-id")
            {
                embedBuilder.Fields[0].Value = String.Format("{0}", aposta);
                await component.UpdateAsync(x => { x.Embed = embedBuilder.Build(); x.Components = new ComponentBuilder().Build(); });
                FimJogo(component, maoUser, maoBot, aposta*2, balance);
            }
            else
            {
                var componentBuilder = new ComponentBuilder()
                   .WithButton("Bater", "bater-id", ButtonStyle.Success, new Emoji("\uD83C\uDCCF"))
                   .WithButton("Manter", "manter-id", ButtonStyle.Danger, new Emoji("\uD83D\uDED1"))
                   .WithButton("Dobrar", "dobrar-id", ButtonStyle.Primary, new Emoji("\u0032\u20E3"), disabled: Math.Floor(balance / 2.0) < aposta || Program.jogos[component.User.Id.ToString()].qntCartasUser > 2);

                await component.UpdateAsync(x => { x.Embed = embedBuilder.Build(); x.Components = componentBuilder.Build(); });
            }
        }

        public async Task HandleButton(SocketMessageComponent component)
        {
            switch (component.Data.CustomId)
            {
                case "aceitar-id":
                    if (component.Message.Embeds.First().Footer.GetValueOrDefault().ToString().Split(' ', ':').Where(e => e != "").ToArray()[2] == component.User.Id.ToString() && Program.jogos.ContainsKey(component.User.Id.ToString()))
                    {
                        MostraJogo(component, component.Data.CustomId);
                    }
                    else
                    {
                        await component.DeferAsync();
                    }
                    break;
                case "cancelar-id":
                    if (component.Message.Embeds.First().Footer.GetValueOrDefault().ToString().Split(' ', ':').Where(e => e != "").ToArray()[2] == component.User.Id.ToString() && Program.jogos.ContainsKey(component.User.Id.ToString()))
                    {
                        await Program.jogos[component.User.Id.ToString()].chamadaComando.DeleteAsync();
                        await component.Message.DeleteAsync();
                        Program.jogos.Remove(component.User.Id.ToString());
                    }
                    else
                    {
                        await component.DeferAsync();
                    }
                    break;
                case "bater-id":
                    if (component.Message.Embeds.First().Footer.GetValueOrDefault().ToString().Split(' ', ':').Where(e => e != "").ToArray()[2] == component.User.Id.ToString() && Program.jogos.ContainsKey(component.User.Id.ToString()))
                    {
                        MostraJogo(component, component.Data.CustomId);
                    }
                    else
                    {
                        await component.DeferAsync();
                    }
                    break;
                case "manter-id":
                    if (Program.jogos.ContainsKey(component.User.Id.ToString()))
                    {
                        int aposta = Int32.Parse(component.Message.Embeds.First().Fields.ElementAt(0).Value);
                        int balance = Int32.Parse(component.Message.Embeds.First().Fields.ElementAt(1).Value);
                        List<string[]> maoBot = stringParaBaralho(component.Message.Embeds.First().Fields[2].Value);
                        List<string[]> maoUser = stringParaBaralho(component.Message.Embeds.First().Fields[3].Value);
                        await component.DeferAsync();
                        FimJogo(component, maoUser, maoBot, aposta, balance);
                    }
                    else
                    {
                        await component.DeferAsync();
                    }
                    break;
                case "dobrar-id":
                    if (component.Message.Embeds.First().Footer.GetValueOrDefault().ToString().Split(' ', ':').Where(e => e != "").ToArray()[2] == component.User.Id.ToString() && Program.jogos.ContainsKey(component.User.Id.ToString()))
                    {
                        MostraJogo(component, component.Data.CustomId);
                    }
                    else
                    {
                        await component.DeferAsync();
                    }
                    break;
            }
        }
    }
}
