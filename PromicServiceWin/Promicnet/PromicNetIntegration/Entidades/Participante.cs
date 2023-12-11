namespace PromicnetIntegration.Types
{
    /// <summary>
    /// Classe responsável por gerenciar os participantes da sessão plenária
    /// </summary>
    public class Participante
    {
        /// <summary>
        /// User_email/login do participante
        /// </summary>
        public string LOGIN { get; set; }
        /// <summary>
        /// ID Agora do Participante
        /// </summary>
        public string ID_AGORA { get; set; }
        /// <summary>
        /// Status da câmera do participante
        /// </summary>
        public bool CAMERA { get; set; }
        /// <summary>
        /// Status do microfone do participante
        /// </summary>
        public bool MICROFONE { get; set; }
    }
}

