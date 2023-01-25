# RPG Viewer Client

## Description

RPG Viewer's official client-side application

## Releases

![Latest](https://img.shields.io/github/v/release/ItharDev/RPG-Viewer-Client?label=Latest)

## Contributors

There should be an error, when opening this project. This is caused by .gitignore, which will add `Privacy` folder to be ignored. This has to be done, so that the server ip-address won't be visible

To solve this, follow the instructions below

1. Create a new folder in `Assets/RPG Viewer/Scripts` named `Privacy`
2. Create a file in the newly created folder. Rename it to `Address.cs`
3. Paste this code block to the new file
```
  using UnityEngine;

  namespace RPG
  {
      public static class Address
      {
          public static string uri = [address here];
      }
  }
  ```
4. Replace `[address here]` with your server ip-address. For example: `"http://127.0.0.1:3000/"` for locahost
5. Save changes, and the error should disappear
