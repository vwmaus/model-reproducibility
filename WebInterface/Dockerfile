FROM microsoft/aspnetcore:2.0

# Set Docker remote API address
ENV DOCKER_REMOTE_API="unix:///var/run/docker.sock"

ARG source

WORKDIR /app

EXPOSE 80

COPY ${source:-obj/Docker/publish} .

ENTRYPOINT ["dotnet", "WebInterface.dll"]
