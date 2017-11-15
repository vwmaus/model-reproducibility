FROM microsoft/aspnetcore:2.0
ARG source
WORKDIR /app
EXPOSE 8011 80 
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "WebInterface.dll"]
