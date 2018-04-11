# cypher-net

A simple example of using symmetric and assymetric encryption in .NET

## Usage

### Generate a pair of keys
```
cypher-net -g
```

A text will appear, copy and paste the public key to "public.key" and the private one to "private.key"

### Encrypting

Scenario: Given an existing file "secret.txt" and the public key provided by the intended receiver, the sender will create a cyphered message.

```
cypher-net secret.txt secret.enc -key @public.key
```

This will take secret.txt, and encrypt it into a new file secret.enc using the public key of the intended receiver (eyes only).

### Decrypting

Scenario: Given a private key and a cyphered message created "secret.enc" the receiver wants to read the message.
```
cypher-net -d secret.enc secret.dec -key @private.key
```
