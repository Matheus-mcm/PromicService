namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado ao orador desinscrever.
    /// </summary>
    public partial class OradorDesinscritoEvent : EventBase
    {
        /// <summary>
        /// 
        /// </summary>
        public string User_email { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="acao"></param>
        /// <param name="id_promicnet"></param>
        /// <param name="user_email"></param>
        /// <param name="id_sala"></param>
        public OradorDesinscritoEvent(string acao, string id_promicnet, string user_email, int id_sala)
        {
            Acao = acao;
            Id_promicnet = id_promicnet;
            User_email = user_email;
            Id_sala = id_sala;
        }
    }
}
