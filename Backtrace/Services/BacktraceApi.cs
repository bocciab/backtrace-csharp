﻿using System;
using Backtrace.Model;
using System.Collections.Generic;
using System.Text;
using Backtrace.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using static Backtrace.FileUpload;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Services
{
    /// <summary>
    /// Create requests to Backtrace API
    /// </summary>
    internal class BacktraceApi<T> : IBacktraceApi<T>
    {
        public const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
        public const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;

        /// <summary>
        /// Get or set request timeout value in milliseconds
        /// </summary>
        public int Timeout { get; set; }

        private readonly string _serverurl;
        private readonly BacktraceCredentials _credentials;

        /// <summary>
        /// Create a new instance of Backtrace API request.
        /// </summary>
        /// <param name="credentials">API credentials</param>
        /// <param name="timeout">Request timeout in milliseconds</param>
        public BacktraceApi(BacktraceCredentials credentials, int timeout = 5000)
        {
            _credentials = credentials;
            //_serverurl = $"{_credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={_credentials.Token}";
            _serverurl = $"{_credentials.BacktraceHostUri.AbsoluteUri}post?format=minidump&token={_credentials.Token}";
            bool isHttps = _credentials.BacktraceHostUri.Scheme == "https";
            //prepare web client to send a data to ssl API
            if (isHttps)
            {
                ServicePointManager.SecurityProtocol = Tls12;
            }
        }

        /// <summary>
        /// Send a backtrace data to server API. 
        /// </summary>
        /// <param name="data">Collected backtrace data</param>
        public void Send(BacktraceData<T> data)
        {

            var json = JsonConvert.SerializeObject(data);
            //using (var client = new WebClient())
            //{
            //    var tempServerUrl = $"{_credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={_credentials.Token}";
            //    client.Headers[HttpRequestHeader.ContentType] = "application/json";
            //    var result = client.UploadString(address: tempServerUrl , method: "POST", data: json);
            //}

            //var json = JsonConvert.SerializeObject(data);
            string filePath = @"D:\data\attachment_data.ldf";
            FileParameter fileParameter = new FileParameter(System.IO.File.ReadAllBytes(filePath), System.IO.Path.GetFileName(filePath));
            //var collection = data.GetJsonData();
            var collection = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            collection["file"] = fileParameter;

            var webResponse = FileUpload.MultipartFormDataPost(_serverurl, collection);

            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            webResponse.Close();

            //var json = JsonConvert.SerializeObject(data);

            //using (var client = new WebClient())
            //{
            //client.Encoding = Encoding.UTF8;
            //client.Headers[HttpRequestHeader.ContentType] = "application/json";
            //var result = client.UploadValues(address: _serverurl, method: "POST", data: collection);
            //var result = client.UploadString(address: _serverurl, method: "POST", data: json);
            //client.
            //client.UploadFile()
            //}
        }

    }
}