using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestSharp;

namespace DOCQR.Revit
{
    class DOCQRclient
    {

        private RestClient client;
        private string UserEmail;

        public DOCQRclient(string serverName)
        {
            client = new RestClient(serverName);
        }


        public void SignIn(string username, string password)
        {
            RestRequest request = new RestRequest("/users/signin", Method.POST);
            request.AddParameter("name", username);
            request.AddParameter("password", password);

            IRestResponse<Token> responce = client.Execute<Token>(request);
            var content = responce.StatusCode;

            if (content == System.Net.HttpStatusCode.OK)
            {
                UserEmail = responce.Data.email;
            }
            else
            {
                throw new Exception("Unable to login " + content);
            }
        }


        public List<string> GetProjects()
        {

            RestRequest request = new RestRequest("/projects", Method.GET);
            request.AddParameter("email", UserEmail);

            IRestResponse<List<string>> responce = client.Execute<List<string>>(request);
            var content = responce.StatusCode;

            if (content == System.Net.HttpStatusCode.OK)
            {
                return responce.Data;
            }
            else
            {
                throw new Exception("Unable to get projects " + content);
            }

        }

    }       // close class


    /// <summary>
    /// Token class
    /// </summary>
    public class Token
    {
        public string email { get; set; }
        public string name { get; set; }
        public string expires { get; set; }
    }       // close class


}
