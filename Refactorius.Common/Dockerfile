# SDK stage			 
FROM microsoft/dotnet:2.0-sdk AS base
RUN apt-get update && apt-get install -y nuget
WORKDIR /app

# Build stage
FROM base AS build-env
WORKDIR /src											  
COPY Refactorius.Common/Refactorius.Common.csproj ./Refactorius.Common/
RUN dotnet restore Refactorius.Common/Refactorius.Common.csproj

COPY Refactorius.Common.Tests/Refactorius.Common.Tests.csproj ./Refactorius.Common.Tests/
RUN dotnet restore Refactorius.Common.Tests/Refactorius.Common.Tests.csproj

# copy src
COPY . .
# test
ENV TEAMCITY_PROJECT_NAME=fake
RUN dotnet test  Refactorius.Common.Tests/Refactorius.Common.Tests.csproj

# Pack stage

# publish
RUN dotnet pack Refactorius.Common/Refactorius.Common.csproj -o /app

FROM alpine
COPY --from=build-env /app /app
ENTRYPOINT /bin/bash -c "cp /app/* /output"

