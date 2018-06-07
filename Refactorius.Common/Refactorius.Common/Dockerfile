# SDK stage			 
FROM microsoft/dotnet:2.0-sdk AS base
RUN apt-get update && apt-get install -y nuget
WORKDIR /app

# Build stage

FROM base AS build-env
WORKDIR /src

COPY Refactorius.Common.csproj .
RUN dotnet restore -nowarn:msb3202,nu1503

COPY . .
#RUN rm -rf .\bin; rm -rf .\obj; 
RUN dotnet restore -nowarn:msb3202,nu1503
#RUN dotnet build -v n -c Release -o /app

FROM build-env AS publish-env
# Pack stage

# publish
RUN dotnet pack -c Release -o /app

