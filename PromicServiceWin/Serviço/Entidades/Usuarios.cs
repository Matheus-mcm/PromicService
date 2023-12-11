namespace PromicService.Entites
{
    public class User
    {
        /// <summary>
        /// ID do usuário no Agora.
        /// </summary>
        public uint uid { get; set; }
        /// <summary>
        /// ID do delegado no Promic.
        /// </summary>
        public int id_delegado { get; set; }
        /// <summary>
        /// Nome do delegado.
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// Email do delegado.
        /// </summary>
        public string login { get; set; }
        /// <summary>
        /// Solicitação de palavra do delegado.
        /// </summary>
        public bool pedidopalavra { get; set; }
        /// <summary>
        /// Estado do microfone do Delegado.
        /// </summary>
        public bool mutedaudio { get; set; }
        /// <summary>
        /// Estado do vídeo do Delegado.
        /// </summary>
        public bool mutedvideo { get; set; }
        /// <summary>
        /// Estado da transmissão do vídeo do delegado.
        /// </summary>
        public bool getvideo { get; set; }
    }
}
