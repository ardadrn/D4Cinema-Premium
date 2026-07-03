# D4Cinema Premium

D4Cinema Premium is a Windows Forms cinema ticketing and administration application developed with C#, .NET Framework 4.7.2 and SQLite.

## Features

- User registration and login
- Movie browsing, search and detail pages
- Cinema and screening selection
- Interactive seat selection and ticket purchasing
- Purchased-ticket history
- Administrator dashboard and statistics
- Movie, cinema hall and user management
- Persistent poster and campaign assets

## Technologies

- C#
- .NET Framework 4.7.2
- Windows Forms
- SQLite
- FontAwesome.Sharp

## Run the project

1. Open `D4Cinema.sln` in Visual Studio 2022.
2. Allow Visual Studio to restore the NuGet packages.
3. Select `Debug` and `x64`.
4. Build and run the solution.

If package restore does not start automatically, right-click the solution and select **Restore NuGet Packages**.

## Demo accounts

| Role | Email | Password |
|---|---|---|
| Administrator | `admin@d4cinema.com` | `admin123` |
| User | `demo@d4cinema.com` | `demo123` |

These credentials are only for the demo database included in the repository.

## Application data

The repository contains initial data in `D4Cinema/DataSeed`. On first run, the application copies the database, logo, posters and campaign images to:

```text
%LOCALAPPDATA%\D4Cinema
```

Administrator-uploaded posters are saved under:

```text
%LOCALAPPDATA%\D4Cinema\Afisler
```

The SQLite database is stored at:

```text
%LOCALAPPDATA%\D4Cinema\D4CinemaDB.sqlite
```

This prevents user-generated data from being deleted by Visual Studio's `Clean` or `Rebuild` operations.

To reset the application to the repository's demo data, close the application and delete `%LOCALAPPDATA%\D4Cinema`. The folder will be recreated on the next launch.
