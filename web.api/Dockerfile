FROM microsoft/dotnet:2.2-sdk
WORKDIR /app
 
# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore
 
# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "web.api.dll"]