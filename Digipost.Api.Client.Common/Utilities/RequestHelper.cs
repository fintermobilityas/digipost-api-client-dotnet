using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Digipost.Api.Client.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace Digipost.Api.Client.Common.Utilities;

internal class RequestHelper
{
    readonly ILogger<RequestHelper> _logger;

    internal RequestHelper(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        HttpClient = httpClient;
        _logger = loggerFactory.CreateLogger<RequestHelper>();
    }

    internal HttpClient HttpClient { get; set; }

    internal Task<T> PostAsync<T>(HttpContent httpContent, XmlDocument messageActionRequestContent, Uri uri, bool skipMetaDataValidation = false)
    {
        ValidateXml(messageActionRequestContent, skipMetaDataValidation);

        var postAsync = HttpClient.PostAsync(uri, httpContent);

        return SendAsync<T>(postAsync);
    }

    internal Task<T> GetAsync<T>(Uri uri)
    {
        return SendAsync<T>(HttpClient.GetAsync(uri));
    }

    internal Task<T> PutAsync<T>(HttpContent httpContent, XmlDocument messageActionRequestContent, Uri uri, bool skipMetaDataValidation = false)
    {
        ValidateXml(messageActionRequestContent, skipMetaDataValidation);

        var postAsync = HttpClient.PutAsync(uri, httpContent);

        return SendAsync<T>(postAsync);
    }

    internal Task<string> DeleteAsync(Uri uri)
    {
        return SendAsync<string>(HttpClient.DeleteAsync(uri));
    }

    internal async Task<Stream> GetStreamAsync(Uri uri)
    {
        var responseTask = HttpClient.GetAsync(uri);
        var httpResponseMessage = await responseTask.ConfigureAwait(false);

        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var responseContent = await ReadResponse(httpResponseMessage).ConfigureAwait(false);
            HandleResponseErrorAndThrow(responseContent, httpResponseMessage.StatusCode);
        }

        return await httpResponseMessage.Content.ReadAsStreamAsync();
    }

    async Task<T> SendAsync<T>(Task<HttpResponseMessage> responseTask)
    {
        var httpResponseMessage = await responseTask.ConfigureAwait(false);

        var responseContent = await ReadResponse(httpResponseMessage).ConfigureAwait(false);

        if (!httpResponseMessage.IsSuccessStatusCode)
            HandleResponseErrorAndThrow(responseContent, httpResponseMessage.StatusCode);

        return HandleSuccessResponse<T>(responseContent);
    }

    void HandleResponseErrorAndThrow(string responseContent, HttpStatusCode statusCode)
    {
        var emptyResponse = string.IsNullOrEmpty(responseContent);

        if (!emptyResponse)
            ThrowNotEmptyResponseError(responseContent);
        else
        {
            ThrowEmptyResponseError(statusCode);
        }
    }

    void ValidateXml(XmlDocument document, bool skipMetaDataValidation)
    {
        if (document.InnerXml.Length == 0)
        {
            return;
        }

        var xmlValidator = new ApiClientXmlValidator(skipMetaDataValidation);
        bool isValidXml;
        string validationMessages;

        if (skipMetaDataValidation || !xmlValidator.CheckIfDataTypesAssemblyIsIncluded())
        {
            isValidXml = xmlValidator.Validate(GetDocumentXmlWithoutMetaData(document), out validationMessages);
        }
        else
        {
            isValidXml = xmlValidator.Validate(document.InnerXml, out validationMessages);
        }

        if (!isValidXml)
        {
            _logger.LogError($"Xml was invalid. Stopped sending message. Feilmelding: '{validationMessages}'");
            throw new XmlException($"Xml was invalid. Stopped sending message. Feilmelding: '{validationMessages}'");
        }
    }

    static string GetDocumentXmlWithoutMetaData(XmlNode document)
    {
        return Regex.Replace(document.InnerXml, @"<data-type[^>]*>(.*?)</data-type>", "").Trim();
    }

    static async Task<string> ReadResponse(HttpResponseMessage requestResult)
    {
        var contentResult = await requestResult.Content.ReadAsStringAsync().ConfigureAwait(false);
        return contentResult;
    }

    void ThrowNotEmptyResponseError(string responseContent)
    {
        var errorDataTransferObject = SerializeUtil.Deserialize<V8.Error>(responseContent);
        var error = errorDataTransferObject.FromDataTransferObject();

        _logger.LogError("Error occured. Message: {message}. Type: {type}. Code: {code}", error.Errormessage, error.Errortype, error.Errorcode);
        throw new ClientResponseException("Error occured, check inner Error object for more information.", error);
    }

    void ThrowEmptyResponseError(HttpStatusCode httpStatusCode)
    {
        _logger.LogError((int)httpStatusCode + ": " + httpStatusCode);
        throw new ClientResponseException((int)httpStatusCode + ": " + httpStatusCode);
    }

    static T HandleSuccessResponse<T>(string responseContent)
    {
        return SerializeUtil.Deserialize<T>(responseContent);
    }
}