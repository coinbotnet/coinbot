# Running



# Building

You'll need .NET Core SDKs to build this project. It is cross-platform framework available here https://dotnet.microsoft.com/download

* Clone this project with `--recursive` flag (you need every submodule to be downloaded)
* Enter root directory and build it with

`dotnet build Coinbot.sln`

## Building docker image

You can use already built images on hub.docker.com. The only official repo is https://hub.docker.com/u/pniewiadomski

To build an image from sources you'll need to run below command in the root directory

`docker build -t coinbot .`

Then you can run it with 

`docker run --rm -it coinbot --help`

# Developing

The preferred way to develop Coinbot is Visual Studio Code https://code.visualstudio.com/ with C# Extension. Feel free to use Visual Studio 2017/2019 Professional. You'll need to manually configure tasks so the connectors will be compiled though.

* Ctrl+Shift+B builds project.
* F5 starts debug

Before running debug you'd like to define `.env` file in the root directory of the project. File should contain `API_KEY` and `SECRET` for the specified exchange. Of course you don't need it as long as you use `--test-mode` in your `launch.json` file

Example of .env file:

```
API_KEY=myapikey
SECRET=mysecret
```

# Testing

Project contains XUnit project which has few useful tests. There's no need to run all the tests at once.

* BinanceTest.cs - contains an example of connector unit test. You'll have to provide API_KEY and SECRET with environmental variables for ex. (Linux / Mac OS X) `API_KEY=abcd SECRET=efgh dotnet test --filter "Category=Binance"` or for Windows PowerShell:

    ```
    $env:API_KEY = "abcdef";
    $env:SECRET = "efgh";
    dotnet test --filter "Category=Binance"
    ```
* SimpleBotTests.cs - contains an example of IBot unit test. Helps to determine what algorithm suits you. At the end of the test you'll get potential profit on USD-BTC pair dummy data.

# Contributing

## Adding new connector

* Clone or fork Coinbot Core project
* Add new classlib within the root directory with the below command:

`dotnet new classlib -n Coinbot.<name of the exchange>`
* (Optionally) Add project to solution. AFAIK Omnisharp doesn't recognize project when it's outside .sln
* Create your own repository on github or any other repo for the submodule
* (Optionally) Copy .gitignore file from Coinbot.Bitbay with `cp .gitignore ../Coinbot.Bittrex`
* Push your repo to remote repository and then add project as a submodule
```
cd Coinbot.<name of the exchange>/
git init
git remote add origin <your remote repository url ex. Github>
git add .
git commit -a
git push origin master
cd ..
git submodule add <your remote repository url ex. Github> Coinbot.<name of the exchange>/
```
* That's it. You should see below message when everything went fine.

`Adding existing repo at 'Coinbot.Bittrex' to the index`

## Improving included algorithms

The actual behavior of bot is programmed in SimpleBot implementation of IBot. If you'd like to improve it's performance or create a new algorithm, feel free to fork project and then commit a merge request. Remember to use Unit tests to determine if your algorithm works as intended.