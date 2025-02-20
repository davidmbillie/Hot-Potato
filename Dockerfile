FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
LABEL cache=true
ARG IMAGE_VERSION

WORKDIR /app
COPY . .

RUN dotnet build -c Release -p:Version=${IMAGE_VERSION}
RUN dotnet publish -c Release --framework=net6.0 --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime

LABEL maintainer "Test Automation Team"

ENV SPEC_LOCATION "https://raw.githubusercontent.com/davidmbillie/Hot-Potato/master/test/RawPotatoSpec.yaml"
ENV REMOTE_ENDPOINT "http://localhost:9000"

COPY --from=build /app/publish /opt/hotpotato/

EXPOSE 3232
ENTRYPOINT dotnet /opt/hotpotato/HotPotato.AspNetCore.Host.dll --SpecLocation=${SPEC_LOCATION} --RemoteEndpoint=${REMOTE_ENDPOINT}
