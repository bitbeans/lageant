#lageant


###Libsodium Authentication Agent


![beta 0.2.1](https://github.com/bitbeans/Lageant/blob/master/img/0.2.1_beta.png)



**There will be bugs. May change. May break.**

### Client Example 


```csharp
//create a new client
var client = new LageantClient();
//connect to memory
if (!client.Connect()) return;
//fetch a stored key (hex string)
var key = client.Keystore.GetKeyById(StringToByteArray("4022a87de0ff0724"));

```