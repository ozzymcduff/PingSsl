PingSsl [![Build status](https://ci.appveyor.com/api/projects/status/fm3itlyal58q2gay?svg=true)](https://ci.appveyor.com/project/wallymathieu/pingssl)

=======

Ping ssl using .net tcp and sslstream

To ping the local machine (using it's name)
```
>pingssl.exe -p=ssl3
```

To ping a remote machine use
```
>pingssl.exe -p=ssl3 -m=remotemachine.domain.com
```
