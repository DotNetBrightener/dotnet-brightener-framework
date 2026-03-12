# Secured API Endpoints for ASP.NET Core Application


&copy; 2025 [DotNet Brightener](mailto:admin@dotnetbrightener.com)


![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.SecuredApi)

## Inspiration

Ever wonder how you could make an API that cannot be inspected via Developer Tools? This can be useful in some very secured scenarios, such as APIs related to financial transactions, where you don't want the API requests can be inspected using browser's deveoper tool. This is where this library comes in. It allows you to create secured API endpoints that, theoretically, cannot be inspected via Developer Tools.

## What Does It Mean?

The library helps you to create API endpoints in your application, that accept the protected payload from the request and responds with protected payload in byte-array. At the client side, you will need to prepare the payload in JSON format, convert the payload to byte-array data, compress it, and then include it in the HTTP request body. When the server responds to the request, it'll also return in `byte[]` type, which you will need to accept `responseType = arraybuffer` to be able to access the data, then you can convert to the original object.

## How Do I Implement It?


### Server Side

```csharp

// Add the secured API services
builder.Services.AddSecuredApi();

// omitted. Other services configuration

var app = builder.Build();

// omitted - Configure the HTTP request pipeline.

// Map the secured API, without specifying the subpath
app.UseSecureApiHandle(); 

// Or use this
// app.UseSecureApiHandle("subpath"); // Map the secured API to a subpath

// Map the secured API handler to /syncUser endpoint, of you didn't specify the subpath
// The endpoint will be /subpath/syncUser if you specify the subpath
app.MapSecuredPost<SyncUserService>("syncUser");


// define your data model
public class UserRecord
{
    // Define the properties of the request object
}

// optional: define the response data model
public class SyncedUserResult
{
    // Define the properties of the response object
}

// define the secured API handler

public class SyncUserService : BaseApiHandler<UserRecord>
{
    protected override async Task<UserRecord> ProcessRequest(UserRecord message)
    {
        // TODO: Implement your logic to process the request

        // Return the processed data. 
        // In this case, the response data has the same type as the requested data
        return message;
    }
}

public class SyncUserService : BaseApiHandler<UserRecord, SyncedUserResult>
{
    protected override async Task<SyncedUserResult> ProcessRequest(UserRecord message)
    {
        // TODO: Implement your logic to process the request

        // Return the processed data. 
        // In this case, the response data has different type as the requested data
        return new SyncUserResult 
        {
            // Set the properties of the response object
        };
    }
}

```


### Client Side

* Javascript:

You will need a gzip compression library to pre-process the data sent to server. You can use [`pako`](https://www.npmjs.com/package/pako) library for this purpose. Here is an example of how you can use it:

```html
<!-- Load pako library -->
<script src="https://unpkg.com/pako@2.1.0/dist/pako.min.js"></script>
```

```javascript
/**
*  Compresses the given message using GZIP compressor
* */
function _compress(message) {
    const jsonMessage = JSON.stringify(message);
    const jsonBytes = new window.TextEncoder().encode(jsonMessage);

    return window.pako.gzip(jsonBytes);
}

/**
*  Decompresses the given message using GZIP compressor then converts it to JSON object
* */
function _decompress(compressedMessage) {
    const bytes = new Uint8Array(compressedMessage);
    const decompressedBytes = window.pako.ungzip(bytes);
    const decodedMessage = new window.TextDecoder().decode(decompressedBytes);
    return JSON.parse(decodedMessage);
}
```

Below is an example of how you prepare and send a request to the secured API endpoint:

```javascript

const yourApiEndpoint = ''; // The actual URL of your API endpoint

// define the body data to be sent to the server
const bodyData = {
};

const requestOptions = {
    method: 'PUT', // The actual method of your API endpoint
    body: _compress(bodyData),
    headers: {
    }
};

const responseData = await fetch(yourApiEndpoint, requestOptions)
    .then(response => {
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.arrayBuffer();
    })
    .then(buffer => {
        const decompressedData = _decompress(buffer);
        console.log(decompressedData);

        return decompressedData;
    })
    .catch(error => {
        console.error('There was a problem with the fetch operation:', error);
    });

// your responsed data is here. It'll be decompressed and converted to a JSON object
console.log('responsed dat: ', responseData);

```

Here is the example if you use `axios` library for making Http requests:

```javascript

const httpClient = axios.create({
    baseURL: 'your_api_based_url'
});

const responseData = await httpClient.put(yourApiEndpoint, _compress(syncData),
    {
        responseType: 'arraybuffer'
    })
    .then(response => {
        const decompressedData = _decompress(response.data);
        console.log(decompressedData);

        return decompressedData;
    })
    .catch(error => {
        console.error(error);
    });

```

* Dart:
> Work In Progress