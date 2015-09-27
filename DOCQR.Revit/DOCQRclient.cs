using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using RestSharp;

namespace DOCQR.Revit
{


    /// <summary>
    /// this class creates a client to set up a communiation between the web server and the desktop applicatoin
    /// </summary>
    public class DOCQRclient
    {

        private RestClient client;
        private string UserEmail;

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="serverName"></param>
        public DOCQRclient(string serverName)
        {
            client = new RestClient(serverName);
        }


        /// <summary>
        /// This method facilitates user sign in 
        /// </summary>
        /// <param name="username">User Name log in</param>
        /// <param name="password">password log in</param>
        public void SignIn(string username, string password)
        {
            RestRequest request = new RestRequest("/users/signin", Method.POST);            // set a new request posting
            request.AddParameter("name", username);                                         // set request parameters
            request.AddParameter("password", password);

            IRestResponse<Token> responce = client.Execute<Token>(request);                 // save the token information sent back from the web server
            var content = responce.StatusCode;                                              // get the status code of the sign in

            if (content == System.Net.HttpStatusCode.OK)                                    // if status code is OK
            {
                UserEmail = responce.Data.email;                                            // save the return hashtag, in this case its the email address
            }
            else
            {
                throw new Exception("Unable to login " + content);
            }
        }


        /// <summary>
        /// This method sends a request to the web server asking for the list of projects
        /// </summary>
        /// <returns></returns>
        public List<string> GetProjects()
        {
            RestRequest request = new RestRequest("/projects", Method.GET);                         // set a new request getting information
            request.AddHeader("email", UserEmail);                                                  // set parameters for the request


            IRestResponse<List<string>> responce = client.Execute<List<string>>(request);           // save the responce
            var content = responce.StatusCode;

            if (content == System.Net.HttpStatusCode.OK)                                            // if the responce was OK
            {
                return responce.Data;                                                               // return the data (list of project names)
            }
            else
            {
                throw new Exception("Unable to get projects " + content);
            }

        }


        /// <summary>
        /// send the model information to the web server
        /// </summary>
        /// <param name="filePath">Full File Path</param>
        /// <param name="projectName">Project Name</param>
        /// <param name="modelName">Model Name</param>
        /// <returns></returns>
        public string SendModelInfo(string filePath, string projectName, string modelName)
        {

            FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);               // read the json file
            using (BinaryReader binaryReader = new BinaryReader(fileStream))
            {

                byte[] jsonFile = binaryReader.ReadBytes((int)fileStream.Length);                                           // conver the file to byte array
                RestRequest request = new RestRequest("/projects", Method.POST);        // GET URL FROM WEB SERVER

                request.AddHeader("user", UserEmail);                                                                       // add parameters to send to web server
                request.AddParameter("modelName", modelName);
                request.AddParameter("projectName", projectName);
                request.AddParameter("jsonFileName", filePath);

                request.AddParameter("Content-Type", "application/stream");                                                 // stream the json file to the body 
                request.AddParameter("jsonFile", jsonFile, ParameterType.RequestBody);
                request.Timeout = 1000 * 60 * 60;                                                                           // set up a time out 

                IRestResponse responce = client.Execute(request);                                                           // execute the request

                if (responce.StatusCode == System.Net.HttpStatusCode.OK)                                                    // if the responce was OK
                {
                    return responce.Content;                                                                                // return the data (list of project names)
                }
                else
                {
                    throw new Exception("Unable to send model " + responce.Content);
                }
            }
        }
    }       // close class


    /// <summary>
    /// Token class used to store information recieved from the web server
    /// </summary>
    public class Token
    {
        public string email { get; set; }
        public string name { get; set; }
        public string expires { get; set; }
    }       // close class


}
