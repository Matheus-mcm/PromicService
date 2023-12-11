using PromicnetIntegration;
using System;
using static PromicnetIntegration.TypesDTO;
using PromicnetIntegration.Types;
using System.Collections.Generic;

namespace Functions
{
    /// <summary>
    /// Classe para mapear as classes do Promicnet para familiarizar com o Promic
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Converte os objetos de Usuário, do Promicnet, para Delegado, do Promic.
        /// </summary>
        /// <param name="user">Objeto usuário do Promicnet.</param>
        /// <returns>Retorna as informações de um Delegado.</returns>
        public Delegado ToDelegado(UserDTO user)
            => new Delegado()
            {
                ID_ORGAO = user.Client_id,
                LOGIN = user.Email,
                NOME = user.Full_name,
                ID_DELEGADO = user.User_id
            };
        /// <summary>
        /// Converte os objetos de Cliente, do Promicnet, para Orgao, do Promic.
        /// </summary>
        /// <param name="client">Objeto de cliente do Promicnet;</param>
        /// <returns>Retorna as informações de um Órgão.</returns>
        public PromicnetIntegration.Types.Orgao ToOrgao(ClientDTO client)
            => new Orgao()
            {
                ID_ORGAO = client.Client_id,
                DELEGADOS = client.Client_users,
                NOME = client.Client_name,
                MAX_PARTICIPANTES = client.Max_session_participants
            };
        /// <summary>
        /// Converte os objetos de Session, do Promicnet, para Reunião, do Promic.
        /// </summary>
        /// <param name="session">Objeto de sessão do Promicnet.</param>
        /// <returns>Retorna as informações de uma Reunião.</returns>
        public Reuniao ToReuniao(SessionDTO session)
        {
            return new Reuniao()
            {
                ID_PRMOICNET = session.Session_id,
                DESCRICAO = session.Session_name,
                DT_INICIO = session.Initial_date,
                SITUACAO = session.Status,
                ID_ORGAO = session.Client_id
            };
        }

        /// <summary>
        /// Converte string para Participante.
        /// </summary>
        /// <param name="participant">User_email do participante.</param>
        /// <returns>Retorna um objeto do tipo Particpante.</returns>
        public Participante ToParticipante(string participant)
            => new Participante()
            {
                LOGIN = participant
            };
        /// <summary>
        /// Converte um objeto Participant para Participante.
        /// </summary>
        /// <param name="participant"></param>
        /// <returns></returns>
        public Participante ToParticipante(ParticipantDTO participant)
            => new Participante()
            {
                ID_AGORA = participant.Agora_id,
                LOGIN = participant.User_email,
                CAMERA = participant.Camera_state == "ON",
            };
        /// <summary>
        /// Converte um objeto de AgoraUsage para UsoAgora
        /// </summary>
        /// <param name="usage">Informações de uso do Agora</param>
        /// <returns></returns>
        public UsoAgora ToUsoAgora(AgoraUsageDTO usage)
        {
            return new UsoAgora()
            {
                CONNECTED_TO = usage.Connected_to,
                FINAL = Convert.ToDateTime(usage.End_date),
                ID = usage.Usage_id,
                INICIO = Convert.ToDateTime(usage.Start_date),
                LOGIN = usage.User_email,
                SEGUNDOS = usage.Seconds,
                TIPO = usage.Type
            };
        }

        /// <summary>
        /// Converte um objeto do tipo UsageSession para USOSESSOES
        /// </summary>
        /// <param name="teste"></param>
        /// <returns></returns>
        public USOSESSOES ToReuniaoComUso(UsageSessionDTO teste)
        => new USOSESSOES()
        {
            DESCRICAO = teste.Session_name,
            ID_ORGAO = teste.Client_id,
            ID_PROMICNET = teste.Session_id,
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="votos"></param>
        /// <returns></returns>
        public List<VotoDTO> ToVotoDtoList(List<VOTO> votos)
        {
            List<VotoDTO> votes = new List<VotoDTO>();
            foreach (VOTO voto in votos)
            {
                votes.Add(new VotoDTO
                {
                    name = voto.NOME_PARTICIPANTE,
                    party = voto.PARTIDO,
                    vote = voto.VOTO_PARTICIPANTE,
                    voted = voto.VOTOU
                });
            }
            return votes;
        }
    }
}
