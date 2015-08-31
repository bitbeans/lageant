#lageant


###Libsodium Authentication Agent

**There will be bugs. May change. May break.**

![beta 0.2.3](https://raw.githubusercontent.com/bitbeans/lageant/master/img/0.2.3_beta.png)


### Scope
- Generate libsodium keys by input ([bytejail](https://bytejail.com), [CurveLock](https://github.com/adamcaudill/CurveLock), [miniLock](https://github.com/kaepora/miniLock))
- Load [minisign](https://github.com/jedisct1/minisign) private keys ([minisign-net](https://github.com/bitbeans/minisign-net))
- Generate random keys 
- Copy keys to clipboard
- Import generated keys (.lkey extension)
- Export generated keys (.lkey extension)

### Client Examples


```csharp
//create a new client
var client = new LageantClient();
//connect to memory
if (!client.Connect()) return;
//fetch a stored key (hex string)
var key = client.Keystore.GetKeyById("4022a87de0ff0724");

//Overloads
var key = client.Keystore.GetKeyById("a hex string or byte array");
var key = client.Keystore.GetKeyByPublicKey("a hex string or byte array");
var key = client.Keystore.GetKeyByPrivateKey("a hex string or byte array");

//Get the whole Keystore (List<Key>)
var keyStore = client.Keystore;


```

#### Client Package Installation

There is a [NuGet package](https://www.nuget.org/packages/lageant-client/) available.

#### There is also: SimpleCrypt
A small example application which uses lageant and [StreamCryptor](https://github.com/bitbeans/StreamCryptor).

![beta 0.2.3](https://raw.githubusercontent.com/bitbeans/lageant/master/img/0.1.0_simplecrypt.png)



## License
[MIT](https://en.wikipedia.org/wiki/MIT_License)