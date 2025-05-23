﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DotNetBrightener.Core.RemoteServices;

public class DefaultRestClientService : IRestClientService
{
    private readonly RestClient _restClient;

    public DefaultRestClientService()
    {
        _restClient = new RestClient();
    }

    public async Task<TResponse> Request<TResponse>(HttpMethod          method, string requestUrl,
                                                    object              body    = null,
                                                    NameValueCollection headers = null)
    {
        var restClient = new RestClient();
        var restMethod = Enum.Parse<Method>(method.Method, true);

        var restRequest = new RestRequest(new Uri(requestUrl), restMethod);

        if (body != null)
        {
            if (restMethod == Method.Post || restMethod == Method.Put || restMethod == Method.Patch)
            {
                restRequest.AddJsonBody(body);
            }
            else
            {
                var serializedBodyAsDictionary =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(body));

                foreach (var keyValuePair in serializedBodyAsDictionary)
                {
                    if (keyValuePair.Value != null)
                    {
                        restRequest.AddQueryParameter(keyValuePair.Key, keyValuePair.Value.ToString()!);
                    }
                }
            }
        }

        if (headers != null && headers.Count > 0)
        {
            foreach (var headerName in headers.AllKeys)
            {
                restClient.AddDefaultHeader(headerName, headers.Get(headerName));
            }
        }

        var response = await restClient.ExecuteAsync<TResponse>(restRequest);

        if (!response.IsSuccessful)
        {

        }

        return response.Data;
    }

    public Task<TResponse> Get<TResponse>(string requestUrl, NameValueCollection headers = null)
    {
        return Request<TResponse>(HttpMethod.Get, requestUrl, null, headers);
    }

    public Task<TResponse> Post<TResponse>(string requestUrl, object body = null, NameValueCollection headers = null)
    {
        return Request<TResponse>(HttpMethod.Post, requestUrl, body, headers);
    }

    public Task<TResponse> Put<TResponse>(string requestUrl, object body = null, NameValueCollection headers = null)
    {
        return Request<TResponse>(HttpMethod.Put, requestUrl, body, headers);
    }

    public Task<TResponse> Delete<TResponse>(string requestUrl, NameValueCollection headers = null)
    {
        return Request<TResponse>(HttpMethod.Delete, requestUrl, null, headers);
    }
}