namespace PromicnetIntegration.Types
{
    /// <summary>
    /// Classe responsável por gerenciar o token de autenticação usado na integração com a API da PromicNet.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Orgao ID do cognito
        /// </summary>
        public readonly string client_id;
        /// <summary>
        /// Secret key do cognito
        /// </summary>
        public readonly string client_secret;
        /// <summary>
        /// url do cognito
        /// /// </summary>
        public readonly string url;
        /// <summary>
        /// Responsável por controlar a validez do token.
        /// </summary>
        public bool Valid { get; private set; }
        /// <summary>
        /// Token de acesso gerado pela API da PromicNet.
        /// </summary>
        public string Access_token { get; set; }
        /// <summary>
        /// Tempo de vida do token em segundos.
        /// </summary>
        public int Expires_in { get; set; }
        /// <summary>
        /// Tipo de token gerado.
        /// </summary>
        public string Token_type { get; set; }
        /// <summary>
        /// Data e hora de criação do token.
        /// </summary>
        public System.DateTime Created_date { get; set; }
        /// <summary>
        /// Cria uma nova instância da classe Token com todos os parâmetros vazios.
        /// </summary>
        public Token() { }
        /// <summary>
        /// Cria uma nova instância da classe Token com as credenciais do cliente (client_id e client_secret) e a URL da API da PromicNet.
        /// </summary>
        /// <param name="url">URL da API da PromicNet.</param>
        /// <param name="client_id">Credencial de identificação do cliente.</param>
        /// <param name="client_secret">Credencial secreta do cliente.</param>
        public Token(string url, string client_id, string client_secret)
        {
            this.client_id = client_id;
            this.client_secret = client_secret;
            this.url = url;

            if (!url.EndsWith("/oauth2/token"))
            {
                this.url += "/oauth2/token";
            }
        }
        /// <summary>
        /// Verifica se o token de acesso gerado ainda é válido com base na data de criação do token e no tempo de vida definido para ele. 
        /// Se o token estiver vazio ou expirado, a propriedade "valid" da classe Token será definida como false. 
        /// Se o token ainda estiver válido, a propriedade "valid" será definida como true. 
        /// Não há retorno deste método, pois ele apenas atualiza a propriedade "valid" da classe Token.
        /// </summary>
        public void IsValid(Token tokn)
        {
            if (System.String.IsNullOrEmpty(tokn.Access_token))
                tokn.Valid = false;
            else if (tokn.Created_date.AddSeconds(tokn.Expires_in) < System.DateTime.Now)
                tokn.Valid = false;
            else
                tokn.Valid = true;
        }
    }
}

