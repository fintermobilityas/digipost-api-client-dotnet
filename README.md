# Unofficial .NET Client Library for Digipost API V8

This library is an unofficial .NET client for the Digipost API V8. It's a fork of the official [Digipost API client for .NET](https://github.com/digipost/digipost-api-client-dotnet).

## Getting Started

Follow these instructions to set up and send a message using the Digipost API from your .NET application.

### Prerequisites

- .NET SDK installed on your machine.
- An IDE or code editor (e.g., Rider, Visual Studio).
- A Digipost account with the necessary credentials (brokerId, certificatePath, certificatePassword, and digipostAddress).

### Getting started

```sh
git clone https://github.com/fintermobilityas/digipost-api-client-dotnet
cd digipost-api-client-dotnet
rider digipost-api-client.sln
```

### Demo

#### Edit the Program File:
Open the `Program.cs` file located in the `Digipost.Api.Demo` directory.

#### Set Up Your Credentials:
Update the `brokerId`, `certificatePath`, `certificatePassword`, and `digipostAddress` in `Program.cs` with the values you received from your Digipost onboarding.

#### Usage
Follow the examples in the `Digipost.Api.Demo` project to learn how to send messages and perform other tasks using the Digipost API.
