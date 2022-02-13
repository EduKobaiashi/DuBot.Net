using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuBot.Modules
{
    public class Jogo
    {
        public List<string[]> baralho;
        public SocketUserMessage chamadaComando;
        public int qntCartasUser;
        public int qntCartasBot;
        public string idServidor;

        public Jogo(List<string[]> baralho, SocketUserMessage chamadaComando, int qntCartasUser, int qntCartasBot, string idServidor)
        {
            this.baralho = baralho;
            this.chamadaComando = chamadaComando;
            this.qntCartasUser = qntCartasUser;
            this.qntCartasBot = qntCartasBot;
            this.idServidor = idServidor;
        }
    }
}
