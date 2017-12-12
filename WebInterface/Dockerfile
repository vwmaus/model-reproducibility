FROM microsoft/dotnet:2.0.0-sdk
WORKDIR /src
COPY . /src
RUN dotnet publish ./WebInterface.csproj -c Release -o /app
EXPOSE 80 
