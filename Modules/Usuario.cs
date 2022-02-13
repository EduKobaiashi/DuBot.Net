using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuBot.Modules
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.Int64)]
        public ulong _id { get; set; }

        [BsonElement("name")]
        public string name { get; set; }

        [BsonElement("pontos")]
        public int pontos { get; set; }

        [BsonElement("ultimoDaily")]
        public string ultimoDaily { get; set; }

        public Usuario(ulong id, string name, int pontos, string ultimoDaily)
        {
            _id = id;
            this.name = name;
            this.pontos = pontos;
            this.ultimoDaily = ultimoDaily;
        }
    }
}
