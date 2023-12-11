namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando um orador for inscrito.
    /// </summary>
    public partial class OradorInscritoEvent : EventBase
    {
        /// <summary>
        /// Email do 
        /// </summary>
        public string User_email { get; }
        /// <summary>
        /// Classe responsável pelo evento
        /// </summary>
        /// <param name="acao"></param>
        /// <param name="id_promicnet"></param>
        /// <param name="user_email"></param>
        /// <param name="id_sala"></param>
        public OradorInscritoEvent(string acao, string id_promicnet, string user_email, int id_sala)
        {
            Acao = acao;
            Id_promicnet = id_promicnet;
            User_email = user_email;
            Id_sala = id_sala;
        }
    }
}
