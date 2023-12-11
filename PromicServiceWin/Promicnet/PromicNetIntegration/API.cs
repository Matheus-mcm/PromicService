using Functions;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using PromicnetIntegration.Types;
using static PromicnetIntegration.TypesDTO;
using System.Security.Policy;

namespace PromicnetIntegration
{
    /// <summary>Classe com todos os métodos para a API do Promicnet</summary>
    public class Api
    {
        /// <summary>Classe padrão para fazer as requisições para o Promicnet.</summary>
        private readonly RestClient client;
        /// <summary>Classe com funções auxiliares.</summary>
        private readonly Util utils;
        /// <summary>Classe para mapear os objetos para o Promic.</summary>
        private readonly Map map;
        /// <summary>Opções padrão para deserializar os objetos retornado pelo Promicnet.</summary>
        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, PropertyNameCaseInsensitive = true };
        /// <summary>Construtor da classe para integração com a API do Promicnet</summary>
        /// <param name="url">URL do ambiente.</param>
        public Api(string url)
        {
            client = new RestClient(url); 
            utils = new Util(); 
            map = new Map();
        }
        #region LOG
        /// <summary>Evento de disparo de mensagens de execução do código.</summary>
        public event EventHandler<LogData> LogEvent;
        /// <summary>Responsável por disparar o evento de mensagem.</summary>
        /// <param name="function">Nome da função que está disparando o evento.</param>
        /// <param name="message">Mensagem a ser disparada.</param>
        /// <param name="status">Mensagem a ser disparada.</param>
        public virtual void Log(string status, string function, string message) => LogEvent?.Invoke(this, new LogData(@class: "Api", status: status, function: function, message: message));
        #endregion
        #region TOKEN
        /// <summary>Responsável por gerar um Token de autenticação para acessar a API do PromicNet.</summary>
        /// <param name="token">Objeto do tipo Token contendo os parâmetros necessários para gerar o token.</param>
        /// <exception cref="ArgumentNullException">Uma das propriedades do Token(Client_id, client_secret ou url) está vazia.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<Token>> ObterTokenPromic(Token token)
        {
            HReturn<Token> result = new HReturn<Token>();
            if (string.IsNullOrEmpty(token.client_id) || string.IsNullOrEmpty(token.client_secret))
            {
                Log("400", "GetToken", "Par de chaves não pode ser vazio!");
                result.MENSAGEM = "Par de chaves não pode ser vazio!";
                result.SUCESSO = false;
                return result;
            }
            if (string.IsNullOrEmpty(token.url) || !Uri.IsWellFormedUriString(token.url, UriKind.Absolute))
            {
                Log("400", "GetToken", $"URL não é válida.");
                result.MENSAGEM = "URL não é válida.";
                result.SUCESSO = false;
                return result;
            }

            RestRequest req = new RestRequest(token.url, Method.Post);

            req.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            req.AddParameter("grant_type", "client_credentials");
            req.AddParameter("scope", "scopes/system");
            req.AddParameter("client_id", token.client_id);
            req.AddParameter("client_secret", token.client_secret);
            try
            {
                RestResponse response = await client.ExecuteAsync(req);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Token tokn = JsonSerializer.Deserialize<Token>(response.Content, serializerOptions);

                    token.Expires_in = tokn.Expires_in;
                    token.Created_date = DateTime.Now;
                    token.Access_token = tokn.Access_token;
                    token.Token_type = tokn.Token_type;
                    token.IsValid(token);

                    result.OBJETO = token;
                    result.SUCESSO = true;
                    result.MENSAGEM = "Token gerado com sucesso.";

                    Log($"{(int)response.StatusCode}", "GetToken", "Token gerado com sucesso!");
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "GetToken", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "GetToken", $"{e.TargetSite.Name} | {e.Message}");
                result.SUCESSO = false;
                result.MENSAGEM = $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        #endregion
        #region USER
        /// <summary>Responsável por cadastrar um novo usuário em um órgão.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="email">User_email do usuário.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <param name="full_name">Nome do usuário.</param>
        /// <param name="id_delegado">ID do Delegado no Promic.</param>
        /// <returns>Retorna um objeto do tipo ` Result` com o objeto do tipo ` Delegado` contendo todas as informações dentro da propriedade ` Result.Item`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando uma, ou mais, das propriedades do usuário (User_email, órgão ou nome) está incorreta.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<Delegado>> CadastrarDelegado(Token token, string email, string orgao, string full_name, int id_delegado)
        {
            HReturn<Delegado> result = new HReturn<Delegado>();

            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "CadastrarDelegado", "Email não pode ser inválido!");
                result.MENSAGEM = "Email não pode ser inválido!";
                result.SUCESSO = false;
                return result;
            }
            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "CadastrarDelegado", "ID do cliente não pode ser vazio!");
                result.MENSAGEM = "ID do cliente não pode ser vazio!";
                result.SUCESSO = false;
                return result;
            }
            if (string.IsNullOrEmpty(full_name))
            {
                Log("400", "CadastrarDelegado", "Nome do usuário não pode ser vazio!");
                result.MENSAGEM = "Nome do usuário não pode ser vazio!";
                result.SUCESSO = false;
                return result;
            }

            RestRequest request = new RestRequest("/user", Method.Post);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            request.AddHeader("Content-Type", "text/plain");
            request.AddParameter("text/plain", $"{{\"email\":\"{email}\",\"client_id\":\"{orgao}\",\"full_name\":\"{full_name}\", \"user_id\":{id_delegado}}}", ParameterType.RequestBody);

            try
            {
                RestResponse response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "CadastrarDelegado", $"Delegado {email} criado com sucesso!");
                    result.SUCESSO = true;
                    result.MENSAGEM = $"Delegado {email} criado com sucesso.";
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "CadastrarDelegado", $"{response.ErrorException.Message} | {response.Content}");
                    result.SUCESSO = false;
                    result.MENSAGEM = "CadastrarDelegado: " + $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "RegisterUser", $"{e.TargetSite.Name} | {e.Message}");
                result.SUCESSO = false;
                result.MENSAGEM = "RegisterUser: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Responsável por buscar as informações de um usuário através do login do usuário (User_email).</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="email">User_email do usuário.</param>
        /// <returns>Retorna um objeto do tipo ` Result` com um objeto do tipo ` Delegado` contendo todas as informações dentro da propriedade ` Result.Item`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o User_email do usuário está inválido.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<Delegado>> ObterDelegado(Token token, string email)
        {
            HReturn<Delegado> result = new HReturn<Delegado>();

            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "ObterDelegado", "Email do usuário está inválido!");
                result.MENSAGEM = "Email do usuário está inválido!";
                result.SUCESSO = false;
                return result;
            }

            RestRequest request = new RestRequest($"/user/{email}", Method.Get);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    UserDTO userDTO = JsonSerializer.Deserialize<UserDTO>(response.Content, serializerOptions);

                    Delegado obj = map.ToDelegado(userDTO);

                    Log($"{(int)response.StatusCode}", "ObterDelegado", $"Delegado {email} obtido com sucesso!");

                    result.SUCESSO = true;
                    result.OBJETO = obj;
                    result.MENSAGEM = $"Delegado {email} obtido com sucesso!";
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "ObterDelegado", $"{response.ErrorException.Message} | {response.Content}");
                    result.SUCESSO = false;
                    result.MENSAGEM = "ObterDelegado: " + $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "ObterDelegado", $"{e.TargetSite.Name} | {e.Message}");
                result.SUCESSO = false;

                result.MENSAGEM = "ObterDelegado: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Responsável por trazer uma lista com todos os usuários de um órgão.</summary>
        /// <param name="token">Token de Autenticação.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <returns>Retorna uma lista com todos os usuários de um órgão.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK do órgão está vazio.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<List<Delegado>>> ObterListaDelegado(Token token, string orgao)
        {
            HReturn<List<Delegado>> result = new HReturn<List<Delegado>>();

            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "ObterListaDelegado", "ID do órgão não pode ser vazio!");
                result.MENSAGEM = "Email do usuário está inválido!";
                return result;
            }

            RestRequest request = new RestRequest($"/user/list/{orgao}", Method.Get);
            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            List<Delegado> obj = new List<Delegado>();
            try
            {
                RestResponse response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    List<UserDTO> userList = JsonSerializer.Deserialize<List<UserDTO>>(response.Content, serializerOptions);

                    foreach (UserDTO user in userList)
                    {
                        obj.Add(map.ToDelegado(user));
                    }

                    Log($"{(int)response.StatusCode}", "ObterListaDelegado", "Lista de delegados obtidos com sucesso!");
                    result.SUCESSO = true;
                    result.OBJETO = obj;
                    result.MENSAGEM = "Lista de delegados obtidos com sucesso!";
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "ObterListaDelegado", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "ObterListaDelegado: " + $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "ObterListaDelegado", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "ObterListaDelegado: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Atualiza as informações de um usuário.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="email">User_email do usuário.</param>
        /// <param name="new_email">User_email que deverá ser alterado usuário.</param>
        /// <param name="full_name">Novo nome do usuário.</param>
        /// <param name="orgao">Objeto com as informações do usuário.</param>
        /// <param name="id_delegado">ID do Delegado no Promic.</param>
        /// <returns>Retorna um objeto do tipo  Result com o objeto do tipo  Delegado.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando uma, ou mais, das propriedades do usuário (User_email, User_email novo, órgão ou nome) está incorreta.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<bool>> AtualizaDelegado(Token token, string email, string new_email, string full_name, string orgao, int id_delegado)
        {
            HReturn<bool> result = new HReturn<bool>();

            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "UpdateUser", "Email do usuário está vazio ou inválido.");
                result.MENSAGEM = "Email do usuário está vazio ou inválido.";
                return result;
            }
            if (string.IsNullOrEmpty(full_name))
            {
                Log("400", "UpdateUser", "Nome do usuário não pode ser vazio!");
                result.MENSAGEM = "Nome do usuário não pode ser vazio!";
                return result;
            }
            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "UpdateUser", "SK do client não pode ser vazio!");
                result.MENSAGEM = "SK do client não pode ser vazio!";
                return result;
            }
            if (string.IsNullOrEmpty(new_email))
            {
                new_email = email;
            }
            else if (!utils.IsValidEmail(new_email))
            {
                Log("400", "UpdateUser", "O novo email do usuário não é válido!");
                result.MENSAGEM = "O novo email do usuário não é válido!";
                return result;
            }

            RestRequest request = new RestRequest($"/user/{email}", Method.Patch);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            request.AddHeader("Content-Type", "text/plain");
            request.AddParameter("text/plain", "{\"email\":\"" + new_email + "\",\"client_id\":\"" + orgao + "\",\"full_name\":\"" + full_name + "\", \"user_id\":" + id_delegado + "}", ParameterType.RequestBody);

            try
            {
                RestResponse response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "UpdateUser", $"Usuário {email} atualizado com sucesso!");
                    result.SUCESSO = true;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "UpdateUser", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "UpdateUser: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "UpdateUser", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "UpdateUser: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Deleta o usuário a partir do seu User_email.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="email">User_email do usuário.</param>
        /// <returns>Retorna um objeto do tipo   Result com o objeto do tipo  Delegado.</returns>>
        /// <exception cref="ArgumentException">Exceção lançada quando o User_email do usuário está incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<bool>> DeletaDelegado(Token token, string email)
        {
            HReturn<bool> result = new HReturn<bool>();

            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "DeleteUser", "Email do usuário está inválido!");
                result.MENSAGEM = "Email do usuário está inválido!";
                return result;
            }

            RestRequest request = new RestRequest($"/user/{email}", Method.Delete);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "DeleteUser", $"Usuário deletado com sucesso!");
                    result.SUCESSO = true;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "DeleteUser", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "DeleteUser: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "DeleteUser", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "UpdateUser: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        #endregion
        #region ÓRGÃO
        /// <summary>Obtém as informações de um órgão a partir do SK dele.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK do órgão está vazio.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<Types.Orgao>> BuscaOrgao(Token token, string orgao)
        {
            HReturn<Types.Orgao> result = new HReturn<Types.Orgao>();

            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "BuscaOrgao", "ID do orgão não pode ser vazio!");
                result.MENSAGEM = "ID do órgão não pode ser vazio!";
                return result;
            }
            RestRequest request = new RestRequest($"/client/{orgao}", Method.Get);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    ClientDTO clientDTO = JsonSerializer.Deserialize<ClientDTO>(response.Content, serializerOptions);
                    Types.Orgao obj = map.ToOrgao(clientDTO);
                    Log($"{(int)response.StatusCode}", "BuscaOrgao", $"Órgão {orgao} obtido com sucesso!");
                    result.SUCESSO = true;
                    result.OBJETO = obj;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "BuscaOrgao", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "BuscaOrgao: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "BuscaOrgao", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "BuscaOrgao: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Obtém uma lista de todos os órgãos.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <returns>Retorna um objeto do tipo  Result com um list de objetos do tipo Orgao.</returns>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<List<Types.Orgao>>> BuscaListaOrgao(Token token)
        {
            HReturn<List<Types.Orgao>> result = new HReturn<List<Types.Orgao>>();

            RestRequest request = new RestRequest("/client", Method.Get);
            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            List<Types.Orgao> obj = new List<Types.Orgao>();

            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {

                    Result<List<ClientDTO>> clientList = JsonSerializer.Deserialize<Result<List<ClientDTO>>>(response.Content, serializerOptions);

                    foreach (ClientDTO clientw in clientList.Clients)
                    {
                        obj.Add(map.ToOrgao(clientw));
                    }
                    Log($"{(int)response.StatusCode}", "BuscaListaOrgao", "Lista de órgãos obtida com sucesso!");
                    result.SUCESSO = true;
                    result.OBJETO = obj;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "BuscaListaOrgao", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "BuscaListaOrgao: " + $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "BuscaListaOrgao", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "BuscaListaOrgao: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Registra um novo órgão no Promicnet.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="nome_orgao">Nome do órgão.</param>
        /// <param name="max_participants">Nome do órgão.</param>
        /// <exception cref="ArgumentException">Nome do órgão está vazia.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<Types.Orgao>> CadastrarOrgao(Token token, string nome_orgao, int max_participants)
        {
            HReturn<Types.Orgao> result = new HReturn<Types.Orgao>();

            if (string.IsNullOrEmpty(nome_orgao))
            {
                Log("400", "CriarOrgao", $"Nome do órgão não pode ser vazio!");
                result.MENSAGEM = "Nome do órgão não pode ser vazio!";
                return result;
            }

            RestRequest request = new RestRequest("/client", Method.Post);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            string body = $"{{\"client_name\":\"{nome_orgao}\",\"max_session_participants\":{max_participants}}}";

            request.AddParameter("application/json", body, ParameterType.RequestBody);


            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    ClientDTO cliente = JsonSerializer.Deserialize<ClientDTO>(response.Content, serializerOptions);

                    Types.Orgao obj = map.ToOrgao(cliente);

                    Log($"{(int)response.StatusCode}", "CriarOrgao", $"Órgão {nome_orgao} criado com sucesso!");
                    result.SUCESSO = true;
                    result.OBJETO = obj;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "CriarOrgao", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "CriarOrgao: " + $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "CriarOrgao", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "CriarOrgao: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Atualiza os dados do órgão.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="name">Nome do órgão.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <param name="max_participants">Número de participantes remotos.</param>
        /// <exception cref="ArgumentException">Exceção lançada quando o Nome ou o SK do órgão está incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<bool>> AtualizarOrgao(Token token, string name, string orgao, int max_participants)
        {
            HReturn<bool> result = new HReturn<bool>();

            if (string.IsNullOrEmpty(name))
            {
                Log("400", "AtualizarOrgao", "Nome do órgão não pode ser vazio!");
                result.MENSAGEM = "Nome do órgão não pode ser vazio!";
                return result;
            }
            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "AtualizarOrgao", "SK do orgão não pode ser vazio!");
                result.MENSAGEM = "SK do órgão não pode ser vazio!";
                return result;
            }

            RestRequest request = new RestRequest($"/client/{orgao}", Method.Patch);
            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            string body = $"{{\"client_name\":\"{name}\",\"max_session_participants\":{max_participants}}}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "AtualizarOrgao", $"Sucesso ao atualizar o orgão {orgao}");
                    result.SUCESSO = true;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "AtualizarOrgao", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "AtualizarOrgao: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "AtualizarOrgao", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "AtualizarOrgao: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Responsável por remover um órgão.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <returns>Retorna um objeto do tipo ` Result` com o resultado da requisição na propriedade ` Result.Success`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK do órgão está vazio.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<bool>> DeletaOrgao(Token token, string orgao)
        {
            HReturn<bool> result = new HReturn<bool>();

            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "DeletaOrgao", "SK do órgão não pode ser vazio!");
                result.MENSAGEM = "SK do órgão não pode ser vazio!";
                return result;
            }

            RestRequest resquest = new RestRequest($"/client/{orgao}", Method.Delete);
            resquest.AddHeader("Authorization", token.Access_token);

            try
            {
                RestResponse response = await client.ExecuteAsync(resquest);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "DeletaOrgao", $"Órgão {orgao} deletado com sucesso!");
                    result.SUCESSO = true;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "DeletaOrgao", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "DeletaOrgao: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "DeletaOrgao", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "DeletaOrgao: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        #endregion
        #region SESSIONS
        /// <summary>Cria uma nova sessão para um órgão a partir do SK do órgão.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <param name="sessionName">Nome da sessão.</param>
        /// <param name="sessionDate">Data para qual a reunião será agendada.</param>
        /// <param name="modo">Modo da reunião.</param>
        /// <returns>Retorna um objeto do tipo ` Result` com um objeto do tipo `Reuniao` contendo as informações da sessão na propriedade ` Result.Item`.</returns>
        /// <exception cref="ArgumentException">Excelçao lançada quando um dos parâmetros orgao ou sessionName estão incorretos.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<Reuniao>> CadastrarReuniao(Token token, string orgao, string sessionName, DateTime sessionDate, string modo)
        {
            HReturn<Reuniao> result = new HReturn<Reuniao>();

            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "CreateSession", "ID do órgão não pode ser vazio!");
                result.MENSAGEM = "ID do órgão não pode ser vazio!";
                return result;
            }
            if (string.IsNullOrEmpty(sessionName))
            {
                Log("400", "CreateSession", "Nome da sessão não pode ser vazio!");
                result.MENSAGEM = "Nome da sessão não pode ser vazio!";
                return result;
            }

            if (modo.Equals("R"))
            {
                modo = "ASK_TO_SPEAK";
            }
            else
            {
                modo = "MANUAL";
            }

            RestRequest request = new RestRequest("/promic/session/" + orgao, Method.Post);
            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody($"{{\"session_name\":\" {sessionName}\",\"initial_date\":\"{sessionDate:s}\", \"mode\":\"{modo}\"}}", DataFormat.Json);


            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    SessionDTO sessionDTO = JsonSerializer.Deserialize<SessionDTO>(response.Content, serializerOptions);
                    Reuniao obj = map.ToReuniao(sessionDTO);
                    Log($"{(int)response.StatusCode}", "CreateSession", $"Sessão ({result}) criada com sucesso!");
                    result.SUCESSO = true;
                    result.OBJETO = obj;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "CreateSession", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "CreateSession: " + $"{response.ErrorException.Message} | {response.Content}";
                }


            }
            catch (Exception e)
            {
                Log("502", "CreateSession", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "CreateSession: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Obtém a lista de sessão de um órgão a partir do seu SK.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <returns>Retorna um objeto do tipo ` Result` com uma lista do tipo `Reuniao` contendo todas as sessões na propriedade ` Result.Items`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK do órgão está incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<List<Reuniao>>> ObterListaReuniao(Token token, string orgao)
        {
            HReturn<List<Reuniao>> result = new HReturn<List<Reuniao>>();

            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "GetSessionsList", "SK do órgão não pode ser vazio!");
                result.MENSAGEM = "SK do órgão não pode ser vazio!";
                return result;
            }

            RestRequest request = new RestRequest($"/promic/session/list/{orgao}", Method.Get);
            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            List<Reuniao> obj = new List<Reuniao>();
            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Result<List<SessionDTO>> sessionList = JsonSerializer.Deserialize<Result<List<SessionDTO>>>(response.Content, serializerOptions);
                    foreach (SessionDTO session in sessionList.Sessions)
                    {
                        obj.Add(map.ToReuniao(session));
                    }
                    Log($"{(int)response.StatusCode}", "GetSessionsList", $"Lista de sessões do órgão {orgao} obtida com sucesso!");
                    result.SUCESSO = true;
                    result.OBJETO = obj;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "GetSessionsList", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "GetSessionsList: " + $"{response.ErrorException.Message} | {response.Content}";
                }


            }
            catch (Exception e)
            {
                Log("502", "GetSessionsList", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "GetSessionsList: " + $"{e.TargetSite.Name} | {e.Message}";
            }

            return result;
        }
        /// <summary>Adiciona um participante em uma sessão a partir do seu SK e do User_email do usuário.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="email">User_email do participante.</param>
        /// <returns>Retorna um objeto do tipo ` Result`, podendo validar o resultado pela propriedade ` Result.Success`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK da sessão ou o User_email do participante está incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<bool>> AdicionarParticipanteReuniao(Token token, string id_promic, string email)
        {
            HReturn<bool> result = new HReturn<bool>();

            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "AdicionarParticipanteReuniao", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK da sessão não pode ser vazio.";
                return result;
            }
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "AdicionarParticipanteReuniao", "Email vazio ou inválido.");
                result.MENSAGEM = "Email vazio ou inválido.";
                return result;
            }

            RestRequest request = new RestRequest("/promic/session/addUser/" + id_promic, Method.Patch);
            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody("{\"user_email\":\"" + email + "\"}", DataFormat.Json);

            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "AdicionarParticipanteReuniao", $"Usuário {email} adicionado na sessão {id_promic} com sucesso.");
                    result.SUCESSO = true;
                    result.OBJETO = true;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "AdicionarParticipanteReuniao", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "AdicionarParticipanteReuniao: " + $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "AdicionarParticipanteReuniao", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "AdicionarParticipanteReuniao: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Atualiza o status de uma sessão a partir do seu SK.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <param name="status">Status a ser alterado.</param>
        /// <returns>Retorna um objeto do tipo ` Result`, podendo validar o resultado pela propriedade ` Result.Success`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada caso o SK da sessão ou o status esteja vazio.</exception>
        /// <exception cref="Exception">Exceção genérica lançada caso ocorra um erro inesperado.</exception>
        public async Task<HReturn<bool>> AtualizaSituacaoReuniao(Token token, string orgao, string id_promic, string status)
        {
            HReturn<bool> result = new HReturn<bool>();

            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "AtualizaSituacaoReuniao", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK da sessão não pode ser vazio.";
                return result;
            }
            if (string.IsNullOrEmpty(status))
            {
                Log("400", "AtualizaSituacaoReuniao", "Status não pode ser vazio.");
                result.MENSAGEM = "Status não pode ser vazio.";
                return result;
            }

            RestRequest request = new RestRequest("/promic/session/status/" + orgao + "/" + id_promic, Method.Patch);
            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            request.AddHeader("Content-Type", "application/json");

            request.AddStringBody("{\"status\":\"" + status + "\"}", DataFormat.Json);


            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "AtualizaSituacaoReuniao", $"Sessão {id_promic} alterada para o status {status}");
                    result.SUCESSO = true;
                    result.OBJETO = true;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "AtualizaSituacaoReuniao", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "AtualizaSituacaoReuniao: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "AtualizaSituacaoReuniao", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "AtualizaSituacaoReuniao: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Atualiza o nome e/ou a data inicial de uma sessão a partir do SK da sessão e do SK do órgão.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="session_name">Nome da sessão.</param>
        /// <param name="initial_date">Data de início da sessão.</param>
        /// <param name="modo">Modo do microfone da sessão no Promicnet. utilize "R" para modo de requisição e "M" para modo manual. </param>
        /// <returns>Retorna um objeto do tipo ` Result`, podendo validar o resultado pela propriedade ` Result.Success`.</returns>
        public async Task<HReturn<bool>> AtualizaInformacaoReuniao(Token token, string orgao, string id_promic, string session_name, DateTime initial_date, string modo)
        {
            HReturn<bool> result = new HReturn<bool>();

            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "AtualizaInformacaoReuniao", "SK do órgão não pode ser vazio.");
                result.MENSAGEM = "SK do órgão não pode ser vazio.";
                return result;
            }
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "AtualizaInformacaoReuniao", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK da sessão não pode ser vazio.";
                return result;
            }
            if (string.IsNullOrEmpty(session_name))
            {
                Log("400", "AtualizaInformacaoReuniao", "Nome da sessão não pode ser vazio.");
                result.MENSAGEM = "Nome da sessão não pode ser vazio.";
                return result;
            }
            if (string.IsNullOrEmpty(modo))
            {
                Log("400", "AtualizaInformacaoReuniao", "Modo da sessão não pode ser vazio.");
                result.MENSAGEM = "Modo da sessão não pode ser vazio.";
                return result;
            }

            if (modo.Equals("R"))
            {
                modo = "ASK_TO_SPEAK";
            }
            else
            {
                modo = "MANUAL";
            }

            RestRequest request = new RestRequest($"/promic/session/update/{orgao}/{id_promic}", Method.Patch);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody($"{{\"session_name\":\" {session_name} \",\"initial_date\":\" {initial_date} \", \"mode\":\"{modo}\"}}", DataFormat.Json);


            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "AtualizaInformacaoReuniao", $"Sessão {id_promic} do órgão {orgao} alterada para {session_name} - {initial_date}.");
                    result.SUCESSO = true;
                    result.OBJETO = true;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "AtualizaInformacaoReuniao", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "AtualizaInformacaoReuniao: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "AtualizaInformacaoReuniao", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "AtualizaInformacaoReuniao: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Obtém os detalhes de uma sessão a partir do seu SK.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="orgao">SK do orgão.</param>
        /// <returns>Retorna um objeto do tipo ` Result` com um objeto do tipo `Reuniao` obtendo as informações da sessão na propriedade ` Result.Item`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada caso o SK da sessão ou do órgão está incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada caso ocorra um erro inesperado.</exception>
        public async Task<HReturn<Reuniao>> ObterReuniao(Token token, string orgao, string id_promic)
        {
            HReturn<Reuniao> result = new HReturn<Reuniao>();

            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "GetSessionDetails", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK do órgão não pode ser vazio.";
                return result;
            }

            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "GetSessionDetails", "SK do órgão não pode ser vazio.");
                result.MENSAGEM = "SK do órgão não pode ser vazio.";
                return result;
            }

            RestRequest request = new RestRequest($"/promic/session/{orgao}/{id_promic}", Method.Get);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");


            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    SessionDTO reuniao = JsonSerializer.Deserialize<SessionDTO>(response.Content, serializerOptions);
                    Reuniao obj = map.ToReuniao(reuniao);
                    Log($"{(int)response.StatusCode}", "GetSessionDetails", $"Detalhes da sessão {id_promic} obtidos com sucesso!");
                    result.SUCESSO = true;
                    result.OBJETO = obj;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "GetSessionDetails", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "GetSessionDetails: " + $"{response.ErrorException.Message} | {response.Content}";
                }


            }
            catch (Exception e)
            {
                Log("502", "GetSessionDetails", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "GetSessionDetails: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Remove um participante de sua sessão a partir do seu User_email e o SK da sessão.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="email">User_email do participante.</param>        
        /// <returns>Retorna um objeto do tipo ` Result`, podendo validar o resultado pela propriedade ` Result.Success`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada caso o SK do client ou o User_email do usuário esteja incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada caso ocorra um erro inesperado.</exception>
        public async Task<HReturn<Reuniao>> RemoverParticipanteReuniao(Token token, string id_promic, string email)
        {
            HReturn<Reuniao> result = new HReturn<Reuniao>();

            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "RemoveUserFromSession", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK da sessão não pode ser vazio.";
                return result;
            }
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "RemoveUserFromSession", "Email vazio ou inválido.");
                result.MENSAGEM = "Email vazio ou inválido.";
                return result;
            }


            RestRequest request = new RestRequest("/promic/session/removeUser/" + id_promic, Method.Patch);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody("{\"user_email\":\"" + email + "\"}", DataFormat.Json);

            Reuniao obj = new Reuniao();

            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "RemoveUserFromSession", $"Participante {email} removido com sucesso da sessão {id_promic})");
                    result.SUCESSO = true;
                    result.OBJETO = obj;
                }

                else
                {
                    Log($"{(int)response.StatusCode}", "RemoveUserFromSession", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "RemoveUserFromSession: " + $"{response.ErrorException.Message} | {response.Content}";
                }


            }
            catch (Exception e)
            {
                Log("502", "RemoveUserFromSession", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "RemoveUserFromSession: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Busca a lista de participantes de uma sessão a partir de seu SK.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <returns>Retorna um objeto do tipo ` Result` com uma lista de objetos do tipo `Participant` contendo todos os participantes na propriedade ` Result.Items`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada caso o SK da sessão esteja incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada caso ocorra um erro inesperado.</exception>
        public async Task<HReturn<List<Participante>>> ObterListaParticipanteReuniao(Token token, string id_promic)
        {
            HReturn<List<Participante>> result = new HReturn<List<Participante>>();

            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "ObterListaParticipanteReuniao", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK da sessão não pode ser vazio.";
                return result;
            }

            RestRequest request = new RestRequest("/promic/session/participants/" + id_promic, Method.Get);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            List<Participante> obj = new List<Participante>();

            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Result<List<string>> listaParticipantes = JsonSerializer.Deserialize<Result<List<string>>>(response.Content, serializerOptions);
                    foreach (string item in listaParticipantes.Session_users)
                    {
                        obj.Add(map.ToParticipante(item));
                    }

                    Log($"{(int)response.StatusCode}", "ObterListaParticipanteReuniao", $"List de participantes da sessão {id_promic} obtida com sucesso!");
                    result.SUCESSO = true;
                    result.OBJETO = obj;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "ObterListaParticipanteReuniao", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "ObterListaParticipanteReuniao: " + $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "ObterListaParticipanteReuniao", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "ObterListaParticipanteReuniao: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Deleta uma sessão a partir de seu SK e do SK do órgão.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="orgao">SK do orgão.</param>       
        /// <returns>Retorna um objeto do tipo ` Result`, podendo validar o resultado pela propriedade ` Result.Success`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK do órgão ou da sessão está vazio.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<bool>> DeletaReuniao(Token token, string orgao, string id_promic)
        {
            HReturn<bool> result = new HReturn<bool>();

            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "DeleteSession", "SK do órgão não pode ser vazio.");
                result.MENSAGEM = "SK da órgão não pode ser vazio.";
                return result;
            }
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "DeleteSession", "SK da sessão não pode ser vazio.");
                throw new ArgumentException("SK da sessão não pode ser vazio.");
            }

            RestRequest request = new RestRequest($"/promic/session/delete/{orgao}/{id_promic}", Method.Delete);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "DeleteSession", $"Sessão {id_promic} do órgão {orgao} deletada com sucesso!");
                    result.SUCESSO = true;
                    result.OBJETO = true;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "DeleteSession", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "DeleteSession: " + $"{response.ErrorException.Message} | {response.Content}";
                }


            }
            catch (Exception e)
            {
                Log("502", "DeleteSession", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "GetParticipantsList: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id_promicnet"></param>
        /// <returns></returns>
        public async Task<HReturn<List<Participante>>> ObterListaParticipantesAtivos(Token token, string id_promicnet)
        {
            HReturn<List<Participante>> result = new HReturn<List<Participante>>();

            if (string.IsNullOrEmpty(id_promicnet))
            {
                Log("400", "ObterListaParticipantesAtivos", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK da sessão não pode ser vazio.";
                return result;
            }

            RestRequest request = new RestRequest("/promic/session/current_users/" + id_promicnet, Method.Get);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            List<Participante> obj = new List<Participante>();

            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Result<List<ParticipantDTO>> listaParticipante = JsonSerializer.Deserialize<Result<List<ParticipantDTO>>>(response.Content, serializerOptions);
                    foreach (ParticipantDTO item in listaParticipante.Connected_users)
                    {
                        obj.Add(map.ToParticipante(item));
                    }

                    result.SUCESSO = true;
                    result.OBJETO = obj;
                    result.MENSAGEM = $"List de participantes da sessão {id_promicnet} obtida com sucesso!";
                    Log($"{(int)response.StatusCode}", "ObterListaParticipantesAtivos", $"List de participantes da sessão {id_promicnet} obtida com sucesso!");
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "ObterListaParticipantesAtivos", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "ObterListaParticipantesAtivos: " + $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "ObterListaParticipantesAtivos", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "ObterListaParticipantesAtivos: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        #endregion
        #region AGORA
        /// <summary>Responsável por obter um Token válido para entrar em uma reunião do Agora.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <returns>Retorna um objeto do tipo ` Result` com um objeto do tipo `AgoraToken` contendo as informações do Token do Agora na propriedade ` Result.Item`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK da sessão está vazio.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<HReturn<Agora>> ObterTokenAgora(Token token, string id_promic)
        {
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "ObterTokenAgora", "SK da sessão não pode ser vázio.");
                throw new ArgumentException("SK da sessão não pode ser vázio.");
            }
            HReturn<Agora> result = new HReturn<Agora>();

            try
            {
                RestRequest request = new RestRequest("/agora/system_generate_token/" + id_promic, Method.Post);
                request.AddHeader("Authorization", $"Bearer {token.Access_token}");
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    AgoraTokenDTO agoraObj = JsonSerializer.Deserialize<AgoraTokenDTO>(response.Content, serializerOptions);
                    result.OBJETO = new Agora
                    {
                        APPID = agoraObj.Appid,
                        CANAL = agoraObj.Channel,
                        TOKEN = agoraObj.Token,
                    };
                    result.SUCESSO = true;
                    Log($"{(int)response.StatusCode}", "ObterTokenAgora", $"Token Agora da sessão {id_promic} obtido com sucesso.");
                }
                else Log($"{(int)response.StatusCode}", "ObterTokenAgora", $"{response.ErrorException.Message} | {response.Content}");
            }
            catch (Exception e)
            {
                Log("502", "ObterTokenAgora", e.Message);
                result.SUCESSO = false;
                result.OBJETO = null;
                result.MENSAGEM = e.Message;
            }
            return result;
        }
        /// <summary>Responsável por ingressar na reunião do Agora como usuário "PROMIC".</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="agora_id">ID do Agora.</param>
        /// <param name="connection_id">ID da conexão WebSocket.</param>
        /// <returns>Está função não tem um retorno. Será retornada via WebSocket no evento `ParticipanteIngressouEvent`.</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<HReturn<bool>> IngressarSessaoAgora(Token token, string id_promic, string agora_id, string connection_id)
        {
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "IngressarSessaoAgora", "SK da sessão não pode ser vázio.");
                throw new ArgumentException("SK da sessão não pode ser vázio.");
            }
            if (string.IsNullOrEmpty(agora_id))
            {
                Log("400", "IngressarSessaoAgora", "ID do Agora não pode ser vázio.");
                throw new ArgumentException("ID do Agora não pode ser vázio.");
            }
            if (string.IsNullOrEmpty(connection_id))
            {
                Log("400", "IngressarSessaoAgora", "ID da conexão WebSocket não pode ser vázio.");
                throw new ArgumentException("ID da conexão WebSocket não pode ser vázio.");
            }
            HReturn<bool> result = new HReturn<bool>();

            try
            {
                RestRequest request = new RestRequest("/agora/system_joined_session/" + id_promic, Method.Post);
                request.AddHeader("Authorization", $"Bearer {token.Access_token}");
                request.AddStringBody(@"{""agora_id"": """ + agora_id + @""", ""connection_id"": """ + connection_id + @"""}", DataFormat.Json);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "IngressarSessaoAgora", "Conectado com sucesso!");
                    result.MENSAGEM = "Conectado com sucesso!";
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "IngressarSessaoAgora", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = $"{response.ErrorException.Message} | {response.Content}";
                }

                result.SUCESSO = response.IsSuccessStatusCode;
                result.OBJETO = response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                Log("502", "IngressarSessaoAgora", e.Message);
                result.MENSAGEM = e.Message;
            }
            return result;
        }
        /// <summary>
        /// Responsável por obter a lista de todas as sessões que têm algum uso do Agora
        /// </summary>
        /// <param name="token">Token de autenticação.</param>
        /// <returns></returns>
        public async Task<HReturn<List<USOSESSOES>>> ObterSessoesComConsumo(Token token)
        {
            HReturn<List<USOSESSOES>> result = new HReturn<List<USOSESSOES>>();
            try
            {
                RestRequest request = new RestRequest("/agora/sessions_with_usage", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token.Access_token}");
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result.OBJETO = new List<USOSESSOES>();
                    Result<List<UsageSessionDTO>> sessoesComUso = JsonSerializer.Deserialize<Result<List<UsageSessionDTO>>>(response.Content, serializerOptions);
                    foreach (UsageSessionDTO t in sessoesComUso.Sessions)
                        result.OBJETO.Add(map.ToReuniaoComUso(t));
                    result.SUCESSO = true;
                    result.MENSAGEM = "Lista de sessões com uso do Agora obtida com sucesso!";
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "ObterSessoesComConsumo", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "ObterSessoesComConsumo", e.Message);
                result.MENSAGEM = e.Message;
            }
            return result;
        }
        /// <summary>
        /// Responsável por obter todos os registros de consumo de uma reunião.
        /// </summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promicnet">ID da Reunião no Promicnet.</param>
        /// <returns></returns>
        public async Task<HReturn<List<UsoAgora>>> ObterConsumoReuniao(Token token, string id_promicnet)
        {
            HReturn<List<UsoAgora>> result = new HReturn<List<UsoAgora>> { OBJETO = new List<UsoAgora>() };
            try
            {
                RestRequest request = new RestRequest("/agora/session_usage/" + id_promicnet, Method.Get);
                request.AddHeader("Authorization", $"Bearer {token.Access_token}");
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Result<List<AgoraUsageDTO>> teste = JsonSerializer.Deserialize<Result<List<AgoraUsageDTO>>>(response.Content, serializerOptions);
                    foreach (AgoraUsageDTO usage in teste.Agora_usage)
                    {
                        result.OBJETO.Add(map.ToUsoAgora(usage));
                    }
                    result.SUCESSO = true;
                    result.MENSAGEM = "Consumo obtido com sucesso!";
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "ObterConsumoReuniao", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = response.ErrorException + " | " + response.Content;
                    result.SUCESSO = false;
                }
            }
            catch (Exception e)
            {
                Log("502", "ObterConsumoReuniao", e.Message);
                result.MENSAGEM = e.Message;
                result.SUCESSO = false;
            }
            return result;
        }
        /// <summary>
        /// Apaga um registro de consumo da base do Promicnet
        /// </summary>
        /// <param name="token">Token de autenticanção</param>
        /// <param name="id_promicnet">ID da Reunião no Promicnet</param>
        /// <returns></returns>
        public async Task<HReturn<string>> DeletarConsumoReuniao(Token token, string id_promicnet)
        {
            HReturn<string> result = new HReturn<string>();
            try
            {
                RestRequest request = new RestRequest("/agora/session_usage/" + id_promicnet, Method.Delete);
                request.AddHeader("Authorization", $"Bearer {token.Access_token}");
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result.SUCESSO = true;
                    result.MENSAGEM = "Deletado com sucesso!";
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "DeletarConsumoReuniao", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = response.ErrorException.Message + " | " + response.Content;
                }
            }
            catch (Exception e)
            {
                Log("502", "DeletarConsumoReuniao", e.Message);
                result.MENSAGEM = e.Message;
            }
            return result;
        }
        #endregion
    }
}