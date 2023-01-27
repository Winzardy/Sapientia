WORKDIR /src/SapientiaLib
COPY SapientiaLib.csproj SapientiaLib.csproj
RUN dotnet restore SapientiaLib.csproj
COPY . .