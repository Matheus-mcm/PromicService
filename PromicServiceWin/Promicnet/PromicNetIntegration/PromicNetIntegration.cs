﻿using Functions;
using PromicnetIntegration.Events;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

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
        public Api(string url) { client = new RestClient(url); utils = new Util(); map = new Map(); }
        #region LOG
        /// <summary>Evento de disparo de mensagens de execução do código.</summary>
        public event EventHandler<Types.LogData> LogEvent;
        /// <summary>Responsável por disparar o evento de mensagem.</summary>
        /// <param name="function">Nome da função que está disparando o evento.</param>
        /// <param name="message">Mensagem a ser disparada.</param>
        /// <param name="status">Mensagem a ser disparada.</param>
        public virtual void Log(string status, string function, string message) => LogEvent?.Invoke(this, new Types.LogData(@class: "Api", status: status, function: function, message: message));
        #endregion
        #region TOKEN
        /// <summary>Responsável por gerar um Token de autenticação para acessar a API do PromicNet.</summary>
        /// <param name="token">Objeto do tipo Token contendo os parâmetros necessários para gerar o token.</param>
        /// <exception cref="ArgumentNullException">Uma das propriedades do Token(Client_id, client_secret ou url) está vazia.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<Types.Token>> ObterTokenPromic(Types.Token token)
        {
            if (string.IsNullOrEmpty(token.client_id) || string.IsNullOrEmpty(token.client_secret))
            {
                Log("400", "GetToken", "Par de chaves não pode ser vazio!");
                throw new ArgumentNullException("Par de chaves não pode ser vazio!");
            }
            if (string.IsNullOrEmpty(token.url) || !Uri.IsWellFormedUriString(token.url, UriKind.Absolute))
            {
                Log("400", "GetToken", $"URL não é válida.");
                throw new ArgumentNullException("URL não é válida.");
            }
            if (!token.url.EndsWith("/oauth2/token")) { token.url += "/oauth2/token"; }

            RestRequest req = new RestRequest(token.url, Method.Post);

            req.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            req.AddParameter("grant_type", "client_credentials");
            req.AddParameter("scope", "scopes/system");
            req.AddParameter("client_id", token.client_id);
            req.AddParameter("client_secret", token.client_secret);
            Types.HReturn<Types.Token> result = new Types.HReturn<Types.Token>();
            try
            {
                RestResponse response = await client.ExecuteAsync(req);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Types.Token tokn = JsonSerializer.Deserialize<Types.Token>(response.Content, serializerOptions);

                    token.Expires_in = tokn.Expires_in;
                    token.Created_date = DateTime.Now;
                    token.Access_token = tokn.Access_token;
                    token.Token_type = tokn.Token_type;
                    token.IsValid(token);

                    result.OBJETO = token;
                    result.SUCESSO = true;

                    Log($"{(int)response.StatusCode}", "GetToken", "Token gerado com sucesso!");
                }
                else Log($"{(int)response.StatusCode}", "GetToken", $"{response.ErrorException.Message} | {response.Content}");
            }
            catch (Exception e)
            {
                Log("502", "GetToken", $"{e.TargetSite.Name} | {e.Message}");
                throw e;
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
        /// <returns>Retorna um objeto do tipo `Types.Result` com o objeto do tipo `Types.DELEGADO` contendo todas as informações dentro da propriedade `Types.Result.Item`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando uma, ou mais, das propriedades do usuário (User_email, órgão ou nome) está incorreta.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<Types.DELEGADO>> CadastrarDelegado(Types.Token token, string email, string orgao, string full_name)
        {
            Types.HReturn<Types.DELEGADO> result = new Types.HReturn<Types.DELEGADO>();

            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "CadastrarDelegado", "Email não pode ser inválido!");
                result.MENSAGEM = "Email não pode ser inválido!";
                return result;
            }
            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "CadastrarDelegado", "ID do cliente não pode ser vazio!");
                result.MENSAGEM = "ID do cliente não pode ser vazio!";
                return result;
            }
            if (string.IsNullOrEmpty(full_name))
            {
                Log("400", "CadastrarDelegado", "Nome do usuário não pode ser vazio!");
                result.MENSAGEM = "Nome do usuário não pode ser vazio!";
                return result;
            }

            RestRequest request = new RestRequest("/user", Method.Post);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            request.AddHeader("Content-Type", "text/plain");
            request.AddParameter("text/plain", $"{{\"email\":\"{email}\",\"client_id\":\"{orgao}\",\"full_name\":\"{full_name}\"}}", ParameterType.RequestBody);

            try
            {
                RestResponse response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "CadastrarDelegado", $"Delegado {email} criado com sucesso!");
                    result.SUCESSO = true;
                    result.MENSAGEM = $"Usuário {email} criado com sucesso!";
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "CadastrarDelegado", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "CadastrarDelegado: " + $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "RegisterUser", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "RegisterUser: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Responsável por buscar as informações de um usuário através do login do usuário (User_email).</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="email">User_email do usuário.</param>
        /// <returns>Retorna um objeto do tipo `Types.Result` com um objeto do tipo `Types.DELEGADO` contendo todas as informações dentro da propriedade `Types.Result.Item`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o User_email do usuário está inválido.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<Types.DELEGADO>> ObterDelegado(Types.Token token, string email)
        {
            Types.HReturn<Types.DELEGADO> result = new Types.HReturn<Types.DELEGADO>();

            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "GetUser", "Email do usuário está inválido!");
                result.MENSAGEM = "Email do usuário está inválido!";
                return result;
            }

            RestRequest request = new RestRequest($"/user/{email}", Method.Get);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {

                    TypesDTO.UserDTO userDTO = JsonSerializer.Deserialize<TypesDTO.UserDTO>(response.Content, serializerOptions);

                    Types.DELEGADO obj = map.ToDelegado(userDTO);

                    Log($"{(int)response.StatusCode}", "GetUser", $"Usário {email} obtido com sucesso!");

                    result.SUCESSO = true;
                    result.OBJETO = obj;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "GetUser", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "GetUser: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "GetUser", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "GetUser: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Responsável por trazer uma lista com todos os usuários de um órgão.</summary>
        /// <param name="token">Token de Autenticação.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <returns>Retorna uma lista com todos os usuários de um órgão.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK do órgão está vazio.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<List<Types.DELEGADO>>> ObterListaDelegado(Types.Token token, string orgao)
        {
            Types.HReturn<List<Types.DELEGADO>> result = new Types.HReturn<List<Types.DELEGADO>>();

            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "ObterListaDelegado", "ID do órgão não pode ser vazio!");
                result.MENSAGEM = "Email do usuário está inválido!";
                return result;
            }

            RestRequest request = new RestRequest($"/user/list/{orgao}", Method.Get);
            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            List<Types.DELEGADO> obj = new List<Types.DELEGADO>();
            try
            {
                RestResponse response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    List<TypesDTO.UserDTO> userList = JsonSerializer.Deserialize<List<TypesDTO.UserDTO>>(response.Content, serializerOptions);

                    foreach (TypesDTO.UserDTO user in userList)
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
        /// <returns>Retorna um objeto do tipo Types.Result com o objeto do tipo Types.DELEGADO.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando uma, ou mais, das propriedades do usuário (User_email, User_email novo, órgão ou nome) está incorreta.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<bool>> AtualizaDelegado(Types.Token token, string email, string new_email, string full_name, string orgao)
        {
            Types.HReturn<bool> result = new Types.HReturn<bool>();

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
            request.AddParameter("text/plain", "{\"email\":\"" + new_email + "\",\"client_id\":\"" + orgao + "\",\"full_name\":\"" + full_name + "\"}", ParameterType.RequestBody);

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
        /// <returns>Retorna um objeto do tipo Types.Types.Result com o objeto do tipo Types.DELEGADO.</returns>>
        /// <exception cref="ArgumentException">Exceção lançada quando o User_email do usuário está incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<bool>> DeletaDelegado(Types.Token token, string email)
        {
            Types.HReturn<bool> result = new Types.HReturn<bool>();

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
        public async Task<Types.HReturn<Types.ORGAO>> BuscaOrgao(Types.Token token, string orgao)
        {
            Types.HReturn<Types.ORGAO> result = new Types.HReturn<Types.ORGAO>();

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
                    TypesDTO.ClientDTO client = JsonSerializer.Deserialize<TypesDTO.ClientDTO>(response.Content, serializerOptions);
                    Types.ORGAO obj = map.ToOrgao(client);
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
        /// <returns>Retorna um objeto do tipo Types.Result com um list de objetos do tipo Orgao.</returns>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<List<Types.ORGAO>>> BuscaListaOrgao(Types.Token token)
        {
            Types.HReturn<List<Types.ORGAO>> result = new Types.HReturn<List<Types.ORGAO>>();

            RestRequest request = new RestRequest("/client", Method.Get);
            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            List<Types.ORGAO> obj = new List<Types.ORGAO>();

            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {

                    TypesDTO.Result<List<TypesDTO.ClientDTO>> clientList = JsonSerializer.Deserialize<TypesDTO.Result<List<TypesDTO.ClientDTO>>>(response.Content, serializerOptions);

                    foreach (TypesDTO.ClientDTO clientw in clientList.Clients)
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
        public async Task<Types.HReturn<Types.ORGAO>> CadastrarOrgao(Types.Token token, string nome_orgao, int max_participants)
        {
            Types.HReturn<Types.ORGAO> result = new Types.HReturn<Types.ORGAO>();

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
                    TypesDTO.ClientDTO cliente = JsonSerializer.Deserialize<TypesDTO.ClientDTO>(response.Content, serializerOptions);

                    Types.ORGAO obj = map.ToOrgao(cliente);

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
        public async Task<Types.HReturn<bool>> AtualizarOrgao(Types.Token token, string name, string orgao, int max_participants)
        {
            Types.HReturn<bool> result = new Types.HReturn<bool>();

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
        /// <returns>Retorna um objeto do tipo `Types.Result` com o resultado da requisição na propriedade `Types.Result.Success`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK do órgão está vazio.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<bool>> DeletaOrgao(Types.Token token, string orgao)
        {
            Types.HReturn<bool> result = new Types.HReturn<bool>();

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
        /// <returns>Retorna um objeto do tipo `Types.Result` com um objeto do tipo `Reuniao` contendo as informações da sessão na propriedade `Types.Result.Item`.</returns>
        /// <exception cref="ArgumentException">Excelçao lançada quando um dos parâmetros orgao ou sessionName estão incorretos.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<Types.REUNIAO>> CadastrarReuniao(Types.Token token, string orgao, string sessionName, DateTime sessionDate)
        {
            Types.HReturn<Types.REUNIAO> result = new Types.HReturn<Types.REUNIAO>();

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

            RestRequest request = new RestRequest("/promic/session/" + orgao, Method.Post);
            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody($"{{\"session_name\":\" {sessionName}\",\"initial_date\":\"{sessionDate:s}\"}}", DataFormat.Json);


            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    TypesDTO.SessionDTO sessionDTO = JsonSerializer.Deserialize<TypesDTO.SessionDTO>(response.Content, serializerOptions);
                    Types.REUNIAO obj = map.ToReuniao(sessionDTO);
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
        /// <returns>Retorna um objeto do tipo `Types.Result` com uma lista do tipo `Reuniao` contendo todas as sessões na propriedade `Types.Result.Items`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK do órgão está incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<List<Types.REUNIAO>>> ObterListaReuniao(Types.Token token, string orgao)
        {
            Types.HReturn<List<Types.REUNIAO>> result = new Types.HReturn<List<Types.REUNIAO>>();

            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "GetSessionsList", "SK do órgão não pode ser vazio!");
                result.MENSAGEM = "SK do órgão não pode ser vazio!";
                return result;
            }

            RestRequest request = new RestRequest($"/promic/session/list/{orgao}", Method.Get);
            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            List<Types.REUNIAO> obj = new List<Types.REUNIAO>();
            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    TypesDTO.Result<List<TypesDTO.SessionDTO>> sessionList = JsonSerializer.Deserialize<TypesDTO.Result<List<TypesDTO.SessionDTO>>>(response.Content, serializerOptions);
                    foreach (TypesDTO.SessionDTO session in sessionList.Sessions)
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
        /// <returns>Retorna um objeto do tipo `Types.Result`, podendo validar o resultado pela propriedade `Types.Result.Success`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK da sessão ou o User_email do participante está incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<bool>> AdicionarParticipanteReuniao(Types.Token token, string id_promic, string email)
        {
            Types.HReturn<bool> result = new Types.HReturn<bool>();

            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "AddUserInSession", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK da sessão não pode ser vazio.";
                return result;
            }
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "AddUserInSession", "Email vazio ou inválido.");
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
                    Log($"{(int)response.StatusCode}", "AddUserInSession", $"Usuário {email} adicionado na sessão {id_promic} com sucesso.");
                    result.SUCESSO = true;
                    result.OBJETO = true;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "AddUserInSession", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "AddUserInSession: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "AddUserInSession", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "AddUserInSession: " + $"{e.TargetSite.Name} | {e.Message}";
            }

            return result;
        }
        /// <summary>Atualiza o status de uma sessão a partir do seu SK.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <param name="status">Status a ser alterado.</param>
        /// <returns>Retorna um objeto do tipo `Types.Result`, podendo validar o resultado pela propriedade `Types.Result.Success`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada caso o SK da sessão ou o status esteja vazio.</exception>
        /// <exception cref="Exception">Exceção genérica lançada caso ocorra um erro inesperado.</exception>
        public async Task<Types.HReturn<bool>> AtualizaSituacaoReuniao(Types.Token token, string orgao, string id_promic, string status)
        {
            Types.HReturn<bool> result = new Types.HReturn<bool>();

            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "UpdateSessionStatus", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK da sessão não pode ser vazio.";
                return result;
            }
            if (string.IsNullOrEmpty(status))
            {
                Log("400", "UpdateSessionStatus", "Status não pode ser vazio.");
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
                    Log($"{(int)response.StatusCode}", "UpdateSessionStatus", $"Sessão {id_promic} alterada para o status {status}");
                    result.SUCESSO = true;
                    result.OBJETO = true;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "UpdateSessionStatus", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "AddUserInSession: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "UpdateSessionStatus", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "UpdateSessionStatus: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Atualiza o nome e/ou a data inicial de uma sessão a partir do SK da sessão e do SK do órgão.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="orgao">SK do órgão.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="session_name">Nome da sessão.</param>
        /// <param name="initial_date">Data de início da sessão.</param>
        /// <returns>Retorna um objeto do tipo `Types.Result`, podendo validar o resultado pela propriedade `Types.Result.Success`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada caso o SK do órgão, SK da sessão ou o nome da sessão esteja incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada caso ocorra um erro inesperado.</exception>
        public async Task<Types.HReturn<bool>> AtualizaInfoReuniao(Types.Token token, string orgao, string id_promic, string session_name, DateTime initial_date)
        {
            Types.HReturn<bool> result = new Types.HReturn<bool>();

            if (string.IsNullOrEmpty(orgao))
            {
                Log("400", "UpdateSession", "SK do órgão não pode ser vazio.");
                result.MENSAGEM = "SK do órgão não pode ser vazio.";
                return result;
            }
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "UpdateSession", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK da sessão não pode ser vazio.";
                return result;
            }
            if (string.IsNullOrEmpty(session_name))
            {
                Log("400", "UpdateSession", "Nome da sessão não pode ser vazio.");
                result.MENSAGEM = "Nome da sessão não pode ser vazio.";
                return result;
            }

            RestRequest request = new RestRequest($"/promic/session/update/{orgao}/{id_promic}", Method.Patch);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");
            request.AddHeader("Content-Type", "application/json");
            request.AddStringBody("{\"session_name\":\"" + session_name + "\",\"initial_date\":\"" + initial_date + "\"}", DataFormat.Json);


            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Log($"{(int)response.StatusCode}", "UpdateSession", $"Sessão {id_promic} do órgão {orgao} alterada para {session_name} - {initial_date}.");
                    result.SUCESSO = true;
                    result.OBJETO = true;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "UpdateSession", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "UpdateSession: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "UpdateSession", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "UpdateSessionStatus: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Obtém os detalhes de uma sessão a partir do seu SK.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="orgao">SK do orgão.</param>
        /// <returns>Retorna um objeto do tipo `Types.Result` com um objeto do tipo `Reuniao` obtendo as informações da sessão na propriedade `Types.Result.Item`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada caso o SK da sessão ou do órgão está incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada caso ocorra um erro inesperado.</exception>
        public async Task<Types.HReturn<Types.REUNIAO>> ObterReuniao(Types.Token token, string orgao, string id_promic)
        {
            Types.HReturn<Types.REUNIAO> result = new Types.HReturn<Types.REUNIAO>();

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
                    TypesDTO.SessionDTO teste = JsonSerializer.Deserialize<TypesDTO.SessionDTO>(response.Content, serializerOptions);
                    Types.REUNIAO obj = map.ToReuniao(teste);
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
        /// <returns>Retorna um objeto do tipo `Types.Result`, podendo validar o resultado pela propriedade `Types.Result.Success`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada caso o SK do client ou o User_email do usuário esteja incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada caso ocorra um erro inesperado.</exception>
        public async Task<Types.HReturn<Types.REUNIAO>> RemoverParticipanteReuniao(Types.Token token, string id_promic, string email)
        {
            Types.HReturn<Types.REUNIAO> result = new Types.HReturn<Types.REUNIAO>();

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

            Types.REUNIAO obj = new Types.REUNIAO();

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
        /// <returns>Retorna um objeto do tipo `Types.Result` com uma lista de objetos do tipo `Participant` contendo todos os participantes na propriedade `Types.Result.Items`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada caso o SK da sessão esteja incorreto.</exception>
        /// <exception cref="Exception">Exceção genérica lançada caso ocorra um erro inesperado.</exception>
        public async Task<Types.HReturn<List<Types.PARTICIPANTE>>> ObterListaParticipanteReuniao(Types.Token token, string id_promic)
        {
            Types.HReturn<List<Types.PARTICIPANTE>> result = new Types.HReturn<List<Types.PARTICIPANTE>>();

            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "GetParticipantsList", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK da sessão não pode ser vazio.";
                return result;
            }


            RestRequest request = new RestRequest("/promic/session/participants/" + id_promic, Method.Get);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            List<Types.PARTICIPANTE> obj = new List<Types.PARTICIPANTE>();

            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    TypesDTO.Result<List<string>> teste = JsonSerializer.Deserialize<TypesDTO.Result<List<string>>>(response.Content, serializerOptions);
                    foreach (string item in teste.Session_users)
                    {
                        obj.Add(map.ToParticipante(item));
                    }

                    Log($"{(int)response.StatusCode}", "GetParticipantsList", $"List de participantes da sessão {id_promic} obtida com sucesso!");
                    result.SUCESSO = true;
                    result.OBJETO = obj;
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "GetParticipantsList", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "GetParticipantsList: " + $"{response.ErrorException.Message} | {response.Content}";
                }

            }
            catch (Exception e)
            {
                Log("502", "GetParticipantsList", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "GetParticipantsList: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        /// <summary>Deleta uma sessão a partir de seu SK e do SK do órgão.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="orgao">SK do orgão.</param>       
        /// <returns>Retorna um objeto do tipo `Types.Result`, podendo validar o resultado pela propriedade `Types.Result.Success`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK do órgão ou da sessão está vazio.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<bool>> DeletaReuniao(Types.Token token, string orgao, string id_promic)
        {
            Types.HReturn<bool> result = new Types.HReturn<bool>();

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
        public async Task<Types.HReturn<List<Types.PARTICIPANTE>>> ObterListaParticipantesAtivos(Types.Token token, string id_promicnet)
        {
            Types.HReturn<List<Types.PARTICIPANTE>> result = new Types.HReturn<List<Types.PARTICIPANTE>>();

            if (string.IsNullOrEmpty(id_promicnet))
            {
                Log("400", "GetParticipantsList", "SK da sessão não pode ser vazio.");
                result.MENSAGEM = "SK da sessão não pode ser vazio.";
                return result;
            }

            RestRequest request = new RestRequest("/promic/session/current_users/" + id_promicnet, Method.Get);

            request.AddHeader("Authorization", $"Bearer {token.Access_token}");

            List<Types.PARTICIPANTE> obj = new List<Types.PARTICIPANTE>();

            try
            {
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    TypesDTO.Result<List<TypesDTO.ParticipantDTO>> teste = JsonSerializer.Deserialize<TypesDTO.Result<List<TypesDTO.ParticipantDTO>>>(response.Content, serializerOptions);
                    foreach (TypesDTO.ParticipantDTO item in teste.Connected_users)
                    {
                        obj.Add(map.ToParticipante(item));
                    }

                    result.SUCESSO = true;
                    result.OBJETO = obj;
                    result.MENSAGEM = $"List de participantes da sessão {id_promicnet} obtida com sucesso!";
                    Log($"{(int)response.StatusCode}", "GetParticipantsList", $"List de participantes da sessão {id_promicnet} obtida com sucesso!");
                }
                else
                {
                    Log($"{(int)response.StatusCode}", "GetParticipantsList", $"{response.ErrorException.Message} | {response.Content}");
                    result.MENSAGEM = "GetParticipantsList: " + $"{response.ErrorException.Message} | {response.Content}";
                }
            }
            catch (Exception e)
            {
                Log("502", "GetParticipantsList", $"{e.TargetSite.Name} | {e.Message}");
                result.MENSAGEM = "GetParticipantsList: " + $"{e.TargetSite.Name} | {e.Message}";
            }
            return result;
        }
        #endregion
        #region AGORA
        /// <summary>Responsável por obter um Token válido para entrar em uma reunião do Agora.</summary>
        /// <param name="token">Token de autenticação.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <returns>Retorna um objeto do tipo `Types.Result` com um objeto do tipo `AgoraToken` contendo as informações do Token do Agora na propriedade `Types.Result.Item`.</returns>
        /// <exception cref="ArgumentException">Exceção lançada quando o SK da sessão está vazio.</exception>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public async Task<Types.HReturn<Types.AGORA>> ObterTokenAgora(Types.Token token, string id_promic)
        {
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "GetAgoraToken", "SK da sessão não pode ser vázio.");
                throw new ArgumentException("SK da sessão não pode ser vázio.");
            }
            Types.HReturn<Types.AGORA> result = new Types.HReturn<Types.AGORA>();

            try
            {
                RestRequest request = new RestRequest("/agora/system_generate_token/" + id_promic, Method.Post);
                request.AddHeader("Authorization", $"Bearer {token.Access_token}");
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    TypesDTO.AgoraTokenDTO teste = JsonSerializer.Deserialize<TypesDTO.AgoraTokenDTO>(response.Content, serializerOptions);
                    result.OBJETO = new Types.AGORA
                    {
                        APPID = teste.Appid,
                        CANAL = teste.Channel,
                        TOKEN = teste.Token,
                    };
                    result.SUCESSO = true;
                    Log($"{(int)response.StatusCode}", "GetAgoraToken", $"Token Agora da sessão {id_promic} obtido com sucesso.");
                }
                else Log($"{(int)response.StatusCode}", "GetAgoraToken", $"{response.ErrorException.Message} | {response.Content}");
            }
            catch (Exception e)
            {
                Log("502", "GetAgoraToken", e.Message);
                throw e;
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
        public async void IngressarSessaoAgora(Types.Token token, string id_promic, string agora_id, string connection_id)
        {
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "JoinAgoraSession", "SK da sessão não pode ser vázio.");
                throw new ArgumentException("SK da sessão não pode ser vázio.");
            }
            if (string.IsNullOrEmpty(agora_id))
            {
                Log("400", "JoinAgoraSession", "ID do Agora não pode ser vázio.");
                throw new ArgumentException("ID do Agora não pode ser vázio.");
            }
            if (string.IsNullOrEmpty(connection_id))
            {
                Log("400", "JoinAgoraSession", "ID da conexão WebSocket não pode ser vázio.");
                throw new ArgumentException("ID da conexão WebSocket não pode ser vázio.");
            }

            try
            {
                RestRequest request = new RestRequest("/agora/system_joined_session/" + id_promic, Method.Post);
                request.AddHeader("Authorization", $"Bearer {token.Access_token}");
                request.AddStringBody(@"{""agora_id"": """ + agora_id + @""", ""connection_id"": """ + connection_id + @"""}", DataFormat.Json);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK) Log($"{(int)response.StatusCode}", "JoinAgoraSession", "Conectado com sucesso!");
                else Log($"{(int)response.StatusCode}", "JoinAgoraSession", $"{response.ErrorException.Message} | {response.Content}");
            }
            catch (Exception e)
            {
                Log("502", "JoinAgoraSession", e.Message);
                throw e;
            }
        }
        //public async Task<Types.HReturn<Types>> GetSessionsWithUsage(Types.Token token)
        //{
        //    try
        //    {
        //        RestRequest request = new RestRequest("/agora/sessions_with_usage", Method.Post);
        //        request.AddHeader("Authorization", $"Bearer {token.Access_token}");
        //        RestResponse response = await client.ExecuteAsync(request);

        //        if (response.StatusCode == HttpStatusCode.OK)
        //        {
        //            TypesDTO.AgoraTokenDTO teste = JsonSerializer.Deserialize<TypesDTO.AgoraTokenDTO>(response.Content, serializerOptions);
        //        }
        //        else Log($"{(int)response.StatusCode}", "GetAgoraToken", $"{response.ErrorException.Message} | {response.Content}");
        //    }
        //    catch (Exception e)
        //    {
        //        Log("502", "GetAgoraToken", e.Message);
        //        throw e;
        //    }
        //    return new Types.HReturn<Types> { };
        //}
        #endregion
    }
    /// <summary>
    /// Classe com os métodos de comunicação via WebSocket com o Promicnet
    /// </summary>
    public class WebSockert
    {
        private readonly WebSocketSharp.WebSocket webSocket;
        private readonly Util utils;
        private readonly string session_sk;
        private readonly string orgao;
        /// <summary>Cosntrutor da classe WebSocket.</summary>
        /// <param name="wsUrl">URL do WebSocket</param>
        /// <param name="token">Token de autenticação (Token.Access_token).</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <param name="orgao">SK do órgão.</param>
        public WebSockert(string wsUrl, string token, string id_promic, string orgao)
        {
            this.session_sk = id_promic;
            this.orgao = orgao;

            webSocket = new WebSocketSharp.WebSocket($"{wsUrl}?token={token}&session_id={id_promic}&client_id={orgao}");
            utils = new Util();
        }
        /// <summary>Evento de mensagens de log.</summary>
        public event EventHandler<Types.LogData> LogEvent;
        /// <summary>Evento disparado quando o Promic ingressar na sessão.</summary>
        public event EventHandler<ConectadoEvent> ConectadoEventHandler;
        /// <summary>Evento disparado quando o Promic ingressar na sessão.</summary>
        public event EventHandler<ParticipanteDesconectadoEvent> ParticipanteDesconectadoEventHandler;
        /// <summary>Evento disparado ao receber um ManterLogado.</summary>
        public event EventHandler<KeepAliveEvent> KeepAliveEventHandler;
        /// <summary>Evento disparado quando um vídeo for ativado.</summary>
        public event EventHandler<VideoAtivadoEvent> VideoAtivadoEventHandler;
        /// <summary>Evento disparado quando um áudio for ativado.</summary>
        public event EventHandler<MicrofoneAtivadoEvent> MicrofoneAtivadoEventHandler;
        /// <summary>Evento disparado quando um áudio for desativado.</summary>
        public event EventHandler<MicrofoneDesativadoEvent> MicrofoneDesativadoEventHandler;
        /// <summary>Evento disparado quando um participante solicitar a palavra.</summary>
        public event EventHandler<PedidoPalavraEvent> PedidoPalavraEventHandler;
        /// <summary>Evento disparado quando uma palavra solicitada for negada.</summary>
        public event EventHandler<PermissaoFalarRecusadaEvent> PermissaoFalarRecusadaEventHandler;
        /// <summary>Evento disparado quando o situacao de um microfone for alterado.</summary>
        public event EventHandler<SitaucaoMicrofoneAlteradaEvent> SitaucaoMicrofoneAlteradaEventHandler;
        /// <summary>Evento disparado quando for detectado que um participante, que está com o microfone desativado, começar a falar.</summary>
        public event EventHandler<PeakDetectEvent> PeakDetectEventHandler;
        /// <summary>Evento disparado quando o status da reunião for alterado.</summary>
        public event EventHandler<SituacaoReuniaoAlteradaEvent> SituacaoReuniaoAlteradaEventHandler;
        /// <summary>Evento disparado quando um participante ingressar na sessão.</summary>
        public event EventHandler<ParticipanteIngressouEvent> ParticipanteIngressouEventHandler;
        /// <summary>Evento disparado quando uma mensagem não específicada for recebida.</summary>
        public event EventHandler<Event> EventHandler;
        /// <summary>Evento disparado quando uma mensagem de erro for disparada pelo WebSocket.</summary>
        public event EventHandler<WebSocketSharp.ErrorEventArgs> ErrorEventHandler;
        /// <summary>Evento disparado quando a situação de um microfone ou câmera for alterada</summary>
        public event EventHandler<SituacaoMidiaEvent> SituacaoMidiaEventHandler;
        #region LOG
        /// <summary>Responsável por disparar o evento de mensagem</summary>
        /// <param name="function">Nome da função que está disparando o evento</param>
        /// <param name="message">Mensagem a ser disparada</param>
        /// <param name="status">Mensagem a ser disparada</param>
        public virtual void Log(string status, string function, string message) => LogEvent?.Invoke(this, new Types.LogData(@class: "WebSocket", status: status, function: function, message: message));
        #endregion
        #region WEBSOCKET
        /// <summary>Estabelece uma conexão com o WebSocket do Promicnet.</summary>
        /// <returns>Retorna um objeto Types.Result contendo as informações da conexão com o servidor dentro do parametro `Types.Result.Item`.</returns>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public void Conectar()
        {
            Log("1", "Connect", $"Conectando à sessão {session_sk} do órgão {orgao}.");

            try
            {
                webSocket.Connect();
                webSocket.OnMessage += OnMessage;
                webSocket.OnClose += OnClose;
                webSocket.OnError += OnError;
                webSocket.OnOpen += OnOpen;
                Log("200", "Connect", "Conectando com sucesso!");
            }
            catch (Exception e)
            {
                Log("502", "Connect", e.Message);
                throw new Exception(e.Message);
            }
        }
        /// <summary>Encerra a conexão com o WebSocket do Promicnet.</summary>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public void Desconectar()
        {
            Log("1", "Disconnect", $"Desconectando...");
            try
            {
                webSocket.Close();
                Log("200", "Disconnect", $"Desconectado com sucesso!");
            }
            catch (Exception e)
            {
                Log("502", "Disconnect", $"{e.Message}");
                throw e;
            }
        }
        /// <summary>Reseta o tempo da conexão com o WebSocket do Promicnet. <br/>
        /// As conexões com o WebSocket duram 10 minutos, para que você não seja desconectado automaticamente, poderá chamar este método a qualquer momento para reiniciar a contagem deste tempo.</summary>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public void ManterLogado()
        {
            Log("1", "KeepAlive", "Keep alive.");
            try
            {
                webSocket.Send("{\"action\": \"keep_alive\"}\"");
            }
            catch (Exception e)
            {
                Log("502", "KeepAlive", $"{e.Message}");
                throw e;
            }
        }
        /// <summary>Ativa o microfone de um participante da sessão.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public void AtivarMicrofone(string email, string id_promic)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "TurnOnMic", "Email inválido!");
                throw new ArgumentException("Email inválido!");
            }
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "TurnOnMic", "SK da sessão inválido!");
                throw new ArgumentException("SK da sessão inválido!");
            }
            Log("1", "TurnOnMic", $"Ligando o microfone do {email} da sessão {id_promic}");
            try
            {
                webSocket.Send(@"{""action"": ""add_active_audio"", ""session_id"": """ + id_promic + @""", ""user_email"": """ + email + @"""}");
            }
            catch (Exception e)
            {
                Log("502", "TurnOnMic", e.Message);
                throw e;
            }
        }
        /// <summary>Ativa o vídeo de um participante da sessão.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public void ChavearVideo(string email, string id_promic)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "WSTurnOnVideo", "Email inválido!");
                throw new ArgumentException("Email inválido!");
            }
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "WSTurnOnVideo", "SK da sessão inválido!");
                throw new ArgumentException("SK da sessão inválido!");
            }
            Log("1", "WSTurnOnVideo", $"Ligando o vídeo do usuário {email}");
            try
            {
                webSocket.Send(@"{ ""action"":""select_active_video"", ""session_id"":""" + id_promic + @""", ""user_email"":""" + email + @""" }");
            }
            catch (Exception e)
            {
                Log("502", "WSTurnOnVideo", e.Message);
                throw e;
            }
        }
        /// <summary>Ativa o vídeo de um participante da sessão.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        ///
        public void DesativarCamera(string email, string id_promic)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "WSTurnOnVideo", "Email inválido!");
                throw new ArgumentException("Email inválido!");
            }
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "WSTurnOnVideo", "SK da sessão inválido!");
                throw new ArgumentException("SK da sessão inválido!");
            }
            Log("1", "WSTurnOnVideo", $"Ligando o vídeo do usuário {email}");
            try
            {
                webSocket.Send(@"{ ""action"":""select_active_video"", ""session_id"":""" + id_promic + @""", ""user_email"":""" + email + @""" }");
            }
            catch (Exception e)
            {
                Log("502", "WSTurnOnVideo", e.Message);
                throw e;
            }
        }
        /// <summary>
        /// Método para chaver no vídeo de um participante
        /// </summary>
        /// <param name="email"></param>
        /// <param name="id_promicnet"></param>
        public void AtivarCamera(string email, string id_promicnet)
        {

        }
        /// <summary>
        /// Método para ativar o preview do vídeo de um participante
        /// </summary>
        /// <param name="email"></param>
        /// <param name="id_promicnet"></param>
        public void AtivarPreview(string email, string id_promicnet)
        {

        }
        /// <summary>
        /// Método para desativar o preview do vídeo de um participante
        /// </summary>
        /// <param name="email"></param>
        /// <param name="id_promicnet"></param>
        public void DesativarPreview(string email, string id_promicnet)
        {

        }
        /// <summary>Desativa o microfone de um pariticpante da sessão.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public void DesativarMicrofone(string email, string id_promic)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "WSTurnOffMic", "Email inválido!");
                throw new ArgumentException("Email inválido!");
            }
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "WSTurnOffMic", "SK da sessão inválido!");
                throw new ArgumentException("SK da sessão inválido!");
            }
            Log("1", "WSTurnOffMic", $"Ligando o vídeo do usuário {email}");
            try
            {
                webSocket.Send(@"{""action"": ""remove_active_audio"",""session_id"": """ + id_promic + @""",""user_email"": """ + email + @"""}");
            }
            catch (Exception e)
            {
                Log("502", "WSTurnOffMic", e.Message);
                throw e;
            }
        }
        /// <summary>Solicita a palavra para um participante.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public void SolicitarPalavra(string email, string id_promic)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "WSRequestTalk", "Email inválido!");
                throw new ArgumentException("Email inválido!");
            }
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "WSRequestTalk", "SK da sessão inválido!");
                throw new ArgumentException("SK da sessão inválido!");
            }
            Log("1", "WSRequestTalk", $"Solicitando palavra do usuário {email}");

            try
            {
                webSocket.Send(@"{""action"": ""ask_to_speak"",""session_id"": """ + id_promic + @""",""user_email"": """ + email + @"""}");
            }
            catch (Exception e)
            {
                Log("502", "WSRequestTalk", e.Message);
                throw e;
            }
        }
        /// <summary>Recusa a solicitação de palavra de um participante.</summary>
        /// <param name="email">User_email do participante.</param>
        /// <param name="id_promic">SK da sessão.</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public void RecusarPedidoPalavra(string email, string id_promic)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "WSCancelRequestTalk", "Email inválido!");
                throw new ArgumentException("Email inválido!");
            }
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "WSCancelRequestTalk", "SK da sessão inválido!");
                throw new ArgumentException("SK da sessão inválido!");
            }
            Log("1", "WSCancelRequestTalk", $"Cancelando solicitação da palavra do usuário {email}");

            try
            {
                webSocket.Send(@"{""action"": ""system_cancel_ask_to_speak"",""session_id"": """ + id_promic + @""",""user_email"": """ + email + @"""}");
            }
            catch (Exception e)
            {
                Log("502", "WSCancelRequestTalk", e.Message);
                throw e;
            }
        }
        /// <summary>Altera o situacao do microfone de um participante.</summary>
        /// <param name="email">User_email do participante</param>
        /// <param name="id_promic">SK da sessão</param>
        /// <param name="state">Estado do microfone</param>
        /// <exception cref="Exception">Exceção genérica lançada quando ocorrer um erro desconhecido.</exception>
        public void AlterarEstadoMicrofone(string email, string id_promic, string state)
        {
            if (string.IsNullOrEmpty(email) || !utils.IsValidEmail(email))
            {
                Log("400", "WSChangeMicState", "Email inválido!");
                throw new ArgumentException("Email inválido!");
            }
            if (string.IsNullOrEmpty(id_promic))
            {
                Log("400", "WSChangeMicState", "SK da sessão inválido!");
                throw new ArgumentException("SK da sessão inválido!");
            }
            if (string.IsNullOrEmpty(state))
            {
                Log("400", "WSChangeMicState", "Estado do microfone inválido!");
                throw new ArgumentException("Estado do microfone inválido!");
            }
            Log("1", "WSChangeMicState", $"Alterando o estado do microfone do {email} da sessão {id_promic} para {state}");

            try
            {
                webSocket.Send(@"{""action"": ""change_mic_state"",""session_id"": """ + id_promic + @""",""user_email"": """ + email + @""",""state"":""" + state + @"""}");
            }
            catch (Exception e)
            {
                Log("502", "WSChangeMicState", e.Message);
                throw e;
            }
        }
        /// <summary>Callback de retorno ao se conectar no WebSocket.</summary>
        public void OnOpen(object sender, EventArgs e)
        {

        }
        /// <summary>Callback de retorno ao ocorrer um erro no WebScoket.</summary>
        private void OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            ErrorEventHandler?.Invoke(this, e);
        }
        /// <summary>Callback de retorno ao fechar uma conexão com o WebSocket.</summary>
        private void OnClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
        }
        /// <summary>Callback de retorno com a trativa de todas as mensagens do WebSocket.</summary>
        private void OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            Event evento = JsonSerializer.Deserialize<Event>(e.Data.ToString(), new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, PropertyNameCaseInsensitive = true });
            switch (evento.Action)
            {
                case "connected_successfully":
                    ConectadoEvent connectedSuccessfully = new ConectadoEvent(evento.Action, evento.Connection_id);
                    ConectadoEventHandler?.Invoke(this, connectedSuccessfully);
                    break;
                case "disconnect":
                    ParticipanteDesconectadoEvent disconnectedSuccessfully = new ParticipanteDesconectadoEvent(evento.Action, evento.Agora_id, evento.User_email);
                    ParticipanteDesconectadoEventHandler?.Invoke(this, disconnectedSuccessfully);
                    break;
                case "keep_alive":
                    KeepAliveEvent keepAlive = new KeepAliveEvent(evento.Action);
                    KeepAliveEventHandler?.Invoke(this, keepAlive);
                    break;
                case "select_active_video":
                    VideoAtivadoEvent selectActiveVideo = new VideoAtivadoEvent(evento.Action, evento.Agora_id, evento.User_email);
                    VideoAtivadoEventHandler?.Invoke(this, selectActiveVideo);
                    break;
                case "add_active_audio":
                    MicrofoneAtivadoEvent addActiveAudio = new MicrofoneAtivadoEvent(evento.Action, evento.Agora_id);
                    MicrofoneAtivadoEventHandler?.Invoke(this, addActiveAudio);
                    break;
                case "remove_active_audio":
                    MicrofoneDesativadoEvent removeActiveAudio = new MicrofoneDesativadoEvent(evento.Action, evento.Agora_id);
                    MicrofoneDesativadoEventHandler?.Invoke(this, removeActiveAudio);
                    break;
                case "ask_to_speak":
                    PedidoPalavraEvent askToSpeak = new PedidoPalavraEvent(evento.Action, evento.Session_id, evento.User_email);
                    PedidoPalavraEventHandler?.Invoke(this, askToSpeak);
                    break;
                case "user_joined_session":
                    if (evento.User_email != "PROMIC")
                    {
                        ParticipanteIngressouEvent userJoinedSessionEvent = new ParticipanteIngressouEvent(evento.Action, evento.Session_id, evento.User_email, evento.Agora_id);
                        ParticipanteIngressouEventHandler?.Invoke(this, userJoinedSessionEvent);
                    }
                    break;
                case "cancel_ask_to_speak":
                    PermissaoFalarRecusadaEvent cancelAskToSpeak = new PermissaoFalarRecusadaEvent(evento.Action, evento.Session_id, evento.User_email);
                    PermissaoFalarRecusadaEventHandler?.Invoke(this, cancelAskToSpeak);
                    break;
                case "change_mic_state":
                    SitaucaoMicrofoneAlteradaEvent changeMicState = new SitaucaoMicrofoneAlteradaEvent(evento.Action, evento.State);
                    SitaucaoMicrofoneAlteradaEventHandler?.Invoke(this, changeMicState);
                    break;
                case "session_status_changed":
                    SituacaoReuniaoAlteradaEvent sessionStatusChanged = new SituacaoReuniaoAlteradaEvent(evento.Action);
                    SituacaoReuniaoAlteradaEventHandler?.Invoke(this, sessionStatusChanged);
                    break;
                case "peak_detect":
                    PeakDetectEvent peakDetect = new PeakDetectEvent(evento.Action, evento.Session_id, evento.User_email, evento.State);
                    PeakDetectEventHandler?.Invoke(this, peakDetect);
                    break;
                case "media_changed_state":
                    SituacaoMidiaEvent situacaoMidia = new SituacaoMidiaEvent(evento.Action, evento.User_email, evento.Media_type, evento.State, evento.Session_id);
                    SituacaoMidiaEventHandler?.Invoke(this, situacaoMidia);
                    break;
                default:
                    EventHandler?.Invoke(this, evento);
                    break;
            }
        }
        #endregion
    }
}