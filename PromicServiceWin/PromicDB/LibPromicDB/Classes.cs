using PromicnetIntegration;
using PromicnetIntegration.Types;
using PromicnetWebsocket;
using PromicnetWebsocket.Messages;
using System;
using System.Collections.Generic;
using static PromicnetIntegration.TypesDTO;

namespace LibPromicDB
{
    public class ControlesLista
    {
        public ControlesLista()
        {
            this.ListaOradores = new List<Oradores>();
            this.PAUTA = new PautaDTO { parts = new List<ParteDTO>() };
            this.ListaPresenca = new List<Presence>();
            this.ListaVotoNominal = new List<VotoNominal>();
        }

        public List<Oradores> ListaOradores { get; set; }
        public PautaDTO PAUTA { get; set; }
        public List<Presence> ListaPresenca { get; set; }
        public PromicnetIntegration.PromicWebSocket WEBSOCKET { get; set; }
        public string ORGAO { get; set; }
        public int ID_SALA { get; set; }
        public int ID_MODELO { get; set; }
        public int ID_REUNIAO { get; set; }
        public int ID_PARTE { get; set; }
        public int ID_PARTE_ANTERIOR { get; set; }
        public int ID_DELEGADO { get; set; }
        public int ID_VOTACAO { get; set; }
        public int ID_RAPIDO { get; set; }
        public int ID_APARTE { get; set; }
        public string TIPOVOT { get; set; }
        public string TIPOPARTE { get; set; }
        public int RESULTADO { get; set; }
        public string NOMINAL { get; set; }
        public int PRESENCA { get; set; }
        public DateTime CRONO_PARTE { get; set; }
        public TimeSpan TEMPO_PARTE { get; set; }
        public DateTime CRONO_ORADOR { get; set; }
        public TimeSpan TEMPO_ORADOR { get; set; }
        public DateTime CRONO_VOTACAO { get; set; }
        public TimeSpan TEMPO_VOTACAO { get; set; }
        public DateTime CRONO_PRESENCA { get; set; }
        public TimeSpan TEMPO_PRESENCA { get; set; }
        public int PAUSA_PARTE { get; set; }
        public int PAUSA_ORADOR { get; set; }
        public int PAUSA_VOTACAO { get; set; }
        public int PAUSA_PRESENCA { get; set; }
        public int CORADORES { get; set; }
        public int CVOTACAO { get; set; }
        public int CVOTACAOORADORES { get; set; }
        public int CAPARTE { get; set; }
        public int DEBATE { get; set; }
        public int ESTADO_ANTERIOR { get; set; }
        public int ESTADO_ATUAL { get; set; }
        public DateTime CRONO_APARTE { get; set; }
        public TimeSpan TEMPO_APARTE { get; set; }
        public DateTime CRONO_MATERIA { get; set; }
        public TimeSpan TEMPO_MATERIA { get; set; }
        public int PAUSA_MATERIA { get; set; }
        public VotacaoTotais VotacaoTotais { get; set; }
        public List<VotoNominal> ListaVotoNominal { get; set; }
        public bool TEMPO_ORADOR_ATUALIZA { get; set; }
        public bool PAUSA_ORADOR_ATUALIZA { get; set; }
        public bool TEMPO_VOTACAO_ATUALIZA { get; set; }
        public bool PAUSA_VOTACAO_ATUALIZA { get; set; }
        public bool TEMPO_PARTE_ATUALIZA { get; set; }
        public bool PAUSA_PARTE_ATUALIZA { get; set; }
        public int PAUSA_APARTE { get; set; }
    }

    public class Oradores
    {
        public int ID_ORADOR { get; set; } = 0;
        public int ID_PARTE { get; set; } = 0;
        public int ID_DELEGADO { get; set; } = 0;
        public string NOME { get; set; } = String.Empty;
        public string PARTIDO { get; set; } = String.Empty;
        public string SITUACAO { get; set; } = String.Empty;
        public int TEMPO { get; set; } = 0;
    }
    public class VotacaoTotais
    {
        public VotacaoTotais()
        {
        }

        public int ID_VOTACAO { get; set; }
        public int TURNO { get; set; }
        public int VOTO1 { get; set; }
        public int VOTO2 { get; set; }
        public int VOTO3 { get; set; }
        public int VOTO4 { get; set; }
        public int VOTO5 { get; set; }
        public int TOTALVOTOS { get; set; }
        public int RESPOSTA { get; set; }
        public int RESULTADO { get; set; }
        public string RESULTADOSTR { get; set; }
        public string SECRETO { get; set; }
        public string REAL { get; set; }
        public string MINERVA { get; set; }
        public string QDMINERVA { get; set; }
        public int VOTOSNECES { get; set; }
        public string TITULO { get; set; }
        public string EMENTA { get; set; }
    }

    public class VotoNominal
    {
        public VotoNominal()
        {
        }

        public int ID_DELEGADO { get; set; }
        public string NOME { get; set; }
        public string PARTIDO { get; set; }
        public string VOTO { get; set; }
        public string VOTOX { get; set; }
        public string DEVEVOTAR { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class Pauta
    {
        public string action { get; set; }
        public string session_id { get; set; }
        public List<Parte> parts { get; set; }
    }
    public class Parte
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool speaker { get; set; }
        public List<Materia> subjects { get; set; }
    }

    public class Materia
    {
        public string action { get; set; }
        public string session_id { get; set; }
        public int id { get; set; }
        public string acronym { get; set; }
        public string name { get; set; }
        public List<Author> authors { get; set; }
        public string description { get; set; }
    }
    public partial class Author
    {
        /// <summary>
        /// Nome do delegado
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Partido
        /// </summary>
        public string party { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>

    public class Votacao
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public int tipo_votacao { get; set; }
        public int vote_time { get; set; }
        public int user_id { get; set; }
    }
    public class Delegado
    {
        public string Nome { get; set; }
        public string Partido { get; set; }
    }
    public class HReturn
    {
        public object Return { get; }
        public int PrimaryKey { get; }
        public string Message { get; }
        public int Code { get; }
        public bool Status { get; }

        public HReturn(bool status, string msg, int code, int pk, object returned)
        {
            Status = status;
            Message = msg;
            Code = code;
            PrimaryKey = pk;
            Return = returned;
        }
    }
}
