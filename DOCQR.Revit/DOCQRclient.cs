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
        private Token _token;
        private string _tokenString;

        public bool IsDummy { get; set; }

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="serverName"></param>
        public DOCQRclient(string serverName)
        {
            client = new RestClient(serverName);
            IsDummy = false;
        }


        /// <summary>
        /// This method facilitates user sign in 
        /// </summary>
        /// <param name="username">User Name log in</param>
        /// <param name="password">password log in</param>
        public void SignIn(string username, string password)
        {
            if (IsDummy)
            {
                _token = new Token() { _id = username.GetHashCode().ToString() };
                return;
            }
            RestRequest request = new RestRequest("/user/signin", Method.POST);            // set a new request posting
            request.AddParameter("email", username);                                         // set request parameters
            request.AddParameter("password", password);

            IRestResponse<Token> responce = client.Execute<Token>(request);                 // save the token information sent back from the web server
            var content = responce.StatusCode;                                              // get the status code of the sign in

            if (content == System.Net.HttpStatusCode.OK)                                    // if status code is OK
            {
                _token = responce.Data;
                _tokenString = responce.Content;
                // save the return hashtag, in this case its the email address
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
        public List<Project> GetProjects()
        {
            if (IsDummy)
            {
                return new List<Project>(new Project[] { new Project() { id = "1", name = "Project1"}, new Project() { id = "2",  name = "Project2"} , new Project() { id = "3", name = "Project3"} });
            }
            RestRequest request = new RestRequest("/projects/" + _token._id, Method.GET);                         // set a new request getting information
            //request.AddHeader("user", _tokenString);
            request.Timeout = 1000 * 10; // set parameters for the request

            request.AddParameter(new Parameter() { ContentType = "application/json", Name = "user", Type = ParameterType.HttpHeader, Value = _tokenString });


            var tmp = client.Execute(request);

            IRestResponse<List<Project>> responce = client.Execute<List<Project>>(request);           // save the responce
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


        private string ModelID;

        public string GetModelID(string projectId)
        {
            if (IsDummy)
            {
                ModelID = "aaa";
                return ModelID;
            }

            RestRequest request = new RestRequest("/newModelID/" + projectId, Method.POST);
            request.AddParameter("user", _token);
           

            IRestResponse resp = client.Execute(request);

            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
            throw new Exception("Unable to get Model ID");
            if (resp.Content != null) this.ModelID = resp.Content.TrimStart('"').TrimEnd('"');
            return this.ModelID;
        }


        public void SendViewInfo(DOCQR.Revit.ViewNames names)
        {
            if (IsDummy)
            {
                return;
            }
            RestRequest request = new RestRequest("/views", Method.POST);
            request.AddParameter("user", _token);
            
            request.AddParameter("modelName", ModelID);

            request.AddParameter("viewnames", names);

            if (client.Execute(request).StatusCode != System.Net.HttpStatusCode.OK) throw new
            Exception("Unable to send View names");
        }

        /// <summary>
        /// send the model information to the web server
        /// </summary>
        /// <param name="filePath">Full File Path</param>
        /// <param name="projectName">Project Name</param>
        /// <param name="modelName">Model Name</param>
        /// <returns></returns>
        public string SendModelInfo(Project proj, string filePath)
        {
            
            if (IsDummy)
            {
                return Guid.NewGuid().ToString();
            }

            //FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);               // read the json file
            //using (BinaryReader binaryReader = new BinaryReader(fileStream))
            {

                byte[] jsonFile = File.ReadAllBytes(filePath); //binaryReader.ReadBytes((int)fileStream.Length);                                           // conver the file to byte array
                RestRequest request = new RestRequest("/views/"+proj.id + "/" + ModelID, Method.POST);        // GET URL FROM WEB SERVER

                System.Diagnostics.Debug.WriteLine("Uploading file to DOCQR: " + jsonFile.Length + " bytes");
                //request.AddParameter("user", _token);                                                                       // add parameters to send to web server
                //request.AddParameter("modelName", this.ModelID);
                ////request.AddParameter("projectName", projectName);
                //request.AddParameter("jsonFileName", filePath);

                request.AddParameter("Content-Type", "application/stream");                                                 // stream the json file to the body 
                request.AddParameter("Content-Length", jsonFile.LongLength);
                request.AddParameter("jsonFile", jsonFile, ParameterType.RequestBody);
                request.Timeout = 1000 * 60 * 60;                                                                           // set up a time out 

                IRestResponse responce = client.Execute(request);                                                           // execute the request

                if (responce.StatusCode == System.Net.HttpStatusCode.OK)                                                    // if the responce was OK
                {
                    return responce.Content.TrimStart('"').TrimEnd('"');                                                                                // return the data (list of project names)
                }
                else
                {
                    throw new Exception("Unable to send model " + responce.StatusCode, new Exception(responce.Content));
                }
            }
        }
    }       // close class


    /// <summary>
    /// Token class used to store information recieved from the web server
    /// </summary>
    public class Token
    {
        public string _id { get; set; }
        public TokenLocal local { get; set; }
    }       // close class

    public class TokenLocal
    {
        public string password { get; set; }
        public string email { get; set; }
    }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }

        public override string ToString()
        {
            return name;
        }
    }
}
