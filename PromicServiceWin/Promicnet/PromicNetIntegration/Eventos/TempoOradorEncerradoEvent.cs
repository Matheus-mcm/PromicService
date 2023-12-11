namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando um orador encerrar seu próprio tempo de inscrição.
    /// </summary>
    public class TempoOradorEncerradoEvent : EventBase
    {
        /// <summary>
        /// ID do participante.
        /// </summary>
        public int User_id { get; }
        /// <summary>
        /// Classe responsável pelo evento.
        /// </summary>
        /// <param name="acao"></param>
        /// <param name="id_promicnet"></param>
        /// <param name="user_id"></param>
        /// <param name="id_sala"></param>
        public TempoOradorEncerradoEvent(string acao, string id_promicnet, int user_id, int id_sala)
        {
            Acao = acao;
            Id_promicnet = id_promicnet;
            User_id = user_id;
            Id_sala = id_sala;
        }
    }
}